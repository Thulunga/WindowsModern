using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Soti.Data.SqlClient;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

/// <summary>
/// Provider class for DeviceHardwareProvider.
/// </summary>
internal sealed class DeviceHardwareDataProvider : IDeviceHardwareProvider
{
    private const string GetAllDeviceHardwareManufacturerSp = "[dbo].[GEN_DeviceHardwareManufacturer_GetAll]";
    private const string GetAllDeviceHardwareSp = "[dbo].[GEN_DeviceHardware_GetAll]";
    private const string InsertDeviceHardwareManufacturerSp = "[dbo].[DeviceHardwareManufacturer_Insert]";
    private const string InsertDeviceHardwareSp = "[dbo].[DeviceHardware_Insert]";
    private const string BulkModifyDeviceHardwareStatusSp = "[dbo].[DeviceHardwareStatus_BulkModify]";
    private const string GetDeviceHardwareStatusSummaryByDeviceIdSp = "[dbo].[DeviceHardwareStatus_GetSummary_ByDeviceId]";
    private const string DeviceHardwareAssetCleanupSp = "[dbo].[DeviceHardwareAsset_Cleanup]";
    private const string DeviceHardwareStatusUpdateHardwareStatusIdByDeviceHardwareSerialNumber = "[dbo].[DeviceHardwareStatus_UpdateHardwareStatusId_ByDeviceHardwareSerialNumber]";
    private const string DeleteDeviceHardwareStatusByDeviceIdSp = "[dbo].[DeviceHardwareStatus_Delete_ByDeviceId]";

    private const string DevId = "DeviceId";
    private const string HardwareId = "DeviceHardwareId";
    private const string StatusId = "DeviceHardwareStatusId";
    private const string HardwareName = "DeviceHardwareName";
    private const string HardwareTypeId = "DeviceHardwareTypeId";
    private const string HardwareManufacturerId = "DeviceHardwareManufacturerId";
    private const string HardwareManufacturerName = "DeviceHardwareManufacturerName";
    private const string HardwareSerialNumber = "DeviceHardwareSerialNumber";
    private const string DeviceHardwareSerialNumbers = "DeviceHardwareSerialNumbers";
    private const string HardwareStatusIdentity = "HardwareStatusId";
    private const string ModifiedDate = "LastModifiedDate";
    private const string RemovedAcknowledgedCutOffDate = "RemovedAcknowledgedCutOffDate";
    private const string RemovedRejectedCutOffDate = "RemovedRejectedCutOffDate";
    private const string DeletedRecordsCount = "DeletedRecordsCount";

    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the DeviceHardwareProvider class.
    /// </summary>
    /// <param name="database">the instance of IDatabase.</param>
    public DeviceHardwareDataProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc />
    public IReadOnlyList<DeviceHardwareData> BulkModifyDeviceHardwareStatus(int deviceId, IEnumerable<DeviceHardwareStatus> deviceHardwareStatus)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (deviceHardwareStatus == null)
        {
            throw new ArgumentNullException(nameof(deviceHardwareStatus));
        }

        var dataTable = CreateDeviceHardwareStatusDataTableForBulkModify(deviceHardwareStatus);
        var command = _database.StoredProcedures[BulkModifyDeviceHardwareStatusSp];
        command.Parameters.Add(DevId, deviceId);
        command.Parameters.Add(DeviceHardwareSerialNumbers, dataTable, SqlDbType.Structured);
        return command.ExecuteReader(ReadDeviceHardwareStatusBulkModify);
    }

    /// <inheritdoc />
    public void InsertDeviceHardwareManufacturer(DeviceHardwareManufacturer deviceHardwareManufacturer)
    {
        if (deviceHardwareManufacturer == null)
        {
            throw new ArgumentNullException(nameof(deviceHardwareManufacturer));
        }

        var command = _database.StoredProcedures[InsertDeviceHardwareManufacturerSp];
        command.Parameters.Add(HardwareManufacturerName, deviceHardwareManufacturer.DeviceHardwareManufacturerName);
        command.Parameters.Add(HardwareManufacturerId, out Ref<int> deviceManufacturerId);
        command.ExecuteNonQuery();
        deviceHardwareManufacturer.DeviceHardwareManufacturerId = deviceManufacturerId;
    }

    /// <inheritdoc />
    public IReadOnlyList<DeviceHardwareManufacturer> GetAllDeviceHardwareManufacturer()
    {
        var command = _database.StoredProcedures[GetAllDeviceHardwareManufacturerSp];

        return command.ExecuteReader(ReadDeviceHardwareManufacturerCollection);
    }

    /// <inheritdoc />
    public void InsertDeviceHardware(DeviceHardware deviceHardware)
    {
        if (deviceHardware == null)
        {
            throw new ArgumentNullException(nameof(deviceHardware));
        }

        var command = _database.StoredProcedures[InsertDeviceHardwareSp];

        command.Parameters.Add(HardwareName, deviceHardware.DeviceHardwareName);
        command.Parameters.Add(HardwareTypeId, deviceHardware.DeviceHardwareTypeId);
        command.Parameters.Add(HardwareManufacturerId, deviceHardware.DeviceHardwareManufacturer.DeviceHardwareManufacturerId);
        command.Parameters.Add(HardwareId, out Ref<int> deviceHardwareId);
        command.ExecuteNonQuery();
        deviceHardware.DeviceHardwareId = deviceHardwareId;
    }

    /// <inheritdoc />
    public IReadOnlyList<DeviceHardware> GetAllDeviceHardware()
    {
        var command = _database.StoredProcedures[GetAllDeviceHardwareSp];

        return command.ExecuteReader(ReadDeviceHardwareCollection);
    }

    /// <inheritdoc />
    public IReadOnlyList<DeviceHardwareData> GetAllDeviceHardwareStatusSummaryByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[GetDeviceHardwareStatusSummaryByDeviceIdSp];
        command.Parameters.Add(DevId, deviceId);

        return command.ExecuteReader(ReadDeviceHardwareStatusSummaryCollection);
    }

    /// <inheritdoc />
    public int DeviceHardwareAssetCleanUp(DateTime removedAcknowledgedCutOffDate, DateTime removedRejectedCutOffDate, out IReadOnlyList<DeviceHardwareData> deviceHardwareData)
    {
        var command = _database.StoredProcedures[DeviceHardwareAssetCleanupSp];

        command.Parameters.Add(RemovedAcknowledgedCutOffDate, removedAcknowledgedCutOffDate);
        command.Parameters.Add(RemovedRejectedCutOffDate, removedRejectedCutOffDate);
        command.Parameters.Add(DeletedRecordsCount, out Ref<int> removedRecords);
        deviceHardwareData = command.ExecuteReader(ReadDeviceHardwareStatusForDeletionAndUpdate);
        return removedRecords;
    }

    /// <inheritdoc />
    public IReadOnlyList<DeviceHardwareData> UpdateDeviceHardwareStatusIdByDeviceHardwareSerialNumber(HardwareStatus hardwareStatusId, string deviceHardwareSerialNumber)
    {
        if (!Enum.IsDefined(typeof(HardwareStatus), hardwareStatusId))
        {
            throw new ArgumentOutOfRangeException($"{nameof(hardwareStatusId)} hardware status id not defined");
        }

        if (string.IsNullOrWhiteSpace(deviceHardwareSerialNumber))
        {
            throw new ArgumentException(nameof(deviceHardwareSerialNumber));
        }

        var command = _database.StoredProcedures[DeviceHardwareStatusUpdateHardwareStatusIdByDeviceHardwareSerialNumber];

        command.Parameters.Add(HardwareStatusIdentity, (int)hardwareStatusId);
        command.Parameters.Add(HardwareSerialNumber, deviceHardwareSerialNumber);
        return command.ExecuteReader(ReadDeviceHardwareStatusForDeletionAndUpdate);
    }

    /// <inheritdoc />
    public IReadOnlyList<DeviceHardwareData> DeleteDeviceHardwareStatusByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[DeleteDeviceHardwareStatusByDeviceIdSp];
        command.Parameters.Add(DevId, deviceId);

        return command.ExecuteReader(ReadDeviceHardwareStatusForDeletionAndUpdate);
    }

    /// <inheritdoc />
    public DeviceHardwareData GetDeviceHardwareStatusByDeviceHardwareSerialNumber(string deviceHardwareSerialNumber)
    {
        if (string.IsNullOrWhiteSpace(deviceHardwareSerialNumber))
        {
            throw new ArgumentException(nameof(deviceHardwareSerialNumber));
        }

        var command = _database.StoredProcedures["[dbo].[DeviceHardwareStatus_GetSummary_ByDeviceHardwareSerialNumber]"];
        command.Parameters.Add(HardwareSerialNumber, deviceHardwareSerialNumber);

        return command.ExecuteReader(ReadDeviceHardwareStatusSummary);
    }

    private static DeviceHardwareData ReadDeviceHardwareStatusSummary(IDataReader reader)
    {
        var result = new DeviceHardwareData();
        var mapping = GetColumnMappingForDeviceHardwareStatus(reader);

        while (reader.Read())
        {
            result = ParseEntityForDeviceHardwareStatus(reader, mapping);
        }
        return result;
    }

    private static IReadOnlyList<DeviceHardwareData> ReadDeviceHardwareStatusBulkModify(IDataReader reader)
    {
        var result = new List<DeviceHardwareData>();
        var mapping = GetColumnMappingForDeviceHardwareStatusBulkModify(reader);

        while (reader.Read())
        {
            result.Add(ParseEntityForDeviceHardwareStatusBulkModify(reader, mapping));
        }

        return result;
    }

    private static IReadOnlyList<DeviceHardwareData> ReadDeviceHardwareStatusSummaryCollection(IDataReader reader)
    {
        var result = new List<DeviceHardwareData>();
        var mapping = GetColumnMappingForDeviceHardwareStatusSummary(reader);

        while (reader.Read())
        {
            result.Add(ParseEntityForDeviceHardwareStatus(reader, mapping));
        }

        return result;
    }

    private static IReadOnlyList<DeviceHardwareData> ReadDeviceHardwareStatusForDeletionAndUpdate(IDataReader reader)
    {
        var result = new List<DeviceHardwareData>();
        var mapping = GetColumnMappingForDeviceHardwareStatusDeletionAndUpdate(reader);

        while (reader.Read())
        {
            result.Add(ParseEntityForDeviceHardwareStatusDeletionAndUpdate(reader, mapping));
        }

        return result;
    }

    private static IReadOnlyList<DeviceHardwareManufacturer> ReadDeviceHardwareManufacturerCollection(IDataReader reader)
    {
        var result = new List<DeviceHardwareManufacturer>();
        var mapping = GetColumnMappingForDeviceHardwareManufacturer(reader);
        while (reader.Read())
        {
            result.Add(ParseEntityForDeviceHardwareManufacturer(reader, mapping));
        }

        return result;
    }

    private static IReadOnlyList<DeviceHardware> ReadDeviceHardwareCollection(IDataReader reader)
    {
        var result = new List<DeviceHardware>();
        var mapping = GetColumnMappingForDeviceHardware(reader);
        while (reader.Read())
        {
            result.Add(ParseEntityForDeviceHardware(reader, mapping));
        }

        return result;
    }

    private static OrdinalColumnMapping GetColumnMappingForDeviceHardwareManufacturer(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            DeviceHardwareManufacturerId = record.GetOrdinal(HardwareManufacturerId),
            DeviceHardwareManufacturerName = record.GetOrdinal(HardwareManufacturerName),
        };
    }

    private static OrdinalColumnMapping GetColumnMappingForDeviceHardware(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            DeviceHardwareId = record.GetOrdinal(HardwareId),
            DeviceHardwareName = record.GetOrdinal(HardwareName),
            DeviceHardwareTypeId = record.GetOrdinal(HardwareTypeId),
            DeviceHardwareManufacturerId = record.GetOrdinal(HardwareManufacturerId)
        };
    }

    private static OrdinalColumnMapping GetColumnMappingForDeviceHardwareStatusSummary(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            DeviceHardwareManufacturerName = record.GetOrdinal(HardwareManufacturerName),
            DeviceHardwareName = record.GetOrdinal(HardwareName),
            DeviceHardwareTypeId = record.GetOrdinal(HardwareTypeId),
            DeviceId = record.GetOrdinal(DevId),
            HardwareStatusId = record.GetOrdinal(HardwareStatusIdentity),
            DeviceHardwareSerialNumber = record.GetOrdinal(HardwareSerialNumber),
            LastModifiedDate = record.GetOrdinal(ModifiedDate),
        };
    }

    private static OrdinalColumnMapping GetColumnMappingForDeviceHardwareStatusBulkModify(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            DeviceHardwareManufacturerName = record.GetOrdinal(HardwareManufacturerName),
            DeviceHardwareName = record.GetOrdinal(HardwareName),
            DeviceHardwareTypeId = record.GetOrdinal(HardwareTypeId),
            DeviceId = record.GetOrdinal(DevId),
            HardwareStatusId = record.GetOrdinal(HardwareStatusIdentity),
            DeviceHardwareSerialNumber = record.GetOrdinal(HardwareSerialNumber),
            LastModifiedDate = record.GetOrdinal(ModifiedDate),
            DeviceHardwareStatusId = record.GetOrdinal(StatusId),
            DeviceHardwareId = record.GetOrdinal(HardwareId)
        };
    }

    private static OrdinalColumnMapping GetColumnMappingForDeviceHardwareStatusDeletionAndUpdate(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            DeviceHardwareStatusId = record.GetOrdinal("DeviceHardwareStatusId"),
            DeviceHardwareId = record.GetOrdinal("DeviceHardwareId"),
        };
    }

    private static OrdinalColumnMapping GetColumnMappingForDeviceHardwareStatus(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            DeviceHardwareManufacturerName = record.GetOrdinal(HardwareManufacturerName),
            DeviceHardwareName = record.GetOrdinal(HardwareName),
            DeviceHardwareTypeId = record.GetOrdinal(HardwareTypeId),
            DeviceId = record.GetOrdinal(DevId),
            HardwareStatusId = record.GetOrdinal(HardwareStatusIdentity),
            DeviceHardwareSerialNumber = record.GetOrdinal(HardwareSerialNumber),
            LastModifiedDate = record.GetOrdinal(ModifiedDate),
        };
    }

    private static DeviceHardwareData ParseEntityForDeviceHardwareStatus(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new DeviceHardwareData
        {
            DeviceHardwareManufacturerName = reader.GetString(mapping.DeviceHardwareManufacturerName),
            DeviceHardwareName = reader.GetString(mapping.DeviceHardwareName),
            DeviceHardwareTypeId = reader.GetByte(mapping.DeviceHardwareTypeId),
            HardwareStatusId = reader.GetByte(mapping.HardwareStatusId),
            DeviceHardwareSerialNumber = reader.GetString(mapping.DeviceHardwareSerialNumber),
            LastModifiedDate = reader.GetDateTime(mapping.LastModifiedDate)
        };
    }

    private static DeviceHardwareData ParseEntityForDeviceHardwareStatusBulkModify(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new DeviceHardwareData
        {
            DeviceHardwareManufacturerName = reader.GetString(mapping.DeviceHardwareManufacturerName),
            DeviceHardwareName = reader.GetString(mapping.DeviceHardwareName),
            DeviceHardwareTypeId = reader.GetByte(mapping.DeviceHardwareTypeId),
            HardwareStatusId = reader.GetByte(mapping.HardwareStatusId),
            DeviceHardwareSerialNumber = reader.GetString(mapping.DeviceHardwareSerialNumber),
            LastModifiedDate = reader.GetDateTime(mapping.LastModifiedDate),
            DeviceHardwareStatusId = reader.GetInt32(mapping.DeviceHardwareStatusId),
            DeviceHardwareId = reader.GetInt32(mapping.DeviceHardwareId)
        };
    }

    private static DeviceHardwareData ParseEntityForDeviceHardwareStatusDeletionAndUpdate(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new DeviceHardwareData
        {
            DeviceHardwareStatusId = reader.GetInt32(mapping.DeviceHardwareStatusId),
            DeviceHardwareId = reader.GetInt32(mapping.DeviceHardwareId)
        };
    }

    private static DeviceHardwareManufacturer ParseEntityForDeviceHardwareManufacturer(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new DeviceHardwareManufacturer
        {
            DeviceHardwareManufacturerId = reader.GetInt16(mapping.DeviceHardwareManufacturerId),
            DeviceHardwareManufacturerName = reader.GetString(mapping.DeviceHardwareManufacturerName)
        };
    }

    private static DeviceHardware ParseEntityForDeviceHardware(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new DeviceHardware
        {
            DeviceHardwareId = reader.GetInt32(mapping.DeviceHardwareId),
            DeviceHardwareName = reader.GetString(mapping.DeviceHardwareName),
            DeviceHardwareTypeId = reader.GetByte(mapping.DeviceHardwareTypeId),
            DeviceHardwareManufacturer = new DeviceHardwareManufacturer
            {
                DeviceHardwareManufacturerId = reader.GetInt16(mapping.DeviceHardwareManufacturerId)
            }
        };
    }

    private static DataTable CreateDeviceHardwareStatusDataTableForBulkModify(IEnumerable<DeviceHardwareStatus> deviceHardwareStatus)
    {
        var dataTable = new DataTable
        {
            Locale = CultureInfo.InvariantCulture
        };
        dataTable.Columns.AddRange(
            [
                new DataColumn(HardwareId, typeof(int)) { AllowDBNull = false },
                new DataColumn(HardwareSerialNumber, typeof(string)) { AllowDBNull = false }
            ]
        );

        foreach (var hardwareStatus in deviceHardwareStatus)
        {
            if (hardwareStatus.DeviceHardwareId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hardwareStatus.DeviceHardwareId));
            }

            if (hardwareStatus.DeviceHardwareSerialNumber == null)
            {
                throw new ArgumentNullException(nameof(hardwareStatus.DeviceHardwareSerialNumber));
            }

            dataTable.Rows.Add(
                hardwareStatus.DeviceHardwareId,
                hardwareStatus.DeviceHardwareSerialNumber
                );
        }

        return dataTable;
    }

    private sealed class OrdinalColumnMapping
    {
        public int DeviceHardwareStatusId { get; internal set; }

        public int DeviceId { get; internal set; }

        public int DeviceHardwareId { get; internal set; }

        public int DeviceHardwareName { get; internal set; }

        public int DeviceHardwareTypeId { get; internal set; }

        public int DeviceHardwareManufacturerId { get; internal set; }

        public int DeviceHardwareManufacturerName { get; internal set; }

        public int HardwareStatusId { get; internal set; }

        public int DeviceHardwareSerialNumber { get; internal set; }

        public int LastModifiedDate { get; internal set; }
    }
}
