using System.Collections.Generic;
using Soti.Api.Metadata;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Contracts.GroupPolicy;

namespace Soti.MobiControl.WindowsModern.Web
{
    /// <summary>
    /// Interface for Windows Modern Profile UI Controller.
    /// </summary>
    [RoutePrefix("windowsModern/profiles")]
    [Authorize]
    public interface IWindowsDeviceProfileUiController
    {
        /// <summary>
        /// Retrieves Modern and Classic applications for Assigned Access Kiosk.
        /// </summary>
        /// <remarks>
        /// This API returns applications for Assigned Access Kiosk.
        /// <br/>
        /// <br/>
        /// Requires the caller be granted the "ManageProfileSetup" permission.
        /// <br/>
        /// <b>(Available Since MobiControl v2025.0.0)</b>
        /// <br/>
        /// </remarks>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpGet]
        [Route("assignedAccess/apps")]
        IEnumerable<AssignedAccessApplication> RequestAssignedAccessApplications();

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
        WindowsModernGroupPolicyObjectsSettings RequestGroupPolicySettings();
    }
}
