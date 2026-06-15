using Com.Empyreanmed.Heracles.CoilConfigurations.V1;
using Com.Empyreanmed.Heracles.CorrectionMatrix.V1;
using Com.Empyreanmed.Heracles.HeaterCurrentConfigs.V1;
using Com.Empyreanmed.Heracles.OutputFactors.V1;
using Com.Empyreanmed.Heracles.ReferenceFields.V1;
using Com.Empyreanmed.Heracles.Warmups.V1;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Core.Models;
using Heracles.Core.Models.RDBMS;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Models.RDBMS;

namespace Heracles.Application.Test.Protos
{
    [TestFixture(typeof(ICollimator), typeof(Com.Empyreanmed.Heracles.Collimators.V1.Collimator))]
    [TestFixture(typeof(IOutputFactor), typeof(OutputFactor))]
    [TestFixture(typeof(IWarmUp), typeof(Warmup))]
    [TestFixture(typeof(IPresetConfiguration), typeof(Com.Empyreanmed.Heracles.PresetConfigurations.V1.PresetConfiguration))]
    [TestFixture(typeof(ICorrectionMatrixEntry), typeof(CorrectionMatrix))]
    [TestFixture(typeof(IHeaterCurrentConfig), typeof(HeaterCurrentConfig))]
    [TestFixture(typeof(IReferenceFieldEntry), typeof(ReferenceField))]
    [TestFixture(typeof(ICoilConfigurationEntry), typeof(CoilConfiguration))]
    internal class SystemProtoTypeConverter_NullReferenceTests<XccType, GrpcType>
    where XccType : class
    where GrpcType : class
    {
        ProtoTypesConverterInvoker<XccType> toProtoInvoker = new();
        ProtoTypesConverterInvoker<GrpcType> fromProtoInvoker = new();
        [Test]
        public void ToProto_NullReferenceTest()
        {
            Assert.Throws<ArgumentNullException>(() => toProtoInvoker.ToProto(null!));
        }
        [Test]
        public void FromProto_NullReferenceTest()
        {
            Assert.Throws<ArgumentNullException>(() => fromProtoInvoker.FromProto(null!));
        }

    }

    internal class SystemTypesProtoTypeConverterTests
    {
    }
}
