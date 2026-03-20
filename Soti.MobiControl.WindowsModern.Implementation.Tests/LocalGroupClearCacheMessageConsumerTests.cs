using System;
using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class LocalGroupClearCacheMessageConsumerTests
{
    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsServiceMock;
    private LocalGroupClearCacheMessageConsumer _localGroupClearCacheMessageConsumer;

    [SetUp]
    public void Setup()
    {
        _programTraceMock = new Mock<IProgramTrace>();
        _windowsDeviceLocalGroupsServiceMock = new Mock<IWindowsDeviceLocalGroupsService>();
        _localGroupClearCacheMessageConsumer = new LocalGroupClearCacheMessageConsumer(_programTraceMock.Object, _windowsDeviceLocalGroupsServiceMock.Object);
    }

    [Test]
    public void Constructor_InvalidArguments()
    {
        Assert.Throws<ArgumentNullException>(() => new LocalGroupClearCacheMessageConsumer(null, _windowsDeviceLocalGroupsServiceMock.Object));
        Assert.Throws<ArgumentNullException>(() => new LocalGroupClearCacheMessageConsumer(_programTraceMock.Object, null));
    }

    [Test]
    public void Consume_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _localGroupClearCacheMessageConsumer.Consume(null));
        _programTraceMock.Verify(x => x.Write(TraceLevel.Verbose, "Caching", It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Consume_Success()
    {
        _windowsDeviceLocalGroupsServiceMock.Setup(x => x.InvalidateCache(false)).Verifiable();

        _localGroupClearCacheMessageConsumer.Consume(new LocalGroupCacheClearMessage());

        _windowsDeviceLocalGroupsServiceMock.Verify(x => x.InvalidateCache(false), Times.Once);
        _programTraceMock.Verify(x => x.Write(TraceLevel.Verbose, "Caching", It.IsAny<string>()), Times.Once);
    }
}
