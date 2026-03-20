using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Soti.Diagnostics;
using Soti.MobiControl.Settings;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Middleware;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.GroupPolicy;
using Soti.MobiControl.WindowsModern.Implementation.Middleware;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <inheritdoc />
internal sealed class WindowsDeviceConfigurationsService : IWindowsDeviceConfigurationsService
{
    private const string SotiServicesBaseUrlSettingName = "SotiServicesBaseUrl";
    private const string GpoSettingsFileName = "GroupPolicyObjectsSettings_v1";
    private const string WindowsProductVersionsFileName = "WindowsProductVersions";
    private const string RegistrationCodeSettingName = "regcode";
    private const string InstallationIdSettingName = "InstallationId";
    private const string WindowsDeviceMatchReEnrollmentSettingName = "WindowsDeviceMatchReenrollment.Options";
    private const string WindowsGroupPolicyObjectsSettingName = "WindowsModernGroupPolicyObjectsSettings";
    private const string WindowsUpdatesPolicyFeatureName = "WindowsUpdatePolicies";
    private const string WindowsProductVersionsSettingName = "ProductVersions";
    private const string SharedFilesEndPointString = "/SharedFiles/";
    private readonly string _sharedFilesUri;
    private readonly ISettingsManagementService _settingsManagementService;
    private readonly IProgramTrace _programTrace;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDeviceConfigurationsService"/> class.
    /// </summary>
    /// <param name="settingsManagementService">The instance of ISettingsManagementService.</param>
    /// <param name="programTrace">The instance of IProgramTrace.</param>
    /// <param name="httpClientProvider">The instance of IHttpClientProvider.</param>
    public WindowsDeviceConfigurationsService(ISettingsManagementService settingsManagementService,
        IProgramTrace programTrace,
        IHttpClientProvider httpClientProvider)
    {
        _settingsManagementService = settingsManagementService;
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _httpClient = httpClientProvider?.Get() ?? throw new ArgumentNullException(nameof(httpClientProvider));
        _sharedFilesUri = GetSotiServicesBaseUrl() + SharedFilesEndPointString;
    }

    /// <inheritdoc />
    public void UpdateWindowsReEnrollmentCriteria(WindowsReEnrollmentCriteria options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        _settingsManagementService.SaveSetting(
            string.Empty,
            WindowsDeviceMatchReEnrollmentSettingName,
            options);
    }

    public WindowsReEnrollmentCriteria GetWindowsReEnrollmentCriteria()
    {
        var setting = _settingsManagementService.GetSetting<WindowsReEnrollmentCriteria>(string.Empty, WindowsDeviceMatchReEnrollmentSettingName);

        return setting ?? new WindowsReEnrollmentCriteria
        {
            HardwareId = false,
            MacAddress = false
        };
    }

    public bool CheckForStrictReEnrollment()
    {
        var reenrollmentToggle = GetWindowsReEnrollmentCriteria();

        return reenrollmentToggle.HardwareId && reenrollmentToggle.MacAddress;
    }

    /// <inheritdoc />
    public WindowsModernGroupPolicyObjectsSettings GetGroupPolicySettings()
    {
        return _settingsManagementService.GetSetting<WindowsModernGroupPolicyObjectsSettings>(string.Empty, WindowsGroupPolicyObjectsSettingName);
    }

    /// <inheritdoc />
    public async Task UpdateGroupPolicySettings()
    {
        var currentSettings = GetGroupPolicySettings();

        if (currentSettings == null)
        {
            throw new ArgumentNullException(nameof(currentSettings));
        }

        var settings = await GetSettings(_sharedFilesUri, GpoSettingsFileName);
        var newSettings = settings.ToModel<WindowsModernGroupPolicyObjectsSettings>();

        if (newSettings != null && newSettings.Version > currentSettings.Version)
        {
            _settingsManagementService.SaveSetting(string.Empty, WindowsGroupPolicyObjectsSettingName, newSettings);
        }
    }

    public async Task UpdateWindowsUpdatesPolicyProductVersionSettings()
    {
        var settings = await GetSettings(_sharedFilesUri, WindowsProductVersionsFileName);
        _settingsManagementService.SaveSetting<IEnumerable<WindowsProductVersion>>(WindowsUpdatesPolicyFeatureName, WindowsProductVersionsSettingName, settings.ToModel<WindowsProductVersion[]>());
    }

    private AuthenticationSettings GetAuthenticationSettings()
    {
        return new AuthenticationSettings
        {
            RegistrationCode = _settingsManagementService.GetSetting<string>(string.Empty, RegistrationCodeSettingName),
            InstallationId = _settingsManagementService.GetSetting<string>(string.Empty, InstallationIdSettingName)
        };
    }

    private async Task<string> GetSettings(string uri, string fileName)
    {
        var authenticationSettings = GetAuthenticationSettings();

        var settings = await _httpClient.GetSettingsAsync(authenticationSettings, uri + fileName);

        if (string.IsNullOrWhiteSpace(settings))
        {
            throw new InvalidDataException("The response from SOTI Service was either null or empty.");
        }

        return settings;
    }

    private string GetSotiServicesBaseUrl()
    {
        return _settingsManagementService.GetSetting<string>(string.Empty, SotiServicesBaseUrlSettingName);
    }
}
