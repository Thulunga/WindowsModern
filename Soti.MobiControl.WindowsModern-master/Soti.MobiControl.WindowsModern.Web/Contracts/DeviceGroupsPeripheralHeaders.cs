using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// Device groups peripheral headers.
/// </summary>
public sealed class DeviceGroupsPeripheralHeaders
{
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
    public PeripheralType PeripheralType { get; set; }

    /// <summary>
    /// Gets or sets the number of device through which peripheral is connected or disconnected.
    /// </summary>
    public int DeviceCount { get; set; }
}
