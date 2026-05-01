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
using BexioApiNet.Abstractions.Models.MasterData.Titles.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
///     Integration tests covering the CRUD entry points of <see cref="TitleService" /> against
///     WireMock stubs. Verifies the path composed from <see cref="TitleConfiguration" />
///     (<c>2.0/title</c>) reaches the handler correctly, the expected HTTP verbs are used
///     (the Bexio Titles API uses <c>POST /2.0/title/{id}</c> for full-replacement edits per
///     the v3.0.0 OpenAPI spec — see
///     <see href="https://docs.bexio.com/#tag/Titles/operation/v2EditTitle" />),
///     pagination + ordering query parameters round-trip through
///     <see cref="QueryParameterTitle" />, and payloads round-trip through the canonical Title
///     schema (<c>id</c> + <c>name</c>).
/// </summary>
public sealed class TitleServiceIntegrationTests : IntegrationTestBase
{
    private const string TitlePath = "/2.0/title";

    private const string TitleResponse = """
                                         {
                                             "id": 1,
                                             "name": "Dr."
                                         }
                                         """;

    private const string TitleListResponse = """
                                             [
                                                 {
                                                     "id": 1,
                                                     "name": "Dr."
                                                 },
                                                 {
                                                     "id": 2,
                                                     "name": "Prof."
                                                 }
                                             ]
                                             """;

    /// <summary>
    ///     <c>TitleService.Get()</c> issues a <c>GET</c> request against
    ///     <c>/2.0/title</c> and deserializes the array of titles into the canonical
    ///     <see cref="BexioApiNet.Abstractions.Models.MasterData.Titles.Title" /> records.
    /// </summary>
    [Test]
    public async Task TitleService_Get_SendsGetRequest_DeserializesList()
    {
        Server
            .Given(Request.Create().WithPath(TitlePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TitleListResponse));

        var service = new TitleService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TitlePath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data[0].Name, Is.EqualTo("Dr."));
            Assert.That(result.Data[1].Id, Is.EqualTo(2));
            Assert.That(result.Data[1].Name, Is.EqualTo("Prof."));
        });
    }

    /// <summary>
    ///     <c>TitleService.Get()</c> appends the supplied <see cref="QueryParameterTitle" />
    ///     values (<c>limit</c>, <c>offset</c>, <c>order_by</c>) to the URL exactly as named by
    ///     the Bexio API.
    /// </summary>
    [Test]
    public async Task TitleService_Get_WithQueryParameter_AppendsLimitOffsetOrderBy()
    {
        Server
            .Given(Request.Create().WithPath(TitlePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TitleService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterTitle(limit: 50, offset: 100, orderBy: "name_desc"),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Url, Does.Contain("limit=50"));
            Assert.That(request.Url, Does.Contain("offset=100"));
            Assert.That(request.Url, Does.Contain("order_by=name_desc"));
        });
    }

    /// <summary>
    ///     <c>TitleService.GetById</c> issues a <c>GET</c> request that includes the target
    ///     id in the URL path and surfaces the returned title on success.
    /// </summary>
    [Test]
    public async Task TitleService_GetById_SendsGetRequest()
    {
        const int id = 1;
        var expectedPath = $"{TitlePath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TitleResponse));

        var service = new TitleService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data.Name, Is.EqualTo("Dr."));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>TitleService.Create</c> sends a <c>POST</c> request whose body is the
    ///     serialized <see cref="TitleCreate" /> payload and surfaces the returned title
    ///     on success.
    /// </summary>
    [Test]
    public async Task TitleService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(TitlePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(TitleResponse));

        var service = new TitleService(ConnectionHandler);

        var payload = new TitleCreate("Dr.");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(result.Data.Name, Is.EqualTo("Dr."));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TitlePath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Dr.\""));
        });
    }

    /// <summary>
    ///     <c>TitleService.Search</c> sends a <c>POST</c> request against
    ///     <c>/2.0/title/search</c> with the <see cref="SearchCriteria" /> list as the JSON body
    ///     and deserializes the returned array of matches.
    /// </summary>
    [Test]
    public async Task TitleService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{TitlePath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TitleListResponse));

        var service = new TitleService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Dr.", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
        });
    }

    /// <summary>
    ///     <c>TitleService.Update</c> sends a <c>POST</c> request against
    ///     <c>/2.0/title/{id}</c>. The Bexio Titles API uses <c>POST</c> (not <c>PUT</c>)
    ///     for full-replacement edits per the v3.0.0 OpenAPI spec.
    /// </summary>
    [Test]
    public async Task TitleService_Update_SendsPostRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{TitlePath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TitleResponse));

        var service = new TitleService(ConnectionHandler);

        var payload = new TitleUpdate("Prof.");

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Prof.\""));
        });
    }

    /// <summary>
    ///     <c>TitleService.Delete</c> issues a <c>DELETE</c> request that includes the
    ///     target id in the URL path and parses the <c>{"success":true}</c> success body.
    /// </summary>
    [Test]
    public async Task TitleService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
        var expectedPath = $"{TitlePath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new TitleService(ConnectionHandler);

        var result = await service.Delete(idToDelete, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
