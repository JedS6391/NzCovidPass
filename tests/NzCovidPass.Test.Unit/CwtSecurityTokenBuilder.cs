using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Dahomey.Cbor;
using Dahomey.Cbor.ObjectModel;
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
            _context.Header[ClaimIds.Header.KeyId] = new CborByteString(Encoding.UTF8.GetBytes(keyId));

            return this;
        }

        public CwtSecurityTokenBuilder WithAlgorithm(string algorithmName)
        {
            _context.Header[ClaimIds.Header.Algorithm] = ClaimIds.Header.AlgorithmMap[algorithmName];

            return this;
        }

        public CwtSecurityTokenBuilder WithCti(Guid cti)
        {
            _context.Payload[ClaimIds.Payload.Cti] = new CborByteString(cti.ToByteArray());

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
            var credentialSubjectObject = new CborObject(new Dictionary<CborValue, CborValue>()
            {
                { "givenName", credential.CredentialSubject.GivenName },
                { "familyName", credential.CredentialSubject.FamilyName },
                { "dob", credential.CredentialSubject.DateOfBirth.ToString("yyyy-MM-dd") },
            });
            var credentialObject = new CborObject(new Dictionary<CborValue, CborValue>()
            {
                { "version", credential.Version },
                { "@context", CborArray.FromCollection(credential.Context) },
                { "type", CborArray.FromCollection(credential.Type) },
                { "credentialSubject", credentialSubjectObject }
            });

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
            var header = new CborObject(_context.Header);

            var b = new ArrayBufferWriter<byte>();
            Cbor.Serialize(header, b);

            return new CwtSecurityToken.Header(header, b.WrittenSpan.ToArray());
        }

        private CwtSecurityToken.Payload BuildPayload()
        {
            var payload = new CborObject(_context.Payload);

            var b = new ArrayBufferWriter<byte>();
            Cbor.Serialize(payload, b);

            return new CwtSecurityToken.Payload(payload, b.WrittenSpan.ToArray());
        }

        private CwtSecurityToken.Signature BuildSignature(CwtSecurityToken.Header header, CwtSecurityToken.Payload payload)
        {
            var signature = _context.SignatureFunc.Invoke(header, payload);

            return new CwtSecurityToken.Signature(signature);
        }

        private class Context
        {
            public Dictionary<CborValue, CborValue> Header { get; } = new Dictionary<CborValue, CborValue>();
            public Dictionary<CborValue, CborValue> Payload { get; } = new Dictionary<CborValue, CborValue>();
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
