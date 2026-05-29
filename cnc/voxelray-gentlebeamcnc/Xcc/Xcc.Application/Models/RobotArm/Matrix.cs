using Xcc.Core.Helpers;

namespace Xcc.Application.Models.RobotArm
{
    public class MovementMatrix : Matrix
    {
        public MovementMatrix() : base(4, 4) 
        {
            mat = IdentityMatrix(rows, cols).mat;
        }
        public MovementMatrix(MovementMatrix other) : base(4, 4)
        {
            mat = IdentityMatrix(rows, cols).mat;
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    this[i, j] = other[i, j];
                }
            }
        }
        public override string ToString()
        {
            return $"{this[0, 0]:0.000}, {this[0, 1]:0.000}, {this[0, 2]:0.000}, {this[0, 3]:0.000}; {this[1, 0]:0.000}, {this[1, 1]:0.000}, {this[1, 2]:0.000}, {this[1, 3]:0.000}; {this[2, 0]:0.000}, {this[2, 1]:0.000}, {this[2, 2]:0.000}, {this[2, 3]:0.000}; {this[3, 0]:0.000}, {this[3, 1]:0.000}, {this[3, 2]:0.000}, {this[3, 3]:0.000}";
        }
    }
    public struct EulerAngles
    {
        public double RotX;
        public double RotY;
        public double RotZ;
    }
}
