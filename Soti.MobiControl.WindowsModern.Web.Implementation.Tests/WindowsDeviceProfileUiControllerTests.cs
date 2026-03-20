using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Soti.MobiControl.AppPolicy.Windows.Model.AssignedAccess;
using Soti.MobiControl.AppPolicy.Windows.Model.Microsoft;
using Soti.MobiControl.AppPolicy.Windows.Services;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Contracts.GroupPolicy;
using Soti.MobiControl.WindowsModern.Web.Enums;
using WindowsModernGroupPolicyObjectsSettings = Soti.MobiControl.WindowsModern.Models.GroupPolicy.WindowsModernGroupPolicyObjectsSettings;
using GroupPolicySettingType = Soti.MobiControl.WindowsModern.Models.GroupPolicy.GroupPolicySettingType;
using GroupPolicySettingState = Soti.MobiControl.WindowsModern.Models.GroupPolicy.GroupPolicySettingState;
using GroupPolicySettingMergeType = Soti.MobiControl.WindowsModern.Models.GroupPolicy.GroupPolicySettingMergeType;
using GroupPolicySettingKeyType = Soti.MobiControl.WindowsModern.Models.GroupPolicy.GroupPolicySettingKeyType;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests
{
    [TestFixture]
    public class WindowsDeviceProfileUiControllerTests
    {
        private static readonly Version SampleGroupPolicySettingVersion = new(1, 0, 0);

        private IWindowsDeviceProfileUiController _controller;

        private Mock<IAssignedAccessApplicationService> _assignedAccessApplicationServiceMock;
        private Mock<IWindowsDeviceConfigurationsService> _windowsDeviceConfigurationsServiceMock;

        [SetUp]
        public void Setup()
        {
            _assignedAccessApplicationServiceMock = new Mock<IAssignedAccessApplicationService>(MockBehavior.Strict);
            _windowsDeviceConfigurationsServiceMock = new Mock<IWindowsDeviceConfigurationsService>(MockBehavior.Strict);

            _controller = new WindowsDeviceProfileUiController(_assignedAccessApplicationServiceMock.Object, _windowsDeviceConfigurationsServiceMock.Object);
        }

        #region Constructor

        [Test]
        public void Constructor_Throws_Exception()
        {
            Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceProfileUiController(null, _windowsDeviceConfigurationsServiceMock.Object));
            Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceProfileUiController(_assignedAccessApplicationServiceMock.Object, null));
        }

        #endregion

        #region RequestAssignedAccessApplications

        [Test]
        public void RequestAssignedAccessApplications_Success()
        {
            var testModernApplications = new List<UwpApplication>
            {
                GetUwpApplication("Calculator", "Microsoft.WindowsCalculator_8wekyb3d8bbwe!App"),
                GetUwpApplication("Maps", "Microsoft.WindowsMaps_8wekyb3d8bbwe!App")
            };

            var testClassicApplications = new List<DesktopApplication>
            {
                GetDesktopApplication("MS Paint", "C:\\Windows\\system32\\mspaint.exe"),
                GetDesktopApplication("NotePad", "C:\\Windows\\system32\\notepad.exe")
            };

            _assignedAccessApplicationServiceMock.Setup(service =>
                    service.GetAssignedAccessConfigurationModernApplications(
                        PlatformTypes.Windows8x | PlatformTypes.WindowsDesktop,
                        new[] { PackageFormatType.Appx,
                            PackageFormatType.Msix,
                            PackageFormatType.AppxBundle,
                            PackageFormatType.MsixBundle },
                        ProcessorArchitectures.Neutral))
                .Returns(testModernApplications);

            _assignedAccessApplicationServiceMock.Setup(service =>
                service.GetAssignedAccessConfigurationClassicDesktopApplications())
                .Returns(testClassicApplications);

            var result = _controller.RequestAssignedAccessApplications().ToList();

            Assert.IsNotNull(result);

            var resultModernApplications = result.Where(app => app.AppType == ApplicationType.UWP).ToArray();
            VerifyModernApplicationDetails(testModernApplications[0], resultModernApplications[0]);
            VerifyModernApplicationDetails(testModernApplications[1], resultModernApplications[1]);

            var resultClassicApplications = result.Where(app => app.AppType == ApplicationType.Desktop).ToArray();
            VerifyClassicApplicationDetails(testClassicApplications[0], resultClassicApplications[0]);
            VerifyClassicApplicationDetails(testClassicApplications[1], resultClassicApplications[1]);

            _assignedAccessApplicationServiceMock.Verify(
                service => service.GetAssignedAccessConfigurationModernApplications(
                    It.Is<PlatformTypes>(types => types == (PlatformTypes.Windows8x | PlatformTypes.WindowsDesktop)),
                    It.IsAny<PackageFormatType[]>(),
                    It.Is<ProcessorArchitectures>(arch => arch == ProcessorArchitectures.Neutral)), Times.Once);
            _assignedAccessApplicationServiceMock.Verify(service => service.GetAssignedAccessConfigurationClassicDesktopApplications(), Times.Once);
        }

        #endregion

        #region RequestGroupPolicySettings

        [Test]
        public void RequestGroupPolicySettings_success()
        {
            var testGroupPolicySubSetting1 = GetGroupPolicySubSetting("ForceStartBackground", "0", GroupPolicySettingType.NumberField, false, GroupPolicySettingMergeType.None, "subKey1", GroupPolicySettingKeyType.RegDword, "regex1", "errorMessage1", true, null, 0, 20);
            var testGroupPolicySubSetting2 = GetGroupPolicySubSetting("ShowOrHideMostUsedApps", "0", GroupPolicySettingType.Dropdown, true, GroupPolicySettingMergeType.None, "subKey2", GroupPolicySettingKeyType.RegDword, "regex2", "errorMessage2", false, null, null, null);

            var testGroupPolicySettingsList = new[]
            {
                GetGroupPolicySetting("settingName1", GroupPolicySettingState.NotConfigured, GroupPolicySettingType.Dropdown, true, GroupPolicySettingMergeType.MergeOr, "key1", GroupPolicySettingKeyType.RegDword, [testGroupPolicySubSetting1]),
                GetGroupPolicySetting("settingName2", GroupPolicySettingState.Disabled, GroupPolicySettingType.Toggle, false, GroupPolicySettingMergeType.MergeAnd, "key2", GroupPolicySettingKeyType.RegSz, [testGroupPolicySubSetting2])
            };

            var testWindowsModernGroupPolicyObjectsSettings = new WindowsModernGroupPolicyObjectsSettings
            {
                Version = SampleGroupPolicySettingVersion,
                GroupPolicySettingGroups =
                [
                    GetGroupPolicySettingGroup("ControlPanelGroup", testGroupPolicySettingsList),
                    GetGroupPolicySettingGroup("StartMenuAndTaskbar", testGroupPolicySettingsList)
                ]
            };

            _windowsDeviceConfigurationsServiceMock.Setup(service => service.GetGroupPolicySettings()).Returns(testWindowsModernGroupPolicyObjectsSettings);

            var result = _controller.RequestGroupPolicySettings();

            Assert.IsNotNull(result);
            VerifyGroupPolicySettingsResult(testWindowsModernGroupPolicyObjectsSettings, result);

            _windowsDeviceConfigurationsServiceMock.Verify(service => service.GetGroupPolicySettings(), Times.Once);
        }

        #endregion

        #region Private Methods

        private static UwpApplication GetUwpApplication(
            string appName,
            string appUserModelId)
        {
            return new UwpApplication
            {
                ApplicationName = appName,
                AppUserModelId = appUserModelId
            };
        }

        private static DesktopApplication GetDesktopApplication(
            string appName,
            string appPath)
        {
            return new DesktopApplication
            {
                ApplicationName = appName,
                ApplicationPath = appPath
            };
        }

        private static void VerifyModernApplicationDetails(
            UwpApplication expected,
            AssignedAccessApplication actual)
        {
            Assert.AreEqual(ApplicationType.UWP, actual.AppType);
            Assert.AreEqual(expected.ApplicationName, actual.AppName);
            Assert.AreEqual(expected.AppUserModelId, actual.AppIdPath);
        }

        private static void VerifyClassicApplicationDetails(
            DesktopApplication expected,
            AssignedAccessApplication actual)
        {
            Assert.AreEqual(ApplicationType.Desktop, actual.AppType);
            Assert.AreEqual(expected.ApplicationName, actual.AppName);
            Assert.AreEqual(expected.ApplicationPath, actual.AppIdPath);
        }

        private static Models.GroupPolicy.GroupPolicySettingGroup GetGroupPolicySettingGroup(
            string label,
            Models.GroupPolicy.GroupPolicySetting[] settings)
        {
            return new Models.GroupPolicy.GroupPolicySettingGroup
            {
                GroupLabel = label,
                Settings = settings
            };
        }

        private static Models.GroupPolicy.GroupPolicySetting GetGroupPolicySetting(
            string settingName,
            GroupPolicySettingState state,
            GroupPolicySettingType type,
            bool isRequired,
            GroupPolicySettingMergeType mergeType,
            string subKey,
            GroupPolicySettingKeyType keyType,
            Models.GroupPolicy.GroupPolicySubSetting[] subSettings
            )
        {
            return new Models.GroupPolicy.GroupPolicySetting
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

        private static Models.GroupPolicy.GroupPolicySubSetting GetGroupPolicySubSetting(
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
            return new Models.GroupPolicy.GroupPolicySubSetting
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
            Contracts.GroupPolicy.WindowsModernGroupPolicyObjectsSettings actual)
        {
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
            Models.GroupPolicy.GroupPolicySetting[] expectedSettings,
            GroupPolicySetting[] actualSettings)
        {
            var index = 0;
            foreach (var actualSetting in actualSettings)
            {
                var expectedSetting = expectedSettings[index];

                Assert.AreEqual(expectedSetting.Id, actualSetting.Id);
                Assert.AreEqual(expectedSetting.State, (GroupPolicySettingState)actualSetting.State);
                Assert.AreEqual(expectedSetting.Type, (GroupPolicySettingType)actualSetting.Type);
                VerifyGroupPolicySubSetting(expectedSetting.SubSettings, actualSetting.SubSettings);

                index++;
            }
        }

        private static void VerifyGroupPolicySubSetting(
            Models.GroupPolicy.GroupPolicySubSetting[] expectedSubSettings,
            GroupPolicySubSetting[] actualSubSettings)
        {
            var index = 0;
            foreach (var actualSubSetting in actualSubSettings)
            {
                var expectedSubSetting = expectedSubSettings[index];

                Assert.AreEqual(expectedSubSetting.Id, actualSubSetting.Id);
                Assert.AreEqual(expectedSubSetting.Value, actualSubSetting.Value);
                Assert.AreEqual(expectedSubSetting.Type, (GroupPolicySettingType)actualSubSetting.Type);
                Assert.AreEqual(expectedSubSetting.IsRequired, actualSubSetting.IsRequired);
                Assert.AreEqual(expectedSubSetting.MinLength, actualSubSetting.MinLength);
                Assert.AreEqual(expectedSubSetting.MaxLength, actualSubSetting.MaxLength);
                Assert.AreEqual(expectedSubSetting.DropdownItems, actualSubSetting.DropdownItems);
                Assert.AreEqual(expectedSubSetting.Regex, actualSubSetting.Regex);
                Assert.AreEqual(expectedSubSetting.ErrorMessage, actualSubSetting.ErrorMessage);

                index++;
            }
        }

        #endregion
    }
}
