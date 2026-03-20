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
using HardwareStatus = Soti.MobiControl.WindowsModern.Web.Enums.HardwareStatus;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Device Hardware Controller.
/// </summary>
[Authorize]
[Route("windows/devices")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[HostingTarget(Target = HostingTarget.PublicApi)]
[FeatureScope(LogFeature.Device)]
public sealed class DeviceHardwareController : ControllerBase, IDeviceHardwareController
{
    private const int DefaultUserId = 1;
    private readonly IDeviceKeyInformationRetrievalService _deviceKeyInformationRetrievalService;
    private readonly IAccessControlManager _accessControlManager;
    private readonly IDeviceHardwareService _deviceHardwareService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceHardwareController"/> class.
    /// </summary>
    /// <param name="deviceKeyInformationRetrievalService">The device key information retrieval service.</param>
    /// <param name="accessControlManager">The access control manager.</param>
    /// <param name="deviceHardwareService">The device hardware service.</param>
    public DeviceHardwareController(
        IDeviceKeyInformationRetrievalService deviceKeyInformationRetrievalService,
        IAccessControlManager accessControlManager,
        IDeviceHardwareService deviceHardwareService)
    {
        _deviceKeyInformationRetrievalService = deviceKeyInformationRetrievalService ?? throw new ArgumentNullException(nameof(deviceKeyInformationRetrievalService));
        _accessControlManager = accessControlManager ?? throw new ArgumentNullException(nameof(accessControlManager));
        _deviceHardwareService = deviceHardwareService ?? throw new ArgumentNullException(nameof(deviceHardwareService));
    }

    /// <summary>
    /// Returns a list of device hardware data
    /// </summary>
    /// <remarks>
    /// This API returns a list of device hardware data existing on a device.
    /// <br/>
    /// Requires the caller be granted "View Groups", "DeviceControl" and "Manage Devices" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2025.1.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract Validation Failed.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    [HttpGet]
    [Route("{deviceId}/deviceHardware")]
    [RegisterOperation(OperationCode = "GetWindowsDeviceHardware")]
    [RequiresSecurityPermission("DeviceControl")]
    public IEnumerable<DeviceHardwareSummary> GetDeviceHardware(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ValidationException($"{nameof(deviceId)} is required and cannot be empty or whitespace.");
        }

        var device = GetDeviceInformation(deviceId);
        CheckForDevicePermissions(device.DeviceId);

        var deviceHardwares = _deviceHardwareService.GetAllDeviceHardwareStatusSummaryByDeviceId(device.DeviceId);
        return deviceHardwares.Select(DeviceHardwareConverter.ToDeviceHardwareSummary);
    }

    /// <summary>
    /// Updates the device hardware status based on the hardware serial number
    /// </summary>
    /// <remarks>
    /// This API updates the device hardware status based on the hardware serial number.
    /// <br/>
    /// <br/>
    /// Requires the caller be granted "DeviceControl" and "Manage Devices" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2025.1.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="hardwareStatusSummary">The hardware status summary.</param>
    /// <response code="204">Success.</response>
    /// <response code="400">Contract Validation Failed.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="422">Business Logic Exception.
    /// <br/>The following ErrorCode values can be returned.<br/>
    /// <ol>
    /// <li>2100 - No device hardware exists for the specified hardware serial number.</li>
    /// <li>2101 - Unable to update the hardware status for the specified hardware serial number.</li>
    /// </ol>
    /// </response>
    [HttpPut]
    [Route("{deviceId}/deviceHardwareStatus")]
    [RegisterOperation(OperationCode = "UpdateWindowsDeviceHardwareStatus")]
    [RequiresSecurityPermission("DeviceControl")]
    public void UpdateDeviceHardwareStatus(string deviceId, [FromBody] DeviceHardwareStatusSumary hardwareStatusSummary)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ValidationException($"{nameof(deviceId)} is required and cannot be empty or whitespace.");
        }

        if (hardwareStatusSummary == null)
        {
            throw new ValidationException($"{nameof(hardwareStatusSummary)} is required and cannot be null.");
        }

        if (!Enum.IsDefined(typeof(HardwareStatus), hardwareStatusSummary.HardwareStatus))
        {
            throw new ValidationException($"Invalid value for {nameof(hardwareStatusSummary.HardwareStatus)}.");
        }

        if (string.IsNullOrWhiteSpace(hardwareStatusSummary.HardwareSerialNumber))
        {
            throw new ValidationException($"{nameof(hardwareStatusSummary.HardwareSerialNumber)} is required and cannot be empty or whitespace.");
        }

        var device = GetDeviceInformation(deviceId);
        CheckForDeviceManagementPermission(device.DeviceId);

        _deviceHardwareService.UpdateDeviceHardwareStatus(
            device.DeviceId,
            deviceId,
            (Models.Enums.HardwareStatus)hardwareStatusSummary.HardwareStatus,
            hardwareStatusSummary.HardwareSerialNumber);
    }

    private DeviceKeyInformation GetDeviceInformation(string deviceId)
    {
        var device = _deviceKeyInformationRetrievalService.GetDeviceKeyInformation(deviceId) ?? throw new SecurityException($"Unable to get deviceId: {deviceId} in DB.");
        return device.DevicePlatform is not DevicePlatform.WindowsDesktop10RS1 and not DevicePlatform.WindowsDesktop10
            ? throw new SecurityException($"The supplied device id '{deviceId}' belongs to a non-supported device platform")
            : device;
    }

    private void CheckForDevicePermissions(int deviceId)
    {
        CheckForDeviceGroupViewPermission(deviceId);
        CheckForDeviceManagementPermission(deviceId);
    }

    private void CheckForDeviceGroupViewPermission(int deviceId)
    {
        _accessControlManager.EnsureHasAccessRight(DeviceGroupPermission.ViewGroup, SecurityAssetType.Device, deviceId);
    }

    private void CheckForDeviceManagementPermission(int deviceId)
    {
        _accessControlManager.EnsureHasAccessRight(
            DevicePermission.ManageAndMoveDevices,
            SecurityAssetType.Device,
            deviceId);
    }
}
