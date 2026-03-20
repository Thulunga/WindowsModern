using Soti.Api.Metadata;
using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Contracts
{
    /// <summary>
    /// Application that can be launched in Single App Kiosk Mode.
    /// </summary>
    [OpenApiSchema("AssignedAccessApplication")]
    public sealed class AssignedAccessApplication
    {
        /// <summary>
        /// Identifies the Allowed Application Type.
        /// </summary>
        /// <value>
        /// UWP, Desktop.
        /// </value>
        public ApplicationType AppType { get; set; }

        /// <summary>
        /// Identifies the Allowed Application Name.
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// Identifies the Allowed Application Id.
        /// </summary>
        /// <value>
        /// For UWP Apps it is AUMID, but for Classic it is DesktopAppPath.
        /// </value>
        public string AppIdPath { get; set; }
    }
}
