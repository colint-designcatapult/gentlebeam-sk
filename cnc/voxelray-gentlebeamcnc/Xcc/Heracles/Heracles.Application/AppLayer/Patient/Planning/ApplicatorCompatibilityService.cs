using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Core.Enums;
using System.ComponentModel.DataAnnotations;
using Xcc.Application.Common;
using static Heracles.Application.Common.StringConstants.TreatmentConsole;

namespace Heracles.Application.AppLayer.Patient.Planning
{
    public class ApplicatorCompatibilityService(ICollimatorModel collimatorModel)
    {
        #region public methods
        public ApplicatorCompatibilityStatus Check(ApplicatorParameters requiredParameters)
        {
            var activeCollimator = collimatorModel.ActiveCollimator;
            var actualConfiguration = activeCollimator?.Configuration;

            var actualParameters = ApplicatorParameters.FromValues(actualConfiguration?.Type, actualConfiguration?.Energy);

            var isCompatible = requiredParameters.Equals(actualParameters);
            string suggestionMessage = string.Empty;
            if (!isCompatible)
            {
                var requiredTargetTypeName = requiredParameters.Type.GetAttribute<DisplayAttribute>().Name;
                var requiredEnergyName = requiredParameters.Energy.GetAttribute<DisplayAttribute>().Name;

                string connectMessage = requiredParameters.Type switch
                {
                    TargetType.TargetType_QC_Collimator => Applicator.SuggestedQcApplicatorMessage,
                    _ => string.Format(Applicator.SuggestedApplicatorStringFormat, requiredTargetTypeName, requiredEnergyName)
                };

                suggestionMessage = GetCompatibilitySuggestionMessage(activeCollimator, actualConfiguration, actualParameters, connectMessage);
            }
            return new ApplicatorCompatibilityStatus(isCompatible, suggestionMessage);
        }

        public ApplicatorCompatibilityStatus Check(Energy requiredEnergy)
        {
            var activeCollimator = collimatorModel.ActiveCollimator;
            var actualConfiguration = activeCollimator?.Configuration;

            var actualParameters = ApplicatorParameters.FromValues(actualConfiguration?.Type, actualConfiguration?.Energy);

            var isCompatible = requiredEnergy == actualParameters?.Energy;
            string suggestionMessage = string.Empty;
            if (!isCompatible)
            {
                var requiredEnergyName = requiredEnergy.GetAttribute<DisplayAttribute>().Name;

                string connectMessage = string.Format(Applicator.SuggestedEnergyStringFormat, requiredEnergyName);

                suggestionMessage = GetCompatibilitySuggestionMessage(activeCollimator, actualConfiguration, actualParameters, connectMessage);
            }
            return new ApplicatorCompatibilityStatus(isCompatible, suggestionMessage);
        }
        #endregion public methods


        #region private methods
        private static string GetCompatibilitySuggestionMessage(
            ICollimator activeCollimator, 
            ICollimatorConfiguration actualConfiguration, 
            ApplicatorParameters? actualParameters, 
            string suggestionMessage)
        {
            string message;
            if (activeCollimator is null)
            {
                message = $"{Applicator.NoConnectedApplicator} {suggestionMessage}";
            }
            else if (actualConfiguration is null)
            {
                message = $"{Applicator.UnregisteredApplicator} {suggestionMessage}";
            }
            else
            {
                var actualTypeName = actualParameters.Value.Type.GetAttribute<DisplayAttribute>().Name;
                var actualEnergyName = actualParameters.Value.Energy.GetAttribute<DisplayAttribute>().Name;
                var connectedApplicatorTypeMessage = string.Format(Applicator.ConnectedApplicatorStringFormat, actualTypeName, actualEnergyName);
                message = $"{connectedApplicatorTypeMessage} {suggestionMessage}";
            }

            return message;
        }
        #endregion private methods
    }
}
