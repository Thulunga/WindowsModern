using System;
using System.ComponentModel.DataAnnotations;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

internal static class DateRangeValidatorMethod
{
    internal static void ValidateDateRange(DateTime? startDateTime, DateTime? endDateTime, bool allowNullValues = true)
    {
        if (startDateTime == null && endDateTime == null && allowNullValues)
        {
            return;
        }

        if (startDateTime == null || endDateTime == null)
        {
            throw new ValidationException("Start time or end time cannot be null.");
        }

        if (startDateTime > endDateTime)
        {
            throw new ValidationException("Start time cannot be later than the end time.");
        }
    }
}
