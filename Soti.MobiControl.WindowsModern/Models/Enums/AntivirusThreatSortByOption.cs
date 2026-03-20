namespace Soti.MobiControl.WindowsModern.Models.Enums;

/// <summary>
/// Antivirus threat sort options.
/// </summary>
public enum AntivirusThreatSortByOption
{
    /// <summary>
    /// Threat name.
    /// </summary>
    ThreatName,

    /// <summary>
    /// Antivirus threat initial detection time.
    /// </summary>
    AntivirusThreatInitialDetectionTime,

    /// <summary>
    /// Antivirus threat last status change time.
    /// </summary>
    AntivirusThreatLastStatusChangeTime,

    /// <summary>
    /// Current detection count.
    /// </summary>
    CurrentDetectionCount
}
