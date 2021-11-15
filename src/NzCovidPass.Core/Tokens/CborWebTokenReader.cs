using Dahomey.Cbor;
using Dahomey.Cbor.ObjectModel;
using Microsoft.Extensions.Logging;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Tokens
{
    /// <inheritdoc cref="ICborWebTokenReader" />
    public class CborWebTokenReader : ICborWebTokenReader
    {
        private readonly ILogger<CborWebTokenReader> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CborWebTokenReader" /> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger{TCategoryName}" /> instance used for writing log messages.</param>
        public CborWebTokenReader(ILogger<CborWebTokenReader> logger)
        {
            _logger = Requires.NotNull(logger);
        }

        /// <inheritdoc />
        public void ReadToken(CborWebTokenReaderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var base32Payload = AddBase32Padding(context.Payload);

            try
            {
                _logger.LogDebug("Decoding base-32 payload '{Payload}'", base32Payload);

                var decodedPayloadBytes = Base32.ToBytes(base32Payload);

                _logger.LogDebug("Decoded base-32 payload bytes (hex) '{Payload}'", Convert.ToHexString(decodedPayloadBytes));

                var decodedCborStructure = Cbor.Deserialize<CborArray>(decodedPayloadBytes);

                _logger.LogDebug("Decoded CBOR structure: {Structure}", decodedCborStructure);

                var rawHeaderBytes = decodedCborStructure[0].GetValueBytes();
                var rawPayloadBytes = decodedCborStructure[2].GetValueBytes();
                var rawSignatureBytes = decodedCborStructure[3].GetValueBytes();

                var header = Cbor.Deserialize<CborObject>(rawHeaderBytes.Span);
                var payload = Cbor.Deserialize<CborObject>(rawPayloadBytes.Span);

                var token = new CborWebToken(
                    new CborWebToken.Header(header, rawHeaderBytes),
                    new CborWebToken.Payload(payload, rawPayloadBytes),
                    new CborWebToken.Signature(rawSignatureBytes));

                context.Succeed(token);
            }
            catch (FormatException formatException)
            {
                _logger.LogError(formatException, "Failed to decode base-32 payload.");

                context.Fail(CborWebTokenReaderContext.InvalidBase32Payload);
            }
            catch (CborException cborException)
            {
                _logger.LogError(cborException, "Failed to decode CBOR structure");

                context.Fail(CborWebTokenReaderContext.FailedToDecodeCborStructure);
            }
        }

        private static string AddBase32Padding(string base32Payload)
        {
            var unpaddedLength = base32Payload.Length % 8;

            if (unpaddedLength != 0)
            {
                base32Payload += new string('=', count: 8 - unpaddedLength);
            }

            return base32Payload;
        }
    }
}
