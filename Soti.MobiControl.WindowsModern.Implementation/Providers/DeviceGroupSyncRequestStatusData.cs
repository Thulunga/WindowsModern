using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Device group sync request status data.
/// </summary>
internal sealed class DeviceGroupSyncRequestStatusData
{
    /// <summary>
    /// Gets or sets device group Id.
    /// </summary>
    public int DeviceGroupId { get; set; }

    /// <summary>
    /// Gets or sets when the device group sync completed.
    /// </summary>
    public DateTime? CompletedOn { get; set; }
}
