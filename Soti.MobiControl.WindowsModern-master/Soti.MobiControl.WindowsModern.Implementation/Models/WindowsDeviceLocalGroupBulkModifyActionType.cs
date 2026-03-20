namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Represents the possible action types for bulk modifications of Windows device local groups.
/// </summary>
internal static class WindowsDeviceLocalGroupBulkModifyActionType
{
    /// <summary>
    /// Represents an update action.
    /// </summary>
    public static readonly string Updated = "UPDATE";

    /// <summary>
    /// Represents an insert (create) action.
    /// </summary>
    public static readonly string Insert = "INSERT";

    /// <summary>
    /// Represents a delete action.
    /// </summary>
    public static readonly string Deleted = "DELETED";
}
