using InvalidDocuments.Server.Models;

namespace InvalidDocuments.Server.Services;

public interface IInvalidDocumentService
{
    Task<InvalidDocument> GetInvalidDocumentAsync(string documentNumber, CancellationToken cancellationToken);
}