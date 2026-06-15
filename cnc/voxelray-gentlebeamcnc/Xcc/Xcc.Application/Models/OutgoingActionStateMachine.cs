using System;
using Xcc.Core.Enums;

namespace Xcc.Application.Models
{
    public class OutgoingActionStateMachine
    {
        public OutgoingActionType Action { get; private set; }

        public OutgoingActionStateMachine(OutgoingActionType transactionType = OutgoingActionType.None)
        {
            Action = transactionType;
        }

        public void AddAction(OutgoingActionType action)
        {
            OutgoingActionType previousAction = Action;
            OutgoingActionType consolidatedAction = action;

            // Now let's handle special cases of action state transition:
            switch (action)
            {
                case OutgoingActionType.None:
                    consolidatedAction = previousAction;
                    break;
                case OutgoingActionType.Create:
                    if (previousAction != OutgoingActionType.None)
                    {
                        throw new ArgumentException("Can't add a field after any other operation");
                    }
                    break;
                case OutgoingActionType.Update:
                    if (previousAction == OutgoingActionType.Delete || previousAction == OutgoingActionType.Ignore)
                    {
                        throw new ArgumentException("Can't update a field after its deletion");
                    }
                    else if (previousAction == OutgoingActionType.Create)
                    {
                        // Merge our updates with initial field creation
                        consolidatedAction = OutgoingActionType.Create;
                    }
                    break;
                case OutgoingActionType.Delete:
                    // If we added something, we just don't need to do anything,
                    // as we remove the field we didn't actually created
                    if (previousAction == OutgoingActionType.Create)
                    {
                        consolidatedAction = OutgoingActionType.Ignore;
                    }
                    else if (previousAction == OutgoingActionType.Delete || previousAction == OutgoingActionType.Ignore)
                    {
                        throw new ArgumentException("Can't delete a field that is already deleted or ignored");
                    }
                    break;
                case OutgoingActionType.Ignore:
                default:
                    // Just keep the action as is
                    break;
            }

            Action = consolidatedAction;
        }

        public void Apply()
        {
            // Reset everything to None exept for Delete/Ignore,
            // as any other action leads to a possibility of Update/Delete actions
            // (and Create too, by mistake, but this shouldn't be accessible, or it will raise an error)
            if (Action != OutgoingActionType.Delete && Action != OutgoingActionType.Ignore)
            {
                Action = OutgoingActionType.None;
            }
            else
            {
                // Once we deleted it, we can then only ignore it 
                Action = OutgoingActionType.Ignore;
            }
        }
    }
}
