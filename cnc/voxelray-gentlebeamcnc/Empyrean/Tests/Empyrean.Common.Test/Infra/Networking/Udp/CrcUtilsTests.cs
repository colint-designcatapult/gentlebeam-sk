using Empyrean.Common.Infra.Networking.Udp;

namespace Empyrean.Common.Test.Infra.Networking.Udp
{
    internal class CrcUtilsTests
    {
        [Test]
        public void GenerateTableTest()
        {
            uint[]? table = null;
            Assert.DoesNotThrow(() => table = CrcUtils.GenerateCrcTable());
            Assert.That(table, Is.Not.Null);
            Assert.That(table.Count, Is.EqualTo(256));
        }

        [Test]
        public void ComputeChecksum_PositiveTest()
        {
            byte[] data = new byte[42];
            Random rnd = new Random();
            rnd.NextBytes(data);

            uint[] table = CrcUtils.GenerateCrcTable();
            uint checksum1 = CrcUtils.ComputeChecksum(data, table);
            uint checksum2 = CrcUtils.ComputeChecksum(data, table);
            Assert.That(checksum1, Is.EqualTo(checksum2));
        }


        [Test]
        public void ComputeChecksum_EmptyInputTest()
        {
            byte[] data = Array.Empty<byte>();

            uint[] table = CrcUtils.GenerateCrcTable();
            uint checksum = 1;
            Assert.DoesNotThrow(() => checksum = CrcUtils.ComputeChecksum(data, table));
            Assert.That(checksum, Is.EqualTo(0));
        }

        [Test]
        public void ComputeChecksum_NullInputTest()
        {
            byte[] data = new byte[4];
            uint[] table = CrcUtils.GenerateCrcTable();
            Assert.Throws<NullReferenceException>(() => CrcUtils.ComputeChecksum(null!, table));
            Assert.Throws<NullReferenceException>(() => CrcUtils.ComputeChecksum(data, null!));
        }

        [Test]
        public void ComputeChecksum_AlteredDataTest()
        {
            byte[] data = new byte[42];
            Random rnd = new Random();
            rnd.NextBytes(data);

            uint[] table = CrcUtils.GenerateCrcTable();
            
            uint checksum1 = CrcUtils.ComputeChecksum(data, table);

            // Prepare altered data:
            byte[] data2 = new byte[42];
            data.CopyTo(data2, 0);
            data2[0] ^= 0x01; // invert a first bit

            uint checksum2 = CrcUtils.ComputeChecksum(data2, table);
            Assert.That(checksum1, Is.Not.EqualTo(checksum2));
        }

        [Test]
        public void ComputeChecksum_InnerTableTest()
        {
            byte[] data = new byte[42];
            Random rnd = new Random();
            rnd.NextBytes(data);

            uint[] table = CrcUtils.GenerateCrcTable();
            uint checksum1 = CrcUtils.ComputeChecksum(data, table);
            uint checksum2 = CrcUtils.ComputeChecksum(data);
            Assert.That(checksum1, Is.EqualTo(checksum2));
        }

        [Test]
        public void GetCRCTest()
        {
            byte[] data = new byte[42];
            Random rnd = new Random();
            rnd.NextBytes(data);

            byte[] crc = CrcUtils.GetCrc(data);
            Assert.That(crc, Is.Not.Null);
            Assert.That(crc.Count, Is.EqualTo(4));
        }
    }
}
