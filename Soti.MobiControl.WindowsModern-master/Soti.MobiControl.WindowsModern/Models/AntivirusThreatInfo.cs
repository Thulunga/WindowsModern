using System;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// AntivirusThreatInfo.
/// </summary>
public abstract class AntivirusThreatInfo
{
    /// <summary>
    /// Gets or sets ExternalThreatId.
    /// </summary>
    public long ExternalThreatId { get; set; }

    /// <summary>
    /// Gets or sets Type.
    /// </summary>
    public AntivirusThreatType Type { get; set; }

    /// <summary>
    /// Gets or sets Severity.
    /// </summary>
    public AntivirusThreatSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets InitialDetectionTime.
    /// </summary>
    public DateTime InitialDetectionTime { get; set; }

    /// <summary>
    /// Gets or sets LastStatusChangeTime.
    /// </summary>
    public DateTime LastStatusChangeTime { get; set; }
}
