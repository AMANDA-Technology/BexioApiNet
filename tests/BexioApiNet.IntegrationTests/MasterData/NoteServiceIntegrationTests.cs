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
using BexioApiNet.Abstractions.Models.MasterData.Notes.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
///     Integration tests for <see cref="NoteService" /> against WireMock stubs. Bexio's
///     v2 notes endpoint exposes list, fetch-by-id, create, search, edit (POST) and delete
///     under <c>/2.0/note</c>. Each test asserts HTTP verb, URL, body serialization with
///     snake_case field names, and deserialization of a fully-populated JSON response that
///     matches the OpenAPI <c>Note</c> schema exactly.
/// </summary>
public sealed class NoteServiceIntegrationTests : IntegrationTestBase
{
    private const string BasePath = "/2.0/note";
    private const string SearchPath = $"{BasePath}/search";

    private const string NoteResponse = """
                                        {
                                            "id": 4,
                                            "user_id": 1,
                                            "event_start": "2026-01-16 14:20:00",
                                            "subject": "API conception",
                                            "info": "Initial planning",
                                            "contact_id": 14,
                                            "project_id": null,
                                            "pr_project_id": null,
                                            "entry_id": null,
                                            "module_id": null
                                        }
                                        """;

    /// <summary>
    ///     <c>Get</c> issues a <c>GET</c> at <c>/2.0/note</c> and deserializes a fully-populated
    ///     array of notes that mirrors the Bexio OpenAPI schema (every property present, including
    ///     the nullable <c>project_id</c>, <c>pr_project_id</c>, <c>entry_id</c>, <c>module_id</c>).
    /// </summary>
    [Test]
    public async Task NoteService_Get_SendsGetRequest_DeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{NoteResponse}]"));

        var service = new NoteService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(result.Data, Has.Count.EqualTo(1));
            var note = result.Data![0];
            Assert.That(note.Id, Is.EqualTo(4));
            Assert.That(note.UserId, Is.EqualTo(1));
            Assert.That(note.EventStart, Is.EqualTo(new DateTime(2026, 1, 16, 14, 20, 0)));
            Assert.That(note.Subject, Is.EqualTo("API conception"));
            Assert.That(note.Info, Is.EqualTo("Initial planning"));
            Assert.That(note.ContactId, Is.EqualTo(14));
            Assert.That(note.ProjectId, Is.Null);
            Assert.That(note.PrProjectId, Is.Null);
            Assert.That(note.EntryId, Is.Null);
            Assert.That(note.ModuleId, Is.Null);
        });
    }

    /// <summary>
    ///     <c>Get</c> with a populated <see cref="QueryParameterNote" /> renders the
    ///     <c>limit</c> and <c>offset</c> values onto the request URI.
    /// </summary>
    [Test]
    public async Task NoteService_Get_WithQueryParameter_RendersLimitAndOffsetOnUri()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new NoteService(ConnectionHandler);

        await service.Get(new QueryParameterNote(20, 100),
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
    ///     <c>GetById</c> issues a <c>GET</c> at <c>/2.0/note/{id}</c> and deserializes the
    ///     full note payload.
    /// </summary>
    [Test]
    public async Task NoteService_GetById_SendsGetRequest()
    {
        const int id = 4;
        var expectedPath = $"{BasePath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(NoteResponse));

        var service = new NoteService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data.Subject, Is.EqualTo("API conception"));
            Assert.That(result.Data.ContactId, Is.EqualTo(14));
        });
    }

    /// <summary>
    ///     <c>Create</c> issues a <c>POST</c> at <c>/2.0/note</c> with a snake_case JSON body
    ///     containing the required <c>user_id</c>, <c>event_start</c>, <c>subject</c> fields.
    /// </summary>
    [Test]
    public async Task NoteService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(NoteResponse));

        var service = new NoteService(ConnectionHandler);

        var payload = new NoteCreate(
            UserId: 1,
            EventStart: new DateTime(2026, 1, 16, 14, 20, 0, DateTimeKind.Utc),
            Subject: "API conception",
            ContactId: 14);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
            Assert.That(request.Body, Does.Contain("\"subject\":\"API conception\""));
            Assert.That(request.Body, Does.Contain("\"contact_id\":14"));
            Assert.That(request.Body, Does.Contain("\"event_start\""));
            Assert.That(result.Data!.Id, Is.EqualTo(4));
        });
    }

    /// <summary>
    ///     <c>Search</c> issues a <c>POST</c> at <c>/2.0/note/search</c> with the
    ///     <see cref="SearchCriteria" /> list as the JSON body.
    /// </summary>
    [Test]
    public async Task NoteService_Search_SendsPostRequest_ToSearchPath()
    {
        Server
            .Given(Request.Create().WithPath(SearchPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{NoteResponse}]"));

        var service = new NoteService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "subject", Value = "API", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SearchPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"subject\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Subject, Is.EqualTo("API conception"));
        });
    }

    /// <summary>
    ///     <c>Update</c> issues a <c>POST</c> at <c>/2.0/note/{id}</c> per the Bexio
    ///     <c>v2EditNote</c> operation (the spec uses POST, not PUT).
    /// </summary>
    [Test]
    public async Task NoteService_Update_SendsPostRequest_WithIdInPath()
    {
        const int id = 4;
        var expectedPath = $"{BasePath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(NoteResponse));

        var service = new NoteService(ConnectionHandler);

        var payload = new NoteUpdate(
            UserId: 1,
            EventStart: new DateTime(2026, 1, 16, 14, 20, 0, DateTimeKind.Utc),
            Subject: "API conception (edited)",
            Info: "Updated body");

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
            Assert.That(request.Body, Does.Contain("\"subject\":\"API conception (edited)\""));
            Assert.That(request.Body, Does.Contain("\"info\":\"Updated body\""));
            Assert.That(result.Data!.Id, Is.EqualTo(id));
        });
    }

    /// <summary>
    ///     <c>Delete</c> issues a <c>DELETE</c> at <c>/2.0/note/{id}</c> and surfaces the
    ///     <c>{ "success": true }</c> EntryDeleted payload.
    /// </summary>
    [Test]
    public async Task NoteService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 4;
        var expectedPath = $"{BasePath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new NoteService(ConnectionHandler);

        var result = await service.Delete(idToDelete, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     A note response that omits the optional <c>info</c> field still deserializes
    ///     successfully — <see cref="BexioApiNet.Abstractions.Models.MasterData.Notes.Note.Info" />
    ///     is nullable per the OpenAPI schema (the field is not in the <c>required</c> list).
    /// </summary>
    [Test]
    public async Task NoteService_GetById_ResponseWithoutInfo_DeserializesNullInfo()
    {
        const int id = 4;
        var expectedPath = $"{BasePath}/{id}";
        const string responseWithoutInfo = """
                                           {
                                               "id": 4,
                                               "user_id": 1,
                                               "event_start": "2026-01-16 14:20:00",
                                               "subject": "API conception",
                                               "contact_id": null,
                                               "project_id": null,
                                               "pr_project_id": null,
                                               "entry_id": null,
                                               "module_id": null
                                           }
                                           """;

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(responseWithoutInfo));

        var service = new NoteService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Info, Is.Null);
            Assert.That(result.Data.Subject, Is.EqualTo("API conception"));
        });
    }
}
