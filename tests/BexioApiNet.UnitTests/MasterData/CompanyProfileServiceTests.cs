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
using BexioApiNet.Abstractions.Models.MasterData.CompanyProfiles;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.UnitTests.MasterData;

/// <summary>
/// Offline unit tests for <see cref="CompanyProfileService"/>. Verifies the read-only lookup
/// forwards list/by-id calls to <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the
/// expected paths.
/// </summary>
[TestFixture]
public sealed class CompanyProfileServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "2.0/company_profile";

    private CompanyProfileService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="CompanyProfileService"/> per test bound to the base-fixture substitute.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new CompanyProfileService(ConnectionHandler);
    }

    /// <summary>
    /// <c>Get</c> hits <c>2.0/company_profile</c> once with a <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsync_WithExpectedPath()
    {
        var response = new ApiResult<List<CompanyProfile>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<CompanyProfile>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.Get();

        result.ShouldBeSameAs(response);
        await ConnectionHandler.Received(1).GetAsync<List<CompanyProfile>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>GetById(id)</c> composes <c>2.0/company_profile/{id}</c> and forwards to the handler.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithComposedIdPath()
    {
        const int id = 42;
        var response = new ApiResult<CompanyProfile>
        {
            IsSuccess = true,
            Data = new CompanyProfile { Id = id, Name = "bexio AG" }
        };
        ConnectionHandler
            .GetAsync<CompanyProfile>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var result = await _sut.GetById(id);

        result.ShouldBeSameAs(response);
        await ConnectionHandler.Received(1).GetAsync<CompanyProfile>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }
}
