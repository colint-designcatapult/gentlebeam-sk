using Empyrean.Common.Infra.Networking.Udp;

namespace Empyrean.Common.Test.Infra.Networking.Udp
{
    internal class UdpClientConnectionTests
    {
        const string hostIp = "127.0.0.1";
        const int hostPort = 0xeeef;

        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public IUdpClientConnection Connection { get; private set; }        
                
        [SetUp]
        public void Setup()
        {
            CancellationTokenSource = new CancellationTokenSource();
            Connection = new UdpClientConnection(hostIp, hostPort);
        }

        [TearDown]
        public void Teardown()
        {
            Connection.Dispose();
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }

        [Test]
        public void Dispose_NotThrows()
        {
            var connection = new UdpClientConnection(hostIp, hostPort);
            
            Assert.DoesNotThrow(() => connection.Dispose());
            
            // Second Dispose for check first branch
            Assert.DoesNotThrow(() => connection.Dispose());
        }

        [Test]
        public void SendAsyncTest()
        {
            byte[] testRequest = { 1, 2, 3, 4 };
            byte[]? receivedRequest = null;
            Task serverTask = TestUtils.Network.RunOnetimeUdpServer(
                hostPort,
                CancellationTokenSource.Token,
                (server, clientEndpoint, request) => receivedRequest = request);

            Assert.DoesNotThrow(() => Connection.SendAsync(testRequest).GetAwaiter().GetResult());
            
            // Wait for server to handle the request (200ms should be enough, this is just to not hang on with the test):
            serverTask.Wait(200);

            Assert.That(receivedRequest, Is.Not.Null);
            Assert.That(receivedRequest, Is.EqualTo(testRequest));
        }

        [Test]
        public void SetEndpointTest()
        {
            int newPort = hostPort + 1;

            byte[] testRequest = { 1, 2, 3, 4 };
            byte[]? receivedRequest = null;
            Task serverTask = TestUtils.Network.RunOnetimeUdpServer(
                newPort,
                CancellationTokenSource.Token,
                (server, clientEndpoint, request) => receivedRequest = request);

            Connection.SetEndpoint(hostIp, newPort);
            Assert.DoesNotThrow(() => Connection.SendAsync(testRequest).GetAwaiter().GetResult());

            // Wait for server to handle the request (200ms should be enough, this is just to not hang on with the test):
            serverTask.Wait(200);

            Assert.That(receivedRequest, Is.Not.Null);
            Assert.That(receivedRequest, Is.EqualTo(testRequest));
        }
    }

    internal class AsyncUdpClientConnection_PortReuseTests
    {
        const string hostIp = "127.0.0.1";
        const int hostPort = 0xeeef;
        const int clientPort = 0xeeee;

        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public IUdpClientConnection Connection { get; private set; }

        [SetUp]
        public void Setup()
        {
            CancellationTokenSource = new CancellationTokenSource();
            Connection = new UdpClientConnection(hostIp, hostPort, clientPort, reusePort: true);
        }

        [TearDown]
        public void Teardown()
        {
            Connection.Dispose();
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }
       
        [Test]
        public void PortReuseTest()
        {
            byte[] testRequest = { 1, 2, 3, 4 };
            byte[]? receivedRequest = null;
            Task serverTask = TestUtils.Network.RunOnetimeUdpServer(
                hostPort,
                CancellationTokenSource.Token,
                (server, clientEndpoint, request) => {
                    receivedRequest = request;
                    server.Send(request, clientEndpoint);
                });


            // Check if we can create one more connection on the same port:
            UdpClientConnection? secondConnection = null;
            Assert.DoesNotThrow(() => secondConnection = new UdpClientConnection(hostIp, hostPort, clientPort, reusePort: true));

            // Now send a request to the server
            Connection.SendAsync(testRequest).GetAwaiter().GetResult();

            // Wait for server to handle the request (200ms should be enough, this is just to not hang on with the test):
            serverTask.Wait(200);

            Assert.That(receivedRequest, Is.Not.Null);
            Assert.That(receivedRequest, Is.EqualTo(testRequest));

            // Now try get request back from first connection:
            var response = Connection.ReceiveAsync(CancellationTokenSource.Token).GetAwaiter().GetResult();
            Assert.That(response, Is.Not.Null);
        }
    }
}
