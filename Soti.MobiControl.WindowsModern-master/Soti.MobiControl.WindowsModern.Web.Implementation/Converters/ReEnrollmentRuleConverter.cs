using System;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using WindowsReEnrollmentCriteriaContract = Soti.MobiControl.WindowsModern.Models.WindowsReEnrollmentCriteria;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters
{
    /// <summary>
    /// Converter class to convert to WindowsReEnrollmentCriteria.
    /// </summary>
    internal static class ReEnrollmentRuleConverter
    {
        /// <summary>
        /// Converts to WindowsReEnrollmentCriteria Model.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <returns>WindowsReEnrollmentCriteria Model.</returns>
        public static WindowsReEnrollmentCriteriaContract ToReEnrollmentRuleCriteriaModel(
            this WindowsReEnrollmentCriteria contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return new Soti.MobiControl.WindowsModern.Models.WindowsReEnrollmentCriteria
            {
                HardwareId = contract.HardwareId,
                MacAddress = contract.MacAddress
            };
        }

        /// <summary>
        /// Converts to WindowsReEnrollmentCriteria Contract.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <returns>WindowsReEnrollmentCriteria Contract.</returns>
        public static WindowsReEnrollmentCriteria ToReEnrollmentRuleCriteriaContract(
            this WindowsReEnrollmentCriteriaContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return new WindowsReEnrollmentCriteria
            {
                HardwareId = contract.HardwareId,
                MacAddress = contract.MacAddress
            };
        }
    }
}