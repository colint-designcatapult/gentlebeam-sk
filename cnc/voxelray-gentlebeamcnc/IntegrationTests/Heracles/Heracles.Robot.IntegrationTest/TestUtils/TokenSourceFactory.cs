namespace Heracles.Robot.IntegrationTest.TestUtils
{
    public class TokenSourceFactory
    {
        public static CancellationTokenSource CreateAutoCancellingTokenSource(int timeout)
        {
            CancellationTokenSource tokenSource = new();
            // Run token auto-cancellation task
            _ = Task.Run(async () =>
            {
                await Task.Delay(timeout, tokenSource.Token);
                tokenSource.Cancel();
            });
            return tokenSource;
        }
    }
}
