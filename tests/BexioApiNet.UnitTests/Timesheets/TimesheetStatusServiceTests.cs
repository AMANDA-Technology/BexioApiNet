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
using BexioApiNet.Abstractions.Models.Timesheets.TimesheetStatus;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Timesheets;

namespace BexioApiNet.UnitTests.Timesheets;

/// <summary>
/// Offline unit tests for <see cref="TimesheetStatusService" />. The service exposes only
/// a read-only <c>Get</c> lookup; these tests verify it forwards to
/// <see cref="IBexioConnectionHandler" /> with the canonical path and null query parameter.
/// </summary>
[TestFixture]
public sealed class TimesheetStatusServiceTests : ServiceTestBase
{
    /// <summary>
    /// Creates a fresh <see cref="TimesheetStatusService" /> bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler" /> substitute for every test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new TimesheetStatusService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/timesheet_status";

    private TimesheetStatusService _sut = null!;

    /// <summary>
    /// Get performs a single <c>GetAsync</c> against <c>2.0/timesheet_status</c>
    /// with a null query parameter and forwards the response verbatim.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsyncOnceWithExpectedPathAndNullQuery()
    {
        var expected = new ApiResult<List<TimesheetStatus>>
        {
            IsSuccess = true,
            Data = [new TimesheetStatus(Id: 1, Name: "Open"), new TimesheetStatus(Id: 2, Name: "In Progress")]
        };
        ConnectionHandler
            .GetAsync<List<TimesheetStatus>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<TimesheetStatus>>(
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
            .GetAsync<List<TimesheetStatus>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<TimesheetStatus>> { IsSuccess = true, Data = [] }));

        await _sut.Get(cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<TimesheetStatus>>(
            ExpectedEndpoint,
            null,
            cts.Token);
    }
}
