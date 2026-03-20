using System;
using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class LocalGroupCacheMessageConsumerTests
{
    private const int DeviceId = 1000001;
    private const int GroupNameId = 1;
    private const string GroupName = "TestGroup";

    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IWindowsDeviceLocalGroupsService> _windowsDeviceLocalGroupsService;
    private LocalGroupCacheMessageConsumer _localGroupCacheMessageConsumer;

    [SetUp]
    public void Setup()
    {
        _programTraceMock = new Mock<IProgramTrace>();
        _windowsDeviceLocalGroupsService = new Mock<IWindowsDeviceLocalGroupsService>();
        _localGroupCacheMessageConsumer = new LocalGroupCacheMessageConsumer(_programTraceMock.Object, _windowsDeviceLocalGroupsService.Object);
    }

    [Test]
    public void Constructor_ShouldThrowArgumentNullException_WhenProgramTraceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new LocalGroupCacheMessageConsumer(null, _windowsDeviceLocalGroupsService.Object));
    }

    [Test]
    public void Consume_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _localGroupCacheMessageConsumer.Consume(null));
        _programTraceMock.Verify(x => x.Write(TraceLevel.Verbose, "Caching", It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Consume_ShouldCallInternalUpdateDeviceLocalGroupFromCache_WithCorrectParameters()
    {
        var cacheMessage = new LocalGroupCacheMessage
        {
            GroupNameId = GroupNameId,
            GroupName = GroupName
        };

        _localGroupCacheMessageConsumer.Consume(cacheMessage);

        _programTraceMock.Verify(x => x.Write(TraceLevel.Verbose, "Caching", It.IsAny<string>()), Times.Exactly(1));
    }
}
