namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Represent Device Windows Local Group Search Data Summary.
/// </summary>
public sealed class DeviceWindowsLocalGroupSearchDataSummary
{
    /// <summary>
    /// Gets or sets windows DeviceId.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets GroupNameId.
    /// </summary>
    public int GroupNameId { get; set; }

    /// <summary>
    /// Gets or sets ModificationWatermark.
    /// </summary>
    public short ModificationWatermark { get; set; }
}
