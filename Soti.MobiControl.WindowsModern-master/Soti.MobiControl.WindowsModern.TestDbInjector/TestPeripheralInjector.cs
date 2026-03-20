using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Soti.Data.SqlClient;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.MobiControl.WindowsModern.TestDbInjector.Converters;
using Soti.MobiControl.WindowsModern.TestDbInjector.Models;
using Soti.Utilities.Collections;

namespace Soti.MobiControl.WindowsModern.TestDbInjector
{
    /// <summary>
    /// TestPeripheralInjector class
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TestPeripheralInjector
    {
        private readonly PeripheralDataProvider _peripheralDataProvider;
        private readonly PeripheralManufacturerProvider _peripheralManufacturerProvider;
        private readonly WindowsDevicePeripheralDataProvider _windowsDevicePeripheralDataProvider;
        public TestPeripheralInjector(IDatabase database)
        {
            if (database == null)
            {
                throw new ArgumentException(nameof(database));
            }

            _peripheralDataProvider = new PeripheralDataProvider(database);
            _peripheralManufacturerProvider = new PeripheralManufacturerProvider(database);
            _windowsDevicePeripheralDataProvider = new WindowsDevicePeripheralDataProvider(database);
        }

        /// <summary>
        /// Inset Peripheral manufacturer data in manufacturer table
        /// </summary>
        /// <param name="manufacturerCode">ManufacturerCode</param>
        /// <param name="manufacturerName">ManufacturerName</param>
        /// <returns>ManufacturerId</returns>
        public int InsertPeripheralManufacturer(string manufacturerCode, string manufacturerName)
        {
            if (manufacturerCode.IsNullOrEmpty())
            {
                throw new ArgumentException(nameof(manufacturerCode));
            }

            if (manufacturerName.IsNullOrEmpty())
            {
                throw new ArgumentException(nameof(manufacturerName));
            }

            var peripheralManufacturerData = new PeripheralManufacturerData()
            {
                ManufacturerName = manufacturerName,
                ManufacturerCode = manufacturerCode
            };

            _peripheralManufacturerProvider.InsertPeripheralManufacturerData(peripheralManufacturerData);

            return peripheralManufacturerData.ManufacturerId;
        }

        /// <summary>
        /// Insert data in Peripheral table
        /// </summary>
        /// <param name="peripheralName">PeripheralName</param>
        /// <param name="peripheralManufacturerId">PeripheralManufacturerId</param>
        /// <param name="peripheralTypeId">The peripheral type id.</param>
        /// <returns>PeripheralId</returns>
        public int InsertPeripheralData(string peripheralName, short peripheralManufacturerId, short peripheralTypeId)
        {
            if (peripheralName.IsNullOrEmpty())
            {
                throw new ArgumentException(nameof(peripheralName));
            }

            if (peripheralManufacturerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(peripheralManufacturerId));
            }

            var peripheralData = new PeripheralData()
            {
                Name = peripheralName,
                ManufacturerId = peripheralManufacturerId,
                PeripheralTypeId = peripheralTypeId
            };

            return _peripheralDataProvider.InsertPeripheralData(peripheralData);
        }

        /// <summary>
        /// Bulk modify device peripherals
        /// </summary>
        /// <param name="deviceId">DeviceId</param>
        /// <param name="devicePeripherals">DevicePeripheralsData type object</param>
        public void BulkModifyPeripherals(int deviceId, IEnumerable<DevicePeripheralsData> devicePeripherals)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var devicePeripheralsList = devicePeripherals.AsArray();
            if (devicePeripheralsList.Count == 0)
            {
                return;
            }

            var peripheralData = devicePeripheralsList.Select(DevicePeripheralDataConverter.ToWindowsDevicePeripheralData);
            _windowsDevicePeripheralDataProvider.BulkModify(deviceId, peripheralData);
        }

        /// <summary>
        /// Bulk delete PeripheralData
        /// </summary>
        /// <param name="peripheralId">PeripheralId</param>
        public void BulkDeletePeripheralData(int peripheralId)
        {
            if (peripheralId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(peripheralId));
            }

            _peripheralDataProvider.BulkDeletePeripheralData(peripheralId);
        }

        /// <summary>
        /// Delete Peripheral Data based on DeviceId
        /// </summary>
        /// <param name="deviceId">DeviceId</param>
        public void DeleteDevicePeripheralData(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            _windowsDevicePeripheralDataProvider.DeleteDevicePeripheralData(deviceId);
        }
    }
}
