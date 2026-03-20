using System;
using System.Collections.Generic;
using System.Data;
using Soti.Data.SqlClient;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

/// <summary>
/// Data provider for <see cref="TmpWindowsDeviceUserData"/> entity.
/// </summary>
internal sealed class TmpWindowsDeviceUserDataProvider : ITmpWindowsDeviceUserDataProvider
{
    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="TmpWindowsDeviceUserDataProvider"/> class.
    /// </summary>
    /// <param name="database">the instance of IDatabase.</param>
    public TmpWindowsDeviceUserDataProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc/>
    public void Delete(IEnumerable<int> entities)
    {
        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        var dataTable = Int32ListToDataTable(entities);

        var command = _database.StoredProcedures["dbo.GEN_TmpWindowsDeviceUser_BulkDelete"];
        command.Parameters.Add("Filter", dataTable, SqlDbType.Structured);

        command.ExecuteNonQuery();
    }

    public IEnumerable<WindowsDeviceTmpLocalUserModel> GetTmpLocalUsersBatch(int skip, int take)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip), "Skip cannot be negative");
        }

        if (take < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take cannot be negative");
        }

        var command = _database.CreateStoredProcedureCommand("dbo.TmpWindowsDeviceUser_GetAllPaginated");
        command.Parameters.Add("SkipRecords", skip);
        command.Parameters.Add("TakeRecords", take);

        return command.ExecuteCollection<WindowsDeviceTmpLocalUserModel>(ParseWindowsDeviceTmpLocalUser);
    }

    private WindowsDeviceTmpLocalUserModel ParseWindowsDeviceTmpLocalUser(IDataRecord record)
    {
        return new WindowsDeviceTmpLocalUserModel
        {
            WindowsDeviceUserId = record.GetInt32(record.GetOrdinal("WindowsDeviceUserId")),
            UserName = record.IsDBNull(record.GetOrdinal("UserNameEncrypted"))
                ? null : GetBytes(record, "UserNameEncrypted"),
            UserFullName = record.IsDBNull(record.GetOrdinal("UserFullNameEncrypted"))
                ? null : GetBytes(record, "UserFullNameEncrypted")
        };
    }

    private static byte[] GetBytes(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? Array.Empty<byte>() : (byte[])record[ordinal];
    }

    private static DataTable Int32ListToDataTable(IEnumerable<int> entities)
    {
        var dataTable = new DataTable();

        dataTable.Columns.Add(new DataColumn("Id", typeof(int)) { AllowDBNull = false });

        foreach (var entity in entities)
        {
            dataTable.Rows.Add(entity);
        }

        return dataTable;
    }
}
