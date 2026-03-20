namespace Soti.MobiControl.WindowsModern.Models.Enums;

/// <summary>
/// Wifi Network Authentication Type Enum.
/// </summary>
public enum NetworkAuthenticationType : byte
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0,

    /// <summary>
    /// The wifi NetworkAuthentication is Open.
    /// </summary>
    Open = 1,

    /// <summary>
    /// The wifi NetworkAuthentication is Shared.
    /// </summary>
    Shared = 2,

    /// <summary>
    /// The wifi NetworkAuthentication is WPA.
    /// </summary>
    WPA = 3,

    /// <summary>
    /// The wifi NetworkAuthentication is WPAPSK.
    /// </summary>
    WPAPSK = 4,

    /// <summary>
    /// The wifi NetworkAuthentication is WPA2.
    /// </summary>
    WPA2 = 5,

    /// <summary>
    /// The wifi NetworkAuthentication is WPA2PSK.
    /// </summary>
    WPA2PSK = 6,

    /// <summary>
    /// The wifi NetworkAuthentication is WPA3.
    /// </summary>
    WPA3 = 7,

    /// <summary>
    /// The wifi NetworkAuthentication is WPA3ENT192.
    /// </summary>
    WPA3ENT192 = 8,

    /// <summary>
    /// The wifi NetworkAuthentication is WPA3ENT.
    /// </summary>
    WPA3ENT = 9,

    /// <summary>
    /// The wifi NetworkAuthentication is WPA3SAE.
    /// </summary>
    WPA3SAE = 10,

    /// <summary>
    /// The wifi NetworkAuthentication is OWE.
    /// </summary>
    OWE = 11
}
