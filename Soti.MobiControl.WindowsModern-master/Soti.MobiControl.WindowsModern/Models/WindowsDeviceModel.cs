using Soti.MobiControl.WindowsModern.Models.Enums;
using System;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Windows Device Model.
/// </summary>
public sealed class WindowsDeviceModel
{
    /// <summary>
    /// Device Id of the device.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Represents Lock status of a device.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Decrypted passcode.
    /// </summary>
    public string Passcode { get; set; }

    /// <summary>
    /// The OS Image ID.
    /// </summary>
    public int? OsImageId { get; set; }

    /// <summary>
    /// The OS Image Deployment Time.
    /// </summary>
    public DateTime? OsImageDeploymentTime { get; set; }

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
    /// Represents Bios Password Status Type.
    /// </summary>
    public BiosPasswordStatusType BiosPasswordStatus { get; set; }
}
