using System.ComponentModel.DataAnnotations;
using Soti.Api.Metadata.DataRetrieval;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// Provides extension methods to validate DataRetrievalOptionsSkipTakeOnly instances.
/// </summary>
internal static class DataRetrievalOptionsValidator
{
    /// <summary>
    /// Validates the skip and take values in the data retrieval options.
    /// </summary>
    /// <param name="dataRetrievalOptions">The data retrieval options to validate.</param>
    /// <exception cref="ValidationException">
    /// Thrown when Skip is less than 0 or Take is less than or equal to 0.
    /// </exception>
    internal static void Validate(this DataRetrievalOptionsSkipTakeOnly dataRetrievalOptions)
    {
        if (dataRetrievalOptions == null)
        {
            return;
        }

        if (dataRetrievalOptions.Skip < 0)
        {
            throw new ValidationException("Skip value should be greater than or equal to zero.");
        }

        if (dataRetrievalOptions.Take <= 0)
        {
            throw new ValidationException("Take value should be greater than zero");
        }
    }
}
