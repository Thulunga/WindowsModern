using System;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Antivirus Threat Status Device Details.
/// </summary>
public sealed class AntivirusThreatStatusDeviceDetails
{
    /// <summary>
    /// Gets or sets DevId.
    /// </summary>
    public string DevId { get; set; }

    /// <summary>
    /// Gets or sets LastStatusChangeTime.
    /// </summary>
    public DateTime LastStatusChangeTime { get; set; }

    /// <summary>
    /// Gets or sets ExternalThreatId.
    /// </summary>
    public long ExternalThreatId { get; set; }

    /// <summary>
    /// Gets or sets ThreatCategory.
    /// </summary>
    public AntivirusThreatType ThreatCategory { get; set; }

    /// <summary>
    /// Gets or sets Severity.
    /// </summary>
    public AntivirusThreatSeverity? Severity { get; set; }
}
