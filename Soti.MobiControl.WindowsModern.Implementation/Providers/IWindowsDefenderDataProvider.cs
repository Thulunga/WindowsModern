using System;
using System.Collections.Generic;
using Soti.MobiControl.Devices;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Data provider interface for Windows defender entity.
/// </summary>
internal interface IWindowsDefenderDataProvider
{
    /// <summary>
    /// Returns Antivirus threat history.
    /// </summary>
    /// <param name="deviceId">The device Id.</param>
    /// <param name="threatTypeIds">List of antivirus threat type Ids.</param>
    /// <param name="threatSeverityIds">List of antivirus threat severity Ids.</param>
    /// <param name="threatStatusIds">List of antivirus threat status Ids.</param>
    /// <param name="statusChangeStartTime">Antivirus threat last status change start time.</param>
    /// <param name="statusChangeEndTime">Antivirus threat last status change end time.</param>
    /// <param name="skip">Specifies antivirus threat data records to skip.</param>
    /// <param name="take">Specifies antivirus threat data records to take.</param>
    /// <param name="sortBy">Specifies the column by which to sort the results.</param>
    /// <param name="order">Specifies the order of sorting.</param>
    /// <param name="totalCount">Specifies antivirus threat data total record count.</param>
    /// <returns>List of AntivirusThreatData</returns>
    IEnumerable<AntivirusThreatData> GetAntivirusThreatHistory(
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
        out int totalCount);

    /// <summary>
    /// Gets List of Threat statuses and their count.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="startTime">Start time for getting threat status count.</param>
    /// <param name="endTime">End time for getting threat status count.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <returns>List of Threat statuses and their count dictionary.</returns>
    IDictionary<byte, int> GetAntivirusThreatStatusCount(IEnumerable<int> groupIds, DateTime startTime, DateTime endTime, int deviceFamily);

    /// <summary>
    /// Gets List of Threat statuses and their count.
    /// </summary>
    /// <param name="deviceId">The device Id.</param>
    /// <param name="lastStatusChangeTimeFrom">Start time for getting threat status count.</param>
    /// <param name="lastStatusChangeTimeTo">End time for getting threat status count.</param>
    /// <returns>List of Threat statuses and their count dictionary.</returns>
    IDictionary<byte, int> GetThreatStatusIdCountForDevice(int deviceId, DateTime lastStatusChangeTimeFrom, DateTime lastStatusChangeTimeTo);

    /// <summary>
    /// Gets Antivirus threat history for a device group.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <param name="threatTypeIds">List of antivirus threat type Ids.</param>
    /// <param name="threatSeverityIds">List of antivirus threat severity Ids.</param>
    /// <param name="statusChangeStartTime">Antivirus threat last status change start time.</param>
    /// <param name="statusChangeEndTime">Antivirus threat last status change end time.</param>
    /// <param name="skip">Specifies antivirus threat data records to skip.</param>
    /// <param name="take">Specifies antivirus threat data records to take.</param>
    /// <param name="total">Returns the total number of records.</param>
    /// <returns>List of AntivirusThreatData</returns>
    IEnumerable<AntivirusThreatData> GetDeviceGroupsAntivirusThreatHistory(
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
        out int total);

    /// <summary>
    /// Check if a specified threat is available.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="antivirusThreatStatus">Antivirus threat status.</param>
    /// <returns>Returns the antivirus threat availability data.</returns>
    AntivirusThreatAvailabilityData GetAntivirusThreatAvailabilityByDeviceId(int deviceId, AntivirusThreatStatus antivirusThreatStatus);

    /// <summary>
    /// Get the deviceIds with Active Threats.
    /// </summary>
    /// <param name="deviceIds">List of deviceIds.</param>
    /// <param name="antivirusThreatStatus">Antivirus threat status.</param>
    /// <returns>List of antivirus threat availability data.</returns>
    IEnumerable<AntivirusThreatAvailabilityData> GetAntivirusThreatAvailabilityByDeviceIds(IEnumerable<int> deviceIds, AntivirusThreatStatus antivirusThreatStatus);

    /// <summary>
    /// Get Device threat status by group Ids and device family.
    /// </summary>
    /// <param name="threatId">External threat id.</param>
    /// <param name="groupIds">List of group ids.</param>
    /// <param name="deviceFamily">Device family.</param>
    /// <param name="statusChangeStartTime">Antivirus threat last status change start time.</param>
    /// <param name="statusChangeEndTime">Antivirus threat last status change end time.</param>
    /// <returns>List of DeviceThreatDetails</returns>
    IReadOnlyCollection<DeviceThreatDetails> GetDeviceThreatStatusByGroupIdsAndDeviceFamily(
        long threatId,
        IEnumerable<int> groupIds,
        DeviceFamily deviceFamily,
        DateTime statusChangeStartTime,
        DateTime statusChangeEndTime);

    /// <summary>
    /// Get Device threat status by group Ids and device family with pagination.
    /// </summary>
    /// <param name="threatId">External threat id.</param>
    /// <param name="groupIds">List of group ids.</param>
    /// <param name="deviceFamily">Device family.</param>
    /// <param name="statusChangeStartTime">Antivirus threat last status change start time.</param>
    /// <param name="statusChangeEndTime">Antivirus threat last status change end time.</param>
    /// <param name="skip">Specifies device threat details records to skip.</param>
    /// <param name="take">Specifies device threat details records to take.</param>
    /// <param name="sortBy">Specifies the column by which to sort the results.</param>
    /// <param name="isDescendingOrder">Arrange data in Descending order.</param>
    /// <param name="totalCount">Returns the total count of records.</param>
    /// <returns>List of DeviceThreatDetails</returns>
    IReadOnlyCollection<DeviceThreatDetails> GetDeviceThreatStatusByGroupIdsAndDeviceFamilyPaginated(
        long threatId,
        IEnumerable<int> groupIds,
        DeviceFamily deviceFamily,
        DateTime statusChangeStartTime,
        DateTime statusChangeEndTime,
        int skip,
        int take,
        AntivirusThreatHistoryDetailsSortByOption sortBy,
        bool isDescendingOrder,
        out int totalCount);

    /// <summary>
    /// Gets antivirus threat status.
    /// </summary>
    /// <param name="deviceGroupIds">The list of device group Ids.</param>
    /// <param name="deviceFamily">The device family Id.</param>
    /// <param name="lastStatusChangeStartDate">Antivirus threat last status change start date.</param>
    /// <param name="lastStatusChangeEndDate">Antivirus threat last status change end date.</param>
    /// <param name="threatStatus">Antivirus threat status Id.</param>
    /// <param name="skip">Specifies antivirus threat data records to skip.</param>
    /// <param name="take">Specifies antivirus threat data records to take.</param>
    /// <param name="isDescendingOrder">Arrange data in Descending order based on Last status change time.</param>
    /// <param name="totalCount">Specifies the total number of records.</param>
    /// <returns>List of Antivirus threat status device data.</returns>
    public IEnumerable<AntivirusThreatStatusDeviceData> GetAntivirusThreatStatus(
        IEnumerable<int> deviceGroupIds,
        DeviceFamily deviceFamily,
        DateTime lastStatusChangeStartDate,
        DateTime lastStatusChangeEndDate,
        AntivirusThreatStatus threatStatus,
        int skip,
        int take,
        bool isDescendingOrder,
        out int totalCount);

    /// <summary>
    /// Returns Antivirus group threat history.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <returns>List of DeviceGroupThreatHistoryData.</returns>
    public IEnumerable<DeviceGroupThreatHistoryData> GetAntivirusGroupThreatHistory(
        IEnumerable<int> groupIds,
        DeviceFamily deviceFamily);

    /// <summary>
    /// Gets device data by antivirus threat status.
    /// </summary>
    /// <param name="deviceGroupIds">The list of device group Ids.</param>
    /// <param name="deviceFamily">The device family Id.</param>
    /// <param name="lastStatusChangeStartDate">Antivirus threat last status change start date.</param>
    /// <param name="lastStatusChangeEndDate">Antivirus threat last status change end date.</param>
    /// <param name="threatStatus">Antivirus threat status Id.</param>
    /// <returns>List of Antivirus threat status data.</returns>
    public IEnumerable<AntivirusThreatStatusDeviceData> GetDeviceDataByAntivirusThreatStatus(
        IEnumerable<int> deviceGroupIds,
        DeviceFamily deviceFamily,
        DateTime lastStatusChangeStartDate,
        DateTime lastStatusChangeEndDate,
        AntivirusThreatStatus threatStatus);

    /// <summary>
    /// Gets all Antivirus threat history.
    /// </summary>
    /// <param name="deviceId">The Device Id.</param>
    /// <returns>List of AntivirusThreatData.</returns>
    IEnumerable<AntivirusThreatData> GetAllAntivirusThreatHistory(int deviceId);

    /// <summary>
    /// Deletes Antivirus threat status by device id.
    /// </summary>
    /// <param name="deviceId">The Device Id.</param>
    void DeleteDeviceAntivirusThreatData(int deviceId);
}
