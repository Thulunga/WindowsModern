namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Membership of local users in local groups on windows devices
/// </summary>
internal sealed class WindowsDeviceLocalGroupUserData
{
    /// <summary>
    /// Gets or sets UserId.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets GroupId.
    /// </summary>
    public int GroupId { get; set; }
}
