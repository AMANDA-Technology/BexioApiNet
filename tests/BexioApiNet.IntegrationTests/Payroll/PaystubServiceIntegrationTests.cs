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

using BexioApiNet.Services.Connectors.Payroll;

namespace BexioApiNet.IntegrationTests.Payroll;

/// <summary>
///     Integration tests for <see cref="PaystubService" /> against WireMock stubs. Verifies the
///     nested path composed from <see cref="PaystubConfiguration" />
///     (<c>4.0/payroll/employees/{employeeId}/paystub-pdf/{year}/{month}</c>) reaches the
///     handler correctly, the PDF bytes are returned verbatim on success and a <c>GET</c>
///     verb is used.
/// </summary>
[Category("Integration")]
public sealed class PaystubServiceIntegrationTests : IntegrationTestBase
{
    /// <summary>
    ///     <c>PaystubService.GetPdf</c> issues a <c>GET</c> against
    ///     <c>/4.0/payroll/employees/{employeeId}/paystub-pdf/{year}/{month}</c> and
    ///     surfaces the raw PDF bytes on success.
    /// </summary>
    [Test]
    public async Task PaystubService_GetPdf_SendsGetRequest_AndReturnsBytes()
    {
        const int employeeId = 1;
        const int year = 2026;
        const int month = 1;
        var expectedPath = $"/4.0/payroll/employees/{employeeId}/paystub-pdf/{year}/{month}";
        var pdfBytes = "%PDF-1.4 fake"u8.ToArray();

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/pdf")
                .WithBody(pdfBytes));

        var service = new PaystubService(ConnectionHandler);

        var result = await service.GetPdf(employeeId, year, month, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.EqualTo(pdfBytes));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
