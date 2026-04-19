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
using System.Net.Http.Headers;
using System.Text;
using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;

namespace BexioApiNet.UnitTests.Infrastructure;

/// <summary>
///     Unit tests for <see cref="BexioConnectionHandler" />. These tests exercise the real
///     implementation with a hand-rolled stub <see cref="HttpMessageHandler" /> so no network
///     traffic occurs. They cover request construction, query-parameter serialization,
///     <see cref="ApiResult{T}" /> mapping for success/error/redirect responses, response
///     header extraction, cancellation propagation and the <see cref="IDisposable" /> contract
///     including the owned-vs-borrowed <see cref="HttpClient" /> branching.
/// </summary>
[TestFixture]
[Category("Unit")]
public class BexioConnectionHandlerTests
{
    private const string BaseUri = "https://api.example.local/";

    /// <summary>
    ///     Fake payload record used for JSON serialization / deserialization assertions.
    /// </summary>
    /// <param name="Id">Arbitrary identifier.</param>
    private sealed record TestItem(int Id);

    /// <summary>
    ///     Minimal request body used by the POST serialization test.
    /// </summary>
    /// <param name="Name">A name-like field.</param>
    /// <param name="Value">A numeric field.</param>
    private sealed record TestPayload(string Name, int Value);

    /// <summary>
    ///     Test double for <see cref="HttpMessageHandler" />. Captures the last request, honours
    ///     the supplied cancellation token (so cancellation tests propagate correctly) and returns
    ///     a configurable <see cref="HttpResponseMessage" />.
    /// </summary>
    private sealed class StubHandler : HttpMessageHandler
    {
        public HttpRequestMessage? CapturedRequest { get; private set; }

        public string? CapturedRequestBody { get; private set; }

        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CapturedRequest = request;

            if (request.Content is not null)
                CapturedRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            return Response;
        }
    }

    /// <summary>
    ///     Builds a <see cref="BexioConnectionHandler" /> wired to the supplied stub message handler
    ///     via an externally managed <see cref="HttpClient" />. Uses the DI-style two-argument
    ///     constructor so disposal of the returned handler does not dispose the HTTP client.
    /// </summary>
    private static (BexioConnectionHandler handler, HttpClient httpClient, StubHandler stub) CreateHandler()
    {
        var stub = new StubHandler();
        var httpClient = new HttpClient(stub) { BaseAddress = new Uri(BaseUri) };
        var configuration = new BexioConfiguration
        {
            BaseUri = BaseUri,
            JwtToken = "test-token",
            AcceptHeaderFormat = "application/json"
        };
        var handler = new BexioConnectionHandler(httpClient, configuration);
        return (handler, httpClient, stub);
    }

    // -----------------------------------------------------------------------
    // A. Request construction — HTTP verb and path
    // -----------------------------------------------------------------------

    /// <summary>
    ///     A GET call must emit an HTTP GET request to the path provided, joined with the configured
    ///     base URI.
    /// </summary>
    [Test]
    public async Task GetAsync_SendsGetRequest_ToCorrectPath()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            await handler.GetAsync<TestItem>("2.0/accounts", null, CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(stub.CapturedRequest.RequestUri, Is.Not.Null);
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath, Is.EqualTo("/2.0/accounts"));
                Assert.That(stub.CapturedRequest.RequestUri.ToString(), Is.EqualTo(BaseUri + "2.0/accounts"));
            });
        }
    }

    /// <summary>
    ///     A POST call must emit an HTTP POST request to the path provided.
    /// </summary>
    [Test]
    public async Task PostAsync_SendsPostRequest_ToCorrectPath()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            await handler.PostAsync<TestItem, TestPayload>(new TestPayload("x", 1), "2.0/accounts",
                CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath, Is.EqualTo("/2.0/accounts"));
            });
        }
    }

    /// <summary>
    ///     A Delete call must emit an HTTP DELETE request to the path provided.
    /// </summary>
    [Test]
    public async Task Delete_SendsDeleteRequest_ToCorrectPath()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            await handler.Delete("2.0/accounts/1", CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Delete));
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath, Is.EqualTo("/2.0/accounts/1"));
            });
        }
    }

    /// <summary>
    ///     Multipart uploads must emit an HTTP POST request whose content is a
    ///     <see cref="MultipartFormDataContent" /> instance.
    /// </summary>
    [Test]
    public async Task PostMultiPartFileAsync_SendsPostRequest()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            var files = new List<Tuple<MemoryStream, string>>
            {
                new(new MemoryStream(new byte[] { 1, 2, 3 }), "file.bin")
            };

            await handler.PostMultiPartFileAsync<TestItem>(files, "2.0/files", CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath, Is.EqualTo("/2.0/files"));
                Assert.That(stub.CapturedRequest.Content, Is.InstanceOf<MultipartFormDataContent>());
            });
        }
    }

    // -----------------------------------------------------------------------
    // B. Query parameter serialization
    // -----------------------------------------------------------------------

    /// <summary>
    ///     Parameters supplied via <see cref="QueryParameter" /> must be appended to the request URI
    ///     as a percent-encoded query string.
    /// </summary>
    [Test]
    public async Task GetAsync_WithQueryParameter_AppendsToUri()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            var queryParameter = new QueryParameter(new Dictionary<string, object>
            {
                ["offset"] = 10,
                ["limit"] = 50
            });

            await handler.GetAsync<TestItem>("2.0/accounts", queryParameter, CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            var query = stub.CapturedRequest!.RequestUri!.Query;
            Assert.Multiple(() =>
            {
                Assert.That(query, Does.StartWith("?"));
                Assert.That(query, Does.Contain("offset=10"));
                Assert.That(query, Does.Contain("limit=50"));
            });
        }
    }

    /// <summary>
    ///     When no <see cref="QueryParameter" /> is supplied the request URI must have no query string.
    /// </summary>
    [Test]
    public async Task GetAsync_WithNullQueryParameter_HasNoQueryString()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            await handler.GetAsync<TestItem>("2.0/accounts", null, CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.RequestUri!.Query, Is.Empty);
                Assert.That(stub.CapturedRequest.RequestUri.ToString(), Does.Not.Contain("?"));
            });
        }
    }

    // -----------------------------------------------------------------------
    // C. Request body serialization (POST)
    // -----------------------------------------------------------------------

    /// <summary>
    ///     POST must serialize the supplied payload to JSON using System.Text.Json defaults
    ///     (PascalCase property names) and send it as the request body.
    /// </summary>
    [Test]
    public async Task PostAsync_SerializesPayloadAsJson()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            var payload = new TestPayload("alpha", 42);

            await handler.PostAsync<TestItem, TestPayload>(payload, "2.0/items", CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.That(stub.CapturedRequest!.Content, Is.InstanceOf<StringContent>());
            Assert.That(stub.CapturedRequestBody, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"Name\":\"alpha\""));
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"Value\":42"));
                Assert.That(stub.CapturedRequest.Content!.Headers.ContentType?.MediaType,
                    Is.EqualTo("application/json"));
            });
        }
    }

    // -----------------------------------------------------------------------
    // D. ApiResult mapping
    // -----------------------------------------------------------------------

    /// <summary>
    ///     A 200 response must produce an <see cref="ApiResult{T}" /> with
    ///     <c>IsSuccess == true</c> and a deserialized payload.
    /// </summary>
    [Test]
    public async Task GetAsync_On200_ReturnsIsSuccessTrue_WithDeserializedData()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            stub.Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"Id\":1}]", Encoding.UTF8, "application/json")
            };

            var result = await handler.GetAsync<List<TestItem>>("2.0/items", null, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result.ApiError, Is.Null);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!, Has.Count.EqualTo(1));
                Assert.That(result.Data![0].Id, Is.EqualTo(1));
            });
        }
    }

    /// <summary>
    ///     A 400 response must produce an <see cref="ApiResult{T}" /> with
    ///     <c>IsSuccess == false</c>, a populated <see cref="ApiError" /> and null <c>Data</c>.
    /// </summary>
    [Test]
    public async Task GetAsync_On400_ReturnsIsSuccessFalse_WithApiError()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            stub.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error_code\":400,\"message\":\"bad\",\"errors\":{}}",
                    Encoding.UTF8, "application/json")
            };

            var result = await handler.GetAsync<TestItem>("2.0/items/1", null, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(result.Data, Is.Null);
                Assert.That(result.ApiError, Is.Not.Null);
                Assert.That(result.ApiError!.ErrorCode, Is.EqualTo(400));
                Assert.That(result.ApiError.Message, Is.EqualTo("bad"));
            });
        }
    }

    /// <summary>
    ///     A 302 Found response is non-successful: the status code is outside the 2xx range, so
    ///     <c>IsSuccess</c> is false and <c>Data</c> is null. The redirect target (<c>Location</c>
    ///     header) is not followed — auto-redirect is disabled on the underlying <see cref="HttpClient"/>
    ///     to prevent the bearer token from leaking to a different host.
    /// </summary>
    [Test]
    public async Task GetAsync_On302Found_ReturnsIsSuccessFalse_WithNullData()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            stub.Response = new HttpResponseMessage(HttpStatusCode.Found)
            {
                Content = new StringContent(string.Empty)
            };

            var result = await handler.GetAsync<TestItem>("2.0/items/1", null, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Found));
                Assert.That(result.Data, Is.Null);
                Assert.That(result.ApiError, Is.Null);
            });
        }
    }

    /// <summary>
    ///     Standard Bexio pagination headers on the response must be parsed into the
    ///     <see cref="ApiResult.ResponseHeaders" /> dictionary using the documented
    ///     <see cref="ApiHeaderNames" /> keys.
    /// </summary>
    [Test]
    public async Task GetAsync_ResponseHeaders_AreMappedCorrectly()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            };
            response.Headers.Add(ApiHeaderNames.RequestLimit, "50");
            response.Headers.Add(ApiHeaderNames.AppliedOffset, "0");
            response.Headers.Add(ApiHeaderNames.TotalResults, "100");
            stub.Response = response;

            var result = await handler.GetAsync<List<TestItem>>("2.0/items", null, CancellationToken.None);

            Assert.That(result.ResponseHeaders, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.ResponseHeaders![ApiHeaderNames.RequestLimit], Is.EqualTo(50));
                Assert.That(result.ResponseHeaders[ApiHeaderNames.AppliedOffset], Is.EqualTo(0));
                Assert.That(result.ResponseHeaders[ApiHeaderNames.TotalResults], Is.EqualTo(100));
            });
        }
    }

    // -----------------------------------------------------------------------
    // E. Dispose semantics
    // -----------------------------------------------------------------------

    /// <summary>
    ///     When constructed with a configuration only the handler owns its <see cref="HttpClient" />,
    ///     so disposing the handler must dispose the client. A subsequent HTTP call must therefore
    ///     fail with <see cref="ObjectDisposedException" />.
    /// </summary>
    [Test]
    public void Dispose_WhenOwnsHttpClient_DisposesClient()
    {
        var configuration = new BexioConfiguration
        {
            BaseUri = BaseUri,
            JwtToken = "test-token",
            AcceptHeaderFormat = "application/json"
        };

        var handler = new BexioConnectionHandler(configuration);
        handler.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await handler.GetAsync<TestItem>("2.0/accounts", null, CancellationToken.None));
    }

    /// <summary>
    ///     When constructed with an externally supplied <see cref="HttpClient" /> the handler must
    ///     leave the client alone on dispose so DI-managed clients keep working for their owner.
    /// </summary>
    [Test]
    public async Task Dispose_WhenDoesNotOwnHttpClient_DoesNotDisposeClient()
    {
        var (handler, httpClient, stub) = CreateHandler();

        handler.Dispose();

        Assert.That(httpClient.BaseAddress, Is.Not.Null);
        Assert.DoesNotThrowAsync(async () =>
            await httpClient.GetAsync(new Uri("2.0/accounts", UriKind.Relative)));
        Assert.That(stub.CapturedRequest, Is.Not.Null);

        await Task.CompletedTask;
        httpClient.Dispose();
    }

    // -----------------------------------------------------------------------
    // F. Cancellation
    // -----------------------------------------------------------------------

    /// <summary>
    ///     An already-cancelled <see cref="CancellationToken" /> must propagate through the handler
    ///     as an <see cref="OperationCanceledException" /> (or a derived <see cref="TaskCanceledException" />).
    /// </summary>
    [Test]
    public void GetAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        var (handler, httpClient, _) = CreateHandler();
        using (handler)
        using (httpClient)
        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            Assert.CatchAsync<OperationCanceledException>(async () =>
                await handler.GetAsync<TestItem>("2.0/accounts", null, cts.Token));
        }
    }

    // -----------------------------------------------------------------------
    // G. PUT / PATCH / action / binary / search / bulk
    // -----------------------------------------------------------------------

    /// <summary>
    ///     A PUT call must emit an HTTP PUT request to the path provided and carry the serialized
    ///     payload as JSON body.
    /// </summary>
    [Test]
    public async Task PutAsync_SendsPutRequest_ToCorrectPath()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            var payload = new TestPayload("alpha", 42);

            await handler.PutAsync<TestItem, TestPayload>(payload, "3.0/accounting/manual_entries/7",
                CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Put));
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath,
                    Is.EqualTo("/3.0/accounting/manual_entries/7"));
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"Name\":\"alpha\""));
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"Value\":42"));
                Assert.That(stub.CapturedRequest.Content!.Headers.ContentType?.MediaType,
                    Is.EqualTo("application/json"));
            });
        }
    }

    /// <summary>
    ///     A PATCH call must emit an HTTP PATCH request to the path provided and carry the
    ///     serialized payload as JSON body.
    /// </summary>
    [Test]
    public async Task PatchAsync_SendsPatchRequest_ToCorrectPath()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            var payload = new TestPayload("beta", 7);

            await handler.PatchAsync<TestItem, TestPayload>(payload, "3.0/currencies/3",
                CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Patch));
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath, Is.EqualTo("/3.0/currencies/3"));
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"Name\":\"beta\""));
                Assert.That(stub.CapturedRequest.Content!.Headers.ContentType?.MediaType,
                    Is.EqualTo("application/json"));
            });
        }
    }

    /// <summary>
    ///     The typed overload of <see cref="BexioConnectionHandler.PostActionAsync{TResult}" /> must
    ///     emit a POST with no body to the supplied action endpoint path.
    /// </summary>
    [Test]
    public async Task PostActionAsync_WithTResult_SendsPostRequest_ToCorrectPath()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            await handler.PostActionAsync<TestItem>("2.0/kb_invoice/42/issue", CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath, Is.EqualTo("/2.0/kb_invoice/42/issue"));
                Assert.That(stub.CapturedRequest.Content, Is.Null);
            });
        }
    }

    /// <summary>
    ///     The void overload of <see cref="BexioConnectionHandler.PostActionAsync" /> must emit a
    ///     POST with no body and surface success through the untyped <see cref="ApiResult{T}" />.
    /// </summary>
    [Test]
    public async Task PostActionAsync_WithVoid_SendsPostRequest_ToCorrectPath()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            var result = await handler.PostActionAsync("2.0/kb_invoice/42/mark_as_sent", CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath,
                    Is.EqualTo("/2.0/kb_invoice/42/mark_as_sent"));
                Assert.That(stub.CapturedRequest.Content, Is.Null);
                Assert.That(result.IsSuccess, Is.True);
            });
        }
    }

    /// <summary>
    ///     <see cref="BexioConnectionHandler.GetBinaryAsync" /> must emit a GET and return the raw
    ///     response bytes verbatim in <see cref="ApiResult{T}.Data" /> instead of attempting JSON
    ///     deserialisation.
    /// </summary>
    [Test]
    public async Task GetBinaryAsync_SendsGetRequest_AndReturnsByteArrayData()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x37 };
            stub.Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfBytes)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/pdf") }
                }
            };

            var result = await handler.GetBinaryAsync("2.0/kb_invoice/42/pdf", CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Get));
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath, Is.EqualTo("/2.0/kb_invoice/42/pdf"));
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!, Is.EqualTo(pdfBytes));
                Assert.That(result.ApiError, Is.Null);
            });
        }
    }

    /// <summary>
    ///     A non-success response from <see cref="BexioConnectionHandler.GetBinaryAsync" /> must
    ///     leave <see cref="ApiResult{T}.Data" /> null and populate <see cref="ApiResult.ApiError" />.
    /// </summary>
    [Test]
    public async Task GetBinaryAsync_On404_ReturnsIsSuccessFalse_WithApiError()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            stub.Response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{\"error_code\":404,\"message\":\"missing\",\"errors\":{}}",
                    Encoding.UTF8, "application/json")
            };

            var result = await handler.GetBinaryAsync("2.0/kb_invoice/999/pdf", CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                Assert.That(result.Data, Is.Null);
                Assert.That(result.ApiError, Is.Not.Null);
                Assert.That(result.ApiError!.Message, Is.EqualTo("missing"));
            });
        }
    }

    /// <summary>
    ///     <see cref="BexioConnectionHandler.PostSearchAsync{TResult}" /> must POST the search
    ///     criteria as a JSON array body and append supplied <see cref="QueryParameter" /> values
    ///     to the request URI as a query string.
    /// </summary>
    [Test]
    public async Task PostSearchAsync_SendsPostRequest_WithBodyAndQueryParameters()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            stub.Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            };

            var criteria = new List<SearchCriteria>
            {
                new() { Field = "name", Value = "Acme", Criteria = "like" },
                new() { Field = "email", Value = "info@example.com", Criteria = "=" }
            };
            var queryParameter = new QueryParameter(new Dictionary<string, object>
            {
                ["limit"] = 25,
                ["offset"] = 50
            });

            await handler.PostSearchAsync<TestItem>(criteria, "2.0/contact/search", queryParameter,
                CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath, Is.EqualTo("/2.0/contact/search"));
                Assert.That(stub.CapturedRequest.RequestUri.Query, Does.Contain("limit=25"));
                Assert.That(stub.CapturedRequest.RequestUri.Query, Does.Contain("offset=50"));
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"field\":\"name\""));
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"value\":\"Acme\""));
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"criteria\":\"like\""));
                Assert.That(stub.CapturedRequest.Content!.Headers.ContentType?.MediaType,
                    Is.EqualTo("application/json"));
            });
        }
    }

    /// <summary>
    ///     When no <see cref="QueryParameter" /> is supplied to <see cref="BexioConnectionHandler.PostSearchAsync{TResult}" />
    ///     the request URI must carry no query string.
    /// </summary>
    [Test]
    public async Task PostSearchAsync_WithoutQueryParameter_HasNoQueryString()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            stub.Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            };

            var criteria = new List<SearchCriteria>
            {
                new() { Field = "name", Value = "Acme", Criteria = "=" }
            };

            await handler.PostSearchAsync<TestItem>(criteria, "2.0/contact/search", null, CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.That(stub.CapturedRequest!.RequestUri!.Query, Is.Empty);
        }
    }

    /// <summary>
    ///     <see cref="BexioConnectionHandler.PostBulkAsync{TResult, TCreate}" /> must POST the full
    ///     payload list as a JSON array body to the supplied bulk endpoint path.
    /// </summary>
    [Test]
    public async Task PostBulkAsync_SendsPostRequest_WithArrayBody()
    {
        var (handler, httpClient, stub) = CreateHandler();
        using (handler)
        using (httpClient)
        {
            var payloads = new List<TestPayload>
            {
                new("first", 1),
                new("second", 2),
                new("third", 3)
            };

            await handler.PostBulkAsync<TestItem, TestPayload>(payloads, "2.0/contact/_bulk_create",
                CancellationToken.None);

            Assert.That(stub.CapturedRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(stub.CapturedRequest!.Method, Is.EqualTo(HttpMethod.Post));
                Assert.That(stub.CapturedRequest.RequestUri!.AbsolutePath, Is.EqualTo("/2.0/contact/_bulk_create"));
                Assert.That(stub.CapturedRequestBody, Does.StartWith("["));
                Assert.That(stub.CapturedRequestBody, Does.EndWith("]"));
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"Name\":\"first\""));
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"Name\":\"second\""));
                Assert.That(stub.CapturedRequestBody, Does.Contain("\"Name\":\"third\""));
                Assert.That(stub.CapturedRequest.Content!.Headers.ContentType?.MediaType,
                    Is.EqualTo("application/json"));
            });
        }
    }
}
