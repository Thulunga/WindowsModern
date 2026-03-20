using Soti.MobiControl.WindowsModern.Implementation.Models;
using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Data provider interface for Peripheral entity.
/// </summary>
internal interface IPeripheralDataProvider
{
    /// <summary>
    /// Get peripheral data by peripheralId.
    /// </summary>
    /// <param name="peripheralId">The peripheral id.</param>
    /// <returns>The peripheral data object.</returns>
    PeripheralData GetPeripheralData(int peripheralId);

    /// <summary>
    /// Insert data into peripheral table
    /// </summary>
    /// <param name="data">The peripheral data.</param>
    int InsertPeripheralData(PeripheralData data);

    /// <summary>
    /// Get all peripheral data.
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<PeripheralData> GetAllPeripheralData();

    /// <summary>
    /// Bulk delete peripheral table.
    /// </summary>
    /// /// <param name="peripheralId">The peripheral id.</param>
    void BulkDeletePeripheralData(int peripheralId);

    /// <summary>
    /// Update the peripheral type.
    /// </summary>
    /// <param name="peripheralId">The peripheral id.</param>
    /// <param name="peripheralTypeId">The peripheral type id.</param>
    void UpdatePeripheralType(int peripheralId, short peripheralTypeId);
}
