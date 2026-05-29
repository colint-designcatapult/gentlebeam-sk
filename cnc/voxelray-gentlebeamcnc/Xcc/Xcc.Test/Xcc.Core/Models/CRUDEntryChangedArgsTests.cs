using Xcc.Core.Enums;
using Xcc.Core.Models;

namespace Xcc.Test.Xcc.Core.Models
{
    public class CRUDEntryChangedArgsTests
    {
        [Test]
        public void CRUDEntryChangedArgs_Defaults(
            [Values(CRUDEntryChangedAction.Undefined,
                CRUDEntryChangedAction.Create,
                CRUDEntryChangedAction.ChangeData,
                CRUDEntryChangedAction.Delete
                )] CRUDEntryChangedAction action)
        {
            var args = new CRUDEntryChangedArgs(action);

            Assert.That(args.Action, Is.EqualTo(action));
            Assert.That(args.Data, Is.Null);
        }
        
        [Test]
        public void CRUDEntryChangedArgs_WithObject(
            [Values(CRUDEntryChangedAction.Undefined,
                CRUDEntryChangedAction.Create,
                CRUDEntryChangedAction.ChangeData,
                CRUDEntryChangedAction.Delete
            )] CRUDEntryChangedAction action)
        {
            object data = new object();
            var args = new CRUDEntryChangedArgs(action, data);

            Assert.That(args.Action, Is.EqualTo(action));
            Assert.That(args.Data, Is.SameAs(data));
        }
    }
}