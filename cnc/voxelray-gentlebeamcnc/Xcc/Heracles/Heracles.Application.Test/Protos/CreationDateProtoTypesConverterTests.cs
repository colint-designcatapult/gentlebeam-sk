using Google.Protobuf.WellKnownTypes;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Models.QualityCheck;
using Heracles.Application.Protos;
using Heracles.Core.Enums;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Application.Models.RDBMS;
using Xcc.Core.Enums;

namespace Heracles.Application.Test.Protos
{
    internal class CreationDateProtoTypesConverterTests
    {
        private static readonly DateTime CreationDate =
            new(2026, 8, 28, 14, 30, 0, DateTimeKind.Local);

        [Test]
        public void SafetyCheckToProto_PreservesCreationDate()
        {
            var proto = ProtoTypesConverter.ToProto(new SafetyCheck
            {
                CreationDate = CreationDate,
                PerformedBy = "operator@example.com"
            });

            AssertCreationDate(proto.CreateDate);
        }

        [Test]
        public void WarmupToProto_PreservesCreationDate()
        {
            var proto = ProtoTypesConverter.ToProto(new WarmUp
            {
                CreationDate = CreationDate,
                Type = WarmupType.Fast
            });

            AssertCreationDate(proto.CreateDate);
        }

        [Test]
        public void QcSampleToProto_PreservesCreationDate()
        {
            var proto = ProtoTypesConverter.ToProto(new QcSampleHeader
            {
                CreationDate = CreationDate
            });

            AssertCreationDate(proto.CreateDate);
        }

        [Test]
        public void QcSampleFieldToProto_PreservesCreationDate()
        {
            var proto = ProtoTypesConverter.ToProto(new QcSampleField
            {
                CreationDate = CreationDate,
                Name = TreatmentFieldName.Plus0L1
            });

            AssertCreationDate(proto.CreateDate);
        }

        [Test]
        public void IntensityToProto_PreservesCreationDate()
        {
            var proto = ProtoTypesConverter.ToProto(new Intensity
            {
                CreationDate = CreationDate,
                DiodeName = "0"
            });

            AssertCreationDate(proto.CreateDate);
        }

        [Test]
        public void QcSampleRoundTrip_PreservesCreationDate()
        {
            var restored = ProtoTypesConverter.FromProto(
                ProtoTypesConverter.ToProto(new QcSampleHeader
                {
                    CreationDate = CreationDate
                }));

            Assert.That(restored.CreationDate, Is.EqualTo(CreationDate));
            Assert.That(restored.CreationDate.Kind, Is.EqualTo(DateTimeKind.Local));
        }

        [Test]
        public void QcSampleFieldRoundTrip_PreservesCreationDate()
        {
            var restored = ProtoTypesConverter.FromProto(
                ProtoTypesConverter.ToProto(new QcSampleField
                {
                    CreationDate = CreationDate,
                    Name = TreatmentFieldName.Plus0L1
                }));

            Assert.That(restored.CreationDate, Is.EqualTo(CreationDate));
            Assert.That(restored.CreationDate.Kind, Is.EqualTo(DateTimeKind.Local));
        }

        private static void AssertCreationDate(Timestamp timestamp)
        {
            Assert.That(timestamp, Is.Not.Null);
            Assert.That(timestamp.ToDateTime(), Is.EqualTo(CreationDate.ToUniversalTime()));
        }
    }
}
