using System;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.System
{
    public struct MagnetometerCorrectionContext(MagnetometerConfiguration magnetometer, double maxMagnetometerCorrectionDelta)
    {
        public readonly MagnetometerConfiguration Magnetometer => magnetometer;
        public readonly double MaxMagnetometerCorrectionDelta => maxMagnetometerCorrectionDelta;

        public DeflectionCurrentCorrection CalculateCorrection(MagnetometerValues magnetometerValues)
        {
            var frontalCorrection = Magnetometer.CalculateCorrection(MagnetometerType.Front, magnetometerValues.Front);
            var backCorrection = Magnetometer.CalculateCorrection(MagnetometerType.Back, magnetometerValues.Back);

            if (DeflectionCurrentCorrection.MaxDelta(frontalCorrection, backCorrection) > MaxMagnetometerCorrectionDelta)
            {
                throw new MagnetometerCorrectionException($"Magnetometer correction delta exceeds the limit");
            }
            else
            {
                return frontalCorrection;
            }
        }
    }

    public class MagnetometerCorrectionException : Exception
    {
        public MagnetometerCorrectionException()
        {
        }

        public MagnetometerCorrectionException(string? message) : base(message)
        {
        }
    }
}
