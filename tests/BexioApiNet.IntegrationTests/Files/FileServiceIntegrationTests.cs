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
using BexioApiNet.Abstractions.Models.Files.Files.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Files;

namespace BexioApiNet.IntegrationTests.Files;

/// <summary>
///     Integration tests covering every entry point of <see cref="FileService" /> against
///     WireMock stubs. Verifies the path composed from <see cref="FileConfiguration" />
///     (<c>3.0/files</c>) reaches the handler correctly, that the expected HTTP verbs are used
///     (POST for upload, PATCH for partial edit, DELETE for removal, GET for list/show/download/preview/usage),
///     and that response bodies are deserialized into the C# models defined in
///     <c>BexioApiNet.Abstractions.Models.Files</c>. Stubbed JSON payloads mirror the OpenAPI
///     schema in <c>doc/openapi/bexio-v3.json</c> so each field round-trips through
///     <see cref="System.Text.Json" /> using the snake_case attribute mapping.
/// </summary>
public sealed class FileServiceIntegrationTests : IntegrationTestBase
{
    private const string FilesPath = "/3.0/files";

    private const string FileResponse = """
        {
            "id": 1,
            "uuid": "474cc93a-2d6f-47e9-bd3f-a5b5a1941314",
            "name": "screenshot.png",
            "size_in_bytes": 218476,
            "extension": "png",
            "mime_type": "image/png",
            "uploader_email": "contact@example.org",
            "user_id": 1,
            "is_archived": false,
            "source_id": 2,
            "source_type": "web",
            "is_referenced": false,
            "created_at": "2018-06-09T08:52:10+00:00"
        }
        """;

    private const string FileUsageResponse = """
        {
            "id": 1,
            "ref_class": "KbInvoice",
            "title": "RE-00001",
            "document_nr": "RE-00001"
        }
        """;

    /// <summary>
    ///     <c>FileService.Get()</c> must issue a <c>GET</c> request against <c>/3.0/files</c>
    ///     and deserialize each returned <see cref="BexioApiNet.Abstractions.Models.Files.Files.File" />
    ///     from the OpenAPI-shaped JSON array.
    /// </summary>
    [Test]
    public async Task FileService_Get_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(FilesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{FileResponse}]"));

        var service = new FileService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(FilesPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));

            var file = result.Data![0];
            Assert.That(file.Id, Is.EqualTo(1));
            Assert.That(file.Uuid, Is.EqualTo(Guid.Parse("474cc93a-2d6f-47e9-bd3f-a5b5a1941314")));
            Assert.That(file.Name, Is.EqualTo("screenshot.png"));
            Assert.That(file.SizeInBytes, Is.EqualTo(218476L));
            Assert.That(file.Extension, Is.EqualTo("png"));
            Assert.That(file.MimeType, Is.EqualTo("image/png"));
            Assert.That(file.UploaderEmail, Is.EqualTo("contact@example.org"));
            Assert.That(file.UserId, Is.EqualTo(1));
            Assert.That(file.IsArchived, Is.False);
            Assert.That(file.SourceId, Is.EqualTo(2));
            Assert.That(file.SourceType, Is.EqualTo("web"));
            Assert.That(file.IsReferenced, Is.False);
            Assert.That(file.CreatedAt, Is.EqualTo(new DateTime(2018, 6, 9, 8, 52, 10, DateTimeKind.Utc)));
        });
    }

    /// <summary>
    ///     <c>FileService.Get()</c> with a populated <see cref="QueryParameterFile" /> must forward
    ///     <c>limit</c>, <c>offset</c>, <c>order_by</c>, and <c>archived_state</c> onto the request URI.
    /// </summary>
    [Test]
    public async Task FileService_Get_WithQueryParameter_ForwardsAllOnUri()
    {
        Server
            .Given(Request.Create().WithPath(FilesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new FileService(ConnectionHandler);

        var queryParameter = new QueryParameterFile(
            Limit: 50,
            Offset: 100,
            OrderBy: "name_asc",
            ArchivedState: "not_archived");

        await service.Get(queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Url, Does.Contain("limit=50"));
            Assert.That(request.Url, Does.Contain("offset=100"));
            Assert.That(request.Url, Does.Contain("order_by=name_asc"));
            Assert.That(request.Url, Does.Contain("archived_state=not_archived"));
        });
    }

    /// <summary>
    ///     <c>FileService.GetById</c> must issue a <c>GET</c> request against <c>/3.0/files/{id}</c>
    ///     and deserialize every field defined on the OpenAPI <c>FileResponse</c> schema.
    /// </summary>
    [Test]
    public async Task FileService_GetById_SendsGetRequest_AndDeserializesAllFields()
    {
        const int id = 1;
        Server
            .Given(Request.Create().WithPath($"{FilesPath}/{id}").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FileResponse));

        var service = new FileService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo($"{FilesPath}/{id}"));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data!.Uuid, Is.EqualTo(Guid.Parse("474cc93a-2d6f-47e9-bd3f-a5b5a1941314")));
            Assert.That(result.Data!.Name, Is.EqualTo("screenshot.png"));
            Assert.That(result.Data!.SizeInBytes, Is.EqualTo(218476L));
            Assert.That(result.Data!.Extension, Is.EqualTo("png"));
            Assert.That(result.Data!.MimeType, Is.EqualTo("image/png"));
            Assert.That(result.Data!.UploaderEmail, Is.EqualTo("contact@example.org"));
            Assert.That(result.Data!.UserId, Is.EqualTo(1));
            Assert.That(result.Data!.IsArchived, Is.False);
            Assert.That(result.Data!.SourceId, Is.EqualTo(2));
            Assert.That(result.Data!.SourceType, Is.EqualTo("web"));
            Assert.That(result.Data!.IsReferenced, Is.False);
        });
    }

    /// <summary>
    ///     <c>FileService.Download</c> must issue a <c>GET</c> request to <c>/3.0/files/{id}/download</c>
    ///     and surface the binary payload returned by Bexio.
    /// </summary>
    [Test]
    public async Task FileService_Download_SendsGetRequest_AndReturnsBytes()
    {
        const int id = 1;
        byte[] expectedBytes = [0x25, 0x50, 0x44, 0x46];

        Server
            .Given(Request.Create().WithPath($"{FilesPath}/{id}/download").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(expectedBytes)
                .WithHeader("Content-Type", "application/octet-stream"));

        var service = new FileService(ConnectionHandler);

        var result = await service.Download(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo($"{FilesPath}/{id}/download"));
            Assert.That(result.Data, Is.EqualTo(expectedBytes));
        });
    }

    /// <summary>
    ///     <c>FileService.Preview</c> must issue a <c>GET</c> request to <c>/3.0/files/{id}/preview</c>
    ///     and return the binary preview payload.
    /// </summary>
    [Test]
    public async Task FileService_Preview_SendsGetRequest_AndReturnsBytes()
    {
        const int id = 1;
        byte[] expectedBytes = [0x89, 0x50, 0x4E, 0x47];

        Server
            .Given(Request.Create().WithPath($"{FilesPath}/{id}/preview").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(expectedBytes)
                .WithHeader("Content-Type", "image/png"));

        var service = new FileService(ConnectionHandler);

        var result = await service.Preview(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo($"{FilesPath}/{id}/preview"));
            Assert.That(result.Data, Is.EqualTo(expectedBytes));
        });
    }

    /// <summary>
    ///     <c>FileService.Usage</c> must issue a <c>GET</c> request against <c>/3.0/files/{id}/usage</c>
    ///     and deserialize every field of the OpenAPI <c>FileUsageResponse</c> schema.
    /// </summary>
    [Test]
    public async Task FileService_Usage_SendsGetRequest_AndDeserializesAllFields()
    {
        const int id = 1;

        Server
            .Given(Request.Create().WithPath($"{FilesPath}/{id}/usage").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FileUsageResponse));

        var service = new FileService(ConnectionHandler);

        var result = await service.Usage(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo($"{FilesPath}/{id}/usage"));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data!.RefClass, Is.EqualTo("KbInvoice"));
            Assert.That(result.Data!.Title, Is.EqualTo("RE-00001"));
            Assert.That(result.Data!.DocumentNr, Is.EqualTo("RE-00001"));
        });
    }

    /// <summary>
    ///     <c>FileService.Upload</c> (stream overload) must send a multipart <c>POST</c> request to
    ///     <c>/3.0/files</c> and return the deserialized <see cref="BexioApiNet.Abstractions.Models.Files.Files.File" />
    ///     metadata array surfaced by Bexio.
    /// </summary>
    [Test]
    public async Task FileService_Upload_WithStreams_SendsMultipartPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(FilesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{FileResponse}]"));

        var service = new FileService(ConnectionHandler);

        var files = new List<Tuple<MemoryStream, string>>
        {
            Tuple.Create(new MemoryStream([0x25, 0x50, 0x44, 0x46]), "letter.pdf")
        };

        var result = await service.Upload(files, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(FilesPath));
            Assert.That(request.Headers, Is.Not.Null);
            Assert.That(request.Headers!.ContainsKey("Content-Type"), Is.True);
            Assert.That(string.Join(";", request.Headers!["Content-Type"]), Does.Contain("multipart/form-data"));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].Name, Is.EqualTo("screenshot.png"));
        });
    }

    /// <summary>
    ///     <c>FileService.Search</c> must issue a <c>POST</c> request against <c>/3.0/files/search</c>
    ///     with the supplied <see cref="SearchCriteria" /> list as the JSON body and deserialize each
    ///     returned file.
    /// </summary>
    [Test]
    public async Task FileService_Search_SendsPostRequest_ToSearchPath_AndDeserializesAllFields()
    {
        var expectedPath = $"{FilesPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{FileResponse}]"));

        var service = new FileService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "screenshot", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"screenshot\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].Name, Is.EqualTo("screenshot.png"));
        });
    }

    /// <summary>
    ///     <c>FileService.Patch</c> must issue a <c>PATCH</c> request against <c>/3.0/files/{id}</c>
    ///     with the supplied <see cref="FilePatch" /> payload — only properties whose values are
    ///     non-null are serialized (<see langword="null" /> values are omitted via
    ///     <see cref="System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull" />).
    /// </summary>
    [Test]
    public async Task FileService_Patch_SendsPatchRequest_WithIdInPath_AndOmitsNullProperties()
    {
        const int id = 1;
        var expectedPath = $"{FilesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPatch())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FileResponse));

        var service = new FileService(ConnectionHandler);

        var payload = new FilePatch(Name: "renamed.png", IsArchived: true);

        var result = await service.Patch(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("PATCH"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"renamed.png\""));
            Assert.That(request.Body, Does.Contain("\"is_archived\":true"));
            Assert.That(request.Body, Does.Not.Contain("source_type"));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
        });
    }

    /// <summary>
    ///     <c>FileService.Delete</c> must issue a <c>DELETE</c> request against <c>/3.0/files/{id}</c>
    ///     and surface the <c>{"success": true}</c> envelope returned by Bexio.
    /// </summary>
    [Test]
    public async Task FileService_Delete_SendsDeleteRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{FilesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new FileService(ConnectionHandler);

        var result = await service.Delete(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
