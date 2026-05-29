namespace Heracles.Core.Models
{
    public interface IServiceEmissionStep
    {
        public string Target { get; set; }
        public int Energy { get; set; }
        public int Duration { get; set; }
        public int ActutalDuration { get; set; }
    }
}
