using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soti.Logging.MobiControl;
using Soti.Logging.MobiControl.Enums;
using Soti.MobiControl.Events;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Identity;
using Soti.MobiControl.WebApi.Foundation.Host;
using Soti.MobiControl.WebApi.Foundation.OperationTracking;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Web.Implementation.Events;

namespace Soti.MobiControl.WindowsModern.Web.Implementation
{
    /// <summary>
    /// Windows Device Configurations Controller.
    /// </summary>
    [Authorize]
    [Route("configurations/windows/reEnrollment")]
    [ApiController]
    [HostingTarget(Target = HostingTarget.PublicApi)]
    [FeatureScope(LogFeature.GlobalSettings)]
    public sealed class WindowsDeviceConfigurationsController : ControllerBase, IWindowsDeviceConfigurationsController
    {
        private const string EnabledKeyword = "enabled";
        private const string DisabledKeyword = "disabled";
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IWindowsDeviceConfigurationsService _windowsReEnrollmentService;
        private readonly IUserIdentityProvider _userIdentityProvider;

        /// <summary>
        /// Initializes a new instance of the WindowsReEnrollmentController class.
        /// </summary>
        public WindowsDeviceConfigurationsController(
            IEventDispatcher eventDispatcher,
            IWindowsDeviceConfigurationsService windowsReEnrollmentService,
            IUserIdentityProvider userIdentityProvider)
        {
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _windowsReEnrollmentService = windowsReEnrollmentService ?? throw new ArgumentNullException(nameof(windowsReEnrollmentService));
            _userIdentityProvider = userIdentityProvider ?? throw new ArgumentNullException(nameof(userIdentityProvider));
        }

        private int CurrentUserId => _userIdentityProvider.GetUserIdentity()?.UserId ?? 0;

        private string CurrentUserName => _userIdentityProvider.GetUserIdentity()?.UserName;

        /// <summary>
        /// Updates the mechanism to identify a windows device.
        /// </summary>
        /// <remarks>
        /// Updates the mechanism to be used by MobiControl to identify a Windows device during Re-enrollment.
        /// <br />Requires the caller be granted the "Manage Servers and Global Settings" permission.
        /// <br /><b>(Available Since MobiControl v2025.0.0)</b>
        /// </remarks>
        /// <param name="options">contract to define the mechanism for device identification.</param>
        /// <response code="204">Success.</response>
        /// <response code="400">Contract Validation Failed.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpPut]
        [Route("deviceMatchCriteria")]
        [RegisterOperation(OperationCode = "SaveWindowsReEnrollmentCriteria")]
        [RequiresSecurityPermission("GlobalSetting")]
        public void SaveWindowsReEnrollmentCriteria([FromBody] WindowsReEnrollmentCriteria options)
        {
            if (options == null)
            {
                throw new ValidationException("Request should have valid information");
            }

            if (options.MacAddress != options.HardwareId)
            {
                throw new ValidationException("HardwareId and MacAddress should be either both true or both false");
            }

            _windowsReEnrollmentService.UpdateWindowsReEnrollmentCriteria(options.ToReEnrollmentRuleCriteriaModel());

            _eventDispatcher.DispatchEvent(new UpdateReEnrollmentEvent(CurrentUserId, CurrentUserName, EventSeverity.Information, (options.HardwareId && options.MacAddress) ? EnabledKeyword : DisabledKeyword));
        }

        /// <summary>
        /// Gets the mechanism to identify a windows device.
        /// </summary>
        /// <remarks>
        /// Gets the mechanism to be used by MobiControl to identify a Windows device during Re-enrollment.
        /// <br />Requires the caller be granted the "Manage Servers and Global Settings" permission.
        /// <br /><b>(Available Since MobiControl v2025.0.0)</b>
        /// </remarks>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpGet]
        [Route("deviceMatchCriteria")]
        [RegisterOperation(OperationCode = "GetWindowsReEnrollmentCriteria")]
        [RequiresSecurityPermission("GlobalSetting")]
        public WindowsReEnrollmentCriteria GetWindowsReEnrollmentCriteria()
        {
            return _windowsReEnrollmentService.GetWindowsReEnrollmentCriteria().ToReEnrollmentRuleCriteriaContract();
        }
    }
}
