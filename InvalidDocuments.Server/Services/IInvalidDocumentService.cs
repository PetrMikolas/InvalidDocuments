using InvalidDocuments.Server.Models;

namespace InvalidDocuments.Server.Services;

public interface IInvalidDocumentService
{
    /// <summary>
    /// Retrieves information about an invalid document asynchronously.
    /// </summary>
    /// <param name="documentNumber">The document number to retrieve information for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An InvalidDocument object containing information about the document.</returns>
    Task<InvalidDocument> GetInvalidDocumentAsync(string documentNumber, CancellationToken cancellationToken);
}