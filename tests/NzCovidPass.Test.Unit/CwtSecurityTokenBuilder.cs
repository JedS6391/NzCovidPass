using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NzCovidPass.Core.Models;
using NzCovidPass.Core.Tokens;

namespace NzCovidPass.Test.Unit
{
    /// <summary>
    /// Builder to aid in the creation of <see cref="CwtSecurityToken" /> instances for testing purposes.
    /// </summary>
    internal class CwtSecurityTokenBuilder
    {
        private readonly Context _context;

        public static CwtSecurityTokenBuilder New => new CwtSecurityTokenBuilder();

        private CwtSecurityTokenBuilder()
        {
            _context = new Context();
        }

        public CwtSecurityTokenBuilder WithKeyId(string keyId)
        {
            _context.Header[ClaimIds.Header.KeyId] = Encoding.UTF8.GetBytes(keyId);

            return this;
        }

        public CwtSecurityTokenBuilder WithAlgorithm(string algorithmName)
        {
            _context.Header[ClaimIds.Header.Algorithm] = ClaimIds.Header.AlgorithmMap[algorithmName];

            return this;
        }

        public CwtSecurityTokenBuilder WithCti(Guid cti)
        {
            _context.Payload[ClaimIds.Payload.Cti] = cti.ToByteArray();

            return this;
        }

        public CwtSecurityTokenBuilder WithIssuer(string issuer)
        {
            _context.Payload[ClaimIds.Payload.Iss] = issuer;

            return this;
        }

        public CwtSecurityTokenBuilder WithExpiry(DateTimeOffset expiry)
        {
            _context.Payload[ClaimIds.Payload.Exp] = expiry.ToUnixTimeSeconds();

            return this;
        }

        public CwtSecurityTokenBuilder WithNotBefore(DateTimeOffset notBefore)
        {
            _context.Payload[ClaimIds.Payload.Nbf] = notBefore.ToUnixTimeSeconds();

            return this;
        }

        public CwtSecurityTokenBuilder WithPublicCovidPassCredential(VerifiableCredential<PublicCovidPass> credential)
        {
            var credentialSubject = new Dictionary<object, object>()
            {
                { "givenName", credential.CredentialSubject.GivenName },
                { "familyName", credential.CredentialSubject.FamilyName },
                { "dob", credential.CredentialSubject.DateOfBirth.ToString("yyyy-MM-dd") },
            };
            var credentialObject = new Dictionary<object, object>()
            {
                { "version", credential.Version },
                { "@context", credential.Context.ToList() },
                { "type", credential.Type.ToList() },
                { "credentialSubject", credentialSubject }
            };

            _context.Payload[ClaimIds.Payload.Vc] = credentialObject;

            return this;
        }

        public CwtSecurityTokenBuilder WithSignatureFunc(Func<CwtSecurityToken.Header, CwtSecurityToken.Payload, byte[]> signatureFunc)
        {
            _context.SignatureFunc = signatureFunc;

            return this;
        }

        public CwtSecurityToken Build()
        {
            var header = BuildHeader();
            var payload = BuildPayload();
            var signature = BuildSignature(header, payload);

            return new CwtSecurityToken(header, payload, signature);
        }

        private CwtSecurityToken.Header BuildHeader()
        {
            return new CwtSecurityToken.Header(_context.Header, Array.Empty<byte>());
        }

        private CwtSecurityToken.Payload BuildPayload()
        {
            return new CwtSecurityToken.Payload(_context.Payload, Array.Empty<byte>());
        }

        private CwtSecurityToken.Signature BuildSignature(CwtSecurityToken.Header header, CwtSecurityToken.Payload payload)
        {
            var signature = _context.SignatureFunc.Invoke(header, payload);

            return new CwtSecurityToken.Signature(signature);
        }

        private class Context
        {
            public Dictionary<object, object> Header { get; } = new Dictionary<object, object>();
            public Dictionary<object, object> Payload { get; } = new Dictionary<object, object>();
            public Func<CwtSecurityToken.Header, CwtSecurityToken.Payload, byte[]> SignatureFunc { get; set; } = (header, payload) => Array.Empty<byte>();
        }

        private static class ClaimIds
        {
            public static class Header
            {
                public const int Algorithm = 1;
                public const int KeyId = 4;

                public static readonly Dictionary<string, int> AlgorithmMap = new Dictionary<string, int>()
                {
                    { SecurityAlgorithms.EcdsaSha256, -7 },
                    { SecurityAlgorithms.Sha256, -16 },
                    { SecurityAlgorithms.Sha512, -44 }
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
