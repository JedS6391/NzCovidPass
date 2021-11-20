using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NzCovidPass.Core;

namespace NzCovidPass.Function
{
    public class VerifyPass
    {
        private readonly ILogger _logger;
        private readonly PassVerifier _passVerifier;

        public VerifyPass(ILoggerFactory loggerFactory, PassVerifier passVerifier)
        {
            _logger = loggerFactory.CreateLogger<VerifyPass>();
            _passVerifier = passVerifier;
        }

        [Function("VerifyPass")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
        {
            var verifyPassRequest = await DeserializeRequestAsync(request);

            if (verifyPassRequest is null || !verifyPassRequest.IsValid())
            {
                return request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var result = await _passVerifier.VerifyAsync(verifyPassRequest.PassPayload);

            var response = request.CreateResponse(result.HasSucceeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest);

            await response.WriteAsJsonAsync<object>(result.HasSucceeded ?
                new ValidPassResponse()
                {
                    GivenName = result.Token.Credential.CredentialSubject.GivenName,
                    FamilyName = result.Token.Credential.CredentialSubject.FamilyName,
                    DateOfBirth = result.Token.Credential.CredentialSubject.DateOfBirth,
                    ExpiresOn = result.Token.Expiry
                } :
                new InvalidPassResponse()
                {
                    FailureCodes = result.FailureReasons.Select(fr => fr.Code)
                });

            return response;
        }

        private static async Task<VerifyPassRequest> DeserializeRequestAsync(HttpRequestData request)
        {
            if (request.Body is null)
            {
                return null;
            }

            return await JsonSerializer.DeserializeAsync<VerifyPassRequest>(request.Body);
        }
    }
}
