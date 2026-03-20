using System.Threading.Tasks;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.GroupPolicy;

namespace Soti.MobiControl.WindowsModern
{
    /// <summary>
    /// Windows Device Configuration Service.
    /// </summary>
    public interface IWindowsDeviceConfigurationsService
    {
        /// <summary>
        /// Updates the Re-Enrollment Criteria for windows.
        /// </summary>
        /// <param name="options">ReEnrollmentRule.</param>
        void UpdateWindowsReEnrollmentCriteria(WindowsReEnrollmentCriteria options);

        /// <summary>
        /// Gets Re-Enrollment Criteria for windows.
        /// </summary>
        WindowsReEnrollmentCriteria GetWindowsReEnrollmentCriteria();

        /// <summary>
        /// Check for Strict Re-enrollment.
        /// </summary>
        bool CheckForStrictReEnrollment();

        /// <summary>
        /// Retrieves the Group Policy Settings in the system.
        /// </summary>
        /// <returns>Group Policy Setting.</returns>
        public WindowsModernGroupPolicyObjectsSettings GetGroupPolicySettings();

        /// <summary>
        /// Updates the Group Policy Settings from Soti Services.
        /// </summary>
        Task UpdateGroupPolicySettings();

        /// <summary>
        /// Updates the Windows Product Version Data in Settings from Soti Services.
        /// </summary>
        Task UpdateWindowsUpdatesPolicyProductVersionSettings();
    }
}
