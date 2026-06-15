namespace Empyrean.Common.Core.Domain.DataManagement.Common
{
    public interface IEntry
    {
        long Id { get; set; }
    }

    public interface INamedEntry : IEntry
    {
        string Name { get; set; }
    }
}
