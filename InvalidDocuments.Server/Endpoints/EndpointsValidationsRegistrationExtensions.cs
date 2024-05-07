using InvalidDocuments.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace InvalidDocuments.Server.Endpoints;

/// <summary>
/// Provides extension methods for registering endpoints related to document validation.
/// </summary>
public static class EndpointsValidationsRegistrationExtensions
{
    /// <summary>
    /// Extension method to map endpoints related to document validation.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance.</param>
    /// <returns>The modified <see cref="WebApplication"/> instance with mapped endpoints.</returns>
    public static WebApplication MapEndpointsValidations(this WebApplication app)
    {
        app.MapGet("validations", async ([FromQuery] string number, [FromServices] IDocumentValidateService documentValidateService, CancellationToken cancellationToken) =>
        {
            number = RemoveWhiteSpace(number);

            if (!IsValidDocumentNumber(number, out string error))
            {
                return Results.BadRequest(error);
            }

            try
            {
                var result = await documentValidateService.ValidateDocumentAsync(number, cancellationToken);

                return Results.Ok(new DocumentValidationDto
                (
                    result.Number,
                    result.Series,
                    result.Type,
                    result.IsRegistered,
                    result.RegisteredFrom,
                    result.BadRequest,
                    result.Error
                ));
            }
            catch (Exception)
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        })
        .WithTags("Validations")
        .WithName("GetDocumentValidation")
        .WithOpenApi(operation => new(operation) { Summary = "Get document validation by number" })
        .Produces<DocumentValidationDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }

    /// <summary>
    /// Removes white spaces from a string.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <returns>The input string without white spaces.</returns>
    internal static string RemoveWhiteSpace(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return Regex.Replace(text, @"\s+", "");
    }

    /// <summary>
    /// Validates the format and length of a document number.
    /// </summary>
    /// <param name="number">The document number to validate.</param>
    /// <param name="error">The error message if validation fails.</param>
    /// <returns>True if the document number is valid; otherwise, false.</returns>
    internal static bool IsValidDocumentNumber(string? number, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrEmpty(number))
        {
            error = "Číslo dokladu nemůže být prázdné.";
            return false;
        }

        if (number.Length > 10)
        {
            error = "Číslo dokladu může obsahovat max. 10 znaků. ";
        }

        string pattern = @"^[a-zA-Z0-9]*$";
        bool isValid = Regex.IsMatch(number, pattern);

        if (!isValid)
        {
            error += "Číslo dokladu obsahuje neplatné znaky.";
        }

        return error == string.Empty;
    }

    internal sealed record DocumentValidationDto(string Number,
                                                 string Series,
                                                 string Type,
                                                 bool IsRegistered,
                                                 string RegisteredFrom,
                                                 bool BadRequest,
                                                 string Error);
}