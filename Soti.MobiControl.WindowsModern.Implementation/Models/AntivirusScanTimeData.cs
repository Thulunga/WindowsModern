using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Models
{
    /// <summary>
    /// Antivirus Scan Time Data.
    /// </summary>
    internal sealed class AntivirusScanTimeData
    {
        /// <summary>
        /// Gets or sets AntivirusLastQuickScanTime.
        /// </summary>
        public DateTime? AntivirusLastQuickScanTime { get; set; }

        /// <summary>
        /// Gets or sets AntivirusLastFullScanTime.
        /// </summary>
        public DateTime? AntivirusLastFullScanTime { get; set; }

        /// <summary>
        /// Gets or sets LastAntivirusSyncTime.
        /// </summary>
        public DateTime? LastAntivirusSyncTime { get; set; }

        /// <summary>
        /// Gets or sets IsThreatsAvailable.
        /// </summary>
        public bool IsThreatsAvailable { get; set; }

        /// <summary>
        /// Gets or sets IsActiveThreatAvailable.
        /// </summary>
        public bool IsActiveThreatAvailable { get; set; }
    }
}
