using Soti.Data.SqlClient;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Soti.Utilities.Collections;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

/// <summary>
/// Windows device peripheral provider class.
/// </summary>
internal sealed class WindowsDevicePeripheralDataProvider : IWindowsDevicePeripheralDataProvider
{
    private const string DevId = "DeviceId";
    private const string Version = "PeripheralVersion";
    private const string Status = "DevicePeripheralStatusId";
    private const string PeripheralIdentity = "PeripheralId";
    private const string DevicePeripherals = "DevicePeripherales";
    private const string LastKnownConnectTime = "LastKnownConnectTime";
    private const string DeletedRowCount = "DeletedRowCount";
    private const int LastConnectTimeHours = -24;
    private const string DeleteDevicePeripheralByDeviceIdSp = "[dbo].[DevicePeripheral_Delete_ByDeviceId]";
    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDevicePeripheralDataProvider"/> class.
    /// </summary>
    /// <param name="database"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public WindowsDevicePeripheralDataProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDevicePeripheralData> GetDevicePeripheralSummaryByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }
        var command = _database.StoredProcedures["dbo.DevicePeripheral_GetSummaryBy_DeviceId"];

        command.Parameters.Add(DevId, deviceId);

        return command.ExecuteReader(ReadDevicePeripheralCollection);
    }

    /// <inheritdoc/>
    public void DeleteDevicePeripheralData(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures["dbo.GEN_DevicePeripheral_DeleteByFk_DeviceId"];

        command.Parameters.Add(DevId, deviceId);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<int, (byte, byte)> BulkModify(int deviceId, IEnumerable<WindowsDevicePeripheralData> devicePeripherals)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (devicePeripherals == null)
        {
            throw new ArgumentNullException(nameof(devicePeripherals));
        }

        var devicePeripheralOutput = new Dictionary<int, (byte, byte)>();
        var dataTable = CreateDataTable_Bulk_Modify(devicePeripherals);

        var command = _database.StoredProcedures["dbo.DevicePeripheral_BulkModify_ForDevice"];
        command.Parameters.Add(DevicePeripherals, dataTable, SqlDbType.Structured);
        command.Parameters.Add(DevId, deviceId);
        command.ExecuteReader(reader =>
        {
            while (reader.Read())
            {
                // Extract the data from the result set
                var peripheralId = reader.GetInt32(0);
                var oldStatus = reader.IsDBNull(1) ? (byte)0 : reader.GetByte(1);
                var newStatus = reader.GetByte(2);

                devicePeripheralOutput[peripheralId] = (oldStatus, newStatus);
            }
        });
        return devicePeripheralOutput;
    }

    /// <inheritdoc/>
    public int CleanUpObsoleteWindowsPeripheralData(out IReadOnlyList<WindowsDevicePeripheralData> windowsDevicePeripheralData)
    {
        var command = _database.StoredProcedures["dbo.DevicePeripheral_Cleanup"];

        command.Parameters.Add(Status, (int)DevicePeripheralStatus.Disconnected);
        command.Parameters.Add(LastKnownConnectTime, DateTime.UtcNow.AddHours(LastConnectTimeHours));
        command.Parameters.Add(DeletedRowCount, out Ref<int> deletedRecordsCount);

        windowsDevicePeripheralData = command.ExecuteReader(ReadWindowsPeripheralCleanUpData);
        return deletedRecordsCount;
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDevicePeripheralData> DeleteDevicePeripheralByDeviceIdAndGetPeripheralData(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[DeleteDevicePeripheralByDeviceIdSp];
        command.Parameters.Add(DevId, deviceId);

        return command.ExecuteReader(ReadWindowsPeripheralCleanUpData);
    }

    /// <inheritdoc/>
    public void Update(WindowsDevicePeripheralData windowsDevicePeripheralData, DateTime lastKnownConnectTime)
    {
        if (windowsDevicePeripheralData.DeviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowsDevicePeripheralData.DeviceId));
        }

        if (windowsDevicePeripheralData.PeripheralId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowsDevicePeripheralData.PeripheralId));
        }

        if (!Enum.IsDefined(typeof(DevicePeripheralStatus), windowsDevicePeripheralData.Status))
        {
            throw new InvalidEnumArgumentException(nameof(windowsDevicePeripheralData.Status));
        }

        if (lastKnownConnectTime == default)
        {
            throw new ArgumentException(nameof(lastKnownConnectTime));
        }

        var command = _database.StoredProcedures["dbo.GEN_DevicePeripheral_Update"];

        command.Parameters.Add("DeviceId", windowsDevicePeripheralData.DeviceId);
        command.Parameters.Add("PeripheralId", windowsDevicePeripheralData.PeripheralId);
        command.Parameters.Add("DevicePeripheralStatusId", (int)windowsDevicePeripheralData.Status);
        command.Parameters.Add("DevicePeripheralStatusIdHasValue", true);
        command.Parameters.Add("LastKnownConnectTime", lastKnownConnectTime);
        command.Parameters.Add("LastKnownConnectTimeHasValue", true);
        command.ExecuteNonQuery();
    }

    private static IReadOnlyList<WindowsDevicePeripheralData> ReadDevicePeripheralCollection(IDataReader reader)
    {
        var result = new List<WindowsDevicePeripheralData>();
        var mapping = GetDevicePeripheralMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseDevicePeripheralData(reader, mapping));
        }

        return result;
    }

    private static IReadOnlyList<WindowsDevicePeripheralData> ReadWindowsPeripheralCleanUpData(IDataReader reader)
    {
        var result = new List<WindowsDevicePeripheralData>();
        var mapping = GetColumnMappingForDevicePeripherals(reader);
        while (reader.Read())
        {
            result.Add(ParseEntityForDevicePeripherals(reader, mapping));
        }

        return result;
    }

    private static OrdinalColumnMapping GetColumnMappingForDevicePeripherals(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            DeviceId = record.GetOrdinal(DevId),
            PeripheralId = record.GetOrdinal(PeripheralIdentity),
        };
    }

    private static WindowsDevicePeripheralData ParseEntityForDevicePeripherals(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new WindowsDevicePeripheralData
        {
            DeviceId = reader.GetInt32(mapping.DeviceId),
            PeripheralId = reader.GetInt32(mapping.PeripheralId),
        };
    }

    private static DataTable CreateDataTable_Bulk_Modify(IEnumerable<WindowsDevicePeripheralData> windowsDevicePeripheralData)
    {
        var dataTable = new DataTable("dbo.UDTTable_DevicePeripheral_BulkModify_ForDevice");
        dataTable.Columns.AddRange(
        [
            new DataColumn(PeripheralIdentity, typeof(int)) { AllowDBNull = false },
            new DataColumn(Version, typeof(string)) { AllowDBNull = false }
        ]);

        foreach (var peripheralData in windowsDevicePeripheralData)
        {
            if (peripheralData.PeripheralId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(peripheralData.PeripheralId));
            }

            if (peripheralData.Version.IsNullOrEmpty())
            {
                throw new ArgumentException(nameof(peripheralData.Version));
            }

            dataTable.Rows.Add(
                peripheralData.PeripheralId,
                peripheralData.Version);
        }

        return dataTable;
    }

    private static OrdinalColumnMapping GetDevicePeripheralMapping(IDataRecord reader)
    {
        // OrdinalColumnMapping for Windows Device Peripheral Info.
        return new OrdinalColumnMapping
        {
            PeripheralId = reader.GetOrdinal(PeripheralIdentity),
            PeripheralVersion = reader.GetOrdinal(Version),
            DevicePeripheralStatusId = reader.GetOrdinal(Status)
        };
    }

    private static WindowsDevicePeripheralData ParseDevicePeripheralData(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new WindowsDevicePeripheralData
        {
            DeviceId = reader.GetInt32(mapping.DeviceId),
            PeripheralId = reader.GetInt32(mapping.PeripheralId),
            Version = reader.GetString(mapping.PeripheralVersion),
            Status = (DevicePeripheralStatus)reader.GetByte(mapping.DevicePeripheralStatusId),
        };
    }

    private sealed class OrdinalColumnMapping
    {
        public int DeviceId { get; set; }

        public int PeripheralId { get; internal set; }

        public int PeripheralVersion { get; internal set; }

        public int DevicePeripheralStatusId { get; internal set; }
    }
}
