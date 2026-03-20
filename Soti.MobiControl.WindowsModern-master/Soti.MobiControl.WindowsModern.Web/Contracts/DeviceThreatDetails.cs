using System;
using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// Device threat details contract.
/// </summary>
public sealed class DeviceThreatDetails
{
    /// <summary>
    /// Gets or sets device Id.
    /// </summary>
    public string DeviceId { get; set; }

    /// <summary>
    /// Gets or sets initial detection time.
    /// </summary>
    public DateTime InitialDetectionTime { get; set; }

    /// <summary>
    /// Gets or sets last status change time.
    /// </summary>
    public DateTime? LastStatusChangeTime { get; set; }

    /// <summary>
    /// Gets or sets antivirus threat status.
    /// </summary>
    public AntivirusThreatStatus ThreatStatus { get; set; }
}
