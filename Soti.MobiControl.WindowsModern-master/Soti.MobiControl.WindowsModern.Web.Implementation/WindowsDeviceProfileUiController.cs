using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soti.Logging.MobiControl;
using Soti.Logging.MobiControl.Enums;
using Soti.MobiControl.AppPolicy.Windows.Model.Microsoft;
using Soti.MobiControl.AppPolicy.Windows.Services;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.WebApi.Foundation.Host;
using Soti.MobiControl.WebApi.Foundation.OperationTracking;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Contracts.GroupPolicy;
using Soti.MobiControl.WindowsModern.Web.Enums;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Windows Modern Profile UI Controller.
/// </summary>
[Authorize]
[Route("windowsModern/profiles")]
[ApiController]
[HostingTarget(Target = HostingTarget.UiApi)]
[FeatureScope(LogFeature.Profiles)]
public sealed class WindowsDeviceProfileUiController : ControllerBase, IWindowsDeviceProfileUiController
{
    private readonly IAssignedAccessApplicationService _assignedAccessApplicationService;
    private readonly IWindowsDeviceConfigurationsService _windowsDeviceConfigurationsService;

    /// <summary>
    /// UI Controller for Windows Modern Profile.
    /// </summary>
    public WindowsDeviceProfileUiController(
        IAssignedAccessApplicationService assignedAccessApplicationService,
        IWindowsDeviceConfigurationsService windowsDeviceConfigurationsService)
    {
        _assignedAccessApplicationService = assignedAccessApplicationService ?? throw new ArgumentNullException(nameof(assignedAccessApplicationService));
        _windowsDeviceConfigurationsService = windowsDeviceConfigurationsService ?? throw new ArgumentNullException(nameof(windowsDeviceConfigurationsService));
    }

    /// <inheritdoc />
    [HttpGet]
    [Route("assignedAccess/apps")]
    [RegisterOperation(OperationCode = "RequestAssignedAccessApplications")]
    [RequiresSecurityPermission("ManageProfileSetup")]
    public IEnumerable<AssignedAccessApplication> RequestAssignedAccessApplications()
    {
        var uwpApplications = _assignedAccessApplicationService
            .GetAssignedAccessConfigurationModernApplications(
                PlatformTypes.Windows8x | PlatformTypes.WindowsDesktop,
                new[] { PackageFormatType.Appx, PackageFormatType.Msix, PackageFormatType.AppxBundle, PackageFormatType.MsixBundle },
                ProcessorArchitectures.Neutral)
            .Select(appData =>
                new AssignedAccessApplication
                {
                    AppType = ApplicationType.UWP,
                    AppIdPath = appData.AppUserModelId,
                    AppName = appData.ApplicationName,
                })
            .OrderBy(app => app.AppName);

        var desktopApplications = _assignedAccessApplicationService
            .GetAssignedAccessConfigurationClassicDesktopApplications()
            .Where(appData => appData.ApplicationPath != null && appData.ApplicationPath != "*")
            .Select(appData =>
                new AssignedAccessApplication
                {
                    AppType = ApplicationType.Desktop,
                    AppName = appData.ApplicationName,
                    AppIdPath = appData.ApplicationPath
                })
            .OrderBy(app => app.AppName);

        return uwpApplications.Concat(desktopApplications);
    }

    /// <summary>
    /// Retrieves List of Group Policy Settings.
    /// </summary>
    /// <remarks>
    /// This API returns list of settings used in Group Policy Objects payload.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "ManageProfileSetup" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2025.1.0)</b>
    /// <br/>
    /// </remarks>
    /// <response code="200">Success.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    [HttpGet]
    [Route("groupPolicyObjects/settings")]
    [RegisterOperation(OperationCode = "RequestGroupPolicySettings")]
    [RequiresSecurityPermission("ManageProfileSetup")]
    public WindowsModernGroupPolicyObjectsSettings RequestGroupPolicySettings()
    {
        return _windowsDeviceConfigurationsService.GetGroupPolicySettings().ToWindowsModernGroupPolicySettingsContract();
    }
}
