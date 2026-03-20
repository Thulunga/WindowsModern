namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Device Hardware Snapshot.
/// </summary>
public sealed class DeviceHardwareSnapshot
{
    /// <summary>
    /// Gets or sets CreationClassName of hardware component.
    /// </summary>
    public string CreationClassName { get; set; }

    /// <summary>
    /// Gets or sets Name of hardware component.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets Manufacturer of hardware component.
    /// </summary>
    public string Manufacturer { get; set; }

    /// <summary>
    /// Gets or sets SerialNumber of hardware component.
    /// </summary>
    public string SerialNumber { get; set; }
}
