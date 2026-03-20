using System;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Antivirus Group Threat Details.
/// </summary>
public sealed class AntivirusGroupThreatHistoryDetails
{
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

    /// <summary>
    /// Gets or sets InitialDetectionTime.
    /// </summary>
    public DateTime InitialDetectionTime { get; set; }

    /// <summary>
    /// Gets or sets LastStatusChangeTime.
    /// </summary>
    public DateTime? LastStatusChangeTime { get; set; }

    /// <summary>
    /// Gets or sets NumberOfDevices.
    /// </summary>
    public int NumberOfDevices { get; set; }
}
