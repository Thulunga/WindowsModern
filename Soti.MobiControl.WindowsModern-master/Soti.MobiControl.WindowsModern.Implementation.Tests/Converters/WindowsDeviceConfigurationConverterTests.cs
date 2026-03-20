using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models.GroupPolicy;
using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Converters;

[TestFixture]
internal sealed class WindowsDeviceConfigurationConverterTests
{
    private const string Settings = "{\r\n    \"Version\": \"2.0.0\",\r\n    \"GroupPolicySettingGroups\": [\r\n        {\r\n            \"GroupLabel\": \"ControlPanel\",\r\n            \"Settings\": [\r\n                {\r\n                    \"Id\": \"NoLockScreen\",\r\n                    \"State\": 2,\r\n                    \"Type\": 3,\r\n                    \"SubSettings\": [],\r\n                    \"IsRequired\": true,\r\n                    \"SubKey\": \"Software\\\\Policies\\\\Microsoft\\\\Windows\\\\Personalization\",\r\n                    \"KeyType\": \"RegDword\",\r\n                    \"MergeType\": \"MergeOr\"\r\n                },\r\n                {\r\n                    \"Id\": \"NoChangingLockScreen\",\r\n                    \"State\": 2,\r\n                    \"Type\": 3,\r\n                    \"SubSettings\": [],\r\n                    \"IsRequired\": true,\r\n                    \"SubKey\": \"Software\\\\Policies\\\\Microsoft\\\\Windows\\\\Personalization\",\r\n                    \"KeyType\": \"RegDword\",\r\n                    \"MergeType\": \"MergeOr\"\r\n                },\r\n                {\r\n                    \"Id\": \"ForceDefaultLockScreen\",\r\n                    \"State\": 2,\r\n                    \"Type\": 3,\r\n                    \"SubSettings\": [\r\n                        {\r\n                            \"Type\": 1,\r\n                            \"Id\": \"LockScreenImage\",\r\n                            \"Value\": \"\",\r\n                            \"Required\": true,\r\n                            \"Regex\": \"^(?:[a-zA-Z]:\\\\\\\\(?:[^\\\\\\\\/:*?\\\"<>|]+\\\\\\\\)*[^\\\\\\\\/:*?\\\"<>|]+\\\\.[a-zA-Z]{3,4}|\\\\\\\\\\\\\\\\[^\\\\\\\\/:*?\\\"<>|]+\\\\\\\\(?:[^\\\\\\\\/:*?\\\"<>|]+\\\\\\\\)*[^\\\\\\\\/:*?\\\"<>|]+\\\\.[a-zA-Z]{3,4})$\",\r\n                            \"ErrorMessage\": \"Invalid_File_Path\",\r\n                            \"SubKey\": \"Software\\\\Policies\\\\Microsoft\\\\Windows\\\\Personalization\",\r\n                            \"KeyType\": \"RegSz\",\r\n                            \"MergeType\": \"None\"\r\n                        },\r\n                        {\r\n                            \"Type\": 0,\r\n                            \"Id\": \"LockScreenOverlaysDisabled\",\r\n                            \"Value\": \"false\",\r\n                            \"Required\": false,\r\n                            \"SubKey\": \"Software\\\\Policies\\\\Microsoft\\\\Windows\\\\Personalization\",\r\n                            \"KeyType\": \"RegDword\",\r\n                            \"MergeType\": \"MergeOr\"\r\n                        }\r\n                    ],\r\n                    \"IsRequired\": false\r\n                },\r\n                \r\n                {\r\n                    \"Id\": \"ShowOrHideUsedApps\",\r\n                    \"State\": 2,\r\n                    \"Type\": 3,\r\n                    \"IsRequired\": false,\r\n                    \"SubSettings\": [\r\n                        {\r\n                            \"Type\": 3,\r\n                            \"Id\": \"ShowOrHideMostUsedApps\",\r\n                            \"Value\": \"0\",\r\n                            \"Required\": false,\r\n                            \"DropdownItems\": {\r\n                                \"Not Configured\": \"0\",\r\n                                \"Show\": \"1\",\r\n                                \"Hide\": \"2\"\r\n                            },\r\n                            \"SubKey\": \"Software\\\\Policies\\\\Microsoft\\\\Windows\\\\Explorer\",\r\n                            \"KeyType\": \"RegDword\",\r\n                            \"MergeType\": \"None\"\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}";

    private static readonly Version ExpectedVersion = new Version(2, 0, 0);

    [Test]
    public void ToModel_ShouldThrowArgumentNullException_WhenSettingsIsNull()
    {
        string settings = null;

        Assert.Throws<ArgumentNullException>(() => settings.ToModel<WindowsProductVersion>());
    }

    [Test]
    public void ToModel_ShouldDeserializeValidJsonCorrectly()
    {
        var settings = "[\r\n  { \"DisplayName\": \"Windows 11\", \"ProductVersion\": \"Windows 11\" },\r\n  { \"DisplayName\": \"Windows 12\", \"ProductVersion\": \"Windows 12\" }\r\n]";

        var result = settings.ToModel<WindowsProductVersion[]>();

        Assert.NotNull(result);
        Assert.AreEqual("Windows 11", result[0].DisplayName);
        Assert.AreEqual("Windows 12", result[1].DisplayName);
        Assert.AreEqual("Windows 11", result[0].ProductVersion);
        Assert.AreEqual("Windows 12", result[1].ProductVersion);

        var result2 = Settings.ToModel<WindowsModernGroupPolicyObjectsSettings>();
        Assert.IsNotNull(result2);
        Assert.AreEqual(result2.Version, ExpectedVersion);
        Assert.IsNotNull(result2.GroupPolicySettingGroups);
    }
}
