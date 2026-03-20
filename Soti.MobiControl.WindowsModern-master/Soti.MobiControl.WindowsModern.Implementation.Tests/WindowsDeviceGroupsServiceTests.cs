using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Soti.MobiControl.DeviceGroupManagement;
using Soti.MobiControl.DeviceGroupManagement.Model;
using Soti.MobiControl.WindowsModern.Implementation.Exceptions;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class WindowsDeviceGroupsServiceTests
{
    private const int DeviceGroupId = 1;
    private const int AntivirusSyncDataNotFoundErrorCode = 11750;
    private Mock<IDeviceGroupSyncRequestDataProvider> _deviceGroupSyncRequestDataProviderMock;
    private Mock<IDeviceGroupManager> _deviceGroupManagerMock;
    private IWindowsDeviceGroupsService _windowsDeviceGroupsService;

    [SetUp]
    public void Setup()
    {
        _deviceGroupSyncRequestDataProviderMock = new Mock<IDeviceGroupSyncRequestDataProvider>();
        _deviceGroupManagerMock = new Mock<IDeviceGroupManager>();

        _windowsDeviceGroupsService = new WindowsDeviceGroupsService(
            _deviceGroupSyncRequestDataProviderMock.Object,
            _deviceGroupManagerMock.Object
        );
    }

    [Test]
    public void ConstructorTests()
    {
        Assert.Throws<ArgumentNullException>(() => new WindowsDeviceGroupsService(null, _deviceGroupManagerMock.Object));
        Assert.Throws<ArgumentNullException>(() => new WindowsDeviceGroupsService(_deviceGroupSyncRequestDataProviderMock.Object, null));
    }

    [TestCase(0, SyncRequestType.Antivirus)]
    [TestCase(-1, SyncRequestType.Antivirus)]
    [TestCase(1, 99)]
    public void GetGroupSyncStatus_Throws_ArgumentOutOfRangeException(int deviceGroupId, byte syncRequestType)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceGroupsService.GetGroupSyncStatus(deviceGroupId, (SyncRequestType)syncRequestType));
    }

    [Test]
    public void GetGroupSyncStatus_Throws_AntivirusSyncDataNotFoundException()
    {
        _deviceGroupSyncRequestDataProviderMock.Setup(provider => provider.GetDeviceGroupsLastSyncTime(It.IsAny<IReadOnlyList<int>>(), It.IsAny<IReadOnlyList<byte>>(), It.IsAny<byte>()))
            .Returns(() => null);
        var exception = Assert.Throws<WindowsModernException>(() => _windowsDeviceGroupsService.GetGroupSyncStatus(DeviceGroupId, SyncRequestType.Antivirus));

        Assert.IsNotNull(exception);
        Assert.IsNotNull(exception.Error);
        Assert.AreEqual(AntivirusSyncDataNotFoundErrorCode, exception.Error.ErrorCode);
    }

    [Test]
    public void GetGroupSyncStatus_SyncInProgress_Success()
    {
        var deviceGroup = new DeviceGroup(DeviceGroupId, "name", "path", null, new DeviceGroupIcon(), null, "refId");
        var data = new DeviceGroupSyncRequestStatusData
        {
            DeviceGroupId = DeviceGroupId,
            CompletedOn = null
        };
        _deviceGroupSyncRequestDataProviderMock.Setup(provider => provider.GetDeviceGroupsLastSyncTime(It.IsAny<IReadOnlyList<int>>(), It.IsAny<IReadOnlyList<byte>>(), It.IsAny<byte>())).Returns(data);
        _deviceGroupManagerMock.Setup(m => m.GetAllAscendantGroups(It.IsAny<int>())).Returns(new DeviceGroupBase[] { deviceGroup });

        var syncInfo = _windowsDeviceGroupsService.GetGroupSyncStatus(DeviceGroupId, SyncRequestType.Antivirus);

        Assert.IsNotNull(syncInfo);
        Assert.AreEqual(SyncRequestStatus.Running, syncInfo.SyncStatus);
        Assert.That(syncInfo.CompletedOn == null);

        _deviceGroupSyncRequestDataProviderMock.Verify(provider => provider.GetDeviceGroupsLastSyncTime(It.IsAny<IReadOnlyList<int>>(), It.IsAny<IReadOnlyList<byte>>(), It.IsAny<byte>()), Times.Once);
        _deviceGroupManagerMock.Verify(m => m.GetAllAscendantGroups(It.IsAny<int>()), Times.Once);
    }

    [Test]
    public void GetGroupSyncStatus_SyncCompleted_Success()
    {
        var completedOn = DateTime.UtcNow;
        var deviceGroup = new DeviceGroup(DeviceGroupId, "name", "path", null, new DeviceGroupIcon(), null, "refId");
        var data = new DeviceGroupSyncRequestStatusData
        {
            DeviceGroupId = DeviceGroupId,
            CompletedOn = completedOn,
        };
        _deviceGroupSyncRequestDataProviderMock.Setup(provider => provider.GetDeviceGroupsLastSyncTime(It.IsAny<IReadOnlyList<int>>(), It.IsAny<IReadOnlyList<byte>>(), It.IsAny<byte>())).Returns(data);
        _deviceGroupManagerMock.Setup(m => m.GetAllAscendantGroups(It.IsAny<int>())).Returns(new DeviceGroupBase[] { deviceGroup });

        var syncInfo = _windowsDeviceGroupsService.GetGroupSyncStatus(DeviceGroupId, SyncRequestType.Antivirus);

        Assert.IsNotNull(syncInfo);
        Assert.AreEqual(SyncRequestStatus.Completed, syncInfo.SyncStatus);
        Assert.That(syncInfo.CompletedOn == completedOn);

        _deviceGroupSyncRequestDataProviderMock.Verify(provider => provider.GetDeviceGroupsLastSyncTime(It.IsAny<IReadOnlyList<int>>(), It.IsAny<IReadOnlyList<byte>>(), It.IsAny<byte>()), Times.Once);
        _deviceGroupManagerMock.Verify(m => m.GetAllAscendantGroups(It.IsAny<int>()), Times.Once);
    }
}
