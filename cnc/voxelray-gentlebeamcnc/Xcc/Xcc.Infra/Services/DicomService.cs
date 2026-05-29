using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.IO.Buffer;
using System.Threading.Tasks;
using static FellowOakDicom.DicomTag;
using static FellowOakDicom.Imaging.PhotometricInterpretation;
using static FellowOakDicom.Imaging.PixelRepresentation;

namespace Xcc.Infra.Services
{
    public class DicomService
    {
        public static async Task WriteDX(byte[] rawBytes, string Filename, ushort SizeX, ushort SizeY, double PitchX,
            double PitchY)
        {
            var ds = new DicomDataset
            {
                { SOPInstanceUID, DicomUIDGenerator.GenerateDerivedFromUUID() },
                { SOPClassUID, DicomUID.DigitalXRayImageStorageForPresentation },
                { Modality, "DX" },
                { PixelSpacing, new double[] {PitchY, PitchX} },
                { BitsAllocated, (ushort)16 },
                new DicomOtherWord(PixelData, new CompositeByteBuffer()),
            };

            DicomPixelData pd = DicomPixelData.Create(ds, true);
            pd.Width = SizeX;
            pd.Height = SizeY;

            pd.BitsStored = 15;
            pd.HighBit = 14;
            pd.SamplesPerPixel = 1;
            pd.PixelRepresentation = Unsigned;
            pd.PhotometricInterpretation = Monochrome2;

            pd.AddFrame(new MemoryByteBuffer(rawBytes));

            var df = new DicomFile(ds);

            await df.SaveAsync(Filename);
        }

        public static async Task WriteDX(string Filename, ushort SizeX, ushort SizeY, double PitchX, double PitchY)
        {
            var ds = new DicomDataset
            {
                { SOPInstanceUID, DicomUIDGenerator.GenerateDerivedFromUUID() },
                { SOPClassUID, DicomUID.DigitalXRayImageStorageForPresentation },
                { Modality, "DX" },
                { PixelSpacing, new double[] {PitchY, PitchX} },
                { BitsAllocated, (ushort)16 },
                new DicomOtherWord(PixelData, new CompositeByteBuffer()),
            };

            DicomPixelData pd = DicomPixelData.Create(ds, true);
            pd.Width = SizeX;
            pd.Height = SizeY;

            pd.BitsStored = 15;
            pd.HighBit = 14;
            pd.SamplesPerPixel = 1;
            pd.PixelRepresentation = Unsigned;
            pd.PhotometricInterpretation = Monochrome2;

            var sdata = new byte [SizeX * SizeY];
            pd.AddFrame(new MemoryByteBuffer(sdata));

            var df = new DicomFile(ds);

            await df.SaveAsync(Filename);
        }
    }
}
