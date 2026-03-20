using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Soti.Data.SqlClient;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

/// <summary>
/// DeviceGroupSyncRequestDataProvider.
/// </summary>
internal sealed class DeviceGroupSyncRequestDataProvider : IDeviceGroupSyncRequestDataProvider
{
    private const string GetByDeviceGroupIdAndSyncRequestTypeId = "[dbo].[DeviceGroupSyncRequest_GetBy_DeviceGroupIdSyncRequestTypeId]";
    private const string GetLastSyncTimeForDeviceGroups = "[dbo].[DeviceGroupSyncRequest_GetLastSyncTime_ForDeviceGroups]";
    private const string NumericIdentifierTableName = "[dbo].[GEN_UDT_NumericIdentifier]";
    private const string TinyIntIdentifierTableName = "[dbo].[GEN_UDT_TinyIntIdentifier]";
    private const string DeviceGroupIdName = "DeviceGroupId";
    private const string DeviceGroupIdsName = "DeviceGroupIds";
    private const string SyncRequestTypeIdName = "SyncRequestTypeId";
    private const string SyncRequestStatusIdsName = "SyncRequestStatusIds";

    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceGroupSyncRequestDataProvider"/> class.
    /// </summary>
    /// <param name="database">the instance of IDatabase.</param>
    public DeviceGroupSyncRequestDataProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc />
    public DeviceGroupSyncRequestData GetByDeviceGroupIdSyncRequestTypeId(int deviceGroupId, byte syncRequestTypeId)
    {
        if (deviceGroupId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceGroupId));
        }

        if (syncRequestTypeId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(syncRequestTypeId));
        }

        var command = _database.StoredProcedures[GetByDeviceGroupIdAndSyncRequestTypeId];
        command.Parameters.Add(DeviceGroupIdName, deviceGroupId);
        command.Parameters.Add(SyncRequestTypeIdName, syncRequestTypeId);
        return command.ExecuteReader(ReadRecord);
    }

    /// <inheritdoc />
    public DeviceGroupSyncRequestStatusData GetDeviceGroupsLastSyncTime(IReadOnlyList<int> deviceGroupIds, IReadOnlyList<byte> syncRequestStatusIds, byte syncRequestTypeId)
    {
        if (deviceGroupIds == null || !deviceGroupIds.Any())
        {
            throw new ArgumentNullException(nameof(deviceGroupIds));
        }

        if (syncRequestStatusIds == null || !syncRequestStatusIds.Any())
        {
            throw new ArgumentNullException(nameof(syncRequestStatusIds));
        }

        if (syncRequestTypeId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(syncRequestTypeId));
        }

        var command = _database.StoredProcedures[GetLastSyncTimeForDeviceGroups];
        command.Parameters.Add(DeviceGroupIdsName, CreateIdentifierTable(deviceGroupIds, NumericIdentifierTableName), SqlDbType.Structured);
        command.Parameters.Add(SyncRequestStatusIdsName, CreateIdentifierTable(syncRequestStatusIds, TinyIntIdentifierTableName), SqlDbType.Structured);
        command.Parameters.Add(SyncRequestTypeIdName, syncRequestTypeId);
        return command.ExecuteReader(ReadDeviceGroupsLastSyncTime);
    }

    private static DeviceGroupSyncRequestData ReadRecord(IDataReader reader)
    {
        return reader.Read() ? ParseEntity(reader, GetColumnMapping(reader)) : default;
    }

    private static DeviceGroupSyncRequestStatusData ReadDeviceGroupsLastSyncTime(IDataReader reader)
    {
        return reader.Read() ? ParseEntityDeviceGroupsLastSyncTime(reader, GetColumnMappingDeviceGroupsLastSyncTime(reader)) : default;
    }

    private static DeviceGroupSyncRequestData ParseEntity(IDataRecord record, OrdinalColumnMapping mapping)
    {
        return new DeviceGroupSyncRequestData
        {
            DeviceGroupSyncRequestId = record.GetInt32(mapping.DeviceGroupSyncRequestId),
            DeviceGroupId = record.GetInt32(mapping.DeviceGroupId),
            SyncRequestType = record.GetByte(mapping.SyncRequestTypeId),
            SyncRequestStatus = record.GetByte(mapping.SyncRequestStatusId),
            StartedOn = record.GetDateTime(mapping.StartedOn),
            CompletedOn = !record.IsDBNull(mapping.CompletedOn) ? record.GetDateTime(mapping.CompletedOn) : null,
        };
    }

    private static DeviceGroupSyncRequestStatusData ParseEntityDeviceGroupsLastSyncTime(IDataRecord record, OrdinalColumnMappingDeviceGroupsLastSyncTime mapping)
    {
        return new DeviceGroupSyncRequestStatusData
        {
            DeviceGroupId = record.GetInt32(mapping.DeviceGroupId),
            CompletedOn = !record.IsDBNull(mapping.CompletedOn) ? record.GetDateTime(mapping.CompletedOn) : null,
        };
    }

    private static OrdinalColumnMapping GetColumnMapping(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            DeviceGroupSyncRequestId = record.GetOrdinal("DeviceGroupSyncRequestId"),
            DeviceGroupId = record.GetOrdinal("DeviceGroupId"),
            SyncRequestTypeId = record.GetOrdinal("SyncRequestTypeId"),
            SyncRequestStatusId = record.GetOrdinal("SyncRequestStatusId"),
            StartedOn = record.GetOrdinal("StartedOn"),
            CompletedOn = record.GetOrdinal("CompletedOn"),
        };
    }

    private static OrdinalColumnMappingDeviceGroupsLastSyncTime GetColumnMappingDeviceGroupsLastSyncTime(IDataRecord record)
    {
        return new OrdinalColumnMappingDeviceGroupsLastSyncTime
        {
            DeviceGroupId = record.GetOrdinal("DeviceGroupId"),
            CompletedOn = record.GetOrdinal("CompletedOn"),
        };
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

    private sealed class OrdinalColumnMapping
    {
        public int DeviceGroupSyncRequestId;
        public int DeviceGroupId;
        public int SyncRequestTypeId;
        public int SyncRequestStatusId;
        public int StartedOn;
        public int CompletedOn;
    }

    private sealed class OrdinalColumnMappingDeviceGroupsLastSyncTime
    {
        public int DeviceGroupId;
        public int CompletedOn;
    }
}
