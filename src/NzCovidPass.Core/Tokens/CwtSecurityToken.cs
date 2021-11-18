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
    /// <para>
    /// The properties exposed by this class are modelled on the structure described in <see href="https://nzcp.covid19.health.nz" />,
    /// as opposed to a more generic CWT structure.
    /// </para>
    /// <see href="https://datatracker.ietf.org/doc/html/rfc8392" />
    /// </remarks>
    public class CwtSecurityToken : SecurityToken
    {
        private readonly Header _header;
        private readonly Payload _payload;
        private readonly Signature _signature;

        /// <summary>
        /// Initializes a new instance of the <see cref="CwtSecurityToken" /> class.
        /// </summary>
        /// <param name="header">The CWT header.</param>
        /// <param name="payload">The CWT payload.</param>
        /// <param name="signature">The CWT signature.</param>
        public CwtSecurityToken(Header header, Payload payload, Signature signature)
        {
            _header = Requires.NotNull(header);
            _payload = Requires.NotNull(payload);
            _signature = Requires.NotNull(signature);
        }

        /// <inheritdoc />
        public override string? Id => Jti;

        /// <inheritdoc />
        public override string? Issuer => _payload.Issuer;

        /// <inheritdoc />
        public override SecurityKey? SecurityKey => null;

        /// <inheritdoc />
        public override SecurityKey? SigningKey { get; set; }

        /// <inheritdoc />
        public override DateTime ValidFrom => NotBefore.Date;

        /// <inheritdoc />
        public override DateTime ValidTo => Expiry.Date;

        /// <summary>
        /// Gets the identifier of the key used to sign the token from the CWT header.
        /// </summary>
        public string? KeyId => _header.KeyId;

        /// <summary>
        /// Gets the algorithm used to sign the token from the CWT header.
        /// </summary>
        public string? Algorithm => _header.Algorithm;

        /// <summary>
        /// Gets the value of the <c>cti</c> claim from the CWT payload, mapped to a JTI value.
        /// </summary>
        /// <remarks>
        /// <see href="https://nzcp.covid19.health.nz/#mapping-jti-cti" />
        /// </remarks>
        public string? Jti => _payload.Jti;

        /// <summary>
        /// Gets the value of the <c>cti</c> claim from the CWT payload.
        /// </summary>
        /// <remarks>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc8392#section-3.1.7" />
        /// </remarks>
        public Guid Cti => _payload.Cti;

        /// <summary>
        /// Gets the value of the <c>exp</c> claim from the CWT payload.
        /// </summary>
        /// <remarks>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc8392#section-3.1.4" />
        /// </remarks>
        public DateTimeOffset Expiry => _payload.Expiry;

        /// <summary>
        /// Gets the value of the <c>nbf</c> claim from the CWT payload.
        /// </summary>
        /// <remarks>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc8392#section-3.1.5" />
        /// </remarks>
        public DateTimeOffset NotBefore => _payload.NotBefore;

        /// <summary>
        /// Gets the value of the <c>vc</c> claim from the CWT payload.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The content of the claim is deserialized based on the structure described in <see href="https://nzcp.covid19.health.nz/#publiccovidpass" />.
        /// </para>
        /// <see href="https://nzcp.covid19.health.nz/#cwt-claims" />
        /// </remarks>
        public VerifiableCredential<PublicCovidPass>? Credential => _payload.Credential;

        /// <summary>
        /// Gets the raw bytes of the CWT header.
        /// </summary>
        public byte[] HeaderBytes => _header.Bytes;

        /// <summary>
        /// Gets the raw bytes of the CWT payload.
        /// </summary>
        public byte[] PayloadBytes => _payload.Bytes;

        /// <summary>
        /// Gets the raw bytes of the CWT signature.
        /// </summary>
        public byte[] SignatureBytes => _signature.Bytes;

        /// <summary>
        /// Represents the protected header of a CWT.
        /// </summary>
        /// <remarks>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc8152#section-2" />
        /// </remarks>
        public class Header
        {
            private readonly CborObject _cborObject;
            private readonly ReadOnlyMemory<byte> _rawHeader;

            /// <summary>
            /// Initializes a new instance of the <see cref="Header" /> class.
            /// </summary>
            /// <param name="cborObject">The CBOR object representing the header.</param>
            /// <param name="rawHeader">The raw bytes of the header.</param>
            public Header(CborObject cborObject, ReadOnlyMemory<byte> rawHeader)
            {
                _cborObject = Requires.NotNull(cborObject);
                _rawHeader = rawHeader;
            }

            /// <summary>
            /// Gets the identifier of the key used to sign the token.
            /// </summary>
            public string? KeyId
            {
                get
                {
                    var keyId = ReadRawClaimValue(_cborObject, ClaimIds.Header.KeyId);

                    if (keyId is null)
                    {
                        return null;
                    }

                    var keyIdBytes = keyId.GetValueBytes();

                    return Encoding.UTF8.GetString(keyIdBytes.Span);
                }
            }

            /// <summary>
            /// Gets the algorithm used to sign the token.
            /// </summary>
            public string? Algorithm
            {
                get
                {
                    var algorithmId = ReadClaimValue<int>(_cborObject, ClaimIds.Header.Algorithm);

                    if (ClaimIds.Header.AlgorithmMap.TryGetValue(algorithmId, out var algorithmName))
                    {
                        return algorithmName;
                    }

                    return null;
                }
            }

            /// <summary>
            /// Gets the raw header bytes.
            /// </summary>
            public byte[] Bytes => _rawHeader.ToArray();
        }

        /// <summary>
        /// Represents the payload of a CWT.
        /// </summary>
        /// <remarks>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc8392" />
        /// </remarks>
        public class Payload
        {
            private readonly CborObject _cborObject;
            private readonly ReadOnlyMemory<byte> _rawPayload;

            /// <summary>
            /// Initializes a new instance of the <see cref="Header" /> class.
            /// </summary>
            /// <param name="cborObject">The CBOR object representing the payload.</param>
            /// <param name="rawPayload">The raw bytes of the payload.</param>
            public Payload(CborObject cborObject, ReadOnlyMemory<byte> rawPayload)
            {
                _cborObject = Requires.NotNull(cborObject);
                _rawPayload = rawPayload;
            }

            /// <summary>
            /// Gets the value of the <c>cti</c> claim, mapped to a JTI value.
            /// </summary>
            public string? Jti => Cti == Guid.Empty ? null : $"urn:uuid:{Cti:D}";

            /// <summary>
            /// Gets the value of the <c>cti</c> claim.
            /// </summary>
            public Guid Cti
            {
                get
                {
                    var cti = ReadRawClaimValue(_cborObject, ClaimIds.Payload.Cti);

                    if (cti is null)
                    {
                        return Guid.Empty;
                    }

                    var ctiBytes = cti.GetValueBytes();

                    return new Guid(ctiBytes.Span);
                }
            }

            /// <summary>
            /// Gets the value of <c>iss</c> claim.
            /// </summary>
            public string? Issuer => ReadClaimValue<string>(_cborObject, ClaimIds.Payload.Iss);

            /// <summary>
            /// Gets the value of <c>exp</c> claim.
            /// </summary>
            public DateTimeOffset Expiry => DateTimeOffset.FromUnixTimeSeconds(ReadClaimValue<int>(_cborObject, ClaimIds.Payload.Exp));

            /// <summary>
            /// Gets the value of <c>nbf</c> claim.
            /// </summary>
            public DateTimeOffset NotBefore => DateTimeOffset.FromUnixTimeSeconds(ReadClaimValue<int>(_cborObject, ClaimIds.Payload.Nbf));

            /// <summary>
            /// Gets the value of <c>vc</c> claim.
            /// </summary>
            public VerifiableCredential<PublicCovidPass>? Credential => ReadClaimValue<VerifiableCredential<PublicCovidPass>>(_cborObject, ClaimIds.Payload.Vc);

            /// <summary>
            /// Gets the raw payload bytes.
            /// </summary>
            public byte[] Bytes => _rawPayload.ToArray();
        }

        /// <summary>
        /// Represents the signature of a CWT.
        /// </summary>
        /// <remarks>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc8392" />
        /// </remarks>
        public class Signature
        {
            private readonly ReadOnlyMemory<byte> _rawSignature;

            /// <summary>
            /// Initializes a new instance of the <see cref="Signature" /> class.
            /// </summary>
            /// <param name="rawSignature">The raw bytes of the signature.</param>
            public Signature(ReadOnlyMemory<byte> rawSignature)
            {
                _rawSignature = rawSignature;
            }

            /// <summary>
            /// Gets the raw signature bytes.
            /// </summary>
            public byte[] Bytes => _rawSignature.ToArray();
        }

        private static T? ReadClaimValue<T>(CborObject cborObject, CborValue claimId)
        {
            var rawClaimValue = ReadRawClaimValue(cborObject, claimId);

            if (rawClaimValue is null)
            {
                return default;
            }

            if (rawClaimValue is CborObject claimObject)
            {
                var claimObjectJson = claimObject.ToString();

                return JsonSerializer.Deserialize<T>(claimObjectJson);
            }

            return rawClaimValue.Value<T>();
        }

        private static CborValue? ReadRawClaimValue(CborObject cborObject, CborValue claimId)
        {
            if (cborObject.TryGetValue(claimId, out var rawClaimValue))
            {
                return rawClaimValue;
            }

            return null;
        }

        private static class ClaimIds
        {
            public static class Header
            {
                public const int Algorithm = 1;
                public const int KeyId = 4;

                public static readonly Dictionary<int, string> AlgorithmMap = new Dictionary<int, string>()
                {
                    { -7, SecurityAlgorithms.EcdsaSha256 },
                    { -16, SecurityAlgorithms.Sha256 },
                    { -44, SecurityAlgorithms.Sha512 },
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
