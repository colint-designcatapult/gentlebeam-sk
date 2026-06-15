namespace Xcc.Core.Models
{
    public interface IEntryAccess<IType>
    {
        IType GetData();
        IType SetData(IType value);
    }
}
