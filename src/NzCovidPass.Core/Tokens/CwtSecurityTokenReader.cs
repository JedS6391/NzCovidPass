using Dahomey.Cbor;
using Dahomey.Cbor.ObjectModel;
using Microsoft.Extensions.Logging;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Tokens
{
    /// <inheritdoc cref="ICwtSecurityTokenReader" />
    public class CwtSecurityTokenReader : ICwtSecurityTokenReader
    {
        private readonly ILogger<CwtSecurityTokenReader> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CwtSecurityTokenReader" /> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger{TCategoryName}" /> instance used for writing log messages.</param>
        public CwtSecurityTokenReader(ILogger<CwtSecurityTokenReader> logger)
        {
            _logger = Requires.NotNull(logger);
        }

        /// <inheritdoc />
        public void ReadToken(CwtSecurityTokenReaderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var base32Payload = AddBase32Padding(context.Payload);

            try
            {
                _logger.LogDebug("Decoding base-32 payload '{Payload}'", base32Payload);

                var decodedPayloadBytes = Base32.ToBytes(base32Payload);

                _logger.LogDebug("Decoded base-32 payload bytes (hex) '{Payload}'", Convert.ToHexString(decodedPayloadBytes));

                var decodedCoseStructure = Cbor.Deserialize<CborArray>(decodedPayloadBytes);

                _logger.LogDebug("Decoded COSE structure: {Structure}", decodedCoseStructure);

                if (!IsValidCoseStructure(decodedCoseStructure))
                {
                    _logger.LogError("Payload is not a valid COSE_Sign1 structure.");

                    context.Fail(CwtSecurityTokenReaderContext.InvalidCoseStructure);

                    return;
                }

                var rawHeaderBytes = decodedCoseStructure[0].GetValueBytes();
                var rawPayloadBytes = decodedCoseStructure[2].GetValueBytes();
                var rawSignatureBytes = decodedCoseStructure[3].GetValueBytes();

                var header = Cbor.Deserialize<CborObject>(rawHeaderBytes.Span);
                var payload = Cbor.Deserialize<CborObject>(rawPayloadBytes.Span);

                var token = new CwtSecurityToken(
                    new CwtSecurityToken.Header(header, rawHeaderBytes),
                    new CwtSecurityToken.Payload(payload, rawPayloadBytes),
                    new CwtSecurityToken.Signature(rawSignatureBytes));

                context.Succeed(token);
            }
            catch (FormatException formatException)
            {
                _logger.LogError(formatException, "Failed to decode base-32 payload.");

                context.Fail(CwtSecurityTokenReaderContext.InvalidBase32Payload);
            }
            catch (CborException cborException)
            {
                _logger.LogError(cborException, "Failed to decode CBOR structure");

                context.Fail(CwtSecurityTokenReaderContext.FailedToDecodeCborStructure);
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

        private bool IsValidCoseStructure(CborArray coseStructure)
        {
            // https://datatracker.ietf.org/doc/html/rfc8152
            // Note this process assumes a COSE_Sign1 structure, which NZ Covid passes should be.

            // We expect a structure in the form [ protected headers, unprotected headers, payload, signature ]
            if (coseStructure.Count != 4)
            {
                _logger.LogError("COSE structure does not have the expected form '[ protected headers, unprotected headers, payload, signature ]'");

                return false;
            }

            // We expect the structure to have the following types of CBOR values [ bytestring, map, bytestring, bytestring ]
            if (coseStructure[0].Type != CborValueType.ByteString ||
                coseStructure[1].Type != CborValueType.Object ||
                coseStructure[2].Type != CborValueType.ByteString ||
                coseStructure[3].Type != CborValueType.ByteString)
            {
                _logger.LogError("COSE structure does not have the expected CBOR types '[ bytestring, map, bytestring, bytestring ]'");

                return false;
            }

            return true;
        }
    }
}
