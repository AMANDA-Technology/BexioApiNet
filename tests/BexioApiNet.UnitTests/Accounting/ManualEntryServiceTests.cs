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
///     Offline unit tests for <see cref="ManualEntryService" />. Each test verifies that the service
///     forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
///     returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class ManualEntryServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="ManualEntryService" /> per test, bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new ManualEntryService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "3.0/accounting/manual_entries";

    private ManualEntryService _sut = null!;

    /// <summary>
    ///     Create forwards the payload and the expected endpoint path to
    ///     <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
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
    ///     Create returns the <see cref="ApiResult{T}" /> produced by the connection handler without
    ///     modification.
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
    ///     AddAttachment (MemoryStream overload) forwards the provided file tuples to
    ///     <see cref="IBexioConnectionHandler.PostMultiPartFileAsync{TResult}" />.
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
    ///     AddAttachment builds the request path from the root manual-entries id and the nested
    ///     manual-entry id. Both identifiers must be present in the final URL.
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
    ///     Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    ///     the expected endpoint path and a null query parameter.
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
    ///     Get (autoPage = true) triggers <see cref="IBexioConnectionHandler.FetchAll{TResult}" /> when
    ///     the <c>X-Total-Count</c> header is present and the initial response only returned a page of
    ///     the full result set.
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
    ///     Get returns the <see cref="ApiResult{T}" /> produced by the connection handler when
    ///     auto-paging is not requested (no additional FetchAll round-trip, result passes through).
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
    ///     Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> exactly once.
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
    ///     Delete builds the request path with the manual-entry id appended to the endpoint root.
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

    private static ManualEntryEntryCreate BuildCreatePayload()
    {
        return new ManualEntryEntryCreate(
            ManualEntryType.manual_single_entry,
            new DateOnly(2025, 1, 1),
            "REF-1",
            []);
    }

    private static ManualEntry BuildManualEntry(int id)
    {
        return new ManualEntry(
            id,
            "manual_single_entry",
            "manual",
            new DateOnly(2025, 1, 1),
            string.Empty,
            null,
            null,
            [],
            null,
            string.Empty);
    }
}
