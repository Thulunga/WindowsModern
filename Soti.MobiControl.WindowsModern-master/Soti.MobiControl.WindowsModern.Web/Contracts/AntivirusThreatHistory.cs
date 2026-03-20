using System;
using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// Antivirus threat history.
/// </summary>
public abstract class AntivirusThreatHistory
{
    /// <summary>
    /// Gets or sets Antivirus threatId.
    /// </summary>
    public long ThreatId { get; set; }

    /// <summary>
    /// Gets or sets Antivirus threat category.
    /// </summary>
    public AntivirusThreatCategory Category { get; set; }

    /// <summary>
    /// Gets or sets Antivirus threat severity.
    /// </summary>
    public AntivirusThreatSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets Initial detection time.
    /// </summary>
    public DateTime InitialDetectionTime { get; set; }

    /// <summary>
    /// Gets or sets Last status change time.
    /// </summary>
    public DateTime LastStatusChangeTime { get; set; }
}
