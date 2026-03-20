using System;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Windows device information.
/// </summary>
internal sealed class WindowsDeviceData
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
    /// Represents Lock status of a device.
    /// </summary>
    public bool IsSandBoxEnabled { get; set; }

    /// <summary>
    /// Encrypted passcode.
    /// </summary>
    public byte[] Passcode { get; set; }

    /// <summary>
    /// DataKey used to encrypt the passcode.
    /// </summary>
    public int? DataKeyId { get; set; }

    /// <summary>
    /// The last check-in time when the windows logged-in user data got updated.
    /// </summary>
    public DateTime? LastCheckInDeviceUserTime { get; set; }

    /// <summary>
    /// The Hardware Id of windows device.
    /// </summary>
    public string HardwareId { get; set; }

    /// <summary>
    /// The Wifi Subnet.
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
    /// Windows defender last quick scan time.
    /// </summary>
    public DateTime? AntivirusLastQuickScanTime { get; set; }

    /// <summary>
    /// Windows defender last full scan time.
    /// </summary>
    public DateTime? AntivirusLastFullScanTime { get; set; }

    /// <summary>
    /// The last sync time when the windows defender data got synced.
    /// </summary>
    public DateTime? AntivirusLastSyncTime { get; set; }

    /// <summary>
    /// The OS Image ID.
    /// </summary>
    public int? OsImageId { get; set; }

    /// <summary>
    /// The OS Image Deployment Time.
    /// </summary>
    public DateTime? OsImageDeploymentTime { get; set; }

    /// <summary>
    /// Represents Bios Password Status Type.
    /// </summary>
    public BiosPasswordStatusType BiosPasswordStatusId { get; set; }
}
