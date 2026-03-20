namespace Soti.MobiControl.WindowsModern.Models.GroupPolicy;

/// <summary>
/// Group Policy Setting Group.
/// </summary>
public sealed class GroupPolicySettingGroup
{
    /// <summary>
    /// Gets or sets the Group Label.
    /// </summary>
    public string GroupLabel { get; set; }

    /// <summary>
    /// Gets or sets the collection of the Group Policy Settings.
    /// </summary>
    public GroupPolicySetting[] Settings { get; set; }
}
