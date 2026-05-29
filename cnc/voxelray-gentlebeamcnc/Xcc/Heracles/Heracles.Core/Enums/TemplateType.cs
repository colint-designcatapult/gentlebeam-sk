namespace Heracles.Core.Enums
{
    public enum TemplateType : int
    {
        /// <summary>
        /// Template used for simulating lesions, including surrounding margins.
        /// </summary>
        Simulation = 1, 

        /// <summary>
        /// Template used during treatment planning, such as defining fields and shielding areas.
        /// </summary>
        Treatment,      

        /// <summary>
        /// Template for follow-up visits or checkups after treatment.
        /// </summary>
        FollowUp,       

        /// <summary>
        /// Template for On-Treatment Verification (OTV) or periodic evaluations during treatment.
        /// </summary>
        OTV,            

        /// <summary>
        /// Template for other types not covered by the specified categories.
        /// </summary>
        Other
    }
}
