using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class DeviceGroupPeripheralServiceTests
{
    private Mock<IDeviceGroupPeripheralDataProvider> _mockDeviceGroupPeripheralDataProvider;
    private IDeviceGroupPeripheralService _service;

    [SetUp]
    public void SetUp()
    {
        _mockDeviceGroupPeripheralDataProvider = new Mock<IDeviceGroupPeripheralDataProvider>(MockBehavior.Strict);
        _service = new DeviceGroupPeripheralService(_mockDeviceGroupPeripheralDataProvider.Object);
    }

    [Test]
    public void ConstructorTests()
    {
        var argumentCount = 1;
        for (var i = 0; i < argumentCount; i++)
        {
            var j = 0;
            Assert.Throws<ArgumentNullException>(() => _ = new DeviceGroupPeripheralService(
                i == j++ ? null : _mockDeviceGroupPeripheralDataProvider.Object));

            argumentCount = j;
        }
    }

    [Test]
    public void GetDeviceGroupsPeripheralSummary_Exception()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _service.GetDeviceGroupsPeripheralSummary(null));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.GetDeviceGroupsPeripheralSummary(new[] { 0, -1 }));
    }

    [Test]
    public void GetDeviceGroupsPeripheralSummary_Success()
    {
        var peripheralSummary = new List<DeviceGroupPeripheralSummary>
        {
            new DeviceGroupPeripheralSummary
            {
                DeviceId = 10001,
                Name = "peripheral 1",
                Manufacturer = "Lenovo",
                PeripheralId = 1,
                Status = DevicePeripheralStatus.Connected,
                PeripheralType = 1
            }
        };
        _mockDeviceGroupPeripheralDataProvider.Setup(m =>
            m.GetDeviceGroupPeripherals(It.IsAny<IEnumerable<int>>())).Returns(peripheralSummary);

        var result = _service.GetDeviceGroupsPeripheralSummary(new[] { 1 });

        Assert.NotNull(result);
        Assert.AreEqual(result.Count, 1);
        _mockDeviceGroupPeripheralDataProvider.Verify(v =>
            v.GetDeviceGroupPeripherals(It.IsAny<IEnumerable<int>>()), Times.Once);
    }

    [Test]
    public void GetPeripheralSummaryByFamilyIdAndGroupIds_Exception()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _service.GetPeripheralSummaryByFamilyIdAndGroupIds(8, null));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.GetPeripheralSummaryByFamilyIdAndGroupIds(8, new[] { 0, -1 }));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.GetPeripheralSummaryByFamilyIdAndGroupIds(0, new[] { 1 }));
    }

    [Test]
    public void GetPeripheralSummaryByFamilyIdAndGroupIds_Success()
    {
        var peripheralSummary = new List<DeviceGroupPeripheralSummary>
        {
            new DeviceGroupPeripheralSummary
            {
                DeviceId = 10001,
                Name = "peripheral 1",
                Manufacturer = "Lenovo",
                PeripheralId = 1,
                Status = DevicePeripheralStatus.Connected,
                PeripheralType = 1
            }
        };
        _mockDeviceGroupPeripheralDataProvider.Setup(m =>
            m.GetPeripheralSummaryByFamilyIdAndGroupIds(It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).Returns(peripheralSummary);

        var result = _service.GetPeripheralSummaryByFamilyIdAndGroupIds(8, new[] { 1 });

        Assert.NotNull(result);
        Assert.AreEqual(result.Count, 1);
        _mockDeviceGroupPeripheralDataProvider.Verify(v =>
            v.GetPeripheralSummaryByFamilyIdAndGroupIds(It.IsAny<int>(), It.IsAny<IEnumerable<int>>()), Times.Once);
    }
}
