namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Peripheral manufacturer data model class.
/// </summary>
internal sealed class PeripheralManufacturerData
{
    /// <summary>
    /// Get or set the ManufacturerId of the peripheral.
    /// </summary>
    public short ManufacturerId { get; set; }

    /// <summary>
    /// Get or set the Manufacturer Name of the Windows device peripheral.
    /// </summary>
    public string ManufacturerName { get; set; }

    /// <summary>
    /// Get or set the Manufacturer code of the Windows device peripheral.
    /// </summary>
    public string ManufacturerCode { get; set; }
}
