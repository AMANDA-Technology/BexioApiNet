/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using BexioApiNet.Abstractions.Models.Files.DocumentTemplates.Enums;
using BexioApiNet.Services.Connectors.Files;

namespace BexioApiNet.IntegrationTests.Files;

/// <summary>
///     Integration tests covering <see cref="DocumentTemplateService" />. The request path is
///     composed from <see cref="DocumentTemplateConfiguration" /> (<c>3.0/document_templates</c>)
///     and must reach WireMock intact when the service is driven through the real connection
///     handler. The response body matches the v3 Bexio OpenAPI <c>ListDocumentTemplate</c> schema
///     exactly so deserialization of every documented field — including the
///     <c>default_for_document_types</c> enum array — can be asserted end-to-end.
/// </summary>
public sealed class DocumentTemplateServiceIntegrationTests : IntegrationTestBase
{
    private const string DocumentTemplatesPath = "/3.0/document_templates";

    /// <summary>
    ///     Fully populated v3 Bexio <c>ListDocumentTemplate</c> response body containing every
    ///     enum value of <c>default_for_document_types</c>, mirroring the example in the OpenAPI
    ///     spec. Designed to exercise the deserializer on the entire schema surface.
    /// </summary>
    private const string DocumentTemplatesResponse = """
                                                     [
                                                         {
                                                             "template_slug": "5f118cbc200a0c76ef1f34b2",
                                                             "name": "Standard template",
                                                             "is_default": true,
                                                             "default_for_document_types": [
                                                                 "type_offer",
                                                                 "type_order",
                                                                 "type_invoice",
                                                                 "type_delivery",
                                                                 "type_credit_voucher",
                                                                 "type_account_statement",
                                                                 "type_article_order"
                                                             ]
                                                         }
                                                     ]
                                                     """;

    /// <summary>
    ///     <c>DocumentTemplateService.Get()</c> must issue a <c>GET</c> against
    ///     <c>/3.0/document_templates</c> and deserialize every documented field of the
    ///     <c>ListDocumentTemplate</c> schema, including all seven document-type enum values.
    /// </summary>
    [Test]
    public async Task DocumentTemplateService_Get_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(DocumentTemplatesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(DocumentTemplatesResponse));

        var service = new DocumentTemplateService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;
        var template = result.Data!.Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(DocumentTemplatesPath));
            Assert.That(template.TemplateSlug, Is.EqualTo("5f118cbc200a0c76ef1f34b2"));
            Assert.That(template.Name, Is.EqualTo("Standard template"));
            Assert.That(template.IsDefault, Is.True);
            Assert.That(template.DefaultForDocumentTypes, Is.EqualTo(new[]
            {
                DocumentTemplateDocumentType.type_offer,
                DocumentTemplateDocumentType.type_order,
                DocumentTemplateDocumentType.type_invoice,
                DocumentTemplateDocumentType.type_delivery,
                DocumentTemplateDocumentType.type_credit_voucher,
                DocumentTemplateDocumentType.type_account_statement,
                DocumentTemplateDocumentType.type_article_order
            }));
        });
    }

    /// <summary>
    ///     When Bexio responds with an empty array, <c>DocumentTemplateService.Get()</c> must
    ///     surface an empty collection on a successful <c>ApiResult</c>.
    /// </summary>
    [Test]
    public async Task DocumentTemplateService_Get_WithEmptyResponse_ReturnsEmptyCollection()
    {
        Server
            .Given(Request.Create().WithPath(DocumentTemplatesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new DocumentTemplateService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
        });
    }

    /// <summary>
    ///     A non-default template (<c>is_default = false</c>, no
    ///     <c>default_for_document_types</c>) must round-trip with an empty enum array — the
    ///     spec documents that field as a simple array, not nullable, so an empty list is the
    ///     valid representation.
    /// </summary>
    [Test]
    public async Task DocumentTemplateService_Get_NonDefaultTemplate_DeserializesEmptyEnumArray()
    {
        const string body = """
                            [
                                {
                                    "template_slug": "abc-123",
                                    "name": "Custom",
                                    "is_default": false,
                                    "default_for_document_types": []
                                }
                            ]
                            """;
        Server
            .Given(Request.Create().WithPath(DocumentTemplatesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(body));

        var service = new DocumentTemplateService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        var template = result.Data!.Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(template.TemplateSlug, Is.EqualTo("abc-123"));
            Assert.That(template.Name, Is.EqualTo("Custom"));
            Assert.That(template.IsDefault, Is.False);
            Assert.That(template.DefaultForDocumentTypes, Is.Empty);
        });
    }
}
