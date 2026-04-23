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
    /// and return a successful <c>ApiResult</c> when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task ItemService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(ItemsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ItemService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ItemsPath));
        });
    }

    /// <summary>
    /// <c>ItemService.GetById(1)</c> must issue a <c>GET</c> request against <c>/2.0/article/1</c>
    /// and deserialize the returned JSON into an <see cref="Item"/> with correct field values.
    /// </summary>
    [Test]
    public async Task ItemService_GetById_SendsGetRequestWithIdInPath()
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
            Assert.That(result.Data?.Id, Is.EqualTo(1));
            Assert.That(result.Data?.InternName, Is.EqualTo("Webhosting"));
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
    /// with the search criteria serialized as the request body.
    /// </summary>
    [Test]
    public async Task ItemService_Search_SendsPostRequestToSearchPath()
    {
        Server
            .Given(Request.Create().WithPath($"{ItemsPath}/search").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

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
