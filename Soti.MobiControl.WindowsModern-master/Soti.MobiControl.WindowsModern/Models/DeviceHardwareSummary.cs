using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Device Hardware Model
/// </summary>
public sealed class DeviceHardwareSummary
{
    /// <summary>
    /// Gets or sets hardware manufacturer name
    /// </summary>
    public string HardwareManufacturerName { get; set; }

    /// <summary>
    /// Gets or sets hardware name
    /// </summary>
    public string HardwareName { get; set; }

    /// <summary>
    /// Gets or sets hardware type id
    /// </summary>
    public DeviceHardwareType DeviceHardwareType { get; set; }

    /// <summary>
    /// Gets or sets hardware status
    /// </summary>
    public HardwareStatus HardwareStatus { get; set; }

    /// <summary>
    /// Gets or sets hardware serial number
    /// </summary>
    public string HardwareSerialNumber { get; set; }
}
