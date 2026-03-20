namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Represents the boot priority configuration for a specific Windows device.
/// </summary>
public sealed class WindowsDeviceBootPriority
{
    /// <summary>
    /// Gets or sets the ID of the device associated with the boot priority setting.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the boot priority item (e.g., USB, HDD, PXE).
    /// </summary>
    public int BootPriorityId { get; set; }

    /// <summary>
    /// Gets or sets the order in which this boot priority should be applied.
    /// </summary>
    public int BootOrder { get; set; }
}
