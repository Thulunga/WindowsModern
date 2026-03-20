using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Soti.MobiControl.WebApi.Foundation.Mvc;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Soti.Api.Metadata.DataRetrieval;
using Soti.Csv;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Devices.DevInfo;
using Soti.MobiControl.Events;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Identity;
using Soti.MobiControl.WebApi.Foundation.InfoHeaders;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.WindowsModern.Web.Enums;
using Soti.MobiControl.WindowsModern.Web.Implementation.Exceptions;
using AntivirusThreatSeverity = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusThreatSeverity;
using AntivirusThreatStatus = Soti.MobiControl.WindowsModern.Web.Enums.AntivirusThreatStatus;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests;

[TestFixture]
public class WindowsDeviceControllerTests
{
    private const int DeviceId = 1000;
    private readonly string _devId = Guid.NewGuid().ToString();
    private static readonly string DeviceReferenceId1 = Guid.NewGuid().ToString();
    private static readonly string DeviceReferenceId2 = Guid.NewGuid().ToString();
    private static readonly IEnumerable<string> DeviceIdentities = new List<string> { DeviceReferenceId1, DeviceReferenceId2 };
    private static readonly Dictionary<string, int> DeviceIdsDictionary = new() { { DeviceReferenceId1, 1 }, { DeviceReferenceId2, 2 } };

    private Mock<IWindowsDeviceLocalUsersService> _windowsDeviceLocalUsersServiceMock;
    private Mock<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsServiceMock;
    private Mock<IAccessControlManager> _accessControlManagerMock;
    private Mock<IDeviceKeyInformationRetrievalService> _deviceKeyInformationRetrievalServiceMock;
    private Mock<IEventDispatcher> _eventDispatcherMock;
    private Mock<IUserIdentityProvider> _userIdentityProviderMock;
    private Mock<IDeviceBitLockerKeyService> _deviceBitLockerKeyServiceMock;
    private Mock<IWindowsDefenderService> _windowsDefenderServiceMock;
    private Mock<IDevInfoService> _devInfoServiceMock;
    private Mock<IWindowsModernDeviceConfigurationProxyService> _windowsModernDeviceConfigurationProxyServiceMock;
    private Mock<ICsvConverter> _csvConverterMock;
    private WindowsDeviceController _controller;

    [SetUp]
    public void Setup()
    {
        _windowsDeviceLocalUsersServiceMock = new Mock<IWindowsDeviceLocalUsersService>(MockBehavior.Strict);
        _windowsDeviceLocalGroupsServiceMock = new Mock<IWindowsDeviceLocalGroupsService>(MockBehavior.Strict);
        _accessControlManagerMock = new Mock<IAccessControlManager>(MockBehavior.Strict);
        _deviceKeyInformationRetrievalServiceMock =
            new Mock<IDeviceKeyInformationRetrievalService>(MockBehavior.Strict);
        _eventDispatcherMock = new Mock<IEventDispatcher>();
        _userIdentityProviderMock = new Mock<IUserIdentityProvider>();
        _deviceBitLockerKeyServiceMock = new Mock<IDeviceBitLockerKeyService>();
        _windowsDefenderServiceMock = new Mock<IWindowsDefenderService>();
        _devInfoServiceMock = new Mock<IDevInfoService>();
        _windowsModernDeviceConfigurationProxyServiceMock = new Mock<IWindowsModernDeviceConfigurationProxyService>();
        _csvConverterMock = new Mock<ICsvConverter>();
        _controller = new WindowsDeviceController(
            _deviceKeyInformationRetrievalServiceMock.Object,
            _deviceBitLockerKeyServiceMock.Object,
            _accessControlManagerMock.Object,
            _eventDispatcherMock.Object,
            _userIdentityProviderMock.Object,
            new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
            _windowsDefenderServiceMock.Object,
            _devInfoServiceMock.Object,
            new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
            new Lazy<ICsvConverter>(() => _csvConverterMock.Object))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public void Constructor_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                _deviceBitLockerKeyServiceMock.Object,
                _accessControlManagerMock.Object,
                _eventDispatcherMock.Object,
                _userIdentityProviderMock.Object,
                new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
                null,
                _windowsDefenderServiceMock.Object,
                _devInfoServiceMock.Object,
                new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                _deviceBitLockerKeyServiceMock.Object,
                _accessControlManagerMock.Object,
                _eventDispatcherMock.Object,
                _userIdentityProviderMock.Object,
                null,
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                _windowsDefenderServiceMock.Object,
                _devInfoServiceMock.Object,
                new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                _deviceBitLockerKeyServiceMock.Object,
                _accessControlManagerMock.Object,
                _eventDispatcherMock.Object,
                null,
                new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                _windowsDefenderServiceMock.Object,
                _devInfoServiceMock.Object,
                new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                _deviceBitLockerKeyServiceMock.Object,
                _accessControlManagerMock.Object,
                null,
                _userIdentityProviderMock.Object,
                new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                _windowsDefenderServiceMock.Object,
                _devInfoServiceMock.Object,
                new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                _deviceBitLockerKeyServiceMock.Object,
                null,
                _eventDispatcherMock.Object,
                _userIdentityProviderMock.Object,
                new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                _windowsDefenderServiceMock.Object,
                _devInfoServiceMock.Object,
                new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                null,
                _accessControlManagerMock.Object,
                _eventDispatcherMock.Object,
                _userIdentityProviderMock.Object,
                new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                _windowsDefenderServiceMock.Object,
                _devInfoServiceMock.Object,
                new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                null,
                _deviceBitLockerKeyServiceMock.Object,
                _accessControlManagerMock.Object,
                _eventDispatcherMock.Object,
                _userIdentityProviderMock.Object,
                new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                _windowsDefenderServiceMock.Object,
                _devInfoServiceMock.Object,
                new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                _deviceBitLockerKeyServiceMock.Object,
                _accessControlManagerMock.Object,
                _eventDispatcherMock.Object,
                _userIdentityProviderMock.Object,
                new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                null,
                _devInfoServiceMock.Object,
                new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                _deviceBitLockerKeyServiceMock.Object,
                _accessControlManagerMock.Object,
                _eventDispatcherMock.Object,
                _userIdentityProviderMock.Object,
                new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                _windowsDefenderServiceMock.Object,
                null,
                new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                _deviceBitLockerKeyServiceMock.Object,
                _accessControlManagerMock.Object,
                _eventDispatcherMock.Object,
                _userIdentityProviderMock.Object,
                new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                _windowsDefenderServiceMock.Object,
                _devInfoServiceMock.Object,
                null,
                new Lazy<ICsvConverter>(() => _csvConverterMock.Object)));
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDeviceController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                _deviceBitLockerKeyServiceMock.Object,
                _accessControlManagerMock.Object,
                _eventDispatcherMock.Object,
                _userIdentityProviderMock.Object,
                new Lazy<IWindowsDeviceLocalUsersService>(() => _windowsDeviceLocalUsersServiceMock.Object),
                new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object),
                _windowsDefenderServiceMock.Object,
                _devInfoServiceMock.Object,
                new Lazy<IWindowsModernDeviceConfigurationProxyService>(() => _windowsModernDeviceConfigurationProxyServiceMock.Object),
                null));
    }

    [Test]
    public void GetWindowsBitLockerKeys_ErrorTests()
    {
        Assert.Throws<ValidationException>(() => _controller.RequestWindowsBitLockerKeys(null));

        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns((DeviceKeyInformation)null);
        Assert.Throws<SecurityException>(() => _controller.RequestWindowsBitLockerKeys(_devId));
        _deviceKeyInformationRetrievalServiceMock.Verify(a => a.GetDeviceKeyInformation(_devId), Times.Once);

        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.Android));
        Assert.Throws<SecurityException>(() => _controller.RequestWindowsBitLockerKeys(_devId));
        _deviceKeyInformationRetrievalServiceMock.Verify(a => a.GetDeviceKeyInformation(_devId), Times.Exactly(2));
    }

    [Test]
    public void GetWindowsBitLockerKeys_Tests()
    {
        const string recoveryKeyId = "{6C0ABADA-DFDC-46CF-9ED8-70997336B7B8}";
        const string recoveryKey = "135652-039402-398794-547756-167629-716947-111012-392832";
        var bitLockerKeyModel = new DeviceBitLockerKey
        {
            DriveName = "C",
            RecoveryKeyId = Guid.Parse(recoveryKeyId),
            RecoveryKey = recoveryKey,
            DriveEncryptionStatus = Models.Enums.DriveEncryptionStatus.Encrypted,
            KeyProtectors = Models.Enums.BitLockerKeyProtectors.Tpm | Models.Enums.BitLockerKeyProtectors.RecoveryPassword,
            DriveType = Models.Enums.DriveType.System
        };
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId));
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _userIdentityProviderMock.Setup(a => a.GetUserIdentity()).Returns(new Security.Identity.Model.UserIdentity());
        _deviceBitLockerKeyServiceMock.Setup(a => a.GetBitLockerKeys(DeviceId)).Returns(new List<DeviceBitLockerKey> { bitLockerKeyModel });
        _eventDispatcherMock.Setup(a => a.DispatchEvent(It.IsAny<IEvent>()));

        var result = _controller.RequestWindowsBitLockerKeys(_devId).FirstOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual(result.DriveName, bitLockerKeyModel.DriveName);
        Assert.AreEqual(result.RecoveryKeyId, bitLockerKeyModel.RecoveryKeyId);
        Assert.AreEqual(result.RecoveryKey, bitLockerKeyModel.RecoveryKey);
        Assert.AreEqual(result.DriveEncryptionStatus, Soti.MobiControl.WindowsModern.Web.Enums.DriveEncryptionStatus.Encrypted);
        Assert.AreEqual(result.KeyProtectors, Soti.MobiControl.WindowsModern.Web.Enums.BitLockerKeyProtectors.Tpm | Soti.MobiControl.WindowsModern.Web.Enums.BitLockerKeyProtectors.RecoveryPassword);
        Assert.AreEqual(result.DriveType, Soti.MobiControl.WindowsModern.Web.Enums.DriveType.System);

        _deviceKeyInformationRetrievalServiceMock.Verify(a => a.GetDeviceKeyInformation(_devId), Times.Once);
        _deviceBitLockerKeyServiceMock.Verify(a => a.GetBitLockerKeys(DeviceId), Times.Once);
        _eventDispatcherMock.Verify(a => a.DispatchEvent(It.IsAny<IEvent>()), Times.Once);
        _userIdentityProviderMock.Verify(a => a.GetUserIdentity(), Times.Exactly(2));
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId), Times.Once);
    }

    [Test]
    public void Scenario_FetchBitLockerKeys_EncryptedSystemDrive_ReturnsExtendedFields()
    {
        GetWindowsBitLockerKeys_Tests();
    }

    [Test]
    public void Scenario_ViewRecoveryKey_LogsEvent()
    {
        GetWindowsBitLockerKeys_Tests();

        _eventDispatcherMock.Verify(a => a.DispatchEvent(It.IsAny<IEvent>()), Times.Once);
    }

    [Test]
    public void Scenario_FetchBitLockerKeys_MultipleDriveTypes_ReturnsAllVolumes()
    {
        var volumes = new List<DeviceBitLockerKey>
        {
            new() { DriveName = "C", RecoveryKeyId = Guid.NewGuid(), RecoveryKey = "system", DriveType = Models.Enums.DriveType.System },
            new() { DriveName = "D", RecoveryKeyId = Guid.NewGuid(), RecoveryKey = "fixed", DriveType = Models.Enums.DriveType.Fixed },
            new() { DriveName = "E", RecoveryKeyId = Guid.NewGuid(), RecoveryKey = "removable", DriveType = Models.Enums.DriveType.Removable }
        };

        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId));
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _userIdentityProviderMock.Setup(a => a.GetUserIdentity()).Returns(new Security.Identity.Model.UserIdentity());
        _deviceBitLockerKeyServiceMock.Setup(a => a.GetBitLockerKeys(DeviceId)).Returns(volumes);
        _eventDispatcherMock.Setup(a => a.DispatchEvent(It.IsAny<IEvent>()));

        var result = _controller.RequestWindowsBitLockerKeys(_devId).ToList();

        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void Scenario_FetchBitLockerKeys_NoVolumes_ReturnsEmptyCollection()
    {
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId));
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _userIdentityProviderMock.Setup(a => a.GetUserIdentity()).Returns(new Security.Identity.Model.UserIdentity());
        _deviceBitLockerKeyServiceMock.Setup(a => a.GetBitLockerKeys(DeviceId)).Returns(Array.Empty<DeviceBitLockerKey>());
        _eventDispatcherMock.Setup(a => a.DispatchEvent(It.IsAny<IEvent>()));

        var result = _controller.RequestWindowsBitLockerKeys(_devId).ToList();

        Assert.That(result, Is.Empty);
        _eventDispatcherMock.Verify(a => a.DispatchEvent(It.IsAny<IEvent>()), Times.Once);
    }

    [TestCaseSource(nameof(GetInvalidDeviceIdOrSid))]
    public void GetDeviceLocalUserPassword_ValidationException(string deviceId, string sid)
    {
        Assert.Throws<ValidationException>(() => _controller.GetLocalUserPassword(deviceId, sid));
    }

    [Test]
    public void GetDeviceLocalUserPassword_Success()
    {
        var windowsLocalUserNameAndPasswordModel = new WindowsLocalUserNameAndPasswordModel
        {
            UserName = "userName1",
            AutoGeneratedPassword = "password1"
        };

        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId));

        _windowsDeviceLocalUsersServiceMock.Setup(q => q.GetLocalUserPasswordByDeviceIdAndSid(It.IsAny<int>(), It.IsAny<string>())).Returns(windowsLocalUserNameAndPasswordModel);
        var result = _controller.GetLocalUserPassword(_devId, "SID");
        Assert.AreEqual(windowsLocalUserNameAndPasswordModel.AutoGeneratedPassword, result.AutoGeneratedPassword);

        _windowsDeviceLocalUsersServiceMock.Verify(q => q.GetLocalUserPasswordByDeviceIdAndSid(DeviceId, "SID"), Times.Once());
        _deviceKeyInformationRetrievalServiceMock.Verify(a => a.GetDeviceKeyInformation(_devId), Times.Once);
        _accessControlManagerMock.Verify(x => x.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId), Times.Once);
    }

    [Test]
    public void GetDeviceLocalUserPassword_SecurityException()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId));

        _windowsDeviceLocalUsersServiceMock.Setup(q => q.GetLocalUserPasswordByDeviceIdAndSid(It.IsAny<int>(), It.IsAny<string>())).Returns(null as WindowsLocalUserNameAndPasswordModel);

        Assert.Throws<SecurityException>(() => _controller.GetLocalUserPassword(_devId, "SID"));

        _windowsDeviceLocalUsersServiceMock.Verify(q => q.GetLocalUserPasswordByDeviceIdAndSid(DeviceId, "SID"), Times.Once());
    }

    [TestCaseSource(nameof(GetInvalidDeviceId))]
    public void GetDeviceLocalUsers_ValidationException(string deviceId)
    {
        Assert.Throws<ValidationException>(() => _controller.GetDeviceLocalUsers(deviceId));
    }

    [TestCaseSource(nameof(GetInvalidDeviceId))]
    public void GetDeviceLocalGroups_ValidationException(string deviceId)
    {
        Assert.Throws<ValidationException>(() => _controller.GetDeviceLocalGroups(deviceId));
    }

    [Test]
    public void GetDeviceLocalUsers_Success()
    {
        var localUserModel = new DeviceLocalUserSummary()
        {
            Sid = "Sid1",
            UserName = "userName1",
            UserGroups = new List<string> { "s1", "s2" },
            IsMobiControlCreated = false
        };
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId));
        _windowsDeviceLocalUsersServiceMock.Setup(q => q.GetDeviceLocalUsersSummaryInfo(It.IsAny<int>())).Returns(new List<DeviceLocalUserSummary> { localUserModel });
        var result = _controller.GetDeviceLocalUsers(_devId).ToList();
        Assert.AreNotEqual(null, result);
        Assert.AreEqual(result.FirstOrDefault().UserName, localUserModel.UserName);
        Assert.AreEqual(result.FirstOrDefault().IsMobiControlCreated, localUserModel.IsMobiControlCreated);
        _windowsDeviceLocalUsersServiceMock.Verify(q => q.GetDeviceLocalUsersSummaryInfo(DeviceId), Times.Once());
    }

    [Test]
    public void ExportAntivirusThreatHistory_Success()
    {
        var dummyData = "dummyData";
        var userId = 1;
        var userName = "test";
        var expectedData = new List<AntivirusDeviceThreatInfo>
        {
            new()
            {
                Type = Models.Enums.AntivirusThreatType.Malware,
                InitialDetectionTime = DateTime.Now.AddDays(-5),
                LastStatusChangeTime = DateTime.Now,
                LastThreatStatus = Models.Enums.AntivirusThreatStatus.ManualStepsRequired,
                ThreatName = "Test Threat",
                CurrentDetectionCount = 3,
                Severity = Models.Enums.AntivirusThreatSeverity.High,
                ExternalThreatId = 1,
            }
        };

        EnsureDeviceGroupPermission();
        _windowsModernDeviceConfigurationProxyServiceMock.Setup(q => q.IsAntivirusPayloadAssigned(DeviceId)).Returns(true);
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _windowsDefenderServiceMock.Setup(x => x.GetAllAntivirusThreatHistory(DeviceId)).Returns(expectedData);
        _csvConverterMock.Setup(m => m.GenerateCsvContent(
                It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                It.IsAny<string[]>(),
                It.IsAny<Stream>(),
                It.IsAny<int>(),
                It.IsAny<CultureInfo>(),
                It.IsAny<Func<string, string>>(),
                It.IsAny<bool>()))
            .Callback<IEnumerable<IDictionary<string, string>>, string[], Stream, int, CultureInfo, Func<string, string>, bool>(
                (records, headers, stream, offset, culture, localize, closeStream) =>
                {
                    using var writer = new StreamWriter(stream);
                    writer.WriteLine(localize(dummyData));
                    writer.FlushAsync();
                })
            .Returns(Task.CompletedTask);

        _eventDispatcherMock.Setup(e => e.DispatchEvent(It.IsAny<IEvent>()));

        _userIdentityProviderMock.Setup(u => u.GetUserIdentity()).Returns(new Security.Identity.Model.UserIdentity { UserId = userId, UserName = userName });
        var result = _controller.ExportDeviceAntivirusThreatHistory(_devId, 0);
        var response = _controller.Response;
        var pushStreamResult = result as PushStreamResult;
        Assert.NotNull(pushStreamResult);
        _eventDispatcherMock.Verify(a => a.DispatchEvent(It.IsAny<IEvent>()), Times.Once);
        _windowsDefenderServiceMock.Verify(s => s.GetAllAntivirusThreatHistory(It.IsAny<int>()), Times.Once);
    }

    [Test]
    public void ExportAntivirusThreatHistory_ValidationException_WhenDeviceIdNull()
    {
        Assert.Throws<ValidationException>(() => _controller.ExportDeviceAntivirusThreatHistory(null, 0));
    }

    [Test]
    public void ExportAntivirusThreatHistory_SecurityException_WhenDeviceNotFound()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(x => x.GetDeviceKeyInformation(It.IsAny<string>())).Returns<DeviceKeyInformation>(null);
        Assert.Throws<SecurityException>(() => _controller.ExportDeviceAntivirusThreatHistory(_devId, 0));
    }

    [Test]
    public void ExportAntivirusThreatHistory_ThrowsWindowsDefenderPayloadNotAssigned_WhenPayloadIsNotAssigned()
    {
        EnsureDeviceGroupPermission();
        _deviceKeyInformationRetrievalServiceMock.Setup(x => x.GetDeviceKeyInformation(It.IsAny<string>())).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _windowsModernDeviceConfigurationProxyServiceMock.Setup(q => q.IsAntivirusPayloadAssigned(DeviceId)).Returns(false);
        Assert.Throws<WindowsModernWebException>(() => _controller.ExportDeviceAntivirusThreatHistory(_devId, 0));
    }

    [Test]
    public void ExportAntivirusThreatHistory_SecurityException_WhenUnsupportedPlatform()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(x => x.GetDeviceKeyInformation(It.IsAny<string>())).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.iOS));
        Assert.Throws<SecurityException>(() => _controller.ExportDeviceAntivirusThreatHistory(_devId, 0));
    }

    [Test]
    public void GetAntivirusThreatHistory_NullInput_ReturnsCorrectResult()
    {
        var totalCount = 1;
        EnsureDeviceGroupPermission();
        var expectedData = new List<AntivirusDeviceThreatInfo>
        {
            new()
            {
                Type = Models.Enums.AntivirusThreatType.Malware,
                InitialDetectionTime = DateTime.Now.AddDays(-5),
                LastStatusChangeTime = DateTime.Now,
                LastThreatStatus = Models.Enums.AntivirusThreatStatus.ManualStepsRequired,
                ThreatName = "Test Threat",
                CurrentDetectionCount = 3,
                Severity = Models.Enums.AntivirusThreatSeverity.High,
                ExternalThreatId = 1,
            }
        };

        _windowsModernDeviceConfigurationProxyServiceMock
        .Setup(x => x.IsAntivirusPayloadAssigned(DeviceId))
        .Returns(true);

        _windowsDefenderServiceMock
            .Setup(x => x.GetAntivirusThreatHistory(
                It.IsAny<int>(),
                It.IsAny<IEnumerable<Models.Enums.AntivirusThreatType>>(),
                It.IsAny<IEnumerable<Models.Enums.AntivirusThreatSeverity>>(),
                It.IsAny<IEnumerable<Models.Enums.AntivirusThreatStatus>>(),
                null,
                null,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AntivirusThreatSortByOption>(),
                It.IsAny<bool>(),
                out totalCount))
            .Returns(expectedData);

        var result = _controller.GetAntivirusThreatHistory(
            _devId,
            null,
            null,
            null,
            null,
            null
        );

        Assert.NotNull(result);
        Assert.AreEqual(expectedData.Select(x => x.ThreatName), result.Select(x => x.Name));
        Assert.AreEqual(expectedData.Select(x => (AntivirusThreatCategory)x.Type), result.Select(x => x.Category));
        Assert.AreEqual(expectedData.Select(x => (AntivirusThreatSeverity)x.Severity), result.Select(x => x.Severity));
        Assert.AreEqual(expectedData.Select(x => (AntivirusThreatStatus)x.LastThreatStatus), result.Select(x => x.LastThreatStatus));
        Assert.AreEqual(expectedData.Select(x => x.InitialDetectionTime), result.Select(x => x.InitialDetectionTime));
        Assert.AreEqual(expectedData.Select(x => x.CurrentDetectionCount), result.Select(x => x.NumberOfDetectionsTillDate));
        Assert.AreEqual(expectedData.Select(x => x.ExternalThreatId), result.Select(x => x.ThreatId));
        _windowsDefenderServiceMock.Verify(x => x.GetAntivirusThreatHistory(
            It.IsAny<int>(),
            It.IsAny<IEnumerable<Models.Enums.AntivirusThreatType>>(),
            It.IsAny<IEnumerable<Models.Enums.AntivirusThreatSeverity>>(),
            It.IsAny<IEnumerable<Models.Enums.AntivirusThreatStatus>>(),
            null,
            null,
            0,
            50,
            It.IsAny<AntivirusThreatSortByOption>(),
            It.IsAny<bool>(),
            out totalCount), Times.Once);
    }

    [Test]
    public void SetInfoHeader_Throws_Exception()
    {
        Assert.Throws<NotSupportedException>(() => ResponseInfoHeaders.SetInfoHeader((ControllerBase)_controller, (InfoHeaderType)100, 5));
    }

    [Test]
    public void GetAntivirusThreatHistory_ValidInput_ReturnsCorrectResult()
    {
        var totalCount = 1;
        EnsureDeviceGroupPermission();
        var expectedData = new List<AntivirusDeviceThreatInfo>
        {
            new()
            {
                Type = Models.Enums.AntivirusThreatType.Malware,
                InitialDetectionTime = DateTime.Now.AddDays(-5),
                LastStatusChangeTime = DateTime.Now,
                LastThreatStatus = Models.Enums.AntivirusThreatStatus.ManualStepsRequired,
                ThreatName = "Test Threat",
                CurrentDetectionCount = 3,
                Severity = Models.Enums.AntivirusThreatSeverity.High,
                ExternalThreatId = 1,
            }
        };

        var startDateTime = DateTime.Now.AddDays(-7);
        var endDateTime = DateTime.Now.AddDays(-1);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };

        _windowsModernDeviceConfigurationProxyServiceMock
        .Setup(x => x.IsAntivirusPayloadAssigned(DeviceId))
        .Returns(true);

        _windowsDefenderServiceMock
            .Setup(x => x.GetAntivirusThreatHistory(
                It.IsAny<int>(),
                It.IsAny<IEnumerable<Models.Enums.AntivirusThreatType>>(),
                It.IsAny<IEnumerable<Models.Enums.AntivirusThreatSeverity>>(),
                It.IsAny<IEnumerable<Models.Enums.AntivirusThreatStatus>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AntivirusThreatSortByOption>(),
                It.IsAny<bool>(),
                out totalCount
            ))
            .Returns(expectedData);

        var result = _controller.GetAntivirusThreatHistory(
            _devId,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory> { AntivirusThreatCategory.Malware },
            new List<AntivirusThreatSeverity> { AntivirusThreatSeverity.Severe },
            new List<AntivirusThreatStatus> { AntivirusThreatStatus.ManualStepsRequired },
            dataRetrievalOptions
        );

        Assert.NotNull(result);
        Assert.AreEqual(expectedData.Select(x => x.ThreatName), result.Select(x => x.Name));
        Assert.AreEqual(expectedData.Select(x => (AntivirusThreatCategory)x.Type), result.Select(x => x.Category));
        Assert.AreEqual(expectedData.Select(x => (AntivirusThreatSeverity)x.Severity), result.Select(x => x.Severity));
        Assert.AreEqual(expectedData.Select(x => (AntivirusThreatStatus)x.LastThreatStatus), result.Select(x => x.LastThreatStatus));
        Assert.AreEqual(expectedData.Select(x => x.InitialDetectionTime), result.Select(x => x.InitialDetectionTime));
        Assert.AreEqual(expectedData.Select(x => x.CurrentDetectionCount), result.Select(x => x.NumberOfDetectionsTillDate));
        Assert.AreEqual(expectedData.Select(x => x.ExternalThreatId), result.Select(x => x.ThreatId));
        _windowsDefenderServiceMock.Verify(x => x.GetAntivirusThreatHistory(
            It.IsAny<int>(),
            It.IsAny<IEnumerable<Models.Enums.AntivirusThreatType>>(),
            It.IsAny<IEnumerable<Models.Enums.AntivirusThreatSeverity>>(),
            It.IsAny<IEnumerable<Models.Enums.AntivirusThreatStatus>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<AntivirusThreatSortByOption>(),
            It.IsAny<bool>(),
            out totalCount), Times.Once);
    }

    [Test]
    public void GetAntivirusThreatHistory_InvalidStatusChangeTimeRange_ThrowsValidationException()
    {
        EnsureDeviceGroupPermission();

        var endDateTime = DateTime.Now.AddDays(-1);
        var startDateTime = DateTime.Now;

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };

        var ex = Assert.Throws<ValidationException>(() => _controller.GetAntivirusThreatHistory(
            _devId,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory>(),
            new List<AntivirusThreatSeverity>(),
            new List<AntivirusThreatStatus>(),
            dataRetrievalOptions));

        Assert.AreEqual("Start time cannot be later than the end time.", ex.Message);

        startDateTime = DateTime.UtcNow;
        endDateTime = DateTime.MinValue;
        Assert.Throws<ValidationException>(() => _controller.GetAntivirusThreatHistory(
            _devId,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory>(),
            new List<AntivirusThreatSeverity>(),
            new List<AntivirusThreatStatus>(),
            dataRetrievalOptions
            ));
    }

    [Test]
    public void GetAntivirusThreatHistory_InvalidSeverityEnum_ThrowsValidationException()
    {
        EnsureDeviceGroupPermission();
        var invalidSeverity = new List<AntivirusThreatSeverity> { (AntivirusThreatSeverity)999 };
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };

        var ex = Assert.Throws<ValidationException>(() => _controller.GetAntivirusThreatHistory(
            _devId,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory>(),
            invalidSeverity,
            new List<AntivirusThreatStatus>(),
            dataRetrievalOptions
            ));

        Assert.AreEqual("One or more values in the 'severities' parameter are invalid.", ex?.Message);
    }

    [Test]
    public void GetAntivirusThreatHistory_NullCategoryEnum_SetDefault()
    {
        EnsureDeviceGroupPermission();
        var invalidCategory = new List<AntivirusThreatCategory> { (AntivirusThreatCategory)999 };
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };

        var ex = Assert.Throws<ValidationException>(() => _controller.GetAntivirusThreatHistory(
            _devId,
            startDateTime,
            endDateTime,
            invalidCategory, new List<AntivirusThreatSeverity>(), new List<AntivirusThreatStatus>(),
            dataRetrievalOptions
            ));

        Assert.AreEqual("One or more values in the 'categories' parameter are invalid.", ex?.Message);
    }

    [Test]
    public void GetAntivirusThreatHistory_InvalidThreatStatusEnum_ThrowsValidationException()
    {
        EnsureDeviceGroupPermission();
        var invalidThreatStatus = new List<AntivirusThreatStatus> { (AntivirusThreatStatus)999 };
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 10
        };

        var ex = Assert.Throws<ValidationException>(() => _controller.GetAntivirusThreatHistory(
            _devId,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory>(), new List<AntivirusThreatSeverity>(), invalidThreatStatus, dataRetrievalOptions));

        Assert.AreEqual("One or more values in the 'lastThreatStatuses' parameter are invalid.", ex?.Message);
    }

    [Test]
    public void GetAntivirusThreatHistory_SkipAndTake_ThrowsValidationException()
    {
        EnsureDeviceGroupPermission();
        var endDateTime = DateTime.Now;
        var startDateTime = DateTime.Now.AddDays(-5);

        var dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = -1,
            Take = 10
        };

        var skip = Assert.Throws<ValidationException>(() => _controller.GetAntivirusThreatHistory(
            _devId,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory>(), new List<AntivirusThreatSeverity>(), new List<AntivirusThreatStatus>(), dataRetrievalOptions));

        Assert.AreEqual("Skip value should be greater than or equal to zero.", skip.Message);

        dataRetrievalOptions = new DataRetrievalOptions
        {
            Skip = 0,
            Take = 0
        };

        var take = Assert.Throws<ValidationException>(() => _controller.GetAntivirusThreatHistory(
            _devId,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory>(), new List<AntivirusThreatSeverity>(), new List<AntivirusThreatStatus>(), dataRetrievalOptions));

        Assert.AreEqual("Take value should be greater than zero", take.Message);
        dataRetrievalOptions = new DataRetrievalOptions
        {
            Order = new DataRetrievalOrder[] {
                new DataRetrievalOrder {By = "LastStatusChangeTime", Descending = true },
                new DataRetrievalOrder {By = "LastStatusChangeTime", Descending = true }
            },
            Skip = 0,
            Take = 10
        };

        Assert.Throws<ValidationException>(() => _controller.GetAntivirusThreatHistory(
            _devId,
            startDateTime,
            endDateTime,
            new List<AntivirusThreatCategory>(),
            new List<AntivirusThreatSeverity>(),
            new List<AntivirusThreatStatus>(),
            dataRetrievalOptions));
    }

    [Test]
    public void GetDeviceLocalGroups_Success()
    {
        var localGroupModel = new DeviceLocalGroupSummary()
        {
            GroupName = "GroupName1",
            IsAdminGroup = true,
        };
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.ManageDeviceUser, SecurityAssetType.Device, DeviceId));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DeviceGroupPermission.ViewGroup, SecurityAssetType.Device, DeviceId));
        _windowsDeviceLocalGroupsServiceMock.Setup(q => q.GetDeviceLocalGroupsSummaryInfo(It.IsAny<int>())).Returns(new List<DeviceLocalGroupSummary> { localGroupModel });
        var result = _controller.GetDeviceLocalGroups(_devId);
        Assert.AreNotEqual(null, result);
        Assert.AreEqual(result.FirstOrDefault()?.GroupName, localGroupModel.GroupName);
        _windowsDeviceLocalGroupsServiceMock.Verify(q => q.GetDeviceLocalGroupsSummaryInfo(DeviceId), Times.Once());
    }

    [Test]
    public void GetAntivirusScanSummary_ThrowsWindowsDefenderPayloadNotAssigned_WhenPayloadIsNotAssigned()
    {
        EnsureDeviceGroupPermission();
        var device = new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10);

        _deviceKeyInformationRetrievalServiceMock
            .Setup(x => x.GetDeviceKeyInformation(_devId))
            .Returns(device);

        _windowsModernDeviceConfigurationProxyServiceMock
            .Setup(x => x.IsAntivirusPayloadAssigned(DeviceId))
            .Returns(false);

        Assert.Throws<WindowsModernWebException>(() => _controller.GetAntivirusScanSummary(_devId));
    }

    [Test]
    public void GetAntivirusScanSummary_ThrowsDeviceFamilyUnsupported_WhenDeviceIsNotSupported()
    {
        var device = new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.iOS);

        _deviceKeyInformationRetrievalServiceMock
            .Setup(x => x.GetDeviceKeyInformation(_devId))
            .Returns(device);

        Assert.Throws<SecurityException>(() => _controller.GetAntivirusScanSummary(_devId));
    }

    [Test]
    public void GetAntivirusScanSummary_ToContract_NullException()
    {
        var device = new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10);
        EnsureDeviceGroupPermission();

        _deviceKeyInformationRetrievalServiceMock
            .Setup(x => x.GetDeviceKeyInformation(_devId))
            .Returns(device);

        _windowsModernDeviceConfigurationProxyServiceMock
            .Setup(x => x.IsAntivirusPayloadAssigned(DeviceId))
            .Returns(true);

        _windowsDefenderServiceMock
            .Setup(x => x.GetAntivirusScanSummary(DeviceId))
            .Returns((AntivirusScanSummary)null);

        Assert.Throws<ArgumentNullException>(() => _controller.GetAntivirusScanSummary(_devId));

        _windowsDefenderServiceMock.Verify(x => x.GetAntivirusScanSummary(DeviceId), Times.Once);
    }

    [Test]
    public void GetAntivirusScanSummary_ReturnsAntivirusScanSummary()
    {
        var device = new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10);
        var lastFullScanTime = DateTime.Now.AddDays(-2);
        var lastQuickScanTime = DateTime.Now.AddDays(-1);
        var threatStatusCount = new Dictionary<Models.Enums.AntivirusThreatStatus, int>()
        {
            { Models.Enums.AntivirusThreatStatus.Active, 1 },
            { Models.Enums.AntivirusThreatStatus.ActionFailed, 0 },
            { Models.Enums.AntivirusThreatStatus.ManualStepsRequired, 1 },
            { Models.Enums.AntivirusThreatStatus.FullScanRequired, 1 },
            { Models.Enums.AntivirusThreatStatus.RebootRequired, 0 },
            { Models.Enums.AntivirusThreatStatus.RemediatedWithNonCriticalFailures, 0 },
            { Models.Enums.AntivirusThreatStatus.Quarantined, 1 },
            { Models.Enums.AntivirusThreatStatus.Removed, 1 },
            { Models.Enums.AntivirusThreatStatus.Cleaned, 0 },
            { Models.Enums.AntivirusThreatStatus.Allowed, 0 },
            { Models.Enums.AntivirusThreatStatus.NoStatusCleared, 1 }
        };
        var antivirusScanSummary = new AntivirusScanSummary
        {
            IsThreatsAvailable = true,
            LastFullScanTime = lastFullScanTime,
            LastQuickScanTime = lastQuickScanTime,
            ThreatStatusCountSummary = threatStatusCount
        };

        EnsureDeviceGroupPermission();

        _deviceKeyInformationRetrievalServiceMock
            .Setup(x => x.GetDeviceKeyInformation(_devId))
            .Returns(device);

        _windowsModernDeviceConfigurationProxyServiceMock
            .Setup(x => x.IsAntivirusPayloadAssigned(DeviceId))
            .Returns(true);

        _windowsDefenderServiceMock
            .Setup(x => x.GetAntivirusScanSummary(DeviceId))
            .Returns(antivirusScanSummary);

        var result = _controller.GetAntivirusScanSummary(_devId);

        Assert.IsNotNull(result);
        Assert.AreEqual(result.IsThreatsAvailable, antivirusScanSummary.IsThreatsAvailable);
        Assert.AreEqual(result.LastFullScanTime, antivirusScanSummary.LastFullScanTime);
        Assert.AreEqual(result.LastQuickScanTime, antivirusScanSummary.LastQuickScanTime);

        _windowsDefenderServiceMock.Verify(x => x.GetAntivirusScanSummary(DeviceId), Times.Once);
    }

    private static IEnumerable<TestCaseData> GetInvalidDeviceIdOrSid()
    {
        yield return new TestCaseData("deviceId", null);
        yield return new TestCaseData("deviceId", string.Empty);
        yield return new TestCaseData("deviceId", " ");
    }

    private void EnsureDeviceGroupPermission()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId));
    }

    private static IEnumerable<TestCaseData> GetInvalidDeviceId()
    {
        yield return new TestCaseData(null);
        yield return new TestCaseData(string.Empty);
        yield return new TestCaseData(" ");
    }

    private static ICollection<DeviceModel> DeviceModelsTestCollection(bool isDeviceKindWindows = true)
    {
        return new List<DeviceModel>()
        {
            new()
            {
                DeviceId = 1,
                DeviceKindId = isDeviceKindWindows ? (byte)DeviceKind.WindowsDesktop : (byte)2,
            },
            new()
            {
                DeviceId = 2,
                DeviceKindId = 3,
            }
        };
    }
}
