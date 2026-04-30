/*
MIT License

Copyright (c) 2022 Philip Naef <philip.naef@amanda-technology.ch>
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
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.IntegrationTests.Banking;

/// <summary>
/// Integration tests covering <see cref="PaymentTypeService" />. The request path is composed from
/// <see cref="PaymentTypeConfiguration" /> (<c>2.0/payment_type</c>) and must reach WireMock
/// intact when the service is driven through the real connection handler. Tests use
/// fully-populated JSON payloads matching the OpenAPI v2.0 schema and assert end-to-end
/// deserialisation of every field.
/// </summary>
public sealed class PaymentTypeServiceIntegrationTests : IntegrationTestBase
{
    private const string PaymentTypesPath = "/2.0/payment_type";

    /// <summary>
    /// Schema-accurate JSON list payload matching <c>GET /2.0/payment_type</c>.
    /// </summary>
    private const string PaymentTypeListJson = """
                                               [
                                                   { "id": 1, "name": "Cash" },
                                                   { "id": 2, "name": "Bank" },
                                                   { "id": 3, "name": "Credit Card" }
                                               ]
                                               """;

    /// <summary>
    /// <c>PaymentTypeService.Get()</c> must issue a <c>GET</c> against
    /// <c>/2.0/payment_type</c>, deserialise the array of payment types, and surface
    /// every <c>id</c>/<c>name</c> field with the correct C# type.
    /// </summary>
    [Test]
    public async Task PaymentTypeService_Get_DeserialisesAllSchemaFields()
    {
        Server
            .Given(Request.Create().WithPath(PaymentTypesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PaymentTypeListJson));

        var service = new PaymentTypeService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(3));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PaymentTypesPath));
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data[0].Name, Is.EqualTo("Cash"));
            Assert.That(result.Data[1].Id, Is.EqualTo(2));
            Assert.That(result.Data[1].Name, Is.EqualTo("Bank"));
            Assert.That(result.Data[2].Id, Is.EqualTo(3));
            Assert.That(result.Data[2].Name, Is.EqualTo("Credit Card"));
        });
    }

    /// <summary>
    /// <c>PaymentTypeService.Get</c> with a <see cref="QueryParameterPaymentType"/> must
    /// forward <c>limit</c>, <c>offset</c>, and <c>order_by</c> to WireMock as URL query
    /// parameters with the values supplied by the caller.
    /// </summary>
    [Test]
    public async Task PaymentTypeService_Get_WithQueryParameter_PassesAllParametersOnUrl()
    {
        Server
            .Given(Request.Create().WithPath(PaymentTypesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new PaymentTypeService(ConnectionHandler);

        await service.Get(
            new QueryParameterPaymentType(Limit: 100, Offset: 50, OrderBy: "name_asc"),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PaymentTypesPath));
            Assert.That(request.Url, Does.Contain("limit=100"));
            Assert.That(request.Url, Does.Contain("offset=50"));
            Assert.That(request.Url, Does.Contain("order_by=name_asc"));
        });
    }

    /// <summary>
    /// <c>PaymentTypeService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/payment_type/search</c> with a <c>v2Search</c>-compatible JSON array body
    /// and deserialise the returned payment types.
    /// </summary>
    [Test]
    public async Task PaymentTypeService_Search_PostsCriteriaAndDeserialisesPaymentTypes()
    {
        var expectedPath = $"{PaymentTypesPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PaymentTypeListJson));

        var service = new PaymentTypeService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Cash", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(3));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"Cash\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data[0].Name, Is.EqualTo("Cash"));
        });
    }

    /// <summary>
    /// <c>PaymentTypeService.Search</c> with a <see cref="QueryParameterPaymentType"/>
    /// forwards <c>limit</c>, <c>offset</c>, and <c>order_by</c> to WireMock as URL
    /// query parameters alongside the JSON body.
    /// </summary>
    [Test]
    public async Task PaymentTypeService_Search_WithQueryParameter_PassesParametersOnUrl()
    {
        var expectedPath = $"{PaymentTypesPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new PaymentTypeService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Bank", Criteria = "=" }
        };

        await service.Search(
            criteria,
            new QueryParameterPaymentType(Limit: 25, Offset: 0, OrderBy: "id_desc"),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Url, Does.Contain("limit=25"));
            Assert.That(request.Url, Does.Contain("offset=0"));
            Assert.That(request.Url, Does.Contain("order_by=id_desc"));
        });
    }
}
