using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Soti.Data.SqlClient;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.Utilities.Collections;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

/// <summary>
/// Data provider for WindowsDeviceGroupData entity.
/// </summary>
internal sealed class WindowsDeviceLocalGroupProvider : IWindowsDeviceLocalGroupProvider
{
    private const string DeviceId = "DeviceId";
    private const string DeviceIds = "DeviceIds";
    private const string DeviceGroupIds = "DeviceGroupIds";
    private const string IsAdminGroup = "IsAdminGroup";
    private const string WindowsDeviceLocalGroupNameId = "WindowsDeviceLocalGroupNameId";
    private const string WindowsDeviceLocalGroupNameIds = "WindowsDeviceLocalGroupNameIds";
    private const string WindowsDeviceLocalGroups = "WindowsDeviceLocalGroups";
    private const string WindowsDeviceLocalGroupUsers = "WindowsDeviceLocalGroupUsers";
    private const string LocalGroupName = "LocalGroupName";
    private const string WindowsDeviceLocalGroupId = "WindowsDeviceLocalGroupId";
    private const string DeletedRowCount = "DeletedRowCount";
    private const string Id = "Id";
    private const string WindowsDeviceUserId = "WindowsDeviceUserId";
    private const string UserName = "UserName";
    private const string DataKeyId = "DataKeyId";
    private const string UserSid = "UserSID";
    private const string WindowsDeviceUserIds = "WindowsDeviceUserIds";
    private const string Action = "Action";
    private const string Filter = "Filter";
    private const string ModificationWatermarkText = "ModificationWatermark";
    private const string WindowsDeviceLocalGroupEntries = "WindowsDeviceLocalGroupEntries";

    private const string GetLocalGroupsByDeviceId = "dbo.GEN_WindowsDeviceLocalGroup_GetByFK_DeviceId";
    private const string GetAllLocalGroupNamesData = "dbo.GEN_WindowsDeviceLocalGroupName_GetAll";
    private const string BulkModifyLocalGroupTable = "dbo.WindowsDeviceLocalGroup_BulkModify";
    private const string BulkModifyLocalGroupUserTable = "dbo.WindowsDeviceLocalGroupUser_BulkModify_ForDevice";
    private const string InsertLocalGroupNameData = "dbo.WindowsDeviceLocalGroupName_Insert";
    private const string GetByLocalGroupName = "dbo.GEN_WindowsDeviceLocalGroupName_GetByIndex_LocalGroupName";
    private const string CleanUpLocalGroupNameTable = "dbo.WindowsDeviceLocalGroupName_Cleanup";
    private const string DeleteAllByDeviceId = "dbo.WindowsDeviceLocalGroup_DeleteAllBy_DeviceId";
    private const string BulkModifyLocalGroupUserForGroup = "dbo.WindowsDeviceLocalGroupUser_BulkModify_ForGroup";
    private const string GetLocalUserDetailsByGroupAndDeviceId = "dbo.WindowsDeviceLocalGroupUser_GetUserDetailsBy_DeviceId_GroupNameId";
    private const string GetLocalUserDetailsByGroupAndDeviceIds = "dbo.WindowsDeviceLocalGroupUser_GetUserDetails_ByGroupNameIdAndDeviceIds";
    private const string GetLocalUserNamesByDeviceIdAndGroupNameIds = "dbo.WindowsDeviceLocalGroupUser_GetUserDetailsBy_DeviceId_GroupNameIds";
    private const string GetDistinctLocalGroupNameIdByDeviceIds = "dbo.WindowsDeviceLocalGroup_GetDistinctWindowsDeviceLocalGroupNameId_ByDeviceIds";
    private const string GetDistinctLocalGroupNameIdByDeviceGroupIds = "dbo.WindowsDeviceLocalGroup_GetDistinctWindowsDeviceLocalGroupNameId_ByDeviceGroupIds";
    private const string GetLocalGroupIdByDeviceIdAndGroupNameId = "dbo.GEN_WindowsDeviceLocalGroup_FindBy_DeviceIdWindowsDeviceLocalGroupNameId";
    private const string GetLocalGroupNameByGroupNameId = "dbo.GEN_WindowsDeviceLocalGroupName_BulkGet";
    private const string GetWindowsDeviceLocalGroupNameByWindowsDeviceUserId = "dbo.WindowsDeviceLocalGroupName_Get_ByWindowsDeviceUserIds";
    private const string UpsertWindowsDeviceLocalGroupData = "dbo.WindowsDeviceLocalGroup_Upsert_ByDeviceIdAndWindowsDeviceLocalGroupNameId";
    private const string InsertWindowsDeviceLocalGroupUser = "dbo.GEN_WindowsDeviceLocalGroupUser_Insert";
    private const string GetWindowsDeviceLocalGroupNameIdByDeviceIds = "dbo.WindowsDeviceLocalGroup_GetWindowsDeviceLocalGroupNameId_ByDeviceIds";
    private const string BulkGetModificationWatermarkByWindowsDeviceLocalGroupUc = "dbo.DevInfo_BulkGetModificationWatermark_ByWindowsDeviceLocalGroupUC";
    private const string GetBulkGroupNameIdByGroupNames = "dbo.WindowsDeviceLocalGroupName_Get_ByLocalGroupNames";
    private const string GetBulkGroupIdbyDeviceIdAndGroupNameIds = "dbo.WindowsDeviceLocalGroup_Find_ByDeviceId_WindowsDeviceLocalGroupNameIds";

    private const string UdtTableWindowsDeviceLocalGroup = "dbo.UDTTable_WindowsDeviceLocalGroup";
    private const string UdtTableWindowsDeviceLocalGroupUser = "dbo.UDTTable_WindowsDeviceLocalGroupUser";
    private const string UdtNumericIdentifier = "dbo.GEN_UDT_NumericIdentifier";
    private const string UdtTableWindowsDeviceLocalGroupBulkGetByUc = "dbo.UDTTable_WindowsDeviceLocalGroup_BulkGetByUC ";

    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the WindowsDeviceGroupDataProvider class.
    /// </summary>
    /// <param name="database">the instance of IDatabase.</param>
    public WindowsDeviceLocalGroupProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc />
    public int GetGroupIdByNameId(int deviceId, int groupNameId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        return GetLocalGroupIdByGroupNameId(deviceId, groupNameId)?.GroupId ?? 0;
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceLocalGroupData> GetAllLocalGroupsDataByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[GetLocalGroupsByDeviceId];
        command.Parameters.Add(DeviceId, deviceId);

        return command.ExecuteReader(ReadLocalGroupDataCollection);
    }

    /// <inheritdoc/>
    public IReadOnlyList<int> GetDistinctLocalGroupNameIdsByDeviceIds(IEnumerable<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        var deviceIdsList = deviceIds.AsArray();
        if (deviceIdsList.Count == 0)
        {
            return new List<int>();
        }

        var dataTable = new DataTable(UdtNumericIdentifier) { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.Add(new DataColumn(Id, typeof(int)) { AllowDBNull = false });
        foreach (var deviceId in deviceIdsList)
        {
            dataTable.Rows.Add(deviceId);
        }

        var command = _database.StoredProcedures[GetDistinctLocalGroupNameIdByDeviceIds];
        command.Parameters.Add(DeviceIds, dataTable, SqlDbType.Structured);

        return command.ExecuteReader(ReadDistinctLocalGroupNameIdsDataList);
    }

    /// <inheritdoc/>
    public IReadOnlyList<int> GetDistinctLocalGroupNameIdsByDeviceGroupIds(ISet<int> deviceGroupIds)
    {
        if (deviceGroupIds == null)
        {
            throw new ArgumentNullException(nameof(deviceGroupIds));
        }

        if (deviceGroupIds.Count == 0)
        {
            return new List<int>();
        }

        var dataTable = new DataTable(UdtNumericIdentifier) { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.Add(new DataColumn(Id, typeof(int)) { AllowDBNull = false });
        foreach (var deviceGroupId in deviceGroupIds)
        {
            dataTable.Rows.Add(deviceGroupId);
        }

        var command = _database.StoredProcedures[GetDistinctLocalGroupNameIdByDeviceGroupIds];
        command.Parameters.Add(DeviceGroupIds, dataTable, SqlDbType.Structured);

        return command.ExecuteReader(ReadDistinctLocalGroupNameIdsDataList);
    }

    /// <inheritdoc/>
    public void InsertLocalGroupName(string localGroupName, out int groupNameId)
    {
        if (string.IsNullOrWhiteSpace(localGroupName))
        {
            throw new ArgumentNullException(nameof(localGroupName));
        }

        var command = _database.StoredProcedures[InsertLocalGroupNameData];

        command.Parameters.Add(LocalGroupName, localGroupName);
        command.Parameters.Add(WindowsDeviceLocalGroupNameId, out Ref<int> windowsDeviceLocalGroupNameId);

        command.ExecuteNonQuery();

        groupNameId = windowsDeviceLocalGroupNameId;
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceLocalGroupNameData> GetAllLocalGroupNames()
    {
        var command = _database.StoredProcedures[GetAllLocalGroupNamesData];

        return command.ExecuteReader(ReadLocalGroupNameCollection);
    }

    /// <inheritdoc/>
    public WindowsDeviceLocalGroupNameData GetLocalGroupNameDataByLocalGroupName(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new ArgumentNullException(nameof(groupName));
        }

        var command = _database.StoredProcedures[GetByLocalGroupName];

        command.Parameters.Add(LocalGroupName, groupName);

        return command.ExecuteReader(ReadLocalGroupNameCollection).FirstOrDefault();
    }

    public Dictionary<string, WindowsDeviceLocalGroupNameData> GetLocalGroupNameDataByLocalGroupNames(
        IEnumerable<string> groupNames)
    {
        groupNames = groupNames?.AsArray();

        if (groupNames == null || !groupNames.Any())
        {
            throw new ArgumentNullException(nameof(groupNames));
        }

        var dataTable = ConvertToTable(groupNames.Distinct());

        var command = _database.StoredProcedures[GetBulkGroupNameIdByGroupNames];
        command.Parameters.Add("LocalGroupNames", dataTable, SqlDbType.Structured);

        var results = command.ExecuteReader(ReadWindowsDeviceLocalGroupNames);
        return results.ToDictionary(x => x.GroupName, x => x);
    }

    /// <inheritdoc/>
    public (int, IReadOnlyList<int>) CleanUpLocalGroupNamesData()
    {
        var command = _database.StoredProcedures[CleanUpLocalGroupNameTable];

        command.Parameters.Add(DeletedRowCount, out Ref<int> deletedRowCount);

        var deletedGroupNameIds = new List<int>();

        command.ExecuteReader(reader =>
        {
            while (reader.Read())
            {
                deletedGroupNameIds.Add(reader.GetInt32(0));
            }
        });

        return (deletedRowCount, deletedGroupNameIds);
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceLocalGroupUserGroupNameData> GetGroupNamesByWindowsDeviceUserIds(IEnumerable<int> windowsDeviceUserIds)
    {
        var windowsDeviceUserIdsList = windowsDeviceUserIds.AsArray();

        if (windowsDeviceUserIdsList.Count == 0)
        {
            throw new ArgumentNullException(nameof(windowsDeviceUserIds));
        }

        var dataTable = new DataTable(UdtNumericIdentifier);
        dataTable.Columns.Add(new DataColumn(Id, typeof(int)) { AllowDBNull = false });
        foreach (var windowsDeviceUserId in windowsDeviceUserIdsList)
        {
            dataTable.Rows.Add(windowsDeviceUserId);
        }

        var command = _database.StoredProcedures[GetWindowsDeviceLocalGroupNameByWindowsDeviceUserId];
        command.Parameters.Add(WindowsDeviceUserIds, dataTable, SqlDbType.Structured);

        return command.ExecuteReader(ReadGroupNames);
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceLocalGroup> LocalGroupTableBulkModify(int deviceId, IEnumerable<WindowsDeviceLocalGroupData> entries)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId), "Device ID must be greater than zero.");
        }

        var entryList = entries.AsArray();

        if (entryList.Count == 0)
        {
            throw new ArgumentNullException(nameof(entries), "Entries collection cannot be empty.");
        }

        var dataTable = CreateLocalGroupDataTable(entryList);

        var command = _database.StoredProcedures[BulkModifyLocalGroupTable];

        command.Parameters.Add(WindowsDeviceLocalGroups, dataTable, SqlDbType.Structured);
        command.Parameters.Add(DeviceId, deviceId);
        var result = new List<WindowsDeviceLocalGroup>();
        command.ExecuteReader(reader =>
        {
            while (reader.Read())
            {
                result.Add(new WindowsDeviceLocalGroup
                {
                    GroupId = reader.GetInt32(reader.GetOrdinal(WindowsDeviceLocalGroupId)),
                    GroupNameId = reader.GetInt32(reader.GetOrdinal(WindowsDeviceLocalGroupNameId)),
                    Action = reader.GetString(reader.GetOrdinal(Action))
                });
            }
        });

        return result;
    }

    /// <inheritdoc/>
    public IReadOnlyList<DeviceWindowsLocalGroupSearchDataSummary> GetDeviceLocalGroupsSearchData(IEnumerable<WindowsDeviceLocalGroupData> entries)
    {
        var entryList = entries?.AsArray() ?? throw new ArgumentNullException(nameof(entries), "Entries collection cannot be null.");

        if (entryList.Count == 0)
        {
            throw new ArgumentException("Entries collection cannot be empty.", nameof(entries));
        }

        var dataTable = CreateLocalGroupSearchDataTable(entryList);

        var command = _database.StoredProcedures[BulkGetModificationWatermarkByWindowsDeviceLocalGroupUc];

        command.Parameters.Add(WindowsDeviceLocalGroupEntries, dataTable, SqlDbType.Structured);

        return command.ExecuteReader(ReadDeviceLocalGroupsSearchCollection);
    }

    /// <inheritdoc />
    public void LocalGroupUserTableBulkModify(int deviceId, IEnumerable<WindowsDeviceLocalGroupUserData> entries)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var windowsDeviceLocalGroupUserEntries = entries.AsArray();
        if (windowsDeviceLocalGroupUserEntries.Count == 0)
        {
            throw new ArgumentNullException(nameof(entries));
        }

        var dataTable = CreateLocalGroupUserDataTable(windowsDeviceLocalGroupUserEntries);

        var command = _database.StoredProcedures[BulkModifyLocalGroupUserTable];

        command.Parameters.Add(WindowsDeviceLocalGroupUsers, dataTable, SqlDbType.Structured);
        command.Parameters.Add(DeviceId, deviceId);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public IReadOnlyList<WindowsDeviceLocalGroupNameData> GetLocalGroupNameByIds(IEnumerable<int> groupNameIds)
    {
        var groupNameIdsList = groupNameIds.AsArray();

        if (groupNameIdsList.Count == 0)
        {
            throw new ArgumentNullException(nameof(groupNameIds));
        }

        var command = _database.StoredProcedures[GetLocalGroupNameByGroupNameId];

        var dataTable = ConvertToTable(groupNameIdsList);

        command.Parameters.Add(Filter, dataTable, SqlDbType.Structured);

        return command.ExecuteReader(ReadLocalGroupNameCollection);
    }

    /// <inheritdoc />
    public void DeleteByDeviceId(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[DeleteAllByDeviceId];
        command.Parameters.Add(DeviceId, deviceId);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public void LocalGroupUserTableBulkModifyForGroup(IEnumerable<int> entries, int localGroupId)
    {
        if (localGroupId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(localGroupId));
        }

        if (entries == null)
        {
            throw new ArgumentNullException(nameof(entries));
        }

        var dataTable = ConvertToTable(entries);

        var command = _database.StoredProcedures[BulkModifyLocalGroupUserForGroup];

        command.Parameters.Add(WindowsDeviceUserIds, dataTable, SqlDbType.Structured);
        command.Parameters.Add(WindowsDeviceLocalGroupId, localGroupId);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceLocalGroupUserDetailsData> GetLocalGroupUserDetailsByDeviceIdAndGroupId(int deviceId, int groupNameId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        var command = _database.StoredProcedures[GetLocalUserDetailsByGroupAndDeviceId];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(WindowsDeviceLocalGroupNameId, groupNameId);

        return command.ExecuteReader(ReadLocalGroupUserDetailsDataCollection);
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceLocalGroupUserDetailsData> GetLocalGroupUserDetailsByDeviceIdsAndGroupId(IEnumerable<int> deviceIds, int groupNameId)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        var deviceIdsList = deviceIds.AsArray();
        if (deviceIdsList.Count == 0)
        {
            return new List<WindowsDeviceLocalGroupUserDetailsData>();
        }

        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        var dataTable = new DataTable(UdtNumericIdentifier) { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.Add(new DataColumn(Id, typeof(int)) { AllowDBNull = false });
        foreach (var deviceId in deviceIdsList)
        {
            dataTable.Rows.Add(deviceId);
        }

        var command = _database.StoredProcedures[GetLocalUserDetailsByGroupAndDeviceIds];
        command.Parameters.Add(DeviceIds, dataTable, SqlDbType.Structured);
        command.Parameters.Add(WindowsDeviceLocalGroupNameId, groupNameId);

        return command.ExecuteReader(ReadLocalGroupUserDetailsDataList);
    }

    /// <inheritdoc />
    public IReadOnlyList<WindowsDeviceLocalGroupUserDetailsModal> GetUserNamesByDeviceIdAndGroupNameIds(int deviceId, IEnumerable<int> groupNameIds)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var groupNameIdsList = groupNameIds.AsArray();
        if (groupNameIdsList.Count == 0)
        {
            return new List<WindowsDeviceLocalGroupUserDetailsModal>();
        }

        var dataTable = new DataTable(UdtNumericIdentifier) { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.Add(new DataColumn(Id, typeof(int)) { AllowDBNull = false });
        foreach (var groupNameId in groupNameIdsList)
        {
            dataTable.Rows.Add(groupNameId);
        }

        var command = _database.StoredProcedures[GetLocalUserNamesByDeviceIdAndGroupNameIds];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(WindowsDeviceLocalGroupNameIds, dataTable, SqlDbType.Structured);

        return command.ExecuteReader(ReadLocalUserNamesList);
    }

    /// <inheritdoc/>
    public WindowsDeviceLocalGroupData GetLocalGroupIdByGroupNameId(int deviceId, int groupNameId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        var command = _database.StoredProcedures[GetLocalGroupIdByDeviceIdAndGroupNameId];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(WindowsDeviceLocalGroupNameId, groupNameId);

        return command.ExecuteReader(ReadLocalGroupId).FirstOrDefault();
    }

    /// <inheritdoc/>
    public void UpsertWindowsDeviceLocalGroup(int deviceId, int groupNameId, out int windowsDeviceLocalGroupId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        var command = _database.StoredProcedures[UpsertWindowsDeviceLocalGroupData];

        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(WindowsDeviceLocalGroupNameId, groupNameId);
        command.Parameters.Add(WindowsDeviceLocalGroupId, out Ref<int> windowsDeviceGroupId);

        command.ExecuteNonQuery();

        windowsDeviceLocalGroupId = windowsDeviceGroupId;
    }

    /// <inheritdoc/>
    public void InsertLocalGroupUser(int windowsDeviceUserId, int windowsDeviceLocalGroupId)
    {
        if (windowsDeviceUserId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowsDeviceUserId));
        }

        if (windowsDeviceLocalGroupId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowsDeviceLocalGroupId));
        }

        var command = _database.StoredProcedures[InsertWindowsDeviceLocalGroupUser];
        command.Parameters.Add(WindowsDeviceLocalGroupId, windowsDeviceLocalGroupId);
        command.Parameters.Add(WindowsDeviceUserId, windowsDeviceUserId);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public IReadOnlyList<DeviceWindowsLocalGroupSearchDataSummary> GetDeviceLocalGroupWatermarks(IEnumerable<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        var deviceIdsList = deviceIds.AsArray();

        if (deviceIdsList.Count == 0)
        {
            return new List<DeviceWindowsLocalGroupSearchDataSummary>();
        }

        var command = _database.StoredProcedures[GetWindowsDeviceLocalGroupNameIdByDeviceIds];
        var datatable = ConvertToTable(deviceIdsList);
        command.Parameters.Add(DeviceIds, datatable, SqlDbType.Structured);
        return command.ExecuteReader(ReadDeviceLocalGroupsSearchCollection);
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceLocalGroupData> GetLocalGroupsByDeviceAndNameIds(
    int deviceId,
    IEnumerable<int> windowsDeviceLocalGroupNameIds)
    {
        windowsDeviceLocalGroupNameIds = windowsDeviceLocalGroupNameIds?.AsArray();

        if (windowsDeviceLocalGroupNameIds == null || !windowsDeviceLocalGroupNameIds.Any())
        {
            throw new ArgumentNullException(nameof(windowsDeviceLocalGroupNameIds));
        }

        var dataTable = ConvertToTable(windowsDeviceLocalGroupNameIds.Distinct());

        var command = _database.StoredProcedures[GetBulkGroupIdbyDeviceIdAndGroupNameIds];
        command.Parameters.Add("DeviceId", deviceId, SqlDbType.Int);
        command.Parameters.Add("WindowsDeviceLocalGroupNameIds", dataTable, SqlDbType.Structured);

        return command.ExecuteReader(ReadWindowsDeviceLocalGroups);
    }

    private static IReadOnlyList<WindowsDeviceLocalGroupUserDetailsData> ReadLocalGroupUserDetailsDataCollection(IDataReader reader)
    {
        var result = new List<WindowsDeviceLocalGroupUserDetailsData>();
        var mapping = GetLocalGroupUserDetailsColumnMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseLocalGroupUserDetailsDataEntity(reader, mapping));
        }

        return result;
    }

    private IReadOnlyList<WindowsDeviceLocalGroupData> ReadWindowsDeviceLocalGroups(IDataReader reader)
    {
        var results = new List<WindowsDeviceLocalGroupData>();
        while (reader.Read())
        {
            results.Add(new WindowsDeviceLocalGroupData
            {
                GroupId = reader.GetInt32(reader.GetOrdinal("WindowsDeviceLocalGroupId")),
                GroupNameId = reader.GetInt32(reader.GetOrdinal("WindowsDeviceLocalGroupNameId"))
            });
        }
        return results;
    }

    private static OrdinalColumnMapping GetLocalGroupUserDetailsColumnMapping(IDataReader reader)
    {
        return new OrdinalColumnMapping
        {
            WindowsDeviceUserId = reader.GetOrdinal(WindowsDeviceUserId),
            UserSid = reader.GetOrdinal(UserSid),
            UserName = reader.GetOrdinal(UserName),
            DataKeyId = reader.GetOrdinal(DataKeyId),
            IsAdminGroup = reader.GetOrdinal(IsAdminGroup)
        };
    }

    private static IReadOnlyCollection<WindowsDeviceLocalGroupData> ReadLocalGroupId(IDataReader reader)
    {
        var result = new List<WindowsDeviceLocalGroupData>();
        var mapping = GetLocalGroupIdDataColumnMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseLocalGroupIdDataEntity(reader, mapping));
        }

        return result;
    }

    private static IEnumerable<WindowsDeviceLocalGroupNameData> ReadWindowsDeviceLocalGroupNames(IDataReader reader)
    {
        var result = new List<WindowsDeviceLocalGroupNameData>();
        while (reader.Read())
        {
            result.Add(new WindowsDeviceLocalGroupNameData
            {
                GroupNameId = reader.GetInt32(reader.GetOrdinal("WindowsDeviceLocalGroupNameId")),
                GroupName = reader.GetString(reader.GetOrdinal("LocalGroupName"))
            });
        }
        return result;
    }

    private static OrdinalColumnMapping GetLocalGroupIdDataColumnMapping(IDataReader reader)
    {
        return new OrdinalColumnMapping
        {
            GroupId = reader.GetOrdinal(WindowsDeviceLocalGroupId),
        };
    }

    private static WindowsDeviceLocalGroupData ParseLocalGroupIdDataEntity(IDataReader reader, OrdinalColumnMapping mapping)
    {
        return new WindowsDeviceLocalGroupData
        {
            GroupId = reader.GetInt32(mapping.GroupId),
        };
    }

    private static IReadOnlyList<WindowsDeviceLocalGroupData> ReadLocalGroupDataCollection(IDataReader reader)
    {
        var result = new List<WindowsDeviceLocalGroupData>();
        var mapping = GetLocalGroupDataColumnMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseLocalGroupDataEntity(reader, mapping));
        }

        return result;
    }

    private static WindowsDeviceLocalGroupUserDetailsData ParseLocalGroupUserDetailsDataEntity(IDataReader reader, OrdinalColumnMapping mapping)
    {
        return new WindowsDeviceLocalGroupUserDetailsData
        {
            WindowsDeviceUserId = reader.GetInt32(mapping.WindowsDeviceUserId),
            UserSid = reader.GetString(mapping.UserSid),
            UserName = reader.GetString(mapping.UserName),
            DataKeyId = reader.GetInt32(mapping.DataKeyId),
            IsAdminGroup = reader.GetBoolean(mapping.IsAdminGroup)
        };
    }

    private static WindowsDeviceLocalGroupData ParseLocalGroupDataEntity(IDataReader reader, OrdinalColumnMapping mapping)
    {
        return new WindowsDeviceLocalGroupData
        {
            GroupId = reader.GetInt32(mapping.GroupId),
            DeviceId = reader.GetInt32(mapping.DeviceId),
            GroupNameId = reader.GetInt32(mapping.GroupNameId),
            IsAdminGroup = reader.GetBoolean(mapping.IsAdminGroup),
        };
    }

    private static OrdinalColumnMapping GetLocalGroupDataColumnMapping(IDataReader reader)
    {
        return new OrdinalColumnMapping
        {
            GroupId = reader.GetOrdinal(WindowsDeviceLocalGroupId),
            DeviceId = reader.GetOrdinal(DeviceId),
            GroupNameId = reader.GetOrdinal(WindowsDeviceLocalGroupNameId),
            IsAdminGroup = reader.GetOrdinal(IsAdminGroup),
        };
    }

    private static IReadOnlyList<WindowsDeviceLocalGroupNameData> ReadLocalGroupNameCollection(IDataReader reader)
    {
        var result = new List<WindowsDeviceLocalGroupNameData>();
        var mapping = GetLocalGroupNameColumnMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseLocalGroupNameEntity(reader, mapping));
        }

        return result;
    }

    private static IReadOnlyList<DeviceWindowsLocalGroupSearchDataSummary> ReadDeviceLocalGroupsSearchCollection(IDataReader reader)
    {
        var result = new List<DeviceWindowsLocalGroupSearchDataSummary>();
        var mapping = GetDeviceLocalGroupsSearchDataColumnMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseDeviceLocalGroupsSearchData(reader, mapping));
        }

        return result;
    }

    private static WindowsDeviceLocalGroupNameData ParseLocalGroupNameEntity(IDataReader reader, OrdinalColumnMapping mapping)
    {
        return new WindowsDeviceLocalGroupNameData
        {
            GroupNameId = reader.GetInt32(mapping.GroupNameId),
            GroupName = reader.GetString(mapping.GroupName),
        };
    }

    private static OrdinalColumnMapping GetLocalGroupNameColumnMapping(IDataReader reader)
    {
        return new OrdinalColumnMapping
        {
            GroupNameId = reader.GetOrdinal(WindowsDeviceLocalGroupNameId),
            GroupName = reader.GetOrdinal(LocalGroupName),
        };
    }

    private static OrdinalColumnMapping GetDeviceLocalGroupsSearchDataColumnMapping(IDataRecord reader)
    {
        return new OrdinalColumnMapping
        {
            GroupNameId = reader.GetOrdinal(WindowsDeviceLocalGroupNameId),
            DeviceId = reader.GetOrdinal(DeviceId),
            ModificationWatermark = reader.GetOrdinal(ModificationWatermarkText),
        };
    }

    private static IReadOnlyList<WindowsDeviceLocalGroupUserGroupNameData> ReadGroupNames(IDataReader reader)
    {
        var result = new List<WindowsDeviceLocalGroupUserGroupNameData>();
        var mapping = GetGroupNamesColumnMapping(reader);

        while (reader.Read())
        {
            result.Add(ParseUserGroupNameEntity(reader, mapping));
        }

        return result;
    }

    private static OrdinalColumnMapping GetGroupNamesColumnMapping(IDataRecord reader)
    {
        return new OrdinalColumnMapping
        {
            WindowsDeviceUserId = reader.GetOrdinal(WindowsDeviceUserId),
            GroupNameId = reader.GetOrdinal(WindowsDeviceLocalGroupNameId),
            GroupName = reader.GetOrdinal(LocalGroupName)
        };
    }

    private static WindowsDeviceLocalGroupUserGroupNameData ParseUserGroupNameEntity(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new WindowsDeviceLocalGroupUserGroupNameData
        {
            WindowsDeviceUserId = reader.GetInt32(mapping.WindowsDeviceUserId),
            WindowsDeviceLocalGroupNameId = reader.GetInt32(mapping.GroupNameId),
            LocalGroupName = reader.GetString(mapping.GroupName)
        };
    }

    private static DeviceWindowsLocalGroupSearchDataSummary ParseDeviceLocalGroupsSearchData(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new DeviceWindowsLocalGroupSearchDataSummary
        {
            GroupNameId = reader.GetInt32(mapping.GroupNameId),
            DeviceId = reader.GetInt32(mapping.DeviceId),
            ModificationWatermark = reader.GetInt16(mapping.ModificationWatermark),
        };
    }

    private static DataTable CreateLocalGroupDataTable(IEnumerable<WindowsDeviceLocalGroupData> windowsDeviceLocalGroupData)
    {
        var dataTable = new DataTable(UdtTableWindowsDeviceLocalGroup)
        {
            Locale = CultureInfo.InvariantCulture,
        };

        dataTable.Columns.AddRange(new[]
        {
            new DataColumn(WindowsDeviceLocalGroupNameId, typeof(int)) { AllowDBNull = false },
            new DataColumn(IsAdminGroup, typeof(bool)) { AllowDBNull = false }
        });

        foreach (var group in windowsDeviceLocalGroupData)
        {
            dataTable.Rows.Add(
                group.GroupNameId,
                group.IsAdminGroup
            );
        }

        return dataTable;
    }

    private static DataTable CreateLocalGroupSearchDataTable(IEnumerable<WindowsDeviceLocalGroupData> windowsDeviceLocalGroupData)
    {
        var dataTable = new DataTable(UdtTableWindowsDeviceLocalGroupBulkGetByUc) { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.AddRange(
        [
            new DataColumn(DeviceId, typeof(int)) { AllowDBNull = false },
            new DataColumn(WindowsDeviceLocalGroupNameId, typeof(int)) { AllowDBNull = false }
        ]);

        foreach (var group in windowsDeviceLocalGroupData)
        {
            dataTable.Rows.Add(
                group.DeviceId,
                group.GroupNameId
            );
        }

        return dataTable;
    }

    private static DataTable CreateLocalGroupUserDataTable(IEnumerable<WindowsDeviceLocalGroupUserData> windowsDeviceLocalGroupUserData)
    {
        var dataTable = new DataTable(UdtTableWindowsDeviceLocalGroupUser) { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.AddRange(
        [
            new DataColumn(WindowsDeviceLocalGroupId, typeof(int)) { AllowDBNull = false },
            new DataColumn(WindowsDeviceUserId, typeof(int)) { AllowDBNull = false }
        ]);

        foreach (var group in windowsDeviceLocalGroupUserData)
        {
            dataTable.Rows.Add(
                group.GroupId,
                group.UserId
            );
        }

        return dataTable;
    }

    private static DataTable ConvertToTable<T>(IEnumerable<T> sourceCollection)
    {
        var groupIdsTable = new DataTable();
        groupIdsTable.Columns.Add("Id", typeof(T));
        foreach (var id in sourceCollection)
        {
            groupIdsTable.Rows.Add(id);
        }

        return groupIdsTable;
    }

    private static IReadOnlyList<WindowsDeviceLocalGroupUserDetailsData> ReadLocalGroupUserDetailsDataList(IDataReader reader)
    {
        var result = new List<WindowsDeviceLocalGroupUserDetailsData>();
        var mapping = BulkGetLocalGroupUserDetailsColumnMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseBulkLocalGroupUserDetailsDataEntity(reader, mapping));
        }

        return result;
    }

    private static OrdinalColumnMapping BulkGetLocalGroupUserDetailsColumnMapping(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            DeviceId = record.GetOrdinal(DeviceId),
            WindowsDeviceUserId = record.GetOrdinal(WindowsDeviceUserId),
            UserSid = record.GetOrdinal(UserSid),
            UserName = record.GetOrdinal(UserName),
            DataKeyId = record.GetOrdinal(DataKeyId),
            IsAdminGroup = record.GetOrdinal(IsAdminGroup)
        };
    }

    private static WindowsDeviceLocalGroupUserDetailsData ParseBulkLocalGroupUserDetailsDataEntity(IDataRecord record, OrdinalColumnMapping mapping)
    {
        return new WindowsDeviceLocalGroupUserDetailsData
        {
            DeviceId = record.GetInt32(mapping.DeviceId),
            WindowsDeviceUserId = record.GetInt32(mapping.WindowsDeviceUserId),
            UserSid = record.GetString(mapping.UserSid),
            UserName = record.GetString(mapping.UserName),
            DataKeyId = record.GetInt32(mapping.DataKeyId),
            IsAdminGroup = record.GetBoolean(mapping.IsAdminGroup)
        };
    }

    private static IReadOnlyList<int> ReadDistinctLocalGroupNameIdsDataList(IDataReader reader)
    {
        var result = new List<int>();
        var mapping = GetDistinctLocalGroupNameIdsColumnMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseDistinctLocalGroupNameIdsDataEntity(reader, mapping));
        }

        return result;
    }

    private static OrdinalColumnMapping GetDistinctLocalGroupNameIdsColumnMapping(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            GroupNameId = record.GetOrdinal(WindowsDeviceLocalGroupNameId)
        };
    }

    private static int ParseDistinctLocalGroupNameIdsDataEntity(IDataRecord record, OrdinalColumnMapping mapping)
    {
        return record.GetInt32(mapping.GroupNameId);
    }

    private static IReadOnlyList<WindowsDeviceLocalGroupUserDetailsModal> ReadLocalUserNamesList(IDataReader reader)
    {
        var result = new List<WindowsDeviceLocalGroupUserDetailsModal>();
        var mapping = GetLocalUserNamesColumnMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseLocalUserNamesEntity(reader, mapping));
        }

        return result;
    }

    private static OrdinalColumnMapping GetLocalUserNamesColumnMapping(IDataRecord record)
    {
        return new OrdinalColumnMapping
        {
            GroupNameId = record.GetOrdinal(WindowsDeviceLocalGroupNameId),
            WindowsDeviceUserId = record.GetOrdinal(WindowsDeviceUserId),
            UserName = record.GetOrdinal(UserName)
        };
    }

    private static WindowsDeviceLocalGroupUserDetailsModal ParseLocalUserNamesEntity(IDataRecord record, OrdinalColumnMapping mapping)
    {
        return new WindowsDeviceLocalGroupUserDetailsModal
        {
            GroupNameId = record.GetInt32(mapping.GroupNameId),
            WindowsDeviceUserId = record.GetInt32(mapping.WindowsDeviceUserId),
            UserName = record.GetString(mapping.UserName)
        };
    }

    /// <summary>
    /// Internal struct that caches column name-ordinal mapping.
    /// </summary>
    private class OrdinalColumnMapping
    {
        public int DeviceId { get; internal set; }

        public int GroupId { get; internal set; }

        public int GroupNameId { get; internal set; }

        public int GroupName { get; internal set; }

        public int IsAdminGroup { get; internal set; }

        public int WindowsDeviceUserId { get; internal set; }

        public int UserSid { get; internal set; }

        public int UserName { get; internal set; }

        public int DataKeyId { get; internal set; }

        public int ModificationWatermark { get; internal set; }
    }
}
