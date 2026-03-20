using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Events
{
    /// <summary>
    /// Event triggered when Successfully updated Re-Enrollment status for windows.
    /// </summary>
    [EventMetadata("UpdatedReEnrollmentEvent", "On saving/updating the Re-enrollment rule", "Strict Re-enrollment rule {0} for Windows Modern Devices", AlertManagerType.System)]
    internal sealed class UpdateReEnrollmentEvent : UserOriginatedEvent, IParameterizedEvent
    {
        /// <summary>
        /// Creates a new instance of UpdateReEnrollmentEvent class.
        /// </summary>
        /// <param name="userPrincipalId">Id of the user requested application update.</param>
        /// <param name="userName">Name of the user requested application update.</param>
        /// <param name="eventSeverity">Event Severity.</param>
        /// <param name="enrollmentStatus">Enrollment Status Enabled or disabled.</param>
        internal UpdateReEnrollmentEvent(int userPrincipalId, string userName, EventSeverity eventSeverity, string enrollmentStatus)
            : base(userPrincipalId, userName, eventSeverity)
        {
            EventAdditionalParameters = new[] { enrollmentStatus };
        }

        public string[] EventAdditionalParameters { get; }
    }
}
