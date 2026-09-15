using System.Buffers.Binary;
using System.Globalization;

namespace Heracles.Indoor.SqliteGrpcServer.Infrastructure;

public static class RecoveryKeyCodec
{
    public static string Format(ReadOnlySpan<byte> mek)
    {
        if (mek.Length != 16)
            throw new ArgumentException("A recovery key requires exactly 16 bytes.", nameof(mek));

        Span<char> result = stackalloc char[55];
        for (var group = 0; group < 8; group++)
        {
            var value = BinaryPrimitives.ReadUInt16BigEndian(mek.Slice(group * 2, 2)) * 11;
            value.TryFormat(result.Slice(group * 7, 6), out _, "D6", CultureInfo.InvariantCulture);
            if (group != 7)
                result[group * 7 + 6] = '-';
        }
        return new string(result);
    }

    public static bool TryParse(string text, out byte[] mek)
    {
        mek = Array.Empty<byte>();
        if (text is null)
            return false;
        var input = text.AsSpan().Trim();
        if (input.Length != 55)
            return false;

        Span<byte> decoded = stackalloc byte[16];
        try
        {
            for (var group = 0; group < 8; group++)
            {
                var value = 0;
                for (var digit = 0; digit < 6; digit++)
                {
                    var character = input[group * 7 + digit];
                    if (character is < '0' or > '9')
                        return false;
                    value = value * 10 + character - '0';
                }
                if (value > 720885 || value % 11 != 0 || (group != 7 && input[group * 7 + 6] != '-'))
                    return false;
                BinaryPrimitives.WriteUInt16BigEndian(decoded.Slice(group * 2, 2), (ushort)(value / 11));
            }
            mek = decoded.ToArray();
            return true;
        }
        finally
        {
            System.Security.Cryptography.CryptographicOperations.ZeroMemory(decoded);
        }
    }
}
