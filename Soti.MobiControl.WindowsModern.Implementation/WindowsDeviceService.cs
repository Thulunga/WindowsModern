using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.SensitiveDataProtection;
using Soti.SensitiveDataProtection.Model;
using static Soti.MobiControl.WindowsModern.Implementation.WindowsDeviceHelperMethods;

namespace Soti.MobiControl.WindowsModern.Implementation;

internal sealed class WindowsDeviceService : IWindowsDeviceService
{
    private readonly IDataKeyService _dataKeyService;
    private readonly IWindowsDeviceDataProvider _windowsDeviceProvider;
    private readonly ISensitiveDataEncryptionService _sensitiveDataEncryptionService;

    public WindowsDeviceService(
        IDataKeyService dataKeyService,
        ISensitiveDataEncryptionService sensitiveDataEncryptionService,
        IWindowsDeviceDataProvider windowsDeviceProvider)
    {
        _dataKeyService = dataKeyService ?? throw new ArgumentNullException(nameof(dataKeyService));
        _sensitiveDataEncryptionService = sensitiveDataEncryptionService ?? throw new ArgumentNullException(nameof(sensitiveDataEncryptionService));
        _windowsDeviceProvider = windowsDeviceProvider ?? throw new ArgumentNullException(nameof(windowsDeviceProvider));
    }

    /// <inheritdoc />
    public bool IsDeviceLocked(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        return _windowsDeviceProvider.IsDeviceLocked(deviceId);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<int, bool> AreDevicesLocked(HashSet<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        return _windowsDeviceProvider.AreDevicesLocked(deviceIds);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<int, bool> GetWinSandBoxStatusByDeviceIds(IEnumerable<int> deviceIds)
    {
        deviceIds = deviceIds.ToList();
        if (deviceIds == null || !deviceIds.Any())
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        return _windowsDeviceProvider.GetSandBoxStatusByIds(deviceIds);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<int, WindowsDeviceModel> BulkGetDeviceDetails(IEnumerable<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        if (!deviceIds.Any())
        {
            return new Dictionary<int, WindowsDeviceModel>();
        }

        var windowsDeviceData = _windowsDeviceProvider.BulkGetDeviceDetails(deviceIds);
        if (windowsDeviceData == null)
        {
            return null;
        }

        return windowsDeviceData
            .ToDictionary(
                w => w.Key,
                w => new WindowsDeviceModel
                {
                    DeviceId = w.Value.DeviceId,
                    IsLocked = w.Value.IsLocked,
                    OsImageId = w.Value.OsImageId,
                    OsImageDeploymentTime = w.Value.OsImageDeploymentTime,
                    BiosPasswordStatus = w.Value.BiosPasswordStatusId,
                }
            );
    }

    /// <inheritdoc />
    public WindowsSandBoxDetails GetWinSandBoxStatusByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }
        var windowsDeviceData = _windowsDeviceProvider.Get(deviceId);
        if (windowsDeviceData == null)
        {
            return null;
        }

        return new WindowsSandBoxDetails
        {
            DeviceId = windowsDeviceData.DeviceId,
            IsSandBoxEnabled = windowsDeviceData.IsSandBoxEnabled
        };
    }

    /// <inheritdoc />
    public IEnumerable<WindowsSandBoxDetails> GetAllWinSandBoxStatus()
    {
        var windowsDeviceDataList = _windowsDeviceProvider.GetAll();

        return windowsDeviceDataList.Select(x => new WindowsSandBoxDetails
        {
            DeviceId = x.DeviceId,
            IsSandBoxEnabled = x.IsSandBoxEnabled
        });
    }

    /// <inheritdoc />
    public WindowsDeviceModel GetByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var windowsDeviceData = _windowsDeviceProvider.Get(deviceId);
        if (windowsDeviceData == null)
        {
            return null;
        }
        if (!windowsDeviceData.IsLocked && windowsDeviceData.Passcode != null)
        {
            throw new InvalidOperationException("Passcode should be null when device is unlocked");
        }

        if (windowsDeviceData.IsLocked && windowsDeviceData.Passcode == null)
        {
            throw new InvalidOperationException("Passcode should not be null when device is locked");
        }
        if (windowsDeviceData.Passcode != null && windowsDeviceData.DataKeyId != null)
        {
            var dataKey = _dataKeyService.GetKey((int)windowsDeviceData.DataKeyId);
            var decryptedPasscode = Deserialize(windowsDeviceData.Passcode, dataKey);
            return new WindowsDeviceModel
            {
                DeviceId = windowsDeviceData.DeviceId,
                IsLocked = windowsDeviceData.IsLocked,
                Passcode = decryptedPasscode,
                OsImageId = windowsDeviceData.OsImageId,
                OsImageDeploymentTime = windowsDeviceData.OsImageDeploymentTime,
                WifiSubnet = windowsDeviceData.WifiSubnet,
                WifiModeId = windowsDeviceData.WifiModeId,
                NetworkAuthenticationId = windowsDeviceData.NetworkAuthenticationId,
                WirelessLanModeId = windowsDeviceData.WirelessLanModeId,
                BiosPasswordStatus = windowsDeviceData.BiosPasswordStatusId
            };
        }

        return new WindowsDeviceModel
        {
            DeviceId = windowsDeviceData.DeviceId,
            IsLocked = windowsDeviceData.IsLocked,
            Passcode = null,
            OsImageId = windowsDeviceData.OsImageId,
            OsImageDeploymentTime = windowsDeviceData.OsImageDeploymentTime,
            WifiSubnet = windowsDeviceData.WifiSubnet,
            WifiModeId = windowsDeviceData.WifiModeId,
            NetworkAuthenticationId = windowsDeviceData.NetworkAuthenticationId,
            WirelessLanModeId = windowsDeviceData.WirelessLanModeId,
            BiosPasswordStatus = windowsDeviceData.BiosPasswordStatusId
        };
    }

    /// <inheritdoc />
    public void Insert(WindowsDeviceModel deviceModel)
    {
        if (deviceModel == null)
        {
            throw new ArgumentNullException(nameof(deviceModel));
        }

        ValidatePasscode(deviceModel);

        if (deviceModel.IsLocked && !string.IsNullOrEmpty(deviceModel.Passcode))
        {
            InsertOrUpdate(_windowsDeviceProvider.Insert, deviceModel);
        }
        else
        {
            _windowsDeviceProvider.Insert(WindowsDeviceDataConverter.ToWindowsDeviceData(deviceModel));
        }
    }

    /// <inheritdoc />
    public void BulkInsertWindowsDevices(IEnumerable<WindowsDeviceModel> windowsDevices)
    {
        if (windowsDevices == null)
        {
            throw new ArgumentNullException(nameof(windowsDevices));
        }

        var deviceList = new List<WindowsDeviceData>();
        foreach (var windowsDevice in windowsDevices)
        {
            ValidatePasscode(windowsDevice);
            if (windowsDevice.IsLocked && !string.IsNullOrEmpty(windowsDevice.Passcode))
            {
                deviceList.Add(ProcessPasscode(windowsDevice));
            }
            else
            {
                deviceList.Add(WindowsDeviceDataConverter.ToWindowsDeviceData(windowsDevice));
            }
        }

        _windowsDeviceProvider.BulkInsertWindowsDevices(deviceList);
    }

    /// <inheritdoc />
    public void UpdateWindowsSandBoxStatus(int deviceId, bool isSandBoxEnabled)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }
        _windowsDeviceProvider.UpdateWindowsSandBoxStatus(deviceId, isSandBoxEnabled);
    }

    /// <inheritdoc />
    public void Update(WindowsDeviceModel deviceModel)
    {
        if (deviceModel == null)
        {
            throw new ArgumentNullException(nameof(deviceModel));
        }

        ValidatePasscode(deviceModel);

        if (deviceModel.IsLocked && !string.IsNullOrEmpty(deviceModel.Passcode))
        {
            InsertOrUpdate(_windowsDeviceProvider.Update, deviceModel);
        }
        else
        {
            _windowsDeviceProvider.Update(
                new WindowsDeviceData
                {
                    DataKeyId = null,
                    DeviceId = deviceModel.DeviceId,
                    IsLocked = deviceModel.IsLocked,
                    Passcode = null
                });
        }
    }

    /// <inheritdoc />
    public void UpdateLoggedInUserLastCheckInTime(int deviceId, DateTime lastCheckInTime)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _windowsDeviceProvider.UpdateLastCheckInDeviceUserTime(deviceId, lastCheckInTime);
    }

    /// <inheritdoc />
    public void UpdateHardwareId(int deviceId, string hardwareId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(hardwareId))
        {
            throw new ArgumentException(nameof(hardwareId));
        }

        _windowsDeviceProvider.UpdateHardwareId(deviceId, hardwareId);
    }

    /// <inheritdoc />
    public void UpdateHardwareIdWifiSubnet(int deviceId, string wifiSubnet, string hardwareId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _windowsDeviceProvider.UpdateHardwareIdWifiSubnet(deviceId, wifiSubnet, hardwareId);
    }

    /// <inheritdoc />
    public void UpdateWindowsDeviceDetails(WindowsModernSnapshot windowsModernSnapshot)
    {
        ArgumentNullException.ThrowIfNull(windowsModernSnapshot);

        if (windowsModernSnapshot.DeviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowsModernSnapshot.DeviceId));
        }

        ValidateNetworkDetailsEnum(windowsModernSnapshot);

        _windowsDeviceProvider.UpdateWindowsDeviceDetails(windowsModernSnapshot);
    }

    /// <inheritdoc />
    public void UpdateDefenderScanInfo(int deviceId, DateTime? lastQuickScanTime, DateTime? lastFullScanTime, DateTime? lastSyncTime)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _windowsDeviceProvider.UpdateDefenderScanInfo(deviceId, lastQuickScanTime, lastFullScanTime, lastSyncTime);
    }

    /// <inheritdoc />
    public void UpdateOsImageInfo(int deviceId, int osImageId, DateTime deployedTime)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (osImageId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(osImageId));
        }

        _windowsDeviceProvider.UpdateOsImageInfo(deviceId, osImageId, deployedTime);
    }

    private WindowsDeviceData ProcessPasscode(WindowsDeviceModel deviceModel)
    {
        var dataKey = _dataKeyService.GetKey();
        var encryptedPasscode = Serialize(deviceModel.Passcode, dataKey);
        return new WindowsDeviceData
        {
            DataKeyId = dataKey.Id,
            DeviceId = deviceModel.DeviceId,
            IsLocked = deviceModel.IsLocked,
            Passcode = encryptedPasscode
        };
    }

    private byte[] Serialize(string text, DataKey key)
    {
        return _sensitiveDataEncryptionService.Encrypt(Encoding.UTF8.GetBytes(text), key);
    }

    private string Deserialize(byte[] encryptedText, DataKey key)
    {
        return Encoding.UTF8.GetString(_sensitiveDataEncryptionService.Decrypt(encryptedText, key));
    }

    private void ValidatePasscode(WindowsDeviceModel deviceModel)
    {
        if (!deviceModel.IsLocked && deviceModel.Passcode != null)
        {
            throw new ArgumentException(nameof(deviceModel.Passcode), "Passcode should be null when device is unlocked");
        }

        if (deviceModel.IsLocked && deviceModel.Passcode == null)
        {
            throw new ArgumentNullException(nameof(deviceModel.Passcode), "Passcode should not be null when device is locked");
        }
    }

    private void InsertOrUpdate(Action<WindowsDeviceData> action, WindowsDeviceModel deviceModel)
    {
        var dataKey = _dataKeyService.GetKey();
        var encryptedPasscode = Serialize(deviceModel.Passcode, dataKey);
        action(new WindowsDeviceData
        {
            DataKeyId = dataKey.Id,
            DeviceId = deviceModel.DeviceId,
            IsLocked = deviceModel.IsLocked,
            Passcode = encryptedPasscode,
            LastCheckInDeviceUserTime = null,
            HardwareId = null
        });
    }
}
