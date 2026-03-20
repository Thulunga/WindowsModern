using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Models.GroupPolicy;

public sealed class GroupPolicySubSetting
{
    /// <summary>
    /// Gets or sets the unique identifier of the Group Policy Sub Setting.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the default value of the Group Policy Sub Setting.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the type of the Group Policy Sub Setting.
    /// </summary>
    public GroupPolicySettingType Type { get; set; }

    /// <summary>
    /// Gets or sets the Min Length of the Group Policy Sub Setting.
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// Gets or sets the Max Length of the Group Policy Sub Setting.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the Required property of the Group Policy Sub Setting.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the Dropdown Items of the Group Policy Sub Setting.
    /// </summary>
    public IReadOnlyDictionary<string, string> DropdownItems { get; set; }

    /// <summary>
    /// Gets or sets the regex used for validation of the Group Policy Sub Setting.
    /// </summary>
    public string Regex { get; set; }

    /// <summary>
    /// Gets or sets the Error Message to be shown for the Group Policy Sub Setting.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the IsRequired property of the Group Policy Sub Setting.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the Merge Type of the Group Policy Sub Setting.
    /// </summary>
    public GroupPolicySettingMergeType MergeType { get; set; }

    /// <summary>
    /// Gets or sets the Sub Key Name of the Group Policy Sub Setting.
    /// </summary>
    public string SubKey { get; set; }

    /// <summary>
    /// Gets or sets the Key Type of the Group Policy Sub Setting.
    /// </summary>
    public GroupPolicySettingKeyType KeyType { get; set; }
}
