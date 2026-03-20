namespace Soti.MobiControl.WindowsModern.Models.Enums;

/// <summary>
/// Enum representing the severity levels of antivirus threats.
/// </summary>
public enum AntivirusThreatSeverity : byte
{
    /// <summary>
    /// Unknown severity level.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Low severity threat.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Moderate severity threat.
    /// </summary>
    Moderate = 2,

    /// <summary>
    /// High severity threat.
    /// </summary>
    High = 4,

    /// <summary>
    /// Severe threat level.
    /// </summary>
    Severe = 5
}
