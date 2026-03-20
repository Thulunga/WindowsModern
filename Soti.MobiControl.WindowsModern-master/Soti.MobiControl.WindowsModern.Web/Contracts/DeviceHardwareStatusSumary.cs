using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
///  Hardware Status Contract.
/// </summary>
public sealed class DeviceHardwareStatusSumary
{
    /// <summary>
    /// Gets or sets hardware status.
    /// </summary>
    public HardwareStatus HardwareStatus { get; set; }

    /// <summary>
    /// Gets or sets device hardware serial number
    /// </summary>
    public string HardwareSerialNumber { get; set; }
}
