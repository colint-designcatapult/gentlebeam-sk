using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xcc.Core.Domain.QualityCheck;

namespace Xcc.Test.Xcc.Core.Domain.QualityCheck
{
    public class QcReadingsTests
    {
        [Test]
        public void Constructor_ExposesImmutableTwoChannelContract()
        {
            var sut = new QcReadings(11u, 22u, 33u, 44u);

            Assert.Multiple(() =>
            {
                Assert.That(sut.Channel0Accumulation, Is.EqualTo(11u));
                Assert.That(sut.Channel1Accumulation, Is.EqualTo(22u));
                Assert.That(sut.Channel0SampleCount, Is.EqualTo(33u));
                Assert.That(sut.Channel1SampleCount, Is.EqualTo(44u));
                Assert.That(sut.Accumulations, Is.EqualTo(new uint[] { 11u, 22u }));
            });

            var mutableView = (IList<uint>)sut.Accumulations;
            Assert.Throws<NotSupportedException>(() => mutableView[0] = 99u);
            Assert.That(sut.Channel0Accumulation, Is.EqualTo(11u));
        }
    }
}
