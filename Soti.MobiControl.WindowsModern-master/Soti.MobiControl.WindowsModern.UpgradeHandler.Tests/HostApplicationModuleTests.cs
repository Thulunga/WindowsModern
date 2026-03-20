using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Soti.HostModule;

namespace Soti.MobiControl.WindowsModern.UpgradeHandler.Tests;

[TestFixture]
public sealed class HostApplicationModuleTests
{
    [Test]
    public void ConfigureHost_NullArgument_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new HostApplicationModule().ConfigureHost(null));
    }

    [Test]
    public void ConfigureHost_DoesNotThrow()
    {
        var mock = new Mock<IHostApplicationModuleContext>();
        var mockServiceCollection = new Mock<IServiceCollection>();
        mock.SetupGet(p => p.Services).Returns(mockServiceCollection.Object);

        Assert.DoesNotThrow(() => new HostApplicationModule().ConfigureHost(mock.Object));
    }
}
