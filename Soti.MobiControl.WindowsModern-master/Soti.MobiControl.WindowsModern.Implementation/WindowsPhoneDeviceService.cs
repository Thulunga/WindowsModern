using Microsoft.Extensions.Caching.Memory;
using Soti.MobiControl.Settings;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.MobiControl.WindowsModern.Models;
using System;
using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Implementation;

internal sealed class WindowsPhoneDeviceService : IWindowsPhoneDeviceService
{
    private readonly WindowsPhoneDeviceProvider _windowsPhoneDeviceProvider;
    private readonly IMemoryCache _deviceSnapshotCache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    private readonly ISettingsManagementService _settingsManagementService;
    private const string FeatureName = "WindowsModern";
    private const string SettingName = "IsDeviceSnaphsotCached";
    private readonly bool _snapShotCacheFlag;

    public WindowsPhoneDeviceService(WindowsPhoneDeviceProvider windowsPhoneDeviceProvider,
        IMemoryCache deviceSnapshotCache,
        ISettingsManagementService settingsManagementService)
    {
        _windowsPhoneDeviceProvider = windowsPhoneDeviceProvider ?? throw new ArgumentNullException(nameof(windowsPhoneDeviceProvider));
        _deviceSnapshotCache = deviceSnapshotCache ?? throw new ArgumentNullException(nameof(deviceSnapshotCache));
        _settingsManagementService = settingsManagementService ?? throw new ArgumentNullException(nameof(settingsManagementService));
        _snapShotCacheFlag = AllowDeviceSnapshotCaching();
    }

    /// <inheritdoc />
    public WindowsPhoneDeviceInfo GetByDeviceId(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        return _windowsPhoneDeviceProvider.GetByDeviceId(deviceId);
    }

    /// <inheritdoc />
    public void Update(WindowsPhoneDeviceInfo deviceInfo, IEnumerable<WindowsPhoneInfoExtendedProperties> extendedProperties)
    {
        ArgumentNullException.ThrowIfNull(deviceInfo);

        var updateFlags = extendedProperties.ToDataColumns();
        _windowsPhoneDeviceProvider.Update(deviceInfo, updateFlags);

        if (_snapShotCacheFlag)
        {
            _deviceSnapshotCache.Set(deviceInfo.DeviceId, deviceInfo.SessionContext, DateTimeOffset.Now.Add(_cacheExpiration));
        }
    }

    public void InvalidateDeviceSnapshotCache(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);
        if (_snapShotCacheFlag)
        {
            _deviceSnapshotCache.Remove(deviceId);
        }
    }

    /// <inheritdoc />
    public void UpdateEnrollmentId(int deviceId, int enrollmentId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        _windowsPhoneDeviceProvider.UpdateEnrollmentId(deviceId, enrollmentId);
    }

    /// <inheritdoc />
    public int GetEnrollmentId(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        return _windowsPhoneDeviceProvider.GetEnrollmentId(deviceId);
    }

    /// <inheritdoc />
    public Tuple<string, int> GetWnsChannelInfo(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        return _windowsPhoneDeviceProvider.GetWnsChannelInfo(deviceId);
    }

    /// <inheritdoc />
    public string GetSessionContextById(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        if (!_snapShotCacheFlag)
        {
            return _windowsPhoneDeviceProvider.GetSessionContextById(deviceId);
        }

        if (_deviceSnapshotCache.TryGetValue<string>(deviceId, out var sessionContext))
        {
            return sessionContext;
        }

        sessionContext = _windowsPhoneDeviceProvider.GetSessionContextById(deviceId);

        if (!string.IsNullOrEmpty(sessionContext))
        {
            _deviceSnapshotCache.Set(deviceId, sessionContext, DateTimeOffset.Now.Add(_cacheExpiration));
        }

        return sessionContext;
    }

    /// <inheritdoc />
    public bool CheckChannel(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        return _windowsPhoneDeviceProvider.CheckChannel(deviceId);
    }

    /// <inheritdoc />
    public int GetChannelStatus(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        return _windowsPhoneDeviceProvider.GetChannelStatus(deviceId);
    }

    /// <inheritdoc />
    public void BlockChannel(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        _windowsPhoneDeviceProvider.BlockChannel(deviceId);
    }

    /// <inheritdoc />
    public TpmVersion GetTpmVersion(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        return _windowsPhoneDeviceProvider.GetTpmVersion(deviceId);
    }

    /// <inheritdoc />
    public Dictionary<int, TpmVersion> GetTpmVersions(IEnumerable<int> deviceIds)
    {
        ArgumentNullException.ThrowIfNull(deviceIds);

        return _windowsPhoneDeviceProvider.GetTpmVersions(deviceIds);
    }

    /// <inheritdoc />
    public IEnumerable<int> GetWnsListeners()
    {
        return _windowsPhoneDeviceProvider.GetWnsListeners();
    }

    private bool AllowDeviceSnapshotCaching()
    {
        return _settingsManagementService.GetSetting<bool>(FeatureName,
            SettingName,
            CachingOptions.AllowCaching,
            defaultValue: true);
    }
}
