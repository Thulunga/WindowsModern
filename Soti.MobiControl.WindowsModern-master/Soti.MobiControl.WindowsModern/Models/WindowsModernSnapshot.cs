using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Windows Modern Snapshot Model.
/// </summary>
public sealed class WindowsModernSnapshot
{
    /// <summary>
    /// Device Id of the device.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Wifi Subnet.
    /// </summary>
    public string WifiSubnet { get; set; }

    /// <summary>
    /// Wifi Mode Id.
    /// </summary>
    public WifiModeType WifiModeId { get; set; }

    /// <summary>
    /// Wifi Network Authentication Id.
    /// </summary>
    public NetworkAuthenticationType NetworkAuthenticationId { get; set; }

    /// <summary>
    /// Wireless Lan Mode Id.
    /// </summary>
    public WirelessLanModeType WirelessLanModeId { get; set; }

    /// <summary>
    /// Windows SandBox Enabled Status.
    /// </summary>
    public bool WindowsSandBoxEnabledStatus { get; set; }

    /// <summary>
    /// Represents Bios Password Status.
    /// </summary>
    public BiosPasswordStatusType BiosPasswordStatus { get; set; }
}
