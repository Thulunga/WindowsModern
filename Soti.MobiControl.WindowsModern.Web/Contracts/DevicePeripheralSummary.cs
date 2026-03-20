using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// Device Peripheral contract
/// </summary>
public sealed class DevicePeripheralSummary
{
    /// <summary>
    /// Gets or sets Peripheral Name of device.
    /// </summary>
    public string PeripheralName { get; set; }

    /// <summary>
    /// Gets or sets Peripheral Manufacturer of device.
    /// </summary>
    public string PeripheralManufacturer { get; set; }

    /// <summary>
    /// Gets or sets Peripheral Version of device.
    /// </summary>
    public string PeripheralVersion { get; set; }

    /// <summary>
    /// Gets or sets Peripheral Status of device.
    /// </summary>
    public DevicePeripheralStatus PeripheralStatus { get; set; }

    /// <summary>
    /// Gets or sets the Peripheral type.
    /// </summary>
    public PeripheralType PeripheralType { get; set; }
}
