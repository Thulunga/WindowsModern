using System;
using System.Collections.Generic;
using System.Linq;
using Soti.MobiControl.DeviceGroupManagement;
using Soti.MobiControl.WindowsModern.Implementation.Exceptions;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.Utilities.Extensions;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <summary>
/// Windows Device Groups Service.
/// </summary>
internal sealed class WindowsDeviceGroupsService : IWindowsDeviceGroupsService
{
    private static readonly List<byte> SyncStatuses =
    [
        (byte)SyncRequestStatus.Queued,
        (byte)SyncRequestStatus.Running,
        (byte)SyncRequestStatus.PartiallyCompleted,
        (byte)SyncRequestStatus.Completed,
    ];
    private readonly IDeviceGroupSyncRequestDataProvider _deviceGroupSyncRequestDataProvider;
    private readonly IDeviceGroupManager _deviceGroupManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDeviceGroupsService"/> class.
    /// </summary>
    /// <param name="deviceGroupSyncRequestDataProvider">The device group sync request data provider.</param>
    /// <param name="deviceGroupManager">The instance of IDeviceGroupManager.</param>
    public WindowsDeviceGroupsService(IDeviceGroupSyncRequestDataProvider deviceGroupSyncRequestDataProvider, IDeviceGroupManager deviceGroupManager)
    {
        _deviceGroupSyncRequestDataProvider = deviceGroupSyncRequestDataProvider ?? throw new ArgumentNullException(nameof(deviceGroupSyncRequestDataProvider));
        _deviceGroupManager = deviceGroupManager ?? throw new ArgumentNullException(nameof(deviceGroupManager));
    }

    /// <inheritdoc />
    public DeviceGroupSyncInfo GetGroupSyncStatus(int deviceGroupId, SyncRequestType syncRequestType)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceGroupId);

        if (!syncRequestType.IsDefined())
        {
            throw new ArgumentOutOfRangeException(nameof(syncRequestType));
        }

        var groupIds = _deviceGroupManager.GetAllAscendantGroups(deviceGroupId).Select(m => m.Id).ToList();
        if (!groupIds.Contains(deviceGroupId))
        {
            groupIds.Add(deviceGroupId);
        }

        var lastSync = _deviceGroupSyncRequestDataProvider.GetDeviceGroupsLastSyncTime(
            groupIds,
            SyncStatuses,
            (byte)syncRequestType
        );

        if (lastSync == null)
        {
            throw WindowsModernException.AntivirusSyncDataNotFound();
        }

        return new DeviceGroupSyncInfo
        {
            SyncStatus = lastSync.CompletedOn == null
                ? SyncRequestStatus.Running
                : SyncRequestStatus.Completed,
            CompletedOn = lastSync.CompletedOn
        };
    }
}
