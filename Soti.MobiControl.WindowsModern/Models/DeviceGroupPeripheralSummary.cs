using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Device Group Peripheral Summary model class.
/// </summary>
public sealed class DeviceGroupPeripheralSummary
{
    /// <summary>
    /// Gets or sets the device id.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets the peripheral name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the manufacturer name.
    /// </summary>
    public string Manufacturer { get; set; }

    /// <summary>
    /// Gets or sets the version of peripheral driver.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the peripheral status.
    /// </summary>
    public DevicePeripheralStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the peripheral type.
    /// </summary>
    public short PeripheralType { get; set; }

    /// <summary>
    /// Gets or sets the Peripheral id of peripheral.
    /// </summary>
    public int PeripheralId { get; set; }
}
