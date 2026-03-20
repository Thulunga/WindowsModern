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
using Soti.Utilities.Collections;
using Soti.Utilities.Extensions;
using static Soti.MobiControl.WindowsModern.Implementation.WindowsDeviceHelperMethods;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;

internal sealed class WindowsDeviceDataProvider : IWindowsDeviceDataProvider
{
    private const string NumericIdentifierUdt = "[dbo].[GEN_UDT_NumericIdentifier]";
    private const string WindowsDeviceFindIsLocked = "[dbo].[WindowsDevice_FindIsLocked]";
    private const string WindowsDeviceBulkFindIsLocked = "[dbo].[WindowsDevice_BulkFindIsLocked]";
    private const string WindowsDeviceInsert = "[dbo].[GEN_WindowsDevice_Insert]";
    private const string WindowsDeviceUpdate = "[dbo].[GEN_WindowsDevice_Update]";
    private const string WindowsDeviceBulkUpdate = "[dbo].[WindowsDevice_BulkSetIsLocked]";
    private const string WindowsDeviceGet = "[dbo].[GEN_WindowsDevice_Get]";
    private const string WindowsDeviceGetAll = "[dbo].[GEN_WindowsDevice_GetAll]";
    private const string SetIsSandBoxEnabled = "[dbo].[WindowsDevice_SetIsSandBoxEnabled]";
    private const string WindowsDeviceBulkGetIsSandBoxEnabled = "[dbo].[WindowsDevice_BulkGetIsSandBoxEnabled]";
    private const string WindowsDeviceUpdateHardwareId = "[dbo].[WindowsDevice_UpdateHardwareId]";
    private const string WindowsDeviceUpdateLastCheckInDeviceUserTime = "[dbo].[WindowsDevice_UpdateLastCheckInDeviceUserTime]";
    private const string WindowsDeviceUpdateDefenderScanInfo = "[dbo].[WindowsDevice_UpdateLastScanTimes]";
    private const string WindowsDeviceBulkGet = "[dbo].[GEN_WindowsDevice_BulkGet]";
    private const string WindowsDeviceUpdateBiosPasswordStatusId = "[dbo].[WindowsDevice_UpdateBIOSPasswordStatusId]";
    private const string WindowsDeviceBulkInsert = "[dbo].[WindowsDevice_BulkInsert]";
    private const string GetAntivirusScanCountsByGroupIds = "dbo.WindowsDevice_GetAntivirusScanCounts_ByPredefinedRanges";
    private const string GetCustomAntivirusScanCountsByGroupIds = "dbo.WindowsDevice_GetAntivirusScanCounts_ByLastScannedDatesRange";
    private const string GetLastAntivirusScanDates = "[dbo].[WindowsDevice_GetLastAntivirusScanDates]";
    private const string GetDeviceLastFullScanDateTimePaginated = "[dbo].[WindowsDevice_GetDeviceLastFullScanDateTimePaginated_ByDeviceGroupIds]";
    private const string GetDeviceLastFullScanDateTime = "[dbo].[WindowsDevice_GetDeviceLastFullScanDateTime_ByDeviceGroupIds]";
    private const string GetDeviceLastQuickScanDateTime = "[dbo].[WindowsDevice_GetDeviceLastQuickScanDateTime_ByDeviceGroupIds]";
    private const string GetDeviceLastQuickScanDateTimePaginated = "[dbo].[WindowsDevice_GetDeviceLastQuickScanDateTimePaginated_ByDeviceGroupIds]";

    private const string Id = "Id";
    private const string DeviceId = "DeviceId";
    private const string DeviceIds = "DeviceIds";
    private const string DataKeyId = "DataKeyId";
    private const string DataKeyIdHasValue = "DataKeyIdHasValue";
    private const string IsLocked = "IsLocked";
    private const string IsSandBoxEnabled = "IsSandBoxEnabled";
    private const string IsLockedHasValue = "IsLockedHasValue";
    private const string Passcode = "Passcode";
    private const string PasscodeHasValue = "PasscodeHasValue";
    private const string LastCheckInDeviceUserTime = "LastCheckInDeviceUserTime";
    private const string HardwareId = "HardwareId";
    private const string WifiSubnet = "WifiSubnet";
    private const string WifiSubnetHasValue = "WifiSubnetHasValue";
    private const string WifiModeId = "WifiModeId";
    private const string WifiModeIdHasValue = "WifiModeIdHasValue";
    private const string BiosPasswordStatusIdHasValue = "BIOSPasswordStatusIdHasValue";
    private const string NetworkAuthenticationId = "NetworkAuthenticationId";
    private const string NetworkAuthenticationIdHasValue = "NetworkAuthenticationIdHasValue";
    private const string WirelessLanModeId = "WirelessLanModeId";
    private const string WirelessLanModeIdHasValue = "WirelessLanModeIdHasValue";
    private const string IsSandBoxEnabledHasValue = "IsSandBoxEnabledHasValue";
    private const string HardwareIdHasValue = "HardwareIdHasValue";
    private const string AntivirusLastQuickScanTime = "AntivirusLastQuickScanTime";
    private const string AntivirusLastFullScanTime = "AntivirusLastFullScanTime";
    private const string AntivirusLastSyncTime = "LastAntivirusSyncTime";
    private const string LastQuickScanTime = "LastQuickScanTime";
    private const string LastFullScanTime = "LastFullScanTime";
    private const string LastSyncTime = "LastSyncTime";
    private const string OsImageId = "OsImageId";
    private const string OsImageDeployedTime = "OsImageDeployedTime";
    private const string BiosPasswordStatusId = "BIOSPasswordStatusId";
    private const string Filter = "Filter";
    private const string DevId = "DevId";
    private const string GroupIds = "DeviceGroupIds";
    private const string FamilyId = "FamilyId";
    private const string SyncTime = "SyncTime";
    private const string DeviceFamily = "DeviceFamilyId";
    private const string LastScannedStartDate = "LastScannedStartDate";
    private const string LastScannedEndDate = "LastScannedEndDate";
    private const string NumericIdentifier = "GEN_UDT_NumericIdentifier";
    private const string AntivirusLastFullScanTimeFrom = "AntivirusLastFullScanTimeFrom";
    private const string AntivirusLastFullScanTimeTo = "AntivirusLastFullScanTimeTo";
    private const string QuickScanCountWithin24Hours = "LastQuickScanCountWithin24Hours";
    private const string FullScanCountWithin24Hours = "LastFullScanCountWithin24Hours";
    private const string QuickScanCountWithin7Days = "LastQuickScanCountWithin7Days";
    private const string FullScanCountWithin7Days = "LastFullScanCountWithin7Days";
    private const string QuickScanCountMoreThan30Days = "LastQuickScanCountMoreThan30Days";
    private const string FullScanCountMoreThan30Days = "LastFullScanCountMoreThan30Days";
    private const string CustomAntivirusLastQuickScanCount = "AntivirusLastQuickScanCount";
    private const string CustomAntivirusLastFullScanCount = "AntivirusLastFullScanCount";
    private const string LastAntivirusSyncTime = "LastAntivirusSyncTime";
    private const string IsThreatAvailable = "IsThreatAvailable";
    private const string ActiveThreatAvailable = "IsActiveThreatAvailable";
    private const string Skip = "SkipRecords";
    private const string Take = "TakeRecords";
    private const string SortOrder = "SortOrder";
    private const string TotalRecords = "TotalRecords";
    private const string AntivirusLastQuickScanTimeFrom = "AntivirusLastQuickScanTimeFrom";
    private const string AntivirusLastQuickScanTimeTo = "AntivirusLastQuickScanTimeTo";

    private static OrdinalColumnMapping _ordinalColumnMapping;
    private static OrdinalColumnMapping _ordinalColumnMappingNetworkDetails;
    private static OrdinalColumnMapping _ordinalColumnMappingDeviceLocked;
    private static OrdinalColumnMapping _ordinalColumnMappingSandBoxEnabled;

    private readonly IDatabase _database;

    /// <summary>
    /// Initializes a new instance of the WindowsDeviceDataProvider class.
    /// </summary>
    /// <param name="database">the instance of IDatabase.</param>
    public WindowsDeviceDataProvider(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <inheritdoc />
    public bool IsDeviceLocked(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[WindowsDeviceFindIsLocked];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(IsLocked, out Ref<bool> isLocked);
        command.ExecuteNonQuery();
        return isLocked.Value;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<int, bool> AreDevicesLocked(HashSet<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        var dataTable = new DataTable(NumericIdentifierUdt);
        dataTable.Columns.AddRange(new[] { new DataColumn(Id, typeof(int)) { AllowDBNull = false } });

        foreach (var deviceId in deviceIds)
        {
            dataTable.Rows.Add(deviceId);
        }

        var command = _database.StoredProcedures[WindowsDeviceBulkFindIsLocked];
        command.Parameters.Add(DeviceIds, dataTable, SqlDbType.Structured);
        return command.ExecuteReader(ReadAreDevicesLockedCollection);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<int, bool> GetSandBoxStatusByIds(IEnumerable<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        var dataTable = new DataTable(NumericIdentifierUdt);
        dataTable.Columns.AddRange(new[] { new DataColumn(Id, typeof(int)) { AllowDBNull = false } });

        foreach (var deviceId in deviceIds)
        {
            dataTable.Rows.Add(deviceId);
        }

        var command = _database.StoredProcedures[WindowsDeviceBulkGetIsSandBoxEnabled];
        command.Parameters.Add(DeviceIds, dataTable, SqlDbType.Structured);
        return command.ExecuteReader(ReadIsSandBoxEnabledByIdsCollection);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<int, WindowsDeviceData> BulkGetDeviceDetails(IEnumerable<int> deviceIds)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        var deviceIdsArray = deviceIds.ToArray();
        if (deviceIdsArray.Length == 0)
        {
            return new Dictionary<int, WindowsDeviceData>();
        }

        var dataTable = new DataTable(NumericIdentifierUdt) { Locale = CultureInfo.InvariantCulture };
        dataTable.Columns.AddRange(new[] { new DataColumn(Id, typeof(int)) { AllowDBNull = false } });

        foreach (var deviceId in deviceIdsArray)
        {
            dataTable.Rows.Add(deviceId);
        }

        var command = _database.StoredProcedures[WindowsDeviceBulkGet];
        command.Parameters.Add(Filter, dataTable, SqlDbType.Structured);
        return command.ExecuteReader(ReadDeviceDetailsByIdsCollection);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<WindowsDeviceData> GetAll()
    {
        var command = _database.StoredProcedures[WindowsDeviceGetAll];

        return command.ExecuteReader(ReadWindowsDeviceCollection);
    }

    /// <inheritdoc />
    public void Update(WindowsDeviceData value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var command = _database.StoredProcedures[WindowsDeviceUpdate];

        command.Parameters.Add(DeviceId, value.DeviceId);
        command.Parameters.Add(IsLocked, value.IsLocked);
        command.Parameters.Add(IsLockedHasValue, true);
        command.Parameters.Add(Passcode, value.Passcode);
        command.Parameters.Add(PasscodeHasValue, true);
        command.Parameters.Add(DataKeyId, value.DataKeyId);
        command.Parameters.Add(DataKeyIdHasValue, true);
        command.Parameters.Add(BiosPasswordStatusId, (byte)value.BiosPasswordStatusId);
        command.Parameters.Add(BiosPasswordStatusIdHasValue, true);

        command.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public void UpdateBiosPasswordStatusId(int deviceId, byte biosPasswordStatusId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (!Enum.IsDefined(typeof(BiosPasswordStatusType), (int)biosPasswordStatusId))
        {
            throw new ArgumentOutOfRangeException(nameof(biosPasswordStatusId), "Invalid BIOS password status ID.");
        }

        var command = _database.StoredProcedures[WindowsDeviceUpdateBiosPasswordStatusId];

        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(BiosPasswordStatusId, biosPasswordStatusId);

        command.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public void UpdateWindowsSandBoxStatus(int deviceId, bool isSandBoxEnabled)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[SetIsSandBoxEnabled];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(IsSandBoxEnabled, isSandBoxEnabled);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public void BulkUpdate(IEnumerable<int> deviceIds, bool isLocked, byte[] passcode, int? dataKeyId)
    {
        if (deviceIds == null)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        var dataTable = new DataTable(NumericIdentifierUdt);
        dataTable.Columns.AddRange(new[] { new DataColumn(Id, typeof(int)) { AllowDBNull = false } });

        foreach (var deviceId in deviceIds)
        {
            dataTable.Rows.Add(deviceId);
        }

        var command = _database.StoredProcedures[WindowsDeviceBulkUpdate];
        command.Parameters.Add(DeviceIds, dataTable, SqlDbType.Structured);
        command.Parameters.Add(IsLocked, isLocked);
        command.Parameters.Add(Passcode, passcode);
        command.Parameters.Add(DataKeyId, dataKeyId);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public void Insert(WindowsDeviceData value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var command = _database.StoredProcedures[WindowsDeviceInsert];

        command.Parameters.Add(DeviceId, value.DeviceId);
        command.Parameters.Add(IsLocked, value.IsLocked);
        command.Parameters.Add(Passcode, value.Passcode);
        command.Parameters.Add(DataKeyId, value.DataKeyId);
        command.Parameters.Add(LastCheckInDeviceUserTime, value.LastCheckInDeviceUserTime);
        command.Parameters.Add(HardwareId, value.HardwareId);
        command.Parameters.Add(OsImageId, value.OsImageId);
        command.Parameters.Add(OsImageDeployedTime, value.OsImageDeploymentTime);

        command.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public void BulkInsertWindowsDevices(IEnumerable<WindowsDeviceData> windowsDevices)
    {
        windowsDevices.SwitchByCount(
            Insert,
            windowsDeviceList =>
            {
                var dataTable = CreateDataTableBulkInsertWindowsDevices(windowsDeviceList);
                var command = _database.StoredProcedures[WindowsDeviceBulkInsert];
                command.Parameters.Add("WindowsDevices", dataTable, SqlDbType.Structured);
                command.ExecuteNonQuery();
            }, () => throw new ArgumentNullException(nameof(windowsDevices)));
    }

    /// <inheritdoc />
    public WindowsDeviceData Get(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[WindowsDeviceGet];
        command.Parameters.Add(DeviceId, deviceId);

        return command.ExecuteReader(ReadWindowsDeviceDetailsRecord);
    }

    /// <inheritdoc />
    public void UpdateLastCheckInDeviceUserTime(int deviceId, DateTime lastCheckInTime)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[WindowsDeviceUpdateLastCheckInDeviceUserTime];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(LastCheckInDeviceUserTime, lastCheckInTime);
        command.ExecuteNonQuery();
    }

    /// <inheritdoc/>
    public void UpdateHardwareId(int deviceId, string hardwareId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(hardwareId))
        {
            throw new ArgumentException(nameof(hardwareId));
        }

        var command = _database.StoredProcedures[WindowsDeviceUpdateHardwareId];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(HardwareId, hardwareId);
        command.ExecuteNonQuery();
    }

    public void UpdateHardwareIdWifiSubnet(int deviceId, string wifiSubnet, string hardwareId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[WindowsDeviceUpdate];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(WifiSubnet, wifiSubnet);
        command.Parameters.Add(WifiSubnetHasValue, true);
        command.Parameters.Add(HardwareId, hardwareId);
        command.Parameters.Add(HardwareIdHasValue, true);

        command.ExecuteNonQuery();
    }

    public void UpdateWindowsDeviceDetails(WindowsModernSnapshot windowsModernSnapshot)
    {
        ArgumentNullException.ThrowIfNull(windowsModernSnapshot);

        if (windowsModernSnapshot.DeviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowsModernSnapshot.DeviceId));
        }

        ValidateNetworkDetailsEnum(windowsModernSnapshot);

        var command = _database.StoredProcedures[WindowsDeviceUpdate];
        command.Parameters.Add(DeviceId, windowsModernSnapshot.DeviceId);
        command.Parameters.Add(WifiModeId, (byte)windowsModernSnapshot.WifiModeId);
        command.Parameters.Add(WifiModeIdHasValue, true);
        command.Parameters.Add(NetworkAuthenticationId, (byte)windowsModernSnapshot.NetworkAuthenticationId);
        command.Parameters.Add(NetworkAuthenticationIdHasValue, true);
        command.Parameters.Add(WirelessLanModeId, (byte)windowsModernSnapshot.WirelessLanModeId);
        command.Parameters.Add(WirelessLanModeIdHasValue, true);
        command.Parameters.Add(IsSandBoxEnabled, windowsModernSnapshot.WindowsSandBoxEnabledStatus);
        command.Parameters.Add(IsSandBoxEnabledHasValue, true);

        command.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public void UpdateDefenderScanInfo(int deviceId, DateTime? quickScanTime, DateTime? fullScanTime, DateTime? syncTime)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[WindowsDeviceUpdateDefenderScanInfo];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add(LastQuickScanTime, quickScanTime == null ? (DateTime?)null : DateTime.SpecifyKind(quickScanTime.Value, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(LastFullScanTime, fullScanTime == null ? (DateTime?)null : DateTime.SpecifyKind(fullScanTime.Value, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(LastSyncTime, syncTime == null ? (DateTime?)null : DateTime.SpecifyKind(syncTime.Value, DateTimeKind.Utc), SqlDbType.DateTime2);

        command.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public void UpdateOsImageInfo(int deviceId, int? osImageId, DateTime? deployedTime)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var command = _database.StoredProcedures[WindowsDeviceUpdate];
        command.Parameters.Add(DeviceId, deviceId);
        command.Parameters.Add("OsImageId", osImageId);
        command.Parameters.Add("OsImageIdHasValue", osImageId.HasValue);
        command.Parameters.Add("OsImageDeployedTime", deployedTime);
        command.Parameters.Add("OsImageDeployedTimeHasValue", deployedTime.HasValue);

        command.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public AntivirusGroupScanSummary GetDeviceGroupsDefaultAntivirusScanSummary(IEnumerable<int> groupIds, DeviceFamily deviceFamily, DateTime lastSyncedTime)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIdsAndFamilyId(deviceGroupIds, deviceFamily);

        var command = _database.StoredProcedures[GetAntivirusScanCountsByGroupIds];
        command.Parameters.Add(GroupIds, CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add(FamilyId, (int)deviceFamily);
        command.Parameters.Add(SyncTime, DateTime.SpecifyKind(lastSyncedTime, DateTimeKind.Utc), SqlDbType.Date);
        return command.ExecuteReader(ReadAntivirusGroupScanSummary);
    }

    /// <inheritdoc />
    public AntivirusGroupScanSummary GetDeviceGroupsCustomAntivirusScanSummary(IEnumerable<int> groupIds, DeviceFamily deviceFamily, DateTime lastScannedStartTime, DateTime lastScannedEndTime)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        ValidateGroupIdsAndFamilyId(deviceGroupIds, deviceFamily);
        ValidateStartAndEndTime(lastScannedStartTime, lastScannedEndTime);

        var command = _database.StoredProcedures[GetCustomAntivirusScanCountsByGroupIds];
        command.Parameters.Add(GroupIds, CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add(FamilyId, (int)deviceFamily);
        command.Parameters.Add(LastScannedStartDate, DateTime.SpecifyKind(lastScannedStartTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(LastScannedEndDate, DateTime.SpecifyKind(lastScannedEndTime, DateTimeKind.Utc), SqlDbType.DateTime2);

        return command.ExecuteReader(ReadCustomAntivirusGroupScanSummary);
    }

    /// <inheritdoc />
    public AntivirusScanTimeData GetAntivirusScanTimeData(int deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId);

        var command = _database.StoredProcedures[GetLastAntivirusScanDates];
        command.Parameters.Add(DeviceId, deviceId);

        return command.ExecuteRecord(ReadAntivirusScanTimeDataRecord);
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusLastScanDeviceData> GetAntivirusLastFullScanSummary(
        IEnumerable<int> groupIds,
        DateTime startTime,
        DateTime endTime,
        DeviceFamily deviceFamily,
        int skip,
        int take,
        bool order,
        out int totalCount)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        if (deviceGroupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(groupIds));
        }

        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(skip, take);
        deviceFamily.EnsureDefined();
        ValidateStartAndEndTime(startTime, endTime);

        var command = _database.StoredProcedures[GetDeviceLastFullScanDateTimePaginated];
        command.Parameters.Add(DeviceFamily, (int)deviceFamily);
        command.Parameters.Add(GroupIds, CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add(AntivirusLastFullScanTimeFrom, DateTime.SpecifyKind(startTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(AntivirusLastFullScanTimeTo, DateTime.SpecifyKind(endTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(Skip, skip);
        command.Parameters.Add(Take, take);
        command.Parameters.Add(SortOrder, order ? "DESC" : "ASC");
        command.Parameters.Add(TotalRecords, out Ref<int> total);
        var result = command.ExecuteReader(ReadAntivirusLastFullScanSummary);
        totalCount = total;
        return result;
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusLastScanDeviceData> GetAntivirusLastFullScanSummary(
        IEnumerable<int> groupIds,
        DateTime startTime,
        DateTime endTime,
        DeviceFamily deviceFamily)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        if (deviceGroupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(groupIds));
        }

        deviceFamily.EnsureDefined();
        ValidateStartAndEndTime(startTime, endTime);

        var command = _database.StoredProcedures[GetDeviceLastFullScanDateTime];
        command.Parameters.Add(DeviceFamily, (int)deviceFamily);
        command.Parameters.Add(GroupIds, CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add(AntivirusLastFullScanTimeFrom, DateTime.SpecifyKind(startTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(AntivirusLastFullScanTimeTo, DateTime.SpecifyKind(endTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        return command.ExecuteReader(ReadAntivirusLastFullScanSummary);
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusLastScanDeviceData> GetAntivirusLastQuickScanSummary(
        IEnumerable<int> groupIds,
        DateTime startTime,
        DateTime endTime,
        DeviceFamily deviceFamily,
        int skip,
        int take,
        bool order,
        out int totalCount)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        if (deviceGroupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(groupIds));
        }

        WindowsDeviceHelperMethods.ValidateDataRetrievalOptions(skip, take);
        deviceFamily.EnsureDefined();
        ValidateStartAndEndTime(startTime, endTime);

        var command = _database.StoredProcedures[GetDeviceLastQuickScanDateTimePaginated];
        command.Parameters.Add(DeviceFamily, (int)deviceFamily);
        command.Parameters.Add(GroupIds, CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add(AntivirusLastQuickScanTimeFrom, DateTime.SpecifyKind(startTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(AntivirusLastQuickScanTimeTo, DateTime.SpecifyKind(endTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(Skip, skip);
        command.Parameters.Add(Take, take);
        command.Parameters.Add(SortOrder, order ? "DESC" : "ASC");
        command.Parameters.Add(TotalRecords, out Ref<int> total);
        var result = command.ExecuteReader(ReadAntivirusLastQuickScanSummary);
        totalCount = total;
        return result;
    }

    /// <inheritdoc />
    public IEnumerable<AntivirusLastScanDeviceData> GetAntivirusLastQuickScanSummary(
        IEnumerable<int> groupIds,
        DateTime startTime,
        DateTime endTime,
        DeviceFamily deviceFamily)
    {
        var deviceGroupIds = groupIds as int[] ?? groupIds.ToArray();
        if (deviceGroupIds.Length == 0)
        {
            throw new ArgumentNullException(nameof(groupIds));
        }

        deviceFamily.EnsureDefined();
        ValidateStartAndEndTime(startTime, endTime);

        var command = _database.StoredProcedures[GetDeviceLastQuickScanDateTime];
        command.Parameters.Add(DeviceFamily, (int)deviceFamily);
        command.Parameters.Add(GroupIds, CreateIntIdentifierTable(deviceGroupIds), SqlDbType.Structured);
        command.Parameters.Add(AntivirusLastQuickScanTimeFrom, DateTime.SpecifyKind(startTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        command.Parameters.Add(AntivirusLastQuickScanTimeTo, DateTime.SpecifyKind(endTime, DateTimeKind.Utc), SqlDbType.DateTime2);
        return command.ExecuteReader(ReadAntivirusLastQuickScanSummary);
    }

    private static DataTable CreateDataTableBulkInsertWindowsDevices(IEnumerable<WindowsDeviceData> devices)
    {
        var dataTable = new DataTable();

        dataTable.Columns.Add(new DataColumn(DeviceId, typeof(int)) { AllowDBNull = false });
        dataTable.Columns.Add(new DataColumn(IsLocked, typeof(bool)) { AllowDBNull = false });
        dataTable.Columns.Add(new DataColumn(Passcode, typeof(byte[])) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(DataKeyId, typeof(int)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(IsSandBoxEnabled, typeof(bool)) { AllowDBNull = false });
        dataTable.Columns.Add(new DataColumn(LastCheckInDeviceUserTime, typeof(DateTime)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(OsImageId, typeof(int)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(OsImageDeployedTime, typeof(DateTime)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(HardwareId, typeof(string)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(WifiSubnet, typeof(string)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(WifiModeId, typeof(int)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(NetworkAuthenticationId, typeof(int)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(WirelessLanModeId, typeof(int)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(AntivirusLastQuickScanTime, typeof(DateTime)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(AntivirusLastFullScanTime, typeof(DateTime)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(AntivirusLastSyncTime, typeof(DateTime)) { AllowDBNull = true });
        dataTable.Columns.Add(new DataColumn(BiosPasswordStatusId, typeof(int)) { AllowDBNull = true });

        foreach (var data in devices)
        {
            dataTable.Rows.Add(
                data.DeviceId,
                data.IsLocked,
                data.Passcode,
                data.DataKeyId,
                data.IsSandBoxEnabled,
                data.LastCheckInDeviceUserTime,
                data.OsImageId,
                data.OsImageDeploymentTime,
                data.HardwareId,
                data.WifiSubnet,
                data.WifiModeId,
                data.NetworkAuthenticationId,
                data.WirelessLanModeId,
                data.AntivirusLastQuickScanTime,
                data.AntivirusLastFullScanTime,
                data.AntivirusLastSyncTime,
                data.BiosPasswordStatusId);
        }

        return dataTable;
    }

    private static OrdinalColumnMapping GetColumnMapping(IDataRecord record)
    {
        _ordinalColumnMapping ??= new OrdinalColumnMapping
        {
            DeviceId = record.GetOrdinal(DeviceId),
            IsLocked = record.GetOrdinal(IsLocked),
            Passcode = record.GetOrdinal(Passcode),
            DataKeyId = record.GetOrdinal(DataKeyId),
            IsSandBoxEnabled = record.GetOrdinal(IsSandBoxEnabled),
            LastCheckInDeviceUserTime = record.GetOrdinal(LastCheckInDeviceUserTime)
        };
        return _ordinalColumnMapping;
    }

    private static OrdinalColumnMapping GetColumnMappingForDeviceDetails(IDataRecord record)
    {
        _ordinalColumnMappingNetworkDetails ??= new OrdinalColumnMapping
        {
            DeviceId = record.GetOrdinal(DeviceId),
            IsLocked = record.GetOrdinal(IsLocked),
            Passcode = record.GetOrdinal(Passcode),
            DataKeyId = record.GetOrdinal(DataKeyId),
            IsSandBoxEnabled = record.GetOrdinal(IsSandBoxEnabled),
            LastCheckInDeviceUserTime = record.GetOrdinal(LastCheckInDeviceUserTime),
            HardwareId = record.GetOrdinal(HardwareId),
            WifiSubnet = record.GetOrdinal(WifiSubnet),
            WifiModeId = record.GetOrdinal(WifiModeId),
            NetworkAuthenticationId = record.GetOrdinal(NetworkAuthenticationId),
            WirelessLanModeId = record.GetOrdinal(WirelessLanModeId),
            AntivirusLastQuickScanTime = record.GetOrdinal(AntivirusLastQuickScanTime),
            AntivirusLastFullScanTime = record.GetOrdinal(AntivirusLastFullScanTime),
            AntivirusLastSyncTime = record.GetOrdinal(AntivirusLastSyncTime),
            OsImageId = record.GetOrdinal(OsImageId),
            OsImageDeployedTime = record.GetOrdinal(OsImageDeployedTime),
            BiosPasswordStatusId = record.GetOrdinal(BiosPasswordStatusId)
        };
        return _ordinalColumnMappingNetworkDetails;
    }

    private static OrdinalColumnMapping GetColumnMappingForIsDeviceLocked(IDataRecord record)
    {
        _ordinalColumnMappingDeviceLocked ??= new OrdinalColumnMapping
        {
            DeviceId = record.GetOrdinal(DeviceId),
            IsLocked = record.GetOrdinal(IsLocked)
        };
        return _ordinalColumnMappingDeviceLocked;
    }

    private static OrdinalColumnMapping GetColumnMappingForIsSandBoxEnabled(IDataRecord record)
    {
        _ordinalColumnMappingSandBoxEnabled ??= new OrdinalColumnMapping
        {
            DeviceId = record.GetOrdinal(DeviceId),
            IsSandBoxEnabled = record.GetOrdinal(IsSandBoxEnabled)
        };
        return _ordinalColumnMappingSandBoxEnabled;
    }

    private static OrdinalColumnMappingAntivirusScanSummary GetColumnMappingForAntivirusLastFullScanSummary(IDataRecord record)
    {
        return new OrdinalColumnMappingAntivirusScanSummary
        {
            DevId = record.GetOrdinal(DevId),
            LastScanTime = record.GetOrdinal(AntivirusLastFullScanTime)
        };
    }

    private static OrdinalColumnMappingAntivirusScanSummary GetColumnMappingForAntivirusLastQuickScanSummary(IDataRecord record)
    {
        return new OrdinalColumnMappingAntivirusScanSummary
        {
            DevId = record.GetOrdinal(DevId),
            LastScanTime = record.GetOrdinal(AntivirusLastQuickScanTime)
        };
    }

    private static IReadOnlyCollection<WindowsDeviceData> ReadWindowsDeviceCollection(IDataReader reader)
    {
        var result = new List<WindowsDeviceData>();
        var mapping = GetColumnMapping(reader);
        while (reader.Read())
        {
            result.Add(ParseEntity(reader, mapping));
        }

        return result;
    }

    private static WindowsDeviceData ReadWindowsDeviceDetailsRecord(IDataReader reader)
    {
        var mapping = GetColumnMappingForDeviceDetails(reader);
        return reader.Read() ? ParseDeviceDetailsEntity(reader, mapping) : default;
    }

    private static IReadOnlyDictionary<int, bool> ReadAreDevicesLockedCollection(IDataReader reader)
    {
        var result = new Dictionary<int, bool>();
        var mapping = GetColumnMappingForIsDeviceLocked(reader);
        while (reader.Read())
        {
            var data = ParseEntityForIsDeviceLocked(reader, mapping);
            result[data.Key] = data.Value;
        }

        return result;
    }

    private static IReadOnlyDictionary<int, bool> ReadIsSandBoxEnabledByIdsCollection(IDataReader reader)
    {
        var result = new Dictionary<int, bool>();
        var mapping = GetColumnMappingForIsSandBoxEnabled(reader);
        while (reader.Read())
        {
            var data = ParseEntityForIsSandboxEnabled(reader, mapping);
            result[data.Key] = data.Value;
        }

        return result;
    }

    private static IReadOnlyDictionary<int, WindowsDeviceData> ReadDeviceDetailsByIdsCollection(IDataReader reader)
    {
        var result = new Dictionary<int, WindowsDeviceData>();
        var mapping = GetColumnMappingForDeviceDetails(reader);
        while (reader.Read())
        {
            var data = ParseDeviceDetailsEntity(reader, mapping);
            result[data.DeviceId] = data;
        }

        return result;
    }

    private static IReadOnlyCollection<AntivirusLastScanDeviceData> ReadAntivirusLastFullScanSummary(IDataReader reader)
    {
        var result = new List<AntivirusLastScanDeviceData>();
        var mapping = GetColumnMappingForAntivirusLastFullScanSummary(reader);
        while (reader.Read())
        {
            result.Add(ParseEntityAntivirusLastScanSummary(reader, mapping));
        }

        return result;
    }

    private static IReadOnlyCollection<AntivirusLastScanDeviceData> ReadAntivirusLastQuickScanSummary(IDataReader reader)
    {
        var result = new List<AntivirusLastScanDeviceData>();
        var mapping = GetColumnMappingForAntivirusLastQuickScanSummary(reader);
        while (reader.Read())
        {
            result.Add(ParseEntityAntivirusLastScanSummary(reader, mapping));
        }

        return result;
    }

    private static AntivirusLastScanDeviceData ParseEntityAntivirusLastScanSummary(IDataRecord reader, OrdinalColumnMappingAntivirusScanSummary mapping)
    {
        return new AntivirusLastScanDeviceData
        {
            DevId = reader.GetString(mapping.DevId),
            LastScanDate = reader.GetDateTime(mapping.LastScanTime)
        };
    }

    private static KeyValuePair<int, bool> ParseEntityForIsDeviceLocked(IDataRecord record, OrdinalColumnMapping mapping)
    {
        return new KeyValuePair<int, bool>(
            record.GetInt32(mapping.DeviceId),
            record.GetBoolean(mapping.IsLocked));
    }

    private static KeyValuePair<int, bool> ParseEntityForIsSandboxEnabled(IDataRecord record, OrdinalColumnMapping mapping)
    {
        return new KeyValuePair<int, bool>(
            record.GetInt32(mapping.DeviceId),
            record.GetBoolean(mapping.IsSandBoxEnabled));
    }

    private static WindowsDeviceData ParseEntity(IDataRecord record, OrdinalColumnMapping mapping)
    {
        return new WindowsDeviceData
        {
            DeviceId = record.GetInt32(mapping.DeviceId),
            IsLocked = record.GetBoolean(mapping.IsLocked),
            Passcode = record.IsDBNull(mapping.Passcode) ? null : (byte[])record.GetValue(mapping.Passcode),
            DataKeyId = record.IsDBNull(mapping.DataKeyId) ? (int?)null : (int?)record.GetValue(mapping.DataKeyId),
            IsSandBoxEnabled = record.GetBoolean(mapping.IsSandBoxEnabled),
            LastCheckInDeviceUserTime = record.IsDBNull(mapping.LastCheckInDeviceUserTime) ? (DateTime?)null : record.GetDateTime(mapping.LastCheckInDeviceUserTime)
        };
    }

    private static WindowsDeviceData ParseDeviceDetailsEntity(IDataRecord record, OrdinalColumnMapping mapping)
    {
        return new WindowsDeviceData
        {
            DeviceId = record.GetInt32(mapping.DeviceId),
            IsLocked = record.GetBoolean(mapping.IsLocked),
            Passcode = record.IsDBNull(mapping.Passcode) ? null : (byte[])record.GetValue(mapping.Passcode),
            DataKeyId = GetNullable<int>(record, mapping.DataKeyId),
            IsSandBoxEnabled = record.GetBoolean(mapping.IsSandBoxEnabled),
            LastCheckInDeviceUserTime = GetNullable<DateTime>(record, mapping.LastCheckInDeviceUserTime),
            HardwareId = GetNullableString(record, mapping.HardwareId),
            WifiSubnet = GetNullableString(record, mapping.WifiSubnet),
            WifiModeId = GetEnum(record, mapping.WifiModeId, WifiModeType.None),
            NetworkAuthenticationId = GetEnum(record, mapping.NetworkAuthenticationId, NetworkAuthenticationType.None),
            WirelessLanModeId = GetEnum(record, mapping.WirelessLanModeId, WirelessLanModeType.None),
            AntivirusLastQuickScanTime = GetNullable<DateTime>(record, mapping.AntivirusLastQuickScanTime),
            AntivirusLastFullScanTime = GetNullable<DateTime>(record, mapping.AntivirusLastFullScanTime),
            AntivirusLastSyncTime = GetNullable<DateTime>(record, mapping.AntivirusLastSyncTime),
            OsImageId = GetNullable<int>(record, mapping.OsImageId),
            OsImageDeploymentTime = GetNullable<DateTime>(record, mapping.OsImageDeployedTime),
            BiosPasswordStatusId = GetEnum(record, mapping.BiosPasswordStatusId, BiosPasswordStatusType.NotConfigured)
        };
    }

    private static T? GetNullable<T>(IDataRecord record, int ordinal) where T : struct
    {
        return record.IsDBNull(ordinal) ? (T?)null : (T)record.GetValue(ordinal);
    }

    private static string GetNullableString(IDataRecord record, int ordinal)
    {
        return record.IsDBNull(ordinal) ? null : (string)record.GetValue(ordinal);
    }

    private static TEnum GetEnum<TEnum>(IDataRecord record, int ordinal, TEnum defaultValue) where TEnum : struct
    {
        return record.IsDBNull(ordinal)
            ? defaultValue
            : (TEnum)Enum.ToObject(typeof(TEnum), record.GetByte(ordinal));
    }

    private sealed class OrdinalColumnMapping
    {
        public int DeviceId { get; internal set; }

        public int IsLocked { get; internal set; }

        public int IsSandBoxEnabled { get; internal set; }

        public int Passcode { get; internal set; }

        public int DataKeyId { get; internal set; }

        public int LastCheckInDeviceUserTime { get; internal set; }

        public int HardwareId { get; internal set; }

        public int WifiSubnet { get; internal set; }

        public int WifiModeId { get; internal set; }

        public int NetworkAuthenticationId { get; internal set; }

        public int WirelessLanModeId { get; internal set; }

        public int AntivirusLastQuickScanTime { get; internal set; }

        public int AntivirusLastFullScanTime { get; internal set; }

        public int AntivirusLastSyncTime { get; internal set; }

        public int OsImageId { get; internal set; }

        public int OsImageDeployedTime { get; internal set; }

        public int BiosPasswordStatusId { get; internal set; }
    }

    private static void ValidateGroupIdsAndFamilyId(IEnumerable<int> groupIds, DeviceFamily deviceFamily)
    {
        if (groupIds == null || !groupIds.Any())
        {
            throw new ArgumentNullException(nameof(groupIds));
        }

        deviceFamily.EnsureDefined();
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

    private static AntivirusGroupScanSummary ReadAntivirusGroupScanSummary(IDataReader reader)
    {
        var mapping = GetColumnMappingForAntivirusGroupScanSummary(reader);
        var result = new AntivirusGroupScanSummary();
        if (reader.Read())
        {
            result = ParseAntivirusGroupScanSummary(reader, mapping);
        }

        return result;
    }

    private static void ValidateStartAndEndTime(DateTime startTime, DateTime endTime)
    {
        if (startTime > endTime)
        {
            throw new ArgumentException("Start time cannot be later than the end time.");
        }
    }

    private static AntivirusGroupScanSummary ReadCustomAntivirusGroupScanSummary(IDataReader reader)
    {
        var mapping = GetColumnMappingForCustomAntivirusGroupScanSummary(reader);

        var result = new AntivirusGroupScanSummary();
        if (reader.Read())
        {
            result = ParseCustomAntivirusGroupScanSummary(reader, mapping);
        }

        return result;
    }

    private static AntivirusScanTimeData ReadAntivirusScanTimeDataRecord(IDataRecord record)
    {
        var mapping = GetColumnMappingForAntivirusScanTimeData(record);
        return ParseDeviceAntivirusScanTimeData(record, mapping);
    }

    private static OrdinalColumnMappingAntivirusScanCounts GetColumnMappingForCustomAntivirusGroupScanSummary(IDataRecord record)
    {
        return new OrdinalColumnMappingAntivirusScanCounts
        {
            AntivirusLastQuickScanCount = record.GetOrdinal(CustomAntivirusLastQuickScanCount),
            AntivirusLastFullScanCount = record.GetOrdinal(CustomAntivirusLastFullScanCount)
        };
    }

    private static OrdinalColumnMappingAntivirusScanTimeData GetColumnMappingForAntivirusScanTimeData(IDataRecord record)
    {
        return new OrdinalColumnMappingAntivirusScanTimeData
        {
            AntivirusLastQuickScanTimeProperty = record.GetOrdinal(AntivirusLastQuickScanTime),
            AntivirusLastFullScanTimeProperty = record.GetOrdinal(AntivirusLastFullScanTime),
            LastAntivirusSyncTimeProperty = record.GetOrdinal(LastAntivirusSyncTime),
            IsThreatAvailableProperty = record.GetOrdinal(IsThreatAvailable),
            IsActiveThreatAvailableProperty = record.GetOrdinal(ActiveThreatAvailable)
        };
    }

    private static AntivirusGroupScanSummary ParseCustomAntivirusGroupScanSummary(IDataRecord record, OrdinalColumnMappingAntivirusScanCounts mapping)
    {
        return new AntivirusGroupScanSummary
        {
            Custom = new AntivirusScanCounts
            {
                QuickScanned = record.IsDBNull(mapping.AntivirusLastQuickScanCount) ? 0 : record.GetInt32(mapping.AntivirusLastQuickScanCount),
                FullScanned = record.IsDBNull(mapping.AntivirusLastFullScanCount) ? 0 : record.GetInt32(mapping.AntivirusLastFullScanCount)
            }
        };
    }
    private static AntivirusScanTimeData ParseDeviceAntivirusScanTimeData(IDataRecord record, OrdinalColumnMappingAntivirusScanTimeData mapping)
    {
        return new AntivirusScanTimeData
        {
            AntivirusLastQuickScanTime = record.IsDBNull(mapping.AntivirusLastQuickScanTimeProperty) ? (DateTime?)null : record.GetDateTime(mapping.AntivirusLastQuickScanTimeProperty),
            AntivirusLastFullScanTime = record.IsDBNull(mapping.AntivirusLastFullScanTimeProperty) ? (DateTime?)null : record.GetDateTime(mapping.AntivirusLastFullScanTimeProperty),
            LastAntivirusSyncTime = record.IsDBNull(mapping.LastAntivirusSyncTimeProperty) ? (DateTime?)null : record.GetDateTime(mapping.LastAntivirusSyncTimeProperty),
            IsThreatsAvailable = record.GetInt32(mapping.IsThreatAvailableProperty) == 1,
            IsActiveThreatAvailable = record.GetInt32(mapping.IsActiveThreatAvailableProperty) == 1
        };
    }

    private static OrdinalColumnMappingAntivirusScanCounts GetColumnMappingForAntivirusGroupScanSummary(IDataRecord record)
    {
        return new OrdinalColumnMappingAntivirusScanCounts
        {
            LastQuickScanCountWithin24Hours = record.GetOrdinal(QuickScanCountWithin24Hours),
            LastFullScanCountWithin24Hours = record.GetOrdinal(FullScanCountWithin24Hours),
            LastQuickScanCountWithin7Days = record.GetOrdinal(QuickScanCountWithin7Days),
            LastFullScanCountWithin7Days = record.GetOrdinal(FullScanCountWithin7Days),
            LastQuickScanCountMoreThan30Days = record.GetOrdinal(QuickScanCountMoreThan30Days),
            LastFullScanCountMoreThan30Days = record.GetOrdinal(FullScanCountMoreThan30Days)
        };
    }

    private static AntivirusGroupScanSummary ParseAntivirusGroupScanSummary(IDataRecord record, OrdinalColumnMappingAntivirusScanCounts mapping)
    {
        return new AntivirusGroupScanSummary
        {
            Within24Hrs = new AntivirusScanCounts
            {
                QuickScanned = record.IsDBNull(mapping.LastQuickScanCountWithin24Hours) ? 0 : record.GetInt32(mapping.LastQuickScanCountWithin24Hours),
                FullScanned = record.IsDBNull(mapping.LastFullScanCountWithin24Hours) ? 0 : record.GetInt32(mapping.LastFullScanCountWithin24Hours)
            },
            Within7Days = new AntivirusScanCounts
            {
                QuickScanned = record.IsDBNull(mapping.LastQuickScanCountWithin7Days) ? 0 : record.GetInt32(mapping.LastQuickScanCountWithin7Days),
                FullScanned = record.IsDBNull(mapping.LastFullScanCountWithin7Days) ? 0 : record.GetInt32(mapping.LastFullScanCountWithin7Days)
            },
            MoreThan30Days = new AntivirusScanCounts
            {
                QuickScanned = record.IsDBNull(mapping.LastQuickScanCountMoreThan30Days) ? 0 : record.GetInt32(mapping.LastQuickScanCountMoreThan30Days),
                FullScanned = record.IsDBNull(mapping.LastFullScanCountMoreThan30Days) ? 0 : record.GetInt32(mapping.LastFullScanCountMoreThan30Days)
            }
        };
    }

    /// <summary>
    ///  Internal struct that caches column name-ordinal mapping.
    /// </summary>
    private sealed class OrdinalColumnMappingAntivirusScanCounts
    {
        public int LastQuickScanCountWithin24Hours { get; set; }

        public int LastFullScanCountWithin24Hours { get; set; }

        public int LastQuickScanCountWithin7Days { get; set; }

        public int LastFullScanCountWithin7Days { get; set; }

        public int LastQuickScanCountMoreThan30Days { get; set; }

        public int LastFullScanCountMoreThan30Days { get; set; }

        public int AntivirusLastQuickScanCount { get; set; }

        public int AntivirusLastFullScanCount { get; set; }
    }

    /// <summary>
    ///  Internal struct that caches column name-ordinal mapping.
    /// </summary>
    private sealed class OrdinalColumnMappingAntivirusScanTimeData
    {
        public int AntivirusLastQuickScanTimeProperty { get; internal set; }

        public int AntivirusLastFullScanTimeProperty { get; internal set; }

        public int LastAntivirusSyncTimeProperty { get; internal set; }

        public int IsThreatAvailableProperty { get; internal set; }

        public int IsActiveThreatAvailableProperty { get; internal set; }
    }

    private sealed class OrdinalColumnMappingAntivirusScanSummary
    {
        public int DevId { get; internal set; }

        public int LastScanTime { get; internal set; }
    }
}
