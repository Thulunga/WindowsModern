namespace Soti.MobiControl.WindowsModern.Web.Contracts.GroupPolicy;

/// <summary>
/// Group Policy Setting.
/// </summary>
public sealed class GroupPolicySetting
{
    /// <summary>
    /// Gets or sets the unique identifier of the Group Policy Setting.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the state of the Group Policy Setting.
    /// </summary>
    public GroupPolicySettingState State { get; set; }

    /// <summary>
    /// Gets or sets the type of the Group Policy Setting.
    /// </summary>
    public GroupPolicySettingType Type { get; set; }

    /// <summary>
    /// Gets or sets the IsRequired property of the Group Policy Setting.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the collection of the Group Policy Sub Settings.
    /// </summary>
    public GroupPolicySubSetting[] SubSettings { get; set; }
}
