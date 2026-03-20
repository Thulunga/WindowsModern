using System;

namespace Soti.MobiControl.WindowsModern.Models.GroupPolicy;

/// <summary>
/// Windows Modern Group Policy Objects Settings.
/// </summary>
public sealed class WindowsModernGroupPolicyObjectsSettings
{
    /// <summary>
    /// Gets or sets the Version of Group Policy Settings.
    /// </summary>
    public Version Version { get; set; }

    /// <summary>
    /// Gets or sets the collection of Group Policy Setting Groups.
    /// </summary>
    public GroupPolicySettingGroup[] GroupPolicySettingGroups { get; set; }
}
