using Soti.MobiControl.WindowsModern.Models;
using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern;

/// <summary>
/// Windows Device Peripheral service interface.
/// </summary>
public interface IWindowsDevicePeripheralService
{
    /// <summary>
    /// Returns the device peripherals info.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns>DevicePeripheralSummary</returns>
    public IEnumerable<DevicePeripheralSummary> GetDevicePeripheralsSummaryInfo(int deviceId);

    /// <summary>
    /// Processes the Windows peripheral keys data.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// /// <param name="devId">The devId identifier.</param>
    /// <param name="peripheralKeysData">The Windows Peripheral keys data.</param>
    void SynchronizePeripheralDataWithSnapshot(int deviceId, string devId, string peripheralKeysData);

    /// <summary>
    /// Clean up the obsolete windows peripheral data.
    /// </summary>
    /// <returns>No of deleted rows count.</returns>
    int CleanUpObsoleteWindowsPeripheralData();

    /// <summary>
    /// Deletes DevicePeripheral table data on basis of DeviceId
    /// </summary>
    /// <param name="deviceId"></param>
    void CleanUpDevicePeripherals(int deviceId);
}
