using FellowOakDicom;

using Heracles.Core.Models.EMR;

using System;
using System.IO;
using System.Linq;

using Xcc.Core.Enums;

namespace Heracles.Indoor.ViewModels
{
    public class RawVolume
    {
        public int SizeX = 0;
        public int SizeY = 0;
        public int SizeZ = 0;

        public float PitchX = 1.0f;
        public float PitchY = 1.0f;
        public float PitchZ = 1.0f;

        public float[] data = null;

        public RawVolume() { }

        public RawVolume(int sx, int sy, int sz)
        {
            SizeX = sx;
            SizeY = sy;
            SizeZ = sz;

            data = new float[sx * sy * sz];
        }

        public RawVolume(string folder)
        {
            var files = Directory.EnumerateFiles(folder, "*.raw", SearchOption.TopDirectoryOnly);

            if (files.Any() == false)
                return;

            SizeZ = files.Count();

            SizeX = 231;
            SizeY = 206;

            PitchX = 0.01f;
            PitchY = 0.01f;
            PitchZ = 0.003f;

            data = new float[SizeX * SizeY * SizeZ];

            var sd = new ushort[SizeX * SizeY];

            for (int z = 0; z < SizeZ; z++)
            {
                var fn = files.ElementAt(z);
                var b = File.ReadAllBytes(fn);

                Buffer.BlockCopy(b, 0, sd, 0, SizeX * SizeY * 2);

                int fi = z * SizeX * SizeY;

                for (int i = 0; i < SizeX * SizeY; i++)
                {
                    data[fi + i] = (float)sd[i] / (float)ushort.MaxValue;
                }
            }

        }

        static public void ToDICOMFolder(RawVolume V, string folder, IPatient P, IDiagnosis D)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string ROOT_UID = "1.2.840.10008";
            var uid = new DicomUID(ROOT_UID, "Image Storage", DicomUidType.SOPClass);

            var NOW = DateTime.Now;

            var STUDY_UID = ROOT_UID + "." + new DicomUIDGenerator().Generate(uid).UID;
            var SERIES_UID = ROOT_UID + "." + new DicomUIDGenerator().Generate(uid).UID;

            var ROOT_INSTANCE_UID = ROOT_UID + "." + new DicomUIDGenerator().Generate(uid).UID;

            for (int z = 0; z < V.SizeZ; z++)
            {

                var n = string.Format("{0:0000}.dcm", z);
                var file_name = Path.Combine(folder, n);

                var df = new DicomFile();

                var CTIS = DicomUID.CTImageStorage;
                var PAIS = DicomUID.PhotoacousticImageStorage;

                df.FileMetaInfo.Add(DicomTag.TransferSyntaxUID, DicomTransferSyntax.ExplicitVRLittleEndian);
                df.FileMetaInfo.Add(DicomTag.MediaStorageSOPClassUID, CTIS);

                var ds = df.Dataset;

                ds.Add(DicomTag.StudyInstanceUID, STUDY_UID);
                ds.Add(DicomTag.SeriesInstanceUID, SERIES_UID);

                string SUID = string.Format("{0}.{1}", ROOT_INSTANCE_UID, z + 1);
                ds.Add(DicomTag.SOPInstanceUID, SUID);

                ds.Add(DicomTag.SOPClassUID, CTIS);
                ds.Add(DicomTag.Modality, "CT");

                //ds.Add(DicomTag.SOPClassUID, PAIS);
                //ds.Add(DicomTag.Modality, "PA");

                ds.Add(DicomTag.SpecificCharacterSet, "");
                ds.Add(DicomTag.ImageType, "ORIGINAL\\PRIMARY");

                ds.Add(DicomTag.StudyDate, NOW);
                ds.Add(DicomTag.SeriesDate, NOW);
                ds.Add(DicomTag.AcquisitionDate, NOW);
                ds.Add(DicomTag.ContentDate, NOW);

                ds.Add(DicomTag.StudyTime, NOW);
                ds.Add(DicomTag.SeriesTime, NOW);
                ds.Add(DicomTag.AcquisitionTime, NOW);
                ds.Add(DicomTag.ContentTime, NOW);

                ds.Add(DicomTag.AccessionNumber, "1");

                ds.Add(DicomTag.Manufacturer, "EMPYREAN MEDICAL SYSTEMS");
                ds.Add(DicomTag.InstitutionName, "EMPYREAN MEDICAL SYSTEMS");

                ds.Add(DicomTag.ReferringPhysicianName, D.Referring);
                ds.Add(DicomTag.StationName, "StationName");


                ds.Add(DicomTag.AcquisitionDateTime, NOW);

                ds.Add(DicomTag.SamplesPerPixel, (ushort)1);
                ds.Add(DicomTag.PhotometricInterpretation, "MONOCHROME2");

                ds.Add(DicomTag.BitsAllocated, (ushort)16);
                ds.Add(DicomTag.BitsStored, (ushort)16);
                ds.Add(DicomTag.HighBit, (ushort)15);

                ds.Add(DicomTag.PixelRepresentation, (ushort)0);

                ds.Add(DicomTag.PixelSpacing, new float[] { V.PitchY, V.PitchX });
                ds.Add(DicomTag.SliceThickness, V.PitchZ);
                ds.Add(DicomTag.SpacingBetweenSlices, V.PitchZ);


                ds.Add(DicomTag.PixelAspectRatio, new int[] { 1, 1 });

                ds.Add(DicomTag.Rows, (ushort)V.SizeY);
                ds.Add(DicomTag.Columns, (ushort)V.SizeX);

                //PATIENT ++

                ds.Add(DicomTag.PatientName, P.LastName + "^" + P.FirstName);

                var S = "O";
                if (P.Sex == Sex.Male) S = "M";
                if (P.Sex == Sex.Female) S = "F";
                ds.Add(DicomTag.PatientSex, S);


                var BD = P.DOB.Value.ToString("yyyyMMdd");
                ds.Add(DicomTag.PatientBirthDate, BD);

                ds.Add(DicomTag.PatientID, P.PatientId.ToString());

                var DT = DateOnly.FromDateTime(NOW);

                if (P.DOB != null)
                {
                    var age = DT.Year - P.DOB.Value.Year;
                    bool C = P.DOB.Value > DT.AddYears(-age);

                    if (C)
                    {
                        age--;
                    }

                    var sage = string.Format("{0:0##}Y", age);
                    ds.Add(DicomTag.PatientAge, sage);
                }
                //PATIENT --

                // STUDY ++
                ds.Add(DicomTag.StudyDescription, D.SiteLocation.ToString());
                ds.Add(DicomTag.StudyID, D.Id.ToString());

                // STUDY --


                var sd = new ushort[V.SizeX * V.SizeY];

                int fi = z * V.SizeX * V.SizeY;

                for (int i = 0; i < V.SizeX * V.SizeY; i++)
                {
                    sd[i] = (ushort)(V.data[fi + i] * ushort.MaxValue);
                }

                ds.AddOrUpdate(DicomVR.OW, DicomTag.PixelData, sd);

                ds.AddOrUpdate(DicomTag.InstanceNumber, z);

                //                ds.Add(DicomTag.PatientOrientation, "P", "R");
                ds.Add(DicomTag.ImagePositionPatient, 0.0f, 0.0f, z * V.PitchZ);
                ds.Add(DicomTag.ImageOrientationPatient, 1.0, 0f, 0f, 0f, 1.0f, 0f);
                ds.Add(DicomTag.SliceLocation, z * V.PitchZ);

                ds.Add(DicomTag.FrameOfReferenceUID, "1.1.1.1");

                ds.Add(DicomTag.RescaleSlope, "1");
                ds.Add(DicomTag.RescaleIntercept, "0");


                df.Save(file_name);
            }
        }

    }

}
