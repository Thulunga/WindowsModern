using System;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;
using NUnit.Framework;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests.Converters;

[TestFixture]
internal sealed class DevicePeripheralConverterTests
{
    [Test]
    public void ToDeviceLocalUserSummary_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() => DevicePeripheralConverter.ToDevicePeripheralSummary(null));
    }
}
