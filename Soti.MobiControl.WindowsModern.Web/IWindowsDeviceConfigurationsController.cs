using Soti.Api.Metadata;
using Soti.MobiControl.WindowsModern.Web.Contracts;

namespace Soti.MobiControl.WindowsModern.Web
{
    /// <summary>
    /// Interface for Windows Modern Device Configuration Controller.
    /// </summary>
    [RoutePrefix("configurations/windows/reEnrollment")]
    public interface IWindowsDeviceConfigurationsController
    {
        /// <summary>
        /// Updates device match options for Windows Re-enrollment.
        /// </summary>
        /// <remarks>
        /// This API updates strict Re-enrollment for Windows.
        /// <b>(Available Since MobiControl v2025.0.0)</b>
        /// <br/>
        /// </remarks>
        /// <response code="204">Success.</response>
        /// <response code="400">Contract Validation Failed.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpPut]
        [Route("deviceMatchCriteria")]
        void SaveWindowsReEnrollmentCriteria([FromBody] WindowsReEnrollmentCriteria options);

        /// <summary>
        /// Gets device match options for Windows Re-enrollment.
        /// </summary>
        /// <remarks>
        /// This API gets strict Re-enrollment for Windows.
        /// <b>(Available Since MobiControl v2025.0.0)</b>
        /// <br/>
        /// </remarks>
        /// <response code="200">Success.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        [HttpGet]
        [Route("deviceMatchCriteria")]
        WindowsReEnrollmentCriteria GetWindowsReEnrollmentCriteria();
    }
}
