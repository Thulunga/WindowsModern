using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IActionResult = Microsoft.AspNetCore.Mvc.IActionResult;
using Soti.Api.Metadata;
using Soti.Api.Metadata.DataRetrieval;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web;

[RoutePrefix("windows/devicegroups")]
public interface IWindowsDeviceGroupsController
{
    /// <summary>
    /// Returns when Last Sync was completed and Sync Status.
    /// </summary>
    /// <remarks>
    /// This API returns when the Windows Defender data was last synced and if Sync is currently In progress.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusLastSyncedStatus")]
    LastSyncSummary GetAntivirusLastSyncedStatus([FromUri] string path);

    /// <summary>
    /// Returns Windows Defender Threat History Summary for group.
    /// </summary>
    /// <remarks>
    /// This API returns Windows Defender Threat History Summary for the group based on the date range selected.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <param name="startDateTime">Start date time.</param>
    /// <param name="endDateTime">End date time.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// <li>11801 - Sync is in progress. Try again later.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusThreatStatus")]
    IDictionary<string, int> GetAntivirusThreatStatusCount(
        [FromUri] string path,
        [FromUri] DateTime? startDateTime = null,
        [FromUri] DateTime? endDateTime = null);

    /// <summary>
    /// Returns Threat History of the Group until last sync time.
    /// </summary>
    /// <remarks>
    /// This API returns the Detailed Threat History for a group which includes, Threat ID, Threat Category, Severity, Initial Detection time, Last Status Change time and Number of Devices on which particular threat was found.
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <param name="startDateTime">Start date time.</param>
    /// <param name="endDateTime">End date time.</param>
    /// <param name="category">Threat category.</param>
    /// <param name="severity">Threat severity.</param>
    /// <param name="dataRetrievalOptions">Data retrieval options.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// <li>11801 - Sync is in progress. Try again later.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusThreatHistory")]
    IEnumerable<AntivirusGroupThreatHistory> GetDeviceGroupsAntivirusThreatHistory(
        [FromUri] string path,
        [FromUri] DateTime? startDateTime = null,
        [FromUri] DateTime? endDateTime = null,
        [FromUri] IEnumerable<AntivirusThreatCategory> category = null,
        [FromUri] IEnumerable<AntivirusThreatSeverity> severity = null,
        [Api.Metadata.ModelBinder(typeof(DataRetrievalOptionsModelBinder))] DataRetrievalOptions dataRetrievalOptions = null);

    /// <summary>
    /// Returns Scan Summary for the Group until last sync time.
    /// </summary>
    /// <remarks>
    /// This API returns the Scan Summary of the Number of Devices Quick Scanned and Full Scanned for time periods 'Within 24 hours', 'Within 7 days',
    /// 'More than 30 days' or Custom Date range based on Last Sync time.
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <param name="scanPeriod">Scan Period.</param>
    /// <param name="startDateTime">Start date time.</param>
    /// <param name="endDateTime">End date time.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// <li>11801 - Sync is in progress. Try again later.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusScanSummary")]
    AntivirusGroupScanSummary GetDeviceGroupsAntivirusScanSummary(
        [FromUri] string path,
        [FromUri] AntivirusScanPeriod scanPeriod,
        [FromUri] DateTime? startDateTime = null,
        [FromUri] DateTime? endDateTime = null);

    /// <summary>
    /// Returns Device IDs and other related details for a threat Id.
    /// </summary>
    /// <remarks>
    /// This API returns the list of Device IDs, Initial Detection time, Last Status Change time and Threat Status for a Threat ID at group level until Last Sync time.
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <param name="threatId">Antivirus threat Id.</param>
    /// <param name="startDateTime">Start date time.</param>
    /// <param name="endDateTime">End date time.</param>
    /// <param name="dataRetrievalOptions">Data retrieval options.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// <li>11801 - Sync is in progress. Try again later.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusThreatHistoryDevices")]
    IEnumerable<DeviceThreatDetails> GetDeviceGroupThreatDetails(
        [FromUri] string path,
        [FromUri] long threatId,
        [FromUri] DateTime? startDateTime = null,
        [FromUri] DateTime? endDateTime = null,
        [Api.Metadata.ModelBinder(typeof(DataRetrievalOptionsModelBinder))] DataRetrievalOptions dataRetrievalOptions = null);

    /// <summary>
    /// Exports Device IDs and other related details for a threat Id as a CSV file.
    /// </summary>
    /// <remarks>
    /// This API exports the list of Device IDs, Initial Detection time, Last Status Change time and Threat Status for a Threat ID at group level until Last Sync time.
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <param name="threatId">Antivirus threat Id.</param>
    /// <param name="startDateTime">Start date time.</param>
    /// <param name="endDateTime">End date time.</param>
    /// <param name="timezoneOffset">Time zone offset in minutes.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// <li>11801 - Sync is in progress. Try again later.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusThreatHistoryDevices/exportReport")]
    IActionResult ExportDeviceGroupThreatDetails
    (
        [FromUri] string path,
        [FromUri] long threatId,
        [FromUri] DateTime? startDateTime = null,
        [FromUri] DateTime? endDateTime = null,
        [FromUri] int timezoneOffset = 0);

    /// <summary>
    /// Exports Threat History of the Group until last sync time.
    /// </summary>
    /// <remarks>
    /// This API exports the Detailed Threat History for a group which includes, Threat ID, Threat Category, Severity, Initial Detection time, Last Status Change time and Number of Devices on which particular threat was found.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <param name="timeZoneOffset">Time zone offset from UTC (in Minutes).</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// <li>11801 - Sync is in progress. Try again later.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusThreatHistory/exportReport")]
    IActionResult ExportDeviceGroupsAntivirusThreatHistory
    ([FromUri] string path, [FromUri] int timeZoneOffset = 0);

    /// <summary>
    /// Returns a list of Device IDs and Last Scan time for a selected time period.
    /// </summary>
    /// <remarks>
    /// This API returns the List of Device IDs and when the last Scanned happened for the selected Scan type within the Last Sync time at group level.
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <param name="scanPeriod">Antivirus scan Period.</param>
    /// <param name="scanType">Antivirus scan type.</param>
    /// <param name="startDateTime">Start date time.</param>
    /// <param name="endDateTime">End date time.</param>
    /// <param name="dataRetrievalOptions">Data retrieval options.</param>
    /// <param name="isDescendingOrder">Arrange device data in Descending order based on Last scan date.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// <li>11801 - Sync is in progress. Try again later.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusScanSummaryDevices")]
    IEnumerable<AntivirusLastScanDeviceSummary> GetDeviceGroupsAntivirusLastScanSummary(
        [FromUri] string path,
        [FromUri] AntivirusScanPeriodSubType scanPeriod,
        [FromUri] AntivirusScanType scanType,
        [FromUri] DateTime? startDateTime = null,
        [FromUri] DateTime? endDateTime = null,
        [FromUri] bool isDescendingOrder = true,
        [Api.Metadata.ModelBinder(typeof(DataRetrievalOptionsModelBinder))] DataRetrievalOptionsSkipTakeOnly dataRetrievalOptions = null);

    /// <summary>
    /// Returns a List of Device IDs, Last Status Change time, Threat ID, Category and Severity of the threat.
    /// </summary>
    /// <remarks>
    /// This API returns a list of Device IDs of Windows Devices in which Windows Defender took Particular action, Last Status Change time, Threat ID, Category and Severity of the threat during a given time interval.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <param name="lastStatusChangeStartDate">The last status change start date.</param>
    /// <param name="lastStatusChangeEndDate">The last status change end date.</param>
    /// <param name="threatStatus">The threat status.</param>
    /// <param name="isDescendingOrder">Arrange data in Descending order based on Last status change time.</param>
    /// <param name="dataRetrievalOptions">Data retrieval options.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// <li>11801 - Sync is in progress. Try again later.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusThreatStatusDevices")]
    public IEnumerable<AntivirusThreatStatusDevicesInfo> GetDeviceGroupsAntivirusThreatStatusDevices(
        [FromUri] string path,
        [FromUri] DateTime lastStatusChangeStartDate,
        [FromUri] DateTime lastStatusChangeEndDate,
        [FromUri] AntivirusThreatStatus threatStatus,
        [FromUri] bool isDescendingOrder = true,
        [Api.Metadata.ModelBinder(typeof(DataRetrievalOptionsModelBinder))] DataRetrievalOptionsSkipTakeOnly dataRetrievalOptions = null);

    /// <summary>
    /// Exports a list of Device IDs and Last Scan time for a selected time period as a CSV file.
    /// </summary>
    /// <remarks>
    /// This API exports the List of Device IDs and when the Last Scan time for the selected Scan type within the Last Sync time at group level as a CSV file.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <param name="scanType">Antivirus scan type.</param>
    /// <param name="scanPeriod">Antivirus scan period.</param>
    /// <param name="lastScannedStartTime">Last scan start time.</param>
    /// <param name="lastScannedEndTime">Last scan end time.</param>
    /// <param name="timeZoneOffset">Time zone offset from UTC (in Minutes).</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// <li>11801 - Sync is in progress. Try again later.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusScanSummaryDevices/exportReport")]
    IActionResult ExportDeviceGroupsAntivirusScanSummaryDevices
        ([FromUri] string path,
        [FromUri] AntivirusScanType scanType,
        [FromUri] AntivirusScanPeriodSubType scanPeriod,
        [FromUri] DateTime? lastScannedStartTime = null,
        [FromUri] DateTime? lastScannedEndTime = null,
        [FromUri] int timeZoneOffset = 0);

    /// <summary>
    /// Exports a List of Device IDs, Last Status Change time, Threat ID, Category and Severity of the threat for a Particular action as a CSV file.
    /// </summary>
    /// <remarks>
    /// This API exports a list of Device IDs of Windows Devices in which Windows Defender took Particular action, Last Status Change time, Threat ID, Category and Severity during a given time interval as a CSV file.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "Web Console" and "View Group" permission.
    /// <br/>
    /// <b>(Available since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The reference ID or the path of the device group.
    /// <br/>When using a reference ID, it must be prepended to the ID value, "referenceId:" (e.g. referenceId%3A7e39724b-6120-4c1f-96a8-c04d4570a974).
    /// <br/>Path must be double URL-encoded (e.g. %255C%255CMy%2520Company). When called from this page, it should be encoded only once (%5C%5CMy%20Company).</param>
    /// <param name="lastStatusChangeStartDate">The last status change start date.</param>
    /// <param name="lastStatusChangeEndDate">The last status change end date.</param>
    /// <param name="threatStatus">The threat status.</param>
    /// <param name="timeZoneOffset">Time zone offset from UTC (in Minutes).</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11750 - Group sync required. Please try again after syncing.</li>
    /// <li>11801 - Sync is in progress. Try again later.</li>
    /// </ol>
    /// </response>
    [Api.Metadata.HttpGet]
    [Api.Metadata.Route("{path}/antivirusThreatStatusDevices/exportReport")]
    IActionResult ExportDeviceGroupAntivirusThreatStatus
        ([FromUri] string path,
        [FromUri] DateTime lastStatusChangeStartDate,
        [FromUri] DateTime lastStatusChangeEndDate,
        [FromUri] AntivirusThreatStatus threatStatus,
        [FromUri] int timeZoneOffset = 0);
}
