namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Antivirus scan counts.
/// </summary>
public sealed class AntivirusScanCounts
{
    /// <summary>
    /// Gets or sets Antivirus quick scanned devices count.
    /// </summary>
    public int QuickScanned { get; set; }

    /// <summary>
    /// Gets or sets Antivirus full scanned devices count.
    /// </summary>
    public int FullScanned { get; set; }
}
