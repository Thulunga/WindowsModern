using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Device Hardware Data
/// </summary>
internal sealed class DeviceHardwareData
{
    /// <summary>
    /// Gets or sets device hardware manufacturer name
    /// </summary>
    public int DeviceHardwareStatusId { get; set; }

    /// <summary>
    /// Gets or sets device hardware manufacturer name
    /// </summary>
    public int DeviceHardwareId { get; set; }

    /// <summary>
    /// Gets or sets device hardware manufacturer name
    /// </summary>
    public string DeviceHardwareManufacturerName { get; set; }

    /// <summary>
    /// Gets or sets device hardware name
    /// </summary>
    public string DeviceHardwareName { get; set; }

    /// <summary>
    /// Gets or sets device hardware type id
    /// </summary>
    public int DeviceHardwareTypeId { get; set; }

    /// <summary>
    /// Gets or sets hardware status id
    /// </summary>
    public int HardwareStatusId { get; set; }

    /// <summary>
    /// Gets or sets device hardware serial number
    /// </summary>
    public string DeviceHardwareSerialNumber { get; set; }

    /// <summary>
    /// Gets or sets last modified date
    /// </summary>
    public DateTime LastModifiedDate { get; set; }
}
