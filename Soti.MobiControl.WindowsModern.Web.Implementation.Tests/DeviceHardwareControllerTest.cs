using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Moq;
using NUnit.Framework;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests;

[TestFixture]
internal sealed class DeviceHardwareControllerTest
{
    private const int DeviceId = 10000001;
    private const string HardwareManufacturerName = "Microsoft";
    private const string HardwareName = "Physical Memory";
    private const int DeviceHardwareType = 1;
    private const int HardwareStatus = 2;
    private const string HardwareSerialNumber = "M0001";
    private readonly string _devId = Guid.NewGuid().ToString();

    private Mock<IDeviceKeyInformationRetrievalService> _deviceKeyInformationRetrievalServiceMock;
    private Mock<IAccessControlManager> _accessControlManagerMock;
    private Mock<IDeviceHardwareService> _deviceHardwareServiceMock;
    private IDeviceHardwareController _windowsDeviceHardwareController;

    [SetUp]
    public void SetUp()
    {
        _accessControlManagerMock = new Mock<IAccessControlManager>(MockBehavior.Strict);
        _deviceKeyInformationRetrievalServiceMock =
            new Mock<IDeviceKeyInformationRetrievalService>(MockBehavior.Strict);
        _deviceHardwareServiceMock = new Mock<IDeviceHardwareService>(MockBehavior.Strict);
        _windowsDeviceHardwareController = new DeviceHardwareController(
            _deviceKeyInformationRetrievalServiceMock.Object,
            _accessControlManagerMock.Object,
            _deviceHardwareServiceMock.Object);
    }

    [Test]
    public void Constructor_Test()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new DeviceHardwareController(null, _accessControlManagerMock.Object, _deviceHardwareServiceMock.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new DeviceHardwareController(_deviceKeyInformationRetrievalServiceMock.Object, null, _deviceHardwareServiceMock.Object));
        Assert.Throws<ArgumentNullException>(() => _ = new DeviceHardwareController(_deviceKeyInformationRetrievalServiceMock.Object, _accessControlManagerMock.Object, null));
    }

    [Test]
    public void GetDeviceHardware_Validation_Test()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns((DeviceKeyInformation)null);
        Assert.Throws<SecurityException>(() => _windowsDeviceHardwareController.GetDeviceHardware(_devId));
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(DeviceKeyInformationAndroidData);
        Assert.Throws<SecurityException>(() => _windowsDeviceHardwareController.GetDeviceHardware(_devId));
        Assert.Throws<ValidationException>(() => _windowsDeviceHardwareController.GetDeviceHardware(null));
    }

    [Test]
    public void GetDeviceHardware_CheckPermissions_Test()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(DeviceKeyInformationWindowsData);
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId));
        _deviceHardwareServiceMock.Setup(d => d.GetAllDeviceHardwareStatusSummaryByDeviceId(It.IsAny<int>())).Returns(new List<DeviceHardwareSummary>());
        var deviceHardwares = _windowsDeviceHardwareController.GetDeviceHardware(_devId);
        Assert.AreEqual(0, deviceHardwares.Count());
        _deviceHardwareServiceMock.Verify(d => d.GetAllDeviceHardwareStatusSummaryByDeviceId(It.IsAny<int>()), Times.Once);
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId), Times.Once);
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId), Times.Once);
    }

    [Test]
    public void GetDeviceHardware_Test()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(DeviceKeyInformationWindowsData);
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId));
        _deviceHardwareServiceMock.Setup(d => d.GetAllDeviceHardwareStatusSummaryByDeviceId(It.IsAny<int>())).Returns(DeviceHardwareModelData);
        var deviceHardwares = _windowsDeviceHardwareController.GetDeviceHardware(_devId);
        Assert.AreEqual(1, deviceHardwares.Count());
        _deviceHardwareServiceMock.Verify(d => d.GetAllDeviceHardwareStatusSummaryByDeviceId(It.IsAny<int>()), Times.Once);
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId), Times.Once);
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId), Times.Once);
    }

    [Test]
    public void GetDeviceHardware_PermissionDenied_Test()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(DeviceKeyInformationWindowsData);
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(DevicePermission.AppFeedbackUpdate, SecurityAssetType.Device, DeviceId));
        _deviceHardwareServiceMock.Setup(d => d.GetAllDeviceHardwareStatusSummaryByDeviceId(It.IsAny<int>())).Returns(DeviceHardwareModelData);
        Assert.Throws<MockException>(() => _windowsDeviceHardwareController.GetDeviceHardware(_devId));
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.AppFeedbackUpdate, SecurityAssetType.Device, DeviceId));
        _deviceHardwareServiceMock.Setup(d => d.GetAllDeviceHardwareStatusSummaryByDeviceId(It.IsAny<int>())).Returns(DeviceHardwareModelData);
        Assert.Throws<MockException>(() => _windowsDeviceHardwareController.GetDeviceHardware(_devId));
        _deviceHardwareServiceMock.Verify(d => d.GetAllDeviceHardwareStatusSummaryByDeviceId(It.IsAny<int>()), Times.Never);
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId), Times.Exactly(2));
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId), Times.Once);
    }

    [Test]
    public void UpdateDeviceHardware_Validation_Test()
    {
        Assert.Throws<ValidationException>(() => _windowsDeviceHardwareController.UpdateDeviceHardwareStatus(null, new Contracts.DeviceHardwareStatusSumary()));
        Assert.Throws<ValidationException>(() => _windowsDeviceHardwareController.UpdateDeviceHardwareStatus(_devId, null));
        Assert.Throws<ValidationException>(() => _windowsDeviceHardwareController.UpdateDeviceHardwareStatus(
            _devId,
            new Contracts.DeviceHardwareStatusSumary()
            {
                HardwareStatus = (Enums.HardwareStatus)7,
                HardwareSerialNumber = HardwareSerialNumber
            }));
        Assert.Throws<ValidationException>(() => _windowsDeviceHardwareController.UpdateDeviceHardwareStatus(
            _devId,
            new Contracts.DeviceHardwareStatusSumary()
            {
                HardwareStatus = (Enums.HardwareStatus)3,
                HardwareSerialNumber = null
            }));
    }

    [Test]
    public void UpdateDeviceHardwareStatus_CheckDeviceManagementPermission_Test()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(DeviceKeyInformationWindowsData);
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId));
        _deviceHardwareServiceMock.Setup(d => d.UpdateDeviceHardwareStatus(
            DeviceId,
            _devId,
            It.IsAny<HardwareStatus>(),
            It.IsAny<string>()));
        _windowsDeviceHardwareController.UpdateDeviceHardwareStatus(
            _devId,
            new Contracts.DeviceHardwareStatusSumary()
            {
                HardwareStatus = (Enums.HardwareStatus)3,
                HardwareSerialNumber = HardwareSerialNumber
            });

        _deviceHardwareServiceMock.Verify(d => d.UpdateDeviceHardwareStatus(
            DeviceId,
            _devId,
            It.IsAny<HardwareStatus>(),
            It.IsAny<string>()));
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId), Times.Once);
    }

    [Test]
    public void UpdateDeviceHardware_PermissionDenied_Test()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(DeviceKeyInformationWindowsData);
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(DevicePermission.AppFeedbackUpdate, SecurityAssetType.Device, DeviceId));
        Assert.Throws<MockException>(() => _windowsDeviceHardwareController.UpdateDeviceHardwareStatus(
            _devId,
            new Contracts.DeviceHardwareStatusSumary()
            {
                HardwareStatus = (Enums.HardwareStatus)3,
                HardwareSerialNumber = HardwareSerialNumber
            }));
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId), Times.Once);
    }

    private DeviceKeyInformation DeviceKeyInformationAndroidData()
    {
        return new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.Android);
    }

    private DeviceKeyInformation DeviceKeyInformationWindowsData()
    {
        return new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10);
    }

    private static List<DeviceHardwareSummary> DeviceHardwareModelData()
    {
        return
        [
            new DeviceHardwareSummary
            {
                HardwareManufacturerName = HardwareManufacturerName,
                HardwareName = HardwareName,
                HardwareSerialNumber = HardwareSerialNumber,
                HardwareStatus = (HardwareStatus)HardwareStatus,
                DeviceHardwareType = (DeviceHardwareType)DeviceHardwareType
            }
        ];
    }
}
