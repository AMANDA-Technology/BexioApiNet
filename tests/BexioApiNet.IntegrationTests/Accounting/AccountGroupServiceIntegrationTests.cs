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

using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
///     Integration tests covering the <see cref="AccountGroupService" /> entry points against
///     WireMock stubs. Verifies the path composed from <see cref="AccountGroupConfiguration" />
///     (<c>2.0/account_groups</c>) reaches the handler correctly and that the expected HTTP
///     verb is used.
/// </summary>
public sealed class AccountGroupServiceIntegrationTests : IntegrationTestBase
{
    private const string AccountGroupsPath = "/2.0/account_groups";

    /// <summary>
    ///     <c>AccountGroupService.Get()</c> must issue a <c>GET</c> request against
    ///     <c>/2.0/account_groups</c> and return a successful <c>ApiResult</c> when the server
    ///     returns an empty array.
    /// </summary>
    [Test]
    public async Task AccountGroupService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(AccountGroupsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new AccountGroupService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AccountGroupsPath));
        });
    }
}
