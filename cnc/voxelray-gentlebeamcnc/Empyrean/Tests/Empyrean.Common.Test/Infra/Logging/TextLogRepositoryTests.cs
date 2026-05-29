using Empyrean.Common.Infra.Logging;

namespace Empyrean.Common.Test.Infra.Logging
{
    public class TextLogRepositoryTests
    {
        private string _testLogFilePath;

        [SetUp]
        public void Setup()
        {
            _testLogFilePath = Path.Combine(Path.GetTempPath(), $"test_log_{Guid.NewGuid()}.log");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testLogFilePath))
                File.Delete(_testLogFilePath);
        }
        
        [Test]
        public void Constructor_ThrowsException(
            [Values(null, "")] string? logFilename)
        {
            var exception = Assert.Throws<Exception>(() => new TextLogRepository(logFilename));
            Assert.That(exception.Message, Contains.Substring("log file is not specified"));
        }
        
        [Test]
        public void Constructor_WithInvalidFilename_ThrowsException()
        {
            var invalidFilename = Path.Combine("Z:", "invalid", "path", "that", "doesnt", "exist", "log.txt");

            var exception = Assert.Throws<Exception>(() => new TextLogRepository(invalidFilename));
            Assert.That(exception.Message, Contains.Substring("log file doesn't exist and could not be created"));
        }
        
        [Test]
        public void Constructor_CreateFile()
        {
            Assert.That(File.Exists(_testLogFilePath), Is.False);
            
            var repository = new TextLogRepository(_testLogFilePath);
            
            Assert.That(File.Exists(_testLogFilePath), Is.True);
        }
        
        [Test]
        public void Log()
        {
            string message = "Test message";
            string severity = "Info";
            string type = "System";

            var repository = new TextLogRepository(_testLogFilePath);
            repository.Log(message, severity, type);
        
            Task.Delay(100).Wait();

            var lines = File.ReadAllLines(_testLogFilePath);
            Assert.That(lines.Length, Is.EqualTo(1));

            var line = lines[0];
            Assert.That(line, Contains.Substring(message));
            Assert.That(line, Contains.Substring(severity));
            Assert.That(line, Contains.Substring(type));
        }
        
        [Test]
        public async Task LogAsync()
        {
            string message = "Test message";
            string severity = "Info";
            string type = "System";

            var repository = new TextLogRepository(_testLogFilePath);
            await repository.LogAsync(message, severity, type);
        
            await Task.Delay(100);

            var lines = File.ReadAllLines(_testLogFilePath);
            Assert.That(lines.Length, Is.EqualTo(1));
            
            var line = lines[0];
            Assert.That(line, Contains.Substring(message));
            Assert.That(line, Contains.Substring(severity));
            Assert.That(line, Contains.Substring(type));
        }
        
        [Test]
        public void Log_ReplaceNewLines()
        {
            string message = $"Test{Environment.NewLine}message";
            string severity = "Info";
            string type = "System";

            var repository = new TextLogRepository(_testLogFilePath);
            repository.Log(message, severity, type);
        
            Task.Delay(100).Wait();

            var lines = File.ReadAllLines(_testLogFilePath);
            Assert.That(lines.Length, Is.EqualTo(1));

            var line = lines[0];
            Assert.That(line, Contains.Substring("Test message"));
            Assert.That(line, Contains.Substring(severity));
            Assert.That(line, Contains.Substring(type));
        }
        
        [Test]
        public void Fetch()
        {
            var now1 = DateTime.Now;
            var now2 = now1.AddMinutes(1);
            File.WriteAllLines(_testLogFilePath, new[]
            {
                $"{now1}   Info   System   Message 1", 
                $"{now2}   Error   System   Message 2"
            });
         
            var repository = new TextLogRepository(_testLogFilePath);   
            var records = repository.Fetch();
            
            // TODO not implemented/commented
            // TODO Assert.That(records.Count, Is.EqualTo(2));
            // TODO 
            // TODO Assert.That(records[0].TimeStamp, Is.EqualTo(now1).Within(TimeSpan.FromSeconds(1)));
            // TODO Assert.That(records[0].Severity, Is.EqualTo("Info"));
            // TODO Assert.That(records[0].Type, Is.EqualTo("System"));
            // TODO Assert.That(records[0].Message, Is.EqualTo("Message 1"));
            // TODO 
            // TODO Assert.That(records[1].TimeStamp, Is.EqualTo(now2).Within(TimeSpan.FromSeconds(1)));
            // TODO Assert.That(records[1].Severity, Is.EqualTo("Error"));
            // TODO Assert.That(records[1].Type, Is.EqualTo("System"));
            // TODO Assert.That(records[1].Message, Is.EqualTo("Message 2"));
        }
        
        [Test]
        public void Fetch_IgnoresEmptyLines()
        {
            var now1 = DateTime.Now;
            var now2 = now1.AddMinutes(1);
            File.WriteAllLines(_testLogFilePath, new[]
            {
                "",
                $"{now1}   Info   System   Message 1",
                "",
                $"{now2}   Error   System   Message 2",
                "",
            });
         
            var repository = new TextLogRepository(_testLogFilePath);   
            var records = repository.Fetch();
            
            // TODO not implemented/commented
            // TODO Assert.That(records.Count, Is.EqualTo(2));
            // TODO 
            // TODO Assert.That(records[0].TimeStamp, Is.EqualTo(now1).Within(TimeSpan.FromSeconds(1)));
            // TODO Assert.That(records[0].Severity, Is.EqualTo("Info"));
            // TODO Assert.That(records[0].Type, Is.EqualTo("System"));
            // TODO Assert.That(records[0].Message, Is.EqualTo("Message 1"));
            // TODO 
            // TODO Assert.That(records[1].TimeStamp, Is.EqualTo(now2).Within(TimeSpan.FromSeconds(1)));
            // TODO Assert.That(records[1].Severity, Is.EqualTo("Error"));
            // TODO Assert.That(records[1].Type, Is.EqualTo("System"));
            // TODO Assert.That(records[1].Message, Is.EqualTo("Message 2"));
        }
        
        [Test]
        public async Task FetchAsync()
        {
            var now1 = DateTime.Now;
            var now2 = now1.AddMinutes(1);
            await File.WriteAllLinesAsync(_testLogFilePath, new[]
            {
                $"{now1}   Info   System   Message 1", 
                $"{now2}   Error   System   Message 2"
            });
            
            var repository = new TextLogRepository(_testLogFilePath);
            var records = await repository.FetchAsync();
            
            Assert.That(records.Count, Is.EqualTo(2));
            
            Assert.That(records[0].TimeStamp, Is.EqualTo(now1).Within(TimeSpan.FromSeconds(1)));
            Assert.That(records[0].Severity, Is.EqualTo("Info"));
            Assert.That(records[0].Type, Is.EqualTo("System"));
            Assert.That(records[0].Message, Is.EqualTo("Message 1"));
            
            Assert.That(records[1].TimeStamp, Is.EqualTo(now2).Within(TimeSpan.FromSeconds(1)));
            Assert.That(records[1].Severity, Is.EqualTo("Error"));
            Assert.That(records[1].Type, Is.EqualTo("System"));
            Assert.That(records[1].Message, Is.EqualTo("Message 2"));
        }
        
        [Test]
        public async Task FetchAsync_IgnoresEmptyLines()
        {
            var now1 = DateTime.Now;
            var now2 = now1.AddMinutes(1);
            await File.WriteAllLinesAsync(_testLogFilePath, new[]
            {
                "",
                $"{now1}   Info   System   Message 1",
                "",
                $"{now2}   Error   System   Message 2",
                "",
            });

            var repository = new TextLogRepository(_testLogFilePath);
            var records = await repository.FetchAsync();
            
            Assert.That(records.Count, Is.EqualTo(2));
            
            Assert.That(records[0].TimeStamp, Is.EqualTo(now1).Within(TimeSpan.FromSeconds(1)));
            Assert.That(records[0].Severity, Is.EqualTo("Info"));
            Assert.That(records[0].Type, Is.EqualTo("System"));
            Assert.That(records[0].Message, Is.EqualTo("Message 1"));
            
            Assert.That(records[1].TimeStamp, Is.EqualTo(now2).Within(TimeSpan.FromSeconds(1)));
            Assert.That(records[1].Severity, Is.EqualTo("Error"));
            Assert.That(records[1].Type, Is.EqualTo("System"));
            Assert.That(records[1].Message, Is.EqualTo("Message 2"));
        }
        
        [Test]
        public async Task Fetch_BadFileContentBreakFetch()
        {
            byte[] badContent = { 0xFF, 0xFE, 0xFD };
            await File.WriteAllBytesAsync(_testLogFilePath, badContent);
            
            var now1 = DateTime.Now;
            var now2 = now1.AddMinutes(1);
            await File.AppendAllLinesAsync(_testLogFilePath, new[]
            {
                $"{now1}   Info   System   Message 1", 
                $"{now2}   Error   System   Message 2"
            });
        
            var repository = new TextLogRepository(_testLogFilePath);
            var records = repository.Fetch();
            
            Assert.That(records.Count, Is.EqualTo(0));
        }
        
        [Test]
        public async Task FetchAsync_BadFileContentBreakFetch()
        {
            byte[] badContent = { 0xFF, 0xFE, 0xFD };
            await File.WriteAllBytesAsync(_testLogFilePath, badContent);
            
            var now1 = DateTime.Now;
            var now2 = now1.AddMinutes(1);
            await File.AppendAllLinesAsync(_testLogFilePath, new[]
            {
                $"{now1}   Info   System   Message 1", 
                $"{now2}   Error   System   Message 2"
            });
        
            var repository = new TextLogRepository(_testLogFilePath);
            var records = await repository.FetchAsync();
            
            Assert.That(records.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void Fetch_ThrowsException()
        {
            var repository = new TextLogRepository(_testLogFilePath);
         
            File.Delete(_testLogFilePath);
            
            var exception = Assert.Throws<Exception>(() => repository.Fetch());
            Assert.That(exception.Message, Contains.Substring("failed to read log file"));
        }
        
        [Test]
        public void FetchAsync_ThrowsException()
        {
            var repository = new TextLogRepository(_testLogFilePath);
         
            File.Delete(_testLogFilePath);
            
            var exception = Assert.ThrowsAsync<Exception>(async () => await repository.FetchAsync());
            Assert.That(exception.Message, Contains.Substring("failed to read log file"));
        }
        
        [Test]
        public async Task LogAsync_MultipleCalls()
        {
            var repository = new TextLogRepository(_testLogFilePath);
            var tasks = new List<Task>();

            var count = 20;
            for (int i = 0; i < count; i++)
            {
                tasks.Add(repository.LogAsync($"Message {i}", "Info", "System"));
            }
            await Task.WhenAll(tasks);
            await Task.Delay(100);

            var lines = await repository.FetchAsync();
            var messages = lines.Select(x => x.Message).ToList();
            
            var expectedMessages = Enumerable.Range(0, count).Select(i => $"Message {i}").ToList();
            Assert.That(lines.Count, Is.EqualTo(count));
            Assert.That(messages, Is.EquivalentTo(expectedMessages));
        }
        
        [Test]
        public async Task Log_ParallelCalls()
        {
            var repository = new TextLogRepository(_testLogFilePath);

            var count = 20;
            Parallel.For(0, count, i =>
            {
                repository.Log($"Parallel message {i}", "Info", "System");
            });
            await Task.Delay(1000);

            var lines = await repository.FetchAsync();
            var messages = lines.Select(x => x.Message).ToList();
            
            var expectedMessages = Enumerable.Range(0, count).Select(i => $"Parallel message {i}").ToList();
            Assert.That(messages.Count, Is.EqualTo(count));
            Assert.That(messages, Is.EquivalentTo(expectedMessages));
        }
        
        [Test]
        public void CanFetch()
        {
            var repository = new TextLogRepository(_testLogFilePath);
            Assert.That(repository.CanFetch(), Is.True);
        }
    }
}