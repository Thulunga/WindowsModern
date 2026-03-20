namespace Soti.MobiControl.WindowsModern.Models.Enums;

/// <summary>
/// Enum representing different categories of antivirus scan range.
/// </summary>
public enum AntivirusScanPeriodSubType : byte
{
    /// <summary>
    /// Within 24 hours.
    /// </summary>
    Within24Hrs = 0,

    /// <summary>
    /// Within 7 days.
    /// </summary>
    Within7Days = 1,

    /// <summary>
    /// More than 30 days.
    /// </summary>
    MoreThan30Days = 2,

    /// <summary>
    /// Custom.
    /// </summary>
    Custom = 3
}
