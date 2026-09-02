namespace Xcc.Core.Domain.GryphonBoard
{
    public readonly struct GcbSession
    {
        public GcbSession(uint id)
        {
            Id = id;
        }

        public uint Id { get; }
    }
}
