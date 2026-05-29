using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class WarmupParametersTests
    {
        [Test]
        public void WarmupParameters_Defaults()
        {
            var sut = new WarmupParameters();

            Assert.That(sut.WarmupType, Is.EqualTo((WarmupType)0));
            Assert.That(sut.HeaterCurrentSetpoint, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.ActiveHeadId, Is.EqualTo(0));
        }
        
        [Test]
        public void Create_Conditioning()
        {
            float current = 1.5f;
            long headId = 123;
            
            var sut = WarmupParameters.Conditioning(current, headId);

            Assert.That(sut.WarmupType, Is.EqualTo(WarmupType.Full));
            Assert.That(sut.HeaterCurrentSetpoint, Is.EqualTo(current).Within(G.Precision));
            Assert.That(sut.ActiveHeadId, Is.EqualTo(headId));
        }
        
        [Test]
        public void Create_FastWarmup()
        {
            float current = 2.0f;
            long headId = 456;
            
            var sut = WarmupParameters.FastWarmup(current, headId);

            Assert.That(sut.WarmupType, Is.EqualTo(WarmupType.Fast));
            Assert.That(sut.HeaterCurrentSetpoint, Is.EqualTo(current).Within(G.Precision));
            Assert.That(sut.ActiveHeadId, Is.EqualTo(headId));
        }
        
        [Test]
        public void Create_Conditioning_DefaultHeadId()
        {
            float current = 1.5f;
            
            var sut = WarmupParameters.Conditioning(current);

            Assert.That(sut.WarmupType, Is.EqualTo(WarmupType.Full));
            Assert.That(sut.HeaterCurrentSetpoint, Is.EqualTo(current).Within(G.Precision));
            Assert.That(sut.ActiveHeadId, Is.EqualTo(0));
        }
        
        [Test]
        public void Create_FastWarmup_DefaultHeadId()
        {
            float current = 2.0f;
            
            var sut = WarmupParameters.FastWarmup(current);

            Assert.That(sut.WarmupType, Is.EqualTo(WarmupType.Fast));
            Assert.That(sut.HeaterCurrentSetpoint, Is.EqualTo(current).Within(G.Precision));
            Assert.That(sut.ActiveHeadId, Is.EqualTo(0));
        }
    }
}