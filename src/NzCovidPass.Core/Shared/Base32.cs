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
        /// <summary>
        /// Converts <paramref name="input" /> which encodes binary data as base-32 digits,
        /// to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="input">The base-32 encoded input.</param>
        /// <returns>The decoded output.</returns>
        public static byte[] ToBytes(string input)
        {
            ArgumentNullException.ThrowIfNull(input);

            // Remove padding characters
            input = input.TrimEnd('=');

            var byteCount = input.Length * 5 / 8;
            var decodedOutput = new byte[byteCount];

            byte currentByte = 0, bitsRemaining = 8;
            int mask = 0, index = 0;

            foreach (var c in input)
            {
                var characterValue = CharToValue(c);

                if (bitsRemaining > 5)
                {
                    mask = characterValue << (bitsRemaining - 5);
                    currentByte = (byte) (currentByte | mask);
                    bitsRemaining -= 5;
                }
                else
                {
                    mask = characterValue >> (5 - bitsRemaining);
                    currentByte = (byte) (currentByte | mask);
                    decodedOutput[index++] = currentByte;
                    currentByte = (byte) (characterValue << (3 + bitsRemaining));
                    bitsRemaining += 3;
                }
            }

            // Didn't end with a full byte
            if (index != byteCount)
            {
                decodedOutput[index] = currentByte;
            }

            return decodedOutput;
        }

        private static int CharToValue(char c)
        {
            var value = (int) c;

            //65-90 == uppercase letters
            if (value < 91 && value > 64)
            {
                return value - 65;
            }
            //50-55 == numbers 2-7
            if (value < 56 && value > 49)
            {
                return value - 24;
            }
            //97-122 == lowercase letters
            if (value < 123 && value > 96)
            {
                return value - 97;
            }

            throw new ArgumentException("Character is not a Base32 character.", nameof(c));
        }
    }
}
