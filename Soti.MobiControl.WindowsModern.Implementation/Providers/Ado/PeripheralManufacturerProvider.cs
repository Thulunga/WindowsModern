using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.Data.SqlClient;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

/// <summary>
/// Peripheral manufacturer provider class.
/// </summary>
internal sealed class PeripheralManufacturerProvider : IPeripheralManufacturerProvider
{
    private const string Manufacturer = "PeripheralManufacturerName";
    private const string ManufacturerCode = "PeripheralManufacturerCode";
    private const string ManufacturerId = "PeripheralManufacturerId";
    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="PeripheralManufacturerProvider"/> class.
    /// </summary>
    /// <param name="database"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public PeripheralManufacturerProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc/>
    public IReadOnlyList<PeripheralManufacturerData> GetAllPeripheralManufacturerData()
    {
        var command = _database.StoredProcedures["dbo.GEN_PeripheralManufacturer_GetAll"];

        return command.ExecuteReader(ReadPeripheralManufacturerCollection);
    }

    /// <inheritdoc/>
    public PeripheralManufacturerData GetPeripheralManufacturerByManufacturerCode(string peripheralManufacturerCode)
    {
        if (string.IsNullOrWhiteSpace(peripheralManufacturerCode))
        {
            throw new ArgumentException("Parameter cannot be null or empty", nameof(peripheralManufacturerCode));
        }

        var command = _database.StoredProcedures["dbo.GEN_PeripheralManufacturer_GetByIndex_PeripheralManufacturerCode"];

        command.Parameters.Add(ManufacturerCode, peripheralManufacturerCode);

        return command.ExecuteReader(ReadPeripheralManufacturerCollection).FirstOrDefault();
    }

    /// <inheritdoc/>
    public void InsertPeripheralManufacturerData(PeripheralManufacturerData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (string.IsNullOrWhiteSpace(data.ManufacturerName))
        {
            throw new ArgumentException(nameof(data.ManufacturerName));
        }

        if (string.IsNullOrWhiteSpace(data.ManufacturerCode))
        {
            throw new ArgumentException(nameof(data.ManufacturerCode));
        }

        var command = _database.StoredProcedures["dbo.PeripheralManufacturer_Insert"];

        command.Parameters.Add(ManufacturerCode, data.ManufacturerCode);
        command.Parameters.Add(Manufacturer, data.ManufacturerName);
        command.Parameters.Add(ManufacturerId, out Ref<short> manufacturerId);

        command.ExecuteNonQuery();
        data.ManufacturerId = manufacturerId;
    }

    private static IReadOnlyList<PeripheralManufacturerData> ReadPeripheralManufacturerCollection(IDataReader reader)
    {
        var result = new List<PeripheralManufacturerData>();
        var mapping = GetPeripheralManufacturerMapping(reader);
        while (reader.Read())
        {
            result.Add(ParsePeripheralManufacturerData(reader, mapping));
        }

        return result;
    }

    private static OrdinalColumnMapping GetPeripheralManufacturerMapping(IDataRecord reader)
    {
        return new OrdinalColumnMapping
        {
            PeripheralManufacturerName = reader.GetOrdinal(Manufacturer),
            PeripheralManufacturerId = reader.GetOrdinal(ManufacturerId),
            PeripheralManufacturerCode = reader.GetOrdinal(ManufacturerCode)
        };
    }

    private static PeripheralManufacturerData ParsePeripheralManufacturerData(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new PeripheralManufacturerData
        {
            ManufacturerId = reader.GetInt16(mapping.PeripheralManufacturerId),
            ManufacturerName = reader.GetString(mapping.PeripheralManufacturerName),
            ManufacturerCode = reader.GetString(mapping.PeripheralManufacturerCode)
        };
    }

    private sealed class OrdinalColumnMapping
    {
        public int PeripheralManufacturerName { get; internal set; }

        public int PeripheralManufacturerCode { get; internal set; }

        public int PeripheralManufacturerId { get; internal set; }
    }
}
