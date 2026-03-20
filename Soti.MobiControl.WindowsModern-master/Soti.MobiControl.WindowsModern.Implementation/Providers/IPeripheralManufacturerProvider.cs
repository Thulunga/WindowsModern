using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Data provider interface for PeripheralManufacturer entity.
/// </summary>
internal interface IPeripheralManufacturerProvider
{
    /// <summary>
    /// Get the peripheral's manufacturer
    /// </summary>
    /// <param name="peripheralManufacturerCode">The peripheral manufacturer code.</param>
    /// <returns> PeripheralManufacturerData.</returns>
    PeripheralManufacturerData GetPeripheralManufacturerByManufacturerCode(string peripheralManufacturerCode);

    /// <summary>
    /// Insert data into peripheral manufacturer table.
    /// </summary>
    /// <param name="data">The peripheral manufacturer data.</param>
    void InsertPeripheralManufacturerData(PeripheralManufacturerData data);

    /// <summary>
    /// Get Peripheral Manufacturer Data.
    /// </summary>
    /// <returns>List of peripheral manufacturer data.</returns>
    IReadOnlyList<PeripheralManufacturerData> GetAllPeripheralManufacturerData();
}
