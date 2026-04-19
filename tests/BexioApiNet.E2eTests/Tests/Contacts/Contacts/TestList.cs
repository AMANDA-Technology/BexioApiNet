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
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.E2eTests.Tests.Contacts.Contacts;

/// <summary>
/// Live E2E tests for <see cref="ContactService.Get"/>. The DI wire-up that exposes
/// <c>Contacts</c> on <see cref="IBexioApiClient"/> is delivered in issue #49; until
/// then the service is constructed directly from the same environment credentials
/// that drive <see cref="BexioE2eTestBase"/>.
/// </summary>
public class TestList : BexioE2eTestBase
{
    /// <summary>
    /// List the first page of contacts and confirm the request round-trips successfully.
    /// </summary>
    [Test]
    public async Task List_ReturnsContacts()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        using var connectionHandler = CreateConnectionHandler();
        var service = new ContactService(connectionHandler);

        var result = await service.Get(new QueryParameterContact(Limit: 5));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    private static BexioConnectionHandler CreateConnectionHandler()
    {
        var baseUri = Environment.GetEnvironmentVariable("BexioApiNet__BaseUri")!;
        var jwtToken = Environment.GetEnvironmentVariable("BexioApiNet__JwtToken")!;
        return new BexioConnectionHandler(
            new BexioConfiguration
            {
                BaseUri = baseUri,
                JwtToken = jwtToken,
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            });
    }
}
