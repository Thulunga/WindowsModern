using System;

namespace Soti.MobiControl.WindowsModern.Web.Contracts;

/// <summary>
/// Represents Last Sync Summary.
/// </summary>
public sealed class LastSyncSummary
{
    /// <summary>
    /// Gets or sets LastSyncedOn.
    /// </summary>
    public DateTime? LastSyncedOn { get; set; }

    /// <summary>
    /// Gets or sets IsSyncInProgress.
    /// </summary>
    public bool IsSyncInProgress { get; set; }
}
