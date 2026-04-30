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
using BexioApiNet.Abstractions.Models.Payroll.Paystubs;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Payroll;

namespace BexioApiNet.UnitTests.Payroll;

/// <summary>
///     Offline unit tests for <see cref="PaystubService" />. Each test asserts that the service
///     forwards its call to <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the
///     expected nested path and returns the handler's <see cref="ApiResult{T}" /> unchanged.
///     The Bexio v4.0 paystub endpoint (
///     <c>4.0/payroll/employees/{employeeId}/paystub-pdf/{year}/{month}</c>) responds with a
///     JSON envelope containing a <see cref="Paystub.Location" /> URI rather than streaming
///     the PDF inline. No network, no filesystem.
/// </summary>
[TestFixture]
[Category("Unit")]
public sealed class PaystubServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="PaystubService" /> bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute before each test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new PaystubService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "4.0/payroll/employees";

    private PaystubService _sut = null!;

    /// <summary>
    ///     <c>GetPdf</c> forwards the call to
    ///     <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the employee id, year
    ///     and month embedded in the path and a <see langword="null" /> query parameter, and
    ///     returns the handler's result unchanged.
    /// </summary>
    [Test]
    public async Task GetPdf_CallsGetAsync_WithExpectedPath()
    {
        var employeeId = Guid.NewGuid();
        const int year = 2026;
        const int month = 1;
        var expected = new ApiResult<Paystub>
        {
            IsSuccess = true,
            Data = new Paystub(new Uri("https://example.test/paystub.pdf"))
        };
        ConnectionHandler
            .GetAsync<Paystub>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.GetPdf(employeeId, year, month);

        await ConnectionHandler.Received(1).GetAsync<Paystub>(
            $"{ExpectedEndpoint}/{employeeId}/paystub-pdf/{year}/{month}",
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(expected));
    }

    /// <summary>
    ///     <c>GetPdf</c> forwards the supplied <see cref="CancellationToken" /> through to the
    ///     connection handler so callers can cancel the call in-flight.
    /// </summary>
    [Test]
    public async Task GetPdf_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<Paystub>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), cts.Token)
            .Returns(new ApiResult<Paystub> { IsSuccess = true });

        await _sut.GetPdf(Guid.NewGuid(), 2026, 1, cts.Token);

        await ConnectionHandler.Received(1).GetAsync<Paystub>(
            Arg.Any<string>(), Arg.Any<QueryParameter?>(), cts.Token);
    }
}
