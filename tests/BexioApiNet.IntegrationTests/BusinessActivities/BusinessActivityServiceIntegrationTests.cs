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
using BexioApiNet.Abstractions.Models.BusinessActivities.BusinessActivity.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.BusinessActivities;

namespace BexioApiNet.IntegrationTests.BusinessActivities;

/// <summary>
/// Integration tests covering <see cref="BusinessActivityService"/>. The request path is composed from
/// <see cref="BusinessActivityConfiguration"/> (<c>2.0/client_service</c>) and must reach WireMock intact
/// when the service is driven through the real connection handler. Stub bodies replicate the exact
/// schemas defined by the v3 OpenAPI spec (<c>v2ClientService</c>) so deserialization is exercised
/// end-to-end.
/// </summary>
public sealed class BusinessActivityServiceIntegrationTests : IntegrationTestBase
{
    private const string BusinessActivitiesPath = "/2.0/client_service";
    private const string BusinessActivitiesSearchPath = "/2.0/client_service/search";

    private const string BusinessActivityResponse = """
                                                    {
                                                        "id": 1,
                                                        "name": "Project Management",
                                                        "default_is_billable": true,
                                                        "default_price_per_hour": 150.0,
                                                        "account_id": 142
                                                    }
                                                    """;

    private const string BusinessActivityListBody = """
                                                    [
                                                        {
                                                            "id": 1,
                                                            "name": "Project Management",
                                                            "default_is_billable": true,
                                                            "default_price_per_hour": 150.0,
                                                            "account_id": 142
                                                        },
                                                        {
                                                            "id": 2,
                                                            "name": "Consulting",
                                                            "default_is_billable": false,
                                                            "default_price_per_hour": null,
                                                            "account_id": null
                                                        }
                                                    ]
                                                    """;

    /// <summary>
    /// <c>BusinessActivityService.Get()</c> must issue a <c>GET</c> against
    /// <c>/2.0/client_service</c> and deserialize every field from the populated list response,
    /// including the optional/nullable <c>default_price_per_hour</c> and <c>account_id</c>.
    /// </summary>
    [Test]
    public async Task BusinessActivityService_Get_DeserializesPopulatedList()
    {
        Server
            .Given(Request.Create().WithPath(BusinessActivitiesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(BusinessActivityListBody));

        var service = new BusinessActivityService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BusinessActivitiesPath));

            Assert.That(result.Data, Has.Count.EqualTo(2));

            var first = result.Data![0];
            Assert.That(first.Id, Is.EqualTo(1));
            Assert.That(first.Name, Is.EqualTo("Project Management"));
            Assert.That(first.DefaultIsBillable, Is.True);
            Assert.That(first.DefaultPricePerHour, Is.EqualTo(150.0m));
            Assert.That(first.AccountId, Is.EqualTo(142));

            var second = result.Data[1];
            Assert.That(second.Id, Is.EqualTo(2));
            Assert.That(second.Name, Is.EqualTo("Consulting"));
            Assert.That(second.DefaultIsBillable, Is.False);
            Assert.That(second.DefaultPricePerHour, Is.Null);
            Assert.That(second.AccountId, Is.Null);
        });
    }

    /// <summary>
    /// <c>BusinessActivityService.Get()</c> must forward the supplied query parameters
    /// (<c>limit</c>, <c>offset</c>, <c>order_by</c>) to the URL.
    /// </summary>
    [Test]
    public async Task BusinessActivityService_Get_ForwardsQueryParameters()
    {
        Server
            .Given(Request.Create().WithPath(BusinessActivitiesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new BusinessActivityService(ConnectionHandler);
        var queryParameter = new QueryParameterBusinessActivity(Limit: 50, Offset: 25, OrderBy: "name_asc");

        await service.Get(queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Url, Does.Contain("limit=50"));
            Assert.That(request.Url, Does.Contain("offset=25"));
            Assert.That(request.Url, Does.Contain("order_by=name_asc"));
        });
    }

    /// <summary>
    /// <c>BusinessActivityService.Create()</c> must issue a <c>POST</c> request against
    /// <c>/2.0/client_service</c> with a body containing the snake_case field names and
    /// must deserialize the returned <see cref="BexioApiNet.Abstractions.Models.BusinessActivities.BusinessActivity.BusinessActivity"/>.
    /// </summary>
    [Test]
    public async Task BusinessActivityService_Create_SendsPostRequestAndDeserializesResponse()
    {
        Server
            .Given(Request.Create().WithPath(BusinessActivitiesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(BusinessActivityResponse));

        var service = new BusinessActivityService(ConnectionHandler);
        var payload = new BusinessActivityCreate(
            Name: "Project Management",
            DefaultIsBillable: true,
            DefaultPricePerHour: 150m,
            AccountId: 142);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(result.Data.Name, Is.EqualTo("Project Management"));
            Assert.That(result.Data.DefaultIsBillable, Is.True);
            Assert.That(result.Data.DefaultPricePerHour, Is.EqualTo(150.0m));
            Assert.That(result.Data.AccountId, Is.EqualTo(142));

            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BusinessActivitiesPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Project Management\""));
            Assert.That(request.Body, Does.Contain("\"default_is_billable\":true"));
            Assert.That(request.Body, Does.Contain("\"default_price_per_hour\":150"));
            Assert.That(request.Body, Does.Contain("\"account_id\":142"));
        });
    }

    /// <summary>
    /// <c>BusinessActivityService.Search()</c> must issue a <c>POST</c> request against
    /// <c>/2.0/client_service/search</c> with the search criteria serialized as the request body.
    /// </summary>
    [Test]
    public async Task BusinessActivityService_Search_SendsPostRequestToSearchPath()
    {
        Server
            .Given(Request.Create().WithPath(BusinessActivitiesSearchPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(BusinessActivityListBody));

        var service = new BusinessActivityService(ConnectionHandler);
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Project", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BusinessActivitiesSearchPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"Project\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
        });
    }
}
