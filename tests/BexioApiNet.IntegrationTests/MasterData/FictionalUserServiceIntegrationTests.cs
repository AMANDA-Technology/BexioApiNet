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

using BexioApiNet.Abstractions.Models.MasterData.FictionalUsers.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
///     Integration tests for <see cref="FictionalUserService" /> against WireMock stubs.
///     The Bexio v3.0 fictional users endpoint exposes list, fetch-by-id, create (POST),
///     update (PATCH) and delete operations under <c>/3.0/fictional_users</c>. Each test
///     verifies HTTP verb, request URL, body serialization, and response deserialization
///     against a JSON payload that matches the OpenAPI schema shape exactly.
/// </summary>
public sealed class FictionalUserServiceIntegrationTests : IntegrationTestBase
{
    private const string BasePath = "/3.0/fictional_users";

    private const string FictionalUserResponse = """
                                                 {
                                                     "id": 4,
                                                     "salutation_type": "male",
                                                     "firstname": "Rudolph",
                                                     "lastname": "Smith",
                                                     "email": "rudolph.smith@bexio.com",
                                                     "title_id": null
                                                 }
                                                 """;

    /// <summary>
    ///     <c>Get()</c> issues a <c>GET</c> against <c>/3.0/fictional_users</c> and deserializes
    ///     a fully-populated array of fictional users matching the OpenAPI schema.
    /// </summary>
    [Test]
    public async Task FictionalUserService_Get_SendsGetRequestAndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{FictionalUserResponse}]"));

        var service = new FictionalUserService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            var first = result.Data![0];
            Assert.That(first.Id, Is.EqualTo(4));
            Assert.That(first.SalutationType, Is.EqualTo("male"));
            Assert.That(first.Firstname, Is.EqualTo("Rudolph"));
            Assert.That(first.Lastname, Is.EqualTo("Smith"));
            Assert.That(first.Email, Is.EqualTo("rudolph.smith@bexio.com"));
            Assert.That(first.TitleId, Is.Null);
        });
    }

    /// <summary>
    ///     <c>Get()</c> with a populated <see cref="QueryParameterFictionalUser" /> renders the
    ///     <c>limit</c> and <c>offset</c> values onto the request URI as expected by the Bexio
    ///     OpenAPI spec.
    /// </summary>
    [Test]
    public async Task FictionalUserService_Get_WithQueryParameter_RendersLimitAndOffsetOnUri()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new FictionalUserService(ConnectionHandler);

        await service.Get(new QueryParameterFictionalUser(20, 100),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(request.RawQuery, Does.Contain("limit=20"));
            Assert.That(request.RawQuery, Does.Contain("offset=100"));
        });
    }

    /// <summary>
    ///     <c>GetById</c> issues a <c>GET</c> at <c>/3.0/fictional_users/{id}</c> and
    ///     deserializes the full response payload.
    /// </summary>
    [Test]
    public async Task FictionalUserService_GetById_SendsGetRequestWithIdInPath()
    {
        const int id = 4;
        var expectedPath = $"{BasePath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FictionalUserResponse));

        var service = new FictionalUserService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data.SalutationType, Is.EqualTo("male"));
            Assert.That(result.Data.Firstname, Is.EqualTo("Rudolph"));
            Assert.That(result.Data.Lastname, Is.EqualTo("Smith"));
            Assert.That(result.Data.Email, Is.EqualTo("rudolph.smith@bexio.com"));
        });
    }

    /// <summary>
    ///     <c>Create</c> issues a <c>POST</c> at <c>/3.0/fictional_users</c> with the
    ///     serialized <see cref="FictionalUserCreate" /> payload using snake_case field names.
    /// </summary>
    [Test]
    public async Task FictionalUserService_Create_SendsPostRequestWithJsonBody()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(FictionalUserResponse));

        var service = new FictionalUserService(ConnectionHandler);

        var payload = new FictionalUserCreate("male", "Rudolph", "Smith", "rudolph.smith@bexio.com");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(request.Body, Does.Contain("\"salutation_type\":\"male\""));
            Assert.That(request.Body, Does.Contain("\"firstname\":\"Rudolph\""));
            Assert.That(request.Body, Does.Contain("\"lastname\":\"Smith\""));
            Assert.That(request.Body, Does.Contain("\"email\":\"rudolph.smith@bexio.com\""));
            Assert.That(request.Body, Does.Not.Contain("\"title_id\""),
                "title_id is omitted from the create payload when null");
            Assert.That(result.Data!.Id, Is.EqualTo(4));
        });
    }

    /// <summary>
    ///     <c>Create</c> serializes a non-null <see cref="FictionalUserCreate.TitleId" />
    ///     onto the request body so optional links to a title reach Bexio.
    /// </summary>
    [Test]
    public async Task FictionalUserService_Create_WithTitleId_IncludesTitleIdInBody()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(FictionalUserResponse));

        var service = new FictionalUserService(ConnectionHandler);

        var payload = new FictionalUserCreate("female", "Anna", "Müller", "anna.mueller@bexio.com", TitleId: 7);

        await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;
        Assert.That(request.Body, Does.Contain("\"title_id\":7"));
    }

    /// <summary>
    ///     <c>Patch</c> issues a <c>PATCH</c> request at <c>/3.0/fictional_users/{id}</c>
    ///     with only the supplied fields (null fields are omitted from the JSON body).
    /// </summary>
    [Test]
    public async Task FictionalUserService_Patch_SendsPatchRequest_OmitsNullFields()
    {
        const int id = 4;
        var expectedPath = $"{BasePath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPatch())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FictionalUserResponse));

        var service = new FictionalUserService(ConnectionHandler);

        var payload = new FictionalUserPatch(Firstname: "Rudolph-Updated");

        var result = await service.Patch(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("PATCH"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"firstname\":\"Rudolph-Updated\""));
            Assert.That(request.Body, Does.Not.Contain("\"salutation_type\""));
            Assert.That(request.Body, Does.Not.Contain("\"lastname\""));
            Assert.That(request.Body, Does.Not.Contain("\"email\""));
            Assert.That(request.Body, Does.Not.Contain("\"title_id\""));
            Assert.That(result.Data!.Id, Is.EqualTo(id));
        });
    }

    /// <summary>
    ///     <c>Delete</c> issues a <c>DELETE</c> request at <c>/3.0/fictional_users/{id}</c>
    ///     and surfaces the <c>{ "success": true }</c> EntryDeleted payload.
    /// </summary>
    [Test]
    public async Task FictionalUserService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 4;
        var expectedPath = $"{BasePath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new FictionalUserService(ConnectionHandler);

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
