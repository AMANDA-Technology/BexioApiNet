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

namespace BexioApiNet.IntegrationTests.Infrastructure;

/// <summary>
/// Verifies that <see cref="BexioConnectionHandler"/> maps HTTP response status codes to
/// <see cref="Abstractions.Models.Api.ApiResult{T}"/> exactly per the documented contract:
/// 2xx are success, everything else (including 302 Found, 4xx and 5xx) is a failure
/// surfaced via <see cref="Abstractions.Models.Api.ApiError"/> rather than thrown exceptions.
/// Auto-redirect is disabled on the underlying <see cref="HttpClient"/>, so 3xx responses are
/// never followed — that would risk leaking the bearer token to a different host.
/// </summary>
public sealed class ErrorResponseTests : IntegrationTestBase
{
    private const string ErrorBody = "{\"error_code\":0,\"message\":\"error\",\"errors\":[]}";

    /// <summary>
    /// A <c>400 Bad Request</c> with a Bexio-style error body is surfaced as
    /// <see cref="Abstractions.Models.Api.ApiResult.IsSuccess"/>=<c>false</c> with a populated
    /// <see cref="Abstractions.Models.Api.ApiResult.ApiError"/> and <see cref="Abstractions.Models.Api.ApiResult{T}.Data"/>=<c>null</c>.
    /// </summary>
    [Test]
    public async Task GetAsync_On400BadRequest_ReturnsIsSuccessFalse_WithApiError()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithBody("{\"error_code\":400,\"message\":\"bad request\",\"errors\":[]}")
                .WithHeader("Content-Type", "application/json"));

        var result = await ConnectionHandler.GetAsync<List<object>>("2.0/accounts", null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ApiError, Is.Not.Null);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    /// <summary>
    /// A <c>401 Unauthorized</c> response is surfaced as a non-successful <see cref="Abstractions.Models.Api.ApiResult{T}"/>.
    /// </summary>
    [Test]
    public async Task GetAsync_On401Unauthorized_ReturnsIsSuccessFalse()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(401)
                .WithBody(ErrorBody)
                .WithHeader("Content-Type", "application/json"));

        var result = await ConnectionHandler.GetAsync<List<object>>("2.0/accounts", null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    /// <summary>
    /// A <c>404 Not Found</c> response is surfaced as a non-successful <see cref="Abstractions.Models.Api.ApiResult{T}"/>.
    /// </summary>
    [Test]
    public async Task GetAsync_On404NotFound_ReturnsIsSuccessFalse()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithBody(ErrorBody)
                .WithHeader("Content-Type", "application/json"));

        var result = await ConnectionHandler.GetAsync<List<object>>("2.0/accounts", null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    /// <summary>
    /// A <c>500 Internal Server Error</c> is surfaced as a non-successful <see cref="Abstractions.Models.Api.ApiResult{T}"/>.
    /// </summary>
    [Test]
    public async Task GetAsync_On500InternalServerError_ReturnsIsSuccessFalse()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(500)
                .WithBody(ErrorBody)
                .WithHeader("Content-Type", "application/json"));

        var result = await ConnectionHandler.GetAsync<List<object>>("2.0/accounts", null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        });
    }

    /// <summary>
    /// A <c>302 Found</c> response is treated as non-successful by <see cref="BexioConnectionHandler"/>.
    /// Auto-redirect is disabled on the underlying <see cref="HttpClient"/> to avoid leaking the bearer
    /// token to a different host, and the redirect target carries no JSON body, so <see cref="Abstractions.Models.Api.ApiResult{T}.Data"/>
    /// is <c>null</c> and the caller must inspect <see cref="Abstractions.Models.Api.ApiResult.StatusCode"/>
    /// to react to the redirect.
    /// </summary>
    [Test]
    public async Task GetAsync_On302Found_ReturnsIsSuccessFalse_WithNullData()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(302));

        var result = await ConnectionHandler.GetAsync<List<object>>("2.0/accounts", null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Found));
        });
    }

    /// <summary>
    /// A <c>200 OK</c> with a JSON body yields <see cref="Abstractions.Models.Api.ApiResult.IsSuccess"/>=<c>true</c>
    /// and deserialized <see cref="Abstractions.Models.Api.ApiResult{T}.Data"/>.
    /// </summary>
    [Test]
    public async Task GetAsync_On200_ReturnsIsSuccessTrue_WithDeserializedData()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("[{\"id\":1},{\"id\":2}]")
                .WithHeader("Content-Type", "application/json"));

        var result = await ConnectionHandler.GetAsync<List<Dictionary<string, int>>>("2.0/accounts", null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }
}
