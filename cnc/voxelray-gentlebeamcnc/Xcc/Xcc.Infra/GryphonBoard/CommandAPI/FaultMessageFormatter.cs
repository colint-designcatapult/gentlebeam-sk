using System;
using System.Globalization;
using System.Text;

namespace Xcc.Infra.GryphonBoard.CommandAPI
{
    public static class FaultMessageFormatter
    {
        public static string Format(string format, ReadOnlySpan<uint> arguments)
        {
            ArgumentNullException.ThrowIfNull(format);

            var message = new StringBuilder(format.Length);
            int argumentIndex = 0;

            for (int formatIndex = 0; formatIndex < format.Length; ++formatIndex)
            {
                char current = format[formatIndex];
                if (current != '%')
                {
                    message.Append(current);
                    continue;
                }

                if (++formatIndex == format.Length)
                    throw new FormatException("Fault format ends with an incomplete specifier.");

                char specifier = format[formatIndex];
                if (specifier == '%')
                {
                    message.Append('%');
                    continue;
                }

                if (argumentIndex >= arguments.Length)
                    throw new FormatException("Fault format consumes more arguments than were supplied.");

                uint rawArgument = arguments[argumentIndex++];
                switch (specifier)
                {
                    case 'd':
                        message.Append(unchecked((int)rawArgument).ToString(CultureInfo.InvariantCulture));
                        break;
                    case 'u':
                        message.Append(rawArgument.ToString(CultureInfo.InvariantCulture));
                        break;
                    case 'x':
                        message.Append(rawArgument.ToString("x", CultureInfo.InvariantCulture));
                        break;
                    case 'X':
                        message.Append(rawArgument.ToString("X", CultureInfo.InvariantCulture));
                        break;
                    case 'f':
                        message.Append(BitConverter.UInt32BitsToSingle(rawArgument).ToString("G9", CultureInfo.InvariantCulture));
                        break;
                    default:
                        throw new FormatException($"Unsupported fault format specifier '%{specifier}'.");
                }
            }

            if (argumentIndex != arguments.Length)
                throw new FormatException("Fault format consumes fewer arguments than were supplied.");

            return message.ToString();
        }
    }
}
