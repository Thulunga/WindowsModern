using System;
using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class InvalidateLoggedInUserCacheMessageConsumerTests
{
    private const int DeviceId = 1000001;

    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IWindowsDeviceLoggedInUserService> _windowsDeviceLoggedInUserServiceMock;
    private InvalidateLoggedInUserCacheMessageConsumer _invalidateLoggedInUserCacheMessageConsumer;

    [SetUp]
    public void Setup()
    {
        _programTraceMock = new Mock<IProgramTrace>();
        _windowsDeviceLoggedInUserServiceMock = new Mock<IWindowsDeviceLoggedInUserService>();
        _invalidateLoggedInUserCacheMessageConsumer = new InvalidateLoggedInUserCacheMessageConsumer(
            _programTraceMock.Object,
            _windowsDeviceLoggedInUserServiceMock.Object);
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenProgramTraceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new InvalidateLoggedInUserCacheMessageConsumer(null, _windowsDeviceLoggedInUserServiceMock.Object));
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenWindowsDeviceLoggedInUserServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new InvalidateLoggedInUserCacheMessageConsumer(_programTraceMock.Object, null));
    }

    [Test]
    public void Consume_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _invalidateLoggedInUserCacheMessageConsumer.Consume(null));
        _programTraceMock.Verify(x => x.Write(TraceLevel.Verbose, "Caching", It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Consume_ShouldCallInvalidateLoggedInUserCache_WithCorrectParameters()
    {
        var cacheMessage = new InvalidateLoggedInUserCacheMessage
        {
            DeviceId = DeviceId
        };

        _windowsDeviceLoggedInUserServiceMock.Setup(x => x.InvalidateLoggedInUserCache(DeviceId, It.IsAny<bool>()))
            .Verifiable();

        _invalidateLoggedInUserCacheMessageConsumer.Consume(cacheMessage);

        _windowsDeviceLoggedInUserServiceMock.Verify(x => x.InvalidateLoggedInUserCache(DeviceId, false), Times.Once);
        _programTraceMock.Verify(x => x.Write(TraceLevel.Verbose, "Caching", It.IsAny<string>()), Times.Once);
    }
}
