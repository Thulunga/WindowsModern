using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soti.Api.Metadata.DataRetrieval;
using Soti.Logging.MobiControl;
using Soti.Logging.MobiControl.Enums;
using Soti.MobiControl.DeviceGroupManagement;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Emails;
using Soti.MobiControl.Emails.Model;
using Soti.MobiControl.Emails.Util;
using Soti.MobiControl.Events;
using Soti.MobiControl.Exceptions;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Identity;
using Soti.MobiControl.Security.Identity.Model;
using Soti.MobiControl.Security.Management.Services;
using Soti.MobiControl.WebApi.Foundation.Host;
using Soti.MobiControl.WebApi.Foundation.InfoHeaders;
using Soti.MobiControl.WebApi.Foundation.Mvc;
using Soti.MobiControl.WebApi.Foundation.OperationTracking;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Enums;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Web.Implementation.Events;
using Soti.Time;
using DataRetrievalOptionsModelBinder = Soti.MobiControl.Api.DataRetrieval.DataRetrievalOptionsModelBinder;
using ICsvConverter = Soti.Csv.ICsvConverter;
using ModelBinderAttribute = Microsoft.AspNetCore.Mvc.ModelBinderAttribute;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Device groups peripheral controller.
/// </summary>
[Authorize]
[Route("deviceGroups")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[HostingTarget(Target = HostingTarget.PublicApi)]
[FeatureScope(LogFeature.DeviceGroup)]
public sealed class DeviceGroupsPeripheralController : ControllerBase, IDeviceGroupsPeripheralController
{
    private const string DeviceGroupsPeripheralReportFilePrefix = "DeviceGroupsPeripherals-report-";
    private readonly IDeviceGroupPeripheralService _deviceGroupPeripheralService;
    private readonly IDeviceGroupIdentityMapper _deviceGroupIdentityMapper;
    private readonly IAccessibleDeviceGroupService _accessibleDeviceGroupService;
    private readonly IAccessControlManager _accessControlManager;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IEmailConfigurationService _emailConfigurationService;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly ICsvConverter _csvConverter;
    private readonly IUserIdentityProvider _userIdentityProvider;
    private readonly ICurrentTimeSupplier _timeSupplier;
    private readonly CultureInfo _cultureInfo;
    private readonly ResourceManager _resourceManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceGroupsPeripheralController"/> class.
    /// </summary>
    /// <param name="deviceGroupPeripheralService">The device group peripheral service.</param>
    /// <param name="deviceGroupIdentityMapper">The device group identity mapper.</param>
    /// <param name="accessibleDeviceGroupService">The accessible device group service.</param>
    /// <param name="accessControlManager">The access control manager service.</param>
    /// <param name="eventDispatcher">The event dispatcher.</param>
    /// <param name="emailConfigurationService">The email configuration service.</param>
    /// <param name="emailNotificationService">The email notification service.</param>
    /// <param name="csvConverter">The CSV converter.</param>
    /// <param name="userIdentityProvider">The user identity provider.</param>
    /// <param name="timeSupplier">The current time supplier.</param>
    public DeviceGroupsPeripheralController(
        IDeviceGroupPeripheralService deviceGroupPeripheralService,
        IDeviceGroupIdentityMapper deviceGroupIdentityMapper,
        IAccessibleDeviceGroupService accessibleDeviceGroupService,
        IAccessControlManager accessControlManager,
        IEventDispatcher eventDispatcher,
        IEmailConfigurationService emailConfigurationService,
        IEmailNotificationService emailNotificationService,
        ICsvConverter csvConverter,
        IUserIdentityProvider userIdentityProvider,
        ICurrentTimeSupplier timeSupplier)
    {
        _deviceGroupPeripheralService = deviceGroupPeripheralService ??
                                        throw new ArgumentNullException(nameof(deviceGroupPeripheralService));
        _deviceGroupIdentityMapper = deviceGroupIdentityMapper ??
                                     throw new ArgumentNullException(nameof(deviceGroupIdentityMapper));
        _accessibleDeviceGroupService = accessibleDeviceGroupService ??
                                        throw new ArgumentNullException(nameof(accessibleDeviceGroupService));
        _accessControlManager = accessControlManager ??
                                throw new ArgumentNullException(nameof(accessControlManager));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _emailConfigurationService = emailConfigurationService ??
                                     throw new ArgumentNullException(nameof(emailConfigurationService));
        _emailNotificationService = emailNotificationService ??
                                    throw new ArgumentNullException(nameof(emailNotificationService));
        _csvConverter = csvConverter ?? throw new ArgumentNullException(nameof(csvConverter));
        _userIdentityProvider = userIdentityProvider ??
                                throw new ArgumentNullException(nameof(userIdentityProvider));
        _cultureInfo = Thread.CurrentThread.CurrentUICulture;
        _resourceManager = new ResourceManager(typeof(Resources.Resources))
        {
            IgnoreCase = true
        };
        _timeSupplier = timeSupplier ?? throw new ArgumentNullException(nameof(timeSupplier));
    }

    private UserIdentity CurrentUser => _userIdentityProvider.GetUserIdentity();
    private int CurrentUserId => CurrentUser?.UserId ?? 0;
    private string CurrentUserName => CurrentUser?.UserName;

    /// <summary>
    /// Download  CSV of device groups peripheral summary listing.
    /// </summary>
    /// <remarks>
    /// This API downloads CSV of the device group peripheral data existing on a device group.
    /// <br/>
    /// Requires the caller be granted "View Peripherals" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2027.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The device group path.</param>
    /// <param name="reportHeaderFields">Peripheral report header fields.</param>
    /// <param name="nameContains">Peripheral name filter.</param>
    /// <param name="peripheralTypes">If specified, returns only peripherals the selected types.</param>
    /// <param name="deviceFamily">The family of device.</param>
    /// <param name="timeZoneOffset">The timezone offset.</param>
    /// <param name="statuses">If specified, returns only peripherals the selected status(es).</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract Validation Failed.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    [HttpGet]
    [Route("{path}/peripherals/{deviceFamily}/actions/exportReport")]
    [RegisterOperation(OperationCode = "DownloadPeripheralsReport")]
    [RequiresSecurityPermission("ViewPeripherals")]
    public IActionResult DownloadPeripheralsReport(
        [FromRoute] string path,
        [FromRoute] DeviceFamily deviceFamily,
        [FromQuery] string[] reportHeaderFields = null,
        [FromQuery] string nameContains = null,
        [FromQuery] string[] statuses = null,
        [FromQuery] string[] peripheralTypes = null,
        [FromQuery] int timeZoneOffset = 0)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        if (!Enum.IsDefined(typeof(DeviceFamily), deviceFamily))
        {
            throw new ValidationException($"Device Family '{deviceFamily}' is invalid.");
        }
        var groupId = _deviceGroupIdentityMapper.GetId(path);
        CheckForDeviceGroupPermission(groupId);

        ValidateReportHeaderFields(reportHeaderFields);
        var peripheralStatuses = ValidatePeripheralStatus(statuses);
        var types = ValidatePeripheralType(peripheralTypes);

        var peripheralSummaries = GetDeviceGroupPeripheralSummaries(path, deviceFamily);
        var deviceGroupsPeripheralSummaries = CalculateDeviceCountAndReturnPeripheralSummary(peripheralSummaries);

        var filteredPeripheralSummaries = FilterPeripheralSummaries(deviceGroupsPeripheralSummaries.ToList(), nameContains, peripheralStatuses, types, new DataRetrievalOptions { Take = int.MaxValue }, out _);
        var peripheralList = filteredPeripheralSummaries.Select(report => DeviceGroupsPeripheralHeaderConverter.ToExportableDictionary(report)).ToArray();

        var response = new PushStreamResult(
            async (outputStream) => { await _csvConverter.GenerateCsvContent(peripheralList, reportHeaderFields, outputStream, timeZoneOffset, _cultureInfo, LocalizePropertyName); },
            GetReportFileName(),
            new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/csv"));
        _eventDispatcher.DispatchEvent(new PeripheralDownloadReportSuccessEvent(CurrentUserId, CurrentUserName, groupId));
        return response;
    }

    /// <summary>
    /// Get device groups peripheral summary listing.
    /// </summary>
    /// <remarks>
    /// This API returns a list of device group peripheral data existing on a device group.
    /// <br/>
    /// Requires the caller be granted "View Peripherals" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2027.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The path of the device group.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <param name="statuses">Peripheral status search filter.</param>
    /// <param name="dataRetrievalOptions">The data retrieval options.</param>
    /// <param name="searchString">Peripheral name search filter.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract Validation Failed.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    [HttpGet]
    [Route("{path}/peripherals/{deviceFamily}")]
    [RegisterOperation(OperationCode = "GetDeviceGroupsPeripherals")]
    [ReturnsInfoHeader(InfoHeaderType.TotalItemsCount)]
    [RequiresSecurityPermission("ViewPeripherals")]
    public IEnumerable<DeviceGroupPeripheralSummary> GetDeviceGroupsPeripherals(
        [FromRoute] string path,
        [FromRoute] DeviceFamily deviceFamily,
        [FromQuery] string searchString = null,
        [FromQuery] IEnumerable<DevicePeripheralStatus> statuses = null,
        [ModelBinder(typeof(DataRetrievalOptionsModelBinder))] DataRetrievalOptions dataRetrievalOptions = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        if (!Enum.IsDefined(typeof(DeviceFamily), deviceFamily))
        {
            throw new ValidationException($"Device Family '{deviceFamily}' is invalid.");
        }

        if (dataRetrievalOptions != null)
        {
            if (dataRetrievalOptions.Skip < 0)
            {
                throw new ValidationException("Skip value should be greater than or equal to zero");
            }

            if (dataRetrievalOptions.Take <= 0)
            {
                throw new ValidationException("Take value should be greater than zero");
            }
        }

        EnsureDevicePeripheralStatus(statuses);

        var groupId = _deviceGroupIdentityMapper.GetId(path);
        CheckForDeviceGroupPermission(groupId);
        var deviceGroupPeripherals = GetDeviceGroupPeripheralSummaries(path, deviceFamily);
        var deviceGroupsPeripheralSummaries = CalculateDeviceCountAndReturnPeripheralSummary(deviceGroupPeripherals);
        deviceGroupsPeripheralSummaries = DeviceGroupPeripheralFilterHelper.Filter(deviceGroupsPeripheralSummaries, searchString, statuses);

        var deviceGroupPeripheralList = deviceGroupsPeripheralSummaries.ToList();
        if (deviceGroupPeripheralList.Count == 0)
        {
            HttpContext.Items["x-total-count"] = 0;
            return [];
        }

        HttpContext.Items["x-total-count"] = deviceGroupPeripheralList.Count;
        deviceGroupsPeripheralSummaries = DeviceGroupPeripheralOrderAndPagination.ApplyDataRetrievalOptions(deviceGroupPeripheralList, dataRetrievalOptions);
        return deviceGroupsPeripheralSummaries;
    }

    /// <summary>
    /// Emails CSV of device groups peripheral summary listing.
    /// </summary>
    /// <remarks>
    /// This API emails peripheral listing summary of device groups based upon filter criteria and configured peripheral columns to targeted recipient as a CSV file.
    /// Supported Header Fields (Add each input as element in Array): <br/>
    /// "Name", "Manufacturer", "Status", "Version", "PeripheralType", "DeviceCount".
    /// <br/>
    /// Requires the caller be granted "Manage Peripherals" and "Email CSV" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2027.0.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="path">The device group path.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <param name="parameters">Parameters required for dispatching email.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract Validation Failed.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Violated logical condition.<br/>The following ErrorCode values can be returned:.<br/>
    /// <ol>
    /// <li>[6021] : Failed to send email.</li>
    /// </ol>
    /// </response>
    [HttpPost]
    [Route("{path}/peripherals/{deviceFamily}/actions/emailReport")]
    [RegisterOperation(OperationCode = "EmailPeripheralsReport")]
    [RequiresSecurityPermission("ManagePeripherals", "EmailCSV")]
    public async Task EmailPeripheralsReport([FromRoute] string path, [FromRoute] DeviceFamily deviceFamily, [FromBody] PeripheralEmailReportParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException(nameof(path));
        }

        if (!Enum.IsDefined(typeof(DeviceFamily), deviceFamily))
        {
            throw new ValidationException($"Device Family '{deviceFamily}' is invalid.");
        }

        if (parameters == null)
        {
            throw new ValidationException("Requested Parameter cannot be null.");
        }
        var groupId = _deviceGroupIdentityMapper.GetId(path);
        CheckForDeviceGroupPermission(groupId);

        ValidateReportHeaderFields(parameters.ReportHeaderFields);
        var statuses = ValidatePeripheralStatus(parameters.Statuses);
        var peripheralTypes = ValidatePeripheralType(parameters.PeripheralType);
        var connection = ValidateAndGetEmailConnectionInfo(parameters.EmailProfileName);
        var recipients = ValidateAndGetRecipients(connection.EmailServerId ?? 0, parameters);

        var peripheralSummaries = GetDeviceGroupPeripheralSummaries(path, deviceFamily);
        var deviceGroupsPeripheralSummaries = CalculateDeviceCountAndReturnPeripheralSummary(peripheralSummaries);

        var filteredPeripheralSummaries = FilterPeripheralSummaries(deviceGroupsPeripheralSummaries.ToList(), parameters.NameContains, statuses, peripheralTypes, new DataRetrievalOptions { Take = int.MaxValue }, out _);
        var peripheralList = filteredPeripheralSummaries.Select(report => DeviceGroupsPeripheralHeaderConverter.ToExportableDictionary(report)).ToArray();

        using (var attachmentStream = new MemoryStream())
        {
            await _csvConverter.GenerateCsvContent(peripheralList, parameters.ReportHeaderFields, attachmentStream, parameters.TimeOffset ?? 0, _cultureInfo, LocalizePropertyName, false);
            attachmentStream.Position = 0;
            var attachment = new Attachment
            {
                Data = attachmentStream,
                Name = GetReportFileName(),
                MediaType = MediaTypeNames.Application.Octet
            };

            DispatchEmail(connection, recipients, parameters, attachment, groupId);
        }
    }

    private static void EnsureDevicePeripheralStatus(IEnumerable<DevicePeripheralStatus> statuses)
    {
        if (statuses == null)
        {
            return;
        }

        foreach (var status in statuses)
        {
            if (!Enum.IsDefined(typeof(DevicePeripheralStatus), status))
            {
                throw new ValidationException($"Device peripheral status '{status}' is invalid.");
            }
        }
    }

    private void CheckForDeviceGroupPermission(int deviceGroupId)
    {
        _accessControlManager.EnsureHasAccessRight(DeviceGroupPermission.ViewGroup, SecurityAssetType.DeviceGroup, deviceGroupId);
    }

    private IReadOnlyCollection<int> GetDescendantGroupIds(string deviceGroupPath)
    {
        var deviceGroupIds = _accessibleDeviceGroupService.GetPermittedDeviceGroupIds(deviceGroupPath, true);

        return deviceGroupIds.ToList();
    }

    private string LocalizePropertyName(string propertyName)
    {
        if (propertyName == null)
        {
            return null;
        }

        var resourceKey = $"lbl_device_group_peripheral_property_{propertyName}";
        var localizedHeader = _resourceManager.GetString(resourceKey, Thread.CurrentThread.CurrentCulture);
        return localizedHeader ?? propertyName;
    }

    private string GetReportFileName()
    {
        return $"{DeviceGroupsPeripheralReportFilePrefix}{_timeSupplier.GetUtcNow():yyyy-MM-dd HH_mm_ss}.csv";
    }

    private IEnumerable<DeviceGroupPeripheralSummary> CalculateDeviceCountAndReturnPeripheralSummary(IReadOnlyList<Models.DeviceGroupPeripheralSummary> deviceGroupPeripheralSummaries)
    {
        return deviceGroupPeripheralSummaries
            .GroupBy(d => new { d.PeripheralId, d.Status })
            .Select(g => new DeviceGroupPeripheralSummary
            {
                Name = g.First().Name,
                Manufacturer = g.First().Manufacturer,
                Version = g.First().Version,
                PeripheralType = (PeripheralType)g.First().PeripheralType,
                Status = (DevicePeripheralStatus)g.Key.Status,
                DeviceCount = g.Select(d => d.DeviceId).Distinct().Count()
            });
    }

    private IEnumerable<DeviceGroupsPeripheralHeaders> FilterPeripheralSummaries(
        IEnumerable<DeviceGroupPeripheralSummary> deviceGroupPeripheralSummaries,
         string nameContains,
        DevicePeripheralStatus[] statuses,
        PeripheralType[] peripheralTypes,
        DataRetrievalOptions dataRetrievalOptions,
        out int totalCount)
    {
        var deviceGroupPeripheralList = deviceGroupPeripheralSummaries.ToList();
        if (!deviceGroupPeripheralList.Any() || deviceGroupPeripheralList.Count == 0)
        {
            totalCount = 0;
            return new List<DeviceGroupsPeripheralHeaders>();
        }

        if (!string.IsNullOrEmpty(nameContains))
        {
            deviceGroupPeripheralList = deviceGroupPeripheralList.Where(e => e.Name.Contains(nameContains, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        var peripheralStatusList = statuses?.ToList() ?? new List<DevicePeripheralStatus>();
        if (peripheralStatusList.Any())
        {
            deviceGroupPeripheralList = deviceGroupPeripheralList.Where(summary => peripheralStatusList.Contains(summary.Status)).ToList();
        }

        var peripheralTypeList = peripheralTypes?.ToList() ?? new List<PeripheralType>();
        if (peripheralTypeList.Any())
        {
            deviceGroupPeripheralList = deviceGroupPeripheralList.Where(summary => peripheralTypeList.Contains(summary.PeripheralType)).ToList();
        }

        totalCount = deviceGroupPeripheralList.Count();
        deviceGroupPeripheralList = DeviceGroupPeripheralOrderAndPagination.ApplyDataRetrievalOptions(deviceGroupPeripheralList, dataRetrievalOptions).ToList();
        return deviceGroupPeripheralList.Select(peripheralSummary => peripheralSummary.ToHeader());
    }

    private static IReadOnlyDictionary<string, TEnum> CreateEnumDictionary<TEnum>()
        where TEnum : struct, Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .ToDictionary(e => e.ToString(), e => e);
    }

    private static DevicePeripheralStatus[] ValidatePeripheralStatus(string[] statuses)
    {
        var peripheralStatusDict = CreateEnumDictionary<DevicePeripheralStatus>();
        return statuses?.Select(status =>
        {
            if (Enum.TryParse<DevicePeripheralStatus>(status, true, out var validStatus))
            {
                return validStatus;
            }
            throw new ValidationException($"Peripheral status '{status}' is invalid.");
        }).ToArray();
    }

    private static PeripheralType[] ValidatePeripheralType(string[] peripheralTypes)
    {
        var peripheralTypesDict = CreateEnumDictionary<PeripheralType>();
        return peripheralTypes?.Select(peripheralType =>
        {
            if (Enum.TryParse<PeripheralType>(peripheralType, true, out var validPeripheralType))
            {
                return validPeripheralType;
            }
            throw new ValidationException($"Peripheral type '{peripheralType}' is invalid.");
        }).ToArray();
    }

    private List<EmailRecipient> ValidateAndGetRecipients(int emailServerId, PeripheralEmailReportParameters parameters)
    {
        var recipients = new List<EmailRecipient>();
        if (parameters.AppendAddresses)
        {
            var defaultRecipients = _emailConfigurationService.GetDefaultRecipients(emailServerId);
            if (defaultRecipients?.Recipients != null)
            {
                recipients.AddRange(defaultRecipients.Recipients);
            }
        }

        recipients.AddRange(GetEmailRecipients(parameters.ToAddresses, EmailAddresseeType.To));
        recipients.AddRange(GetEmailRecipients(parameters.CcAddresses, EmailAddresseeType.CarbonCopy));
        recipients.AddRange(GetEmailRecipients(parameters.BccAddresses, EmailAddresseeType.BlindCarbonCopy));

        if (!recipients.Any())
        {
            throw new ValidationException("No destination email addresses specified.");
        }

        foreach (var recipient in recipients)
        {
            if (!EmailValidation.IsEmailValid(recipient.Email))
            {
                throw new ValidationException($"Invalid email address specified '{recipient.Email}'.");
            }
        }

        return recipients;
    }

    private static IEnumerable<EmailRecipient> GetEmailRecipients(IEnumerable<string> emailAddress, EmailAddresseeType emailAddresseeType)
    {
        var recipients = new List<EmailRecipient>();
        if (emailAddress != null)
        {
            recipients.AddRange(
                emailAddress.Select(
                    email => new EmailRecipient
                    {
                        AddresseeType = emailAddresseeType,
                        Email = email
                    }));
        }

        return recipients;
    }

    private void DispatchEmail(EmailServerConnection connection, List<EmailRecipient> recipients, PeripheralEmailReportParameters parameters, Attachment attachment, int groupId)
    {
        var emailMessage = new EmailMessage
        {
            Subject = parameters.EmailSubject,
            TextBody = parameters.EmailBody,
            Priority = EmailPriority.Normal,
            Recipients = recipients.ToArray(),
            Attachment = attachment
        };

        _emailNotificationService.SendImmediateEmailAndThrowExceptionOnFailure(connection, emailMessage);

        var emails = string.Join(", ", recipients.Select(f => f.Email));

        _eventDispatcher.DispatchEvent(new PeripheralEmailReportSuccessEvent(emails, CurrentUserId, CurrentUserName, groupId));
    }

    private EmailServerConnection ValidateAndGetEmailConnectionInfo(string emailProfileName)
    {
        if (string.IsNullOrWhiteSpace(emailProfileName))
        {
            throw new ValidationException("Email Profile Name cannot be null.");
        }

        var connection = _emailConfigurationService.GetByName(emailProfileName);
        if (connection?.EmailServerId == null || connection.EmailServerId.Value <= 0)
        {
            throw new SecurityException($"SMTP connection with Name '{emailProfileName}' is not accessible or does not have a server Id.");
        }

        return connection;
    }

    private static void ValidateReportHeaderFields(string[] inputHeaderFieldNames)
    {
        if (inputHeaderFieldNames == null || !inputHeaderFieldNames.Any())
        {
            throw new ValidationException("Report Header Fields must be specified.");
        }

        var fields = typeof(DeviceGroupsPeripheralHeaders)
            .GetProperties()
            .Select(f => f.Name);

        var isHeaderField = inputHeaderFieldNames.All(f => fields.Contains(f, StringComparer.OrdinalIgnoreCase));
        if (!isHeaderField)
        {
            throw new ValidationException("Report Header Fields have invalid values.");
        }
    }

    private IReadOnlyList<Models.DeviceGroupPeripheralSummary> GetDeviceGroupPeripheralSummaries(string path, DeviceFamily deviceFamily)
    {
        return deviceFamily == DeviceFamily.All ?
            _deviceGroupPeripheralService.GetDeviceGroupsPeripheralSummary(GetDescendantGroupIds(path))
            : _deviceGroupPeripheralService.GetPeripheralSummaryByFamilyIdAndGroupIds((int)deviceFamily, GetDescendantGroupIds(path));
    }
}
