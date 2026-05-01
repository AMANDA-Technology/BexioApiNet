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
using BexioApiNet.Abstractions.Models.Items.Items.Views;
using BexioApiNet.Services.Connectors.Items;

namespace BexioApiNet.IntegrationTests.Items;

/// <summary>
/// Integration tests covering the CRUD entry points of <see cref="ItemService"/> against
/// WireMock stubs. Verifies the path composed from <see cref="ItemConfiguration"/>
/// (<c>2.0/article</c>) reaches the handler correctly, that the expected HTTP verbs are used
/// (Bexio uses <c>POST</c> for both create and edit), and that payloads are serialized with
/// the expected snake_case field names.
/// </summary>
public sealed class ItemServiceIntegrationTests : IntegrationTestBase
{
    private const string ItemsPath = "/2.0/article";

    private const string ItemResponse = """
        {
            "id": 1,
            "user_id": 1,
            "article_type_id": 2,
            "contact_id": null,
            "deliverer_code": null,
            "deliverer_name": null,
            "deliverer_description": null,
            "intern_code": "wh-2019",
            "intern_name": "Webhosting",
            "intern_description": null,
            "purchase_price": null,
            "sale_price": "49.90",
            "purchase_total": null,
            "sale_total": null,
            "currency_id": null,
            "tax_income_id": null,
            "tax_id": null,
            "tax_expense_id": null,
            "unit_id": null,
            "is_stock": false,
            "stock_id": null,
            "stock_place_id": null,
            "stock_nr": 0,
            "stock_min_nr": 0,
            "stock_reserved_nr": 0,
            "stock_available_nr": 0,
            "stock_picked_nr": 0,
            "stock_disposed_nr": 0,
            "stock_ordered_nr": 0,
            "width": null,
            "height": null,
            "weight": null,
            "volume": null,
            "html_text": null,
            "remarks": null,
            "delivery_price": null,
            "article_group_id": null,
            "account_id": null,
            "expense_account_id": null
        }
        """;

    /// <summary>
    /// <c>ItemService.Get()</c> must issue a <c>GET</c> request against <c>/2.0/article</c>
    /// and deserialize each returned <see cref="BexioApiNet.Abstractions.Models.Items.Items.Item"/>
    /// from the OpenAPI-shaped JSON array returned by Bexio.
    /// </summary>
    [Test]
    public async Task ItemService_Get_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(ItemsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ItemResponse}]"));

        var service = new ItemService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ItemsPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].UserId, Is.EqualTo(1));
            Assert.That(result.Data![0].ArticleTypeId, Is.EqualTo(2));
            Assert.That(result.Data![0].InternCode, Is.EqualTo("wh-2019"));
            Assert.That(result.Data![0].InternName, Is.EqualTo("Webhosting"));
            Assert.That(result.Data![0].SalePrice, Is.EqualTo("49.90"));
            Assert.That(result.Data![0].IsStock, Is.False);
        });
    }

    /// <summary>
    /// <c>ItemService.GetById(1)</c> must issue a <c>GET</c> request against <c>/2.0/article/1</c>
    /// and deserialize every property defined on the OpenAPI Item schema.
    /// </summary>
    [Test]
    public async Task ItemService_GetById_SendsGetRequestWithIdInPath_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath($"{ItemsPath}/1").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ItemResponse));

        var service = new ItemService(ConnectionHandler);

        var result = await service.GetById(1, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo($"{ItemsPath}/1"));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(result.Data!.UserId, Is.EqualTo(1));
            Assert.That(result.Data!.ArticleTypeId, Is.EqualTo(2));
            Assert.That(result.Data!.ContactId, Is.Null);
            Assert.That(result.Data!.DelivererCode, Is.Null);
            Assert.That(result.Data!.InternCode, Is.EqualTo("wh-2019"));
            Assert.That(result.Data!.InternName, Is.EqualTo("Webhosting"));
            Assert.That(result.Data!.PurchasePrice, Is.Null);
            Assert.That(result.Data!.SalePrice, Is.EqualTo("49.90"));
            Assert.That(result.Data!.PurchaseTotal, Is.Null);
            Assert.That(result.Data!.SaleTotal, Is.Null);
            Assert.That(result.Data!.CurrencyId, Is.Null);
            Assert.That(result.Data!.TaxIncomeId, Is.Null);
            Assert.That(result.Data!.TaxId, Is.Null);
            Assert.That(result.Data!.TaxExpenseId, Is.Null);
            Assert.That(result.Data!.UnitId, Is.Null);
            Assert.That(result.Data!.IsStock, Is.False);
            Assert.That(result.Data!.StockId, Is.Null);
            Assert.That(result.Data!.StockPlaceId, Is.Null);
            Assert.That(result.Data!.StockNr, Is.EqualTo(0));
            Assert.That(result.Data!.StockMinNr, Is.EqualTo(0));
            Assert.That(result.Data!.StockReservedNr, Is.EqualTo(0));
            Assert.That(result.Data!.StockAvailableNr, Is.EqualTo(0));
            Assert.That(result.Data!.StockPickedNr, Is.EqualTo(0));
            Assert.That(result.Data!.StockDisposedNr, Is.EqualTo(0));
            Assert.That(result.Data!.StockOrderedNr, Is.EqualTo(0));
            Assert.That(result.Data!.Width, Is.Null);
            Assert.That(result.Data!.Height, Is.Null);
            Assert.That(result.Data!.Weight, Is.Null);
            Assert.That(result.Data!.Volume, Is.Null);
            Assert.That(result.Data!.HtmlText, Is.Null);
            Assert.That(result.Data!.Remarks, Is.Null);
            Assert.That(result.Data!.DeliveryPrice, Is.Null);
            Assert.That(result.Data!.ArticleGroupId, Is.Null);
            Assert.That(result.Data!.AccountId, Is.Null);
            Assert.That(result.Data!.ExpenseAccountId, Is.Null);
        });
    }

    /// <summary>
    /// <c>ItemService.Create()</c> must issue a <c>POST</c> request against <c>/2.0/article</c>
    /// with a body containing the expected snake_case field names.
    /// </summary>
    [Test]
    public async Task ItemService_Create_SendsPostRequestWithBody()
    {
        Server
            .Given(Request.Create().WithPath(ItemsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ItemResponse));

        var service = new ItemService(ConnectionHandler);
        var payload = new ItemCreate(
            UserId: 1,
            ArticleTypeId: 2,
            InternCode: "wh-2019",
            InternName: "Webhosting",
            SalePrice: "49.90");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ItemsPath));
            Assert.That(request.Body, Does.Contain("intern_code"));
            Assert.That(request.Body, Does.Contain("intern_name"));
            Assert.That(request.Body, Does.Contain("article_type_id"));
        });
    }

    /// <summary>
    /// <c>ItemService.Search()</c> must issue a <c>POST</c> request against <c>/2.0/article/search</c>
    /// with the search criteria serialized as the request body and deserialize the populated
    /// JSON array returned by Bexio.
    /// </summary>
    [Test]
    public async Task ItemService_Search_SendsPostRequestToSearchPath_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath($"{ItemsPath}/search").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ItemResponse}]"));

        var service = new ItemService(ConnectionHandler);
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "intern_name", Value = "Webhosting", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo($"{ItemsPath}/search"));
            Assert.That(request.Body, Does.Contain("\"field\":\"intern_name\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"Webhosting\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].InternName, Is.EqualTo("Webhosting"));
        });
    }

    /// <summary>
    /// <c>ItemService.Update(1, ...)</c> must issue a <c>POST</c> request against
    /// <c>/2.0/article/1</c> — Bexio uses POST for item edits.
    /// </summary>
    [Test]
    public async Task ItemService_Update_SendsPostRequestWithIdInPath()
    {
        Server
            .Given(Request.Create().WithPath($"{ItemsPath}/1").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ItemResponse));

        var service = new ItemService(ConnectionHandler);
        var payload = new ItemUpdate(
            UserId: 1,
            InternCode: "wh-2019",
            InternName: "Webhosting Updated");

        var result = await service.Update(1, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo($"{ItemsPath}/1"));
            Assert.That(request.Body, Does.Contain("intern_name"));
        });
    }

    /// <summary>
    /// <c>ItemService.Delete(1)</c> must issue a <c>DELETE</c> request against <c>/2.0/article/1</c>.
    /// </summary>
    [Test]
    public async Task ItemService_Delete_SendsDeleteRequestWithIdInPath()
    {
        Server
            .Given(Request.Create().WithPath($"{ItemsPath}/1").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new ItemService(ConnectionHandler);

        var result = await service.Delete(1, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo($"{ItemsPath}/1"));
        });
    }
}
