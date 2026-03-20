namespace Soti.MobiControl.WindowsModern.Models.Enums;

/// <summary>
/// BIOS Password Status Id. Enum of:
/// 0 - NotConfigured - BIOS Payload is not configured,
/// 1 - Enforced - BIOS Password is enforced by Admin,
/// 2 - Prompted - BIOS password is enforced via prompt.
/// </summary>
public enum BiosPasswordStatusType
{
    /// <summary>
    ///  BIOS Payload is not configured.
    /// </summary>
    NotConfigured = 0,

    /// <summary>
    /// Enforced - BIOS Password is enforced by Admin.
    /// </summary>
    Enforced = 1,

    /// <summary>
    /// BIOS password is enforced via prompt.
    /// </summary>
    Prompted = 2
}
