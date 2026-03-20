namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Represents a BIOS boot priority item with its identifier and name.
/// </summary>
public sealed class WindowsBootPriority
{
    /// <summary>
    /// Gets or sets the unique identifier of the boot priority item.
    /// </summary>
    public int BootPriorityId { get; set; }

    /// <summary>
    /// Gets or sets the name of the boot priority item (e.g., "USB", "HDD", "PXE").
    /// </summary>
    public string BootPriorityName { get; set; }
}
