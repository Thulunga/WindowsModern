using System;
using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Implementation.Models
{
    /// <summary>
    /// Windows LocalUser Keys.
    /// </summary>
    internal sealed class WindowsDeviceLocalUserSnapShot
    {
        /// <summary>
        /// Gets or sets SID.
        /// </summary>
        public string SID { get; set; }

        /// <summary>
        /// Gets or sets Username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets UserGroups.
        /// </summary>
        public IEnumerable<string> UserGroups { get; set; }

        /// <summary>
        /// Gets or sets DeviceId.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets PasswordLastModified.
        /// </summary>
        public DateTime? PasswordLastModified { get; set; }
    }
}
