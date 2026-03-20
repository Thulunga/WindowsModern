using System;
using NUnit.Framework;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
internal sealed class WindowsDeviceHelperMethodsTests
{
    public enum TestEnum : byte
    {
        TestValue1 = 1,
        TestValue2 = 2
    }

    [Test]
    public void IsValidEnumValue_Throws_ArgumentOutOfRangeException()
    {
        var enumValue = (TestEnum)byte.MaxValue;
        Assert.Throws<ArgumentOutOfRangeException>(() => WindowsDeviceHelperMethods.IsValidEnumValue(enumValue));
    }

    [Test]
    public void IsValidEnumValue_SuccessTest()
    {
        var enumValue = TestEnum.TestValue2;
        Assert.DoesNotThrow(() => WindowsDeviceHelperMethods.IsValidEnumValue(enumValue));
    }

    [Test]
    public void ToEnum_Throws_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WindowsDeviceHelperMethods.ToEnum<TestEnum>(byte.MaxValue));
    }

    [Test]
    public void ToEnum_SuccessTest()
    {
        Assert.DoesNotThrow(() => WindowsDeviceHelperMethods.ToEnum<TestEnum>(1));
    }
}
