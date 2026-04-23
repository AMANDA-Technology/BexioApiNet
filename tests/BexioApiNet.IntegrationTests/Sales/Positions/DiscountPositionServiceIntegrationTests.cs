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

using BexioApiNet.Abstractions.Models.Sales.Positions;
using BexioApiNet.Services.Connectors.Sales.Positions;

namespace BexioApiNet.IntegrationTests.Sales.Positions;

/// <summary>
/// Smoke-level integration tests for <see cref="DiscountPositionService"/> against WireMock stubs.
/// Verifies that the correct HTTP method and URL path are used for each of the 5 operations.
/// </summary>
public sealed class DiscountPositionServiceIntegrationTests : IntegrationTestBase
{
    private const string DocumentType = "kb_invoice";
    private const int DocumentId = 1;
    private const int PositionId = 10;
    private static readonly string ListPath = $"/2.0/{DocumentType}/{DocumentId}/kb_position_discount";
    private static readonly string SinglePath = $"/2.0/{DocumentType}/{DocumentId}/kb_position_discount/{PositionId}";

    private const string DiscountPositionResponse = """
        {
            "id": 10,
            "type": "KbPositionDiscount",
            "text": "5% loyalty discount",
            "is_percentual": true,
            "value": "5.000000",
            "discount_total": "25.00"
        }
        """;

    /// <summary>
    /// GetAll must issue a <c>GET</c> request to the list path and return a successful result.
    /// </summary>
    [Test]
    public async Task DiscountPositionService_GetAll_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(ListPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new DiscountPositionService(ConnectionHandler);

        var result = await service.GetAll(DocumentType, DocumentId, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ListPath));
        });
    }

    /// <summary>
    /// GetById must issue a <c>GET</c> request that includes the position id in the path.
    /// </summary>
    [Test]
    public async Task DiscountPositionService_GetById_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(DiscountPositionResponse));

        var service = new DiscountPositionService(ConnectionHandler);

        var result = await service.GetById(DocumentType, DocumentId, PositionId, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.InstanceOf<PositionDiscount>());
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
        });
    }

    /// <summary>
    /// Create must issue a <c>POST</c> request to the list path and return the created position.
    /// </summary>
    [Test]
    public async Task DiscountPositionService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(ListPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(DiscountPositionResponse));

        var service = new DiscountPositionService(ConnectionHandler);
        var payload = new PositionDiscount { Text = "5% loyalty discount", IsPercentual = true, Value = "5.000000" };

        var result = await service.Create(DocumentType, DocumentId, payload, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ListPath));
        });
    }

    /// <summary>
    /// Update must issue a <c>POST</c> request to the single-position path — Bexio uses POST
    /// for position updates rather than PUT.
    /// </summary>
    [Test]
    public async Task DiscountPositionService_Update_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(DiscountPositionResponse));

        var service = new DiscountPositionService(ConnectionHandler);
        var payload = new PositionDiscount { Text = "Updated discount", IsPercentual = false, Value = "50.000000" };

        var result = await service.Update(DocumentType, DocumentId, PositionId, payload, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
        });
    }

    /// <summary>
    /// Delete must issue a <c>DELETE</c> request to the single-position path.
    /// </summary>
    [Test]
    public async Task DiscountPositionService_Delete_SendsDeleteRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new DiscountPositionService(ConnectionHandler);

        var result = await service.Delete(DocumentType, DocumentId, PositionId, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
        });
    }
}
