namespace Heracles.Indoor.ViewModels
{
    public class PhotoAcousticProtocol
    {
        public string Name { get; set; }
        public SensorDistance SensorDistanceX { get; set; }
        public SensorDistance SensorDistanceY { get; set; }
        public double Sample { set; get; }
        public EnergyMode EnergyMode { get; set; }
        public double LaserEnergy { set; get; }
        public double SoundSpeed { set; get; }
        public GridSize GridSize { get; set; }
        public double ZeroPadding { set; get; }
        public bool EnableAutofocus { set; get; }
    }
}
