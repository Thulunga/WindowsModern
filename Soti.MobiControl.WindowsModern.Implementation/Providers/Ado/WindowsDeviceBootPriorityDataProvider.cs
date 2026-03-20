using System;
using System.Collections.Generic;
using System.Data;
using Soti.Data.SqlClient;
using System.Linq;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

/// <inheritdoc/>
internal sealed class WindowsDeviceBootPriorityDataProvider : IWindowsDeviceBootPriorityDataProvider
{
    private const string BootOrder = "BootOrder";
    private const string BootPriorityId = "BootPriorityId";
    private const string BootPriorityName = "BootPriorityName";
    private const string DeviceId = "DeviceId";
    private const string Filter = "Filter";
    private const string Id = "Id";
    private const string WindowsDeviceBootPriorities = "WindowsDeviceBootPriorities";
    private const string GenUdtNumericIdentifier = "GEN_UDT_NumericIdentifier";
    private const string GenUdtVarcharIdentifier = "GEN_UDT_VarcharIdentifier";
    private const string GenUdtSmallIntIdentifier = "GEN_UDT_SmallIntIdentifier";
    private const string UdtTableWindowsDeviceBootPriority = "UDTTable_WindowsDeviceBootPriority";
    private const string UdtTableWindowsDeviceBootPriorityForDevice = "UDTTable_WindowsDeviceBootPriority_ForDevice";

    private readonly IDatabase _database;

    /// <summary>
    /// WindowsDeviceBootPriorityDataProvider
    /// </summary>
    /// <param name="database"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public WindowsDeviceBootPriorityDataProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc/>
    public void DeleteByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures["dbo.GEN_WindowsDeviceBootPriority_DeleteByFk_DeviceId"];
        command.Parameters.Add(DeviceId, deviceId);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceBootPriority> GetByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures["dbo.GEN_WindowsDeviceBootPriority_GetByFK_DeviceId"];
        command.Parameters.Add(DeviceId, deviceId);

        return command.ExecuteReader(ReadDeviceBootPriorityCollection);
    }

    /// <inheritdoc/>
    public void BulkModify(IEnumerable<WindowsDeviceBootPriority> data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (!data.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(data));
        }
        var table = CreateBootPriorityDataTable(data, includeDeviceId: true);
        var command = _database.StoredProcedures["dbo.WindowsDeviceBootPriority_BulkModify"];
        command.Parameters.Add(WindowsDeviceBootPriorities, table, SqlDbType.Structured);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public void BulkModifyForDevice(IEnumerable<WindowsDeviceBootPriority> data, int deviceId)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (!data.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(data));
        }

        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var table = CreateBootPriorityDataTable(data, includeDeviceId: false);
        var command = _database.StoredProcedures["dbo.WindowsDeviceBootPriority_BulkModify_ForDevice"];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(WindowsDeviceBootPriorities, table, SqlDbType.Structured);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceBootPriority> BulkGetByDeviceId(IEnumerable<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        deviceIds = deviceIds.ToList();
        if (!deviceIds.Any())
        {
            throw new ArgumentException($"{nameof(deviceIds)} cannot be empty");
        }

        var table = new DataTable(GenUdtNumericIdentifier);
        table.Columns.Add(Id, typeof(int));

        foreach (var id in deviceIds)
        {
            table.Rows.Add(id);
        }

        var command = _database.StoredProcedures["dbo.GEN_WindowsDeviceBootPriority_BulkGetByFK_DeviceId"];
        command.Parameters.Add(Filter, table, SqlDbType.Structured);

        return command.ExecuteReader(ReadDeviceBootPriorityCollection);
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsBootPriority> BulkInsertAndGet(IEnumerable<string> priorityNames)
    {
        var priorityNameList = priorityNames?.ToList();

        if (priorityNameList == null || priorityNameList.Count == 0)
        {
            throw new ArgumentException($"{nameof(priorityNames)} cannot be null or empty.");
        }

        var table = new DataTable(GenUdtVarcharIdentifier);
        table.Columns.Add(Id, typeof(string));
        foreach (var name in priorityNameList)
        {
            table.Rows.Add(name);
        }

        var command = _database.StoredProcedures["dbo.WindowsBootPriority_BulkInsertAndGet"];
        command.Parameters.Add("BootPriorityNames", table, SqlDbType.Structured);

        return command.ExecuteReader(ReadBootPriorityCollection);
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsBootPriority> BulkGetByIds(IEnumerable<short> priorityIds)
    {
        var priorityIdList = priorityIds?.ToList();
        if (priorityIdList == null || priorityIdList.Count == 0)
        {
            throw new ArgumentException($"{nameof(priorityIds)} cannot be null or empty.");
        }

        var table = new DataTable(GenUdtSmallIntIdentifier);
        table.Columns.Add(Id, typeof(short));
        foreach (var id in priorityIdList)
        {
            table.Rows.Add(id);
        }

        var command = _database.StoredProcedures["dbo.GEN_WindowsBootPriority_BulkGet"];
        command.Parameters.Add(Filter, table, SqlDbType.Structured);

        return command.ExecuteReader(ReadBootPriorityCollection);
    }

    private static DataTable CreateBootPriorityDataTable(IEnumerable<WindowsDeviceBootPriority> data, bool includeDeviceId)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var table = new DataTable(includeDeviceId
            ? UdtTableWindowsDeviceBootPriority
            : UdtTableWindowsDeviceBootPriorityForDevice);

        if (includeDeviceId)
        {
            table.Columns.Add(DeviceId, typeof(int));
        }

        table.Columns.Add(BootPriorityId, typeof(short));
        table.Columns.Add(BootOrder, typeof(byte));

        foreach (var item in data)
        {
            if (includeDeviceId)
            {
                table.Rows.Add(item.DeviceId, item.BootPriorityId, item.BootOrder);
            }
            else
            {
                table.Rows.Add(item.BootPriorityId, item.BootOrder);
            }
        }

        return table;
    }

    private static IReadOnlyList<WindowsDeviceBootPriority> ReadDeviceBootPriorityCollection(IDataReader reader)
    {
        var result = new List<WindowsDeviceBootPriority>();
        var mapping = GetBootDevicePriorityMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseDeviceBootPriority(reader, mapping));
        }

        return result;
    }

    private static WindowsDeviceBootPriority ParseDeviceBootPriority(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new WindowsDeviceBootPriority
        {
            DeviceId = reader.GetInt32(mapping.DeviceId),
            BootPriorityId = reader.GetInt16(mapping.BootPriorityId),
            BootOrder = reader.GetByte(mapping.BootOrder)
        };
    }

    private static OrdinalColumnMapping GetBootDevicePriorityMapping(IDataRecord reader)
    {
        return new OrdinalColumnMapping
        {
            DeviceId = reader.GetOrdinal(DeviceId),
            BootPriorityId = reader.GetOrdinal(BootPriorityId),
            BootOrder = reader.GetOrdinal(BootOrder)
        };
    }

    private static IReadOnlyList<WindowsBootPriority> ReadBootPriorityCollection(IDataReader reader)
    {
        var result = new List<WindowsBootPriority>();
        var mapping = GetBootPriorityMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseBootPriority(reader, mapping));
        }

        return result;
    }

    private static WindowsBootPriority ParseBootPriority(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new WindowsBootPriority
        {
            BootPriorityId = reader.GetInt16(mapping.BootPriorityId),
            BootPriorityName = reader.GetString(mapping.BootPriorityName)
        };
    }

    private static OrdinalColumnMapping GetBootPriorityMapping(IDataRecord reader)
    {
        return new OrdinalColumnMapping
        {
            BootPriorityId = reader.GetOrdinal(BootPriorityId),
            BootPriorityName = reader.GetOrdinal(BootPriorityName)
        };
    }

    private sealed class OrdinalColumnMapping
    {
        /// <summary>
        /// Gets or sets the ordinal index for the DeviceId column.
        /// </summary>
        public int DeviceId { get; internal set; }

        /// <summary>
        /// Gets or sets the ordinal index for the BootPriorityId column.
        /// </summary>
        public int BootPriorityId { get; internal set; }

        /// <summary>
        /// Gets or sets the ordinal index for the BootOrder column.
        /// </summary>
        public int BootOrder { get; internal set; }

        /// <summary>
        /// Gets or sets the ordinal position of the BootPriorityName column.
        /// </summary>
        public int BootPriorityName { get; internal set; }
    }
}
