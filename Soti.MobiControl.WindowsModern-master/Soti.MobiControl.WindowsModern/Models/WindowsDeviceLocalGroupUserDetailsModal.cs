namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Local user details modal associated with a local group
/// </summary>
public sealed class WindowsDeviceLocalGroupUserDetailsModal
{
    /// <summary>
    /// Gets or sets GroupNameId.
    /// </summary>
    public int GroupNameId { get; set; }

    /// <summary>
    /// Gets or sets DeviceId.
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
    /// Gets or sets IsAdminGroup.
    /// Default value: false.
    /// </summary>
    public bool IsAdminGroup { get; set; }
}
