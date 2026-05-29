using System.Threading.Tasks;
using System;
using Xcc.Core.Logging;

namespace Xcc.Application.Helpers
{
    public class ProfilingExecutor
    {
        public static async Task ExecuteTaskAsync(Func<Task> task, ILogWriter logWriter, string prefixMsg = "")
        {
            var t0 = DateTime.UtcNow;
            await task.Invoke();
            var t1 = DateTime.UtcNow;
            var td = t1.Subtract(t0);
            LogMessage(td, prefixMsg, logWriter);
        }

        public static T Execute<T>(Func<T> func, ILogWriter logWriter, string prefixMsg = "") where T : class
        {
            var t0 = DateTime.UtcNow;
            var result = func.Invoke();
            var t1 = DateTime.UtcNow;
            var td = t1.Subtract(t0);
            LogMessage(td, prefixMsg, logWriter);
            return result;
        }

        public static void Execute(Action action, ILogWriter logWriter, string prefixMsg = "")
        {
            var t0 = DateTime.UtcNow;
            action.Invoke();
            var t1 = DateTime.UtcNow;
            var td = t1.Subtract(t0);
            LogMessage(td, prefixMsg, logWriter);
        }

        public static async Task<T> ExecuteTaskAsync<T>(Func<Task<T>> task, ILogWriter logWriter, string prefixMsg = "")
        {
            var t0 = DateTime.UtcNow;
            var result = await task.Invoke();
            var t1 = DateTime.UtcNow;
            var td = t1.Subtract(t0);
            LogMessage(td, prefixMsg, logWriter);
            return result;
        }

        private static void LogMessage(TimeSpan executionTime, string prefix, ILogWriter logWriter)
        {
            _ = logWriter.LogAsync($"{prefix}: time ms {executionTime.TotalMilliseconds}", Core.Enums.LogRecordSeverity.Info, Core.Enums.LogRecordType.System);
        }
    }
}
