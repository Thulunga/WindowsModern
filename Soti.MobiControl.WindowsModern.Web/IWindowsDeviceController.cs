using System;
using IActionResult = Microsoft.AspNetCore.Mvc.IActionResult;
using System.Collections.Generic;
using Soti.Api.Metadata;
using Soti.Api.Metadata.DataRetrieval;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web;

[RoutePrefix("windows/devices")]
public interface IWindowsDeviceController
{
    /// <summary>
    /// Returns the BitLocker keys information for a device.
    /// </summary>
    /// <remarks>
    /// This API returns the BitLocker keys information for a device.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "View Decrypted Personal Recovery Key" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2024.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <returns>Returns the Bitlocker keys information for a device.</returns>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract Validation Failed.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    [HttpPost]
    [Route("{deviceId}/actions/fetchBitLockerKeys")]
    IEnumerable<WindowsBitLockerKey> RequestWindowsBitLockerKeys([FromUri] string deviceId);

    /// <summary>
    /// Returns the auto-generated password of a local user.
    /// </summary>
    /// <remarks>
    /// This API returns the auto-generated password of a local user.
    /// <br/>
    /// Requires the caller be granted "View Groups" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2025.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="securityId">The device local user identifier.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    [HttpPost]
    [Route("{deviceId}/localUser/{securityId}/action/fetchPassword")]
    DeviceLocalUserPassword GetLocalUserPassword([FromUri] string deviceId, [FromUri] string securityId);

    /// <summary>
    /// Returns a list of local users.
    /// </summary>
    /// <remarks>
    /// This API returns a list of local users existing on a device.
    /// <br/>
    /// Requires the caller be granted "View Groups" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2025.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    [HttpGet]
    [Route("{deviceId}/localUsers")]
    IEnumerable<DeviceLocalUserSummary> GetDeviceLocalUsers([FromUri] string deviceId);

    /// <summary>
    /// Returns a list of local groups.
    /// </summary>
    /// <remarks>
    /// This API returns a list of local groups existing on a device
    /// <br/>
    /// Requires the caller be granted "Configure Device/ Device Groups", "View Groups" and "Manage Device Users(s)" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2025.1.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    [HttpGet]
    [Route("{deviceId}/localGroups")]
    IEnumerable<DeviceLocalGroupSummary> GetDeviceLocalGroups([FromUri] string deviceId);

    /// <summary>
    /// Returns Threat History of the device until last sync time.
    /// </summary>
    /// <remarks>
    /// This API returns the Detailed Threat History which includes, Threat Id, Threat Category, Threat Name, Severity, Initial Detection time, Last Status Change time,
    /// Number of Detections till date and Latest Threat Status.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted "Device Control" and "View Group" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="startDateTime">The start date time.</param>
    /// <param name="endDateTime">The end date time.</param>
    /// <param name="categories">The threat categories.</param>
    /// <param name="severities">The threat severities.</param>
    /// <param name="lastThreatStatuses">The last threat statuses.</param>
    /// <param name="dataRetrievalOptions">The data retrieval options. 50 records will be returned if no 'Take' value is provided.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation Failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11800 - Windows Defender payload is not assigned.</li>
    /// </ol>
    /// </response>
    [HttpGet]
    [Route("{deviceId}/antivirusThreatHistory")]
    IEnumerable<AntivirusDeviceThreatHistory> GetAntivirusThreatHistory(
    [FromUri] string deviceId,
    [FromUri] DateTime? startDateTime = null,
    [FromUri] DateTime? endDateTime = null,
    [FromUri] IEnumerable<AntivirusThreatCategory> categories = null,
    [FromUri] IEnumerable<AntivirusThreatSeverity> severities = null,
    [FromUri] IEnumerable<Enums.AntivirusThreatStatus> lastThreatStatuses = null,
    [ModelBinder(typeof(DataRetrievalOptionsModelBinder))] DataRetrievalOptions dataRetrievalOptions = null);

    /// <summary>
    /// Returns Scan time and Threat summary based on last sync time.
    /// </summary>
    /// <remarks>
    /// This API returns the last Quick Scan time, Full Scan time, Active Threat Status, and Threat Status in 24 hours based on the Last Sync Time.
    /// <br/>
    /// Requires the caller be granted "Device Control" and "View Group" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2026.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation Failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11800 - Windows Defender payload is not assigned.</li>
    /// </ol>
    /// </response>
    [HttpGet]
    [Route("{deviceId}/antivirusScanSummary")]
    AntivirusScanSummary GetAntivirusScanSummary([FromUri] string deviceId);

    /// <summary>
    /// Returns a CSV file with Threat History of the device until last sync time.
    /// </summary>
    /// <remarks>
    /// This API exports the Detailed Threat History which includes, Threat Id, Threat Category, Threat Name, Severity, Initial Detection time,
    /// Last Status Change time, Number Of Detections till date and Latest Threat Status.
    /// <br/>
    /// <br/>
    /// Requires the caller to be granted "Device Control" and "View Group" permission.
    /// <br/>
    /// <b>
    /// (Available Since MobiControl v2026.0.0)
    /// </b>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="timeZoneOffset">Time zone offset from UTC (in minutes).</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>11800 - Windows Defender payload is not assigned.</li>
    /// </ol>
    /// </response>
    [HttpGet]
    [Route("{deviceId}/antivirusThreatHistory/exportReport")]
    IActionResult ExportDeviceAntivirusThreatHistory(
        [FromUri] string deviceId,
        [FromUri] int timeZoneOffset);
}
