using System;
using System.Linq;
using Soti.MobiControl.InstallerServices.Services;
using Soti.MobiControl.InstallerServices.Config;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.UpgradeHandler;

internal sealed class LocalUserUpgradeService : IUpgradeService
{
    private const string UpgradeServiceName = nameof(LocalUserUpgradeService);
    private static readonly Version MinApplicableToVersion = new(2025, 1, 0);
    private static readonly Version MinApplicableFromVersion = new(2025, 0, 0);
    private readonly ITraceLogger _traceLogger;
    private readonly IWindowsDeviceLocalUsersService _windowsDeviceLocalUsersService;
    private readonly IWindowsDeviceLocalGroupsService _windowsDeviceLocalGroupsService;

    public LocalUserUpgradeService(
        ITraceLogger traceLogger,
        IWindowsDeviceLocalUsersService windowsDeviceLocalUsersService,
        IWindowsDeviceLocalGroupsService windowsDeviceLocalGroupsService)
    {
        _traceLogger = traceLogger ?? throw new ArgumentNullException(nameof(traceLogger));
        _windowsDeviceLocalUsersService = windowsDeviceLocalUsersService ?? throw new ArgumentNullException(nameof(windowsDeviceLocalUsersService));
        _windowsDeviceLocalGroupsService = windowsDeviceLocalGroupsService ?? throw new ArgumentNullException(nameof(windowsDeviceLocalGroupsService));
    }

    public int Priority => 1;

    public bool IsUpgradeApplicable(UpgradeQualifier config)
    {
        return config == null ? throw new ArgumentNullException(nameof(config)) : CheckIsUpgradeApplicable(config);
    }

    public void Upgrade(UpgradeQualifier config)
    {
        if (!IsUpgradeApplicable(config))
        {
            return;
        }

        var localUserData = _windowsDeviceLocalUsersService.GetDeviceLocalUsersSummary()?.ToArray();
        if (localUserData == null || localUserData.Length == 0)
        {
            _traceLogger.Trace($"{UpgradeServiceName}: No data found for Local user table");
            return;
        }

        foreach (var localUser in localUserData)
        {
            MigrateLocalUserData(localUser);
        }

        _traceLogger.Trace($"{UpgradeServiceName}: The migration of group names has been successfully completed.");
    }

    private static bool CheckIsUpgradeApplicable(UpgradeQualifier config)
    {
        if (config.UpgradeFrom != null && config.UpgradeTo != null && config.UpgradeFrom > config.UpgradeTo)
        {
            throw new InvalidOperationException($"{UpgradeServiceName}:Upgrade from version '{config.UpgradeFrom}' is higher than upgrade to version '{config.UpgradeTo}'.");
        }

        return config.UpgradeFrom >= MinApplicableFromVersion && config.UpgradeFrom < MinApplicableToVersion && config.UpgradeTo >= MinApplicableToVersion;
    }

    private void MigrateLocalUserData(WindowsDeviceLocalUserModel localUser)
    {
        if (localUser.UserGroups == null || !localUser.UserGroups.Any())
        {
            return;
        }
        foreach (var name in localUser.UserGroups)
        {
            var windowsDeviceLocalGroupNameId = _windowsDeviceLocalGroupsService.InsertLocalGroupName(name);
            var windowsDeviceLocalGroupId = _windowsDeviceLocalGroupsService.UpsertWindowsDeviceLocalGroup(localUser.DeviceId, windowsDeviceLocalGroupNameId);
            _windowsDeviceLocalGroupsService.InsertLocalGroupUser(localUser.WindowsDeviceLocalUserId, windowsDeviceLocalGroupId);
        }
    }
}
