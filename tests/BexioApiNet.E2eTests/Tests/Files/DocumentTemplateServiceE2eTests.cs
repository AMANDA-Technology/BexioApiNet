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

namespace BexioApiNet.E2eTests.Tests.Files;

/// <summary>
///     Live E2E coverage for <c>GET /3.0/document_templates</c>. Read-only endpoint, so no
///     create/update/delete lifecycle is exercised. Skipped automatically when Bexio credentials
///     are missing via <see cref="BexioE2eTestBase" />.
/// </summary>
public class DocumentTemplateServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    ///     Retrieves the document template list from the live Bexio API and asserts the call is
    ///     successful and returns at least one template (Bexio always seeds a default template per
    ///     tenant). Performs structural assertions against the OpenAPI <c>ListDocumentTemplate</c>
    ///     schema — every field must be present and well-typed.
    /// </summary>
    [Test]
    public async Task GetAll_StructurallyMatchesOpenApiSchema()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.DocumentTemplates.Get();

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data, Is.Not.Empty, "Bexio should expose at least one document template per tenant.");
        });

        var template = res.Data!.First();
        Assert.Multiple(() =>
        {
            Assert.That(template.TemplateSlug, Is.Not.Null.And.Not.Empty);
            Assert.That(template.Name, Is.Not.Null.And.Not.Empty);
            Assert.That(template.DefaultForDocumentTypes, Is.Not.Null);
        });
    }
}
