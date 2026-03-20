namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Windows device user SID data.
/// </summary>
internal sealed class WindowsDeviceUserSidData
{
    /// <summary>
    /// Gets or sets SID of the user.
    /// </summary>
    public string UserSID { get; set; }

    /// <summary>
    /// Gets or sets ID of the device.
    /// </summary>
    public int DeviceId { get; set; }
}
