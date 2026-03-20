namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Local user details associated with a local group
/// </summary>
internal sealed class WindowsDeviceLocalGroupUserDetailsData
{
    /// <summary>
    /// Get or set the ID of device.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the Windows device user.
    /// </summary>
    public int WindowsDeviceUserId { get; set; }

    /// <summary>
    /// Gets or sets the UserSID of the user.
    /// </summary>
    public string UserSid { get; set; }

    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the data key ID associated with the user.
    /// </summary>
    public int DataKeyId { get; set; }

    /// <summary>
    /// Gets or sets IsAdminGroup.
    /// Default value: false.
    /// </summary>
    public bool IsAdminGroup { get; set; }
}
