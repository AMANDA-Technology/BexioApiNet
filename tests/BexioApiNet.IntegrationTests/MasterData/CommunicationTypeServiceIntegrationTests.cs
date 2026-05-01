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

using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
/// Integration tests covering the read/search entry points of <see cref="CommunicationTypeService" />
/// against WireMock stubs. Bexio exposes the resource under the legacy URL segment
/// <c>communication_kind</c>; the tests verify the path composed from
/// <see cref="CommunicationTypeConfiguration" /> reaches the handler correctly, that the
/// <c>limit</c>/<c>offset</c> query parameters are emitted on the URI, and that the response
/// JSON deserialises into the
/// <see cref="BexioApiNet.Abstractions.Models.MasterData.CommunicationTypes.CommunicationType"/>
/// record.
/// </summary>
public sealed class CommunicationTypeServiceIntegrationTests : IntegrationTestBase
{
    private const string CommunicationTypePath = "/2.0/communication_kind";

    private const string CommunicationTypeResponse = """
                                                     {
                                                         "id": 1,
                                                         "name": "Mobile Phone"
                                                     }
                                                     """;

    /// <summary>
    /// <c>CommunicationTypeService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/communication_kind</c> and surface every field on the returned
    /// <c>CommunicationType</c>.
    /// </summary>
    [Test]
    public async Task CommunicationTypeService_Get_SendsGetRequest_AndDeserialisesEveryField()
    {
        Server
            .Given(Request.Create().WithPath(CommunicationTypePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{CommunicationTypeResponse}]"));

        var service = new CommunicationTypeService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CommunicationTypePath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data[0].Name, Is.EqualTo("Mobile Phone"));
        });
    }

    /// <summary>
    /// <c>CommunicationTypeService.Get</c> with a <see cref="QueryParameterCommunicationType"/>
    /// must emit the <c>limit</c> and <c>offset</c> query string parameters on the URI as
    /// documented in the OpenAPI spec.
    /// </summary>
    [Test]
    public async Task CommunicationTypeService_Get_WithPagination_AppendsLimitAndOffsetQueryParameters()
    {
        Server
            .Given(Request.Create().WithPath(CommunicationTypePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new CommunicationTypeService(ConnectionHandler);

        var queryParameter = new QueryParameterCommunicationType(Limit: 25, Offset: 75);

        await service.Get(queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CommunicationTypePath));
            Assert.That(request.Url, Does.Contain("limit=25"));
            Assert.That(request.Url, Does.Contain("offset=75"));
        });
    }

    /// <summary>
    /// <c>CommunicationTypeService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/communication_kind/search</c> with the <see cref="SearchCriteria" /> list as
    /// the JSON body and deserialise the array response.
    /// </summary>
    [Test]
    public async Task CommunicationTypeService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{CommunicationTypePath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{CommunicationTypeResponse}]"));

        var service = new CommunicationTypeService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Phone", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Name, Is.EqualTo("Mobile Phone"));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"Phone\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
        });
    }
}
