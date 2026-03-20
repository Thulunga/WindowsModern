using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Soti.Data.SqlClient;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

/// <summary>
/// Peripheral provider class.
/// </summary>
internal sealed class PeripheralDataProvider : IPeripheralDataProvider
{
    private const string Name = "PeripheralName";
    private const string ManufacturerId = "PeripheralManufacturerId";
    private const string PeripheralIdentity = "PeripheralId";
    private const string PeripheralTypeId = "PeripheralTypeId";
    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="PeripheralDataProvider"/> class.
    /// </summary>
    /// <param name="database"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public PeripheralDataProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc/>
    public PeripheralData GetPeripheralData(int peripheralId)
    {
        if (peripheralId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(peripheralId));
        }

        var command = _database.StoredProcedures["dbo.GEN_Peripheral_Get"];
        command.Parameters.Add(PeripheralIdentity, peripheralId);

        return command.ExecuteReader(ReadPeripheralCollection).FirstOrDefault();
    }

    /// <inheritdoc/>
    public int InsertPeripheralData(PeripheralData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (string.IsNullOrWhiteSpace(data.Name))
        {
            throw new ArgumentException(nameof(data.Name));
        }

        if (data.ManufacturerId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(data.ManufacturerId));
        }

        var command = _database.StoredProcedures["dbo.Peripheral_Insert"];

        command.Parameters.Add(Name, data.Name);
        command.Parameters.Add(ManufacturerId, data.ManufacturerId);
        command.Parameters.Add(PeripheralTypeId, data.PeripheralTypeId);
        command.Parameters.Add(PeripheralIdentity, out Ref<int> peripheralId);

        command.ExecuteNonQuery();
        return peripheralId;
    }

    /// <inheritdoc/>
    public IReadOnlyList<PeripheralData> GetAllPeripheralData()
    {
        var command = _database.StoredProcedures["dbo.GEN_Peripheral_GetAll"];

        return command.ExecuteReader(ReadPeripheralCollection);
    }

    /// <inheritdoc/>
    public void BulkDeletePeripheralData(int peripheralId)
    {
        if (peripheralId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(peripheralId));
        }

        var dataTable = new DataTable("GEN_UDT_NumericIdentifier");
        dataTable.Columns.Add(new DataColumn("Id", typeof(int)));

        var row = dataTable.NewRow();
        row["Id"] = peripheralId;
        dataTable.Rows.Add(row);

        var command = _database.StoredProcedures["dbo.GEN_Peripheral_BulkDelete"];
        command.Parameters.Add("Filter", dataTable, SqlDbType.Structured);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public void UpdatePeripheralType(int peripheralId, short peripheralTypeId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(peripheralId);
        ArgumentOutOfRangeException.ThrowIfNegative(peripheralTypeId);

        var command = _database.StoredProcedures["dbo.Peripheral_UpdatePeripheralTypeId"];

        command.Parameters.Add(PeripheralIdentity, peripheralId);
        command.Parameters.Add(PeripheralTypeId, peripheralTypeId);

        command.ExecuteNonQuery();
    }

    private static IReadOnlyList<PeripheralData> ReadPeripheralCollection(IDataReader reader)
    {
        var result = new List<PeripheralData>();
        var mapping = GetPeripheralDataMapping(reader);
        while (reader.Read())
        {
            result.Add(ParsePeripheralData(reader, mapping));
        }

        return result;
    }

    private static PeripheralData ParsePeripheralData(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new PeripheralData
        {
            PeripheralId = reader.GetInt32(mapping.PeripheralId),
            Name = reader.GetString(mapping.PeripheralName),
            ManufacturerId = reader.GetInt16(mapping.PeripheralManufacturerId),
            PeripheralTypeId = reader.GetByte(mapping.PeripheralTypeId)
        };
    }

    private static OrdinalColumnMapping GetPeripheralDataMapping(IDataRecord reader)
    {
        // OrdinalColumnMapping for Peripheral Info.
        return new OrdinalColumnMapping
        {
            PeripheralId = reader.GetOrdinal(PeripheralIdentity),
            PeripheralName = reader.GetOrdinal(Name),
            PeripheralManufacturerId = reader.GetOrdinal(ManufacturerId),
            PeripheralTypeId = reader.GetOrdinal(PeripheralTypeId)
        };
    }

    private sealed class OrdinalColumnMapping
    {
        public int PeripheralId { get; internal set; }

        public int PeripheralName { get; internal set; }

        public int PeripheralManufacturerId { get; internal set; }

        public int PeripheralTypeId { get; internal set; }
    }
}
