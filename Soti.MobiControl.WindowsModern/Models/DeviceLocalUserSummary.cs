using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Models
{
    /// <summary>
    /// Represents Device Local User Summary.
    /// </summary>
    public class DeviceLocalUserSummary
    {
        /// <summary>
        /// Gets or sets Username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets UserGroups.
        /// </summary>
        public IEnumerable<string> UserGroups { get; set; }

        /// <summary>
        /// Gets or sets SID.
        /// </summary>
        public string Sid { get; set; }

        /// <summary>
        /// Gets or sets IsMobiControlCreated.
        /// </summary>
        public bool IsMobiControlCreated { get; set; }
    }
}
