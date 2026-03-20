using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Device Group Sync Request- Sync request initiated by a user to get some data from all devices of particular group.
/// </summary>
internal sealed class DeviceGroupSyncRequestData
{
    /// <summary>
    /// Device Group Sync Request Id, PK, internal id.
    /// </summary>
    public int DeviceGroupSyncRequestId { get; set; }

    /// <summary>
    /// Device group internal identifier. Reference to DeviceGroup.DeviceGroupId.
    /// </summary>
    public int DeviceGroupId { get; set; }

    /// <summary>
    /// Sync Request Type Id. Reference to SyncRequestType.SyncRequestTypeId.
    /// </summary>
    public byte SyncRequestType { get; set; }

    /// <summary>
    /// Sync Request Status internal identifier. Reference to SyncRequestStatus.SyncRequestStatusId.
    /// </summary>
    public byte SyncRequestStatus { get; set; }

    /// <summary>
    /// Datetime when Request was started
    /// Table default value: DateTime.UtcNow.
    /// </summary>
    public DateTime StartedOn { get; set; }

    /// <summary>
    /// Datetime when Request was completed.
    /// </summary>
    public DateTime? CompletedOn { get; set; }
}
