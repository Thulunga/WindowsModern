using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class WindowsDeviceBiosServiceTests
{
    private WindowsDeviceBiosService _service;
    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IWindowsDeviceBootPriorityService> _bootPriorityServiceMock;
    private Mock<IWindowsDeviceDataProvider> _windowsDeviceDataProviderMock;

    [SetUp]
    public void SetUp()
    {
        _bootPriorityServiceMock = new Mock<IWindowsDeviceBootPriorityService>();
        _windowsDeviceDataProviderMock = new Mock<IWindowsDeviceDataProvider>();
        _programTraceMock = new Mock<IProgramTrace>();

        _service = new WindowsDeviceBiosService(
            _programTraceMock.Object,
            _bootPriorityServiceMock.Object,
            _windowsDeviceDataProviderMock.Object);
    }

    [Test]
    public void GetBiosBootPrioritySummary_NoBootData_ReturnsEmpty()
    {
        var deviceIds = new[] { 1 };

        _bootPriorityServiceMock.Setup(x => x.BulkGetByDeviceId(It.IsAny<IEnumerable<int>>()))
            .Returns((List<WindowsDeviceBootPriority>)null!);

        var result = _service.GetBiosBootPrioritySummary(deviceIds);

        Assert.IsEmpty(result);

        _bootPriorityServiceMock.Verify(x => x.BulkGetByDeviceId(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(deviceIds))), Times.Once);
    }

    [Test]
    public void GetBiosBootPrioritySummary_ReturnsFormattedSummary()
    {
        var deviceIds = new[] { 1 };
        var bootData = new List<WindowsDeviceBootPriority>
        {
            new() { DeviceId = 1, BootPriorityId = 1, BootOrder = 1 },
            new() { DeviceId = 1, BootPriorityId = 2, BootOrder = 2 }
        };
        var priorities = new List<WindowsBootPriority>
        {
            new() { BootPriorityId = 1, BootPriorityName = "USB" },
            new() { BootPriorityId = 2, BootPriorityName = "HDD" }
        };

        _bootPriorityServiceMock.Setup(x => x.BulkGetByDeviceId(deviceIds)).Returns(bootData);
        _bootPriorityServiceMock.Setup(x => x.BulkGetByIds(It.IsAny<IEnumerable<short>>())).Returns(priorities);

        var result = _service.GetBiosBootPrioritySummary(deviceIds);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result.Single().DeviceId);
        Assert.AreEqual("USB, HDD", result.Single().BootPriority);

        _bootPriorityServiceMock.Verify(x => x.BulkGetByDeviceId(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(deviceIds))), Times.Once);
        _bootPriorityServiceMock.Verify(x => x.BulkGetByIds(It.Is<IEnumerable<short>>(ids => ids.SequenceEqual(new short[] { 1, 2 }))), Times.Once);
    }

    [Test]
    public void SynchronizeBiosBootOrder_ValidInput_CallsBulkModifyForDevice()
    {
        var bootOrder = "USB,HDD";
        var deviceId = 100;
        var priorities = new List<WindowsBootPriority>
        {
            new() { BootPriorityId = 1, BootPriorityName = "USB" },
            new() { BootPriorityId = 2, BootPriorityName = "HDD" }
        };

        _bootPriorityServiceMock.Setup(x => x.BulkInsertAndGet(It.IsAny<IEnumerable<string>>()))
            .Returns(priorities);

        _service.SynchronizeBiosBootOrder(deviceId, bootOrder);

        _bootPriorityServiceMock.Verify(x =>
            x.BulkInsertAndGet(It.Is<IEnumerable<string>>(names =>
                names.SequenceEqual(new[] { "USB", "HDD" }))), Times.Once);
    }

    [Test]
    public void SynchronizeBiosPayloadStatus_ValidEnum_UpdatesPasswordStatus()
    {
        var deviceId = 1;
        var payloadStatus = "Enforced";

        _windowsDeviceDataProviderMock
            .Setup(x => x.UpdateBiosPasswordStatusId(deviceId, (byte)BiosPasswordStatusType.Enforced))
            .Verifiable();

        _service.SynchronizeBiosPayloadStatus(deviceId, payloadStatus);

        _windowsDeviceDataProviderMock.Verify(
            x => x.UpdateBiosPasswordStatusId(deviceId, (byte)BiosPasswordStatusType.Enforced),
            Times.Once,
            "UpdateBiosPasswordStatusId was not called with the expected parameters."
        );
    }

    [Test]
    public void SynchronizeBiosPayloadStatus_InvalidEnum_LogsError()
    {
        var deviceId = 1;
        var invalidPayloadStatus = "InvalidStatus";

        _service.SynchronizeBiosPayloadStatus(deviceId, invalidPayloadStatus);

        _programTraceMock.Verify(
            x => x.Write(TraceLevel.Error, It.IsAny<string>(), It.Is<string>(msg => msg.Contains($"Unknown PayloadStatus: {invalidPayloadStatus}"))),
            Times.Once);
    }

    [TestCase("GetBiosBootPrioritySummary", null, null, typeof(ArgumentNullException))]
    [TestCase("GetBiosBootPrioritySummary", new int[0], null, typeof(ArgumentOutOfRangeException))]

    [TestCase("SynchronizeBiosBootOrder", 0, "USB", typeof(ArgumentOutOfRangeException))]
    [TestCase("SynchronizeBiosBootOrder", 1, null, typeof(ArgumentException))]
    [TestCase("SynchronizeBiosBootOrder", 1, "", typeof(ArgumentException))]
    [TestCase("SynchronizeBiosBootOrder", 1, "   ", typeof(ArgumentException))]

    [TestCase("SynchronizeBiosPayloadStatus", 0, "Enforced", typeof(ArgumentOutOfRangeException))]
    [TestCase("SynchronizeBiosPayloadStatus", -1, "None", typeof(ArgumentOutOfRangeException))]
    [TestCase("SynchronizeBiosPayloadStatus", 1, null, typeof(ArgumentException))]
    [TestCase("SynchronizeBiosPayloadStatus", 1, "", typeof(ArgumentException))]
    [TestCase("SynchronizeBiosPayloadStatus", 1, "  ", typeof(ArgumentException))]
    public void BiosService_InvalidInputs_ThrowsExpectedException(string method, object arg1, object arg2, Type expectedException)
    {
        var ex = method switch
        {
            "GetBiosBootPrioritySummary" =>
                Assert.Throws(Is.InstanceOf<Exception>(), () => _service.GetBiosBootPrioritySummary((int[])arg1)),

            "SynchronizeBiosBootOrder" =>
                Assert.Throws(Is.InstanceOf<Exception>(), () => _service.SynchronizeBiosBootOrder((int)arg1, (string)arg2)),

            "SynchronizeBiosPayloadStatus" =>
                Assert.Throws(Is.InstanceOf<Exception>(), () => _service.SynchronizeBiosPayloadStatus((int)arg1, (string)arg2)),

            _ => throw new InvalidOperationException("Unknown method case")
        };

        Assert.That(ex, Is.TypeOf(expectedException));
    }
}
