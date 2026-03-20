using System;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests.Converters
{
    [TestFixture]
    public class ReEnrollmentRuleConverterTests
    {
        [Test]
        public void ToReEnrollmentRuleModel_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => ReEnrollmentRuleConverter.ToReEnrollmentRuleCriteriaModel(null));
        }

        [Test]
        public void ToReEnrollmentRuleContract_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => ReEnrollmentRuleConverter.ToReEnrollmentRuleCriteriaContract(null));
        }
    }
}
