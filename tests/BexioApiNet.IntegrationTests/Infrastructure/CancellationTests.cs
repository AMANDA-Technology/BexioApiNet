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
/// Verifies that the <see cref="BexioConnectionHandler"/> honours a caller-supplied
/// <see cref="CancellationToken"/> and surfaces cancellation as
/// <see cref="OperationCanceledException"/> (or a subclass such as <see cref="TaskCanceledException"/>).
/// Cancellation is exercised both mid-flight (slow WireMock response cancelled after 100&nbsp;ms)
/// and before the request is even sent (token cancelled up front).
/// </summary>
public sealed class CancellationTests : IntegrationTestBase
{
    /// <summary>
    /// A slow in-flight <c>GET</c> is cancelled via its <see cref="CancellationToken"/> and
    /// the connection handler propagates the cancellation instead of completing normally.
    /// </summary>
    [Test]
    public void GetAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        Server.Given(Request.Create().WithPath("/2.0/slow").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("[]")
                .WithHeader("Content-Type", "application/json")
                .WithDelay(TimeSpan.FromSeconds(10)));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await ConnectionHandler.GetAsync<List<object>>("2.0/slow", null, cts.Token));
    }

    /// <summary>
    /// A token that is already cancelled before <c>GetAsync</c> is invoked must cause
    /// immediate cancellation rather than an HTTP round-trip. <see cref="HttpClient"/>
    /// surfaces a <see cref="TaskCanceledException"/> (an <see cref="OperationCanceledException"/>
    /// subclass), so <see cref="Assert.CatchAsync{T}(NUnit.Framework.AsyncTestDelegate)"/>
    /// is used to accept either concrete type.
    /// </summary>
    [Test]
    public void GetAsync_WhenCancelledBeforeRequest_ThrowsOperationCanceledException()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("[]")
                .WithHeader("Content-Type", "application/json"));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await ConnectionHandler.GetAsync<List<object>>("2.0/accounts", null, cts.Token));
    }

    /// <summary>
    /// A slow in-flight <c>POST</c> is cancelled via its <see cref="CancellationToken"/> and
    /// the connection handler propagates the cancellation.
    /// </summary>
    [Test]
    public void PostAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        Server.Given(Request.Create().WithPath("/2.0/slow").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{}")
                .WithHeader("Content-Type", "application/json")
                .WithDelay(TimeSpan.FromSeconds(10)));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var payload = new { name = "test" };

        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await ConnectionHandler.PostAsync<object, object>(payload, "2.0/slow", cts.Token));
    }
}
