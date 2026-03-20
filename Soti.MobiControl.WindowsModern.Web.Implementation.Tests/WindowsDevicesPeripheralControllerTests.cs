using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.MobiControl.Security.Authorization.Permissions;
using Soti.MobiControl.Security.Authorization;
using Soti.MobiControl.Devices;
using NUnit.Framework;
using Moq;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests;

[TestFixture]
internal sealed class WindowsDevicePeripheralControllerTests
{
    private const int DeviceId = 1000;
    private readonly string _devId = Guid.NewGuid().ToString();

    private Mock<IWindowsDevicePeripheralService> _windowsDevicePeripheralServiceMock;
    private Mock<IAccessControlManager> _accessControlManagerMock;
    private Mock<IDeviceKeyInformationRetrievalService> _deviceKeyInformationRetrievalServiceMock;
    private IWindowsDevicesPeripheralController _controller;

    [SetUp]
    public void Setup()
    {
        _deviceKeyInformationRetrievalServiceMock =
            new Mock<IDeviceKeyInformationRetrievalService>(MockBehavior.Strict);
        _accessControlManagerMock = new Mock<IAccessControlManager>(MockBehavior.Strict);
        _windowsDevicePeripheralServiceMock = new Mock<IWindowsDevicePeripheralService>();

        _controller = new WindowsDevicesPeripheralController(
            _deviceKeyInformationRetrievalServiceMock.Object,
            _accessControlManagerMock.Object,
            _windowsDevicePeripheralServiceMock.Object
        );
    }

    [Test]
    public void Constructor_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDevicesPeripheralController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                _accessControlManagerMock.Object,
                null
            )
        );
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDevicesPeripheralController(
                _deviceKeyInformationRetrievalServiceMock.Object,
                null,
                _windowsDevicePeripheralServiceMock.Object
            )
        );
        Assert.Throws<ArgumentNullException>(() =>
            _ = new WindowsDevicesPeripheralController(
                null,
                _accessControlManagerMock.Object,
                _windowsDevicePeripheralServiceMock.Object
            )
        );
    }

    [TestCaseSource(nameof(GetInvalidDeviceId))]
    public void GetDevicePeripherals_Throws_Validation_Exception(string deviceId)
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        Assert.Throws<ValidationException>(() => _controller.GetDevicePeripherals(deviceId));
    }

    [Test]
    public void GetDevicePeripherals_Throws_Vaidation_Exception()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.Android));
        Assert.Throws<ValidationException>(() => _controller.GetDevicePeripherals(_devId));
    }

    [Test]
    public void GetDevicePeripherals_Success()
    {
        var peripheralModel = new DevicePeripheralSummary()
        {
            Name = "Peripheral1",
            Manufacturer = "Manufacturer",
            Version = "10.2022.1.2",
            Status = DevicePeripheralStatus.Connected
        };

        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10RS1));
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId));
        _windowsDevicePeripheralServiceMock.Setup(x => x.GetDevicePeripheralsSummaryInfo(It.IsAny<int>())).Returns(new List<Models.DevicePeripheralSummary> { peripheralModel });

        var result = _controller.GetDevicePeripherals(_devId).ToList();
        Assert.AreNotEqual(null, result);
        Assert.AreEqual(result.FirstOrDefault()?.PeripheralName, peripheralModel.Name);
        Assert.AreEqual(result.FirstOrDefault()?.PeripheralManufacturer, peripheralModel.Manufacturer);
        Assert.AreEqual(result.FirstOrDefault()?.PeripheralVersion, peripheralModel.Version);

        _windowsDevicePeripheralServiceMock.Verify(q => q.GetDevicePeripheralsSummaryInfo(DeviceId), Times.Once);
    }

    [Test]
    public void GetDevicePeripheral_CheckPermissions_Test()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(DeviceKeyInformationWindowsData);
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId));
        var devicePeripherals = _controller.GetDevicePeripherals(_devId);
        Assert.AreEqual(0, devicePeripherals.Count());
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId), Times.Once);
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId), Times.Once);
    }

    [Test]
    public void GetDevicePeripheral_PermissionDenied_Test()
    {
        _deviceKeyInformationRetrievalServiceMock.Setup(a => a.GetDeviceKeyInformation(_devId)).Returns(DeviceKeyInformationWindowsData);
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(DevicePermission.AppFeedbackUpdate, SecurityAssetType.Device, DeviceId));
        Assert.Throws<MockException>(() => _controller.GetDevicePeripherals(_devId));
        _accessControlManagerMock.Setup(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId));
        _accessControlManagerMock.Setup(x => x.EnsureHasAccessRight(DevicePermission.AppFeedbackUpdate, SecurityAssetType.Device, DeviceId));
        Assert.Throws<MockException>(() => _controller.GetDevicePeripherals(_devId));
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(It.IsAny<DeviceGroupPermission>(), SecurityAssetType.Device, DeviceId), Times.Exactly(2));
        _accessControlManagerMock.Verify(a => a.EnsureHasAccessRight(DevicePermission.ManageAndMoveDevices, SecurityAssetType.Device, DeviceId), Times.Once);
    }

    private static IEnumerable<TestCaseData> GetInvalidDeviceId()
    {
        yield return new TestCaseData(null);
        yield return new TestCaseData(string.Empty);
        yield return new TestCaseData(" ");
    }

    private DeviceKeyInformation DeviceKeyInformationWindowsData()
    {
        return new DeviceKeyInformation(DeviceId, _devId, DevicePlatform.WindowsDesktop10);
    }
}
