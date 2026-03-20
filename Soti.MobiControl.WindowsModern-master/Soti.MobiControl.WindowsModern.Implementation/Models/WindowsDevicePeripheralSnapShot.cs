namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Windows device Peripheral snapshot Keys.
/// </summary>
internal sealed class WindowsDevicePeripheralSnapShot
{
    /// <summary>
    /// Gets or sets Peripheral Name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets Peripheral DeviceId.
    /// </summary>
    public string PeripheralDeviceId { get; set; }

    /// <summary>
    /// Gets or sets DeviceID.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets Peripheral Manufacturer name.
    /// </summary>
    public string Manufacturer { get; set; }

    /// <summary>
    /// Gets or sets Peripheral Version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets Manufacturer id of peripheral.
    /// </summary>
    public short ManufacturerId { get; set; }

    /// <summary>
    /// Gets or sets Category of peripheral.
    /// </summary>
    public string Category { get; set; }
}
