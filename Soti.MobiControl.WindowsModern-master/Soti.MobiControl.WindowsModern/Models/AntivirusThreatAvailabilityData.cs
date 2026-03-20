using System;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Antivirus Threat Availability Data.
/// </summary>
public sealed class AntivirusThreatAvailabilityData
{
    /// <summary>
    /// Gets or sets the DeviceId.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets LastAntivirusSyncTime.
    /// </summary>
    public DateTime? LastAntivirusSyncTime { get; set; }

    /// <summary>
    /// Gets or sets IsThreatAvailable.
    /// </summary>
    public bool IsThreatAvailable { get; set; }
}
