namespace NzCovidPass.Core.Shared
{
    /// <summary>
    /// Helpers for base-32 encoded data.
    /// </summary>
    /// <remarks>
    /// Taken from <see href="https://stackoverflow.com/a/7135008" />
    /// </remarks>
    internal class Base32
    {
        private const string Symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private const int BitsPerSymbol = 5;
        private const int BitsPerByte = 8;
        private const int SymbolMask = 31; // 2^5 - 1

        /// <summary>
        /// Converts <paramref name="input" /> which encodes binary data as base-32 digits,
        /// to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="input">The base-32 encoded input.</param>
        /// <returns>The decoded output.</returns>
        public static byte[] ToBytes(string input)
        {
            ArgumentNullException.ThrowIfNull(input);

            if (input.Length == 0)
            {
                return Array.Empty<byte>();
            }

            var byteCount = input.Length * BitsPerSymbol / BitsPerByte;
            var decodedOutput = new byte[byteCount];

            byte bitsRemaining = 0;
            int buffer = 0, index = 0;

            foreach (var c in input)
            {
                // Ignore padding characters
                if (c == '=')
                {
                    continue;
                }

                var symbolIndex = Symbols.IndexOf(c);

                if (symbolIndex == -1)
                {
                    throw new FormatException($"'{c}' is not a valid base-32 character.");
                }

                var symbolByte = (byte) symbolIndex;

                buffer <<= BitsPerSymbol;
                buffer |= symbolByte & SymbolMask;
                bitsRemaining += BitsPerSymbol;

                if (bitsRemaining >= BitsPerByte)
                {
                    var b = (byte) (buffer >> (bitsRemaining - BitsPerByte));
                    decodedOutput[index++] = b;
                    bitsRemaining -= BitsPerByte;
                }
            }

            return decodedOutput;
        }
    }
}
