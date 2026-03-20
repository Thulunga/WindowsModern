using System;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests.Converters;

[TestFixture]
internal sealed class GroupPolicySettingsConverterTests
{
    [Test]
    public void ToWindowsModernGroupPolicySettingsContract_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => GroupPolicySettingsConverter.ToWindowsModernGroupPolicySettingsContract(null));
    }
}
