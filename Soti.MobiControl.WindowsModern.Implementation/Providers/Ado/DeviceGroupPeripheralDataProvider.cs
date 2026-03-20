using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Soti.Data.SqlClient;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

/// <summary>
/// Data provider class of device group peripheral.
/// </summary>
internal sealed class DeviceGroupPeripheralDataProvider : IDeviceGroupPeripheralDataProvider
{
    private const string NumericIdentifierTableName = "[dbo].[GEN_UDT_NumericIdentifier]";
    private const string DeviceGroupIdsName = "DeviceGroupIds";
    private const string Name = "PeripheralName";
    private const string DevId = "DeviceId";
    private const string ManufacturerName = "PeripheralManufacturerName";
    private const string TypeId = "PeripheralTypeId";
    private const string PeripheralStatusId = "DevicePeripheralStatusId";
    private const string Version = "PeripheralVersion";
    private const string PerId = "PeripheralId";

    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceGroupPeripheralDataProvider"/> class.
    /// </summary>
    /// <param name="database"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public DeviceGroupPeripheralDataProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc cref = "IDeviceGroupPeripheralDataProvider" />
    public IReadOnlyList<DeviceGroupPeripheralSummary> GetDeviceGroupPeripherals(IEnumerable<int> deviceGroupIds)
    {
        ArgumentNullException.ThrowIfNull(deviceGroupIds);

        var enumerable = deviceGroupIds.ToList();
        enumerable.ForEach(id => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(deviceGroupIds)));

        var command = _database.StoredProcedures["dbo.DevicePeripheral_GetSummary_ByDeviceGroupIds"];
        command.Parameters.Add(DeviceGroupIdsName, CreateIdentifierTable(enumerable, NumericIdentifierTableName), SqlDbType.Structured);
        return command.ExecuteReader(ReadDeviceGroupsPeripheral);
    }

    /// <inheritdoc cref = "IDeviceGroupPeripheralDataProvider" />
    public IReadOnlyList<DeviceGroupPeripheralSummary> GetPeripheralSummaryByFamilyIdAndGroupIds(int deviceFamilyId, IEnumerable<int> deviceGroupIds)
    {
        ArgumentNullException.ThrowIfNull(deviceGroupIds);

        var enumerable = deviceGroupIds.ToList();
        enumerable.ForEach(id => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(deviceGroupIds)));

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceFamilyId);

        var command = _database.StoredProcedures["dbo.DevicePeripheral_GetSummary_ByDeviceFamilyIdAndDeviceGroupIds"];
        command.Parameters.Add(DeviceGroupIdsName, CreateIdentifierTable(enumerable, NumericIdentifierTableName), SqlDbType.Structured);
        command.Parameters.Add("DeviceFamilyId", deviceFamilyId);
        return command.ExecuteReader(ReadDeviceGroupsPeripheral);
    }

    private static DataTable CreateIdentifierTable<T>(IEnumerable<T> values, string tableName)
    {
        var table = new DataTable(tableName) { Locale = CultureInfo.InvariantCulture };
        table.Columns.Add("Id", typeof(T));

        foreach (var value in values)
        {
            table.Rows.Add(value);
        }

        return table;
    }

    private static IReadOnlyList<DeviceGroupPeripheralSummary> ReadDeviceGroupsPeripheral(IDataReader dataReader)
    {
        var result = new List<DeviceGroupPeripheralSummary>();
        var mapping = GetColumnMappingForDeviceGroupPeripheral(dataReader);

        while (dataReader.Read())
        {
            result.Add(ParseEntityForDeviceGroupPeripheral(dataReader, mapping));
        }
        return result;
    }

    private static OrdinalColumnMapping GetColumnMappingForDeviceGroupPeripheral(IDataRecord reader)
    {
        return new OrdinalColumnMapping
        {
            DeviceId = reader.GetOrdinal(DevId),
            PeripheralName = reader.GetOrdinal(Name),
            PeripheralManufacturerName = reader.GetOrdinal(ManufacturerName),
            PeripheralTypeId = reader.GetOrdinal(TypeId),
            PeripheralVersion = reader.GetOrdinal(Version),
            DevicePeripheralStatusId = reader.GetOrdinal(PeripheralStatusId),
            PeripheralId = reader.GetOrdinal(PerId)
        };
    }

    private static DeviceGroupPeripheralSummary ParseEntityForDeviceGroupPeripheral(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new DeviceGroupPeripheralSummary
        {
            DeviceId = reader.GetInt32(mapping.DeviceId),
            Name = reader.GetString(mapping.PeripheralName),
            Manufacturer = reader.GetString(mapping.PeripheralManufacturerName),
            Version = reader.GetString(mapping.PeripheralVersion),
            PeripheralType = reader.GetByte(mapping.PeripheralTypeId),
            Status = (DevicePeripheralStatus)reader.GetByte(mapping.DevicePeripheralStatusId),
            PeripheralId = reader.GetInt32(mapping.PeripheralId)
        };
    }

    private sealed class OrdinalColumnMapping
    {
        public int DeviceId { get; internal set; }

        public int PeripheralName { get; internal set; }

        public int PeripheralManufacturerName { get; internal set; }

        public int PeripheralVersion { get; internal set; }

        public int DevicePeripheralStatusId { get; internal set; }

        public int PeripheralTypeId { get; internal set; }

        public int PeripheralId { get; internal set; }
    }
}
