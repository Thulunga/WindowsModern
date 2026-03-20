using System;
using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Web.Contracts
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
        /// Gets or sets LastFullScanTime.
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
        public IDictionary<string, int> ThreatStatusCountSummary { get; set; }
    }
}
