using System.Text;
using System.Text.Json;
using Dahomey.Cbor.ObjectModel;
using Microsoft.IdentityModel.Tokens;
using NzCovidPass.Core.Models;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Tokens
{
    /// <summary>
    /// A <see cref="SecurityToken" /> designed for representing a CBOR Web Token (CWT).
    /// </summary>
    /// <remarks>
    /// <see href="https://datatracker.ietf.org/doc/html/rfc8392" />
    /// </remarks>
    public class CborWebToken : SecurityToken
    {
        private readonly Header _header;
        private readonly Payload _payload;
        private readonly Signature _signature;

        /// <summary>
        /// Initializes a new instance of the <see cref="CborWebToken" /> class.
        /// </summary>
        /// <param name="header">The CWT header.</param>
        /// <param name="payload">The CWT payload.</param>
        /// <param name="signature">The CWT signature.</param>
        public CborWebToken(Header header, Payload payload, Signature signature)
        {
            _header = Requires.NotNull(header);
            _payload = Requires.NotNull(payload);
            _signature = Requires.NotNull(signature);
        }

        public override string Id => Jti;
        public override string Issuer => _payload.Issuer;
        public override SecurityKey SecurityKey => null;
        public override SecurityKey SigningKey { get; set; }
        public override DateTime ValidFrom => NotBefore.Date;
        public override DateTime ValidTo => Expiry.Date;

        public string KeyId => _header.KeyId;
        public string Algorithm => _header.Algorithm;
        public string Jti => _payload.Jti;
        public Guid Cti => _payload.Cti;
        public DateTimeOffset Expiry => _payload.Expiry;
        public DateTimeOffset NotBefore => _payload.NotBefore;
        public PublicCovidPass Credentials => _payload.Credentials;
        public byte[] HeaderBytes => _header.Bytes;
        public byte[] PayloadBytes => _payload.Bytes;
        public byte[] SignatureBytes => _signature.Bytes;

        public class Header
        {
            private readonly CborObject _cborObject;
            private readonly ReadOnlyMemory<byte> _rawHeader;

            public Header(CborObject cborObject, ReadOnlyMemory<byte> rawHeader)
            {
                _cborObject = Requires.NotNull(cborObject);
                _rawHeader = rawHeader;
            }

            // TODO: Improve this
            public string KeyId => Encoding.UTF8.GetString(_cborObject[Constants.Header.KeyId].GetValueBytes().Span);

            public string Algorithm => Constants.Header.CoseAlgorithmMap[ReadClaimValue<int>(_cborObject, Constants.Header.Algorithm)];

            public byte[] Bytes => _rawHeader.ToArray();
        }

        public class Payload
        {
            private readonly CborObject _cborObject;
            private readonly ReadOnlyMemory<byte> _rawPayload;

            public Payload(CborObject cborObject, ReadOnlyMemory<byte> rawPayload)
            {
                _cborObject = Requires.NotNull(cborObject);
                _rawPayload = rawPayload;
            }

            public string Jti => $"urn:uuid:{Cti:D}";

            // TODO: Improve this
            public Guid Cti => new Guid(_cborObject[Constants.Payload.Cti].GetValueBytes().Span);

            public string Issuer => ReadClaimValue<string>(_cborObject, Constants.Payload.Iss);

            public DateTimeOffset Expiry => DateTimeOffset.FromUnixTimeSeconds(ReadClaimValue<int>(_cborObject, Constants.Payload.Exp));

            public DateTimeOffset NotBefore => DateTimeOffset.FromUnixTimeSeconds(ReadClaimValue<int>(_cborObject, Constants.Payload.Nbf));

            public PublicCovidPass? Credentials => JsonSerializer.Deserialize<PublicCovidPass>(ReadClaimValueAsString(_cborObject, Constants.Payload.Vc));

            public byte[] Bytes => _rawPayload.ToArray();
        }

        public class Signature
        {
            private readonly ReadOnlyMemory<byte> _rawSignature;

            public Signature(ReadOnlyMemory<byte> rawSignature)
            {
                _rawSignature = rawSignature;
            }

            public byte[] Bytes => _rawSignature.ToArray();
        }

        private static T ReadClaimValue<T>(CborObject cborObject, CborValue claimId)
        {
            var rawClaimValue = cborObject[claimId];

            var decodedClaimValue = rawClaimValue.Value<T>();

            return decodedClaimValue;
        }

        private static string ReadClaimValueAsString(CborObject cborObject, CborValue claimId)
        {
            var rawClaimValue = cborObject[claimId];

            return rawClaimValue.ToString();
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
