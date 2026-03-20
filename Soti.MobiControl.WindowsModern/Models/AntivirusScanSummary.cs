using System;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models
{
    /// <summary>
    /// AntivirusScanSummary entity class.
    /// </summary>
    public sealed class AntivirusScanSummary
    {
        /// <summary>
        /// Gets or sets LastQuickScanTime.
        /// </summary>
        public DateTime? LastQuickScanTime { get; set; }

        /// <summary>
        /// Gets or sets LastFullScanStartTime.
        /// </summary>
        public DateTime? LastFullScanTime { get; set; }

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

        /// <summary>
        /// Gets or sets ThreatStatusCountSummary.
        /// </summary>
        public IDictionary<AntivirusThreatStatus, int> ThreatStatusCountSummary { get; set; }
    }
}
