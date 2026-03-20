using Soti.Data.SqlClient;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.TestDbInjector.Converters;
using Soti.MobiControl.WindowsModern.TestDbInjector.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Soti.Utilities.Collections;

namespace Soti.MobiControl.WindowsModern.TestDbInjector;

/// <summary>
/// TestHardwareInjector class
/// </summary>
[ExcludeFromCodeCoverage]
public class TestHardwareInjector
{
    private readonly DeviceHardwareDataProvider _deviceHardwareProvider;

    public TestHardwareInjector(IDatabase database)
    {
        if (database == null)
        {
            throw new ArgumentException(nameof(database));
        }

        _deviceHardwareProvider = new DeviceHardwareDataProvider(database);
    }

    /// <summary>
    /// Insert DeviceHardwareManufacturer table data
    /// </summary>
    /// <param name="hardwareManufacturer">Hardware manufacturer name</param>
    /// <returns>HardwareManufacturerId</returns>
    public int InsertDeviceHardwareManufacturer(string hardwareManufacturer)
    {
        if (hardwareManufacturer.IsNullOrEmpty())
        {
            throw new ArgumentException(nameof(hardwareManufacturer));
        }

        var deviceHardwareManufacturer = new DeviceHardwareManufacturer()
        {
            DeviceHardwareManufacturerName = hardwareManufacturer,
        };

        _deviceHardwareProvider.InsertDeviceHardwareManufacturer(deviceHardwareManufacturer);

        return deviceHardwareManufacturer.DeviceHardwareManufacturerId;
    }

    /// <summary>
    /// Insert data into DeviceHardware table
    /// </summary>
    /// <param name="deviceHardwareName">Hardware name</param>
    /// <param name="deviceHardwareTypeId">Hardware type id</param>
    /// <param name="deviceHardwareManufacturer">DeviceHardwareManufacturer object</param>
    /// <returns>DeviceHardwareId</returns>
    public int InsertDeviceHardware(string deviceHardwareName, int deviceHardwareTypeId, DeviceHardwareManufacturer deviceHardwareManufacturer)
    {
        if (deviceHardwareName.IsNullOrEmpty())
        {
            throw new ArgumentException(nameof(deviceHardwareName));
        }

        if (deviceHardwareTypeId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceHardwareTypeId));
        }

        if (deviceHardwareManufacturer == null)
        {
            throw new ArgumentNullException(nameof(deviceHardwareManufacturer));
        }

        var deviceHardware = new DeviceHardware()
        {
            DeviceHardwareName = deviceHardwareName,
            DeviceHardwareTypeId = deviceHardwareTypeId,
            DeviceHardwareManufacturer = deviceHardwareManufacturer
        };

        _deviceHardwareProvider.InsertDeviceHardware(deviceHardware);

        return deviceHardware.DeviceHardwareId;
    }

    /// <summary>
    /// Bulk modify DeviceHardware
    /// </summary>
    /// <param name="deviceId">DeviceId</param>
    /// <param name="deviceHardwareStatus">List of DeviceHardwareStatusData object</param>
    /// <returns>Readonly list of DeviceHardwareStatusId</returns>
    public IReadOnlyList<int> BulkModifyDeviceHardware(int deviceId, IEnumerable<DeviceHardwareStatusData> deviceHardwareStatus)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var deviceHardwareStatusList = deviceHardwareStatus.AsArray();
        if (deviceHardwareStatusList.Count == 0)
        {
            return new List<int>();
        }

        var deviceHardwareStatusData = deviceHardwareStatusList.Select(DeviceHardwareStatusDataConverter.ToDeviceHardwareStatus);

        var result = _deviceHardwareProvider.BulkModifyDeviceHardwareStatus(deviceId, deviceHardwareStatusData);

        return result.Select(x => x.DeviceHardwareStatusId).AsArray();
    }

    /// <summary>
    /// DeviceHardwareAssetCleanUp for data cleanup
    /// </summary>
    /// <param name="removedAcknowledgedCutOffDate">RemovedAcknowledgedCutOffDate</param>
    /// <param name="removedRejectedCutOffDate">RemovedRejectedCutOffDate</param>
    public void DeviceHardwareAssetCleanUp(DateTime removedAcknowledgedCutOffDate, DateTime removedRejectedCutOffDate)
    {
        if (removedAcknowledgedCutOffDate == default)
        {
            throw new ArgumentException(nameof(removedAcknowledgedCutOffDate));
        }

        if (removedRejectedCutOffDate == default)
        {
            throw new ArgumentException(nameof(removedRejectedCutOffDate));
        }

        _deviceHardwareProvider.DeviceHardwareAssetCleanUp(removedAcknowledgedCutOffDate, removedRejectedCutOffDate, out var deviceHardwareData);
    }

    /// <summary>
    /// Delete DeviceHardwareStatus table data based on DeviceId
    /// </summary>
    /// <param name="deviceId">DeviceId</param>
    public void DeleteDeviceHardwareStatusByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _deviceHardwareProvider.DeleteDeviceHardwareStatusByDeviceId(deviceId);
    }
}
