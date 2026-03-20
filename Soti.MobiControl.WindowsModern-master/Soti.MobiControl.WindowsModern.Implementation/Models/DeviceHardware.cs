using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Device Hardware
/// </summary>
internal sealed class DeviceHardware
{
    /// <summary>
    /// Gets or sets DeviceHardwareId.
    /// </summary>
    public int DeviceHardwareId { get; set; }

    /// <summary>
    /// Gets or sets DeviceHardwareName.
    /// </summary>
    public string DeviceHardwareName { get; set; }

    /// <summary>
    /// Gets or sets DeviceHardwareTypeId.
    /// </summary>
    public int DeviceHardwareTypeId { get; set; }

    /// <summary>
    /// Gets or sets DeviceHardwareManufacturer.
    /// </summary>
    public DeviceHardwareManufacturer DeviceHardwareManufacturer { get; set; }
}
