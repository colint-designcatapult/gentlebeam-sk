namespace Heracles.Core.Models
{
    public interface IFieldEntryBase : IFieldBase
    {
        public float Current { get; set; }
        public double Duration { get; set; }
        public float Actual { get; set; }
        public int DisplayValue { get; set; }
    }
}
