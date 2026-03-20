using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soti.Logging.MobiControl;
using Soti.Logging.MobiControl.Enums;
using Soti.MobiControl.DeviceGroupManagement;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Management.Services;
using Soti.MobiControl.WebApi.Foundation.Host;
using Soti.MobiControl.WebApi.Foundation.OperationTracking;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Windows Modern Device Groups UI Controller.
/// </summary>
[Authorize]
[Route("windows/devicegroups")]
[ApiController]
[HostingTarget(Target = HostingTarget.UiApi)]
[FeatureScope(LogFeature.Device)]
public class WindowsDeviceGroupsUiController : ControllerBase, IWindowsDeviceGroupsUiController
{
    private readonly Lazy<IDeviceGroupIdentityMapper> _deviceGroupIdentityMapper;
    private readonly Lazy<IAccessibleDeviceGroupService> _accessibleDeviceGroupService;
    private readonly IAccessControlManager _accessControlManager;
    private readonly Lazy<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDeviceGroupsUiController"/> class.
    /// </summary>
    /// <param name="deviceGroupIdentityMapper">The device group identity mapper.</param>
    /// <param name="accessControlManager">The access control manager.</param>
    /// <param name="accessibleDeviceGroupService">The accessible device group service.</param>
    /// <param name="windowsDeviceLocalGroupsService">The windows device local group service.</param>
    public WindowsDeviceGroupsUiController(
        Lazy<IDeviceGroupIdentityMapper> deviceGroupIdentityMapper,
        IAccessControlManager accessControlManager,
        Lazy<IAccessibleDeviceGroupService> accessibleDeviceGroupService,
        Lazy<IWindowsDeviceLocalGroupsService> windowsDeviceLocalGroupsService)
    {
        _deviceGroupIdentityMapper = deviceGroupIdentityMapper ?? throw new ArgumentNullException(nameof(deviceGroupIdentityMapper));
        _accessControlManager = accessControlManager ?? throw new ArgumentNullException(nameof(accessControlManager));
        _accessibleDeviceGroupService = accessibleDeviceGroupService ?? throw new ArgumentNullException(nameof(accessibleDeviceGroupService));
        _windowsDeviceLocalGroupsService = windowsDeviceLocalGroupsService ?? throw new ArgumentNullException(nameof(windowsDeviceLocalGroupsService));
    }

    /// <summary>
    /// Returns a list of local groups across all Windows devices in the given device group.
    /// </summary>
    /// <param name="path">The reference ID or the path of the device group.</param>
    /// <returns>List of local groups.</returns>
    [HttpGet]
    [Route("{path}/localGroups")]
    [RegisterOperation(OperationCode = "WindowsModern_GetDeviceGroupLocalGroups")]
    [RequiresSecurityPermission("DeviceControl")]
    public IEnumerable<string> GetDeviceGroupLocalGroups(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException("Path cannot be null or empty.");
        }

        _accessControlManager.EnsureHasAccessRight(
            DeviceGroupPermission.ViewGroup,
            SecurityAssetType.DeviceGroup,
            _deviceGroupIdentityMapper.Value.GetId(path));

        return _windowsDeviceLocalGroupsService.Value
            .GetDeviceGroupsLocalGroups(_accessibleDeviceGroupService.Value.GetPermittedDeviceGroupIds(path, true));
    }
}
