using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// DeviceGroupThreatHistoryData.
/// </summary>
internal sealed class DeviceGroupThreatHistoryData
{
    /// <summary>
    /// Gets or sets ExternalThreatId.
    /// </summary>
    public long ExternalThreatId { get; set; }

    /// <summary>
    /// Gets or sets TypeId.
    /// </summary>
    public byte TypeId { get; set; }

    /// <summary>
    /// Gets or sets SeverityId.
    /// </summary>
    public byte? SeverityId { get; set; }

    /// <summary>
    /// Gets or sets InitialDetectionTime.
    /// </summary>
    public DateTime InitialDetectionTime { get; set; }

    /// <summary>
    /// Gets or sets LastStatusChangeTime.
    /// </summary>
    public DateTime? LastStatusChangeTime { get; set; }

    /// <summary>
    /// Gets or sets NoOfDevices.
    /// </summary>
    public int NoOfDevices { get; set; }
}
