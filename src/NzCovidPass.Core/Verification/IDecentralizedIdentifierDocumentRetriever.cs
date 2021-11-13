namespace NzCovidPass.Core.Verification
{
    public interface IDecentralizedIdentifierDocumentRetriever
    {
        Task<DecentralizedIdentifierDocument> GetDocumentAsync(string issuer);
    }
}
