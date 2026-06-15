using Heracles.Core.Enums;

namespace Heracles.Core.Models.RDBMS
{
    public interface IXRayEmissionStep
    {
        public long Id { get; set; }
        public string Target { get; set; }
        public TreatmentFieldName Name { get; set; }
        public int Energy { get; set; }
        public float Duration { get; set; }
        public float ActualDuration { get; set; }
        /// <summary>
        /// TargetMA
        /// </summary>
        public float Current { get; set; }
    }
}
