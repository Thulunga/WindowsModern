using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Soti.Data.SqlClient;
using Soti.MobiControl.Devices;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.Utilities.Extensions;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

internal sealed class WindowsDefenderDataProvider : IWindowsDefenderDataProvider
{
    private const string Id = "Id";
    private const string DeviceId = "DeviceId";
    private const string DevId = "DevId";
    private const string DeviceIds = "DeviceIds";
    private const string LastStatusChangeStartTime = "AntivirusThreatLastStatusChangeTimeFrom";
    private const string LastStatusChangeEndTime = "AntivirusThreatLastStatusChangeTimeTo";
    private const string AntivirusThreatTypeIds = "AntivirusThreatTypeIds";
    private const string AntivirusThreatSeverityIds = "AntivirusThreatSeverityIds";
    private const string AntivirusLastThreatStatusIds = "AntivirusLastThreatStatusIds";
    private const string AntivirusThreatTypeId = "AntivirusThreatTypeId";
    private const string AntivirusThreatSeverityId = "AntivirusThreatSeverityId";
    private const string AntivirusLastThreatStatusId = "AntivirusLastThreatStatusId";
    private const string DeviceHasLastThreatStatusId = "DeviceHasLastThreatStatusId";
    private const string NoOfDevices = "NoOfDevices";
    private const string Skip = "SkipRecords";
    private const string Take = "TakeRecords";
    private const string GetThreatDetectionHistoryByGroupIds = "DeviceAntivirusThreatStatus_GetThreatDetectionHistoryBy_DeviceGroupIds";
    private const string GetPaginatedSummaryByDeviceId = "dbo.DeviceAntivirusThreatStatus_GetPaginatedSummary_ByDeviceId";
    private const string GetActiveThreatExistsByDeviceId = "dbo.DeviceAntivirusThreatStatus_CheckIfExists_ByDeviceIdLastStatusId";
    private const string GetActiveThreatsExistsByDeviceIds = "dbo.DeviceAntivirusThreatStatus_CheckIfExists_ByDeviceIdsLastStatusId";
    private const string GetThreatStatusIdCount = "[dbo].[DeviceAntivirusThreatStatus_GetThreatStatusIdCount_ByAntivirusThreatLastStatusChangeTimeRange]";
    private const string DeleteDeviceAntivirusThreatStatusByFkDeviceId = "[dbo].[GEN_DeviceAntivirusThreatStatus_DeleteByFk_DeviceId]";
    private const string ExternalAntivirusThreatId = "ExternalAntivirusThreatId";
    private const string GetAllAntivirusHistoryByDeviceId = "dbo.DeviceAntivirusThreatStatus_GetSummary_ByDeviceId";
    private const string AntivirusThreatName = "ThreatName";
    private const string AntivirusThreatInitialDetectionTime = "AntivirusThreatInitialDetectionTime";
    private const string InitialDetectionTime = "InitialDetectionTime";
    private const string AntivirusThreatLastStatusChangeTime = "AntivirusThreatLastStatusChangeTime";
    private const string LastStatusChangeTime = "LastStatusChangeTime";
    private const string AntivirusCurrentDetectionCount = "CurrentDetectionCount";
    private const string TinyIntIdentifier = "GEN_UDT_TinyIntIdentifier";
    private const string ThreatStatusCount = "ThreatStatusCount";
    private const string GroupIds = "DeviceGroupIds";
    private const string DeviceFamily = "DeviceFamilyId";
    private const string NumericIdentifier = "GEN_UDT_NumericIdentifier";
    private const string GetThreatStatusIdCountByDeviceId = "[dbo].[DeviceAntivirusThreatStatus_GetThreatStatusIdCountBy_DeviceId]";
    private const string GetPaginatedLastStatusChangeDates = "[dbo].[DeviceAntivirusThreatStatus_GetDeviceLastStatusPaginated]";
    private const string GetLastStatusChangeDates = "[dbo].[DeviceAntivirusThreatStatus_GetDeviceLastStatus]";
    private const string LastAntivirusSyncTime = "LastAntivirusSyncTime";
    private const string DeviceGroupIds = "DeviceGroupIds";
    private const string DeviceFamilyId = "DeviceFamilyId";
    private const string TotalRecords = "TotalRecords";
    private const string FamilyId = "FamilyId";
    private const string GetDeviceThreatStatusByThreatId = "dbo.DeviceAntivirusThreatStatus_GetDeviceDetectionBy_ExternalAntivirusThreatId";
    private const string GetDeviceThreatStatusByThreatIdPaginated = "dbo.DeviceAntivirusThreatStatus_GetDeviceDetectionPaginatedBy_ExternalAntivirusThreatId";
    private const string SortBy = "SortBy";
    private const string SortOrder = "SortOrder";
    private const string GetAntivirusThreatId = "[dbo].[DeviceAntivirusThreatStatus_AggBy_ExternalAntivirusThreatId]";
    private const string NumberOfDevices = "NumberOfDevices";

    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDefenderDataProvider"/> class.
    /// </summary>
    /// <param name="database">The instance of IDatabase.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public WindowsDefenderDataProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusThreatData> GetAntivirusThreatHistory(
        int deviceId,
        IEnumerable<byte> threatTypeIds,
        IEnumerable<byte> threatSeverityIds,
        IEnumerable<byte> threatStatusIds,
        DateTime statusChangeStartTime,
        DateTime statusChangeEndTime,
        int skip,
        int take,
        AntivirusThreatSortByOption sortBy,
        bool order,
        out int totalCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);
        ArgumentNullException.ThrowIfNull(threatTypeIds);
        var typeIds = threatTypeIds as byte[] ?? threatTypeIds.ToArray();
        if (typeIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(threatTypeIds));
        }

        ArgumentNullException.ThrowIfNull(threatSeverityIds);
        var severityIds = threatSeverityIds as byte[] ?? threatSeverityIds.ToArray();
        if (severityIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(threatSeverityIds));
        }

        ArgumentNullException.ThrowIfNull(threatStatusIds);
        var statusIds = threatStatusIds as byte[] ?? threatStatusIds.ToArray();
        if (statusIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(threatStatusIds));
        }

        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(skip, take);
        ValidateStartAndEndTime(statusChangeStartTime, statusChangeEndTime);
        sortBy.EnsureDefined();

        var command = _database.StoredProcedures[GetPaginatedSummaryByDeviceId];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(LastStatusChangeStartTime, DateTime.SpecifyKind(statusChangeStartTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(LastStatusChangeEndTime, DateTime.SpecifyKind(statusChangeEndTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(AntivirusThreatTypeIds, CreateTinyIntIdentifierTable(typeIds), SqlDbType.Structured);
        command.Parameters.Add(AntivirusThreatSeverityIds, CreateTinyIntIdentifierTable(severityIds), SqlDbType.Structured);
        command.Parameters.Add(AntivirusLastThreatStatusIds, CreateTinyIntIdentifierTable(statusIds), SqlDbType.Structured);
        command.Parameters.Add(Skip, skip);
        command.Parameters.Add(Take, take);
        command.Parameters.Add(SortBy, sortBy.ToString());
        command.Parameters.Add(SortOrder, order ? "DESC" : "ASC");
        command.Parameters.Add(TotalRecords, out Ref<int> total);
        var threatHistory = command.ExecuteReader(ReadCollection);
        totalCount = total;
        return threatHistory;
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusThreatStatusDeviceData> GetAntivirusThreatStatus(
        IEnumerable<int> deviceGroupIds,
        DeviceFamily deviceFamily,
        DateTime lastStatusChangeStartDate,
        DateTime lastStatusChangeEndDate,
        AntivirusThreatStatus threatStatus,
        int skip,
        int take,
        bool isDescendingOrder,
        out int totalCount)
    {
        ArgumentNullException.ThrowIfNull(deviceGroupIds);
        var groupIds = deviceGroupIds as int[] ?? deviceGroupIds.ToArray();
        if (groupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(deviceGroupIds));
        }

        deviceFamily.EnsureDefined();
        threatStatus.EnsureDefined();
        ValidateStartAndEndTime(lastStatusChangeStartDate, lastStatusChangeEndDate);
        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(skip, take);

        var command = _database.StoredProcedures[GetPaginatedLastStatusChangeDates];
        command.Parameters.Add(GroupIds, CreateIntIdentifierTable(groupIds), SqlDbType.Structured);
        command.Parameters.Add(DeviceFamilyId, (int)deviceFamily);
        command.Parameters.Add(LastStatusChangeStartTime, DateTime.SpecifyKind(lastStatusChangeStartDate, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(LastStatusChangeEndTime, DateTime.SpecifyKind(lastStatusChangeEndDate, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(AntivirusLastThreatStatusId, (int)threatStatus);
        command.Parameters.Add(Skip, skip);
        command.Parameters.Add(Take, take);
        command.Parameters.Add(SortOrder, isDescendingOrder ? "DESC" : "ASC");
        command.Parameters.Add(TotalRecords, out Ref<int> total);
        var result = command.ExecuteReader(ReadGroupThreatStatusData);
        totalCount = total;
        return result;
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusThreatStatusDeviceData> GetDeviceDataByAntivirusThreatStatus(
        IEnumerable<int> deviceGroupIds,
        DeviceFamily deviceFamily,
        DateTime lastStatusChangeStartDate,
        DateTime lastStatusChangeEndDate,
        AntivirusThreatStatus threatStatus)
    {
        ArgumentNullException.ThrowIfNull(deviceGroupIds);
        var groupIds = deviceGroupIds as int[] ?? deviceGroupIds.ToArray();
        if (groupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(deviceGroupIds));
        }

        deviceFamily.EnsureDefined();
        threatStatus.EnsureDefined();
        ValidateStartAndEndTime(lastStatusChangeStartDate, lastStatusChangeEndDate);

        var command = _database.StoredProcedures[GetLastStatusChangeDates];
        command.Parameters.Add(GroupIds, CreateIntIdentifierTable(groupIds), SqlDbType.Structured);
        command.Parameters.Add(DeviceFamilyId, (int)deviceFamily);
        command.Parameters.Add(LastStatusChangeStartTime, DateTime.SpecifyKind(lastStatusChangeStartDate, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(LastStatusChangeEndTime, DateTime.SpecifyKind(lastStatusChangeEndDate, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(AntivirusLastThreatStatusId, (byte)threatStatus);
        return command.ExecuteReader(ReadGroupThreatStatusData);
    }

    /// <inheritdoc />
    public IDictionary<byte, int> GetThreatStatusIdCountForDevice(int deviceId, DateTime lastStatusChangeTimeFrom, DateTime lastStatusChangeTimeTo)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        ValidateStartAndEndTime(lastStatusChangeTimeFrom, lastStatusChangeTimeTo);

        var command = _database.StoredProcedures[GetThreatStatusIdCountByDeviceId];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(LastStatusChangeStartTime, DateTime.SpecifyKind(lastStatusChangeTimeFrom, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(LastStatusChangeEndTime, DateTime.SpecifyKind(lastStatusChangeTimeTo, DateTimeKind.Utc), SqlDbType.DateTime2);

        return command.ExecuteReader(ReadWindowsThreatStatusCountData);
    }

    /// <inheritdoc />
    public IDictionary<byte, int> GetAntivirusThreatStatusCount(IEnumerable<int> groupIds, DateTime startTime, DateTime endTime, int deviceFamily)
    {
        ArgumentNullException.ThrowIfNull(groupIds);
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        if (deviceGroupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(groupIds));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceFamily);

        ValidateStartAndEndTime(startTime, endTime);

        var command = _database.StoredProcedures[GetThreatStatusIdCount];
        command.Parameters.Add(DeviceFamily, deviceFamily);
        command.Parameters.Add(GroupIds, CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add(LastStatusChangeStartTime, DateTime.SpecifyKind(startTime, DateTimeKind.Utc), SqlDbType.DateTime);
        command.Parameters.Add(LastStatusChangeEndTime, DateTime.SpecifyKind(endTime, DateTimeKind.Utc), SqlDbType.DateTime);
        return command.ExecuteReader(ReadWindowsThreatStatusCountData);
    }

    /// <inheritdoc />
    public AntivirusThreatAvailabilityData GetAntivirusThreatAvailabilityByDeviceId(int deviceId, AntivirusThreatStatus antivirusThreatStatus)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        antivirusThreatStatus.EnsureDefined();

        var command = _database.StoredProcedures[GetActiveThreatExistsByDeviceId];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(AntivirusLastThreatStatusId, (int)antivirusThreatStatus);
        command.Parameters.Add(DeviceHasLastThreatStatusId, out Ref<bool> isThreatExists);
        command.Parameters.Add(LastAntivirusSyncTime, out Ref<DateTime?> lastAntivirusSyncTime);
        command.ExecuteNonQuery();

        return new AntivirusThreatAvailabilityData
        {
            LastAntivirusSyncTime = lastAntivirusSyncTime,
            IsThreatAvailable = isThreatExists
        };
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusThreatAvailabilityData> GetAntivirusThreatAvailabilityByDeviceIds(IEnumerable<int> deviceIds, AntivirusThreatStatus antivirusThreatStatus)
    {
        var deviceIdentifiers = deviceIds as int[] ?? deviceIds.ToArray();
        if (deviceIdentifiers == null || !deviceIdentifiers.Any())
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        antivirusThreatStatus.EnsureDefined();

        var command = _database.StoredProcedures[GetActiveThreatsExistsByDeviceIds];
        command.Parameters.Add(DeviceIds, CreateIntIdentifierTable(deviceIdentifiers), SqlDbType.Structured);
        command.Parameters.Add(AntivirusLastThreatStatusId, (int)antivirusThreatStatus);

        return command.ExecuteReader(ReadAntivirusThreatAvailabilityRecord);
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusThreatData> GetDeviceGroupsAntivirusThreatHistory(
        IEnumerable<int> groupIds,
        DeviceFamily deviceFamily,
        IEnumerable<byte> threatTypeIds,
        IEnumerable<byte> threatSeverityIds,
        DateTime statusChangeStartTime,
        DateTime statusChangeEndTime,
        int skip,
        int take,
        AntivirusThreatHistorySortByOption sortBy,
        bool order,
        out int total)
    {
        ArgumentNullException.ThrowIfNull(groupIds);
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        if (deviceGroupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(groupIds));
        }

        ArgumentNullException.ThrowIfNull(threatTypeIds);
        var typeIds = threatTypeIds as byte[] ?? threatTypeIds.ToArray();
        if (typeIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(threatTypeIds));
        }

        ArgumentNullException.ThrowIfNull(threatSeverityIds);
        var severityIds = threatSeverityIds as byte[] ?? threatSeverityIds.ToArray();
        if (severityIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(threatSeverityIds));
        }

        deviceFamily.EnsureDefined();
        ValidateStartAndEndTime(statusChangeStartTime, statusChangeEndTime);
        sortBy.EnsureDefined();
        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(skip, take);

        var command = _database.StoredProcedures[GetThreatDetectionHistoryByGroupIds];
        command.Parameters.Add("DeviceGroupIds", CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add("FamilyId", (int)deviceFamily);
        command.Parameters.Add("LastStatusChangeStartTime", DateTime.SpecifyKind(statusChangeStartTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add("LastStatusChangeEndTime", DateTime.SpecifyKind(statusChangeEndTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add("ThreatTypeIds", CreateTinyIntIdentifierTable(typeIds), SqlDbType.Structured);
        command.Parameters.Add("ThreatSeverityIds", CreateTinyIntIdentifierTable(severityIds), SqlDbType.Structured);
        command.Parameters.Add(Skip, skip);
        command.Parameters.Add(Take, take);
        command.Parameters.Add("SortBy", sortBy.ToString());
        command.Parameters.Add("SortOrder", order ? "DESC" : "ASC");
        command.Parameters.Add("IsTotalReturned", 1);
        command.Parameters.Add(TotalRecords, out Ref<int> totalCount);
        var result = command.ExecuteReader(ReadAntivirusThreatCollection);
        total = totalCount;
        return result;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<DeviceThreatDetails> GetDeviceThreatStatusByGroupIdsAndDeviceFamily(
        long threatId,
        IEnumerable<int> groupIds,
        DeviceFamily deviceFamily,
        DateTime statusChangeStartTime,
        DateTime statusChangeEndTime)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(threatId);
        ArgumentNullException.ThrowIfNull(groupIds);
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        if (deviceGroupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(groupIds));
        }

        deviceFamily.EnsureDefined();
        ValidateStartAndEndTime(statusChangeStartTime, statusChangeEndTime);

        var command = _database.StoredProcedures[GetDeviceThreatStatusByThreatId];
        command.Parameters.Add(ExternalAntivirusThreatId, threatId, SqlDbType.BigInt);
        command.Parameters.Add(DeviceGroupIds, CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add(DeviceFamilyId, deviceFamily, SqlDbType.Int);
        command.Parameters.Add(LastStatusChangeStartTime, DateTime.SpecifyKind(statusChangeStartTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(LastStatusChangeEndTime, DateTime.SpecifyKind(statusChangeEndTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        return command.ExecuteReader(ReadDeviceAntiVirusThreatStatusCollection);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<DeviceThreatDetails> GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(
        long threatId,
        IEnumerable<int> groupIds,
        DeviceFamily deviceFamily,
        DateTime statusChangeStartTime,
        DateTime statusChangeEndTime,
        int skip,
        int take,
        AntivirusThreatHistoryDetailsSortByOption sortBy,
        bool isDescendingOrder,
        out int totalCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(threatId);
        ArgumentNullException.ThrowIfNull(groupIds);
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        if (deviceGroupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(groupIds));
        }

        deviceFamily.EnsureDefined();
        sortBy.EnsureDefined();
        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(skip, take);
        ValidateStartAndEndTime(statusChangeStartTime, statusChangeEndTime);

        var command = _database.StoredProcedures[GetDeviceThreatStatusByThreatIdPaginated];
        command.Parameters.Add(ExternalAntivirusThreatId, threatId, SqlDbType.BigInt);
        command.Parameters.Add(DeviceGroupIds, CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add(DeviceFamilyId, deviceFamily, SqlDbType.Int);
        command.Parameters.Add(LastStatusChangeStartTime, DateTime.SpecifyKind(statusChangeStartTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(LastStatusChangeEndTime, DateTime.SpecifyKind(statusChangeEndTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(Skip, skip, SqlDbType.Int);
        command.Parameters.Add(Take, take, SqlDbType.Int);
        command.Parameters.Add(SortBy, sortBy.ToString());
        command.Parameters.Add(SortOrder, isDescendingOrder ? "DESC" : "ASC");
        command.Parameters.Add(TotalRecords, out Ref<int> totalRecords, SqlDbType.Int);
        var deviceThreatDetails = command.ExecuteReader(ReadDeviceAntiVirusThreatStatusCollection);
        totalCount = totalRecords.Value;
        return deviceThreatDetails;
    }

    /// <inheritdoc />
    public IEnumerable<DeviceGroupThreatHistoryData> GetAntivirusGroupThreatHistory(IEnumerable<int> groupIds, DeviceFamily deviceFamily)
    {
        ArgumentNullException.ThrowIfNull(groupIds);
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        if (deviceGroupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(groupIds));
        }

        deviceFamily.EnsureDefined();
        var command = _database.StoredProcedures[GetAntivirusThreatId];
        command.Parameters.Add(DeviceGroupIds, CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add(FamilyId, (int)deviceFamily);
        return command.ExecuteReader(ReadGroupThreatHistoryData);
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusThreatData> GetAllAntivirusThreatHistory(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        var command = _database.StoredProcedures[GetAllAntivirusHistoryByDeviceId];
        command.Parameters.Add(DeviceId, deviceId);

        return command.ExecuteReader(ReadCollection);
    }

    /// <inheritdoc />
    public void DeleteDeviceAntivirusThreatData(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        var command = _database.StoredProcedures[DeleteDeviceAntivirusThreatStatusByFkDeviceId];
        command.Parameters.Add(DeviceId, deviceId);
        command.ExecuteNonQuery();
    }

    private static void ValidateStartAndEndTime(DateTime startTime, DateTime endTime)
    {
        if (startTime > endTime)
        {
            throw new ArgumentException("Start time cannot be later than the end time.");
        }
    }

    private static IEnumerable<AntivirusThreatData> ReadAntivirusThreatCollection(IDataReader reader)
    {
        var results = new List<AntivirusThreatData>();
        var mapping = GetAntivirusThreatColumnMapping(reader);

        while (reader.Read())
        {
            results.Add(ParseAntivirusThreatEntity(reader, mapping));
        }

        return results;
    }

    private static OrdinalColumnMapping GetAntivirusThreatColumnMapping(IDataRecord reader)
    {
        return new OrdinalColumnMapping
        {
            ExternalThreatId = reader.GetOrdinal(ExternalAntivirusThreatId),
            TypeId = reader.GetOrdinal(AntivirusThreatTypeId),
            SeverityId = reader.GetOrdinal(AntivirusThreatSeverityId),
            InitialDetectionTime = reader.GetOrdinal(AntivirusThreatInitialDetectionTime),
            LastStatusChangeTime = reader.GetOrdinal(AntivirusThreatLastStatusChangeTime),
            NoOfDevices = reader.GetOrdinal(NoOfDevices),
        };
    }

    private static AntivirusThreatData ParseAntivirusThreatEntity(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new AntivirusThreatData
        {
            ExternalThreatId = reader.GetInt64(mapping.ExternalThreatId),
            TypeId = reader.GetByte(mapping.TypeId),
            SeverityId = reader.GetByte(mapping.SeverityId),
            InitialDetectionTime = reader.GetDateTime(mapping.InitialDetectionTime),
            LastStatusChangeTime = reader.GetDateTime(mapping.LastStatusChangeTime),
            NoOfDevices = reader.GetInt32(mapping.NoOfDevices)
        };
    }

    private static IReadOnlyCollection<AntivirusThreatData> ReadCollection(IDataReader reader)
    {
        var result = new List<AntivirusThreatData>();
        var mapping = GetColumnMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseEntity(reader, mapping));
        }

        return result;
    }

    private static IReadOnlyCollection<AntivirusThreatStatusDeviceData> ReadGroupThreatStatusData(IDataReader reader)
    {
        var result = new List<AntivirusThreatStatusDeviceData>();
        var mapping = GetColumnMappingWindowsGroupThreatStatusData(reader);
        while (reader.Read())
        {
            result.Add(ParseEntityWindowsGroupThreatStatusData(reader, mapping));
        }

        return result;
    }

    private static IEnumerable<AntivirusThreatAvailabilityData> ReadAntivirusThreatAvailabilityRecord(IDataReader reader)
    {
        var mapping = GetColumnMappingForAntivirusThreatAvailability(reader);
        var result = new List<AntivirusThreatAvailabilityData>();
        while (reader.Read())
        {
            result.Add(ParseAntivirusThreatAvailabilityData(reader, mapping));
        }

        return result;
    }

    private static IDictionary<byte, int> ReadWindowsThreatStatusCountData(IDataReader reader)
    {
        var result = new Dictionary<byte, int>();
        var mapping = GetColumnMappingWindowsThreatStatusCount(reader);
        while (reader.Read())
        {
            var countByThreatStatus = ParseEntityWindowsThreatStatusCount(reader, mapping);
            if (countByThreatStatus.AntivirusLastThreatStatusId != null && countByThreatStatus.DeviceCount != null)
            {
                result.Add((byte)countByThreatStatus.AntivirusLastThreatStatusId, (int)countByThreatStatus.DeviceCount);
            }
        }

        return result;
    }

    private IReadOnlyCollection<DeviceThreatDetails> ReadDeviceAntiVirusThreatStatusCollection(IDataReader reader)
    {
        var result = new List<DeviceThreatDetails>();
        var mapping = GetColumnMappingForDeviceAntivirusThreatStatus(reader);
        while (reader.Read())
        {
            result.Add(ParseDeviceAntivirusThreatStatus(reader, mapping));
        }

        return result;
    }

    private static IReadOnlyCollection<DeviceGroupThreatHistoryData> ReadGroupThreatHistoryData(IDataReader reader)
    {
        var result = new List<DeviceGroupThreatHistoryData>();
        var mapping = GetColumnMappingWindowsGroupThreatHistoryData(reader);
        while (reader.Read())
        {
            result.Add(ParseEntityWindowsGroupThreatHistoryData(reader, mapping));
        }

        return result;
    }

    private static OrdinalColumnMappingWindowsGroupThreatStatusData GetColumnMappingWindowsGroupThreatStatusData(IDataRecord record)
    {
        return new OrdinalColumnMappingWindowsGroupThreatStatusData
        {
            DevId = record.GetOrdinal(DevId),
            LastStatusChangeTime = record.GetOrdinal(AntivirusThreatLastStatusChangeTime),
            ExternalAntivirusThreatId = record.GetOrdinal(ExternalAntivirusThreatId),
            AntivirusThreatTypeId = record.GetOrdinal(AntivirusThreatTypeId),
            AntivirusThreatSeverityId = record.GetOrdinal(AntivirusThreatSeverityId),
        };
    }

    private static OrdinalColumnMappingWindowsThreatStatusCount GetColumnMappingWindowsThreatStatusCount(IDataRecord record)
    {
        return new OrdinalColumnMappingWindowsThreatStatusCount
        {
            AntivirusLastThreatStatusId = record.GetOrdinal(AntivirusLastThreatStatusId),
            ThreatStatusCount = record.GetOrdinal(ThreatStatusCount)
        };
    }

    private static OrdinalColumnMappingAntivirusThreatAvailabilityData GetColumnMappingForAntivirusThreatAvailability(IDataRecord record)
    {
        return new OrdinalColumnMappingAntivirusThreatAvailabilityData()
        {
            DeviceHasLastThreatStatusId = record.GetOrdinal(DeviceHasLastThreatStatusId),
            LastAntivirusSyncTime = record.GetOrdinal(LastAntivirusSyncTime)
        };
    }

    private static OrdinalColumnMappingDeviceAntivirusThreatStatus GetColumnMappingForDeviceAntivirusThreatStatus(IDataRecord record)
    {
        return new OrdinalColumnMappingDeviceAntivirusThreatStatus
        {
            DeviceId = record.GetOrdinal(DevId),
            AntivirusLastThreatStatusId = record.GetOrdinal(AntivirusLastThreatStatusId),
            AntivirusThreatInitialDetectionTime = record.GetOrdinal(AntivirusThreatInitialDetectionTime),
            AntivirusThreatLastStatusChangeTime = record.GetOrdinal(AntivirusThreatLastStatusChangeTime)
        };
    }

    private static OrdinalColumnMappingWindowsGroupThreatHistoryData GetColumnMappingWindowsGroupThreatHistoryData(IDataRecord record)
    {
        return new OrdinalColumnMappingWindowsGroupThreatHistoryData
        {
            ExternalAntivirusThreatId = record.GetOrdinal(ExternalAntivirusThreatId),
            AntivirusThreatTypeId = record.GetOrdinal(AntivirusThreatTypeId),
            AntivirusThreatSeverityId = record.GetOrdinal(AntivirusThreatSeverityId),
            InitialDetectionTime = record.GetOrdinal(InitialDetectionTime),
            LastStatusChangeTime = record.GetOrdinal(LastStatusChangeTime),
            NumberOfDevices = record.GetOrdinal(NumberOfDevices),
        };
    }

    private static ThreatStatusCount ParseEntityWindowsThreatStatusCount(IDataRecord record, OrdinalColumnMappingWindowsThreatStatusCount mapping)
    {
        return new ThreatStatusCount
        {
            AntivirusLastThreatStatusId = record.GetByte(mapping.AntivirusLastThreatStatusId),
            DeviceCount = record.GetInt32(mapping.ThreatStatusCount)
        };
    }

    private static AntivirusThreatStatusDeviceData ParseEntityWindowsGroupThreatStatusData(IDataRecord record, OrdinalColumnMappingWindowsGroupThreatStatusData mapping)
    {
        return new AntivirusThreatStatusDeviceData
        {
            DevId = record.GetString(mapping.DevId).Trim(),
            LastStatusChangeTime = record.GetDateTime(mapping.LastStatusChangeTime),
            ExternalThreatId = record.GetInt64(mapping.ExternalAntivirusThreatId),
            TypeId = record.GetByte(mapping.AntivirusThreatTypeId),
            SeverityId = !record.IsDBNull(mapping.AntivirusThreatSeverityId) ? record.GetByte(mapping.AntivirusThreatSeverityId) : null,
        };
    }

    private static AntivirusThreatAvailabilityData ParseAntivirusThreatAvailabilityData(IDataRecord record, OrdinalColumnMappingAntivirusThreatAvailabilityData mapping)
    {
        return new AntivirusThreatAvailabilityData
        {
            DeviceId = record.GetInt32(mapping.DeviceId),
            IsThreatAvailable = record.GetInt32(mapping.DeviceHasLastThreatStatusId) == 1,
            LastAntivirusSyncTime = record.IsDBNull(mapping.LastAntivirusSyncTime) ? null : record.GetDateTime(mapping.LastAntivirusSyncTime)
        };
    }

    private static DeviceThreatDetails ParseDeviceAntivirusThreatStatus(IDataRecord record, OrdinalColumnMappingDeviceAntivirusThreatStatus mapping)
    {
        return new DeviceThreatDetails
        {
            DevId = record.GetString(mapping.DeviceId).Trim(),
            ThreatStatus = record.GetByte(mapping.AntivirusLastThreatStatusId).ToEnum<AntivirusThreatStatus>(),
            InitialDetectionTime = record.GetDateTime(mapping.AntivirusThreatInitialDetectionTime),
            LastStatusChangeTime = record.GetDateTime(mapping.AntivirusThreatLastStatusChangeTime),
        };
    }

    private static DeviceGroupThreatHistoryData ParseEntityWindowsGroupThreatHistoryData(IDataRecord reader, OrdinalColumnMappingWindowsGroupThreatHistoryData mapping)
    {
        return new DeviceGroupThreatHistoryData
        {
            ExternalThreatId = reader.GetInt64(mapping.ExternalAntivirusThreatId),
            TypeId = reader.GetByte(mapping.AntivirusThreatTypeId),
            SeverityId = !reader.IsDBNull(mapping.AntivirusThreatSeverityId) ? reader.GetByte(mapping.AntivirusThreatSeverityId) : null,
            InitialDetectionTime = reader.GetDateTime(mapping.InitialDetectionTime),
            LastStatusChangeTime = !reader.IsDBNull(mapping.LastStatusChangeTime) ? reader.GetDateTime(mapping.LastStatusChangeTime) : null,
            NoOfDevices = reader.GetInt32(mapping.NumberOfDevices),
        };
    }

    private static OrdinalColumnMapping GetColumnMapping(IDataRecord reader)
    {
        return new OrdinalColumnMapping
        {
            ExternalThreatId = reader.GetOrdinal(ExternalAntivirusThreatId),
            TypeId = reader.GetOrdinal(AntivirusThreatTypeId),
            ThreatName = reader.GetOrdinal(AntivirusThreatName),
            InitialDetectionTime = reader.GetOrdinal(AntivirusThreatInitialDetectionTime),
            LastStatusChangeTime = reader.GetOrdinal(AntivirusThreatLastStatusChangeTime),
            LastThreatStatusId = reader.GetOrdinal(AntivirusLastThreatStatusId),
            CurrentDetectionCount = reader.GetOrdinal(AntivirusCurrentDetectionCount),
            SeverityId = reader.GetOrdinal(AntivirusThreatSeverityId)
        };
    }

    private static AntivirusThreatData ParseEntity(IDataRecord reader, OrdinalColumnMapping mapping)
    {
        return new AntivirusThreatData
        {
            ExternalThreatId = reader.GetInt64(mapping.ExternalThreatId),
            ThreatName = reader.GetString(mapping.ThreatName),
            InitialDetectionTime = reader.GetDateTime(mapping.InitialDetectionTime),
            LastStatusChangeTime = reader.GetDateTime(mapping.LastStatusChangeTime),
            CurrentDetectionCount = reader.GetInt32(mapping.CurrentDetectionCount),
            TypeId = reader.GetByte(mapping.TypeId),
            LastThreatStatusId = reader.GetByte(mapping.LastThreatStatusId),
            SeverityId = reader.GetByte(mapping.SeverityId)
        };
    }

    private static DataTable CreateTinyIntIdentifierTable(IEnumerable<byte> values)
    {
        var table = new DataTable(TinyIntIdentifier);
        table.Columns.Add(Id, typeof(byte));

        foreach (var value in values)
        {
            table.Rows.Add(value);
        }

        return table;
    }

    private static DataTable CreateIntIdentifierTable(IEnumerable<int> values)
    {
        var table = new DataTable(NumericIdentifier) { Locale = CultureInfo.InvariantCulture };
        table.Columns.Add(Id, typeof(int));

        foreach (var value in values)
        {
            table.Rows.Add(value);
        }

        return table;
    }

    /// <summary>
    ///  Internal struct that caches column name-ordinal mapping.
    /// </summary>
    private sealed class OrdinalColumnMapping
    {
        public int ExternalThreatId { get; set; }

        public int TypeId { get; set; }

        public int ThreatName { get; set; }

        public int SeverityId { get; set; }

        public int InitialDetectionTime { get; set; }

        public int LastStatusChangeTime { get; set; }

        public int CurrentDetectionCount { get; set; }

        public int LastThreatStatusId { get; set; }

        public int NoOfDevices { get; set; }
    }

    /// <summary>
    ///  Internal struct that caches column name-ordinal mapping.
    /// </summary>
    private sealed class OrdinalColumnMappingWindowsGroupThreatStatusData
    {
        public int DevId { get; internal set; }

        public int LastStatusChangeTime { get; internal set; }

        public int ExternalAntivirusThreatId { get; set; }

        public int AntivirusThreatTypeId { get; internal set; }

        public int AntivirusThreatSeverityId { get; internal set; }
    }

    /// <summary>
    ///  Internal struct that caches column name-ordinal mapping.
    /// </summary>
    private sealed class OrdinalColumnMappingWindowsThreatStatusCount
    {
        public int AntivirusLastThreatStatusId { get; internal set; }

        public int ThreatStatusCount { get; internal set; }
    }

    /// <summary>
    ///  Internal struct that caches column name-ordinal mapping.
    /// </summary>
    private sealed class OrdinalColumnMappingAntivirusThreatAvailabilityData
    {
        public int DeviceId { get; internal set; }

        public int LastAntivirusSyncTime { get; internal set; }

        public int DeviceHasLastThreatStatusId { get; internal set; }
    }

    /// <summary>
    ///  Internal struct that caches column name-ordinal mapping.
    /// </summary>
    private sealed class OrdinalColumnMappingWindowsGroupThreatHistoryData
    {
        public int ExternalAntivirusThreatId { get; set; }

        public int AntivirusThreatTypeId { get; internal set; }

        public int AntivirusThreatSeverityId { get; internal set; }

        public int InitialDetectionTime { get; internal set; }

        public int LastStatusChangeTime { get; internal set; }

        public int NumberOfDevices { get; internal set; }
    }

    private sealed class OrdinalColumnMappingDeviceAntivirusThreatStatus
    {
        public int DeviceId;
        public int AntivirusLastThreatStatusId;
        public int AntivirusThreatInitialDetectionTime;
        public int AntivirusThreatLastStatusChangeTime;
    }
}
