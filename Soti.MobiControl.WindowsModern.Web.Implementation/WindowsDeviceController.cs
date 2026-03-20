using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Security;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Soti.Api.Metadata.DataRetrieval;
using Soti.Csv;
using Soti.Logging.MobiControl;
using Soti.Logging.MobiControl.Enums;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Devices.DevInfo;
using Soti.MobiControl.Events;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Identity;
using Soti.MobiControl.Security.Identity.Model;
using Soti.MobiControl.WebApi.Foundation.Host;
using Soti.MobiControl.WebApi.Foundation.InfoHeaders;
using Soti.MobiControl.WebApi.Foundation.Mvc;
using Soti.MobiControl.WebApi.Foundation.OperationTracking;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Enums;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Web.Implementation.Events;
using Soti.MobiControl.WindowsModern.Web.Implementation.Exceptions;
using Soti.Utilities.Extensions;
using static Soti.MobiControl.WindowsModern.Web.Implementation.DateRangeValidatorMethod;
using AntivirusThreatSeverity = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusThreatSeverity;
using AntivirusThreatStatus = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusThreatStatus;
using DataRetrievalOptionsModelBinder = Soti.MobiControl.Api.DataRetrieval.DataRetrievalOptionsModelBinder;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Windows Modern Device Action Controller.
/// </summary>
[Authorize]
[Route("windows/devices")]
[ApiController]
[HostingTarget(Target = HostingTarget.PublicApi)]
[FeatureScope(LogFeature.Device)]
public sealed class WindowsDeviceController : ControllerBase, IWindowsDeviceController
{
    private const int DefaultTake = 50;
    private const int DefaultSkip = 0;
    private const bool DefaultDataRetrievalDesc = true;
    private const string ApplicationCsv = "application/csv";

    private readonly IDeviceKeyInformationRetrievalService _deviceKeyInformationRetrievalService;
    private readonly IDeviceBitLockerKeyService _deviceBitLockerKeyService;
    private readonly IAccessControlManager _accessControlManager;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IUserIdentityProvider _userIdentityProvider;
    private readonly Lazy<IWindowsDeviceLocalUsersService> _windowsDeviceLocalUsersService;
    private readonly Lazy<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsService;
    private readonly IWindowsDefenderService _windowsDefenderService;
    private readonly Lazy<IWindowsModernDeviceConfigurationProxyService> _windowsModernDeviceConfigurationProxyService;
    private readonly IDevInfoService _devInfoService;
    private readonly Lazy<ICsvConverter> _csvConverter;
    private readonly CultureInfo _cultureInfo = Thread.CurrentThread.CurrentUICulture;
    private readonly ResourceManager _resourceManager = new(typeof(Resources.Resources))
    {
        IgnoreCase = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDeviceController"/> class.
    /// </summary>
    /// <param name="deviceKeyInformationRetrievalService">The device key information retrieval service.</param>
    /// <param name="deviceBitLockerKeyService">The device bit locker key service.</param>
    /// <param name="accessControlManager">The access control manager.</param>
    /// <param name="eventDispatcher">The event dispatcher.</param>
    /// <param name="userIdentityProvider">The user identity provider.</param>
    /// <param name="windowsDeviceLocalUsersService">The windows device local user service.</param>
    /// <param name="windowsDeviceLocalGroupsService">The windows device local group service.</param>
    /// <param name="windowsDefenderService">The Windows defender service.</param>
    /// <param name="devInfoService">The dev info service.</param>
    /// <param name="windowsModernDeviceConfigurationProxyService">The Windows modern device configuration proxy service.</param>
    /// <param name="csvConverter">The csv converter.</param>
    public WindowsDeviceController(
        IDeviceKeyInformationRetrievalService deviceKeyInformationRetrievalService,
        IDeviceBitLockerKeyService deviceBitLockerKeyService,
        IAccessControlManager accessControlManager,
        IEventDispatcher eventDispatcher,
        IUserIdentityProvider userIdentityProvider,
        Lazy<IWindowsDeviceLocalUsersService> windowsDeviceLocalUsersService,
        Lazy<IWindowsDeviceLocalGroupsService> windowsDeviceLocalGroupsService,
        IWindowsDefenderService windowsDefenderService,
        IDevInfoService devInfoService,
        Lazy<IWindowsModernDeviceConfigurationProxyService> windowsModernDeviceConfigurationProxyService,
        Lazy<ICsvConverter> csvConverter)
    {
        _deviceKeyInformationRetrievalService = ValidateArgument(deviceKeyInformationRetrievalService, nameof(deviceKeyInformationRetrievalService));
        _deviceBitLockerKeyService = ValidateArgument(deviceBitLockerKeyService, nameof(deviceBitLockerKeyService));
        _accessControlManager = ValidateArgument(accessControlManager, nameof(accessControlManager));
        _eventDispatcher = ValidateArgument(eventDispatcher, nameof(eventDispatcher));
        _userIdentityProvider = ValidateArgument(userIdentityProvider, nameof(userIdentityProvider));
        _windowsDeviceLocalUsersService = ValidateArgument(windowsDeviceLocalUsersService, nameof(windowsDeviceLocalUsersService));
        _windowsDeviceLocalGroupsService = ValidateArgument(windowsDeviceLocalGroupsService, nameof(windowsDeviceLocalGroupsService));
        _windowsDefenderService = ValidateArgument(windowsDefenderService, nameof(windowsDefenderService));
        _devInfoService = ValidateArgument(devInfoService, nameof(devInfoService));
        _windowsModernDeviceConfigurationProxyService = ValidateArgument(windowsModernDeviceConfigurationProxyService, nameof(windowsModernDeviceConfigurationProxyService));
        _csvConverter = ValidateArgument(csvConverter, nameof(csvConverter));
    }

    /// <summary>
    /// Returns the BitLocker keys information for a device.
    /// </summary>
    /// <remarks>
    /// This API returns the BitLocker keys information for a device.
    /// Each volume entry includes the drive letter, recovery key id, decrypted recovery key, drive encryption status,
    /// key protectors (flags enum), and drive type.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "View Decrypted Personal Recovery Key" permission.
    /// <br/>
    /// BLE event: "Administrator has requested to view the BitLocker Recovery Key(s)" is logged when this view action is performed.
    /// <br/>
    /// <b>(Available Since MobiControl v2024.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Failed to 'fetch BitLocker keys' because the contract is invalid. Please check MS logs for more details.</response>
    /// <response code="401">Failed to 'fetch BitLocker keys' due to unauthorized access or invalid reference to the specified object. Please check MS logs for more details.</response>
    /// <response code="403">Failed to 'fetch BitLocker keys' due to unauthorized access or invalid reference to the specified object. Please check MS logs for more details.</response>
    /// <response code="500">Failed to 'fetch BitLocker keys'. Please check MS logs for more details.</response>
    [HttpPost]
    [Route("{deviceId}/actions/fetchBitLockerKeys")]
    [RegisterOperation(OperationCode = "FetchWindowsModernBitLockerKeys")]
    [RequiresSecurityPermission("DeviceControl", "ViewDecryptedRecoveryKey")]
    public IEnumerable<WindowsBitLockerKey> RequestWindowsBitLockerKeys([FromRoute] string deviceId)
    {
        var deviceInfo = GetDeviceInfo(deviceId);
        CheckForDeviceGroupPermission(deviceInfo);
        var result = _deviceBitLockerKeyService.GetBitLockerKeys(deviceInfo.DeviceId);

        _eventDispatcher.DispatchEvent(new ViewedBitLockerRecoveryKeysEvent(CurrentUserInfo?.UserId ?? 0, CurrentUserInfo?.UserName, deviceInfo.DeviceId, deviceInfo.DevId));
        return result.Select(r => r.ToRecoveryKeysContract());
    }

    /// <summary>
    /// Returns the auto-generated password of a local user.
    /// </summary>
    /// <remarks>
    /// This API returns the auto-generated password of a local user.
    /// <br/>
    /// Requires the caller be granted "Manage Devices" permission.
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
    [RegisterOperation(OperationCode = "GetDeviceLocalUsersPassword")]
    [RequiresSecurityPermission("DeviceControl")]
    public DeviceLocalUserPassword GetLocalUserPassword(string deviceId, string securityId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ValidationException("DeviceId must be specified.");
        }

        if (string.IsNullOrWhiteSpace(securityId))
        {
            throw new ValidationException("SID must be specified.");
        }

        var device = GetDeviceInfo(deviceId);

        CheckForDeviceManagementPermission(device);

        var deviceLocalUser = _windowsDeviceLocalUsersService.Value.GetLocalUserPasswordByDeviceIdAndSid(device.DeviceId, securityId);
        if (deviceLocalUser == null)
        {
            throw new SecurityException($"No local user with SID {securityId} in the device {deviceId}");
        }

        if (deviceLocalUser.AutoGeneratedPassword != null)
        {
            _eventDispatcher.DispatchEvent(new ViewedLocalUserAutoGeneratedPasswordEvent(CurrentUserInfo?.UserId ?? 0, CurrentUserInfo?.UserName, device.DeviceId, deviceId, deviceLocalUser.UserName));
        }

        return new DeviceLocalUserPassword { AutoGeneratedPassword = deviceLocalUser.AutoGeneratedPassword };
    }

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
    [RegisterOperation(OperationCode = "GetWindowsModernDeviceAntivirusThreatHistory")]
    [RequiresSecurityPermission("DeviceControl")]
    [ReturnsInfoHeader(InfoHeaderType.TotalItemsCount)]
    [SwaggerIgnore]
    public IEnumerable<AntivirusDeviceThreatHistory> GetAntivirusThreatHistory(
        [FromRoute] string deviceId,
        [FromQuery] DateTime? startDateTime = null,
        [FromQuery] DateTime? endDateTime = null,
        [FromQuery] IEnumerable<AntivirusThreatCategory> categories = null,
        [FromQuery] IEnumerable<AntivirusThreatSeverity> severities = null,
        [FromQuery] IEnumerable<AntivirusThreatStatus> lastThreatStatuses = null,
        [ModelBinder(typeof(DataRetrievalOptionsModelBinder))] DataRetrievalOptions dataRetrievalOptions = null)
    {
        var device = GetDeviceInfo(deviceId);
        CheckForDeviceGroupPermission(device);

        startDateTime = startDateTime?.ToUniversalTime();
        endDateTime = endDateTime?.ToUniversalTime();

        var threatCategories = categories?.ToArray();
        var threatSeverities = severities?.ToArray();
        var threatStatuses = lastThreatStatuses?.ToArray();

        if (threatCategories != null && !threatCategories.AreAllDefined())
        {
            throw new ValidationException($"One or more values in the '{nameof(categories)}' parameter are invalid.");
        }

        if (threatSeverities != null && !threatSeverities.AreAllDefined())
        {
            throw new ValidationException($"One or more values in the '{nameof(severities)}' parameter are invalid.");
        }

        if (threatStatuses != null && !threatStatuses.AreAllDefined())
        {
            throw new ValidationException($"One or more values in the '{nameof(lastThreatStatuses)}' parameter are invalid.");
        }

        ValidateDateRange(startDateTime, endDateTime);
        dataRetrievalOptions.Validate();

        if (dataRetrievalOptions?.Order?.Length > 1)
        {
            throw new ValidationException("Sorting by multiple fields is not supported.");
        }

        var sortBy = dataRetrievalOptions?.Order?.FirstOrDefault()?.By;
        var sortByOption = AntivirusThreatSortByOption.AntivirusThreatLastStatusChangeTime;
        if (sortBy != null && !Enum.TryParse(sortBy, true, out sortByOption))
        {
            throw new ValidationException($"Invalid sort value: '{sortBy}'.");
        }

        if (!_windowsModernDeviceConfigurationProxyService.Value.IsAntivirusPayloadAssigned(device.DeviceId))
        {
            throw WindowsModernWebException.WindowsDefenderPayloadNotAssigned();
        }

        var threatHistory = _windowsDefenderService.GetAntivirusThreatHistory(
            device.DeviceId,
            threatCategories?.ToModel() ?? Enumerable.Empty<AntivirusThreatType>(),
            threatSeverities?.ToModel() ?? Enumerable.Empty<Models.Enums.AntivirusThreatSeverity>(),
            threatStatuses?.ToModel() ?? Enumerable.Empty<Models.Enums.AntivirusThreatStatus>(),
            startDateTime,
            endDateTime,
            dataRetrievalOptions?.Skip ?? DefaultSkip,
            dataRetrievalOptions?.Take ?? DefaultTake,
            sortByOption,
            dataRetrievalOptions?.Order?.FirstOrDefault()?.Descending ?? DefaultDataRetrievalDesc,
            out var totalCount
        ).ToDeviceThreatContract();

        this.SetInfoHeader(InfoHeaderType.TotalItemsCount, totalCount);

        return threatHistory;
    }

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
    [RegisterOperation(OperationCode = "ExportWindowsModernDeviceAntivirusThreatHistory")]
    [RequiresSecurityPermission("DeviceControl")]
    [SwaggerIgnore]
    public IActionResult ExportDeviceAntivirusThreatHistory(
        [FromRoute] string deviceId,
        [FromQuery] int timeZoneOffset)

    {
        var device = GetDeviceInfo(deviceId);
        CheckForDeviceGroupPermission(device);

        if (!_windowsModernDeviceConfigurationProxyService.Value.IsAntivirusPayloadAssigned(device.DeviceId))
        {
            throw WindowsModernWebException.WindowsDefenderPayloadNotAssigned();
        }

        var antivirusThreatHistoryData = _windowsDefenderService.GetAllAntivirusThreatHistory(device.DeviceId);

        var antivirusThreatHistoryDataDictionary =
            antivirusThreatHistoryData.Select(antivirusThreatHistory => antivirusThreatHistory.ToExportableDictionary(timeZoneOffset));
        var threatHistoryHeaders = typeof(Models.AntivirusDeviceThreatInfo).GetProperties().Select(p => p.Name).ToArray();
        var csvFileName = $"Device_DefenderData-{DateTime.UtcNow:yyyy-MM-dd-HH_mm_ss}.csv";
        var response = new PushStreamResult(
            async (outputStream) => { await _csvConverter.Value.GenerateCsvContent(antivirusThreatHistoryDataDictionary, threatHistoryHeaders, outputStream, timeZoneOffset, _cultureInfo, LocalizeHeaderField); },
            csvFileName,
            new MediaTypeHeaderValue("application/csv"));
        _eventDispatcher.DispatchEvent(new DefenderDataExportedEvent(CurrentUserInfo?.UserId ?? 0, CurrentUserInfo?.UserName, device.DeviceId, deviceId));
        return response;
    }

    /// <summary>
    /// Returns a list of local users.
    /// </summary>
    /// <remarks>
    /// This API returns a list of local users existing on a device
    /// <br/>
    /// Requires the caller be granted "Manage Devices" permission.
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
    [RegisterOperation(OperationCode = "GetDeviceLocalUsers")]
    [RequiresSecurityPermission("DeviceControl")]
    public IEnumerable<DeviceLocalUserSummary> GetDeviceLocalUsers(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ValidationException("DeviceId must be specified.");
        }

        var device = GetDeviceInfo(deviceId);

        CheckForDeviceManagementPermission(device);

        var deviceLocalUsers = _windowsDeviceLocalUsersService.Value.GetDeviceLocalUsersSummaryInfo(device.DeviceId);
        return deviceLocalUsers.Select(x => x.ToDeviceLocalUserSummary());
    }

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
    [ApiExplorerSettings(IgnoreApi = true)]
    [RegisterOperation(OperationCode = "GetWindowsDeviceLocalGroups")]
    [RequiresSecurityPermission("DeviceControl")]
    public IEnumerable<DeviceLocalGroupSummary> GetDeviceLocalGroups(string deviceId)
    {
        var device = GetDeviceInfo(deviceId);

        CheckForDeviceGroupPermission(device);
        CheckForDeviceUserManagementPermission(device.DeviceId);

        var deviceLocalGroups = _windowsDeviceLocalGroupsService.Value.GetDeviceLocalGroupsSummaryInfo(device.DeviceId);
        return deviceLocalGroups.Select(x => x.ToDeviceLocalGroupSummary());
    }

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
    [RegisterOperation(OperationCode = "GetWindowsModernAntivirusScanSummary")]
    [RequiresSecurityPermission("DeviceControl")]
    [SwaggerIgnore]
    public AntivirusScanSummary GetAntivirusScanSummary(string deviceId)
    {
        var device = GetDeviceInfo(deviceId);
        CheckForDeviceGroupPermission(device);

        if (!_windowsModernDeviceConfigurationProxyService.Value.IsAntivirusPayloadAssigned(device.DeviceId))
        {
            throw WindowsModernWebException.WindowsDefenderPayloadNotAssigned();
        }

        return _windowsDefenderService.GetAntivirusScanSummary(device.DeviceId).ToContract();
    }

    private string LocalizeHeaderField(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        var resourceKey = $"lbl_windows_modern_defender_property_{fieldName}";
        var localizedFieldName = _resourceManager.GetString(resourceKey, Thread.CurrentThread.CurrentCulture);
        return localizedFieldName ?? fieldName;
    }

    private DeviceKeyInformation GetDeviceInfo(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ValidationException(nameof(deviceId));
        }

        var device = _deviceKeyInformationRetrievalService.GetDeviceKeyInformation(deviceId);

        if (device == null)
        {
            throw new SecurityException($"Unable to get deviceId: {deviceId} in DB.");
        }

        if (device.DevicePlatform != DevicePlatform.WindowsDesktop10RS1 && device.DevicePlatform != DevicePlatform.WindowsDesktop10)
        {
            throw new SecurityException($"The supplied device id '{deviceId}' belongs to a non-supported device platform");
        }

        return device;
    }

    private void CheckForDeviceManagementPermission(DeviceKeyInformation device)
    {
        _accessControlManager.EnsureHasAccessRight(
            DevicePermission.ManageAndMoveDevices,
            SecurityAssetType.Device,
            device.DeviceId);
    }

    private void CheckForDeviceUserManagementPermission(int deviceId)
    {
        _accessControlManager.EnsureHasAccessRight(
            DevicePermission.ManageDeviceUser,
            SecurityAssetType.Device,
            deviceId);
    }

    private void CheckForDeviceGroupPermission(DeviceKeyInformation device)
    {
        _accessControlManager.EnsureHasAccessRight(DeviceGroupPermission.ViewGroup, SecurityAssetType.Device, device.DeviceId);
    }

    private static T ValidateArgument<T>(T argument, string name)
    {
        return EqualityComparer<T>.Default.Equals(argument, default) ? throw new ArgumentNullException(name) : argument;
    }

    private UserIdentity CurrentUserInfo => _userIdentityProvider.GetUserIdentity();
}
