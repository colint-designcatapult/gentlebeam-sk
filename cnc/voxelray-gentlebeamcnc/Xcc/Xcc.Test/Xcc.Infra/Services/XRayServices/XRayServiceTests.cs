using Empyrean.Common.Infra.Networking.Udp;
using Moq;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Infra.GryphonBoard.CommandAPI;
using Xcc.Test.Xcc.Infra;

namespace Xcc.Test.Xcc.Infra.Services.XRayServices
{
    [NonParallelizable]
    internal class XRayServiceTests
    {
        Mock<IGcbXRayCommandOperator> fakeCommandOperator = new();
        Mock<IGcbCommunicationService> fakeCommunicationService = new();
        Mock<ILogWriter> fakeLogService = new();
        IGcbXRayCommandOperator actualCommandOperator = new GcbXRayCommandOperator();

        [SetUp]
        public void SetUp()
        {
            fakeCommandOperator = new();
            fakeCommunicationService = new();
            fakeLogService = new();
            actualCommandOperator = new GcbXRayCommandOperator();
        }

        private GcbCommandInterface MakeService(bool useFakeCommandOperator = false)
        {
            if (useFakeCommandOperator)
            {
                return new GcbCommandInterface(fakeCommandOperator.Object, fakeCommunicationService.Object, fakeLogService.Object);
            }
            else
            {
                return new GcbCommandInterface(actualCommandOperator, fakeCommunicationService.Object, fakeLogService.Object);
            }
        }

        private static GcbOperationalPoint MakeOperationalPoint()
        {
            return new GcbOperationalPoint
            {
                TotalPointTime = 2.0f,
                RemainingPointTime = 1.0f,
                SetpointKv = 50.0f,
                FilamentSetpoint = 3500.0f,
                TargetMA = 2.0f,
                XCoilSetpoint = 0.1f,
                YCoilSetpoint = 0.2f,
                FocusCoilSetpoint = 2000.0f
            };
        }

        private static FaultEntry MakeFaultEntry(SystemFault faultType)
        {
            const string format = "Filament fault.";
            return new FaultEntry(
                faultType,
                CrcUtils.ComputeChecksum(System.Text.Encoding.ASCII.GetBytes(format)),
                GcbStateNew.Warmup,
                0x1eadc0de,
                format,
                format);
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void GetVersionInfoTest()
        {
            VersionInfo versionInfo = new()
            {
                FirmwareVersion = "1.2.3",
                FirmwareChecksum = 4,
                Mode = FirmwareMode.Demo,
                HvpsFirmwareVersion = "5.6.7",
                HvpsMode = FirmwareMode.Normal
            };
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateVersionInfoResponse(0, versionInfo)));

            var service = MakeService();

            VersionInfo? receivedVersionInfo = null;
            Assert.DoesNotThrow(() => receivedVersionInfo = service.GetVersionInfo().GetAwaiter().GetResult());
            Assert.That(receivedVersionInfo, Is.Not.Null);
            Assert.That(receivedVersionInfo, Is.EqualTo(versionInfo));
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => MakeService());
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void SendOperationalPoint_PositiveTest([Values] OperationalPointCmdType commandType)
        {
            var fieldStatuses = Enumerable.Repeat(0, 9).ToList();
            var responseData =
                GcbXRayCmdResponseGenerator.GenerateOperationalPointResponse(0, commandType, fieldStatuses);
            
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(responseData));

            var service = MakeService();

            Assert.DoesNotThrow(
                () => service.SendOperationalPoint(
                    commandType, 
                    operationalPoint: MakeOperationalPoint(),
                    session: new GcbSession(id: 42)
                    ).GetAwaiter().GetResult());
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void SendOperationalPoint_WrongPointStatusTest([Values] OperationalPointCmdType commandType)
        {
            var fieldStatuses = Enumerable.Repeat(0, 9).ToList();
            fieldStatuses[0] = (int)OperationalPointStatus.InvalidValue; // Make first point status invalud
            
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateOperationalPointResponse(0, commandType, fieldStatuses)));

            var service = MakeService();
            
            Assert.Throws<Exception>(
                () => service.SendOperationalPoint(
                    commandType,
                    operationalPoint: MakeOperationalPoint(),
                    session: new GcbSession(id: 42)
                    ).GetAwaiter().GetResult());
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void SendOperationalPoint_InvalidResponsePacketTest()
        {
            var invalidPointResponsePacket = UdpPacketBuilder.BuildPacket(
                packetType: (uint)GCBPacketType.OperationalPointLoadingResponse,
                packetCounter: 0,
                payload: [0]).Buffer;

            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(invalidPointResponsePacket));

            var service = MakeService();

            Assert.Throws<Exception>(
                () => service.SendOperationalPoint(
                    OperationalPointCmdType.Load, 
                    operationalPoint: MakeOperationalPoint(),
                    session: new GcbSession(id: 0)
                    ).GetAwaiter().GetResult());
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void SendOperationalPoint_NullResponsePacketTest()
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>())).Returns(Task.FromResult((byte[])null!));

            var service = MakeService(useFakeCommandOperator: false);
            
            Assert.ThrowsAsync<ArgumentNullException>(
                () => service.SendOperationalPoint(
                    OperationalPointCmdType.Load,
                    operationalPoint: MakeOperationalPoint(),
                    session: new GcbSession(id: 0)
                ));
        }


        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void SendDirectiveCommand_PositiveTest()
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateDirectiveResponse(0, status: GcbProcessingStatus.OK)));

            var service = MakeService();

            Assert.DoesNotThrow(() => service.SendDirectiveCommand(GCBDirectiveCommandNew.Initialize).GetAwaiter().GetResult());
            // Just make same assertions for all types of directives now:
            Assert.DoesNotThrow(() => service.Stop().GetAwaiter().GetResult());
            Assert.DoesNotThrow(() => service.Initialize().GetAwaiter().GetResult());
            Assert.DoesNotThrow(() => service.StagePlan().GetAwaiter().GetResult());
            Assert.DoesNotThrow(() => service.ClearFaults().GetAwaiter().GetResult());
            Assert.DoesNotThrow(() => service.ClearPlan().GetAwaiter().GetResult());
            Assert.DoesNotThrow(() => service.ResetTimers().GetAwaiter().GetResult());
        }

        [TestCase(GcbProcessingStatus.OutOfBounds)]
        [TestCase(GcbProcessingStatus.AccessError)]
        [TestCase(GcbProcessingStatus.InvalidValue)]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void SendDirectiveCommand_NegativeProcessingStatusTest(GcbProcessingStatus processingStatus)
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateDirectiveResponse(0, status: processingStatus)));            

            var service = MakeService();

            Assert.Throws<Exception>(() => service.SendDirectiveCommand(GCBDirectiveCommandNew.Initialize).GetAwaiter().GetResult());
            // Just make same assertions for all types of directives now:
            Assert.Throws<Exception>(() => service.Stop().GetAwaiter().GetResult());
            Assert.Throws<Exception>(() => service.Initialize().GetAwaiter().GetResult());
            Assert.Throws<Exception>(() => service.StagePlan().GetAwaiter().GetResult());
            Assert.Throws<Exception>(() => service.ClearFaults().GetAwaiter().GetResult());
            Assert.Throws<Exception>(() => service.ClearPlan().GetAwaiter().GetResult());
            Assert.Throws<Exception>(() => service.ResetTimers().GetAwaiter().GetResult());
        }

        [TestCase(GcbProcessingStatus.OK, GcbProcessingStatus.OK, false)]
        [TestCase(GcbProcessingStatus.InvalidValue, GcbProcessingStatus.OK, true)]
        [TestCase(GcbProcessingStatus.OK, GcbProcessingStatus.InvalidValue, true)]
        [TestCase(GcbProcessingStatus.InvalidValue, GcbProcessingStatus.InvalidValue, true)]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void ReleasePlanTest(GcbProcessingStatus scopeStatus, GcbProcessingStatus authStatus, bool throwsException)
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateReleasePlanResponse(0, scopeStatus, authStatus)));

            var service = MakeService();

            var scope = GCBReleaseCommandScope.Plan;
            var session = new GcbSession(id: 42);


            if (throwsException)
            {
                Assert.Throws<Exception>(() => service.ReleasePlan(scope, session).GetAwaiter().GetResult());
            }
            else
            {
                Assert.DoesNotThrow(() => service.ReleasePlan(scope, session).GetAwaiter().GetResult());
            }
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void NewSessionCommandTest([Values] GcbProcessingStatus responseStatus)
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateNewSessionResponse(0, responseStatus, sessionId:42)));

            var service = MakeService();

            if (responseStatus != GcbProcessingStatus.OK)
            {
                Assert.Throws<Exception>(() => service.NewSession().GetAwaiter().GetResult());
            }
            else
            {
                Assert.DoesNotThrow(() => service.NewSession().GetAwaiter().GetResult());
            }
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void GetFaultsCommandTest()
        {
            FaultEntry entry = MakeFaultEntry(SystemFault.FilamentFault);
            var update = new FaultUpdate(1, 0, 1, entry);
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateFaultInfoResponse(0, update)));

            var service = MakeService();

            FaultSnapshot? snapshot = null;
            Assert.DoesNotThrow(() => snapshot = service.GetFaults().GetAwaiter().GetResult());
            Assert.That(snapshot!.Entries, Is.EqualTo(new[] { entry }));
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void GetFaultsCommand_NullResponseTest()
        {
            // Return null response
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult((byte[])null!));

            var service = MakeService(useFakeCommandOperator: false);

            Assert.ThrowsAsync<ArgumentNullException>(() => service.GetFaults());
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void GetFaultsCommand_EmptyResponseTest()
        {
            // Return null response
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(Array.Empty<byte>()));

            var service = MakeService();

            Assert.ThrowsAsync<ArgumentNullException>(() => service.GetFaults());
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void ConditioningCommandTest([Values] GcbProcessingStatus responseStatus)
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateConditioningResponse(0, responseStatus)));

            var service = MakeService();

            if (responseStatus != GcbProcessingStatus.OK)
            {
                Assert.Throws<Exception>(() => service.Conditioning(conditioningSetpoint: 2000f).GetAwaiter().GetResult());
            }
            else
            {
                Assert.DoesNotThrow(() => service.Conditioning(conditioningSetpoint: 2000f).GetAwaiter().GetResult());
            }
        }

        [Test]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void WarmupCommandTest([Values] GcbProcessingStatus responseStatus)
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateWarmUpResponse(0, responseStatus)));

            var service = MakeService();

            if (responseStatus != GcbProcessingStatus.OK)
            {
                Assert.Throws<Exception>(() => service.WarmUp(warmupSetpoint: 2000f).GetAwaiter().GetResult());
            }
            else
            {
                Assert.DoesNotThrow(() => service.WarmUp(warmupSetpoint: 2000f).GetAwaiter().GetResult());
            }
        }


        [TestCase(GcbProcessingStatus.OK)]
        [TestCase(GcbProcessingStatus.AccessError)]
        [TestCase(GcbProcessingStatus.OutOfBounds)]
        [SkipIfCi("Infrastructure test requiring GCB hardware connection")]
        public void OperationalPointQueryCommandTest(GcbProcessingStatus responseStatus)
        {
            GcbOperationalPoint point = MakeOperationalPoint();
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateOperationalPointQueryResponse(0, responseStatus, point)));

            var service = MakeService();

            if (responseStatus != GcbProcessingStatus.OK)
            {
                Assert.Throws<Exception>(() => service.QueryPoint().GetAwaiter().GetResult());
            }
            else
            {
                GcbOperationalPoint? receivedPoint = null;
                Assert.DoesNotThrow(() => receivedPoint = service.QueryPoint().GetAwaiter().GetResult());
                Assert.That(receivedPoint, Is.Not.Null);
                Assert.That(receivedPoint?.Equals(point), Is.True);
            }
        }

        [Test]
        public async Task QcbPingCommand_ParsesFirmwareResponse()
        {
            fakeCommunicationService.Setup(service => service.SendRequestAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(GcbXRayCmdResponseGenerator.GenerateQcbResponse(
                    0,
                    GCBPacketType.QcbPingResponse,
                    1, 0, 0, 0, 0));

            Assert.That(await MakeService(useFakeCommandOperator: false).PingQcb(), Is.True);
        }

        [Test]
        public async Task QcbStartCommand_SendsStartAndReservedZero()
        {
            byte[]? request = null;
            fakeCommunicationService.Setup(service => service.SendRequestAsync(It.IsAny<byte[]>()))
                .Callback<byte[]>(value => request = value)
                .ReturnsAsync(GcbXRayCmdResponseGenerator.GenerateQcbResponse(
                    0,
                    GCBPacketType.QcbReadingsCommandResponse,
                    0u, 0u, 0u, 0u, (uint)QcSessionStatus.Armed));

            await MakeService(useFakeCommandOperator: false).StartQcbReadings();

            var packet = new UdpPacket(request!);
            Assert.Multiple(() =>
            {
                Assert.That(packet.PacketType, Is.EqualTo((uint)GCBPacketType.QcbReadingsCommand));
                Assert.That((uint)packet[0], Is.EqualTo(1u));
                Assert.That((uint)packet[1], Is.Zero);
            });
        }

        [Test]
        public async Task QcbStopCommand_ReturnsFiveUnsignedFirmwareWords()
        {
            const uint channel0 = 0xF0000001u;
            const uint channel1 = 23u;
            const uint channel0Samples = 9u;
            const uint channel1Samples = 10u;
            fakeCommunicationService
                .Setup(service => service.SendRequestAsync(It.IsAny<byte[]>(), It.IsAny<int>()))
                .ReturnsAsync(GcbXRayCmdResponseGenerator.GenerateQcbReadingsResponse(
                    0,
                    channel0,
                    channel1,
                    channel0Samples,
                    channel1Samples,
                    (uint)QcSessionStatus.Complete));

            QcReadings readings = await MakeService(useFakeCommandOperator: false).StopQcbReadings();

            Assert.Multiple(() =>
            {
                Assert.That(readings.Channel0Accumulation, Is.EqualTo(channel0));
                Assert.That(readings.Channel1Accumulation, Is.EqualTo(channel1));
                Assert.That(readings.Channel0SampleCount, Is.EqualTo(channel0Samples));
                Assert.That(readings.Channel1SampleCount, Is.EqualTo(channel1Samples));
            });
        }

        [Test]
        public async Task QcbStopCommand_PollsPendingStatusesUntilComplete()
        {
            byte[] Response(QcSessionStatus status) =>
                GcbXRayCmdResponseGenerator.GenerateQcbReadingsResponse(
                    0, 101u, 202u, 3u, 4u, (uint)status);
            fakeCommunicationService
                .SetupSequence(service => service.SendRequestAsync(It.IsAny<byte[]>(), It.IsAny<int>()))
                .ReturnsAsync(Response(QcSessionStatus.Starting))
                .ReturnsAsync(Response(QcSessionStatus.Accumulating))
                .ReturnsAsync(Response(QcSessionStatus.Stopping))
                .ReturnsAsync(Response(QcSessionStatus.Complete));

            QcReadings readings = await MakeService(useFakeCommandOperator: false).StopQcbReadings();

            Assert.Multiple(() =>
            {
                Assert.That(readings.Accumulations, Is.EqualTo(new uint[] { 101u, 202u }));
                Assert.That(readings.Channel0SampleCount, Is.EqualTo(3u));
                Assert.That(readings.Channel1SampleCount, Is.EqualTo(4u));
            });
            fakeCommunicationService.Verify(
                service => service.SendRequestAsync(It.IsAny<byte[]>(), It.IsAny<int>()),
                Times.Exactly(4));
        }

        [Test]
        public void QcbStopCommand_ErrorStatusFailsImmediately()
        {
            fakeCommunicationService
                .Setup(service => service.SendRequestAsync(It.IsAny<byte[]>(), It.IsAny<int>()))
                .ReturnsAsync(GcbXRayCmdResponseGenerator.GenerateQcbReadingsResponse(
                    0, 0u, 0u, 0u, 0u, (uint)QcSessionStatus.Error));

            Assert.ThrowsAsync<QcAcquisitionException>(
                async () => await MakeService(useFakeCommandOperator: false).StopQcbReadings());
            fakeCommunicationService.Verify(
                service => service.SendRequestAsync(It.IsAny<byte[]>(), It.IsAny<int>()),
                Times.Once);
        }

        [Test]
        public void QcbStopCommand_UsesOneTwoSecondDeadline()
        {
            fakeCommunicationService
                .Setup(service => service.SendRequestAsync(It.IsAny<byte[]>(), It.IsAny<int>()))
                .ReturnsAsync(GcbXRayCmdResponseGenerator.GenerateQcbReadingsResponse(
                    0, 0u, 0u, 0u, 0u, (uint)QcSessionStatus.Stopping));
            var elapsed = System.Diagnostics.Stopwatch.StartNew();

            Assert.ThrowsAsync<TimeoutException>(
                async () => await MakeService(useFakeCommandOperator: false).StopQcbReadings());

            elapsed.Stop();
            Assert.That(elapsed.Elapsed, Is.InRange(TimeSpan.FromMilliseconds(1900), TimeSpan.FromMilliseconds(2600)));
            fakeCommunicationService.Verify(
                service => service.SendRequestAsync(
                    It.IsAny<byte[]>(),
                    It.Is<int>(timeout => timeout > 0 && timeout <= 2000)),
                Times.AtLeast(2));
        }


        [Test]
        public void ParseStatusResponse_PositiveTest([Values] GcbProcessingStatus status)
        {
            var packetType = GCBPacketType.DirectiveCmdResponse;
            byte[] responsePacket = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)packetType,
                packetCounter: 1,
                payload: [(int)status /*Processing status*/]);

            UdpPacket? parsedPacket = null;
            Assert.DoesNotThrow(() => parsedPacket = GcbCommandInterface.ParseAndValidateResponseData(responsePacket, packetType, expectedPayloadLength: 1));
            Assert.That(parsedPacket, Is.Not.Null);
            Assert.That((int)parsedPacket[0], Is.EqualTo((int)status));
        }

        [Test]
        public void ParseStatusResponse_WrongPacketTypeTest()
        {
            var actualPacketType = GCBPacketType.DirectiveCmdResponse;
            var expectedPacketType = GCBPacketType.FaultInfoResponse;
            byte[] responsePacket = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)actualPacketType,
                packetCounter: 1,
                payload: [(int)GcbProcessingStatus.InvalidValue /*Processing status*/]);

            Assert.Throws<Exception>(() => GcbCommandInterface.ParseAndValidateResponseData(responsePacket, expectedPacketType, expectedPayloadLength: 1));
        }

        [Test]
        public void ParseStatusResponse_WrongPayloadSizeTest()
        {
            byte[] responsePacket = UdpPacketBuilder.BuildRawPacket(
                packetType: 0,
                packetCounter: 1,
                payload: [0]);

            Assert.Throws<Exception>(() => GcbCommandInterface.ParseAndValidateResponseData(responsePacket, 0, expectedPayloadLength: 2));
        }        
    }
}
