namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Represents BIOS boot priority settings for a device.
/// </summary>
public sealed class BiosBootPrioritySummary
{
    /// <summary>
    /// The ID of the device this boot priority belongs to.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// The name of the boot priority item (e.g., "USB", "Network", "HDD").
    /// </summary>
    public string BootPriority { get; set; }
}
