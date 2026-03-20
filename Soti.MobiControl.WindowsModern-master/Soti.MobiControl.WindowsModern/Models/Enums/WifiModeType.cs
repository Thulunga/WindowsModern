namespace Soti.MobiControl.WindowsModern.Models.Enums;

/// <summary>
/// Wifi Mode Type Enum.
/// </summary>
public enum WifiModeType : byte
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0,

    /// <summary>
    /// The wifi mode is Infrastructure.
    /// </summary>
    ESS = 1,

    /// <summary>
    /// The wifi mode is Ad-hoc.
    /// </summary>
    IBSS = 2
}
