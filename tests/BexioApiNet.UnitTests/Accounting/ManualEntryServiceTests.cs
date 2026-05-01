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
using BexioApiNet.Abstractions.Models.Accounting.ManualEntries;
using BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Enums;
using BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Views;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.UnitTests.Accounting;

/// <summary>
/// Offline unit tests for <see cref="ManualEntryService" />. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
/// returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class ManualEntryServiceTests : ServiceTestBase
{
    /// <summary>
    /// Creates a fresh <see cref="ManualEntryService" /> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new ManualEntryService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "3.0/accounting/manual_entries";

    private ManualEntryService _sut = null!;

    /// <summary>
    /// Create forwards the payload and the expected endpoint path to
    /// <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<ManualEntry> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<ManualEntry, ManualEntryEntryCreate>(
                Arg.Any<ManualEntryEntryCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<ManualEntry, ManualEntryEntryCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}" /> produced by the connection handler without
    /// modification.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<ManualEntry> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<ManualEntry, ManualEntryEntryCreate>(
                Arg.Any<ManualEntryEntryCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// AddAttachment (MemoryStream overload) forwards the provided file tuples to
    /// <see cref="IBexioConnectionHandler.PostMultiPartFileAsync{TResult}" />.
    /// </summary>
    [Test]
    public async Task AddAttachment_WithStreams_CallsPostMultiPartFileAsync()
    {
        var files = new List<Tuple<MemoryStream, string>>
        {
            Tuple.Create(new MemoryStream([1, 2, 3]), "test.pdf")
        };
        var response = new ApiResult<IReadOnlyList<ManualEntryEntryFile>>
        {
            IsSuccess = true,
            Data = []
        };
        ConnectionHandler
            .PostMultiPartFileAsync<IReadOnlyList<ManualEntryEntryFile>>(
                Arg.Any<List<Tuple<MemoryStream, string>>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.AddAttachment(1, 2, files);

        await ConnectionHandler.Received(1).PostMultiPartFileAsync<IReadOnlyList<ManualEntryEntryFile>>(
            files,
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// AddAttachment builds the request path from the root manual-entries id and the nested
    /// manual-entry id. Both identifiers must be present in the final URL.
    /// </summary>
    [Test]
    public async Task AddAttachment_PathContainsManuelEntriesIdAndManuelEntryId()
    {
        const int manualEntriesId = 42;
        const int manualEntryId = 7;
        var files = new List<Tuple<MemoryStream, string>>
        {
            Tuple.Create(new MemoryStream([1, 2, 3]), "test.pdf")
        };
        var response = new ApiResult<IReadOnlyList<ManualEntryEntryFile>>
        {
            IsSuccess = true,
            Data = []
        };
        string? capturedPath = null;
        ConnectionHandler
            .PostMultiPartFileAsync<IReadOnlyList<ManualEntryEntryFile>>(
                Arg.Any<List<Tuple<MemoryStream, string>>>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.AddAttachment(manualEntriesId, manualEntryId, files);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{manualEntriesId}/entries/{manualEntryId}/files"));
    }

    /// <summary>
    /// Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    /// the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<ManualEntry>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<ManualEntry>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<ManualEntry>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get (autoPage = true) triggers <see cref="IBexioConnectionHandler.FetchAll{TResult}" /> when
    /// the <c>X-Total-Count</c> header is present and the initial response only returned a page of
    /// the full result set.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_CallsFetchAll()
    {
        const int totalResults = 10;
        var initialData = new List<ManualEntry> { BuildManualEntry(1), BuildManualEntry(2) };
        var initial = new ApiResult<List<ManualEntry>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<ManualEntry>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<ManualEntry>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<ManualEntry>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}" /> produced by the connection handler when
    /// auto-paging is not requested (no additional FetchAll round-trip, result passes through).
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<ManualEntry>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<ManualEntry>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> exactly once.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDelete()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(42);

        await ConnectionHandler.Received(1).Delete(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete builds the request path with the manual-entry id appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task Delete_PathContainsId()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    /// Put forwards the payload and builds the path <c>{endpoint}/{manualEntryId}</c> via
    /// <see cref="IBexioConnectionHandler.PutAsync{TResult,TUpdate}" />.
    /// </summary>
    [Test]
    public async Task Put_CallsPutAsyncWithExpectedPath()
    {
        const int manualEntryId = 42;
        var payload = BuildUpdatePayload();
        var response = new ApiResult<ManualEntry> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PutAsync<ManualEntry, ManualEntryUpdate>(
                Arg.Any<ManualEntryUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Put(manualEntryId, payload);

        await ConnectionHandler.Received(1).PutAsync<ManualEntry, ManualEntryUpdate>(
            payload,
            $"{ExpectedEndpoint}/{manualEntryId}",
            Arg.Any<CancellationToken>());
        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{manualEntryId}"));
    }

    /// <summary>
    /// Put returns the <see cref="ApiResult{T}" /> produced by the connection handler unchanged.
    /// </summary>
    [Test]
    public async Task Put_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildUpdatePayload();
        var response = new ApiResult<ManualEntry> { IsSuccess = true };
        ConnectionHandler
            .PutAsync<ManualEntry, ManualEntryUpdate>(
                Arg.Any<ManualEntryUpdate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Put(1, payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetNextRefNr builds <c>{endpoint}/next_ref_nr</c> and forwards to
    /// <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with a null query parameter.
    /// </summary>
    [Test]
    public async Task GetNextRefNr_CallsGetAsyncWithExpectedPath()
    {
        var response = new ApiResult<ManualEntryNextRefNr> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<ManualEntryNextRefNr>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetNextRefNr();

        await ConnectionHandler.Received(1).GetAsync<ManualEntryNextRefNr>(
            $"{ExpectedEndpoint}/next_ref_nr",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetFiles (no auto-page) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once
    /// with the expected compound-entry file listing path and does not trigger FetchAll.
    /// </summary>
    [Test]
    public async Task GetFiles_WithoutAutoPage_CallsGetAsyncWithExpectedPath()
    {
        const int manualEntryId = 42;
        var response = new ApiResult<List<ManualEntryFile>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<ManualEntryFile>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetFiles(manualEntryId);

        await ConnectionHandler.Received(1).GetAsync<List<ManualEntryFile>?>(
            $"{ExpectedEndpoint}/{manualEntryId}/files",
            null,
            Arg.Any<CancellationToken>());
        await ConnectionHandler.DidNotReceive().FetchAll<ManualEntryFile>(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetFiles (autoPage = true) triggers
    /// <see cref="IBexioConnectionHandler.FetchAll{TResult}" /> when the <c>X-Total-Count</c>
    /// header is present and the initial response only returned a page.
    /// </summary>
    [Test]
    public async Task GetFiles_WithAutoPage_CallsFetchAll()
    {
        const int manualEntryId = 42;
        const int totalResults = 10;
        var initialData = new List<ManualEntryFile> { BuildManualEntryFile(1), BuildManualEntryFile(2) };
        var initial = new ApiResult<List<ManualEntryFile>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<ManualEntryFile>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<ManualEntryFile>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.GetFiles(manualEntryId, autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<ManualEntryFile>(
            initialData.Count,
            totalResults,
            $"{ExpectedEndpoint}/{manualEntryId}/files",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetFileById builds <c>{endpoint}/{manualEntryId}/files/{fileId}</c> and forwards via
    /// <see cref="IBexioConnectionHandler.GetAsync{TResult}" />.
    /// </summary>
    [Test]
    public async Task GetFileById_CallsGetAsyncWithExpectedPath()
    {
        const int manualEntryId = 42;
        const int fileId = 7;
        var response = new ApiResult<ManualEntryFileDetail> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<ManualEntryFileDetail>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetFileById(manualEntryId, fileId);

        await ConnectionHandler.Received(1).GetAsync<ManualEntryFileDetail>(
            $"{ExpectedEndpoint}/{manualEntryId}/files/{fileId}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetEntryFiles builds <c>{endpoint}/{manualEntryId}/entries/{entryId}/files</c> and forwards
    /// via <see cref="IBexioConnectionHandler.GetAsync{TResult}" />.
    /// </summary>
    [Test]
    public async Task GetEntryFiles_CallsGetAsyncWithExpectedPath()
    {
        const int manualEntryId = 42;
        const int entryId = 7;
        var response = new ApiResult<List<ManualEntryFile>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<ManualEntryFile>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetEntryFiles(manualEntryId, entryId);

        await ConnectionHandler.Received(1).GetAsync<List<ManualEntryFile>?>(
            $"{ExpectedEndpoint}/{manualEntryId}/entries/{entryId}/files",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetEntryFiles (autoPage = true) triggers
    /// <see cref="IBexioConnectionHandler.FetchAll{TResult}" /> on the entry-files path.
    /// </summary>
    [Test]
    public async Task GetEntryFiles_WithAutoPage_CallsFetchAllOnEntryPath()
    {
        const int manualEntryId = 42;
        const int entryId = 7;
        const int totalResults = 8;
        var initialData = new List<ManualEntryFile> { BuildManualEntryFile(1) };
        var initial = new ApiResult<List<ManualEntryFile>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<ManualEntryFile>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<ManualEntryFile>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.GetEntryFiles(manualEntryId, entryId, autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<ManualEntryFile>(
            initialData.Count,
            totalResults,
            $"{ExpectedEndpoint}/{manualEntryId}/entries/{entryId}/files",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// GetEntryFileById builds <c>{endpoint}/{manualEntryId}/entries/{entryId}/files/{fileId}</c>
    /// and forwards via <see cref="IBexioConnectionHandler.GetAsync{TResult}" />.
    /// </summary>
    [Test]
    public async Task GetEntryFileById_CallsGetAsyncWithExpectedPath()
    {
        const int manualEntryId = 42;
        const int entryId = 7;
        const int fileId = 3;
        var response = new ApiResult<ManualEntryFileDetail> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<ManualEntryFileDetail>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetEntryFileById(manualEntryId, entryId, fileId);

        await ConnectionHandler.Received(1).GetAsync<ManualEntryFileDetail>(
            $"{ExpectedEndpoint}/{manualEntryId}/entries/{entryId}/files/{fileId}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// DeleteFile builds the request path <c>{endpoint}/{manualEntryId}/files/{fileId}</c> and
    /// forwards to <see cref="IBexioConnectionHandler.Delete" />.
    /// </summary>
    [Test]
    public async Task DeleteFile_CallsDeleteWithExpectedPath()
    {
        const int manualEntryId = 42;
        const int fileId = 7;
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.DeleteFile(manualEntryId, fileId);

        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedEndpoint}/{manualEntryId}/files/{fileId}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// DeleteEntryFile builds the request path
    /// <c>{endpoint}/{manualEntryId}/entries/{entryId}/files/{fileId}</c> and forwards to
    /// <see cref="IBexioConnectionHandler.Delete" />.
    /// </summary>
    [Test]
    public async Task DeleteEntryFile_CallsDeleteWithExpectedPath()
    {
        const int manualEntryId = 42;
        const int entryId = 7;
        const int fileId = 3;
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.DeleteEntryFile(manualEntryId, entryId, fileId);

        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedEndpoint}/{manualEntryId}/entries/{entryId}/files/{fileId}",
            Arg.Any<CancellationToken>());
    }

    private static ManualEntryEntryCreate BuildCreatePayload()
    {
        return new ManualEntryEntryCreate(
            ManualEntryType.manual_single_entry,
            new DateOnly(2025, 1, 1),
            "REF-1",
            []);
    }

    private static ManualEntryUpdate BuildUpdatePayload()
    {
        return new ManualEntryUpdate(
            ManualEntryType.manual_single_entry,
            new DateOnly(2025, 1, 1),
            "REF-1",
            [],
            null);
    }

    private static ManualEntry BuildManualEntry(int id)
    {
        return new ManualEntry(
            id,
            "manual_single_entry",
            new DateOnly(2025, 1, 1),
            string.Empty,
            null,
            null,
            [],
            null,
            string.Empty);
    }

    private static ManualEntryFile BuildManualEntryFile(int id)
    {
        return new ManualEntryFile(
            id,
            Guid.NewGuid().ToString(),
            $"file-{id}.pdf",
            1024,
            "pdf",
            "application/pdf",
            null,
            1,
            false,
            null,
            false,
            DateTime.UtcNow);
    }
}
