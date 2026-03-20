using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Windows device peripheral data model class.
/// </summary>
internal sealed class WindowsDevicePeripheralData
{
    /// <summary>
    /// Get or set the ID of device.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Get or set peripheralId
    /// </summary>
    public int PeripheralId { get; set; }

    /// <summary>
    /// Get or set the status of the Windows device peripheral.
    /// </summary>
    public DevicePeripheralStatus Status { get; set; }

    /// <summary>
    /// Get or set the version of the Windows device peripheral.
    /// </summary>
    public string Version { get; set; }
}
