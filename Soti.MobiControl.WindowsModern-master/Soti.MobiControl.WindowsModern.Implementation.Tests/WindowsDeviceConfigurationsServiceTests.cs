using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using Soti.MobiControl.Settings;
using Soti.MobiControl.WindowsModern.Middleware;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.GroupPolicy;
using RichardSzalay.MockHttp;
using Soti.Diagnostics;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using System.Threading.Tasks;
using System.Text.Json;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests
{
    [TestFixture]
    internal sealed class WindowsDeviceConfigurationsServiceTests
    {
        private const string WindowsDeviceMatchReEnrollmentSettingName = "WindowsDeviceMatchReenrollment.Options";
        private const string WindowsGroupPolicyObjectsSettingName = "WindowsModernGroupPolicyObjectsSettings";
        private const string GetGroupPolicySettingsUri = "/SharedFiles/GroupPolicyObjectsSettings_v1";
        private const string GetWindowsProductVersionsSettingsUri = "/SharedFiles/WindowsProductVersions";

        private static readonly Version SampleGroupPolicySettingVersion = new(1, 0, 0);
        private static readonly Version LatestGroupPolicySettingVersion = new(2, 0, 0);

        private WindowsModernGroupPolicyObjectsSettings _testWindowsModernGroupPolicyObjectsSettings;
        private GroupPolicySetting[] _testGroupPolicySettingsList;

        private Mock<ISettingsManagementService> _settingsManagementServiceMock;
        private Mock<IProgramTrace> _programTraceMock;
        private Mock<IHttpClientProvider> _httpClientProviderMock;
        private MockHttpMessageHandler _httpMessageHandlerMock;
        private WindowsDeviceConfigurationsService _windowsDeviceConfigurationsService;

        [SetUp]
        public void Setup()
        {
            _settingsManagementServiceMock = new Mock<ISettingsManagementService>(MockBehavior.Strict);
            _programTraceMock = new Mock<IProgramTrace>(MockBehavior.Strict);
            _httpClientProviderMock = new Mock<IHttpClientProvider>(MockBehavior.Strict);
            _httpMessageHandlerMock = new MockHttpMessageHandler();

            _httpClientProviderMock.Setup(s => s.Get()).Returns(_httpMessageHandlerMock.ToHttpClient);
            _settingsManagementServiceMock.Setup(service => service.GetSetting<string>(
                string.Empty,
                "SotiServicesBaseUrl",
                default,
                ObfuscationOption.None,
                default)).Returns("https://services-test.soti.net");

            _windowsDeviceConfigurationsService = new WindowsDeviceConfigurationsService(_settingsManagementServiceMock.Object, _programTraceMock.Object, _httpClientProviderMock.Object);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var testGroupPolicySubSetting1 = GetGroupPolicySubSetting("ForceStartBackground", "0", GroupPolicySettingType.NumberField, false, GroupPolicySettingMergeType.None, "subKey1", GroupPolicySettingKeyType.RegDword, "regex1", "errorMessage1", true, null, 0, 20);
            var testGroupPolicySubSetting2 = GetGroupPolicySubSetting("ShowOrHideMostUsedApps", "0", GroupPolicySettingType.Dropdown, true, GroupPolicySettingMergeType.None, "subKey2", GroupPolicySettingKeyType.RegDword, "regex2", "errorMessage2", false, null, null, null);

            _testGroupPolicySettingsList =
            [
                GetGroupPolicySetting("settingName1", GroupPolicySettingState.NotConfigured, GroupPolicySettingType.Dropdown, true, GroupPolicySettingMergeType.MergeOr, "key1", GroupPolicySettingKeyType.RegDword, [testGroupPolicySubSetting1]),
                GetGroupPolicySetting("settingName2", GroupPolicySettingState.Disabled, GroupPolicySettingType.Toggle, false, GroupPolicySettingMergeType.MergeAnd, "key2", GroupPolicySettingKeyType.RegSz, [testGroupPolicySubSetting2])
            ];

            _testWindowsModernGroupPolicyObjectsSettings = new WindowsModernGroupPolicyObjectsSettings
            {
                Version = SampleGroupPolicySettingVersion,
                GroupPolicySettingGroups =
                [
                    GetGroupPolicySettingGroup("ControlPanelGroup", _testGroupPolicySettingsList),
                    GetGroupPolicySettingGroup("StartMenuAndTaskbar", _testGroupPolicySettingsList)
                ]
            };
        }

        [TearDown]
        public void TearDown()
        {
            _httpMessageHandlerMock?.Dispose();
        }

        [Test]
        public void UpdateReEnrollment_SuccessWithToggleOn()
        {
            _settingsManagementServiceMock.Setup(s => s.SaveSetting<WindowsReEnrollmentCriteria>(String.Empty, "WindowsDeviceMatchReenrollment.Options", It.IsAny<WindowsReEnrollmentCriteria>(), default));
            Assert.DoesNotThrow(() => _windowsDeviceConfigurationsService.UpdateWindowsReEnrollmentCriteria(GetReEnrollmentCriteria(true, true)));
            _settingsManagementServiceMock.Verify(
                m => m.SaveSetting(
                    string.Empty,
                    WindowsDeviceMatchReEnrollmentSettingName,
                    It.IsAny<WindowsReEnrollmentCriteria>(),
                    ObfuscationOption.None), Times.Once);
        }

        [Test]
        public void UpdateReEnrollment_SuccessWithToggleOff()
        {
            _settingsManagementServiceMock.Setup(s => s.SaveSetting<WindowsReEnrollmentCriteria>(String.Empty, "WindowsDeviceMatchReenrollment.Options", It.IsAny<WindowsReEnrollmentCriteria>(), default));
            _windowsDeviceConfigurationsService.UpdateWindowsReEnrollmentCriteria(GetReEnrollmentCriteria(false, false));
            _settingsManagementServiceMock.Verify(
                m => m.SaveSetting(
                    string.Empty,
                    WindowsDeviceMatchReEnrollmentSettingName,
                    It.IsAny<WindowsReEnrollmentCriteria>(),
                    ObfuscationOption.None), Times.Once);
        }

        [Test]
        public void UpdateReEnrollment_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _windowsDeviceConfigurationsService.UpdateWindowsReEnrollmentCriteria(null));
            _settingsManagementServiceMock.Verify(
                m => m.SaveSetting(
                    string.Empty,
                    WindowsDeviceMatchReEnrollmentSettingName,
                    It.IsAny<string>(),
                    ObfuscationOption.None), Times.Never);
        }

        [Test]
        public void GetReEnrollment_ToggleOff()
        {
            _settingsManagementServiceMock.Setup(m => m.GetSetting<WindowsReEnrollmentCriteria>(
                string.Empty,
                WindowsDeviceMatchReEnrollmentSettingName,
                default,
                ObfuscationOption.None,
                default)).Returns(GetReEnrollmentCriteria(false, false));
            var result = _windowsDeviceConfigurationsService.GetWindowsReEnrollmentCriteria();
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HardwareId);
            Assert.IsFalse(result.MacAddress);
        }

        [Test]
        public void GetReEnrollment_ToggleOn()
        {
            _settingsManagementServiceMock.Setup(m => m.GetSetting<WindowsReEnrollmentCriteria>(
                string.Empty,
                WindowsDeviceMatchReEnrollmentSettingName,
                default,
                ObfuscationOption.None,
                default)).Returns(GetReEnrollmentCriteria(true, true));
            var result = _windowsDeviceConfigurationsService.GetWindowsReEnrollmentCriteria();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HardwareId);
            Assert.IsTrue(result.MacAddress);
        }

        [Test]
        public void CheckForStrictReEnrollment_HardwareIdAndMacAddressMatch_ReturnsTrue()
        {
            _settingsManagementServiceMock.Setup(m => m.GetSetting<WindowsReEnrollmentCriteria>(
                string.Empty,
                WindowsDeviceMatchReEnrollmentSettingName,
                default,
                ObfuscationOption.None,
                default)).Returns(GetReEnrollmentCriteria(true, true));
            var result = _windowsDeviceConfigurationsService.CheckForStrictReEnrollment();
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckForStrictReEnrollment_HardwareIdAndMacAddressMatch_ReturnsFalse()
        {
            _settingsManagementServiceMock.Setup(m => m.GetSetting<WindowsReEnrollmentCriteria>(
                string.Empty,
                WindowsDeviceMatchReEnrollmentSettingName,
                default,
                ObfuscationOption.None,
                default)).Returns(GetReEnrollmentCriteria(false, false));
            var result = _windowsDeviceConfigurationsService.CheckForStrictReEnrollment();
            Assert.IsFalse(result);
        }

        [Test]
        public void CheckForStrictReEnrollment_HardwareIdAndMacAddressDoesNotMatch_ReturnsFalse()
        {
            _settingsManagementServiceMock.Setup(m => m.GetSetting<WindowsReEnrollmentCriteria>(
                string.Empty,
                WindowsDeviceMatchReEnrollmentSettingName,
                default,
                ObfuscationOption.None,
                default)).Returns(GetReEnrollmentCriteria(false, true));
            var result = _windowsDeviceConfigurationsService.CheckForStrictReEnrollment();
            Assert.IsFalse(result);
        }

        [Test]
        public void GetGroupPolicySettings_Success()
        {
            _settingsManagementServiceMock.Setup(service => service.GetSetting<WindowsModernGroupPolicyObjectsSettings>(
                string.Empty,
                WindowsGroupPolicyObjectsSettingName,
                default,
                ObfuscationOption.None,
                default)).Returns(_testWindowsModernGroupPolicyObjectsSettings);

            var result = _windowsDeviceConfigurationsService.GetGroupPolicySettings();

            Assert.IsNotNull(result);
            VerifyGroupPolicySettingsResult(_testWindowsModernGroupPolicyObjectsSettings, result);

            _settingsManagementServiceMock.Verify(service => service.GetSetting<WindowsModernGroupPolicyObjectsSettings>(
                string.Empty,
                WindowsGroupPolicyObjectsSettingName,
                default,
                ObfuscationOption.None,
                default),
                Times.Once);
        }

        [Test]
        public async Task UpdateGroupPolicySettings_Success()
        {
            _settingsManagementServiceMock.Setup(service => service.GetSetting<WindowsModernGroupPolicyObjectsSettings>(
                string.Empty,
                WindowsGroupPolicyObjectsSettingName,
                default,
                ObfuscationOption.None,
                default)).Returns(_testWindowsModernGroupPolicyObjectsSettings);

            _settingsManagementServiceMock.Setup(s => s.GetSetting<string>(string.Empty, "regcode", default, default, default)).Returns("ABCDEFGHIJKLMNOP");
            _settingsManagementServiceMock.Setup(s => s.GetSetting<string>(string.Empty, "InstallationId", default, default, default)).Returns("ABCDEFGHIJKLMNOP");
            _settingsManagementServiceMock.Setup(s => s.SaveSetting<WindowsModernGroupPolicyObjectsSettings>("", "WindowsModernGroupPolicyObjectsSettings", It.IsAny<WindowsModernGroupPolicyObjectsSettings>(), default));

            var latestWindowsModernGroupPolicyObjectsSettings = new WindowsModernGroupPolicyObjectsSettings
            {
                Version = LatestGroupPolicySettingVersion,
                GroupPolicySettingGroups =
                [
                    GetGroupPolicySettingGroup("PowerOptions", _testGroupPolicySettingsList),
                    GetGroupPolicySettingGroup("DisplaySettings", _testGroupPolicySettingsList)
                ]
            };

            _httpMessageHandlerMock
                .When(GetGroupPolicySettingsUri)
                .Respond("application/json", JsonSerializer.Serialize(latestWindowsModernGroupPolicyObjectsSettings));

            await _windowsDeviceConfigurationsService.UpdateGroupPolicySettings();

            _settingsManagementServiceMock.Verify(service => service.GetSetting<WindowsModernGroupPolicyObjectsSettings>(
                    string.Empty,
                    WindowsGroupPolicyObjectsSettingName,
                    default,
                    ObfuscationOption.None,
                    default),
                Times.Once);
        }

        [Test]
        public async Task UpdateWindowsUpdatesPolicyProductVersionSettings_Success()
        {
            _settingsManagementServiceMock.Setup(s => s.GetSetting<string>(string.Empty, "regcode", default, default, default)).Returns("ABCDEFGHIJKLMNOP");
            _settingsManagementServiceMock.Setup(s => s.GetSetting<string>(string.Empty, "InstallationId", default, default, default)).Returns("ABCDEFGHIJKLMNOP");
            _settingsManagementServiceMock.Setup(s => s.SaveSetting<IEnumerable<WindowsProductVersion>>("WindowsUpdatePolicies", "ProductVersions", It.IsAny<WindowsProductVersion[]>(), default));
            WindowsProductVersion[] productVersions =
            [
                new()
                {
                    DisplayName = "Windows 13",
                    ProductVersion = "Windows 13"
                }
            ];

            _httpMessageHandlerMock
                .When(GetWindowsProductVersionsSettingsUri)
                .Respond("application/json", JsonSerializer.Serialize(productVersions));

            await _windowsDeviceConfigurationsService.UpdateWindowsUpdatesPolicyProductVersionSettings();

            _settingsManagementServiceMock.Verify(service => service.GetSetting<string>(
                    string.Empty,
                    "SotiServicesBaseUrl",
                    default,
                    ObfuscationOption.None,
                    default),
                Times.Once);
        }

        [Test]
        public void UpdateWindowsUpdatesPolicyProductVersionSettings_apiCall_Returns_Empty_String()
        {
            _settingsManagementServiceMock.Setup(s => s.GetSetting<string>(string.Empty, "regcode", default, default, default)).Returns("ABCDEFGHIJKLMNOP");
            _settingsManagementServiceMock.Setup(s => s.GetSetting<string>(string.Empty, "InstallationId", default, default, default)).Returns("ABCDEFGHIJKLMNOP");
            _programTraceMock.Setup(s => s.Write(TraceLevel.Error, nameof(WindowsDeviceConfigurationsService), It.IsAny<string>()));
            _httpMessageHandlerMock
                .When(GetWindowsProductVersionsSettingsUri)
                .Respond("application/json", "");

            Assert.ThrowsAsync<InvalidDataException>(_windowsDeviceConfigurationsService.UpdateWindowsUpdatesPolicyProductVersionSettings);
        }

        private WindowsReEnrollmentCriteria GetReEnrollmentCriteria(bool hardwareId, bool macAddress)
        {
            var mockRequest = new WindowsReEnrollmentCriteria()
            {
                HardwareId = hardwareId,
                MacAddress = macAddress
            };
            return mockRequest;
        }

        private static GroupPolicySettingGroup GetGroupPolicySettingGroup(
            string label,
            GroupPolicySetting[] settings)
        {
            return new GroupPolicySettingGroup
            {
                GroupLabel = label,
                Settings = settings
            };
        }

        private static GroupPolicySetting GetGroupPolicySetting(
            string settingName,
            GroupPolicySettingState state,
            GroupPolicySettingType type,
            bool isRequired,
            GroupPolicySettingMergeType mergeType,
            string subKey,
            GroupPolicySettingKeyType keyType,
            GroupPolicySubSetting[] subSettings)
        {
            return new GroupPolicySetting
            {
                Id = settingName,
                State = state,
                Type = type,
                IsRequired = isRequired,
                MergeType = mergeType,
                KeyType = keyType,
                SubKey = subKey,
                SubSettings = subSettings
            };
        }

        private static GroupPolicySubSetting GetGroupPolicySubSetting(
            string settingName,
            string value,
            GroupPolicySettingType type,
            bool isRequired,
            GroupPolicySettingMergeType mergeType,
            string subKey,
            GroupPolicySettingKeyType keyType,
            string regex,
            string errorMessage,
            bool required,
            IReadOnlyDictionary<string, string> dropdownItems,
            int? minLength,
            int? maxLength)
        {
            return new GroupPolicySubSetting
            {
                Id = settingName,
                Value = value,
                Type = type,
                IsRequired = isRequired,
                MergeType = mergeType,
                KeyType = keyType,
                SubKey = subKey,
                MinLength = minLength,
                MaxLength = maxLength,
                Required = required,
                DropdownItems = dropdownItems,
                Regex = regex,
                ErrorMessage = errorMessage,
            };
        }

        private static void VerifyGroupPolicySettingsResult(
            WindowsModernGroupPolicyObjectsSettings expected,
            WindowsModernGroupPolicyObjectsSettings actual)
        {
            Assert.AreEqual(expected.Version, actual.Version);

            var index = 0;
            foreach (var actualGroup in actual.GroupPolicySettingGroups)
            {
                var expectedGroup = expected.GroupPolicySettingGroups[index];

                Assert.AreEqual(expectedGroup.GroupLabel, actualGroup.GroupLabel);
                VerifyGroupPolicySetting(expectedGroup.Settings, actualGroup.Settings);

                index++;
            }
        }

        private static void VerifyGroupPolicySetting(
            GroupPolicySetting[] expectedSettings,
            GroupPolicySetting[] actualSettings)
        {
            var index = 0;
            foreach (var actualSetting in actualSettings)
            {
                var expectedSetting = expectedSettings[index];

                Assert.AreEqual(expectedSetting.Id, actualSetting.Id);
                Assert.AreEqual(expectedSetting.State, actualSetting.State);
                Assert.AreEqual(expectedSetting.Type, actualSetting.Type);
                Assert.AreEqual(expectedSetting.IsRequired, actualSetting.IsRequired);
                Assert.AreEqual(expectedSetting.MergeType, actualSetting.MergeType);
                Assert.AreEqual(expectedSetting.KeyType, actualSetting.KeyType);
                Assert.AreEqual(expectedSetting.SubKey, actualSetting.SubKey);
                VerifyGroupPolicySubSetting(expectedSetting.SubSettings, actualSetting.SubSettings);

                index++;
            }
        }

        private static void VerifyGroupPolicySubSetting(
            GroupPolicySubSetting[] expectedSubSettings,
            GroupPolicySubSetting[] actualSubSettings)
        {
            var index = 0;
            foreach (var actualSubSetting in actualSubSettings)
            {
                var expectedSubSetting = expectedSubSettings[index];

                Assert.AreEqual(expectedSubSetting.Id, actualSubSetting.Id);
                Assert.AreEqual(expectedSubSetting.Value, actualSubSetting.Value);
                Assert.AreEqual(expectedSubSetting.Type, actualSubSetting.Type);
                Assert.AreEqual(expectedSubSetting.IsRequired, actualSubSetting.IsRequired);
                Assert.AreEqual(expectedSubSetting.MergeType, actualSubSetting.MergeType);
                Assert.AreEqual(expectedSubSetting.KeyType, actualSubSetting.KeyType);
                Assert.AreEqual(expectedSubSetting.SubKey, actualSubSetting.SubKey);
                Assert.AreEqual(expectedSubSetting.MinLength, actualSubSetting.MinLength);
                Assert.AreEqual(expectedSubSetting.MaxLength, actualSubSetting.MaxLength);
                Assert.AreEqual(expectedSubSetting.DropdownItems, actualSubSetting.DropdownItems);
                Assert.AreEqual(expectedSubSetting.Regex, actualSubSetting.Regex);
                Assert.AreEqual(expectedSubSetting.ErrorMessage, actualSubSetting.ErrorMessage);

                index++;
            }
        }
    }
}
