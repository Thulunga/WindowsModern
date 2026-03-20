using System;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Represents Device Group Sync Info.
/// </summary>
public sealed class DeviceGroupSyncInfo
{
    /// <summary>
    /// Gets or sets SyncStatus.
    /// </summary>
    public SyncRequestStatus SyncStatus { get; set; }

    /// <summary>
    /// Gets or sets CompletedOn.
    /// </summary>
    public DateTime? CompletedOn { get; set; }
}
