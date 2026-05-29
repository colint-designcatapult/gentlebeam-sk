namespace Heracles.Robot.Models.Enums
{
    /// <summary>
    /// Actuator control board packet type
    /// </summary>
    public enum AcbPacketType : int
    {
        StatusPoll = 1,
        Led,
        System,
        Motors,
        Actuators
    }
}
