using System.Collections.Generic;
using System.Threading.Tasks;
using Soti.Api.Metadata;
using Soti.Api.Metadata.DataRetrieval;
using Soti.MobiControl.Devices;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Enums;
using IActionResult = Microsoft.AspNetCore.Mvc.IActionResult;

namespace Soti.MobiControl.WindowsModern.Web;

[RoutePrefix("deviceGroups")]
public interface IDeviceGroupsPeripheralController
{
    /// <summary>
    /// Returns device groups peripheral summary.
    /// </summary>
    [HttpGet]
    [Route("{path}/peripherals/{deviceFamily}")]
    IEnumerable<DeviceGroupPeripheralSummary> GetDeviceGroupsPeripherals(
        [FromUri] string path,
        [FromUri] DeviceFamily deviceFamily,
        [FromUri] string searchString = null,
        [FromUri] IEnumerable<DevicePeripheralStatus> statuses = null,
        [ModelBinder(typeof(DataRetrievalOptionsModelBinder))] DataRetrievalOptions dataRetrievalOptions = null);

    /// <summary>
    /// Returns Download CSV of device groups peripheral summary.
    /// </summary>
    [HttpGet]
    [Route("{path}/peripherals/{deviceFamily}/actions/exportReport")]
    IActionResult DownloadPeripheralsReport(
        [FromUri] string path,
        [FromUri] DeviceFamily deviceFamily,
        [FromUri] string[] reportHeaderFields = null,
        [FromUri] string nameContains = null,
        [FromUri] string[] statuses = null,
        [FromUri] string[] peripheralTypes = null,
        [FromUri] int timeZoneOffset = 0);

    /// <summary>
    /// Email the CSV device groups peripheral summary.
    /// </summary>
    [HttpPost]
    [Route("{path}/peripherals/{deviceFamily}/actions/emailReport")]
    Task EmailPeripheralsReport([FromUri] string path, [FromUri] DeviceFamily deviceFamily, [FromBody] PeripheralEmailReportParameters parameters);
}
