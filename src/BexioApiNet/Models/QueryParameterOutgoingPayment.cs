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

namespace BexioApiNet.Models;

/// <summary>
/// Query parameters for <c>GET /4.0/purchase/outgoing-payments</c>. <c>bill_id</c> is mandatory per the Bexio v4.0 spec;
/// all other parameters are optional and only added to the final URI when supplied.
/// <see href="https://docs.bexio.com/#tag/Outgoing-Payment/operation/ApiOutgoingPaymentList_GET">List Outgoing Payments</see>
/// </summary>
public sealed record QueryParameterOutgoingPayment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParameterOutgoingPayment"/> record.
    /// </summary>
    /// <param name="billId">Bill identifier to filter by. Required by the Bexio API.</param>
    /// <param name="limit">Page size (1 or more). Bexio default is 100.</param>
    /// <param name="page">1-based page number. Bexio default is 1.</param>
    /// <param name="order">Sort direction. Must be <c>asc</c> or <c>desc</c>. Bexio default is <c>asc</c>.</param>
    /// <param name="sort">Field name to sort by (e.g. <c>payment_type</c>).</param>
    public QueryParameterOutgoingPayment(
        Guid billId,
        int? limit = null,
        int? page = null,
        string? order = null,
        string? sort = null)
    {
        var parameters = new Dictionary<string, object>
        {
            ["bill_id"] = billId
        };

        if (limit is { } l) parameters["limit"] = l;
        if (page is { } p) parameters["page"] = p;
        if (!string.IsNullOrWhiteSpace(order)) parameters["order"] = order;
        if (!string.IsNullOrWhiteSpace(sort)) parameters["sort"] = sort;

        QueryParameter = new(parameters);
    }

    /// <summary>
    /// Serializable query parameter dictionary forwarded to <c>ConnectionHandler</c>.
    /// </summary>
    public QueryParameter QueryParameter { get; }
}
