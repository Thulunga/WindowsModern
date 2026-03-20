using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Moq;
using NUnit.Framework;
using Soti.MobiControl.DeviceGroupManagement;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Management.Services;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests;

[TestFixture]
internal sealed class WindowsDeviceGroupsUiControllerTests
{
    private const string GroupPath = "groupPath";
    private const int GroupId = 1;

    private Mock<IAccessControlManager> _accessControlManagerMock;
    private Mock<IDeviceGroupIdentityMapper> _deviceGroupIdentityMapperMock;
    private Mock<IAccessibleDeviceGroupService> _accessibleDeviceGroupServiceMock;
    private Mock<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsServiceMock;
    private IWindowsDeviceGroupsUiController _controller;

    [SetUp]
    public void Setup()
    {
        _accessControlManagerMock = new Mock<IAccessControlManager>(MockBehavior.Strict);
        _deviceGroupIdentityMapperMock = new Mock<IDeviceGroupIdentityMapper>();
        _accessibleDeviceGroupServiceMock = new Mock<IAccessibleDeviceGroupService>();
        _windowsDeviceLocalGroupsServiceMock = new Mock<IWindowsDeviceLocalGroupsService>(MockBehavior.Strict);
        _controller = new WindowsDeviceGroupsUiController(
            new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
            _accessControlManagerMock.Object,
            new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object)
        );
    }

    #region Constructor

    [Test]
    public void Constructor_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceGroupsUiController(
            null,
            _accessControlManagerMock.Object,
            new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceGroupsUiController(
            new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
            null,
            new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceGroupsUiController(
            new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
            _accessControlManagerMock.Object,
            null,
            new Lazy<IWindowsDeviceLocalGroupsService>(() => _windowsDeviceLocalGroupsServiceMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _controller = new WindowsDeviceGroupsUiController(
            new Lazy<IDeviceGroupIdentityMapper>(() => _deviceGroupIdentityMapperMock.Object),
            _accessControlManagerMock.Object,
            new Lazy<IAccessibleDeviceGroupService>(() => _accessibleDeviceGroupServiceMock.Object),
            null));
    }

    #endregion

    #region GetDeviceGroupLocalGroups

    [Test]
    public void GetDeviceGroupLocalGroups_Throws_ValidationException()
    {
        Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupLocalGroups(null));
        Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupLocalGroups(string.Empty));
        Assert.Throws<ValidationException>(() => _controller.GetDeviceGroupLocalGroups(" "));
    }

    [Test]
    public void GetDeviceGroupLocalGroups_Success()
    {
        var localGroups = new List<string> { "Users", "Administrators" };
        ISet<int> groupIds = new HashSet<int> { GroupId, GroupId + 1 };
        EnsureDeviceGroupRights();

        _accessibleDeviceGroupServiceMock.Setup(x => x.GetPermittedDeviceGroupIds(GroupPath, true))
            .Returns(groupIds).Verifiable();
        _windowsDeviceLocalGroupsServiceMock.Setup(x => x.GetDeviceGroupsLocalGroups(groupIds))
            .Returns(localGroups).Verifiable();

        var result = _controller.GetDeviceGroupLocalGroups(GroupPath);

        _accessibleDeviceGroupServiceMock.Verify(x => x.GetPermittedDeviceGroupIds(GroupPath, true), Times.Once);
        _windowsDeviceLocalGroupsServiceMock.Verify(x => x.GetDeviceGroupsLocalGroups(groupIds), Times.Once);

        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(localGroups, result);
    }

    #endregion

    #region Private Methods

    private void EnsureDeviceGroupRights()
    {
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.DeviceGroup, GroupId));
        _deviceGroupIdentityMapperMock.Setup(m => m.GetId(GroupPath)).Returns(GroupId);
    }

    #endregion

}
