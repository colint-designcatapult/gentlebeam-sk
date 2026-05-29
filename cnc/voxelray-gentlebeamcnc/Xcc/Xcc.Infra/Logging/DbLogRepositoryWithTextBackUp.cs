using Empyrean.Common.Infra.Settings;
using Prism.Events;
using Xcc.Core.Logging;

namespace Xcc.Infra.Logging;

public class DbLogRepositoryWithTextBackUp : DbLogRepository
{
    public DbLogRepositoryWithTextBackUp(
        ILogCommands logCommands, 
        ITextLogSettings textLogSettings, 
        IEventAggregator eventAggregator) 
        : base(
            logCommands, 
            textLogSettings, 
            eventAggregator, 
            backUpLogWriter: new TextLogRepositoryAdapter(textLogSettings, eventAggregator))
    {
    }
}