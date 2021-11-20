using System.Formats.Cbor;
using Microsoft.Extensions.Logging;
using NzCovidPass.Core.Cbor;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Cwt
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

                if (!TryReadCoseStructure(decodedPayloadBytes, out var decodedCoseStructure) || decodedCoseStructure is null)
                {
                    _logger.LogError("Unable to read COSE structure");

                    context.Fail(CwtSecurityTokenReaderContext.FailedToDecodeCborStructure);

                    return;
                }

                _logger.LogDebug("Decoded COSE structure '{Structure}'", decodedCoseStructure);

                if (!IsValidCoseSingleSignerStructure(decodedCoseStructure))
                {
                    _logger.LogError("Payload is not a valid COSE_Sign1 structure");

                    context.Fail(CwtSecurityTokenReaderContext.InvalidCoseStructure);

                    return;
                }

                // By this point, we've validated the COSE structure so we can safely extract
                // the structure components to their respective types.
                var headerByteString = decodedCoseStructure[0] as CborByteString;
                var payloadByteString = decodedCoseStructure[2] as CborByteString;
                var signatureByteString = decodedCoseStructure[3] as CborByteString;

                if (!TryReadData(headerByteString!, out var headerData) || headerData is null)
                {
                    _logger.LogError("Unable to read CWT header data");

                    context.Fail(CwtSecurityTokenReaderContext.FailedToDecodeCborStructure);

                    return;
                }

                if (!TryReadData(payloadByteString!, out var payloadData) || payloadData is null)
                {
                    _logger.LogError("Unable to read CWT payload data");

                    context.Fail(CwtSecurityTokenReaderContext.FailedToDecodeCborStructure);

                    return;
                }

                var token = new CwtSecurityToken(
                    new CwtSecurityToken.Header(headerData, headerByteString!.Value),
                    new CwtSecurityToken.Payload(payloadData, payloadByteString!.Value),
                    new CwtSecurityToken.Signature(signatureByteString!.Value));

                context.Succeed(token);
            }
            catch (FormatException formatException)
            {
                _logger.LogError(formatException, "Failed to decode base-32 payload");

                context.Fail(CwtSecurityTokenReaderContext.InvalidBase32Payload);
            }
            catch (CborContentException cborContentException)
            {
                _logger.LogError(cborContentException, "Failed to decode CBOR structure");

                context.Fail(CwtSecurityTokenReaderContext.FailedToDecodeCborStructure);
            }
        }

        private bool TryReadCoseStructure(byte[] decodedPayloadBytes, out CborArray? coseStructure)
        {
            var cborReader = new CborReader(decodedPayloadBytes);

            if (!IsCoseSingleSignerDataObject(cborReader))
            {
                _logger.LogError("Unable to read payload as COSE single signer structure");

                coseStructure = null;

                return false;
            }

            if (!cborReader.TryReadArray(out coseStructure) || coseStructure is null)
            {
                _logger.LogError("Unable to read payload as CBOR array type");

                coseStructure = null;

                return false;
            }

            return true;
        }

        private static bool TryReadData(CborByteString byteString, out IReadOnlyDictionary<object, object>? data)
        {
            var cborReader = new CborReader(byteString.Value);

            if (!cborReader.TryReadMap(out var cborMap) || cborMap is null)
            {
                data = null;

                return false;
            }

            data = cborMap.ToGenericDictionary();

            return true;
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

        private static bool IsCoseSingleSignerDataObject(CborReader reader)
        {
            // https://www.iana.org/assignments/cbor-tags/cbor-tags.xhtml
            const CborTag CoseSingleSignerTag = (CborTag) 18;

            var state = reader.PeekState();

            if (state != CborReaderState.Tag)
            {
                return false;
            }

            var tag = reader.ReadTag();

            return tag == CoseSingleSignerTag;
        }

        private bool IsValidCoseSingleSignerStructure(CborArray coseStructure)
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
                coseStructure[1].Type != CborValueType.Map ||
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
