namespace Heracles.Application.AppLayer.Patient.Planning
{
    public readonly struct ApplicatorCompatibilityStatus(bool isCompatible, string compatibilityMessage)
    {
        public static ApplicatorCompatibilityStatus Compatible { get; } 
            = new ApplicatorCompatibilityStatus(true, string.Empty);

        public bool IsCompatible => isCompatible;
        public string CompatibilityMessage => compatibilityMessage;
    }
}
