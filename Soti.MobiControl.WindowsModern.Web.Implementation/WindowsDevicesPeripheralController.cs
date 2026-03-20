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
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.WebApi.Foundation.Host;
using Soti.MobiControl.WebApi.Foundation.OperationTracking;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Windows Modern Device Peripheral controller.
/// </summary>
[Authorize]
[Route("windows/devices")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[HostingTarget(Target = HostingTarget.PublicApi)]
[FeatureScope(LogFeature.Device)]
public sealed class WindowsDevicesPeripheralController : ControllerBase, IWindowsDevicesPeripheralController
{
    private const int DefaultUserId = 1;

    private readonly IDeviceKeyInformationRetrievalService _deviceKeyInformationRetrievalService;
    private readonly IAccessControlManager _accessControlManager;
    private readonly IWindowsDevicePeripheralService _devicePeripheralService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDevicesPeripheralController"/> class.
    /// </summary>
    /// <param name="deviceKeyInformationRetrievalService">The device key information retrieval service.</param>
    /// <param name="accessControlManager">The access control manager.</param>
    /// <param name="devicePeripheralService">The device peripheral service.</param>
    public WindowsDevicesPeripheralController(
        IDeviceKeyInformationRetrievalService deviceKeyInformationRetrievalService,
        IAccessControlManager accessControlManager,
        IWindowsDevicePeripheralService devicePeripheralService)
    {
        _deviceKeyInformationRetrievalService = deviceKeyInformationRetrievalService ?? throw new ArgumentNullException(nameof(deviceKeyInformationRetrievalService));
        _accessControlManager = accessControlManager ?? throw new ArgumentNullException(nameof(accessControlManager));
        _devicePeripheralService = devicePeripheralService ?? throw new ArgumentNullException(nameof(devicePeripheralService));
    }

    /// <summary>
    /// Returns a list of peripherals.
    /// </summary>
    /// <remarks>
    /// This API returns a list of peripherals of a device
    /// <br/>
    /// Requires the caller be granted  "DeviceControl" and "Manage and Move Devices" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2025.1.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    [HttpGet]
    [Route("{deviceId}/peripherals")]
    [RegisterOperation(OperationCode = "GetWindowsDevicePeripherals")]
    [RequiresSecurityPermission("DeviceControl")]
    public IEnumerable<DevicePeripheralSummary> GetDevicePeripherals(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ValidationException("DeviceId must be specified.");
        }

        var device = GetDeviceInformation(deviceId);

        CheckForDevicePermissions(device.DeviceId);

        var devicePeripherals = _devicePeripheralService.GetDevicePeripheralsSummaryInfo(device.DeviceId);
        return devicePeripherals.Select(x => x.ToDevicePeripheralSummary());
    }

    private DeviceKeyInformation GetDeviceInformation(string deviceId)
    {
        var device = _deviceKeyInformationRetrievalService.GetDeviceKeyInformation(deviceId);

        if (device == null)
        {
            throw new SecurityException($"Unable to get deviceId: {deviceId} in DB.");
        }

        if (device.DevicePlatform != DevicePlatform.WindowsDesktop10RS1 && device.DevicePlatform != DevicePlatform.WindowsDesktop10)
        {
            throw new ValidationException($"The supplied device id '{deviceId}' belongs to a non-supported device platform");
        }

        return device;
    }

    private void CheckForDevicePermissions(int deviceId)
    {
        _accessControlManager.EnsureHasAccessRight(
            DeviceGroupPermission.ViewGroup,
            SecurityAssetType.Device, deviceId);
        _accessControlManager.EnsureHasAccessRight(
            DevicePermission.ManageAndMoveDevices,
            SecurityAssetType.Device,
            deviceId);
    }
}
