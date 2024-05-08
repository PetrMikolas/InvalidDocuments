using FluentAssertions;
using InvalidDocuments.Server.Endpoints;
using InvalidDocuments.Server.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using NUnit.Framework.Internal;
using System.Net;

namespace InvalidDocuments.UnitTests;

internal class DocumentValidateServiceTests
{
    private DocumentValidateService _documentValidateService;
    private readonly Mock<IConfiguration> _mockConfiguration = new();
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler = new();
    private InvalidDocument _expectedInvalidDocument;
    private DocumentValidationResult _expectedDocumentValidationResult;
    private string _xmlInvalidDocument;
    private string _number;
    private int _type;

    [SetUp]
    public void Setup()
    {
        _xmlInvalidDocument = "<doklady_neplatne posl_zmena=\"12.8.2010\" pristi_zmeny=\"\">" +
                                        "<dotaz typ=\"OPs\" cislo=\"183579\" serie=\"AA81\"/>" +
                                        "<odpoved aktualizovano=\"24.4.2024\" evidovano=\"ano\" evidovano_od=\"15.4.2024\"/>" +
                                     "</doklady_neplatne>";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(_xmlInvalidDocument) });

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        var section = new ConfigurationBuilder()
          .AddInMemoryCollection(new Dictionary<string, string>
          {
            {"DocumentTypes:0", "0"},
            {"DocumentTypes:1", "4"},
            {"DocumentTypes:2", "6"}
          }!)
          .Build()
          .GetSection("DocumentTypes");

        _mockConfiguration.Setup(config => config.GetSection("DocumentTypes")).Returns(section);
        _mockConfiguration.Setup(config => config["BaseUrlApiMvcr"]).Returns("http://example.com/api");

        var logger = new NullLogger<DocumentValidateService>();

        _documentValidateService = new DocumentValidateService(httpClient, _mockConfiguration.Object, logger);

        _expectedInvalidDocument = new InvalidDocument
        {
            Request = new() { Number = "183579", Series = "AA81", Type = "OPs" },
            Response = new() { Registered = "ano", RegisteredFrom = "15.4.2024", Updated = "24.4.2024" },
            Error = new(),
            LastChange = "12.8.2010",
            NextChanges = string.Empty,
        };

        _expectedDocumentValidationResult = new DocumentValidationResult
        {
            Number = "183579",
            Series = "AA81",
            Type = "občanský průkaz",
            IsRegistered = true,
            RegisteredFrom = "15.4.2024",
            BadRequest = false,
            Error = string.Empty,
        };

        _number = "183579AA81";
        _type = 0;
    }

    [Test]
    public async Task ValidateDocumentAsync_InvalidDocument_Success()
    {     
        // Act
        var result = await _documentValidateService.ValidateDocumentAsync(_number);

        // Assert
        result.Should().BeEquivalentTo(_expectedDocumentValidationResult);
    }

    [Test]
    [TestCase("")]
    [TestCase(null!)]
    public async Task ValidateDocumentAsync_DocumentNumberIsNullOrEmpty_Success(string number)
    {
        // Act
        var result = await _documentValidateService.ValidateDocumentAsync(number);

        // Assert
        result.Error.Should().Be("Číslo dokladu nemůže být prázdné.");
        result.Number.Should().BeEmpty();
        result.IsRegistered.Should().BeFalse();
    }

    [Test]
    public async Task ValidateDocumentByDocumentTypeAsync_InvalidDocument_Success()
    {               
        // Act
        var result = await _documentValidateService.ValidateDocumentByDocumentTypeAsync(_number, _type, default);

        // Assert     
        result.Should().BeEquivalentTo(_expectedDocumentValidationResult);
    }

    [Test]
    public async Task ValidateDocumentByDocumentTypeAsync_ValidDocument_Success()
    {
        // Arrange 
        var number = "123456ABCD";        

        string xmlValidDocument = "<doklady_neplatne posl_zmena=\"12.8.2010\" pristi_zmeny=\"\">" +
                                     "<dotaz typ=\"OPs\" cislo=\"123456\" serie=\"ABCD\"/>" +
                                     "<odpoved aktualizovano=\"24.4.2024\" evidovano=\"ne\"/>" +
                                  "</doklady_neplatne>";

        var expectedResult = new DocumentValidationResult
        {
            Number = "123456",
            Series = "ABCD",
            Type = "občanský průkaz",
            IsRegistered = false,
            RegisteredFrom = string.Empty,
            BadRequest = false,
            Error = string.Empty,
        };
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(xmlValidDocument) });

        // Act
        var result = await _documentValidateService.ValidateDocumentByDocumentTypeAsync(number, _type, default);

        // Assert     
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public async Task ValidateDocumentByDocumentTypeAsync_EmptyBaseUrl_ThrowsArgumentException()
    {
        // Arrange
        _mockConfiguration.Setup(config => config["BaseUrlApiMvcr"]).Returns(string.Empty);

        // Act
        var resultAction = () => _documentValidateService.ValidateDocumentByDocumentTypeAsync(_number, _type, default);

        // Assert
        await resultAction.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    [TestCase("")]
    [TestCase(null!)]
    public async Task ValidateDocumentByDocumentTypeAsync_DocumentNumberNullOrEmpty_ThrowsArgumentException(string number)
    {
        // Act
        var resultAction = () => _documentValidateService.ValidateDocumentByDocumentTypeAsync(number, _type, default);

        // Assert
        await resultAction.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    [TestCase("123456ABCD", true, "")]
    [TestCase("", false, "Číslo dokladu nemůže být prázdné.")]
    [TestCase(null!, false, "Číslo dokladu nemůže být prázdné.")]
    [TestCase("123456789ABCD", false, "Číslo dokladu může obsahovat max. 10 znaků. ")]
    [TestCase("1234*56", false, "Číslo dokladu obsahuje neplatné znaky.")]
    public void IsValidDocumentNumber_Success(string number, bool expectedResult, string expectedError)
    {
        // Act
        var isValid = EndpointsValidationsRegistrationExtensions.IsValidDocumentNumber(number, out string error);

        // Assert     
        isValid.Should().Be(expectedResult);
        error.Should().Be(expectedError);
    }

    [Test]
    public void RemoveWhiteSpace_Success()
    {
        // Arrange 
        string text = " 1 2 3 4 A B C D ";

        // Act
        var cleanedText = EndpointsValidationsRegistrationExtensions.RemoveWhiteSpace(text);

        // Assert     
        cleanedText.Should().Be("1234ABCD");
    }

    [Test]
    public void RemoveWhiteSpace_TextIsNull_ThrowsArgumentNullException()
    {
        // Act 
        var resultAction = () => EndpointsValidationsRegistrationExtensions.RemoveWhiteSpace(null!);

        // Assert
        resultAction.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void DeserializeXmlToInvalidDocument_Success()
    {        
        // Act
        var deserializedDocument = _documentValidateService.DeserializeXmlToInvalidDocument(_xmlInvalidDocument);

        // Assert     
        deserializedDocument.Should().BeEquivalentTo(_expectedInvalidDocument);
    }

    [Test]
    public void DeserializeXmlToInvalidDocument_XmlIsNull_ArgumentNullException()
    {
        // Act 
        var resultAction = () => _documentValidateService.DeserializeXmlToInvalidDocument(null!);

        // Assert
        resultAction.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void DeserializeXmlToInvalidDocument_XmlIsInvalid_ThrowsInvalidOperationException()
    {
        // Arrange 
        string xml = "<invalid_xml></invalid_xml>";

        // Act 
        var resultAction = () => _documentValidateService.DeserializeXmlToInvalidDocument(xml);

        // Assert
        resultAction.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void MapInvalidDocumentToDocumentValidationResult_Success()
    {   
        // Act
        var result = _documentValidateService.MapInvalidDocumentToDocumentValidationResult(_expectedInvalidDocument);

        // Assert     
        result.Should().BeEquivalentTo(_expectedDocumentValidationResult);
    }

    [Test]
    public void MapInvalidDocumentToDocumentValidationResult_InvalidDocumentIsNull_ThrowsArgumentNullException()
    {
        // Act 
        var resultAction = () => _documentValidateService.MapInvalidDocumentToDocumentValidationResult(null!);

        // Assert
        resultAction.Should().Throw<ArgumentNullException>();
    }

    [Test]
    [TestCase("OP", "občanský průkaz")]
    [TestCase("OPs", "občanský průkaz")]
    [TestCase("CD", "cestovní pas")]
    [TestCase("CDj", "cestovní pas")]
    [TestCase("ZP", "zbrojní průkaz / zbrojní licence")]
    [TestCase("", "")]
    public void GetDocumentType_Success(string input, string expectedOutput)
    {
        // Act
        var type = _documentValidateService.GetDocumentType(input);

        // Assert     
        type.Should().Be(expectedOutput);
    }

    [Test]
    public void GetDocumentType_TypeIsNull_ThrowsArgumentNullException()
    {
        // Act 
        var resultAction = () => _documentValidateService.GetDocumentType(null!);

        // Assert
        resultAction.Should().Throw<ArgumentNullException>();
    }
}