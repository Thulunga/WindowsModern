using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soti.Logging.MobiControl;
using Soti.Logging.MobiControl.Enums;
using Soti.MobiControl.Devices.DevInfo;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.WebApi.Foundation.Host;
using Soti.MobiControl.WebApi.Foundation.OperationTracking;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Windows Modern DevInfo UI Controller.
/// </summary>
[Authorize]
[Route("windowsModern/devInfo")]
[ApiController]
[HostingTarget(Target = HostingTarget.UiApi)]
[FeatureScope(LogFeature.FirmwareManagement)]
public class WindowsDevInfoUiController : ControllerBase, IWindowsDevInfoUiController
{
    private readonly IDevInfoService _devInfoService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDevInfoUiController" /> class.
    /// </summary>
    /// <param name="devInfoService">The Dev Info Service.</param>
    public WindowsDevInfoUiController(IDevInfoService devInfoService)
    {
        _devInfoService = devInfoService ?? throw new ArgumentNullException(nameof(_devInfoService));
    }

    /// <summary>
    /// Returns model list based on manufacturer.
    /// </summary>
    /// <remarks>
    /// Requires the caller be granted the "DeviceControl" permission.
    /// <br/>
    /// <b>(Available Since MobiControl v2026.1.0)</b>
    /// <br/>
    /// </remarks>
    /// <param name="manufacturer">Manufacturer of the Device.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Contract validation failed.</response>
    /// <response code="403">Forbidden.</response>
    /// <returns>List of Models.</returns>
    [HttpGet]
    [Route("{manufacturer}/models")]
    [RequiresSecurityPermission("DeviceControl")]
    [RegisterOperation(OperationCode = "GetModelsByManufacturer")]
    public IEnumerable<string> GetModelsByManufacturer([FromRoute] string manufacturer)
    {
        if (string.IsNullOrWhiteSpace(manufacturer))
        {
            throw new ValidationException(nameof(manufacturer));
        }

        if (!Enum.TryParse(typeof(Manufacturer), manufacturer, out var manufacturerEnum))
        {
            throw new ValidationException($"Manufacturer '{manufacturer}' is invalid.");
        }

        return _devInfoService.GetModelsByManufacturer((Manufacturer)manufacturerEnum);
    }
}
