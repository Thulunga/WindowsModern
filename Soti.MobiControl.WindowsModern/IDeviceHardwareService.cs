using System;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern;

/// <summary>
/// Windows Device Hardware Service.
/// </summary>
public interface IDeviceHardwareService
{
    /// <summary>
    /// Processes the Windows Device Hardware keys data.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="devId">The device identifier.</param>
    /// <param name="windowsDeviceHardwareKeyData">The Windows Device Hardware key data.</param>
    void SynchronizeWindowsDeviceHardwareDataWithSnapshot(int deviceId, string devId, string windowsDeviceHardwareKeyData);

    /// <summary>
    /// Get All Device Hardware Status Summary By DeviceId.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <returns>List of DeviceHardwareModel</returns>
    IEnumerable<DeviceHardwareSummary> GetAllDeviceHardwareStatusSummaryByDeviceId(int deviceId);

    /// <summary>
    /// Clean up Device Hardware Status by Device Id.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    void CleanUpDeviceHardwareStatus(int deviceId);

    /// <summary>
    /// Updates hardware status based on the device hardware serial number
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <param name="devId">Dev Id.</param>
    /// <param name="hardwareStatus">Hardware Status.</param>
    /// <param name="deviceHardwareSerialNumber">Device Hardware Serial Number.</param>
    void UpdateDeviceHardwareStatus(int deviceId, string devId, HardwareStatus hardwareStatus, string deviceHardwareSerialNumber);

    /// <summary>Clean up records for device hardware asset for RemovedAcknowledged and RemovedRejected HardwareStatus.
    /// and child tables DeviceHardware and DeviceHardwareManufacturer if no records exist DeviceHardwareStatus table.
    /// RemovedAcknowledged - 24 hrs and RemovedRejected - 30 days
    /// </summary>
    /// <returns>Return delete records count.</returns>
    int CleanUpDeviceHardwareAsset();

    /// <summary>
    /// Returns all the device hardware manufacturers matching ids.
    /// </summary>
    /// <param name="deviceHardwareManufacturerIds">Device Hardware manufacturer Ids.</param>
    /// <returns>
    /// An <see cref="IEnumerable{DeviceHardwareManufacturer}"/> containing the list of device hardware manufacturers.
    /// If no manufacturers are found, it returns an empty collection.
    /// </returns>
    /// <remarks>
    /// This method queries the data source for the list of device hardware manufacturers and returns the results.
    /// It may involve database operations or caching mechanisms depending on the implementation.
    /// </remarks>
    [Obsolete("Use it from Soti.MobiControl.Devices(IDeviceHardwareManufacturerService).")]
    IEnumerable<DeviceHardwareManufacturer> GetDeviceHardwareManufacturersByIds(int[] deviceHardwareManufacturerIds);

    /// <summary>
    /// Fetches or creates a new device hardware manufacturer.
    /// </summary>
    /// <param name="manufacturerName">Device Hardware manufacturer Name.</param>
    /// <returns>Returns the hardware manufacturer id</returns>
    [Obsolete("Use it from Soti.MobiControl.Devices(IDeviceHardwareManufacturerService).")]
    int FetchOrCreateDeviceHardwareManufacturer(string manufacturerName);
}
