using Soti.MobiControl.WindowsModern.Implementation.Models;
using System;
using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Data provider interface for WindowsDevicePeripheral entity.
/// </summary>
internal interface IWindowsDevicePeripheralDataProvider
{
    /// <summary>
    /// Gets device peripherals Data.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <returns> List of WindowsDevicePeripheralData.</returns>
    IReadOnlyList<WindowsDevicePeripheralData> GetDevicePeripheralSummaryByDeviceId(int deviceId);

    /// <summary>
    /// Delete device peripheral data based on peripheralId
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    void DeleteDevicePeripheralData(int deviceId);

    /// <summary>
    /// Updates the windows device peripheral data.
    /// Update all existing peripheral data for device and add new data.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="devicePeripherals">The device peripherals.</param>
    /// <returns>List of peripheral which is connected and disconnected and it's old and new status.</returns>
    IReadOnlyDictionary<int, (byte, byte)> BulkModify(int deviceId, IEnumerable<WindowsDevicePeripheralData> devicePeripherals);

    /// <summary>
    /// Clean up all disconnected peripheral data fro device peripheral table.
    /// </summary>
    /// <returns>The no of device peripheral data cleaned up.</returns>
    int CleanUpObsoleteWindowsPeripheralData(out IReadOnlyList<WindowsDevicePeripheralData> windowsData);

    /// <summary>
    /// Delete Device Peripheral by DeviceId.
    /// </summary>
    IReadOnlyList<WindowsDevicePeripheralData> DeleteDevicePeripheralByDeviceIdAndGetPeripheralData(int deviceId);

    /// <summary>
    /// Update the entry in DevicePeripheral table for particular peripheralId and DeviceId.
    /// </summary>
    void Update(WindowsDevicePeripheralData windowsDevicePeripheralData, DateTime lastKnownConnectTime);
}
