namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Local group keys on windows devices.
/// </summary>
internal sealed class WindowsDeviceLocalGroupData
{
    /// <summary>
    /// Gets or sets GroupId.
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Gets or sets windows DeviceId.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets GroupNameId.
    /// </summary>
    public int GroupNameId { get; set; }

    /// <summary>
    /// Gets or sets IsAdminGroup.
    /// Default value: false.
    /// </summary>
    public bool IsAdminGroup { get; set; }
}
