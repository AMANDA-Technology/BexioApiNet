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
/// Verifies that a single shared <see cref="BexioConnectionHandler"/> can service multiple
/// in-flight requests concurrently without cross-talk — both for many parallel requests of
/// the same HTTP verb and for mixed verbs (GET / POST / DELETE) fired together.
/// </summary>
public sealed class ConcurrencyTests : IntegrationTestBase
{
    /// <summary>
    /// Fires five simultaneous <c>GET</c> requests against five different paths and
    /// asserts all of them observe <see cref="Abstractions.Models.Api.ApiResult.IsSuccess"/> = <c>true</c>.
    /// </summary>
    [Test]
    public async Task MultipleGetAsync_Concurrently_AllSucceed()
    {
        for (var i = 1; i <= 5; i++)
        {
            Server.Given(Request.Create().WithPath($"/2.0/resource/{i}").UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"[{{\"id\":{i}}}]")
                    .WithHeader("Content-Type", "application/json"));
        }

        var tasks = Enumerable.Range(1, 5)
            .Select(i => ConnectionHandler.GetAsync<List<Dictionary<string, int>>>($"2.0/resource/{i}", null, CancellationToken.None))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.That(results, Has.Length.EqualTo(5));
        Assert.That(results.All(r => r.IsSuccess), Is.True);
    }

    /// <summary>
    /// Fires a <c>GET</c>, <c>POST</c> and <c>DELETE</c> simultaneously against distinct paths
    /// and asserts all three calls complete successfully.
    /// </summary>
    [Test]
    public async Task MixedHttpVerbs_Concurrently_AllSucceed()
    {
        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("[{\"id\":1}]")
                .WithHeader("Content-Type", "application/json"));

        Server.Given(Request.Create().WithPath("/2.0/accounts").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"id\":2}")
                .WithHeader("Content-Type", "application/json"));

        Server.Given(Request.Create().WithPath("/2.0/accounts/3").UsingDelete())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("true")
                .WithHeader("Content-Type", "application/json"));

        var getTask = ConnectionHandler.GetAsync<List<Dictionary<string, int>>>("2.0/accounts", null, CancellationToken.None);
        var postTask = ConnectionHandler.PostAsync<Dictionary<string, int>, object>(new { name = "new" }, "2.0/accounts", CancellationToken.None);
        var deleteTask = ConnectionHandler.Delete("2.0/accounts/3", CancellationToken.None);

        await Task.WhenAll(getTask, postTask, deleteTask);

        Assert.That(getTask.Result.IsSuccess, Is.True);
        Assert.That(postTask.Result.IsSuccess, Is.True);
        Assert.That(deleteTask.Result.IsSuccess, Is.True);
    }
}
