namespace Xcc.Core.Domain.GryphonBoard
{
    public struct GcbSession
    {
        public GcbSession(uint id, int totalPoints)
        {
            Id = id;
            TotalPoints = totalPoints;
        }

        public uint Id { get; }
        public int TotalPoints { get; }
    }
}
