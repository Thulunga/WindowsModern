using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Represent Device Peripheral Summary
/// </summary>
public class DevicePeripheralSummary
{
    /// <summary>
    /// Gets or sets Name of peripheral
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets Manufacturer of peripheral
    /// </summary>
    public string Manufacturer { get; set; }

    /// <summary>
    /// Gets or sets version
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets Status of peripheral
    /// </summary>
    public DevicePeripheralStatus Status { get; set; }

    /// <summary>
    /// Gets or sets peripheral type.
    /// </summary>
    public PeripheralType PeripheralType { get; set; }
}
