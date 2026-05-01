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
/// Query parameters for <c>GET /4.0/expenses</c>. All parameters are optional;
/// only supplied values are appended to the final URI. Mirrors the Bexio v3 OpenAPI spec.
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
public sealed record QueryParameterExpense
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParameterExpense"/> record.
    /// </summary>
    /// <param name="limit">Page size. Bexio default is <c>100</c>.</param>
    /// <param name="page">1-based page number. Bexio default is <c>1</c>.</param>
    /// <param name="order">Sort direction. Must be <c>asc</c> or <c>desc</c>. Bexio default is <c>asc</c>.</param>
    /// <param name="sort">Field name to sort by (e.g. <c>document_no</c>).</param>
    /// <param name="vendor">Substring filter across <c>lastname_company</c> and <c>firstname_suffix</c>.</param>
    /// <param name="grossMin">Lower bound (inclusive) for <c>gross</c>.</param>
    /// <param name="grossMax">Upper bound (inclusive) for <c>gross</c>.</param>
    /// <param name="netMin">Lower bound (inclusive) for <c>net</c>.</param>
    /// <param name="netMax">Upper bound (inclusive) for <c>net</c>.</param>
    /// <param name="paidOnStart">Lower bound (inclusive) for <c>paid_on</c>.</param>
    /// <param name="paidOnEnd">Upper bound (inclusive) for <c>paid_on</c>.</param>
    /// <param name="createdAtStart">Lower bound (inclusive) for <c>created_at</c>.</param>
    /// <param name="createdAtEnd">Upper bound (inclusive) for <c>created_at</c>.</param>
    /// <param name="title">Substring filter on <c>title</c>.</param>
    /// <param name="currencyCode">Filter on <c>currency_code</c>.</param>
    /// <param name="documentNo">Substring filter on <c>document_no</c>.</param>
    /// <param name="supplierId">Exact match on <c>supplier_id</c>.</param>
    /// <param name="projectId">Exact match on <c>project_id</c>.</param>
    public QueryParameterExpense(
        int? limit = null,
        int? page = null,
        string? order = null,
        string? sort = null,
        string? vendor = null,
        double? grossMin = null,
        double? grossMax = null,
        double? netMin = null,
        double? netMax = null,
        DateOnly? paidOnStart = null,
        DateOnly? paidOnEnd = null,
        DateTimeOffset? createdAtStart = null,
        DateTimeOffset? createdAtEnd = null,
        string? title = null,
        string? currencyCode = null,
        string? documentNo = null,
        int? supplierId = null,
        Guid? projectId = null)
    {
        var parameters = new Dictionary<string, object>();

        if (limit is { } l) parameters["limit"] = l;
        if (page is { } p) parameters["page"] = p;
        if (!string.IsNullOrWhiteSpace(order)) parameters["order"] = order;
        if (!string.IsNullOrWhiteSpace(sort)) parameters["sort"] = sort;
        if (!string.IsNullOrWhiteSpace(vendor)) parameters["vendor"] = vendor;
        if (grossMin is { } gmin) parameters["gross_min"] = gmin;
        if (grossMax is { } gmax) parameters["gross_max"] = gmax;
        if (netMin is { } nmin) parameters["net_min"] = nmin;
        if (netMax is { } nmax) parameters["net_max"] = nmax;
        if (paidOnStart is { } pos) parameters["paid_on_start"] = pos.ToString("yyyy-MM-dd");
        if (paidOnEnd is { } poe) parameters["paid_on_end"] = poe.ToString("yyyy-MM-dd");
        if (createdAtStart is { } cas) parameters["created_at_start"] = cas.ToString("yyyy-MM-ddTHH:mm:sszzz");
        if (createdAtEnd is { } cae) parameters["created_at_end"] = cae.ToString("yyyy-MM-ddTHH:mm:sszzz");
        if (!string.IsNullOrWhiteSpace(title)) parameters["title"] = title;
        if (!string.IsNullOrWhiteSpace(currencyCode)) parameters["currency_code"] = currencyCode;
        if (!string.IsNullOrWhiteSpace(documentNo)) parameters["document_no"] = documentNo;
        if (supplierId is { } sid) parameters["supplier_id"] = sid;
        if (projectId is { } pid) parameters["project_id"] = pid;

        QueryParameter = new(parameters);
    }

    /// <summary>
    /// Serializable query parameter dictionary forwarded to <c>ConnectionHandler</c>.
    /// </summary>
    public QueryParameter QueryParameter { get; }
}
