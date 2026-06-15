using Empyrean.Common.Infra.Networking.Udp;
using Moq;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Infra.GryphonBoard.CommandAPI;

namespace Xcc.Test.Xcc.Infra.Services.XRayServices
{
    internal class XRayServiceTests
    {
        Mock<IGcbXRayCommandOperator> fakeCommandOperator = new();
        Mock<IGcbCommunicationService> fakeCommunicationService = new();
        Mock<ILogWriter> fakeLogService = new();
        IGcbXRayCommandOperator actualCommandOperator = new GcbXRayCommandOperator();

        private GcbCommandInterface MakeService(bool useFakeCommandOperator = true)
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

        private static GcbOperationalPoint MakeOperationalPoint(int index)
        {
            return new GcbOperationalPoint
            {
                PointIndex = index,
                TotalPointTime = 2.0f,
                RemainingPointTime = 1.0f,
                SetpointKv = 50.0f,
                FilamentSetpoint = 3500.0f,
                TargetMA = 2.0f,
                XCoilSetpoint = 0.1f,
                YCoilSetpoint = 0.2f,
                FocusCoilSetpoint = 2000.0f,
                AutoExecution = true
            };
        }

        private static FaultEntry MakeFaultEntry(GCBFaultBit faultType)
        {
            return new FaultEntry()
            {
                FaultId = (int)faultType,
                FaultType = faultType,
                FaultEntryState = (int)GcbStateNew.Warmup,
                FaultIdSupportingDetails = GCBFaultDetails.FilamentFaultSetpointOvercurrentError,
                FaultTimeValue = 0x1eadc0de,
                ExpectedParameter = 2500f,
                ExpectedParameterSupportingDetails = 1,
                ParameterTolerance = 2000f,
                MeasuredParameter = 3000f,
                MeasuredParameterSupportingDetails = 2
            };
        }

        [Test]
        public void GetVersionInfoTest()
        {
            VersionInfo versionInfo = new()
            {
                Major = 1,
                Minor = 2,
                Level = 3,
                FirmwareChecksum = 4,
                Mode = FirmwareMode.Demo
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
        public void SendOperationalPoint_PositiveTest([Values] OperationalPointCmdType commandType)
        {
            var fieldStatuses = Enumerable.Repeat(0, 11).ToList();
            var responseData =
                GcbXRayCmdResponseGenerator.GenerateOperationalPointResponse(0, commandType, fieldStatuses);
            
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(responseData));

            var service = MakeService();

            Assert.DoesNotThrow(
                () => service.SendOperationalPoint(
                    commandType, 
                    operationalPoint: MakeOperationalPoint(1), 
                    session: new GcbSession(id: 42, totalPoints: 1)
                    ).GetAwaiter().GetResult());
        }

        [Test]
        public void SendOperationalPoint_WrongPointStatusTest([Values] OperationalPointCmdType commandType)
        {
            var fieldStatuses = Enumerable.Repeat(0, 11).ToList();
            fieldStatuses[0] = (int)OperationalPointStatus.InvalidValue; // Make first point status invalud
            
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateOperationalPointResponse(0, commandType, fieldStatuses)));

            var service = MakeService();
            
            Assert.Throws<Exception>(
                () => service.SendOperationalPoint(
                    commandType,
                    operationalPoint: MakeOperationalPoint(1),
                    session: new GcbSession(id: 42, totalPoints: 1)
                    ).GetAwaiter().GetResult());
        }

        [Test]
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
                    operationalPoint: MakeOperationalPoint(1),
                    session: new GcbSession(id: 0, totalPoints: 0)
                    ).GetAwaiter().GetResult());
        }

        [Test]
        public void SendOperationalPoint_NullResponsePacketTest()
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>())).Returns(Task.FromResult((byte[])null!));

            var service = MakeService(useFakeCommandOperator: false);
            
            Assert.ThrowsAsync<ArgumentNullException>(
                () => service.SendOperationalPoint(
                    OperationalPointCmdType.Load,
                    operationalPoint: MakeOperationalPoint(1),
                    session: new GcbSession(id: 0, totalPoints: 0)
                ));
        }


        [Test]
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
        public void ReleasePlanTest(GcbProcessingStatus scopeStatus, GcbProcessingStatus authStatus, bool throwsException)
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateReleasePlanResponse(0, scopeStatus, authStatus)));

            var service = MakeService();

            var scope = GCBReleaseCommandScope.Plan;
            var session = new GcbSession(id: 42, totalPoints: 1);


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
        public void NewSessionCommandTest([Values] GcbProcessingStatus responseStatus)
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateNewSessionResponse(0, responseStatus, sessionId:42)));

            var service = MakeService();

            if (responseStatus != GcbProcessingStatus.OK)
            {
                Assert.Throws<Exception>(() => service.NewSession(totalPoints: 3).GetAwaiter().GetResult());
            }
            else
            {
                Assert.DoesNotThrow(() => service.NewSession(totalPoints: 3).GetAwaiter().GetResult());
            }
        }

        [Test]
        public void GetFaultsCommandTest()
        {
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateFaultInfoResponse(0, MakeFaultEntry(GCBFaultBit.FilamentFault))));

            var service = MakeService();

            FaultEntry? faultEntry = null;
            Assert.DoesNotThrow(() => faultEntry = service.GetFaults().GetAwaiter().GetResult());
        }

        [Test]
        public void GetFaultsCommand_NullResponseTest()
        {
            // Return null response
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult((byte[])null!));

            var service = MakeService(useFakeCommandOperator: false);

            Assert.ThrowsAsync<ArgumentNullException>(() => service.GetFaults());
        }

        [Test]
        public void GetFaultsCommand_EmptyResponseTest()
        {
            // Return null response
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(Array.Empty<byte>()));

            var service = MakeService();

            Assert.ThrowsAsync<ArgumentNullException>(() => service.GetFaults());
        }

        [Test]
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
        public void OperationalPointQueryCommandTest(GcbProcessingStatus responseStatus)
        {
            GcbOperationalPoint point = MakeOperationalPoint(1);
            fakeCommunicationService.Setup(cmd => cmd.SendRequestAsync(It.IsAny<byte[]>()))
                .Returns(Task.FromResult(GcbXRayCmdResponseGenerator.GenerateOperationalPointQueryResponse(0, responseStatus, point)));

            var service = MakeService();

            if (responseStatus != GcbProcessingStatus.OK)
            {
                Assert.Throws<Exception>(() => service.QueryPoint(1).GetAwaiter().GetResult());
            }
            else
            {
                GcbOperationalPoint? receivedPoint = null;
                Assert.DoesNotThrow(() => receivedPoint = service.QueryPoint(1).GetAwaiter().GetResult());
                Assert.That(receivedPoint, Is.Not.Null);
                Assert.That(receivedPoint?.Equals(point), Is.True);
            }
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
