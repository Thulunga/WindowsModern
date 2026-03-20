using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Resources;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Soti.Api.Metadata.DataRetrieval;
using Soti.Logging.MobiControl;
using Soti.Logging.MobiControl.Enums;
using Soti.MobiControl.DeviceGroupManagement;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Management.Services;
using Soti.MobiControl.WebApi.Foundation.Host;
using Soti.MobiControl.WebApi.Foundation.InfoHeaders;
using Soti.MobiControl.WebApi.Foundation.Mvc;
using Soti.MobiControl.WebApi.Foundation.OperationTracking;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Enums;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Web.Implementation.Exceptions;
using Soti.Utilities.Extensions;
using static Soti.MobiControl.WindowsModern.Web.Implementation.DateRangeValidatorMethod;
using AntivirusGroupThreatHistoryDetails = Soti.MobiControl.WindowsModern.Models.AntivirusGroupThreatHistoryDetails;
using AntivirusScanPeriod = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusScanPeriod;
using AntivirusScanPeriodSubType = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusScanPeriodSubType;
using AntivirusScanType = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusScanType;
using AntivirusThreatSeverity = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusThreatSeverity;
using AntivirusThreatStatus = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusThreatStatus;
using DataRetrievalOptionsModelBinder = Soti.MobiControl.Api.DataRetrieval.DataRetrievalOptionsModelBinder;
using ICsvConverter = Soti.Csv.ICsvConverter;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Windows Modern Device Groups Controller.
/// </summary>
[Authorize]
[Route("windows/devicegroups")]
[ApiController]
[HostingTarget(Target = HostingTarget.PublicApi)]
[SwaggerIgnore]
[FeatureScope(LogFeature.DeviceGroup)]
public class WindowsDeviceGroupsController : ControllerBase, IWindowsDeviceGroupsController
{
    private const int DefaultTake = 50;
    private const int DefaultSkip = 0;
    private const bool DefaultDataRetrievalDesc = true;
    private const string ApplicationCsv = "application/csv";
    private const SyncRequestType AntivirusSyncRequestType = SyncRequestType.Antivirus;

    private readonly Lazy<IDeviceGroupIdentityMapper> _deviceGroupIdentityMapper;
    private readonly Lazy<IAccessibleDeviceGroupService> _accessibleDeviceGroupService;
    private readonly IAccessControlManager _accessControlManager;
    private readonly IWindowsDeviceGroupsService _windowsDeviceGroupsService;
    private readonly IWindowsDefenderService _windowsDefenderService;
    private readonly Lazy<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsService;
    private readonly Lazy<ICsvConverter> _csvConverter;
    private readonly ResourceManager _resourceManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDeviceGroupsController"/> class.
    /// </summary>
    /// <param name="deviceGroupIdentityMapper">The device group identity mapper.</param>
    /// <param name="accessControlManager">The access control manager.</param>
    /// <param name="windowsDeviceGroupsService">The windows device groups service.</param>
    /// <param name="windowsDefenderService">The windows defender service.</param>
    /// <param name="accessibleDeviceGroupService">The accessible device group service.</param>
    /// <param name="windowsDeviceLocalGroupsService">The windows device local group service.</param>
    /// <param name="csvConverter">The csv converter.</param>
    public WindowsDeviceGroupsController(
        Lazy<IDeviceGroupIdentityMapper> deviceGroupIdentityMapper,
        IAccessControlManager accessControlManager,
        IWindowsDeviceGroupsService windowsDeviceGroupsService,
        IWindowsDefenderService windowsDefenderService,
        Lazy<IAccessibleDeviceGroupService> accessibleDeviceGroupService,
        Lazy<IWindowsDeviceLocalGroupsService> windowsDeviceLocalGroupsService,
        Lazy<ICsvConverter> csvConverter)
    {
        _deviceGroupIdentityMapper = deviceGroupIdentityMapper ?? throw new ArgumentNullException(nameof(deviceGroupIdentityMapper));
        _accessControlManager = accessControlManager ?? throw new ArgumentNullException(nameof(accessControlManager));
        _windowsDeviceGroupsService = windowsDeviceGroupsService ?? throw new ArgumentNullException(nameof(windowsDeviceGroupsService));
        _windowsDefenderService = windowsDefenderService ?? throw new ArgumentNullException(nameof(windowsDefenderService));
        _accessibleDeviceGroupService = accessibleDeviceGroupService ?? throw new ArgumentNullException(nameof(accessibleDeviceGroupService));
        _windowsDeviceLocalGroupsService = windowsDeviceLocalGroupsService ?? throw new ArgumentNullException(nameof(windowsDeviceLocalGroupsService));
        _csvConverter = csvConverter ?? throw new ArgumentNullException(nameof(csvConverter));

        _resourceManager = new ResourceManager(typeof(Resources.Resources))
        {
            IgnoreCase = true
        };
    }

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
    [HttpGet]
    [Route("{path}/antivirusLastSyncedStatus")]
    [RegisterOperation(OperationCode = "WindowsModern_DeviceGroups_GetAntivirusLastSyncedStatus")]
    [RequiresSecurityPermission("WebConsole")]
    public LastSyncSummary GetAntivirusLastSyncedStatus([FromRoute] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);

        return _windowsDeviceGroupsService.GetGroupSyncStatus(groupId, AntivirusSyncRequestType).ToContract();
    }

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
    [HttpGet]
    [Route("{path}/antivirusThreatStatus")]
    [RegisterOperation(OperationCode = "WindowsModern_DeviceGroups_GetAntivirusThreatStatusCount")]
    [RequiresSecurityPermission("WebConsole")]
    public IDictionary<string, int> GetAntivirusThreatStatusCount(
        [FromRoute] string path,
        [FromQuery] DateTime? startDateTime = null,
        [FromQuery] DateTime? endDateTime = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        startDateTime = startDateTime?.ToUniversalTime();
        endDateTime = endDateTime?.ToUniversalTime();

        ValidateDateRange(startDateTime, endDateTime);

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        CheckSyncStatus(groupId);

        return _windowsDefenderService.GetAntivirusThreatStatusCount(GetDescendantGroupIds(path), startDateTime, endDateTime).ToThreatStatusSummary();
    }

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
    [HttpGet]
    [Route("{path}/antivirusThreatHistory")]
    [RegisterOperation(OperationCode = "WindowsModern_GetDeviceGroupsAntivirusThreatHistory")]
    [ReturnsInfoHeader(InfoHeaderType.TotalItemsCount)]
    [RequiresSecurityPermission("WebConsole")]
    public IEnumerable<AntivirusGroupThreatHistory> GetDeviceGroupsAntivirusThreatHistory(
        [FromRoute] string path,
        [FromQuery] DateTime? startDateTime = null,
        [FromQuery] DateTime? endDateTime = null,
        [FromQuery] IEnumerable<AntivirusThreatCategory> category = null,
        [FromQuery] IEnumerable<AntivirusThreatSeverity> severity = null,
        [ModelBinder(typeof(DataRetrievalOptionsModelBinder))] DataRetrievalOptions dataRetrievalOptions = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        startDateTime = startDateTime?.ToUniversalTime();
        endDateTime = endDateTime?.ToUniversalTime();

        var threatCategories = category?.ToArray();
        var threatSeverities = severity?.ToArray();
        if (threatCategories != null && !threatCategories.AreAllDefined())
        {
            throw new ValidationException($"One or more values in the '{nameof(category)}' parameter are invalid.");
        }

        if (threatSeverities != null && !threatSeverities.AreAllDefined())
        {
            throw new ValidationException($"One or more values in the '{nameof(severity)}' parameter are invalid.");
        }

        ValidateDateRange(startDateTime, endDateTime);
        dataRetrievalOptions.Validate();

        if (dataRetrievalOptions?.Order?.Length > 1)
        {
            throw new ValidationException("Sorting by multiple fields is not supported.");
        }

        var sortBy = dataRetrievalOptions?.Order?.FirstOrDefault()?.By;
        var sortByOption = AntivirusThreatHistorySortByOption.LastStatusChangeTime;
        if (sortBy != null && !Enum.TryParse(sortBy, true, out sortByOption))
        {
            throw new ValidationException($"Invalid sort value: '{sortBy}'.");
        }

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        CheckSyncStatus(groupId);

        var result = _windowsDefenderService.GetDeviceGroupsAntivirusThreatHistory(
            GetDescendantGroupIds(path),
            threatCategories?.ToModel() ?? Enumerable.Empty<AntivirusThreatType>(),
            threatSeverities?.ToModel() ?? Enumerable.Empty<Models.Enums.AntivirusThreatSeverity>(),
            startDateTime,
            endDateTime,
            dataRetrievalOptions?.Skip ?? DefaultSkip,
            dataRetrievalOptions?.Take ?? DefaultTake,
            sortByOption,
            dataRetrievalOptions?.Order?.FirstOrDefault()?.Descending ?? DefaultDataRetrievalDesc,
            out var total);

        this.SetInfoHeader(InfoHeaderType.TotalItemsCount, total);
        return result?.ToGroupThreatContract();
    }

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
    [HttpGet]
    [Route("{path}/antivirusScanSummary")]
    [RegisterOperation(OperationCode = "WindowsModern_GetDeviceGroupsAntivirusScanSummary")]
    [RequiresSecurityPermission("WebConsole")]
    public AntivirusGroupScanSummary GetDeviceGroupsAntivirusScanSummary(
        [FromRoute] string path,
        [FromQuery] AntivirusScanPeriod scanPeriod,
        [FromQuery] DateTime? startDateTime = null,
        [FromQuery] DateTime? endDateTime = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        startDateTime = startDateTime?.ToUniversalTime();
        endDateTime = endDateTime?.ToUniversalTime();

        if (!Enum.IsDefined(typeof(AntivirusScanPeriod), scanPeriod))
        {
            throw new ValidationException("The 'scanPeriod' parameter is invalid.");
        }

        ValidateDateRange(startDateTime, endDateTime, scanPeriod != AntivirusScanPeriod.Custom);

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        var lastSyncSummary = _windowsDeviceGroupsService.GetGroupSyncStatus(groupId, AntivirusSyncRequestType);

        if (lastSyncSummary is { SyncStatus: SyncRequestStatus.Running })
        {
            throw WindowsModernWebException.WindowsDefenderSyncIsInProgress();
        }

        var groupIds = GetDescendantGroupIds(path);
        return scanPeriod switch
        {
            AntivirusScanPeriod.Default => _windowsDefenderService.GetDeviceGroupsDefaultAntivirusScanSummary(groupIds, lastSyncSummary.CompletedOn.Value)?.ToContract(),
            AntivirusScanPeriod.Custom => _windowsDefenderService.GetDeviceGroupsCustomAntivirusScanSummary(groupIds, startDateTime.Value, endDateTime.Value)?.ToContract(),
            _ => new AntivirusGroupScanSummary()
        };
    }

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
    [HttpGet]
    [Route("{path}/antivirusThreatHistoryDevices")]
    [RequiresSecurityPermission("WebConsole")]
    [ReturnsInfoHeader(InfoHeaderType.TotalItemsCount)]
    [RegisterOperation(OperationCode = "WindowsModern_GetDeviceGroupThreatDetails")]
    public IEnumerable<DeviceThreatDetails> GetDeviceGroupThreatDetails(
        [FromRoute] string path,
        [FromQuery] long threatId,
        [FromQuery] DateTime? startDateTime = null,
        [FromQuery] DateTime? endDateTime = null,
        [ModelBinder(typeof(DataRetrievalOptionsModelBinder))] DataRetrievalOptions dataRetrievalOptions = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        startDateTime = startDateTime?.ToUniversalTime();
        endDateTime = endDateTime?.ToUniversalTime();

        if (threatId <= 0)
        {
            throw new ValidationException(nameof(threatId));
        }

        ValidateDateRange(startDateTime, endDateTime);
        dataRetrievalOptions.Validate();

        var skip = dataRetrievalOptions?.Skip ?? DefaultSkip;
        var take = dataRetrievalOptions?.Take ?? DefaultTake;

        if (dataRetrievalOptions?.Order?.Length > 1)
        {
            throw new ValidationException("Sorting by multiple fields is not supported.");
        }

        var sortBy = dataRetrievalOptions?.Order?.FirstOrDefault()?.By;
        var sortByOption = AntivirusThreatHistoryDetailsSortByOption.LastStatusChangeTime;
        if (sortBy != null && !Enum.TryParse(sortBy, true, out sortByOption))
        {
            throw new ValidationException($"Invalid sort value: '{sortBy}'.");
        }

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        CheckSyncStatus(groupId);

        var threatDetails = _windowsDefenderService.GetDeviceThreatStatusByGroupIdsPaginated(
            threatId,
            GetDescendantGroupIds(path),
            startDateTime,
            endDateTime,
            skip,
            take,
            sortByOption,
            dataRetrievalOptions?.Order?.FirstOrDefault()?.Descending ?? DefaultDataRetrievalDesc,
            out var totalCount);
        this.SetInfoHeader(InfoHeaderType.TotalItemsCount, totalCount);
        return threatDetails.Select(threatStatus => threatStatus.ToContract());
    }

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
    [HttpGet]
    [Route("{path}/antivirusThreatHistoryDevices/exportReport")]
    [RequiresSecurityPermission("WebConsole")]
    [RegisterOperation(OperationCode = "WindowsModern_ExportDeviceGroupThreatDetails")]
    public IActionResult ExportDeviceGroupThreatDetails
    (
        [FromRoute] string path,
        [FromQuery] long threatId,
        [FromQuery] DateTime? startDateTime = null,
        [FromQuery] DateTime? endDateTime = null,
        [FromQuery] int timezoneOffset = 0)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        if (threatId <= 0)
        {
            throw new ValidationException(nameof(threatId));
        }

        startDateTime = startDateTime?.ToUniversalTime();
        endDateTime = endDateTime?.ToUniversalTime();

        ValidateDateRange(startDateTime, endDateTime);

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        CheckSyncStatus(groupId);

        var deviceThreatDetails = _windowsDefenderService.GetDeviceThreatStatusByGroupIds(threatId, GetDescendantGroupIds(path), startDateTime, endDateTime)
            .OrderByDescending(deviceThreatDetails => deviceThreatDetails.LastStatusChangeTime);
        var threatDetailsHeaderFields = typeof(Models.DeviceThreatDetails).GetProperties().Select(p => p.Name).ToArray();
        var deviceThreatDetailsDictionary = deviceThreatDetails.Select(threatDetails => threatDetails.ToDictionary(timezoneOffset));
        var filename = $"DeviceGroup_ThreatHistory_{threatId}_{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}.csv";

        var response = new PushStreamResult(
                async (outputStream) => { await _csvConverter.Value.GenerateCsvContent(deviceThreatDetailsDictionary, threatDetailsHeaderFields, outputStream, timezoneOffset, Thread.CurrentThread.CurrentUICulture, LocalizePropertyName); },
                filename,
                new MediaTypeHeaderValue(ApplicationCsv));
        return response;
    }

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
    [HttpGet]
    [Route("{path}/antivirusThreatHistory/exportReport")]
    [RegisterOperation(OperationCode = "WindowsModern_ExportDeviceGroupsAntivirusThreatHistory")]
    [RequiresSecurityPermission("WebConsole")]
    public IActionResult ExportDeviceGroupsAntivirusThreatHistory
    ([FromRoute] string path, [FromQuery] int timeZoneOffset = 0)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        CheckSyncStatus(groupId);

        var data = _windowsDefenderService.GetAntivirusThreatHistoryDetails(GetDescendantGroupIds(path)).OrderByDescending(d => d.LastStatusChangeTime);
        var threatHistoryHeaderFields = typeof(AntivirusGroupThreatHistoryDetails).GetProperties().Select(p => p.Name).ToArray();
        var threatHistoryDictionary = data.Select(threatHistory => threatHistory.ToDictionary(timeZoneOffset));
        var filename = $"DeviceGroup_ThreatHistory_{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}.csv";

        var response = new PushStreamResult(
            async (outputStream) => { await _csvConverter.Value.GenerateCsvContent(threatHistoryDictionary, threatHistoryHeaderFields, outputStream, timeZoneOffset, Thread.CurrentThread.CurrentUICulture, LocalizePropertyName); },
            filename,
            new MediaTypeHeaderValue(ApplicationCsv));
        return response;
    }

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
    [HttpGet]
    [Route("{path}/antivirusScanSummaryDevices")]
    [RegisterOperation(OperationCode = "WindowsModern_GetDeviceGroupsAntivirusLastScanSummary")]
    [ReturnsInfoHeader(InfoHeaderType.TotalItemsCount)]
    [RequiresSecurityPermission("WebConsole")]
    public IEnumerable<AntivirusLastScanDeviceSummary> GetDeviceGroupsAntivirusLastScanSummary(
        [FromRoute] string path,
        [FromQuery] AntivirusScanPeriodSubType scanPeriod,
        [FromQuery] AntivirusScanType scanType,
        [FromQuery] DateTime? startDateTime = null,
        [FromQuery] DateTime? endDateTime = null,
        [FromQuery] bool isDescendingOrder = true,
        [ModelBinder(typeof(DataRetrievalOptionsModelBinder))]
        DataRetrievalOptionsSkipTakeOnly dataRetrievalOptions = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        startDateTime = startDateTime?.ToUniversalTime();
        endDateTime = endDateTime?.ToUniversalTime();
        if (!scanPeriod.IsDefined())
        {
            throw new ValidationException($"Invalid enum value '{scanPeriod}'");
        }

        if (!scanType.IsDefined())
        {
            throw new ValidationException($"Invalid enum value '{scanType}'");
        }

        ValidateDateRange(startDateTime, endDateTime);
        dataRetrievalOptions.Validate();

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        var lastSyncSummary = _windowsDeviceGroupsService.GetGroupSyncStatus(groupId, AntivirusSyncRequestType);

        if (lastSyncSummary is { SyncStatus: SyncRequestStatus.Running })
        {
            throw WindowsModernWebException.WindowsDefenderSyncIsInProgress();
        }

        var antivirusLastScanInfo = new Models.AntivirusLastScanDetailRequest
        {
            GroupIds = GetDescendantGroupIds(path),
            SyncCompletedOn = lastSyncSummary.CompletedOn.Value,
            AntivirusScanType = scanType.ToModel(),
            AntivirusScanPeriod = scanPeriod.ToModel(),
            LastScanStartDate = startDateTime,
            LastScanEndDate = endDateTime,
            Skip = dataRetrievalOptions?.Skip ?? DefaultSkip,
            Take = dataRetrievalOptions?.Take ?? DefaultTake,
            Order = isDescendingOrder,
        };

        var result = _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(
            antivirusLastScanInfo,
            out var totalCount);

        this.SetInfoHeader(InfoHeaderType.TotalItemsCount, totalCount);
        return result?.ToLastScanSummary();
    }

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
    [HttpGet]
    [Route("{path}/antivirusThreatStatusDevices")]
    [RegisterOperation(OperationCode = "WindowsModern_GetDeviceGroupsAntivirusThreatStatusDevices")]
    [ReturnsInfoHeader(InfoHeaderType.TotalItemsCount)]
    [RequiresSecurityPermission("WebConsole")]
    public IEnumerable<AntivirusThreatStatusDevicesInfo> GetDeviceGroupsAntivirusThreatStatusDevices(
        [FromRoute] string path,
        [FromQuery] DateTime lastStatusChangeStartDate,
        [FromQuery] DateTime lastStatusChangeEndDate,
        [FromQuery] AntivirusThreatStatus threatStatus,
        [FromQuery] bool isDescendingOrder = true,
        [ModelBinder(typeof(DataRetrievalOptionsModelBinder))]
        DataRetrievalOptionsSkipTakeOnly dataRetrievalOptions = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        lastStatusChangeStartDate = lastStatusChangeStartDate.ToUniversalTime();
        lastStatusChangeEndDate = lastStatusChangeEndDate.ToUniversalTime();

        ValidateDateRange(lastStatusChangeStartDate, lastStatusChangeEndDate, false);
        dataRetrievalOptions.Validate();
        if (!threatStatus.IsDefined())
        {
            throw new ValidationException($"Invalid enum value '{threatStatus}'");
        }

        var skip = dataRetrievalOptions?.Skip ?? DefaultSkip;
        var take = dataRetrievalOptions?.Take ?? DefaultTake;

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        CheckSyncStatus(groupId);

        var result = _windowsDefenderService.GetDeviceGroupsAntivirusThreatStatus(
            GetDescendantGroupIds(path),
            lastStatusChangeStartDate,
            lastStatusChangeEndDate,
            threatStatus.ToModel(),
            skip,
            take,
            isDescendingOrder,
            out var total);

        this.SetInfoHeader(InfoHeaderType.TotalItemsCount, total);
        return result?.ToContract();
    }

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
    [HttpGet]
    [Route("{path}/antivirusScanSummaryDevices/exportReport")]
    [RegisterOperation(OperationCode = "WindowsModern_ExportDeviceGroupsAntivirusScanSummaryDevices")]
    [RequiresSecurityPermission("WebConsole")]
    public IActionResult ExportDeviceGroupsAntivirusScanSummaryDevices
        ([FromRoute] string path,
        [FromQuery] AntivirusScanType scanType,
        [FromQuery] AntivirusScanPeriodSubType scanPeriod,
        [FromQuery] DateTime? lastScannedStartTime = null,
        [FromQuery] DateTime? lastScannedEndTime = null,
        [FromQuery] int timeZoneOffset = 0)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        lastScannedStartTime = lastScannedStartTime?.ToUniversalTime();
        lastScannedEndTime = lastScannedEndTime?.ToUniversalTime();
        if (!scanPeriod.IsDefined())
        {
            throw new ValidationException($"Invalid enum value '{scanPeriod}'");
        }

        if (!scanType.IsDefined())
        {
            throw new ValidationException($"Invalid enum value '{scanType}'");
        }

        ValidateDateRange(lastScannedStartTime, lastScannedEndTime, scanPeriod != AntivirusScanPeriodSubType.Custom);

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        var lastSyncSummary = _windowsDeviceGroupsService.GetGroupSyncStatus(groupId, AntivirusSyncRequestType);

        if (lastSyncSummary is { SyncStatus: SyncRequestStatus.Running })
        {
            throw WindowsModernWebException.WindowsDefenderSyncIsInProgress();
        }

        var data = _windowsDefenderService.GetDeviceGroupsAntivirusLastScanSummary(
            GetDescendantGroupIds(path),
            lastSyncSummary.CompletedOn.Value,
            scanType.ToModel(),
            scanPeriod.ToModel(),
            lastScannedStartTime,
            lastScannedEndTime).OrderByDescending(d => d.LastScanDate);

        var scanSummaryHeaderFields = typeof(Models.AntivirusLastScanDeviceSummary).GetProperties().Select(p => p.Name).ToArray();
        var scanSummaryDictionary = data.Select(scanSummary => scanSummary.ToDictionary(timeZoneOffset));
        var filename = $"DeviceGroup_ScanHistory_{scanType}_{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}.csv";

        var response = new PushStreamResult(
        async (outputStream) =>
        {
            await _csvConverter.Value.GenerateCsvContent(scanSummaryDictionary, scanSummaryHeaderFields, outputStream, timeZoneOffset, Thread.CurrentThread.CurrentUICulture, LocalizePropertyName);
        },
        filename,
        new MediaTypeHeaderValue("application/csv"));
        return response;
    }

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
    [HttpGet]
    [Route("{path}/antivirusThreatStatusDevices/exportReport")]
    [RegisterOperation(OperationCode = "WindowsModern_ExportDeviceGroupAntivirusThreatStatus")]
    [RequiresSecurityPermission("WebConsole")]
    public IActionResult ExportDeviceGroupAntivirusThreatStatus
    ([FromRoute] string path,
    [FromQuery] DateTime lastStatusChangeStartDate,
    [FromQuery] DateTime lastStatusChangeEndDate,
    [FromQuery] AntivirusThreatStatus threatStatus,
    [FromQuery] int timeZoneOffset = 0)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        ValidateDateRange(lastStatusChangeStartDate, lastStatusChangeEndDate);
        if (!threatStatus.IsDefined())
        {
            throw new ValidationException($"Invalid enum value '{threatStatus}'");
        }

        lastStatusChangeStartDate = lastStatusChangeStartDate.ToUniversalTime();
        lastStatusChangeEndDate = lastStatusChangeEndDate.ToUniversalTime();

        var groupId = _deviceGroupIdentityMapper.Value.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        CheckSyncStatus(groupId);

        var deviceThreatStatus = _windowsDefenderService.GetAllAntivirusThreatStatus(GetDescendantGroupIds(path), lastStatusChangeStartDate, lastStatusChangeEndDate, threatStatus.ToModel())
            .OrderByDescending(deviceThreatStatus => deviceThreatStatus.LastStatusChangeTime);
        var threatStatusHeaderFields = typeof(Models.AntivirusThreatStatusDeviceDetails).GetProperties().Select(p => p.Name).ToArray();
        var deviceThreatStatusDictionary = deviceThreatStatus.Select(status => status.ToExportThreatStatusDictionary(timeZoneOffset));
        var filename = $"DeviceGroup_ThreatStatus_{threatStatus}_{DateTime.UtcNow:yyyy-MM-dd HH_mm_ss}.csv";

        var response = new PushStreamResult(
            async (outputStream) => { await _csvConverter.Value.GenerateCsvContent(deviceThreatStatusDictionary, threatStatusHeaderFields, outputStream, timeZoneOffset, Thread.CurrentThread.CurrentUICulture, LocalizePropertyName); },
            filename,
            new MediaTypeHeaderValue(ApplicationCsv));
        return response;
    }

    private string LocalizePropertyName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return null;
        }

        var resourceKey = $"lbl_windows_modern_defender_property_{propertyName}";
        var localizedHeader = _resourceManager.GetString(resourceKey, Thread.CurrentThread.CurrentCulture);
        return localizedHeader ?? propertyName;
    }

    private void CheckForDeviceGroupPermission(int deviceGroupId)
    {
        _accessControlManager.EnsureHasAccessRight(DeviceGroupPermission.ViewGroup, SecurityAssetType.DeviceGroup, deviceGroupId);
    }

    private void CheckSyncStatus(int deviceGroupId)
    {
        var syncStatus = _windowsDeviceGroupsService.GetGroupSyncStatus(deviceGroupId, AntivirusSyncRequestType);
        if (syncStatus.SyncStatus == SyncRequestStatus.Running)
        {
            throw WindowsModernWebException.WindowsDefenderSyncIsInProgress();
        }
    }

    private ISet<int> GetDescendantGroupIds(string deviceGroupPath)
    {
        var deviceGroupIds = _accessibleDeviceGroupService.Value.GetPermittedDeviceGroupIds(deviceGroupPath, true);

        return deviceGroupIds;
    }
}
