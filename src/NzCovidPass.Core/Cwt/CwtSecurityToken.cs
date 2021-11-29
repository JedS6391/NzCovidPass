using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using NzCovidPass.Core.Models;
using NzCovidPass.Core.Shared;

namespace NzCovidPass.Core.Cwt
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

        /// <summary>
        /// Gets the value of the <c>jti</c> claim.
        /// </summary>
        /// <remarks>
        public override string? Id => Jti;

        /// <summary>
        /// Gets the value of the <c>iss</c> claim.
        /// </summary>
        public override string? Issuer => _payload.Issuer;

        /// <inheritdoc />
        public override SecurityKey? SecurityKey => null;

        /// <summary>
        /// Gets or sets the <see cref="SecurityKey" /> that signed the token.
        /// </summary>
        /// <remarks>
        /// This property will only be set once the token signature been validated.
        /// </remarks>
        public override SecurityKey? SigningKey { get; set; }

        /// <summary>
        /// Gets the value of the <c>nbf</c> claim, represented as a UTC <see cref="DateTime" />.
        /// </summary>
        public override DateTime ValidFrom => NotBefore.UtcDateTime;

        /// <summary>
        /// Gets the value of the <c>exp</c> claim, represented as a UTC <see cref="DateTime" />.
        /// </summary>
        public override DateTime ValidTo => Expiry.UtcDateTime;

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
        /// The value of the claim is converted from a UNIX time expressed in seconds to a <see cref="DateTimeOffset" />.
        /// </remarks>
        public DateTimeOffset Expiry => _payload.Expiry;

        /// <summary>
        /// Gets the value of the <c>nbf</c> claim from the CWT payload.
        /// </summary>
        /// <remarks>
        /// The value of the claim is converted from a UNIX time expressed in seconds to a <see cref="DateTimeOffset" />.
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
            private readonly IReadOnlyDictionary<object, object> _claims;
            private readonly byte[] _headerBytes;

            /// <summary>
            /// Initializes a new instance of the <see cref="Header" /> class.
            /// </summary>
            /// <param name="claims">The claims contained in the header.</param>
            /// <param name="headerBytes">The raw bytes of the header.</param>
            public Header(IReadOnlyDictionary<object, object> claims, byte[] headerBytes)
            {
                _claims = Requires.NotNull(claims);
                _headerBytes = headerBytes;
            }

            /// <summary>
            /// Gets the identifier of the key used to sign the token.
            /// </summary>
            public string? KeyId
            {
                get
                {
                    var keyId = ReadRawClaimValue(_claims, ClaimIds.Header.KeyId);

                    if (keyId is null)
                    {
                        return null;
                    }

                    var keyIdBytes = keyId as byte[];

                    return Encoding.UTF8.GetString(keyIdBytes!);
                }
            }

            /// <summary>
            /// Gets the algorithm used to sign the token.
            /// </summary>
            public string? Algorithm
            {
                get
                {
                    var algorithmId = ReadClaimValue<int>(_claims, ClaimIds.Header.Algorithm);

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
            public byte[] Bytes => _headerBytes;
        }

        /// <summary>
        /// Represents the payload of a CWT.
        /// </summary>
        /// <remarks>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc8392" />
        /// </remarks>
        public class Payload
        {
            private readonly IReadOnlyDictionary<object, object> _claims;
            private readonly byte[] _payloadBytes;

            /// <summary>
            /// Initializes a new instance of the <see cref="Header" /> class.
            /// </summary>
            /// <param name="claims">The claims contained in the payload.</param>
            /// <param name="payloadBytes">The raw bytes of the payload.</param>
            public Payload(IReadOnlyDictionary<object, object> claims, byte[] payloadBytes)
            {
                _claims = Requires.NotNull(claims);
                _payloadBytes = payloadBytes;
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
                    var cti = ReadRawClaimValue(_claims, ClaimIds.Payload.Cti);

                    if (cti is null)
                    {
                        return Guid.Empty;
                    }

                    var ctiBytes = cti as byte[];

                    return new Guid(ctiBytes!);
                }
            }

            /// <summary>
            /// Gets the value of <c>iss</c> claim.
            /// </summary>
            public string? Issuer => ReadClaimValue<string>(_claims, ClaimIds.Payload.Iss);

            /// <summary>
            /// Gets the value of <c>exp</c> claim.
            /// </summary>
            /// <remarks>
            /// <see href="https://datatracker.ietf.org/doc/html/rfc8392#section-3.1.4" />
            /// </remarks>
            public DateTimeOffset Expiry => DateTimeOffset.FromUnixTimeSeconds(ReadClaimValue<long>(_claims, ClaimIds.Payload.Exp));

            /// <summary>
            /// Gets the value of <c>nbf</c> claim.
            /// </summary>
            /// <remarks>
            /// <see href="https://datatracker.ietf.org/doc/html/rfc8392#section-3.1.5" />
            /// </remarks>
            public DateTimeOffset NotBefore => DateTimeOffset.FromUnixTimeSeconds(ReadClaimValue<long>(_claims, ClaimIds.Payload.Nbf));

            /// <summary>
            /// Gets the value of <c>vc</c> claim.
            /// </summary>
            public VerifiableCredential<PublicCovidPass>? Credential
            {
                get
                {
                    var credential = ReadRawClaimValue(_claims, ClaimIds.Payload.Vc);

                    if (credential is null)
                    {
                        return null;
                    }

                    // TODO: This is expensive so should ideally be cached or done another way.
                    var credentialJson = JsonSerializer.Serialize(credential);

                    return JsonSerializer.Deserialize<VerifiableCredential<PublicCovidPass>>(credentialJson);
                }
            }

            /// <summary>
            /// Gets the raw payload bytes.
            /// </summary>
            public byte[] Bytes => _payloadBytes;
        }

        /// <summary>
        /// Represents the signature of a CWT.
        /// </summary>
        /// <remarks>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc8392" />
        /// </remarks>
        public class Signature
        {
            private readonly byte[] _signatureBytes;

            /// <summary>
            /// Initializes a new instance of the <see cref="Signature" /> class.
            /// </summary>
            /// <param name="signatureBytes">The raw bytes of the signature.</param>
            public Signature(byte[] signatureBytes)
            {
                _signatureBytes = signatureBytes;
            }

            /// <summary>
            /// Gets the raw signature bytes.
            /// </summary>
            public byte[] Bytes => _signatureBytes;
        }

        private static T? ReadClaimValue<T>(IReadOnlyDictionary<object, object> claims, object claimId)
        {
            var rawClaimValue = ReadRawClaimValue(claims, claimId);

            if (rawClaimValue is null)
            {
                return default;
            }

            return (T) Convert.ChangeType(rawClaimValue, typeof(T));
        }

        private static object? ReadRawClaimValue(IReadOnlyDictionary<object, object> claims, object claimId)
        {
            if (claims.TryGetValue(claimId, out var rawClaimValue))
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
