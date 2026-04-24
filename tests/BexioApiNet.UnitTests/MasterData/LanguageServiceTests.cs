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
using BexioApiNet.Abstractions.Models.MasterData.Languages;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.UnitTests.MasterData;

/// <summary>
/// Offline unit tests for <see cref="LanguageService"/>. Verifies the read-only lookup forwards its
/// calls to <see cref="IBexioConnectionHandler"/> with the expected verb, path, and arguments.
/// </summary>
[TestFixture]
public sealed class LanguageServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "2.0/language";
    private const string ExpectedSearchEndpoint = "2.0/language/search";

    private LanguageService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="LanguageService"/> per test bound to the base-fixture substitute.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new LanguageService(ConnectionHandler);
    }

    /// <summary>
    /// <c>Get</c> hits <c>2.0/language</c> via <see cref="IBexioConnectionHandler.GetAsync{TResult}"/>
    /// with a <see langword="null"/> query parameter when no caller parameters are supplied.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsync_WithExpectedPath()
    {
        var response = new ApiResult<List<Language>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Language>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Get();

        result.ShouldBeSameAs(response);
        await ConnectionHandler.Received(1).GetAsync<List<Language>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A supplied <see cref="QueryParameterLanguage"/> is forwarded verbatim to the handler.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterLanguage(Limit: 50, Offset: 25);
        ConnectionHandler
            .GetAsync<List<Language>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Language>?> { IsSuccess = true, Data = [] }));

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Language>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Search</c> posts the supplied <see cref="SearchCriteria"/> list to
    /// <c>2.0/language/search</c> via <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}"/>.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsync_WithExpectedPathAndBody()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "German", Criteria = "=" }
        };
        var response = new ApiResult<List<Language>>
        {
            IsSuccess = true,
            Data = [new Language { Id = 1, Name = "German", Iso6391 = "de" }]
        };
        ConnectionHandler
            .PostSearchAsync<Language>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Search(criteria);

        result.ShouldBeSameAs(response);
        await ConnectionHandler.Received(1).PostSearchAsync<Language>(
            criteria,
            ExpectedSearchEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A <see cref="QueryParameterLanguage"/> supplied to <c>Search</c> is forwarded to the handler.
    /// </summary>
    [Test]
    public async Task Search_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "iso_639_1", Value = "de", Criteria = "=" }
        };
        var queryParameter = new QueryParameterLanguage(Limit: 10, Offset: 0);
        ConnectionHandler
            .PostSearchAsync<Language>(Arg.Any<List<SearchCriteria>>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<Language>> { IsSuccess = true, Data = [] }));

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<Language>(
            criteria,
            ExpectedSearchEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }
}
