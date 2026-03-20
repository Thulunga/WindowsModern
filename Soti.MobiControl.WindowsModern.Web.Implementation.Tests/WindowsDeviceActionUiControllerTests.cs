using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security;
using Moq;
using NUnit.Framework;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Devices.DevInfo;
using Soti.MobiControl.Events;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Identity;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Web.Implementation.Events;
using Soti.MobiControl.WindowsModern.Web.Implementation.Exceptions;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests;

[TestFixture]
internal class WindowsDeviceActionUiControllerTests
{
    private const int DevId1 = 1;
    private const string DeviceId1 = "DEVICE_ID_1";
    private static readonly string DeviceReferenceId1 = Guid.NewGuid().ToString();
    private static readonly string DeviceReferenceId2 = Guid.NewGuid().ToString();
    private static readonly IEnumerable<string> DeviceIdentities = new List<string> { DeviceReferenceId1, DeviceReferenceId2 };
    private static readonly Dictionary<string, int> DeviceIdsDictionary = new() { { DeviceReferenceId1, 1 }, { DeviceReferenceId2, 2 } };

    private Mock<IDeviceKeyInformationRetrievalService> _deviceKeyInformationRetrievalServiceMock;
    private Mock<IEventDispatcher> _eventDispatcherMock;
    private Mock<IWindowsDeviceService> _windowsDeviceServiceMock;
    private Mock<IUserIdentityProvider> _userIdentityProviderMock;
    private Mock<IAccessControlManager> _accessControlManagerMock;
    private Mock<IDevInfoService> _devInfoServiceMock;
    private Mock<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsServiceMock;
    private IWindowsDeviceActionUiController _controller;

    [SetUp]
    public void Setup()
    {
        _deviceKeyInformationRetrievalServiceMock = new Mock<IDeviceKeyInformationRetrievalService>(MockBehavior.Strict);
        _eventDispatcherMock = new Mock<IEventDispatcher>(MockBehavior.Strict);
        _windowsDeviceServiceMock = new Mock<IWindowsDeviceService>(MockBehavior.Strict);
        _accessControlManagerMock = new Mock<IAccessControlManager>(MockBehavior.Strict);
        _userIdentityProviderMock = new Mock<IUserIdentityProvider>(MockBehavior.Strict);
        _devInfoServiceMock = new Mock<IDevInfoService>();
        _windowsDeviceLocalGroupsServiceMock = new Mock<IWindowsDeviceLocalGroupsService>(MockBehavior.Strict);
        _controller = new WindowsDeviceActionUiController(
            _deviceKeyInformationRetrievalServiceMock.Object,
            _userIdentityProviderMock.Object,
            _accessControlManagerMock.Object,
            _eventDispatcherMock.Object,
            _windowsDeviceServiceMock.Object,
            _devInfoServiceMock.Object,
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object));
    }

    #region Constructor

    [Test]
    public void Constructor_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceActionUiController(
            null,
            _userIdentityProviderMock.Object,
            _accessControlManagerMock.Object,
            _eventDispatcherMock.Object,
            _windowsDeviceServiceMock.Object,
            _devInfoServiceMock.Object,
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceActionUiController(
            _deviceKeyInformationRetrievalServiceMock.Object,
            null,
            _accessControlManagerMock.Object,
            _eventDispatcherMock.Object,
            _windowsDeviceServiceMock.Object,
            _devInfoServiceMock.Object,
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceActionUiController(
            _deviceKeyInformationRetrievalServiceMock.Object,
            _userIdentityProviderMock.Object,
            null,
            _eventDispatcherMock.Object,
            _windowsDeviceServiceMock.Object,
            _devInfoServiceMock.Object,
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceActionUiController(
            _deviceKeyInformationRetrievalServiceMock.Object,
            _userIdentityProviderMock.Object,
            _accessControlManagerMock.Object,
            null,
            _windowsDeviceServiceMock.Object,
            _devInfoServiceMock.Object,
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceActionUiController(
            _deviceKeyInformationRetrievalServiceMock.Object,
            _userIdentityProviderMock.Object,
            _accessControlManagerMock.Object,
            _eventDispatcherMock.Object,
            null,
            _devInfoServiceMock.Object,
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceActionUiController(
            _deviceKeyInformationRetrievalServiceMock.Object,
            _userIdentityProviderMock.Object,
            _accessControlManagerMock.Object,
            _eventDispatcherMock.Object,
            _windowsDeviceServiceMock.Object,
            null,
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceActionUiController(
            _deviceKeyInformationRetrievalServiceMock.Object,
            _userIdentityProviderMock.Object,
            _accessControlManagerMock.Object,
            _eventDispatcherMock.Object,
            _windowsDeviceServiceMock.Object,
            _devInfoServiceMock.Object,
            null));
    }

    #endregion

    #region RequestLockPasscode

    [Test]
    public void RequestLockPasscode_Throws()
    {
        Assert.Throws<ValidationException>(() => _controller.RequestLockPasscode(null));

        _deviceKeyInformationRetrievalServiceMock.Setup(service => service.GetDeviceKeyInformation(DeviceId1)).Returns(It.IsAny<DeviceKeyInformation>());
        Assert.Throws<SecurityException>(() => _controller.RequestLockPasscode(DeviceId1));
        _deviceKeyInformationRetrievalServiceMock.Verify(service => service.GetDeviceKeyInformation(DeviceId1), Times.Once);

        _deviceKeyInformationRetrievalServiceMock.Invocations.Clear();
        _deviceKeyInformationRetrievalServiceMock.Setup(service => service.GetDeviceKeyInformation(DeviceId1)).Returns(new DeviceKeyInformation(DevId1, DeviceId1, DevicePlatform.Android));
        Assert.Throws<SecurityException>(() => _controller.RequestLockPasscode(DeviceId1));
        _deviceKeyInformationRetrievalServiceMock.Verify(service => service.GetDeviceKeyInformation(DeviceId1), Times.Once);
    }

    [Test]
    public void RequestLockPasscode_Success()
    {
        var testPasscode = "123456";
        var lockedWindowsDeviceModel = GetWindowsDeviceModel(DevId1, true, testPasscode);

        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DevId1));
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DevicePermission>(), SecurityAssetType.Device, DevId1));
        _userIdentityProviderMock.Setup(a => a.GetUserIdentity()).Returns(new Security.Identity.Model.UserIdentity());
        _deviceKeyInformationRetrievalServiceMock.Setup(service => service.GetDeviceKeyInformation(DeviceId1)).Returns(new DeviceKeyInformation(DevId1, DeviceId1, DevicePlatform.WindowsDesktop10RS1));
        _windowsDeviceServiceMock.Setup(service => service.GetByDeviceId(DevId1)).Returns(lockedWindowsDeviceModel);
        _eventDispatcherMock.Setup(a => a.DispatchEvent(It.IsAny<IEvent>()));

        var lockDeviceResult = _controller.RequestLockPasscode(DeviceId1);
        Assert.IsNotNull(lockDeviceResult);
        Assert.AreEqual(testPasscode, lockDeviceResult);
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(DeviceGroupPermission.ViewGroup, SecurityAssetType.Device, DevId1), Times.Once);
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(DevicePermission.DisablePasscodeLock, SecurityAssetType.Device, DevId1), Times.Once);

        _deviceKeyInformationRetrievalServiceMock.Verify(service => service.GetDeviceKeyInformation(DeviceId1), Times.Once);
        _eventDispatcherMock.Verify(ed => ed.DispatchEvent(It.IsAny<ViewedLockPasscodeEvent>()), Times.Once);
        _windowsDeviceServiceMock.Verify(service => service.GetByDeviceId(DevId1), Times.Once);

        _deviceKeyInformationRetrievalServiceMock.Invocations.Clear();
        _eventDispatcherMock.Invocations.Clear();
        _windowsDeviceServiceMock.Invocations.Clear();

        var unlockedWindowsDeviceModel = GetWindowsDeviceModel(DevId1);
        _windowsDeviceServiceMock.Setup(service => service.GetByDeviceId(DevId1)).Returns(unlockedWindowsDeviceModel);

        var unlockedDeviceResult = _controller.RequestLockPasscode(DeviceId1);
        Assert.That(unlockedDeviceResult, Is.Null);

        _deviceKeyInformationRetrievalServiceMock.Verify(service => service.GetDeviceKeyInformation(DeviceId1), Times.Once);
        _eventDispatcherMock.Verify(ed => ed.DispatchEvent(It.IsAny<ViewedLockPasscodeEvent>()), Times.Once);
        _windowsDeviceServiceMock.Verify(service => service.GetByDeviceId(DevId1), Times.Once);
    }

    #endregion

    #region GetDevicesLocalGroups

    [Test]
    public void GetDevicesLocalGroups_Throws_ValidationException()
    {
        Assert.Throws<ValidationException>(() => _controller.GetDevicesLocalGroups(null));
        Assert.Throws<ValidationException>(() => _controller.GetDevicesLocalGroups(new List<string>()));
    }

    [Test]
    public void GetDevicesLocalGroups_Throws_SecurityException()
    {
        _devInfoServiceMock.Setup(x => x.FindDeviceIdsByDevIds(It.IsAny<IEnumerable<string>>()))
            .Returns(new Dictionary<string, int> { { DeviceReferenceId1, 1 } }).Verifiable();

        Assert.Throws<SecurityException>(() => _controller.GetDevicesLocalGroups(DeviceIdentities));

        _devInfoServiceMock.Verify(x => x.FindDeviceIdsByDevIds(DeviceIdentities), Times.Once);
    }

    [Test]
    public void GetDevicesLocalGroups_Throws_BLException()
    {
        _devInfoServiceMock.Setup(x => x.FindDeviceIdsByDevIds(It.IsAny<IEnumerable<string>>()))
            .Returns(DeviceIdsDictionary).Verifiable();
        _devInfoServiceMock.Setup(x => x.GetByDeviceIds(It.IsAny<int[]>()))
            .Returns(DeviceModelsTestCollection(false)).Verifiable();
        _accessControlManagerMock.Setup(x => x.EnsureHasAnyAccessRights(
            It.IsAny<HashSet<SecurityPermission>>(),
            SecurityAssetType.Device,
            It.IsAny<HashSet<int>>())).Verifiable();

        Assert.Throws<WindowsModernWebException>(() => _controller.GetDevicesLocalGroups(DeviceIdentities));

        _devInfoServiceMock.Verify(x => x.FindDeviceIdsByDevIds(DeviceIdentities), Times.Once);
        _devInfoServiceMock.Verify(x => x.GetByDeviceIds(It.IsAny<int[]>()), Times.Once);
        _accessControlManagerMock.Verify(x => x.EnsureHasAnyAccessRights(
                It.IsAny<HashSet<SecurityPermission>>(),
                SecurityAssetType.Device,
                It.IsAny<HashSet<int>>()),
            Times.Once);
    }

    [Test]
    public void GetDevicesLocalGroups_Success()
    {
        var localGroups = new List<string> { "Users", "Administrators" };

        _devInfoServiceMock.Setup(x => x.FindDeviceIdsByDevIds(It.IsAny<IEnumerable<string>>()))
            .Returns(DeviceIdsDictionary).Verifiable();
        _devInfoServiceMock.Setup(x => x.GetByDeviceIds(It.IsAny<int[]>()))
            .Returns(DeviceModelsTestCollection()).Verifiable();
        _accessControlManagerMock.Setup(x => x.EnsureHasAnyAccessRights(
            It.IsAny<HashSet<SecurityPermission>>(),
            SecurityAssetType.Device,
            It.IsAny<HashSet<int>>())).Verifiable();
        _windowsDeviceLocalGroupsServiceMock.Setup(x => x.GetDevicesLocalGroups(It.IsAny<IEnumerable<int>>()))
            .Returns(localGroups).Verifiable();

        var result = _controller.GetDevicesLocalGroups(DeviceIdentities);

        _devInfoServiceMock.Verify(x => x.FindDeviceIdsByDevIds(DeviceIdentities), Times.Once);
        _devInfoServiceMock.Verify(x => x.GetByDeviceIds(It.IsAny<int[]>()), Times.Once);
        _accessControlManagerMock.Verify(x => x.EnsureHasAnyAccessRights(
                It.IsAny<HashSet<SecurityPermission>>(),
                SecurityAssetType.Device,
                It.IsAny<HashSet<int>>()),
            Times.Once);
        _windowsDeviceLocalGroupsServiceMock.Verify(x => x.GetDevicesLocalGroups(It.IsAny<IEnumerable<int>>()));

        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(localGroups, result);
    }

    #endregion

    #region Private Methods

    private WindowsDeviceModel GetWindowsDeviceModel(
        int deviceId,
        bool isLocked = false,
        string passcode = null)
    {
        return new WindowsDeviceModel
        {
            DeviceId = deviceId,
            IsLocked = isLocked,
            Passcode = passcode
        };
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

    #endregion
}
