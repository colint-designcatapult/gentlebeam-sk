using System;

namespace Xcc.Core.Domain.GryphonBoard
{
    public struct DeflectionCurrentCorrection(double xCoil, double yCoil)
    {
        public double XCoil => xCoil;
        public double YCoil => yCoil;

        public DeflectionCurrentCorrection() : this(0.0, 0.0) { } 

        public static double MaxDelta(DeflectionCurrentCorrection frontalCorrection, DeflectionCurrentCorrection backCorrection)
        {
            return double.Max(
                double.Abs(frontalCorrection.XCoil - backCorrection.XCoil),
                double.Abs(frontalCorrection.YCoil - backCorrection.YCoil));
        }

        public double CorrectX(double deflectionCurrentX) => xCoil + deflectionCurrentX;
        public double CorrectY(double deflectionCurrentY) => yCoil + deflectionCurrentY;
    }
}
