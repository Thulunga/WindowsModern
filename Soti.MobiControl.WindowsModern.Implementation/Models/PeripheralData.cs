namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Peripheral data model class.
/// </summary>
internal sealed class PeripheralData
{
    /// <summary>
    /// Gets or sets the Peripheral id of peripheral.
    /// </summary>
    public int PeripheralId { get; set; }

    /// <summary>
    /// Get or set the Name of the Windows device peripheral.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Get or set the ManufacturerId of the peripheral.
    /// </summary>
    public short ManufacturerId { get; set; }

    /// <summary>
    /// Gets or sets the PeripheralType of peripheral.
    /// </summary>
    public short PeripheralTypeId { get; set; }
}
