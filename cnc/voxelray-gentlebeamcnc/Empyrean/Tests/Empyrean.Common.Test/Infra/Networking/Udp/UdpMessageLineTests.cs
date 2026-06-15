using Empyrean.Common.Infra.Networking.Udp;

namespace Empyrean.Common.Test.Infra.Networking.Udp
{
    public class UdpMessageLineTests
    {
        private UdpMessageLine _messageLine;

        [SetUp]
        public void Setup()
        {
            _messageLine = new UdpMessageLine();
        }
        
        [Test]
        public async Task WaitForResponseAsync_ReturnsNull()
        {
            int messageId = 1;

            var result = await _messageLine.WaitForResponseAsync(messageId, 200, CancellationToken.None);
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public async Task WaitForResponseAsync_AddAndRemoveRequest()
        {
            int messageId = 2;
            _messageLine.AddRequest(messageId);
            _messageLine.RemoveRequest(messageId);

            var result = await _messageLine.WaitForResponseAsync(messageId, 200, CancellationToken.None);
            Assert.That(result, Is.Null);
        }
        [Test]
        public async Task WaitForResponseAsync_ResponseWithoutRequest_ReturnsNull()
        {
            int messageId = 3;
            byte[] response = { 1, 2, 3 };
            _messageLine.AddResponse(messageId, response);

            var result = await _messageLine.WaitForResponseAsync(messageId, 200, CancellationToken.None);
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public async Task WaitForResponseAsync()
        {
            int messageId = 111;
            byte[] expectedResponse = { 10, 11, 12 };
            _messageLine.AddRequest(messageId);
            _messageLine.AddResponse(messageId, expectedResponse);

            {
                var result = await _messageLine.WaitForResponseAsync(messageId, 1000, CancellationToken.None);
                Assert.That(result, Is.EqualTo(expectedResponse));
            }
            
            // Repeated call is also works
            {
                var result = await _messageLine.WaitForResponseAsync(messageId, 1000, CancellationToken.None);
                Assert.That(result, Is.EqualTo(expectedResponse));
            }
        }
        
        [Test]
        public async Task WaitForResponseAsync_Multiple()
        {
            int messageId1 = 123;
            int messageId2 = 456;
            byte[] response1 = { 1, 2, 3 };
            byte[] response2 = { 4, 5, 6 };

            _messageLine.AddRequest(messageId1);
            _messageLine.AddRequest(messageId2);
            _messageLine.AddResponse(messageId1, response1);
            _messageLine.AddResponse(messageId2, response2);

            var result1 = await _messageLine.WaitForResponseAsync(messageId1, 200, CancellationToken.None);
            var result2 = await _messageLine.WaitForResponseAsync(messageId2, 200, CancellationToken.None);

            Assert.That(result1, Is.EqualTo(response1));
            Assert.That(result2, Is.EqualTo(response2));
        }
        
        [Test]
        public async Task WaitForResponseAsync_And_Reset()
        {
            int messageId = 111;
            byte[] expectedResponse = { 10, 11, 12 };
            _messageLine.AddRequest(messageId);
            _messageLine.AddResponse(messageId, expectedResponse);

            {
                var result = await _messageLine.WaitForResponseAsync(messageId, 1000, CancellationToken.None);
                Assert.That(result, Is.EqualTo(expectedResponse));
            }
            
            _messageLine.RemoveRequest(messageId);
            
            {
                var result = await _messageLine.WaitForResponseAsync(messageId, 1000, CancellationToken.None);
                Assert.That(result, Is.Null);
            }
        }
        
        [Test]
        public async Task WaitForResponseAsync_WithDelay()
        {
            int messageId = 222;
            byte[] expectedResponse = { 20, 21, 22 };
            _messageLine.AddRequest(messageId);

            var task = _messageLine.WaitForResponseAsync(messageId, 1000, CancellationToken.None);
            await Task.Delay(100);
            _messageLine.AddResponse(messageId, expectedResponse);
        
            var result = await task;
            Assert.That(result, Is.EqualTo(expectedResponse));
        }
        
        [Test]
        public async Task WaitForResponseAsync_WhenNoResponse_ReturnsNull()
        {
            int messageId = 333;
            _messageLine.AddRequest(messageId);

            var result = await _messageLine.WaitForResponseAsync(messageId, 200, CancellationToken.None);
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public async Task WaitForResponseAsync_WhenCancelled_ReturnsNull()
        {
            int messageId = 444;
            byte[] response = { 20, 21, 22 };
            _messageLine.AddRequest(messageId);
            var cts = new CancellationTokenSource();

            var task = _messageLine.WaitForResponseAsync(messageId, 5000, cts.Token);
            await Task.Delay(100);
            await cts.CancelAsync();
            _messageLine.AddResponse(messageId, response);
        
            var result = await task;
            Assert.That(result, Is.Null);
        }
    }
}