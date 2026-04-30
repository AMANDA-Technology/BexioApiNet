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

using BexioApiNet.Abstractions.Enums.MasterData;
using BexioApiNet.Abstractions.Models.MasterData.Comments.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
/// Integration tests covering the read/create entry points of <see cref="CommentService" /> against
/// WireMock stubs. Verifies that the URL is composed with the polymorphic <c>kb_document_type</c>
/// segment and the document id, that the OpenAPI-documented <c>limit</c>/<c>offset</c> query
/// parameters are emitted on the URI when supplied, and that responses populate every documented
/// field on the <see cref="BexioApiNet.Abstractions.Models.MasterData.Comments.Comment"/> record.
/// </summary>
public sealed class CommentServiceIntegrationTests : IntegrationTestBase
{
    private const int DocumentId = 4;
    private const int CommentId = 7;
    private const string DocumentTypeSegment = "kb_invoice";
    private static readonly string CommentListPath = $"/2.0/{DocumentTypeSegment}/{DocumentId}/comment";
    private static readonly string CommentByIdPath = $"{CommentListPath}/{CommentId}";

    private const string CommentResponse = """
                                           {
                                               "id": 4,
                                               "text": "Sample comment",
                                               "user_id": 1,
                                               "user_email": "peter.smith@example.com",
                                               "user_name": "Peter Smith",
                                               "date": "2019-07-18 15:41:53",
                                               "is_public": false,
                                               "image": "R0lGODlhAQABAIAAAAUEBAAAACwAAAAAAQABAAACAkQBADs=",
                                               "image_path": "https://my.bexio.com/img/profile_picture/j2cbWl-yp3zT9oOh9jHTAA/Ds8buEV0HXZsvuBm3df8SQ.png?type=thumb"
                                           }
                                           """;

    /// <summary>
    /// <c>CommentService.Get</c> must issue a <c>GET</c> request against
    /// <c>/2.0/{kb_document_type}/{document_id}/comment</c> and populate every documented
    /// field on the resulting <c>Comment</c> from the JSON body.
    /// </summary>
    [Test]
    public async Task CommentService_Get_SendsGetRequest_AndDeserialisesEveryField()
    {
        Server
            .Given(Request.Create().WithPath(CommentListPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{CommentResponse}]"));

        var service = new CommentService(ConnectionHandler);

        var result = await service.Get(KbDocumentType.Invoice, DocumentId, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CommentListPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));

            var comment = result.Data![0];
            Assert.That(comment.Id, Is.EqualTo(4));
            Assert.That(comment.Text, Is.EqualTo("Sample comment"));
            Assert.That(comment.UserId, Is.EqualTo(1));
            Assert.That(comment.UserEmail, Is.EqualTo("peter.smith@example.com"));
            Assert.That(comment.UserName, Is.EqualTo("Peter Smith"));
            Assert.That(comment.Date, Is.EqualTo("2019-07-18 15:41:53"));
            Assert.That(comment.IsPublic, Is.False);
            Assert.That(comment.Image, Is.EqualTo("R0lGODlhAQABAIAAAAUEBAAAACwAAAAAAQABAAACAkQBADs="));
            Assert.That(comment.ImagePath, Is.EqualTo("https://my.bexio.com/img/profile_picture/j2cbWl-yp3zT9oOh9jHTAA/Ds8buEV0HXZsvuBm3df8SQ.png?type=thumb"));
        });
    }

    /// <summary>
    /// <c>CommentService.Get</c> with a <see cref="QueryParameterComment"/> must propagate the
    /// <c>limit</c> and <c>offset</c> query parameters onto the request URI exactly as Bexio
    /// documents them in the OpenAPI spec.
    /// </summary>
    [Test]
    public async Task CommentService_Get_WithPagination_AppendsLimitAndOffsetQueryParameters()
    {
        Server
            .Given(Request.Create().WithPath(CommentListPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new CommentService(ConnectionHandler);

        var queryParameter = new QueryParameterComment(Limit: 50, Offset: 100);

        await service.Get(KbDocumentType.Invoice, DocumentId, queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CommentListPath));
            Assert.That(request.Url, Does.Contain("limit=50"));
            Assert.That(request.Url, Does.Contain("offset=100"));
        });
    }

    /// <summary>
    /// <c>CommentService.GetById</c> must issue a <c>GET</c> request that appends the comment id
    /// to the document-scoped collection path and fully populate every field on the returned
    /// <c>Comment</c>.
    /// </summary>
    [Test]
    public async Task CommentService_GetById_SendsGetRequest_AndDeserialisesEveryField()
    {
        Server
            .Given(Request.Create().WithPath(CommentByIdPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CommentResponse));

        var service = new CommentService(ConnectionHandler);

        var result = await service.GetById(KbDocumentType.Invoice, DocumentId, CommentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(4));
            Assert.That(result.Data.Text, Is.EqualTo("Sample comment"));
            Assert.That(result.Data.UserId, Is.EqualTo(1));
            Assert.That(result.Data.UserName, Is.EqualTo("Peter Smith"));
            Assert.That(result.Data.UserEmail, Is.EqualTo("peter.smith@example.com"));
            Assert.That(result.Data.IsPublic, Is.False);
            Assert.That(result.Data.Image, Is.Not.Null);
            Assert.That(result.Data.ImagePath, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CommentByIdPath));
        });
    }

    /// <summary>
    /// <c>CommentService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="CommentCreate" /> payload using the snake_case property names
    /// from the OpenAPI spec, and surface the returned comment on success.
    /// </summary>
    [Test]
    public async Task CommentService_Create_SendsPostRequest_WithSnakeCaseBody()
    {
        Server
            .Given(Request.Create().WithPath(CommentListPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(CommentResponse));

        var service = new CommentService(ConnectionHandler);

        var payload = new CommentCreate
        {
            Text = "Sample comment",
            UserId = 1,
            UserName = "Peter Smith",
            UserEmail = "peter.smith@example.com",
            IsPublic = false
        };

        var result = await service.Create(KbDocumentType.Invoice, DocumentId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(4));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CommentListPath));
            Assert.That(request.Body, Does.Contain("\"text\":\"Sample comment\""));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
            Assert.That(request.Body, Does.Contain("\"user_name\":\"Peter Smith\""));
            Assert.That(request.Body, Does.Contain("\"user_email\":\"peter.smith@example.com\""));
            Assert.That(request.Body, Does.Contain("\"is_public\":false"));
        });
    }
}
