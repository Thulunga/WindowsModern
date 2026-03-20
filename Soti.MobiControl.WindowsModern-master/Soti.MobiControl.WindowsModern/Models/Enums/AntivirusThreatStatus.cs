namespace Soti.MobiControl.WindowsModern.Models.Enums;

/// <summary>
/// Enum representing the status of an antivirus threat.
/// </summary>
public enum AntivirusThreatStatus : byte
{
    /// <summary>
    /// Threat is active.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Action on threat failed.
    /// </summary>
    ActionFailed = 1,

    /// <summary>
    /// Manual steps are required to address the threat.
    /// </summary>
    ManualStepsRequired = 2,

    /// <summary>
    /// Full system scan is required.
    /// </summary>
    FullScanRequired = 3,

    /// <summary>
    /// System reboot is required.
    /// </summary>
    RebootRequired = 4,

    /// <summary>
    /// Remediated but with non-critical failures.
    /// </summary>
    RemediatedWithNonCriticalFailures = 5,

    /// <summary>
    /// Threat has been quarantined.
    /// </summary>
    Quarantined = 6,

    /// <summary>
    /// Threat has been removed.
    /// </summary>
    Removed = 7,

    /// <summary>
    /// Threat has been cleaned.
    /// </summary>
    Cleaned = 8,

    /// <summary>
    /// Threat has been allowed.
    /// </summary>
    Allowed = 9,

    /// <summary>
    /// No status, threat is cleared.
    /// </summary>
    NoStatusCleared = 10
}
