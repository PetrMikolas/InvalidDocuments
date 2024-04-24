using InvalidDocuments.Server.Enums;
using InvalidDocuments.Server.Helpers;
using InvalidDocuments.Server.Models;

namespace InvalidDocuments.Server.Services;

public class InvalidDocumentService(HttpClient httpClient, IConfiguration configuration, ILogger<InvalidDocumentService> logger) : IInvalidDocumentService
{
    public async Task<InvalidDocument> GetInvalidDocumentAsync(string documentNumber, CancellationToken cancellationToken)
    {
        var documentTypes = configuration.GetSection("DocumentTypes").Get<List<int>>();

        try
        {
            if (documentTypes is null)
            {
                logger.LogError("Z konfigurace nelze načíst hodnotu DocumentTypes.");
                throw new ArgumentException(null, nameof(documentTypes));
            }

            var tasks = new List<Task<InvalidDocument>>();

            documentTypes.ForEach(documentType => tasks.Add(GetInvalidDocumentByDocumentType(documentNumber, documentType, cancellationToken)));

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);

                if (completedTask.Result.Response.Registered == TruthValue.ano.ToString())
                {
                    return await completedTask;
                }
            }

            return new InvalidDocument
            {
                Request = new Request { Number = documentNumber },
                Response = new Response { Registered = TruthValue.ne.ToString() }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Neočekávaná chyba při ověřování platnosti dokladu.");

            return new InvalidDocument
            {
                Request = new Request { Number = documentNumber },
                Response = new Response { Registered = TruthValue.ne.ToString() },
                Error = new Error { Text = "Neočekávaná chyba. O problému víme a pracujeme na nápravě." }
            };
        }
    }

    /// <summary>
    /// Retrieves information about an invalid document based on the document type asynchronously.
    /// </summary>
    /// <param name="number">The document number to retrieve information for.</param>
    /// <param name="type">The document type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An InvalidDocument object containing information about the document.</returns>
    private async Task<InvalidDocument> GetInvalidDocumentByDocumentType(string number, int type, CancellationToken cancellationToken)
    {
        var baseUrlApi = configuration["BaseUrlApiMvcr"];

        if (string.IsNullOrEmpty(baseUrlApi))
        {
            logger.LogError("Z konfigurace nelze načíst hodnotu BaseUrlApiMvcr.");
            throw new ArgumentException(null, nameof(baseUrlApi));
        }

        var requestUri = $"{baseUrlApi}?dotaz={number}&doklad={type}";

        try
        {
            var data = await httpClient.GetStringAsync(requestUri, cancellationToken);
            return Helper.DeserializeXmlToObject<InvalidDocument>(data) ?? new();
        }
        catch (Exception)
        {
            throw;
        }
    }
}