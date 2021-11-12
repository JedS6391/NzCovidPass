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

        public bool TryReadToken(byte[] data, out CborWebToken? token)
        {
            ArgumentNullException.ThrowIfNull(data);

            try 
            {                
                var decodedCborStructure = Dahomey.Cbor.Cbor.Deserialize<CborArray>(data);
                
                _logger.LogDebug("Decoded CBOR structure: {Structure}", decodedCborStructure);

                var encodedHeaderBytes = decodedCborStructure[0].GetValueBytes();
                var encodedPayloadBytes = decodedCborStructure[2].GetValueBytes();              
                  
                var header = Dahomey.Cbor.Cbor.Deserialize<CborObject>(encodedHeaderBytes.Span);
                var payload = Dahomey.Cbor.Cbor.Deserialize<CborObject>(encodedPayloadBytes.Span);

                token = new CborWebToken(new CborWebToken.Header(header), new CborWebToken.Payload(payload));

                return true;
            }
            catch (CborException cborException)
            {
                _logger.LogError(cborException, "Failed to decode CBOR structure");

                token = null;

                return false;
            }
        }
    }
}