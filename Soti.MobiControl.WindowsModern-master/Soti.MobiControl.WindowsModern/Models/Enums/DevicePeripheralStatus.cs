namespace Soti.MobiControl.WindowsModern.Models.Enums;

/// <summary>
/// Peripheral status enum
/// </summary>
public enum DevicePeripheralStatus : byte
{
    /// <summary>
    /// Peripheral is connected
    /// </summary>
    Connected = 1,

    /// <summary>
    /// Peripheral is not connected.
    /// </summary>
    Disconnected = 2,
}
