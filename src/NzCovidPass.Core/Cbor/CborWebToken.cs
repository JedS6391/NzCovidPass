using System.Text;
using Dahomey.Cbor.ObjectModel;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Cbor
{
    public class CborWebToken
    {
        private readonly Header _header;
        private readonly Payload _payload;

        public CborWebToken(Header header, Payload payload)
        {
            _header = Requires.NotNull(header);
            _payload = Requires.NotNull(payload);
        }

        public string KeyId => _header.KeyId;
        public string Algorithm => _header.Algorithm;
        public string Jti => _payload.Jti;
        public Guid Cti => _payload.Cti;
        public string Issuer => _payload.Issuer;
        public DateTimeOffset Expiry => _payload.Expiry;
        public DateTimeOffset NotBefore => _payload.NotBefore;

        public class Header 
        {
            private readonly CborObject _cborObject;

            public Header(CborObject cborObject)
            {
                _cborObject = Requires.NotNull(cborObject);
            }

            // TODO: Improve this
            public string KeyId => Encoding.UTF8.GetString(_cborObject[Constants.Header.KeyId].GetValueBytes().Span);

            public string Algorithm => Constants.Header.CoseAlgorithmMap[ReadClaimValue<int>(_cborObject, Constants.Header.Algorithm)];
        }
        
        public class Payload 
        {
            private readonly CborObject _cborObject;

            public Payload(CborObject cborObject)
            {
                _cborObject = Requires.NotNull(cborObject);
            }

            public string Jti => $"urn:uuid:{Cti:D}";

            // TODO: Improve this
            public Guid Cti => new Guid(_cborObject[Constants.Payload.Cti].GetValueBytes().Span);

            public string Issuer => ReadClaimValue<string>(_cborObject, Constants.Payload.Iss);

            public DateTimeOffset Expiry => DateTimeOffset.FromUnixTimeSeconds(ReadClaimValue<int>(_cborObject, Constants.Payload.Exp));

            public DateTimeOffset NotBefore => DateTimeOffset.FromUnixTimeSeconds(ReadClaimValue<int>(_cborObject, Constants.Payload.Nbf));            
        }

        private static T ReadClaimValue<T>(CborObject cborObject, int claimId)
        {
            var rawClaimValue = cborObject[claimId];

            var decodedClaimValue = rawClaimValue.Value<T>();

            return decodedClaimValue;
        }

        private static class Constants
        {
            public static class Header 
            {
                public const int Algorithm = 1;
                public const int KeyId = 4;

                public static readonly Dictionary<int, string> CoseAlgorithmMap = new Dictionary<int, string>()
                {
                    { -7, "ES256" }
                };
            }

            public static class Payload 
            {
                public const int Iss = 1;
                public const int Exp = 4;
                public const int Nbf = 5;
                public const int Cti = 7;
                public const string Vc = "vc";
            }
        }        
    }
}