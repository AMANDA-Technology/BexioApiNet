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
using BexioApiNet.Abstractions.Enums.MasterData;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.MasterData.Comments;
using BexioApiNet.Abstractions.Models.MasterData.Comments.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.UnitTests.MasterData;

/// <summary>
/// Offline unit tests for <see cref="CommentService"/>. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected route
/// (composed from the polymorphic <c>kb_document_type</c> URL discriminator and the document id),
/// honours the <c>limit</c> / <c>offset</c> query parameters from the OpenAPI spec, and
/// supports the canonical <c>autoPage</c> + <c>X-Total-Count</c> pagination contract. No network,
/// no filesystem access.
/// </summary>
[TestFixture]
public sealed class CommentServiceTests : ServiceTestBase
{
    private const int DocumentId = 4;
    private const int CommentId = 7;

    private CommentService _sut = null!;

    /// <summary>Creates a fresh <see cref="CommentService"/> bound to the substitute per test.</summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new CommentService(ConnectionHandler);
    }

    /// <summary>
    /// <see cref="KbDocumentTypeExtensions.ToBexioString"/> maps every defined enum value
    /// to the snake_case Bexio path segment.
    /// </summary>
    [TestCase(KbDocumentType.Offer, "kb_offer")]
    [TestCase(KbDocumentType.Order, "kb_order")]
    [TestCase(KbDocumentType.Invoice, "kb_invoice")]
    public void ToBexioString_MapsEnumValueToBexioSegment(KbDocumentType kbDocumentType, string expectedSegment)
    {
        kbDocumentType.ToBexioString().ShouldBe(expectedSegment);
    }

    /// <summary>
    /// Get composes the route from the polymorphic discriminator and document id and forwards
    /// the call to <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with a null query parameter
    /// when no caller parameters are supplied.
    /// </summary>
    [TestCase(KbDocumentType.Invoice, "kb_invoice")]
    [TestCase(KbDocumentType.Offer, "kb_offer")]
    public async Task Get_CallsGetAsync_WithExpectedPath(KbDocumentType kbDocumentType, string expectedSegment)
    {
        var response = new ApiResult<List<Comment>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Comment>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(kbDocumentType, DocumentId);

        await ConnectionHandler.Received(1).GetAsync<List<Comment>?>(
            $"2.0/{expectedSegment}/{DocumentId}/comment",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}"/> produced by the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<Comment>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Comment>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get(KbDocumentType.Invoice, DocumentId);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    /// When a <see cref="QueryParameterComment"/> is supplied, its inner
    /// <see cref="QueryParameter"/> instance is forwarded to the connection
    /// handler verbatim — the OpenAPI spec lists <c>limit</c> and <c>offset</c>
    /// as supported query parameters on the comment list endpoint.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterComment(Limit: 50, Offset: 100);
        ConnectionHandler
            .GetAsync<List<Comment>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Comment>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(KbDocumentType.Invoice, DocumentId, queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Comment>?>(
            $"2.0/kb_invoice/{DocumentId}/comment",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When <c>autoPage</c> is on and the first response advertises a
    /// <c>X-Total-Count</c> header, the service calls <c>FetchAll</c> with the
    /// count of already-fetched items, the total, the same path, and the
    /// same query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderPresent_CallsFetchAll()
    {
        var firstComment = NewComment(1);
        var firstPage = new ApiResult<List<Comment>?>
        {
            IsSuccess = true,
            Data = [firstComment],
            ResponseHeaders = new Dictionary<string, int?>
            {
                [ApiHeaderNames.TotalResults] = 3
            }
        };
        ConnectionHandler
            .GetAsync<List<Comment>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(firstPage));
        var remaining = new List<Comment> { NewComment(2), NewComment(3) };
        ConnectionHandler
            .FetchAll<Comment>(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(remaining));

        var result = await _sut.Get(KbDocumentType.Invoice, DocumentId, autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Comment>(
            1,
            3,
            $"2.0/kb_invoice/{DocumentId}/comment",
            null,
            Arg.Any<CancellationToken>());
        result.Data.ShouldNotBeNull();
        result.Data!.Count.ShouldBe(3);
    }

    /// <summary>
    /// When <c>autoPage</c> is requested but the response carries no
    /// <c>X-Total-Count</c> header, the service does not invoke <c>FetchAll</c>.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_WhenTotalResultsHeaderMissing_DoesNotCallFetchAll()
    {
        var response = new ApiResult<List<Comment>?>
        {
            IsSuccess = true,
            Data = [NewComment(1)],
            ResponseHeaders = new Dictionary<string, int?>()
        };
        ConnectionHandler
            .GetAsync<List<Comment>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Get(KbDocumentType.Invoice, DocumentId, autoPage: true);

        result.ShouldBeSameAs(response);
        await ConnectionHandler.DidNotReceive().FetchAll<Comment>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetById appends the comment id to the path for both discriminator values exercised here.
    /// </summary>
    [TestCase(KbDocumentType.Invoice, "kb_invoice")]
    [TestCase(KbDocumentType.Order, "kb_order")]
    public async Task GetById_CallsGetAsync_WithCommentIdInPath(KbDocumentType kbDocumentType, string expectedSegment)
    {
        var response = new ApiResult<Comment> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Comment>(
                Arg.Do<string>(p => capturedPath = p),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(kbDocumentType, DocumentId, CommentId);

        capturedPath.ShouldBe($"2.0/{expectedSegment}/{DocumentId}/comment/{CommentId}");
    }

    /// <summary>
    /// Create posts the payload to the list path for both discriminator values exercised here.
    /// </summary>
    [TestCase(KbDocumentType.Invoice, "kb_invoice")]
    [TestCase(KbDocumentType.Offer, "kb_offer")]
    public async Task Create_CallsPostAsync_WithExpectedPath(KbDocumentType kbDocumentType, string expectedSegment)
    {
        var payload = new CommentCreate
        {
            Text = "Sample comment",
            UserId = 1,
            UserName = "Peter Smith",
            IsPublic = false
        };
        var response = new ApiResult<Comment> { IsSuccess = true };
        CommentCreate? capturedPayload = null;
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Comment, CommentCreate>(
                Arg.Do<CommentCreate>(p => capturedPayload = p),
                Arg.Do<string>(p => capturedPath = p),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(kbDocumentType, DocumentId, payload);

        capturedPath.ShouldBe($"2.0/{expectedSegment}/{DocumentId}/comment");
        capturedPayload.ShouldBeSameAs(payload);
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}"/> produced by the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResult()
    {
        var payload = new CommentCreate
        {
            Text = "Sample comment",
            UserId = 1,
            UserName = "Peter Smith"
        };
        var response = new ApiResult<Comment> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Comment, CommentCreate>(
                Arg.Any<CommentCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(KbDocumentType.Invoice, DocumentId, payload);

        result.ShouldBeSameAs(response);
    }

    /// <summary>
    /// The cancellation token supplied by the caller must be forwarded to the
    /// connection handler on <c>Get</c> so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<Comment>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Comment>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(KbDocumentType.Invoice, DocumentId, cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<Comment>?>(
            $"2.0/kb_invoice/{DocumentId}/comment",
            null,
            cts.Token);
    }

    private static Comment NewComment(int id) => new()
    {
        Id = id,
        Text = $"Comment {id}",
        UserId = 1,
        UserName = "Peter Smith"
    };
}
