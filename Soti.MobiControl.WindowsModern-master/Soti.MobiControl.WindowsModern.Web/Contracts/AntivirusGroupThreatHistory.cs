namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// Antivirus Group Threat History.
/// </summary>
public sealed class AntivirusGroupThreatHistory : AntivirusThreatHistory
{
    /// <summary>
    /// Get or sets the number of devices associated with the threat.
    /// </summary>
    public int NoOfDevices { get; set; }
}
