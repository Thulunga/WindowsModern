using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Soti.Diagnostics;
using Soti.MobiControl.DeploymentServerExtensions.Contracts;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.SensitiveDataProtection;
using Soti.SensitiveDataProtection.Model;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <inheritdoc />
/// <seealso cref="Soti.MobiControl.WindowsModern.IDeviceBitLockerKeyService" />
internal sealed class DeviceBitLockerKeyService : IDeviceBitLockerKeyService
{
    private const string Delimiter = "##";
    private const string RegexPattern = @"^DriveLetter:([\w]{1}):,KeyProtectorID:{([\w\d-]{36})},NumericalPassword:(?:([\d-]{55}))?$";
    private const string LogName = "WindowsBitLocker";
    private const string ResetBitLocker = "ENCRYPTED_DRIVES_RESET";

    private readonly IProgramTrace _programTrace;
    private readonly IDataKeyService _dataKeyService;
    private readonly ISensitiveDataEncryptionService _sensitiveDataEncryptionService;
    private readonly IDeviceBitLockerKeyProvider _deviceBitLockerKeyProvider;
    private readonly Lazy<IMessagePublisher> _messagePublisher;
    private readonly Lazy<IDeviceAdministrationCallback> _deviceAdministrationCallback;

    private static readonly MemoryCache MemoryCache = new(new MemoryCacheOptions
    {
        ExpirationScanFrequency = TimeSpan.FromMinutes(15)
    });

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceBitLockerKeyService"/> class.
    /// </summary>
    /// <param name="programTrace">The program trace.</param>
    /// <param name="dataKeyService">The data key service.</param>
    /// <param name="sensitiveDataEncryptionService">The sensitive data encryption service.</param>
    /// <param name="deviceBitLockerKeyProvider">The device bit locker key provider.</param>
    /// <param name="messagePublisher">The message publisher.</param>
    /// <param name="deviceAdministrationCallback">The device administration callback.</param>
    public DeviceBitLockerKeyService(
        IProgramTrace programTrace,
        IDataKeyService dataKeyService,
        ISensitiveDataEncryptionService sensitiveDataEncryptionService,
        IDeviceBitLockerKeyProvider deviceBitLockerKeyProvider,
        Lazy<IMessagePublisher> messagePublisher,
        Lazy<IDeviceAdministrationCallback> deviceAdministrationCallback)
    {
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _dataKeyService = dataKeyService ?? throw new ArgumentNullException(nameof(dataKeyService));
        _sensitiveDataEncryptionService = sensitiveDataEncryptionService ?? throw new ArgumentNullException(nameof(sensitiveDataEncryptionService));
        _deviceBitLockerKeyProvider = deviceBitLockerKeyProvider ?? throw new ArgumentNullException(nameof(deviceBitLockerKeyProvider));
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        _deviceAdministrationCallback = deviceAdministrationCallback ?? throw new ArgumentNullException(nameof(deviceAdministrationCallback));
    }

    /// <inheritdoc />
    public bool AreBitLockerKeysAvailable(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        if (MemoryCache.TryGetValue(deviceId, out bool value))
        {
            return value;
        }

        value = _deviceBitLockerKeyProvider.AreBitLockerKeysAvailable(deviceId);
        MemoryCache.Set(deviceId, value,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

        return value;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<int, bool> AreBitLockerKeysAvailable(HashSet<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        return _deviceBitLockerKeyProvider.AreBitLockerKeysAvailable(deviceIds);
    }

    /// <inheritdoc />
    public IEnumerable<DeviceBitLockerKey> GetBitLockerKeys(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var bitLockerKeysData = _deviceBitLockerKeyProvider.GetBitLockerKeys(deviceId).ToArray();

        var dataKeysCollection = GetDataKeys(bitLockerKeysData.Select(x => x.DataKeyId).Distinct());

        return (
            from bitLockerKey in bitLockerKeysData
            select new DeviceBitLockerKey
            {
                DriveName = bitLockerKey.DriveName,
                RecoveryKeyId = bitLockerKey.RecoveryKeyId,
                RecoveryKey = Encoding.UTF8.GetString(_sensitiveDataEncryptionService.Decrypt(bitLockerKey.RecoveryKey, dataKeysCollection[bitLockerKey.DataKeyId])),
                DriveEncryptionStatus = bitLockerKey.DriveEncryptionStatus,
                KeyProtectors = bitLockerKey.KeyProtectors,
                DriveType = bitLockerKey.DriveType
            }).ToList();
    }

    /// <inheritdoc />
    public void ProcessBitLockerKeysData(int deviceId, string bitLockerKeysData)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        if (string.IsNullOrWhiteSpace(bitLockerKeysData) || bitLockerKeysData == ResetBitLocker)
        {
            DeleteBitLockerKeysData(deviceId);
        }
        else
        {
            var bitLockerKeys = ParseBitLockerKeys(deviceId, bitLockerKeysData).ToArray();

            if (!bitLockerKeys.Any())
            {
                return;
            }

            var existingBitLockerKeys = _deviceBitLockerKeyProvider.GetBitLockerKeys(deviceId).ToArray();

            // If the recovery keys count is same and RecoveryKeyId along with DriveName is matching, then we don't need to update the records.
            if (bitLockerKeys.Length == existingBitLockerKeys.Length
                && bitLockerKeys.All(bitLockerKey =>
                    existingBitLockerKeys.Any(x =>
                        x.RecoveryKeyId.ToString().Equals(bitLockerKey.RecoveryKeyId.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        && string.Equals(x.DriveName, bitLockerKey.DriveName, StringComparison.InvariantCultureIgnoreCase))))
            {
                return;
            }

            // Stored info for drives in "Golden Lock" shouldn't be lost.
            bitLockerKeys = CheckGoldenLock(bitLockerKeys, existingBitLockerKeys);

            if (bitLockerKeys.Length == 0)
            {
                return;
            }

            var dataKey = _dataKeyService.GetKey();

            var bitLockerKeysList = (
                from bitLockerKey in bitLockerKeys
                select new DeviceBitLockerKeyData
                {
                    DriveName = bitLockerKey.DriveName,
                    RecoveryKeyId = bitLockerKey.RecoveryKeyId,
                    DataKeyId = dataKey.Id,
                    RecoveryKey = _sensitiveDataEncryptionService.Encrypt(Encoding.UTF8.GetBytes(bitLockerKey.RecoveryKey), dataKey)
                }).ToList();

            _deviceBitLockerKeyProvider.UpdateBitLockerKeys(deviceId, bitLockerKeysList);
            MemoryCache.Remove(deviceId);
            _deviceAdministrationCallback.Value.SendInvalidateBitLockerCacheNotification(deviceId);

            _programTrace.Write(TraceLevel.Verbose, LogName, $"BitLocker key(s) information updated for {deviceId}");
        }
    }

    /// <inheritdoc />
    public void DeleteBitLockerKeysData(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        _deviceBitLockerKeyProvider.DeleteBitLockerKeys(deviceId);
        MemoryCache.Remove(deviceId);
        _deviceAdministrationCallback.Value.SendInvalidateBitLockerCacheNotification(deviceId);

        _programTrace.Write(TraceLevel.Verbose, LogName, $"BitLocker key(s) information deleted for {deviceId}");
    }

    /// <inheritdoc />
    public void InvalidateBitLockerCache(int deviceId, bool notify)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);
        MemoryCache.Remove(deviceId);
        if (notify)
        {
            PublishInvalidateCacheMessage(deviceId);
        }
    }

    private IEnumerable<DeviceBitLockerKey> ParseBitLockerKeys(int deviceId, string bitLockerKeyData)
    {
        try
        {
            var bitLockerKeys = new List<DeviceBitLockerKey>();

            var bitLockerKeysArray = bitLockerKeyData.Split(Delimiter.ToArray(), StringSplitOptions.RemoveEmptyEntries);

            var regexPattern = new Regex(RegexPattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

            foreach (var keyString in bitLockerKeysArray)
            {
                var match = regexPattern.Match(keyString);
                if (match.Success)
                {
                    bitLockerKeys.Add(new DeviceBitLockerKey
                    {
                        DriveName = match.Groups[1].Value,
                        RecoveryKeyId = Guid.Parse(match.Groups[2].Value),
                        RecoveryKey = match.Groups[3].Success ? match.Groups[3].Value : string.Empty
                    });
                    continue;
                }

                _programTrace.Write(TraceLevel.Error, LogName,
                    $"Unable to process BitLocker key information for {deviceId} as the input string was not in the proper format.");
                return Enumerable.Empty<DeviceBitLockerKey>();
            }

            return bitLockerKeys;
        }
        catch (Exception ex)
        {
            _programTrace.Write(TraceLevel.Error, LogName, $"Unable to process BitLocker key information for {deviceId} as the input string was not in the proper format.");
            _programTrace.Write(TraceLevel.Error, LogName, ex.Message);
            return Enumerable.Empty<DeviceBitLockerKey>();
        }
    }

    private IDictionary<int, DataKey> GetDataKeys(IEnumerable<int> dataKeyIds)
    {
        var dataKeys = new Dictionary<int, DataKey>();

        foreach (var dataKey in dataKeyIds)
        {
            dataKeys[dataKey] = _dataKeyService.GetKey(dataKey);
        }

        return dataKeys;
    }

    private DeviceBitLockerKey[] CheckGoldenLock(IEnumerable<DeviceBitLockerKey> bitLockerKeys, DeviceBitLockerKeyData[] existingBitLockerKeys)
    {
        var dataKeysCollection = GetDataKeys(existingBitLockerKeys.Select(x => x.DataKeyId).Distinct());

        return bitLockerKeys
            .Where(bitLockerKey =>
            {
                if (!string.IsNullOrEmpty(bitLockerKey.RecoveryKey))
                {
                    return true;
                }

                var match = existingBitLockerKeys.FirstOrDefault(x => x.RecoveryKeyId == bitLockerKey.RecoveryKeyId);
                if (match == null)
                {
                    return false;
                }

                bitLockerKey.RecoveryKey = Encoding.UTF8.GetString(
                    _sensitiveDataEncryptionService.Decrypt(match.RecoveryKey, dataKeysCollection[match.DataKeyId]));
                return true;
            })
            .ToArray();
    }

    private void PublishInvalidateCacheMessage(int deviceId)
    {
        _messagePublisher.Value.Publish(
            new InvalidateBitLockerCacheMessage
            {
                DeviceId = deviceId
            },
            ApplicableServer.Dse,
            ApplicableServer.Ms);
    }
}
