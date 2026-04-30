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
///     Integration tests for <see cref="PaystubService" /> against WireMock stubs. Verifies
///     the nested path composed from <see cref="PaystubConfiguration" />
///     (<c>4.0/payroll/employees/{employeeId}/paystub-pdf/{year}/{month}</c>) reaches the
///     handler correctly, that a <c>GET</c> verb is used and that the JSON envelope
///     containing the <c>location</c> URL deserializes into the C# model.
/// </summary>
[Category("Integration")]
public sealed class PaystubServiceIntegrationTests : IntegrationTestBase
{
    private static readonly Guid TestEmployeeId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479");

    private const string PaystubResponse = """
                                           {
                                               "location": "https://example.test/paystub/2026-01.pdf"
                                           }
                                           """;

    /// <summary>
    ///     <c>PaystubService.GetPdf</c> issues a <c>GET</c> against
    ///     <c>/4.0/payroll/employees/{employeeId}/paystub-pdf/{year}/{month}</c> and
    ///     deserializes the JSON envelope, surfacing the <see cref="System.Uri" /> on the
    ///     C# model.
    /// </summary>
    [Test]
    public async Task PaystubService_GetPdf_SendsGetRequest_AndReturnsLocation()
    {
        const int year = 2026;
        const int month = 1;
        var expectedPath = $"/4.0/payroll/employees/{TestEmployeeId}/paystub-pdf/{year}/{month}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(PaystubResponse));

        var service = new PaystubService(ConnectionHandler);

        var result = await service.GetPdf(TestEmployeeId, year, month, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Location, Is.Not.Null);
            Assert.That(result.Data.Location!.ToString(), Is.EqualTo("https://example.test/paystub/2026-01.pdf"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
