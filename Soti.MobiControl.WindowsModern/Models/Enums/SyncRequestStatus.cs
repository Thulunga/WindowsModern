namespace Soti.MobiControl.WindowsModern.Models.Enums;

/// <summary>
/// Sync Request Status Enum.
/// </summary>
public enum SyncRequestStatus : byte
{
    /// <summary>
    /// The Sync Request Status is Queued.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// The Sync Request Status is Running.
    /// </summary>
    Running = 2,

    /// <summary>
    /// The Sync Request Status is PartiallyCompleted.
    /// </summary>
    PartiallyCompleted = 3,

    /// <summary>
    /// The Sync Request Status is Completed.
    /// </summary>
    Completed = 4,

    /// <summary>
    /// The Sync Request Status is Error.
    /// </summary>
    Error = 5,

    /// <summary>
    /// The Sync Request Status is TimedOut.
    /// </summary>
    TimedOut = 6,
}
