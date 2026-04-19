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

using System.Net;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;

namespace BexioApiNet.IntegrationTests.Infrastructure;

/// <summary>
/// Verifies the additional HTTP verb extensions introduced for Phase 0 API coverage
/// (PUT, PATCH, POST action, binary GET, POST search, POST bulk) against a real WireMock
/// server. Each test drives the real <see cref="BexioConnectionHandler"/> end-to-end so
/// URL composition, verb selection, body serialization and response decoding are covered
/// for the concrete network stack — not just the in-process stub <see cref="HttpMessageHandler"/>.
/// </summary>
public sealed class HttpVerbExtensionTests : IntegrationTestBase
{
    /// <summary>
    /// Simple response payload used for successful responses that return a single JSON object.
    /// </summary>
    /// <param name="Id">Identifier echoed back by the WireMock stub.</param>
    /// <param name="Name">Name echoed back by the WireMock stub.</param>
    private sealed record TestItem(int Id, string Name);

    /// <summary>
    /// Simple request payload used by PUT / PATCH / search / bulk tests to verify body
    /// serialization on the wire.
    /// </summary>
    /// <param name="Name">A name-like field that lets the body be recognised in the request log.</param>
    /// <param name="Value">A numeric field serialised alongside <c>Name</c>.</param>
    private sealed record TestPayload(string Name, int Value);

    /// <summary>
    /// <see cref="BexioConnectionHandler.PutAsync{TResult,TUpdate}"/> must send an HTTP <c>PUT</c>
    /// with the serialised payload as the request body, and surface the server's JSON response
    /// as a successful <see cref="ApiResult{T}"/>.
    /// </summary>
    [Test]
    public async Task PutAsync_SendsPutRequest_WithJsonBody()
    {
        const string path = "/3.0/accounting/manual_entries/7";

        Server
            .Given(Request.Create().WithPath(path).UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Id\":7,\"Name\":\"updated\"}")
                .WithHeader("Content-Type", "application/json"));

        var result = await ConnectionHandler.PutAsync<TestItem, TestPayload>(
            new TestPayload("updated", 42),
            "3.0/accounting/manual_entries/7",
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(7));
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
            Assert.That(request.Body, Does.Contain("\"Name\":\"updated\""));
            Assert.That(request.Body, Does.Contain("\"Value\":42"));
        });
    }

    /// <summary>
    /// <see cref="BexioConnectionHandler.PatchAsync{TResult,TPatch}"/> must send an HTTP
    /// <c>PATCH</c> request with the serialised payload as the request body.
    /// </summary>
    [Test]
    public async Task PatchAsync_SendsPatchRequest_WithJsonBody()
    {
        const string path = "/3.0/currencies/3";

        Server
            .Given(Request.Create().WithPath(path).UsingPatch())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Id\":3,\"Name\":\"patched\"}")
                .WithHeader("Content-Type", "application/json"));

        var result = await ConnectionHandler.PatchAsync<TestItem, TestPayload>(
            new TestPayload("patched", 1),
            "3.0/currencies/3",
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Name, Is.EqualTo("patched"));
            Assert.That(request.Method, Is.EqualTo("PATCH"));
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
            Assert.That(request.Body, Does.Contain("\"Name\":\"patched\""));
        });
    }

    /// <summary>
    /// The typed overload <see cref="BexioConnectionHandler.PostActionAsync{TResult}"/> must
    /// send an HTTP <c>POST</c> with no body and deserialise the response into the requested type.
    /// </summary>
    [Test]
    public async Task PostActionAsync_WithTResult_SendsPostRequest_WithoutBody()
    {
        const string path = "/2.0/kb_invoice/42/issue";

        Server
            .Given(Request.Create().WithPath(path).UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"Id\":42,\"Name\":\"issued\"}")
                .WithHeader("Content-Type", "application/json"));

        var result = await ConnectionHandler.PostActionAsync<TestItem>(
            "2.0/kb_invoice/42/issue",
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(42));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
            Assert.That(request.Body, Is.Null.Or.Empty);
        });
    }

    /// <summary>
    /// The void overload <see cref="BexioConnectionHandler.PostActionAsync"/> must send an HTTP
    /// <c>POST</c> with no body and return a non-typed successful <see cref="ApiResult{T}"/>.
    /// </summary>
    [Test]
    public async Task PostActionAsync_WithVoid_SendsPostRequest_WithoutBody()
    {
        const string path = "/2.0/kb_invoice/42/mark_as_sent";

        Server
            .Given(Request.Create().WithPath(path).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(204));

        var result = await ConnectionHandler.PostActionAsync(
            "2.0/kb_invoice/42/mark_as_sent",
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
            Assert.That(request.Body, Is.Null.Or.Empty);
        });
    }

    /// <summary>
    /// <see cref="BexioConnectionHandler.GetBinaryAsync"/> must send an HTTP <c>GET</c> and
    /// return the raw response bytes verbatim — no JSON deserialisation is attempted so a
    /// non-JSON binary payload (a PDF, for example) is preserved byte-for-byte.
    /// </summary>
    [Test]
    public async Task GetBinaryAsync_SendsGetRequest_AndReturnsRawBytes()
    {
        const string path = "/2.0/kb_invoice/42/pdf";
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x37 };

        Server
            .Given(Request.Create().WithPath(path).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(pdfHeader)
                .WithHeader("Content-Type", "application/pdf"));

        var result = await ConnectionHandler.GetBinaryAsync(
            "2.0/kb_invoice/42/pdf",
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!, Is.EqualTo(pdfHeader));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
        });
    }

    /// <summary>
    /// An error response to a binary download must be surfaced as a non-successful
    /// <see cref="ApiResult{T}"/> with <c>Data</c>=<c>null</c> and the Bexio error body
    /// deserialised into <see cref="ApiResult.ApiError"/>.
    /// </summary>
    [Test]
    public async Task GetBinaryAsync_On404_ReturnsIsSuccessFalse_WithApiError()
    {
        const string path = "/2.0/kb_invoice/999/pdf";

        Server
            .Given(Request.Create().WithPath(path).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithBody("{\"error_code\":404,\"message\":\"missing\",\"errors\":[]}")
                .WithHeader("Content-Type", "application/json"));

        var result = await ConnectionHandler.GetBinaryAsync(
            "2.0/kb_invoice/999/pdf",
            TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(result.Data, Is.Null);
            Assert.That(result.ApiError, Is.Not.Null);
            Assert.That(result.ApiError!.Message, Is.EqualTo("missing"));
        });
    }

    /// <summary>
    /// <see cref="BexioConnectionHandler.PostSearchAsync{TResult}"/> must send an HTTP
    /// <c>POST</c> with a JSON array body of <see cref="SearchCriteria"/> and append the
    /// supplied <see cref="QueryParameter"/> values to the request URI as a query string.
    /// </summary>
    [Test]
    public async Task PostSearchAsync_SendsPostRequest_WithBodyAndQueryParameters()
    {
        const string path = "/2.0/contact/search";

        Server
            .Given(Request.Create().WithPath(path).UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("[{\"Id\":1,\"Name\":\"Acme\"}]")
                .WithHeader("Content-Type", "application/json"));

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Acme", Criteria = "like" }
        };
        var queryParameter = new QueryParameter(new Dictionary<string, object>
        {
            ["limit"] = 25,
            ["offset"] = 50
        });

        var result = await ConnectionHandler.PostSearchAsync<TestItem>(
            criteria,
            "2.0/contact/search",
            queryParameter,
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;
        var rawQuery = request.RawQuery ?? string.Empty;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Name, Is.EqualTo("Acme"));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
            Assert.That(rawQuery, Does.Contain("limit=25"));
            Assert.That(rawQuery, Does.Contain("offset=50"));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"Acme\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
        });
    }

    /// <summary>
    /// When no <see cref="QueryParameter"/> is supplied to <see cref="BexioConnectionHandler.PostSearchAsync{TResult}"/>
    /// the request URI must not carry a query string.
    /// </summary>
    [Test]
    public async Task PostSearchAsync_WithoutQueryParameter_HasNoQueryString()
    {
        const string path = "/2.0/contact/search";

        Server
            .Given(Request.Create().WithPath(path).UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("[]")
                .WithHeader("Content-Type", "application/json"));

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Acme", Criteria = "=" }
        };

        var result = await ConnectionHandler.PostSearchAsync<TestItem>(
            criteria,
            "2.0/contact/search",
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
            Assert.That(request.RawQuery, Is.Null.Or.Empty);
        });
    }

    /// <summary>
    /// <see cref="BexioConnectionHandler.PostBulkAsync{TResult,TCreate}"/> must send an HTTP
    /// <c>POST</c> with a JSON array body carrying every supplied payload and deserialise the
    /// server's list response.
    /// </summary>
    [Test]
    public async Task PostBulkAsync_SendsPostRequest_WithJsonArrayBody()
    {
        const string path = "/2.0/contact/_bulk_create";

        Server
            .Given(Request.Create().WithPath(path).UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("[{\"Id\":1,\"Name\":\"first\"},{\"Id\":2,\"Name\":\"second\"}]")
                .WithHeader("Content-Type", "application/json"));

        var payloads = new List<TestPayload>
        {
            new("first", 1),
            new("second", 2)
        };

        var result = await ConnectionHandler.PostBulkAsync<TestItem, TestPayload>(
            payloads,
            "2.0/contact/_bulk_create",
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![1].Id, Is.EqualTo(2));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
            Assert.That(request.Body, Does.StartWith("["));
            Assert.That(request.Body, Does.EndWith("]"));
            Assert.That(request.Body, Does.Contain("\"Name\":\"first\""));
            Assert.That(request.Body, Does.Contain("\"Name\":\"second\""));
        });
    }
}
