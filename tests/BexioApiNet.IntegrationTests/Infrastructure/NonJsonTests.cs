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

namespace BexioApiNet.IntegrationTests.Infrastructure;

/// <summary>
/// Verifies <see cref="BexioConnectionHandler"/> behaviour when the remote server returns
/// non-JSON content (empty bodies, plain text, raw boolean literals). These cases are
/// important because the live Bexio API legitimately returns non-JSON for some endpoints
/// — most notably a raw <c>true</c> literal for successful <c>DELETE</c> calls.
/// </summary>
public sealed class NonJsonTests : IntegrationTestBase
{
    /// <summary>
    /// A <c>200 OK</c> with an empty body is surfaced without the connection handler
    /// throwing — the <see cref="Abstractions.Models.Api.ApiResult.IsSuccess"/> flag still
    /// reflects the HTTP status code.
    /// </summary>
    [Test]
    public async Task GetAsync_WhenResponseBodyIsEmpty_DoesNotThrow()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(""));

        var result = await ConnectionHandler.GetAsync<List<object>>("2.0/accounts", null, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
    }

    /// <summary>
    /// A <c>200 OK</c> with a plain-text body that is not valid JSON is surfaced without
    /// the connection handler throwing. <c>Data</c> may be <c>null</c> or <c>default</c>
    /// in that case — deserialisation failures should not bubble up as unhandled exceptions.
    /// </summary>
    [Test]
    public async Task GetAsync_WhenResponseBodyIsPlainText_DoesNotThrow()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("plain text not json"));

        Assert.DoesNotThrowAsync(async () =>
            await ConnectionHandler.GetAsync<List<object>>("2.0/accounts", null, CancellationToken.None));
    }

    /// <summary>
    /// A <c>200 OK</c> with the raw body <c>true</c> (what Bexio returns for a successful
    /// <c>DELETE</c>) is surfaced as <see cref="Abstractions.Models.Api.ApiResult.IsSuccess"/>=<c>true</c>
    /// via <see cref="BexioConnectionHandler.Delete"/>.
    /// </summary>
    [Test]
    public async Task Delete_On200_ReturnsIsSuccessTrue()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts/1").UsingDelete())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("true")
                .WithHeader("Content-Type", "application/json"));

        var result = await ConnectionHandler.Delete("2.0/accounts/1", CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
    }
}
