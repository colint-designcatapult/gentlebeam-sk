using Empyrean.Common.Infra.Networking.Udp;
using Empyrean.Common.Test.TestUtils;
using Heracles.Core.Models;
using Heracles.Robot.Models;
using Heracles.Robot.Models.Enums;
using Heracles.Robot.Services;
using Moq;
using Xcc.Application.Models;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;

namespace Heracles.Robot.Test.Models
{
    internal class AcbCommunicationServiceTests
    {
        private CancellationTokenSource AppCancellationTokenSource { get; set; }

        private ISystemEndPoint AcbEndPoint { get; } = new SystemEndPoint("127.0.0.1:22222");
        private const int RECEIVE_TIMEOUT = 100;
        
        private Mock<IAppGlobals> fakeAppGlobals = new();
        private Mock<Core.Models.IHeraclesMainSettings> fakeAppSettings = new();
        private Mock<ILogRepository> fakeLogService;
        private Mock<IAcbCommConnectionFactory> mockConnectionFactory = new();
        private IAcbMessageConverter? messageGenerator;

        [OneTimeSetUp]
        public void OneTimeSetup() 
        {
            messageGenerator = new AcbMessageConverter();
            
            mockConnectionFactory.Setup(cmd => cmd.GetAcbCommConnection())
                .Returns(() => new UdpClientConnection(AcbEndPoint.Ip(), AcbEndPoint.Port!.Value, clientPort:22));
        }

        [SetUp]
        public void Setup()
        {
            AppCancellationTokenSource = new CancellationTokenSource();
            fakeAppGlobals.SetupGet(cmd => cmd.AppCancellationTokenSource).Returns(AppCancellationTokenSource);
            fakeAppSettings.SetupGet(cmd => cmd.AcbCommandsEndPoint).Returns(AcbEndPoint);
            fakeAppSettings.SetupGet(cmd => cmd.AcbReceiveTimeout).Returns(RECEIVE_TIMEOUT);
            fakeLogService = new Mock<ILogRepository>();
        }

        [TearDown]
        public void Teardown()
        {
            AppCancellationTokenSource.Cancel();
            AppCancellationTokenSource.Dispose();
        }

        #region Tests
        [Test]
        public void Constructor_PositiveTest()
        {
            IAcbCommunicationService? service = null;
            Assert.DoesNotThrow(() => service = GetService());
            Assert.DoesNotThrow(() => service?.Dispose());
        }


        [Test]
        public void StartTest()
        {
            using var service = GetService();
            Assert.DoesNotThrow(service.Start);
            Assert.Throws<InvalidOperationException>(service.Start); // repetitive start does not throw either

            //// Startup error message only, the service will not write to the log
            //// from the listening task until it gets unlocked on its cancellation:
            //VerifyLogServiceCall(Times.Once()); // service doesn't write log anymore, it uses UdpReceiveErrorEvent instead

            Assert.DoesNotThrow(service.Dispose);
        }


        [Test] 
        public void SendAsync_PositiveTest()
        {
            Task echoServer = MakeEchoServer();

            using (var service = GetService())
            {
                service.Start();

                var msg = GetMessage();

                byte[]? response = null;
                Assert.DoesNotThrow(() => response = service.SendRequestAsync(msg!).GetAwaiter().GetResult());
                Assert.That(response, Is.Not.Null);
            }
        }


        [Test]
        public void StopTest()
        {
            using (var service = GetService())
            {
                service.Start();
                Assert.DoesNotThrow(() => service.Stop());
                // Consequential stop should not raise exception
                Assert.DoesNotThrow(() => service.Stop());
            }
        }


        [Test]
        public void RestartTest()
        {
            Task echoServer = MakeEchoServer();

            using (var service = GetService())
            {
                service.Start();
                service.Stop();
                service.Start();

                var msg = GetMessage();

                byte[]? response = null;
                Assert.DoesNotThrow(() => response = service.SendRequestAsync(msg!).GetAwaiter().GetResult());
                Assert.That(response, Is.Not.Null);
            }
        }


        [Test]
        public void SendAsyncBeforeStartTest()
        {
            using (var service = GetService())
            {
                var msg = messageGenerator?.GenerateActuatorCommandMessage(
                    id: AcbActuatorId.Image,
                    command: AcbActuatorCommand.Lock);

                Assert.Throws<InvalidOperationException>(() => service.SendRequestAsync(msg!).GetAwaiter().GetResult());
            }
        }

        [Test]
        public void SendAsyncAfterStopTest()
        {
            using (var service = GetService())
            {
                service.Start();
                service.Stop();
                byte[]? msg = GetMessage();

                Assert.Throws<InvalidOperationException>(() => service.SendRequestAsync(msg!).GetAwaiter().GetResult());
            }
        }


        [Test]
        public void SkipInvalidResponseTest()
        {
            // We use echo response for test here,
            // it's OK as request/response message structure is the same:
            Task echoServer = Network.RunOnetimeUdpServer(
                AcbEndPoint.Port!.Value,
                AppCancellationTokenSource.Token,
                (server, clientEndpoint, request) => {
                    request[request.Length - 1] -= 1; // invalidate CRC
                    server.Send(request, clientEndpoint);
                });


            using var service = GetService();
            service.Start();

            var msg = GetMessage();

            byte[]? response = null;
            Assert.Throws<UdpException>(() => response = service.SendRequestAsync(msg!).GetAwaiter().GetResult());
            Assert.That(response, Is.Null);
        }

        #endregion Tests

        #region Private methods
        private void VerifyLogServiceCall(Times times)
        {
            fakeLogService.Verify(x => x.LogAsync(It.IsAny<string>(), It.IsAny<LogRecordSeverity>(), It.IsAny<LogRecordType>()), times);
        }
        
        IAcbCommunicationService GetService()
        {
            return new AcbCommunicationServiceNew(
                fakeAppGlobals.Object,
                fakeAppSettings.Object,
                mockConnectionFactory.Object);
        }

        private byte[]? GetMessage()
        {
            return messageGenerator?.GenerateActuatorCommandMessage(
                id: AcbActuatorId.Image,
                command: AcbActuatorCommand.Lock);
        }

        private Task MakeEchoServer()
        {
            // We use echo response for test here,
            // it's OK as request/response message structure is the same:
            return Network.RunOnetimeUdpServer(
                AcbEndPoint.Port!.Value,
                AppCancellationTokenSource.Token,
                (server, clientEndpoint, request) => server.Send(request, clientEndpoint));
        }

        #endregion Private methods
    }
}
