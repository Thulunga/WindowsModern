using System;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern;

/// <summary>
/// Windows defender service.
/// </summary>
public interface IWindowsDefenderService
{
    /// <summary>
    /// Get antivirus threat history.
    /// </summary>
    /// <param name="deviceId">The device Id.</param>
    /// <param name="types">List of antivirus threat type.</param>
    /// <param name="severities">List of antivirus threat severity.</param>
    /// <param name="statuses">List of antivirus threat status.</param>
    /// <param name="startDate">Antivirus threat last status change start date.</param>
    /// <param name="endDate">Antivirus threat last status change end date.</param>
    /// <param name="skip">Specifies antivirus threat info records to skip.</param>
    /// <param name="take">Specifies antivirus threat info records to take.</param>
    /// <param name="sortBy">Specifies the column by which to sort the results.</param>
    /// <param name="order">Specifies the order of sorting.</param>
    /// <param name="totalCount">Specifies antivirus threat info total record count.</param>
    /// <returns>List of AntivirusDeviceThreatInfo.</returns>
    IEnumerable<AntivirusDeviceThreatInfo> GetAntivirusThreatHistory(
        int deviceId,
        IEnumerable<AntivirusThreatType> types,
        IEnumerable<AntivirusThreatSeverity> severities,
        IEnumerable<AntivirusThreatStatus> statuses,
        DateTime? startDate,
        DateTime? endDate,
        int skip,
        int take,
        AntivirusThreatSortByOption sortBy,
        bool order,
        out int totalCount);

    /// <summary>
    /// Get antivirus scan summary.
    /// </summary>
    /// <param name="deviceId">The device Id.</param>
    /// <returns>AntivirusScanSummary</returns>
    AntivirusScanSummary GetAntivirusScanSummary(int deviceId);

    /// <summary>
    /// Get antivirus threat history for group.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="types">List of antivirus threat type.</param>
    /// <param name="severities">List of antivirus threat severity.</param>
    /// <param name="startDate">Antivirus threat last status change start date.</param>
    /// <param name="endDate">Antivirus threat last status change end date.</param>
    /// <param name="skip">Specifies antivirus threat info records to skip.</param>
    /// <param name="take">Specifies antivirus threat info records to take.</param>
    /// <param name="sortBy">Specifies antivirus threat history sort by option.</param>
    /// <param name="isDescendingSortOrder">Specifies antivirus threat history sort order.</param>
    /// <param name="total">Returns the total number of records.</param>
    /// <returns>List of AntivirusGroupThreatInfo.</returns>
    IEnumerable<AntivirusGroupThreatInfo> GetDeviceGroupsAntivirusThreatHistory(
        IEnumerable<int> groupIds,
        IEnumerable<AntivirusThreatType> types,
        IEnumerable<AntivirusThreatSeverity> severities,
        DateTime? startDate,
        DateTime? endDate,
        int skip,
        int take,
        AntivirusThreatHistorySortByOption sortBy,
        bool isDescendingSortOrder,
        out int total);

    /// <summary>
    /// Gets the active threat availability data for the deviceId.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <returns>Returns the antivirus threat availability data.</returns>
    AntivirusThreatAvailabilityData GetActiveThreatsStatusByDeviceId(int deviceId);

    /// <summary>
    /// Gets the active antivirus threat availability data for deviceIds.
    /// </summary>
    /// <param name="deviceIds">List of deviceIds.</param>
    /// <returns>List of antivirus threat availability data for active threats.</returns>
    IEnumerable<AntivirusThreatAvailabilityData> GetActiveThreatsStatusByDeviceIds(IEnumerable<int> deviceIds);

    /// <summary>
    /// Gets all Antivirus threat history.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>List of AntivirusDeviceThreatInfo.</returns>
    IEnumerable<AntivirusDeviceThreatInfo> GetAllAntivirusThreatHistory(int deviceId);

    /// <summary>
    /// Gets device groups antivirus threat status device details.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="lastStatusChangeStartDate">Antivirus threat last status change start date.</param>
    /// <param name="lastStatusChangeEndDate">Antivirus threat last status change end date.</param>
    /// <param name="threatStatus">The threat status Id.</param>
    /// <param name="skip">Specifies device threat status details records to skip.</param>
    /// <param name="take">Specifies device threat status details records to take.</param>
    /// <param name="isDescendingOrder">Arrange data in Descending order based on Last status change time.</param>
    /// <param name="total">Returns the total number of records.</param>
    /// <returns></returns>
    IEnumerable<AntivirusThreatStatusDeviceDetails> GetDeviceGroupsAntivirusThreatStatus(
        IEnumerable<int> groupIds,
        DateTime lastStatusChangeStartDate,
        DateTime lastStatusChangeEndDate,
        AntivirusThreatStatus threatStatus,
        int skip,
        int take,
        bool isDescendingOrder,
        out int total);

    /// <summary>
    /// Gets Antivirus threat status count.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="startDate">Start time for getting threat status count.</param>
    /// <param name="endDate">End time for getting threat status count.</param>
    /// <returns>List of Threat statuses and their count dictionary.</returns>
    IDictionary<AntivirusThreatStatus, int> GetAntivirusThreatStatusCount(IEnumerable<int> groupIds, DateTime? startDate, DateTime? endDate);

    /// <summary>
    /// Get antivirus scan summary for the device groups.
    /// </summary>
    /// <param name="groupIds">List of groupIds.</param>
    /// <param name="syncTime">The last sync completed time..</param>
    /// <returns>Antivirus Group Scan Summary.</returns>
    AntivirusGroupScanSummary GetDeviceGroupsDefaultAntivirusScanSummary(IEnumerable<int> groupIds, DateTime syncTime);

    /// <summary>
    /// Get antivirus scan summary for the device groups within the specified date range.
    /// </summary>
    /// <param name="groupIds">List of groupIds.</param>
    /// <param name="startDate">The start date time.</param>
    /// <param name="endDate">The end date time.</param>
    /// <returns>Antivirus Group Scan Summary.</returns>
    AntivirusGroupScanSummary GetDeviceGroupsCustomAntivirusScanSummary(IEnumerable<int> groupIds, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets device groups antivirus last scan summary.
    /// </summary>
    /// <param name="antivirusLastScanDetailRequest">Antivirus last scan detail request.</param>
    /// <param name="totalCount">Specifies total record count.</param>
    /// <returns>List of antivirus last scan summary.</returns>
    IEnumerable<AntivirusLastScanDeviceSummary> GetDeviceGroupsAntivirusLastScanSummary(AntivirusLastScanDetailRequest antivirusLastScanDetailRequest, out int totalCount);

    /// <summary>
    /// Gets all antivirus threat status device details.
    /// </summary>
    /// <param name="deviceGroupIds">List of group Ids.</param>
    /// <param name="lastStatusChangeStartDate">Antivirus threat last status change start date.</param>
    /// <param name="lastStatusChangeEndDate">Antivirus threat last status change end date.</param>
    /// <param name="threatStatus">The threat status Id.</param>
    /// <returns>List of Devices and Antivirus Threat Status Details.</returns>
    IEnumerable<AntivirusThreatStatusDeviceDetails> GetAllAntivirusThreatStatus(
        IEnumerable<int> deviceGroupIds,
        DateTime lastStatusChangeStartDate,
        DateTime lastStatusChangeEndDate,
        AntivirusThreatStatus threatStatus);

    /// <summary>
    /// Get Device threat status by group Ids.
    /// </summary>
    /// <param name="threatId">External threat Id.</param>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="startDate">Antivirus threat last status change start date.</param>
    /// <param name="endDate">Antivirus threat last status change end date.</param>
    /// <returns>List of DeviceThreatDetails.</returns>
    IReadOnlyCollection<DeviceThreatDetails> GetDeviceThreatStatusByGroupIds(
        long threatId,
        IEnumerable<int> groupIds,
        DateTime? startDate,
        DateTime? endDate);

    /// <summary>
    /// Get Device threat status by group Ids with pagination.
    /// </summary>
    /// <param name="threatId">External threat Id.</param>
    /// <param name="groupIds">List of group ids.</param>
    /// <param name="startDate">Antivirus threat last status change start date.</param>
    /// <param name="endDate">Antivirus threat last status change end date.</param>
    /// <param name="skip">Specifies device threat details records to skip.</param>
    /// <param name="take">Specifies device threat details records to take.</param>
    /// <param name="sortBy">Specifies the column by which to sort the results.</param>
    /// <param name="isDescendingOrder">Arrange data in Descending order.</param>
    /// <param name="totalCount">Returns the total number of records.</param>
    /// <returns>List of DeviceThreatDetails.</returns>
    IReadOnlyCollection<DeviceThreatDetails> GetDeviceThreatStatusByGroupIdsPaginated(
        long threatId,
        IEnumerable<int> groupIds,
        DateTime? startDate,
        DateTime? endDate,
        int skip,
        int take,
        AntivirusThreatHistoryDetailsSortByOption sortBy,
        bool isDescendingOrder,
        out int totalCount);

    /// <summary>
    /// Gets Antivirus threat history details.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <returns>List of AntivirusGroupThreatHistoryDetails.</returns>
    IEnumerable<AntivirusGroupThreatHistoryDetails> GetAntivirusThreatHistoryDetails(IEnumerable<int> groupIds);

    /// <summary>
    /// Gets device groups antivirus last scan summary.
    /// </summary>
    /// <param name="groupIds">List of groupIds.</param>
    /// <param name="syncCompletedOn">The group sync request completed date time.</param>
    /// <param name="antivirusScanType">The antivirus scan type.</param>
    /// <param name="antivirusScanPeriod">The antivirus scan period.</param>
    /// <param name="lastScanStartDate">The last scan start date.</param>
    /// <param name="lastScanEndDate">The last scan end date.</param>
    /// <returns>List of antivirus last scan summary.</returns>
    IEnumerable<AntivirusLastScanDeviceSummary> GetDeviceGroupsAntivirusLastScanSummary(IEnumerable<int> groupIds, DateTime syncCompletedOn, AntivirusScanType antivirusScanType, AntivirusScanPeriodSubType antivirusScanPeriod, DateTime? lastScanStartDate, DateTime? lastScanEndDate);

    /// <summary>
    /// Delete device antivirus threat status by device id.
    /// </summary>
    /// <param name="deviceId">The integer device id.</param>
    void DeleteDeviceAntivirusThreatData(int deviceId);

    /// <summary>
    /// Delete defender scan data by device id.
    /// </summary>
    /// <param name="deviceId">The integer device id.</param>
    void DeleteWindowsDefenderData(int deviceId);
}
