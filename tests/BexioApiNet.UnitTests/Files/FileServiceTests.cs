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
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Files.Files.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Files;
using BexioFile = BexioApiNet.Abstractions.Models.Files.Files.File;
using FileUsage = BexioApiNet.Abstractions.Models.Files.Files.FileUsage;

namespace BexioApiNet.UnitTests.Files;

/// <summary>
///     Offline unit tests for <see cref="FileService" />. Each test verifies that the service
///     forwards its call to <see cref="IBexioConnectionHandler" /> with the expected verb, path,
///     and payload, and returns the handler's <see cref="ApiResult{T}" /> unchanged. No network,
///     no live Bexio calls. Temporary files (Upload FileInfo overload only) are created under the
///     OS temp folder and deleted in <c>[TearDown]</c>.
/// </summary>
[TestFixture]
public sealed class FileServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="FileService" /> per test, bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new FileService(ConnectionHandler);
    }

    /// <summary>
    ///     Cleans up any temporary files created by upload tests that exercise the
    ///     <see cref="FileInfo" /> overload.
    /// </summary>
    [TearDown]
    public void CleanUpTempFiles()
    {
        foreach (var path in _tempFiles)
            if (File.Exists(path))
                File.Delete(path);

        _tempFiles.Clear();
    }

    private const string ExpectedEndpoint = "3.0/files";

    private FileService _sut = null!;
    private readonly List<string> _tempFiles = [];

    /// <summary>
    ///     Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once
    ///     with the expected <c>3.0/files</c> path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<BexioFile>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<BexioFile>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<BexioFile>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get returns the <see cref="ApiResult{T}" /> produced by the connection handler unchanged
    ///     when <c>autoPage</c> is not requested (no FetchAll round-trip).
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<List<BexioFile>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<BexioFile>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     Get forwards the <see cref="QueryParameterFile.QueryParameter" /> payload to the handler
    ///     when a typed query parameter object is supplied.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_ForwardsUnderlyingQueryParameter()
    {
        var queryParameter = new QueryParameterFile(Offset: 10, OrderBy: "name_asc", ArchivedState: "all");
        var response = new ApiResult<List<BexioFile>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<BexioFile>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<BexioFile>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get (<c>autoPage = true</c>) triggers <see cref="IBexioConnectionHandler.FetchAll{TResult}" />
    ///     when the <c>X-Total-Count</c> header is present and the initial response only contained a
    ///     page of the total result set.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_CallsFetchAll()
    {
        const int totalResults = 10;
        var initialData = new List<BexioFile> { BuildFile(1), BuildFile(2) };
        var initial = new ApiResult<List<BexioFile>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<BexioFile>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<BexioFile>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<BexioFile>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     GetById builds the <c>3.0/files/{id}</c> path and forwards the call to
    ///     <see cref="IBexioConnectionHandler.GetAsync{TResult}" />.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<BexioFile> { IsSuccess = true, Data = BuildFile(id) };
        ConnectionHandler
            .GetAsync<BexioFile>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(id);

        await ConnectionHandler.Received(1).GetAsync<BexioFile>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     Download builds the <c>3.0/files/{id}/download</c> path and forwards the call to
    ///     <see cref="IBexioConnectionHandler.GetBinaryAsync" />.
    /// </summary>
    [Test]
    public async Task Download_CallsGetBinaryAsyncWithExpectedPath()
    {
        const int id = 42;
        var response = new ApiResult<byte[]> { IsSuccess = true, Data = [1, 2, 3] };
        ConnectionHandler
            .GetBinaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Download(id);

        await ConnectionHandler.Received(1).GetBinaryAsync(
            $"{ExpectedEndpoint}/{id}/download",
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     Download forwards the supplied <see cref="CancellationToken" /> to the connection
    ///     handler so callers can cancel a download in-flight.
    /// </summary>
    [Test]
    public async Task Download_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetBinaryAsync(Arg.Any<string>(), cts.Token)
            .Returns(new ApiResult<byte[]> { IsSuccess = true });

        await _sut.Download(1, cts.Token);

        await ConnectionHandler.Received(1).GetBinaryAsync(Arg.Any<string>(), cts.Token);
    }

    /// <summary>
    ///     Preview builds the <c>3.0/files/{id}/preview</c> path and forwards the call to
    ///     <see cref="IBexioConnectionHandler.GetBinaryAsync" />.
    /// </summary>
    [Test]
    public async Task Preview_CallsGetBinaryAsyncWithExpectedPath()
    {
        const int id = 42;
        var response = new ApiResult<byte[]> { IsSuccess = true, Data = [1, 2, 3] };
        ConnectionHandler
            .GetBinaryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Preview(id);

        await ConnectionHandler.Received(1).GetBinaryAsync(
            $"{ExpectedEndpoint}/{id}/preview",
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     Usage builds the <c>3.0/files/{id}/usage</c> path and forwards the call to
    ///     <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with a null query parameter.
    /// </summary>
    [Test]
    public async Task Usage_CallsGetAsyncWithExpectedPath()
    {
        const int id = 42;
        var response = new ApiResult<FileUsage>
        {
            IsSuccess = true,
            Data = new FileUsage(id, "KbInvoice", "RE-00001", "RE-00001")
        };
        ConnectionHandler
            .GetAsync<FileUsage>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Usage(id);

        await ConnectionHandler.Received(1).GetAsync<FileUsage>(
            $"{ExpectedEndpoint}/{id}/usage",
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     Upload (<see cref="MemoryStream" /> overload) forwards the supplied tuple list and the
    ///     <c>3.0/files</c> path to <see cref="IBexioConnectionHandler.PostMultiPartFileAsync{TResult}" />.
    /// </summary>
    [Test]
    public async Task Upload_WithStreams_CallsPostMultiPartFileAsync()
    {
        var files = new List<Tuple<MemoryStream, string>>
        {
            Tuple.Create(new MemoryStream([1, 2, 3]), "test.pdf")
        };
        var response = new ApiResult<IReadOnlyList<BexioFile>>
        {
            IsSuccess = true,
            Data = []
        };
        ConnectionHandler
            .PostMultiPartFileAsync<IReadOnlyList<BexioFile>>(
                Arg.Any<List<Tuple<MemoryStream, string>>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Upload(files);

        await ConnectionHandler.Received(1).PostMultiPartFileAsync<IReadOnlyList<BexioFile>>(
            files,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     Upload (<see cref="FileInfo" /> overload) reads the file bytes from disk, converts them to
    ///     in-memory streams paired with the original filename, and forwards the result to
    ///     <see cref="IBexioConnectionHandler.PostMultiPartFileAsync{TResult}" /> with the
    ///     <c>3.0/files</c> path.
    /// </summary>
    [Test]
    public async Task Upload_WithFileInfo_ReadsBytesAndCallsPostMultiPartFileAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"fileservicetest-{Guid.NewGuid():N}.pdf");
        byte[] expectedBytes = [9, 8, 7];
        await File.WriteAllBytesAsync(path, expectedBytes);
        _tempFiles.Add(path);

        var files = new List<FileInfo> { new(path) };

        List<Tuple<MemoryStream, string>>? capturedFiles = null;
        ConnectionHandler
            .PostMultiPartFileAsync<IReadOnlyList<BexioFile>>(
                Arg.Do<List<Tuple<MemoryStream, string>>>(f => capturedFiles = f),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<IReadOnlyList<BexioFile>> { IsSuccess = true, Data = [] });

        await _sut.Upload(files);

        await ConnectionHandler.Received(1).PostMultiPartFileAsync<IReadOnlyList<BexioFile>>(
            Arg.Any<List<Tuple<MemoryStream, string>>>(),
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
        Assert.That(capturedFiles, Is.Not.Null);
        Assert.That(capturedFiles, Has.Count.EqualTo(1));
        Assert.That(capturedFiles![0].Item2, Is.EqualTo(Path.GetFileName(path)));
        Assert.That(capturedFiles[0].Item1.ToArray(), Is.EqualTo(expectedBytes));
    }

    /// <summary>
    ///     Search forwards the supplied <see cref="SearchCriteria" /> list to
    ///     <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}" /> against the
    ///     <c>3.0/files/search</c> path, with a null query parameter when none is supplied.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsyncWithExpectedPathAndBody()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "report", Criteria = "like" }
        };
        var response = new ApiResult<List<BexioFile>>
        {
            IsSuccess = true,
            Data = [BuildFile(1)]
        };
        ConnectionHandler
            .PostSearchAsync<BexioFile>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Search(criteria);

        await ConnectionHandler.Received(1).PostSearchAsync<BexioFile>(
            criteria,
            $"{ExpectedEndpoint}/search",
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     When a <see cref="QueryParameterFile" /> is passed to <c>Search</c> the inner
    ///     <see cref="QueryParameter" /> is forwarded to the handler so pagination / archived-state
    ///     parameters reach the URI.
    /// </summary>
    [Test]
    public async Task Search_WithQueryParameter_PassesUnderlyingQueryParameterToHandler()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "invoice", Criteria = "=" }
        };
        var queryParameter = new QueryParameterFile(10, 0, ArchivedState: "not_archived");
        ConnectionHandler
            .PostSearchAsync<BexioFile>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<BexioFile>> { IsSuccess = true, Data = [] }));

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<BexioFile>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Patch forwards the <see cref="FilePatch" /> payload and the <c>3.0/files/{id}</c> path
    ///     to <see cref="IBexioConnectionHandler.PatchAsync{TResult,TPatch}" />.
    /// </summary>
    [Test]
    public async Task Patch_CallsPatchAsyncWithIdInPath()
    {
        const int id = 99;
        var payload = new FilePatch("renamed.png", true);
        var response = new ApiResult<BexioFile>
        {
            IsSuccess = true,
            Data = BuildFile(id)
        };
        ConnectionHandler
            .PatchAsync<BexioFile, FilePatch>(
                Arg.Any<FilePatch>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Patch(id, payload);

        await ConnectionHandler.Received(1).PatchAsync<BexioFile, FilePatch>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with the
    ///     <c>3.0/files/{id}</c> path.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDeleteWithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(id);

        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    private static BexioFile BuildFile(int id)
    {
        return new BexioFile(
            id,
            Guid.NewGuid(),
            $"file-{id}.pdf",
            1024,
            "pdf",
            "application/pdf",
            null,
            1,
            false,
            null,
            "web",
            false,
            DateTime.UtcNow);
    }
}
