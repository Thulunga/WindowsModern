using System.ComponentModel.DataAnnotations;
using Moq;
using NUnit.Framework;
using Soti.MobiControl.Events;
using Soti.MobiControl.Security.Identity;
using Soti.MobiControl.WindowsModern.Web.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Web.Implementation.Events;
using WindowsReEnrollmentCriteria = Soti.MobiControl.WindowsModern.Web.Contracts.WindowsReEnrollmentCriteria;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests
{
    [TestFixture]
    public class WindowsDeviceConfigurationsControllerTests
    {
        private Mock<IWindowsDeviceConfigurationsService> _mockWindowsDeviceConfigurationsService;
        private Mock<IEventDispatcher> _eventDispatcherMock;
        private Mock<IUserIdentityProvider> _userIdentityProviderMock;
        private IWindowsDeviceConfigurationsController _windowsDeviceConfigurationsController;

        [SetUp]
        public void Setup()
        {
            _mockWindowsDeviceConfigurationsService = new Mock<IWindowsDeviceConfigurationsService>();
            _eventDispatcherMock = new Mock<IEventDispatcher>();
            _userIdentityProviderMock = new Mock<IUserIdentityProvider>();
            _windowsDeviceConfigurationsController = new WindowsDeviceConfigurationsController(
                _eventDispatcherMock.Object,
                _mockWindowsDeviceConfigurationsService.Object,
                _userIdentityProviderMock.Object);
        }

        [Test]
        public void UpdateReEnrollment_HardwareIdNullOptionThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => _windowsDeviceConfigurationsController.SaveWindowsReEnrollmentCriteria(GetReEnrollmentRule(false, true)));
            Assert.Throws<ValidationException>(() => _windowsDeviceConfigurationsController.SaveWindowsReEnrollmentCriteria(null));
        }

        [Test]
        public void UpdateReEnrollment_true()
        {
            _mockWindowsDeviceConfigurationsService.Setup(m => m.UpdateWindowsReEnrollmentCriteria(It.IsAny<Soti.MobiControl.WindowsModern.Models.WindowsReEnrollmentCriteria>()));
            _eventDispatcherMock.Setup(m => m.DispatchEvent(It.IsAny<IEvent>()));
            _windowsDeviceConfigurationsController.SaveWindowsReEnrollmentCriteria(GetReEnrollmentRule(true, true));
            _mockWindowsDeviceConfigurationsService.Verify(m => m.UpdateWindowsReEnrollmentCriteria(It.IsAny<Soti.MobiControl.WindowsModern.Models.WindowsReEnrollmentCriteria>()), Times.Once);
            _eventDispatcherMock.Verify(m => m.DispatchEvent(It.IsAny<UpdateReEnrollmentEvent>()), Times.Once);
        }

        [Test]
        public void UpdateReEnrollment_SuccessWithToggleOff()
        {
            _mockWindowsDeviceConfigurationsService.Setup(m => m.UpdateWindowsReEnrollmentCriteria(It.IsAny<Soti.MobiControl.WindowsModern.Models.WindowsReEnrollmentCriteria>()));
            _eventDispatcherMock.Setup(m => m.DispatchEvent(It.IsAny<IEvent>()));
            _windowsDeviceConfigurationsController.SaveWindowsReEnrollmentCriteria(GetReEnrollmentRule(false, false));
            _mockWindowsDeviceConfigurationsService.Verify(m => m.UpdateWindowsReEnrollmentCriteria(It.IsAny<Soti.MobiControl.WindowsModern.Models.WindowsReEnrollmentCriteria>()), Times.Once);
            _eventDispatcherMock.Verify(m => m.DispatchEvent(It.IsAny<UpdateReEnrollmentEvent>()), Times.Once);
        }

        [Test]
        public void UpdateReEnrollment_NullException()
        {
            Assert.Throws<ValidationException>(() => _windowsDeviceConfigurationsController.SaveWindowsReEnrollmentCriteria(null));
        }

        [Test]
        public void GetReEnrollment_Success()
        {
            _mockWindowsDeviceConfigurationsService.Setup(m => m.GetWindowsReEnrollmentCriteria()).Returns(GetReEnrollmentRule(true, true).ToReEnrollmentRuleCriteriaModel());
            var result = _windowsDeviceConfigurationsController.GetWindowsReEnrollmentCriteria();
            Assert.NotNull(result);
            Assert.AreEqual(true, result.HardwareId);
            Assert.AreEqual(true, result.MacAddress);
        }

        [Test]
        public void GetReEnrollment_SuccessWithToggleOff()
        {
            _mockWindowsDeviceConfigurationsService.Setup(m => m.GetWindowsReEnrollmentCriteria()).Returns(GetReEnrollmentRule(false, false).ToReEnrollmentRuleCriteriaModel());
            var result = _windowsDeviceConfigurationsController.GetWindowsReEnrollmentCriteria();
            Assert.NotNull(result);
            Assert.AreEqual(false, result.HardwareId);
            Assert.AreEqual(false, result.MacAddress);
        }

        #region Private
        private WindowsReEnrollmentCriteria GetReEnrollmentRule(bool hardwareId, bool macAddress)
        {
            var mockRequest = new WindowsReEnrollmentCriteria()
            {
                HardwareId = hardwareId,
                MacAddress = macAddress
            };
            return mockRequest;
        }

        #endregion Private
    }
}
