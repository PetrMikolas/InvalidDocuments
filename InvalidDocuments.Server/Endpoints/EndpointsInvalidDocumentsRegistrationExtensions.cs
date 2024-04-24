using InvalidDocuments.Server.Helpers;
using InvalidDocuments.Server.Models;
using InvalidDocuments.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvalidDocuments.Server.Endpoints;

/// <summary>
/// Provides extension methods for registering endpoints related to invalid documents.
/// </summary>
public static class EndpointsInvalidDocumentsRegistrationExtensions
{
    /// <summary>
    /// Extension method to map endpoints related to invalid documents.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance.</param>
    /// <returns>The modified <see cref="WebApplication"/> instance with mapped endpoints.</returns>
    public static WebApplication MapEndpointsInvalidDocuments(this WebApplication app)
    {
        app.MapGet("documents", async ([FromServices] IInvalidDocumentService invalidDocumentService, [FromQuery] string number, CancellationToken cancellationToken) =>
        {
            number = number.RemoveWhiteSpace();

            if (!Helper.IsValidDocumentNumber(number, out string error))
            {
                return Results.BadRequest(error);
            }

            try
            {             
                var response = await invalidDocumentService.GetInvalidDocumentAsync(number, cancellationToken);
                return Results.Ok(response.GetInvalidDocumentDto());
            }
            catch (Exception)
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        })
        .WithTags("InvalidDocuments")
        .WithName("GetInvalidDocument")
        .WithOpenApi(operation => new(operation) { Summary = "Get invalid document by number" })
        .Produces<InvalidDocumentDto>(StatusCodes.Status200OK)        
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}