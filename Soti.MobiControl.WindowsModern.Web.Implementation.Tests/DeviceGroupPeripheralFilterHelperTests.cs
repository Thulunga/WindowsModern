using System;
using NUnit.Framework;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests;

[TestFixture]
public class DeviceGroupPeripheralFilterHelperTests
{
    [Test]
    public void Filter_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => DeviceGroupPeripheralFilterHelper.Filter(null, "", null));
    }
}
