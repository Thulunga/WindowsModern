using System.Collections.Generic;
using Soti.Api.Metadata;

namespace Soti.MobiControl.WindowsModern.Web;

/// <summary>
/// Windows Modern Device Groups UI Controller.
/// </summary>
[RoutePrefix("windows/devicegroups")]
public interface IWindowsDeviceGroupsUiController
{
    /// <summary>
    /// Returns a list of local groups across all Windows devices in the given device group.
    /// </summary>
    /// <param name="path">The reference ID or the path of the device group.</param>
    /// <returns>List of local groups.</returns>
    [HttpGet]
    [Route("{path}/localGroups")]
    IEnumerable<string> GetDeviceGroupLocalGroups(string path);
}
