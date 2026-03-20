using System;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests.Converters
{
    internal class DeviceLocalUserConverterTests
    {
        [Test]
        public void ToDeviceLocalUserSummary_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => DeviceLocalUserConverter.ToDeviceLocalUserSummary(null));
        }
    }
}
