using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Data provider interface for <see cref="DeviceGroupSyncRequestData"/> entity.
/// </summary>
internal interface IDeviceGroupSyncRequestDataProvider
{
    /// <summary>
    /// Selects DeviceGroupSyncRequestData entities using filter.
    /// </summary>
    /// <param name="deviceGroupId">DeviceGroupId filter value.</param>
    /// <param name="syncRequestTypeId">syncRequestTypeId filter value.</param>
    /// <returns>Filtered collection of DeviceGroupSyncRequestData entities.</returns>
    DeviceGroupSyncRequestData GetByDeviceGroupIdSyncRequestTypeId(int deviceGroupId, byte syncRequestTypeId);

    /// <summary>
    /// Retrieves the most recent sync request completion data across the device groups.
    /// </summary>
    /// <param name="deviceGroupIds">The collection of device group Ids.</param>
    /// <param name="syncRequestStatusIds">The collection of sync request status Ids.</param>
    /// <param name="syncRequestTypeId">The sync request type Id.</param>
    /// <returns>Most recent device group Id and its sync completed time.</returns>
    DeviceGroupSyncRequestStatusData GetDeviceGroupsLastSyncTime(IReadOnlyList<int> deviceGroupIds, IReadOnlyList<byte> syncRequestStatusIds, byte syncRequestTypeId);
}
