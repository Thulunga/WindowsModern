using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Soti.SensitiveDataProtection;
using Soti.MobiControl.InstallerServices.Services;
using Soti.MobiControl.InstallerServices.Config;
using Soti.MobiControl.WindowsModern.Models;
using Soti.Transactions;

namespace Soti.MobiControl.WindowsModern.UpgradeHandler;

/// <summary>
/// LocalUserUserNameUpgradeService.
/// </summary>
internal sealed class LocalUserUserNameUpgradeService : IUpgradeService
{
    private const string UpgradeServiceName = nameof(LocalUserUserNameUpgradeService);
    private static readonly Version MinApplicableToVersion = new(2026, 0, 0);
    private static readonly Version MinApplicableFromVersion = new(2025, 0, 0);
    private readonly ITraceLogger _traceLogger;
    private readonly IWindowsDeviceLocalUsersService _windowsDeviceLocalUsersService;
    private readonly IDataKeyService _dataKeyService;
    private readonly ISensitiveDataEncryptionService _sensitiveDataEncryptionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalUserUserNameUpgradeService"/> class.
    /// </summary>
    /// <param name="traceLogger">The trace logger for recording operations.</param>
    /// <param name="windowsDeviceLocalUsersService">The service for managing Windows device local users.</param>
    /// <param name="dataKeyService">The service for managing data keys.</param>
    /// <param name="sensitiveDataEncryptionService">The service for encrypting sensitive data.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
    public LocalUserUserNameUpgradeService(
        ITraceLogger traceLogger,
        IWindowsDeviceLocalUsersService windowsDeviceLocalUsersService,
        IDataKeyService dataKeyService,
        ISensitiveDataEncryptionService sensitiveDataEncryptionService)
    {
        _traceLogger = traceLogger ?? throw new ArgumentNullException(nameof(traceLogger));
        _windowsDeviceLocalUsersService = windowsDeviceLocalUsersService ?? throw new ArgumentNullException(nameof(windowsDeviceLocalUsersService));
        _dataKeyService = dataKeyService ?? throw new ArgumentNullException(nameof(dataKeyService));
        _sensitiveDataEncryptionService = sensitiveDataEncryptionService ?? throw new ArgumentNullException(nameof(sensitiveDataEncryptionService));
    }
    public int Priority => 1;

    /// <summary>
    /// Checks if the upgrade should be applied based on the configuration.
    /// </summary>
    /// <param name="config">Upgrade configuration to check</param>
    /// <exception cref="ArgumentNullException">Thrown if config is null</exception>
    public bool IsUpgradeApplicable(UpgradeQualifier config)
    {
        return config == null ? throw new ArgumentNullException(nameof(config)) : CheckIsUpgradeApplicable(config);
    }

    /// <summary>
    /// Performs the upgrade process for local user data.
    /// </summary>
    /// <param name="config">Upgrade configuration parameters</param>
    /// <exception cref="ArgumentNullException">Thrown if config is null</exception>
    public void Upgrade(UpgradeQualifier config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (!IsUpgradeApplicable(config))
        {
            return;
        }

        var tmpLocalUserData = _windowsDeviceLocalUsersService.GetAllTmpLocalUsers()?.ToArray();
        if (tmpLocalUserData == null || tmpLocalUserData.Length == 0)
        {
            _traceLogger.Trace($"{UpgradeServiceName}: No data found for tmp Local user table");
            return;
        }

        var decryptedUsers = DecryptAllLocalUsersData(tmpLocalUserData);

        TransactionHelper.RunInTransaction(() =>
        {
            _windowsDeviceLocalUsersService.UpdateDecryptedLocalUsers(decryptedUsers);
            _windowsDeviceLocalUsersService.DeleteTmpLocalUsers(tmpLocalUserData);
        });

        _traceLogger.Trace($"{UpgradeServiceName}: The the decryption of names has been successfully completed.");
    }

    private static bool CheckIsUpgradeApplicable(UpgradeQualifier config)
    {
        if (config.UpgradeFrom > config.UpgradeTo)
        {
            throw new InvalidOperationException($"{UpgradeServiceName}:Upgrade from version '{config.UpgradeFrom}' is higher than upgrade to version '{config.UpgradeTo}'.");
        }

        return config.UpgradeFrom >= MinApplicableFromVersion && config.UpgradeFrom < MinApplicableToVersion && config.UpgradeTo >= MinApplicableToVersion;
    }

    private List<DecryptedLocalUserModel> DecryptAllLocalUsersData(IEnumerable<WindowsDeviceTmpLocalUserModel> localUsers)
    {
        var localUsersList = localUsers?.ToList() ?? new();

        if (!localUsersList.Any())
        {
            return new();
        }

        var userIds = localUsersList.Select(u => u.WindowsDeviceUserId).ToList();
        var userToDataKeyIdMap = _windowsDeviceLocalUsersService.GetDataKeyIdByUserId(userIds);

        var decryptedUsers = new List<DecryptedLocalUserModel>(localUsersList.Count);

        foreach (var user in localUsersList)
        {
            if (!userToDataKeyIdMap.TryGetValue(user.WindowsDeviceUserId, out var dataKeyId))
            {
                _traceLogger.Trace($"{UpgradeServiceName}: No DataKeyId found for UserId {user.WindowsDeviceUserId}");
                continue;
            }

            var dataKey = _dataKeyService.GetKey(dataKeyId);
            if (dataKey == null)
            {
                _traceLogger.Trace($"{UpgradeServiceName}: Failed to get data key {dataKeyId}");
                continue;
            }

            if (user.UserName == null || user.UserName.Length == 0)
            {
                _traceLogger.Trace($"{UpgradeServiceName}: {nameof(user.UserName)} cannot be null or empty for UserId {user.WindowsDeviceUserId}");
                continue;
            }

            var decryptedUsername = Encoding.UTF8.GetString(_sensitiveDataEncryptionService.Decrypt(user.UserName, dataKey));

            var decryptedUserFullName = user.UserFullName is { Length: > 0 }
                ? Encoding.UTF8.GetString(_sensitiveDataEncryptionService.Decrypt(user.UserFullName, dataKey))
                : null;

            decryptedUsers.Add(new DecryptedLocalUserModel()
            {
                WindowsDeviceUserId = user.WindowsDeviceUserId,
                UserName = decryptedUsername,
                UserFullName = decryptedUserFullName
            });
        }

        return decryptedUsers;
    }
}
