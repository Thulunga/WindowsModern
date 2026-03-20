using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// Device Hardware Summary.
/// </summary>
public sealed class DeviceHardwareSummary
{
    /// <summary>
    /// Gets or sets manufacturer name of the hardware.
    /// </summary>
    public string HardwareManufacturerName { get; set; }

    /// <summary>
    /// Gets or sets hardware Name.
    /// </summary>
    public string HardwareName { get; set; }

    /// <summary>
    /// Gets or sets type of hardware.
    /// </summary>
    public DeviceHardwareType DeviceHardwareType { get; set; }

    /// <summary>
    /// Gets or sets status for hardware.
    /// </summary>
    public HardwareStatus HardwareStatus { get; set; }

    /// <summary>
    /// Gets or sets serial number of hardware.
    /// </summary>
    public string HardwareSerialNumber { get; set; }
}
