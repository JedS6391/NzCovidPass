using NzCovidPass.Core.Models;
namespace NzCovidPass.Core.Verification
{
    /// <summary>
    /// Provides mechanisms for retrieving Decentralized Identifier (DID) documents.
    /// </summary>
    public interface IDecentralizedIdentifierDocumentRetriever
    {
        /// <summary>
        /// Retrieves the Decentralized Identifier (DID) document associated with the provided <paramref name="issuer" />.
        /// </summary>
        /// <param name="issuer">The issuer to retrieve the DID document for.</param>
        /// <returns>A task representing the asynchronous operation. The result contains the <see cref="DecentralizedIdentifierDocument" /> retrieved.</returns>
        Task<DecentralizedIdentifierDocument> GetDocumentAsync(string issuer);
    }
}
