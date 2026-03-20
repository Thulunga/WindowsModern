namespace Soti.MobiControl.WindowsModern.Models.GroupPolicy;

/// <summary>
/// Group Policy Setting Merge Type.
/// </summary>
public enum GroupPolicySettingMergeType
{
    /// <summary>
    /// Takes incoming value.
    /// </summary>
    None,

    /// <summary>
    /// Performs Merge Or operation between values of conflicting GPO settings.
    /// </summary>
    MergeOr,

    /// <summary>
    /// Performs Merge And operation between values of conflicting GPO settings.
    /// </summary>
    MergeAnd,

    /// <summary>
    /// Custom merge operation for settingsPageVisibility
    /// </summary>
    SettingsPageVisibility
}
