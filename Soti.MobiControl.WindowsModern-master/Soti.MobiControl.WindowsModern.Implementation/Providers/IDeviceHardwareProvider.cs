using System;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// IDeviceHardwareProvider
/// </summary>
internal interface IDeviceHardwareProvider
{
    /// <summary>
    /// Bulk Modify Device Hardware Status.
    /// </summary>
    /// <param name="deviceId">DeviceId</param>
    /// <param name="deviceHardwareStatus">List of deviceHardwareStatus.</param>
    IReadOnlyList<DeviceHardwareData> BulkModifyDeviceHardwareStatus(int deviceId, IEnumerable<DeviceHardwareStatus> deviceHardwareStatus);

    /// <summary>
    /// Insert Device Hardware Manufacturer.
    /// </summary>
    /// <param name="deviceHardwareManufacturer">DeviceHardwareManufacturer.</param>
    void InsertDeviceHardwareManufacturer(DeviceHardwareManufacturer deviceHardwareManufacturer);

    /// <summary>
    /// Get All Device Hardware Manufacturer.
    /// </summary>
    IReadOnlyList<DeviceHardwareManufacturer> GetAllDeviceHardwareManufacturer();

    /// <summary>
    /// Insert Device Hardware.
    /// </summary>
    void InsertDeviceHardware(DeviceHardware deviceHardware);

    /// <summary>
    /// Get All Device Hardware.
    /// </summary>
    IReadOnlyList<DeviceHardware> GetAllDeviceHardware();

    /// <summary>
    /// Get All DeviceHardware Status Summary By DeviceId.
    /// </summary>
    IReadOnlyList<DeviceHardwareData> GetAllDeviceHardwareStatusSummaryByDeviceId(int deviceId);

    /// <summary>
    /// Clean Up Device Hardware Asset which are in Removed Acknowledged, Remove Rejected states.
    /// </summary>
    int DeviceHardwareAssetCleanUp(DateTime removedAcknowledgedCutOffDate, DateTime removedRejectedCutOffDate, out IReadOnlyList<DeviceHardwareData> deviceHardwareData);

    /// <summary>
    /// Delete Device Hardware Status by DeviceId.
    /// </summary>
    IReadOnlyList<DeviceHardwareData> DeleteDeviceHardwareStatusByDeviceId(int deviceId);

    /// <summary>
    /// Update Device Hardware Status id by Device Hardware SerialNumber.
    /// </summary>
    IReadOnlyList<DeviceHardwareData> UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus hardwareStatusId, string deviceHardwareSerialNumber);

    /// <summary>
    /// Get Device Hardware Status By Device Hardware Serial Number.
    /// </summary>
    DeviceHardwareData GetDeviceHardwareStatusByDeviceHardwareSerialNumber(string deviceHardwareSerialNumber);
}
