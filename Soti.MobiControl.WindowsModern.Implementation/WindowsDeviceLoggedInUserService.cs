using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Soti.Diagnostics;
using Soti.MobiControl.DeploymentServerExtensions.Contracts;
using Soti.MobiControl.Events;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Implementation.Events;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.SensitiveDataProtection;
using Soti.Transactions;

namespace Soti.MobiControl.WindowsModern.Implementation;

internal sealed class WindowsDeviceLoggedInUserService : IWindowsDeviceLoggedInUserService
{
    private readonly IWindowsDeviceUserProvider _windowsDeviceUserProvider;
    private readonly IWindowsDeviceService _windowsDeviceService;
    private readonly IProgramTrace _programTrace;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IDataKeyService _dataKeyService;
    private readonly Lazy<IMessagePublisher> _messagePublisher;
    private readonly Lazy<IDeviceAdministrationCallback> _deviceAdministrationCallback;

    private static readonly MemoryCache MemoryCache = new(new MemoryCacheOptions
    {
        ExpirationScanFrequency = TimeSpan.FromMinutes(15)
    });

    public WindowsDeviceLoggedInUserService(
        IWindowsDeviceUserProvider windowsDeviceUserProvider,
        IWindowsDeviceService windowsDeviceService,
        IProgramTrace programTrace,
        IEventDispatcher eventDispatcher,
        IDataKeyService dataKeyService,
        Lazy<IMessagePublisher> messagePublisher,
        Lazy<IDeviceAdministrationCallback> deviceAdministrationCallback)
    {
        _windowsDeviceUserProvider = windowsDeviceUserProvider ?? throw new ArgumentNullException(nameof(windowsDeviceUserProvider));
        _windowsDeviceService = windowsDeviceService ?? throw new ArgumentNullException(nameof(windowsDeviceService));
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _dataKeyService = dataKeyService ?? throw new ArgumentNullException(nameof(dataKeyService));
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        _deviceAdministrationCallback = deviceAdministrationCallback ?? throw new ArgumentNullException(nameof(deviceAdministrationCallback));
    }

    /// <inheritdoc />
    public WindowsDeviceLoggedInUserModel GetWindowsDeviceLoggedInUserData(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        _programTrace.Write(TraceLevel.Verbose, nameof(WindowsDeviceLoggedInUserService), $"Fetching the Windows Logged-In User for {deviceId}");

        if (MemoryCache.TryGetValue(deviceId, out WindowsDeviceLoggedInUserModel loggedInUserData))
        {
            return loggedInUserData;
        }

        loggedInUserData = _windowsDeviceUserProvider.GetLoggedInUserDataByDeviceId(deviceId)?.ToWindowsDeviceUserModel();
        MemoryCache.Set(deviceId, loggedInUserData,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

        return loggedInUserData;
    }

    /// <inheritdoc />
    public void ProcessWindowsDeviceLoggedInUser(WindowsDeviceLoggedInUserModel windowsDeviceLoggedInUserModel)
    {
        ArgumentNullException.ThrowIfNull(windowsDeviceLoggedInUserModel);

        var devId = windowsDeviceLoggedInUserModel.DevId;
        if (string.IsNullOrWhiteSpace(devId))
        {
            throw new ArgumentException(nameof(devId));
        }

        if (windowsDeviceLoggedInUserModel.IsUserLoggedIn && (string.IsNullOrWhiteSpace(windowsDeviceLoggedInUserModel.UserName) || string.IsNullOrWhiteSpace(windowsDeviceLoggedInUserModel.UserSID)))
        {
            _eventDispatcher.DispatchEvent(new LoggedInUserFailureEvent(windowsDeviceLoggedInUserModel.DeviceId, devId));
            _programTrace.Write(
                TraceLevel.Error,
                nameof(WindowsDeviceLoggedInUserService),
                $"Failed to process the windows logged-in user information for {windowsDeviceLoggedInUserModel.DeviceId}");
            return;
        }

        var windowsDeviceLoggedInUserData = GetWindowsDeviceLoggedInUserData(windowsDeviceLoggedInUserModel.DeviceId);

        if (!windowsDeviceLoggedInUserModel.IsUserLoggedIn)
        {
            if (windowsDeviceLoggedInUserData != null)
            {
                _eventDispatcher.DispatchEvent(
                    new NoUserLoggedInEvent(
                        windowsDeviceLoggedInUserModel.DeviceId,
                        devId,
                        windowsDeviceLoggedInUserData.UserName,
                        windowsDeviceLoggedInUserData.UserFullName));
                LogOffWindowsDeviceUser(windowsDeviceLoggedInUserModel.DeviceId);
            }

            return;
        }

        if (windowsDeviceLoggedInUserData == null)
        {
            _eventDispatcher.DispatchEvent(
                new UserLoggedInEvent(
                    windowsDeviceLoggedInUserModel.DeviceId,
                    devId,
                    windowsDeviceLoggedInUserModel.UserName,
                    windowsDeviceLoggedInUserModel.UserFullName));
        }
        else
        {
            var isSameUser = windowsDeviceLoggedInUserModel.UserSID == windowsDeviceLoggedInUserData.UserSID &&
                             windowsDeviceLoggedInUserModel.UserName == windowsDeviceLoggedInUserData.UserName &&
                             windowsDeviceLoggedInUserModel.UserFullName == windowsDeviceLoggedInUserData.UserFullName;

            if (isSameUser)
            {
                return;
            }

            _eventDispatcher.DispatchEvent(
                new LoggedInUserChangeDetectedEvent(
                    windowsDeviceLoggedInUserModel.DeviceId,
                    devId,
                    windowsDeviceLoggedInUserModel.UserName,
                    windowsDeviceLoggedInUserModel.UserFullName,
                    windowsDeviceLoggedInUserData.UserName,
                    windowsDeviceLoggedInUserData.UserFullName));
        }

        var dataKey = _dataKeyService.GetKey();
        var windowsDeviceUserData = windowsDeviceLoggedInUserModel.ToWindowsDeviceUserData(dataKey.Id);
        TransactionHelper.RunInTransaction(() =>
        {
            _windowsDeviceUserProvider.ModifyLoggedInUser(windowsDeviceUserData);
            _windowsDeviceService.UpdateLoggedInUserLastCheckInTime(windowsDeviceLoggedInUserModel.DeviceId, DateTime.UtcNow);
            MemoryCache.Remove(windowsDeviceLoggedInUserModel.DeviceId);
            _deviceAdministrationCallback.Value.SendInvalidateLoggedInUserCacheNotification(windowsDeviceLoggedInUserModel.DeviceId);
        });

        _programTrace.Write(TraceLevel.Verbose, nameof(WindowsDeviceLoggedInUserService), $"Windows Logged-In User information updated for {windowsDeviceLoggedInUserModel.DeviceId}");
    }

    /// <inheritdoc />
    public void LogOffWindowsDeviceUser(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _windowsDeviceUserProvider.LogOffUserByDeviceId(deviceId);
        MemoryCache.Remove(deviceId);
        _deviceAdministrationCallback.Value.SendInvalidateLoggedInUserCacheNotification(deviceId);

        _programTrace.Write(TraceLevel.Verbose, nameof(WindowsDeviceLoggedInUserService), $"Log Off Windows Device User for {deviceId}");
    }

    /// <inheritdoc />
    public IEnumerable<WindowsDeviceLoggedInUserModel> GetWindowsDeviceLoggedInUserDataByDeviceIds(IEnumerable<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        deviceIds = deviceIds.ToList();
        if (!deviceIds.Any())
        {
            throw new ArgumentException(nameof(deviceIds));
        }

        _programTrace.Write(TraceLevel.Verbose, nameof(WindowsDeviceLoggedInUserService), $"Fetching the Windows Logged-In User for {deviceIds}");
        return _windowsDeviceUserProvider.GetLoggedInUserDataByDeviceIds(deviceIds)?.Select(x =>
        {
            return x.ToWindowsDeviceUserModel();
        });
    }

    /// <inheritdoc />
    public void InvalidateLoggedInUserCache(int deviceId, bool notify)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);
        MemoryCache.Remove(deviceId);
        if (notify)
        {
            PublishInvalidateCacheMessage(deviceId);
        }
    }

    private void PublishInvalidateCacheMessage(int deviceId)
    {
        _messagePublisher.Value.Publish(
            new InvalidateLoggedInUserCacheMessage
            {
                DeviceId = deviceId
            },
            ApplicableServer.Dse,
            ApplicableServer.Ms);
    }
}
