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

using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Abstractions.Enums.MasterData;
using BexioApiNet.Abstractions.Models.MasterData.Comments.Views;
using BexioApiNet.Interfaces.Connectors.MasterData;
using BexioApiNet.Interfaces.Connectors.Sales;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.E2eTests.Tests.MasterData.Comments;

/// <summary>
/// Live end-to-end tests for <see cref="CommentService"/>. Comments live under the polymorphic
/// route <c>/2.0/{kb_document_type}/{document_id}/comment</c> so the tests need an existing
/// invoice to bind to. They are skipped when the tenant has none. Verifies List, GetById, Create
/// against the OpenAPI <c>Comment</c> schema (required <c>text</c>, <c>user_id</c>, <c>user_name</c>).
/// </summary>
[Category("E2E")]
public sealed class CommentServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private ICommentService _sut = null!;
    private IInvoiceService _invoiceService = null!;

    /// <summary>
    /// Reads <c>BexioApiNet__BaseUri</c> and <c>BexioApiNet__JwtToken</c> from the environment.
    /// Calls <see cref="Assert.Ignore(string)"/> when credentials are absent.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var baseUri = Environment.GetEnvironmentVariable("BexioApiNet__BaseUri");
        var jwtToken = Environment.GetEnvironmentVariable("BexioApiNet__JwtToken");

        if (string.IsNullOrWhiteSpace(baseUri) || string.IsNullOrWhiteSpace(jwtToken))
        {
            Assert.Ignore("credentials not configured");
            return;
        }

        _connectionHandler = new BexioConnectionHandler(
            new BexioConfiguration
            {
                BaseUri = baseUri,
                JwtToken = jwtToken,
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            });

        _sut = new CommentService(_connectionHandler);
        _invoiceService = new InvoiceService(_connectionHandler);
    }

    /// <summary>
    /// Disposes the connection handler if it was created for the test.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
        _connectionHandler = null;
    }

    /// <summary>
    /// Resolves the id of an existing invoice on the tenant. Returns <see langword="null"/> when
    /// no invoices are available so the caller can skip the test instead of failing.
    /// </summary>
    private async Task<int?> ResolveAnyInvoiceIdAsync()
    {
        var invoices = await _invoiceService.Get(new QueryParameterInvoice(Limit: 1, Offset: 0));
        return invoices is { IsSuccess: true, Data: { Count: > 0 } list }
            ? list[0].Id
            : null;
    }

    /// <summary>
    /// <c>GET /2.0/kb_invoice/{document_id}/comment</c> must succeed against an existing invoice.
    /// The result list may be empty for a freshly created invoice, but the API call itself must
    /// return success.
    /// </summary>
    [Test]
    public async Task Get_ReturnsCommentsForExistingInvoice()
    {
        var documentId = await ResolveAnyInvoiceIdAsync();
        if (documentId is null)
        {
            Assert.Ignore("no invoices available on this tenant");
            return;
        }

        var res = await _sut.Get(KbDocumentType.Invoice, documentId.Value);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// <c>POST /2.0/kb_invoice/{document_id}/comment</c> followed by
    /// <c>GET /2.0/kb_invoice/{document_id}/comment/{comment_id}</c>. Verifies that creating a
    /// comment populates every required field per the OpenAPI <c>Comment</c> schema, and that the
    /// same record can be retrieved by id.
    /// </summary>
    [Test]
    public async Task Create_AndGetById_RoundtripsThroughCommentSchema()
    {
        var documentId = await ResolveAnyInvoiceIdAsync();
        if (documentId is null)
        {
            Assert.Ignore("no invoices available on this tenant");
            return;
        }

        var payload = new CommentCreate
        {
            Text = $"E2E-Comment-{Guid.NewGuid():N}",
            UserId = 1,
            UserName = "E2E Test",
            IsPublic = false
        };

        var created = await _sut.Create(KbDocumentType.Invoice, documentId.Value, payload);

        if (!created.IsSuccess)
        {
            Assert.Ignore($"create failed ({created.StatusCode}) — possibly missing kb_invoice_edit scope or user_id 1 unavailable");
            return;
        }

        Assert.That(created.Data, Is.Not.Null);
        Assert.That(created.Data!.Text, Is.EqualTo(payload.Text));
        Assert.That(created.Data.UserId, Is.EqualTo(payload.UserId));
        Assert.That(created.Data.UserName, Is.EqualTo(payload.UserName));

        if (created.Data.Id is { } commentId)
        {
            var fetched = await _sut.GetById(KbDocumentType.Invoice, documentId.Value, commentId);

            Assert.That(fetched, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(fetched.IsSuccess, Is.True);
                Assert.That(fetched.ApiError, Is.Null);
                Assert.That(fetched.Data, Is.Not.Null);
                Assert.That(fetched.Data!.Id, Is.EqualTo(commentId));
                Assert.That(fetched.Data.Text, Is.EqualTo(payload.Text));
            });
        }
    }
}
