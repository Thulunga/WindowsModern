using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soti.Logging.MobiControl;
using Soti.Logging.MobiControl.Enums;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Devices.DevInfo;
using Soti.MobiControl.Events;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Identity;
using Soti.MobiControl.WebApi.Foundation.Host;
using Soti.MobiControl.WebApi.Foundation.OperationTracking;
using Soti.MobiControl.WindowsModern.Web.Implementation.Events;
using Soti.MobiControl.WindowsModern.Web.Implementation.Exceptions;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Windows Modern Device Action UI Controller.
/// </summary>
[Authorize]
[Route("windows/devices")]
[ApiController]
[HostingTarget(Target = HostingTarget.UiApi)]
[FeatureScope(LogFeature.Device)]
public class WindowsDeviceActionUiController : ControllerBase, IWindowsDeviceActionUiController
{
    private readonly IDeviceKeyInformationRetrievalService _deviceKeyInformationRetrievalService;
    private readonly IUserIdentityProvider _userIdentityProvider;
    private readonly IAccessControlManager _accessControlManager;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IWindowsDeviceService _windowsDeviceService;
    private readonly IDevInfoService _devInfoService;
    private readonly Lazy<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsService;

    /// <summary>
    /// UI Controller for Windows Device Actions.
    /// </summary>
    /// <param name="deviceKeyInformationRetrievalService">The device key information retrieval service.</param>
    /// <param name="userIdentityProvider">The user identity provider.</param>
    /// <param name="accessControlManager">The access control manager.</param>
    /// <param name="eventDispatcher">The event dispatcher.</param>
    /// <param name="windowsDeviceService">Windows device service.</param>
    /// <param name="devInfoService">The dev info service.</param>
    /// <param name="windowsDeviceLocalGroupsService">The windows device local group service.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public WindowsDeviceActionUiController(
        IDeviceKeyInformationRetrievalService deviceKeyInformationRetrievalService,
        IUserIdentityProvider userIdentityProvider,
        IAccessControlManager accessControlManager,
        IEventDispatcher eventDispatcher,
        IWindowsDeviceService windowsDeviceService,
        IDevInfoService devInfoService,
        Lazy<IWindowsDeviceLocalGroupsService> windowsDeviceLocalGroupsService)
    {
        _deviceKeyInformationRetrievalService = deviceKeyInformationRetrievalService ?? throw new ArgumentNullException(nameof(deviceKeyInformationRetrievalService));
        _userIdentityProvider = userIdentityProvider ?? throw new ArgumentNullException(nameof(userIdentityProvider));
        _accessControlManager = accessControlManager ?? throw new ArgumentNullException(nameof(accessControlManager));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _windowsDeviceService = windowsDeviceService ?? throw new ArgumentNullException(nameof(windowsDeviceService));
        _devInfoService = devInfoService ?? throw new ArgumentNullException(nameof(devInfoService));
        _windowsDeviceLocalGroupsService = windowsDeviceLocalGroupsService ?? throw new ArgumentNullException(nameof(windowsDeviceLocalGroupsService));
    }

    private int CurrentUserId => _userIdentityProvider.GetUserIdentity()?.UserId ?? 0;

    private string CurrentUserName => _userIdentityProvider.GetUserIdentity()?.UserName;

    /// <summary>
    /// Retrieves Lock Passcode for a specific Windows Modern Device.
    /// </summary>
    /// <remarks>
    /// This API returns the Lock Passcode for a device.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted the "Web Console" permission and "Disable Passcode Lock" Device permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2024.1.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract Validation Failed.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    [HttpPost]
    [Route("{deviceId}/actions/fetchLockPasscode")]
    [RegisterOperation(OperationCode = "RequestLockPasscode")]
    [RequiresSecurityPermission("DeviceControl")]
    public string RequestLockPasscode([FromRoute] string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ValidationException("Device Id is missing");
        }

        var device = _deviceKeyInformationRetrievalService.GetDeviceKeyInformation(deviceId);

        if (device == null)
        {
            throw new SecurityException($"Unable to get device id {deviceId} from DB.");
        }

        if (device.DevicePlatform != DevicePlatform.WindowsDesktop10 && device.DevicePlatform != DevicePlatform.WindowsDesktop10RS1)
        {
            throw new SecurityException($"The supplied device id {deviceId} belongs to a non-supported device platform.");
        }

        _accessControlManager.EnsureHasAccessRight(DeviceGroupPermission.ViewGroup, SecurityAssetType.Device, device.DeviceId);
        _accessControlManager.EnsureHasAccessRight(DevicePermission.DisablePasscodeLock, SecurityAssetType.Device, device.DeviceId);

        _eventDispatcher.DispatchEvent(new ViewedLockPasscodeEvent(CurrentUserId, CurrentUserName, device.DeviceId, device.DevId));

        return _windowsDeviceService.GetByDeviceId(device.DeviceId)?.Passcode;
    }

    /// <summary>
    /// Returns a list of local groups across given devices.
    /// </summary>
    /// <param name="deviceIdentities">List of deviceIdentities.</param>
    /// <returns>List of local groups.</returns>
    [HttpPost]
    [Route("localGroups")]
    [RegisterOperation(OperationCode = "GetWindowsDevicesLocalGroups")]
    [RequiresSecurityPermission("DeviceControl")]
    [FeatureScope(LogFeature.DeviceAction)]
    public IEnumerable<string> GetDevicesLocalGroups([FromBody] IEnumerable<string> deviceIdentities)
    {
        if (deviceIdentities == null)
        {
            throw new ValidationException("Device Identities cannot be null.");
        }

        var identities = deviceIdentities.Distinct().Select(str => str.Trim()).ToList();
        if (identities.Count == 0)
        {
            throw new ValidationException("Device Identities cannot be empty.");
        }

        var devIds = _devInfoService.FindDeviceIdsByDevIds(identities).Values.ToArray();
        if (devIds.Length != identities.Count)
        {
            throw new SecurityException("Some of the DeviceId/DeviceIds specified does not exist.");
        }

        _accessControlManager.EnsureHasAnyAccessRights(
            new HashSet<SecurityPermission>
            {
                DeviceGroupPermission.ViewGroup
            },
            SecurityAssetType.Device,
            new HashSet<int>(devIds));

        var devices = _devInfoService.GetByDeviceIds(devIds);
        return devices.All(x => x.DeviceKindId != (byte)DeviceKind.WindowsDesktop)
            ? throw WindowsModernWebException.RequireWindowsDesktopDevice()
            : _windowsDeviceLocalGroupsService.Value.GetDevicesLocalGroups(devices.Select(x => x.DeviceId));
    }
}
