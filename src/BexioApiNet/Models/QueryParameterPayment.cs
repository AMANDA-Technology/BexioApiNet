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
/// Optional query parameters for the Bexio banking payments list endpoint
/// (<c>GET /4.0/banking/payments</c>). The v4 list endpoint uses <c>page</c> and
/// <c>per-page</c> for pagination and a <c>filter-by</c> expression for filtering,
/// which differs from the <c>limit</c>/<c>offset</c> convention of older endpoints.
/// <see href="https://docs.bexio.com/#tag/Payments/operation/NewFetchAllPayments"/>
/// </summary>
/// <param name="Page">1-based page offset to skip.</param>
/// <param name="PerPage">Page size (maximum 2000 per Bexio API contract).</param>
/// <param name="FilterBy">Filter expression. See the Bexio docs for syntax details.</param>
public sealed record QueryParameterPayment(
    int? Page = null,
    int? PerPage = null,
    string? FilterBy = null
)
{
    /// <summary>
    /// The underlying <see cref="BexioApiNet.Models.QueryParameter"/> forwarded to
    /// <see cref="BexioApiNet.Interfaces.IBexioConnectionHandler.GetAsync{TResult}"/>.
    /// Only the properties that have been set by the caller are added to the dictionary.
    /// </summary>
    public QueryParameter? QueryParameter { get; } = Build(Page, PerPage, FilterBy);

    private static QueryParameter? Build(int? page, int? perPage, string? filterBy)
    {
        var parameters = new Dictionary<string, object>();

        if (page is { } p)
            parameters["page"] = p;

        if (perPage is { } pp)
            parameters["per-page"] = pp;

        if (!string.IsNullOrWhiteSpace(filterBy))
            parameters["filter-by"] = filterBy;

        return parameters.Count is 0 ? null : new QueryParameter(parameters);
    }
}
