using System;
using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class InvalidateBitLockerCacheMessageConsumerTests
{
    private const int DeviceId = 1000001;

    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IDeviceBitLockerKeyService> _deviceBitLockerKeyServiceMock;
    private InvalidateBitLockerCacheMessageConsumer _invalidateBitLockerCacheMessageConsumer;

    [SetUp]
    public void Setup()
    {
        _programTraceMock = new Mock<IProgramTrace>();
        _deviceBitLockerKeyServiceMock = new Mock<IDeviceBitLockerKeyService>();
        _invalidateBitLockerCacheMessageConsumer = new InvalidateBitLockerCacheMessageConsumer(
            _programTraceMock.Object,
            _deviceBitLockerKeyServiceMock.Object);
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenProgramTraceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new InvalidateBitLockerCacheMessageConsumer(null, _deviceBitLockerKeyServiceMock.Object));
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenDeviceBitLockerKeyServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new InvalidateBitLockerCacheMessageConsumer(_programTraceMock.Object, null));
    }

    [Test]
    public void Consume_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _invalidateBitLockerCacheMessageConsumer.Consume(null));
        _programTraceMock.Verify(x => x.Write(TraceLevel.Verbose, "Caching", It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Consume_ShouldCallInvalidateBitLockerCache_WithCorrectParameters()
    {
        var cacheMessage = new InvalidateBitLockerCacheMessage
        {
            DeviceId = DeviceId
        };

        _deviceBitLockerKeyServiceMock.Setup(x => x.InvalidateBitLockerCache(DeviceId, It.IsAny<bool>()))
            .Verifiable();

        _invalidateBitLockerCacheMessageConsumer.Consume(cacheMessage);

        _deviceBitLockerKeyServiceMock.Verify(x => x.InvalidateBitLockerCache(DeviceId, false), Times.Once);
        _programTraceMock.Verify(x => x.Write(TraceLevel.Verbose, "Caching", It.IsAny<string>()), Times.Once);
    }
}
