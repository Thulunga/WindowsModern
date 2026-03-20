using System;
using System.Linq;
using Soti.MobiControl.WindowsModern.Web.Contracts.GroupPolicy;
using WindowsModernGroupPolicyObjectsSettings = Soti.MobiControl.WindowsModern.Web.Contracts.GroupPolicy.WindowsModernGroupPolicyObjectsSettings;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

internal static class GroupPolicySettingsConverter
{
    public static WindowsModernGroupPolicyObjectsSettings ToWindowsModernGroupPolicySettingsContract(
        this Models.GroupPolicy.WindowsModernGroupPolicyObjectsSettings model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        return new WindowsModernGroupPolicyObjectsSettings
        {
            GroupPolicySettingGroups = model.GroupPolicySettingGroups.Select(group => new GroupPolicySettingGroup
            {
                GroupLabel = group.GroupLabel,
                Settings = group.Settings.Select(setting => new GroupPolicySetting
                {
                    Id = setting.Id,
                    State = (GroupPolicySettingState)setting.State,
                    Type = (GroupPolicySettingType)setting.Type,
                    IsRequired = setting.IsRequired,
                    SubSettings = setting.SubSettings.Select(subSetting => new GroupPolicySubSetting
                    {
                        Id = subSetting.Id,
                        Value = subSetting.Value,
                        Type = (GroupPolicySettingType)subSetting.Type,
                        IsRequired = subSetting.IsRequired,
                        MinLength = subSetting.MinLength,
                        MaxLength = subSetting.MaxLength,
                        Required = subSetting.Required,
                        DropdownItems = subSetting.DropdownItems,
                        Regex = subSetting.Regex,
                        ErrorMessage = subSetting.ErrorMessage,
                    }).ToArray()
                }).ToArray()
            }).ToArray()
        };
    }
}
