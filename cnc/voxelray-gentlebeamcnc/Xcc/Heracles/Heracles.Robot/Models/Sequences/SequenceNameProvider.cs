using System;

namespace Heracles.Robot.Models.Sequences
{
    public class SequenceNameProvider
    {
        public static string Provide(SequenceId sequenceId, Workspace workspace)
        {
            switch(sequenceId)
            {
                case SequenceId.PickHead:
                    switch (workspace)
                    {
                        case Workspace.Left:
                            return SequenceName.LeftUserPickHead;
                        case Workspace.Right:
                            return SequenceName.RightUserPickHead;
                        default:
                            throw new ArgumentException("Unknown argument: sequenceId=" + sequenceId.ToString() + " workspace=" + workspace.ToString());
                    }
                case SequenceId.Treat:
                    switch (workspace)
                    {
                        case Workspace.Left:
                            return SequenceName.LeftUserTreat;
                        case Workspace.Right:
                            return SequenceName.RightUserTreat;
                        default:
                            throw new ArgumentException("Unknown argument: sequenceId=" + sequenceId.ToString() + " workspace=" + workspace.ToString());
                    }
                case SequenceId.PlaceHead:
                    switch (workspace)
                    {
                        case Workspace.Left:
                            return SequenceName.LeftUserPlaceHead;
                        case Workspace.Right:
                            return SequenceName.RightUserPlaceHead;
                        default:
                            throw new ArgumentException("Unknown argument: sequenceId=" + sequenceId.ToString() + " workspace=" + workspace.ToString());
                    }
                case SequenceId.PlaceHeadFromQC:
                    switch (workspace)
                    {
                        case Workspace.Left:
                            return SequenceName.LeftUserPlaceHeadFromQC;
                        default:
                            throw new ArgumentException("Unknown argument: sequenceId=" + sequenceId.ToString() + " workspace=" + workspace.ToString());
                    }
                case SequenceId.QC:
                    switch (workspace)
                    {
                        case Workspace.Left:
                            return SequenceName.QC;
                        default:
                            throw new ArgumentException("Unknown argument: sequenceId=" + sequenceId.ToString() + " workspace=" + workspace.ToString());
                    }
                default:
                    throw new ArgumentException("Unknown argument: sequenceId=" + sequenceId.ToString());
            }
        }
    }
}
