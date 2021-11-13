using Dahomey.Cbor;
using Dahomey.Cbor.ObjectModel;
using Microsoft.Extensions.Logging;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Cbor
{
    public class CborWebTokenReader : ICborWebTokenReader
    {
        private readonly ILogger<CborWebTokenReader> _logger;

        public CborWebTokenReader(ILogger<CborWebTokenReader> logger)
        {
            _logger = Requires.NotNull(logger);
        }

        public bool TryReadToken(string base32Payload, out CborWebToken? token)
        {
            ArgumentNullException.ThrowIfNull(base32Payload);

            base32Payload = AddBase32Padding(base32Payload);

            _logger.LogDebug("Decoding base-32 payload '{Payload}'", base32Payload);

            var decodedPayloadBytes = Base32.ToBytes(base32Payload);

            _logger.LogDebug("Decoded base-32 payload bytes (hex) '{Payload}'", Convert.ToHexString(decodedPayloadBytes));

            try
            {
                var decodedCborStructure = Dahomey.Cbor.Cbor.Deserialize<CborArray>(decodedPayloadBytes);

                _logger.LogDebug("Decoded CBOR structure: {Structure}", decodedCborStructure);

                var rawHeaderBytes = decodedCborStructure[0].GetValueBytes();
                var rawPayloadBytes = decodedCborStructure[2].GetValueBytes();
                var rawSignatureBytes = decodedCborStructure[3].GetValueBytes();

                var header = Dahomey.Cbor.Cbor.Deserialize<CborObject>(rawHeaderBytes.Span);
                var payload = Dahomey.Cbor.Cbor.Deserialize<CborObject>(rawPayloadBytes.Span);

                token = new CborWebToken(
                    new CborWebToken.Header(header, rawHeaderBytes),
                    new CborWebToken.Payload(payload, rawPayloadBytes),
                    new CborWebToken.Signature(rawSignatureBytes));

                return true;
            }
            catch (CborException cborException)
            {
                _logger.LogError(cborException, "Failed to decode CBOR structure");

                token = null;

                return false;
            }
        }

        private string AddBase32Padding(string base32Payload)
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
