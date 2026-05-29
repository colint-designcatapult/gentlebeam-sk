using System;
using System.Collections.Generic;
using System.Linq;

namespace Xcc.Application.Helpers
{
    public class ByteArrayUtils
    {
        public static byte[] JoinByteArrays(byte[] a, byte[] b)
        {
            byte[] result = new byte[a.Length + b.Length];

            Buffer.BlockCopy(a, 0, result, 0, a.Length);
            Buffer.BlockCopy(b, 0, result, a.Length, b.Length);

            return result;
        }

        public static byte[] JoinByteArrays(IEnumerable<byte[]> arrays)
        {
            int size = arrays.Select(a => a.Length).Sum();

            byte[] result = new byte[size];

            int pos = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, pos, array.Length);
                pos += array.Length;
            }

            return result;
        }
    }
}
