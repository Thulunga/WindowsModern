using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// Antivirus Device Threat History.
/// </summary>
public sealed class AntivirusDeviceThreatHistory : AntivirusThreatHistory
{
    /// <summary>
    /// Gets or sets Threat name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets Number of detections till date.
    /// </summary>
    public int? NumberOfDetectionsTillDate { get; set; }

    /// <summary>
    /// Gets or sets Antivirus last threat status.
    /// </summary>
    public AntivirusThreatStatus? LastThreatStatus { get; set; }
}
