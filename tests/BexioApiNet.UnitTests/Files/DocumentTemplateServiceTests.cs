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
using BexioApiNet.Abstractions.Models.Files.DocumentTemplates;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Files;

namespace BexioApiNet.UnitTests.Files;

/// <summary>
/// Offline unit tests for <see cref="DocumentTemplateService" />. The service exposes only
/// a read-only <c>Get</c> lookup against the v3.0 <c>document_templates</c> route; these tests
/// verify it forwards to <see cref="IBexioConnectionHandler" /> with the canonical path and
/// null query parameter.
/// </summary>
[TestFixture]
public sealed class DocumentTemplateServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "3.0/document_templates";

    /// <summary>
    /// Creates a fresh <see cref="DocumentTemplateService" /> bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler" /> substitute for every test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new DocumentTemplateService(ConnectionHandler);
    }

    private DocumentTemplateService _sut = null!;

    /// <summary>
    /// Get performs a single <c>GetAsync</c> against <c>3.0/document_templates</c>
    /// with a null query parameter and forwards the response verbatim.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsyncOnceWithExpectedPathAndNullQuery()
    {
        var expected = new ApiResult<List<DocumentTemplate>>
        {
            IsSuccess = true,
            Data = []
        };
        ConnectionHandler
            .GetAsync<List<DocumentTemplate>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Get();

        result.ShouldBeSameAs(expected);
        await ConnectionHandler.Received(1).GetAsync<List<DocumentTemplate>>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied by the caller must be forwarded to the connection
    /// handler on <c>Get</c> so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<DocumentTemplate>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<DocumentTemplate>> { IsSuccess = true, Data = [] }));

        await _sut.Get(cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<DocumentTemplate>>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }
}
