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
/// Query parameters for <c>GET /4.0/expenses/expenses</c>. All parameters are optional;
/// only supplied values are appended to the final URI.
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
public sealed record QueryParameterExpense
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParameterExpense"/> record.
    /// </summary>
    /// <param name="limit">Page size (1-500). Bexio default is 100.</param>
    /// <param name="page">1-based page number. Bexio default is 1.</param>
    /// <param name="order">Sort direction. Must be <c>asc</c> or <c>desc</c>. Bexio default is <c>asc</c>.</param>
    /// <param name="sort">Field name to sort by (e.g. <c>document_no</c>).</param>
    /// <param name="searchTerm">Free-text search term (3-255 characters).</param>
    /// <param name="status">Status filter — one of <c>DRAFTS</c>, <c>TODO</c>, <c>PAID</c>, <c>OVERDUE</c>.</param>
    /// <param name="expenseDateStart">Lower bound (inclusive) for <c>expense_date</c>.</param>
    /// <param name="expenseDateEnd">Upper bound (inclusive) for <c>expense_date</c>.</param>
    /// <param name="dueDateStart">Lower bound (inclusive) for <c>due_date</c>.</param>
    /// <param name="dueDateEnd">Upper bound (inclusive) for <c>due_date</c>.</param>
    /// <param name="vendorRef">Substring filter on <c>vendor_ref</c>.</param>
    /// <param name="title">Substring filter on <c>title</c>.</param>
    /// <param name="currencyCode">Substring filter on <c>currency_code</c>.</param>
    /// <param name="pendingAmountMin">Lower bound (inclusive) for <c>pending_amount</c>.</param>
    /// <param name="pendingAmountMax">Upper bound (inclusive) for <c>pending_amount</c>.</param>
    /// <param name="vendor">Substring filter across <c>lastname_company</c> and <c>firstname_suffix</c>.</param>
    /// <param name="grossMin">Lower bound (inclusive) for <c>gross</c>.</param>
    /// <param name="grossMax">Upper bound (inclusive) for <c>gross</c>.</param>
    /// <param name="netMin">Lower bound (inclusive) for <c>net</c>.</param>
    /// <param name="netMax">Upper bound (inclusive) for <c>net</c>.</param>
    /// <param name="documentNo">Substring filter on <c>document_no</c>.</param>
    /// <param name="supplierId">Exact match on <c>supplier_id</c>.</param>
    public QueryParameterExpense(
        int? limit = null,
        int? page = null,
        string? order = null,
        string? sort = null,
        string? searchTerm = null,
        string? status = null,
        DateOnly? expenseDateStart = null,
        DateOnly? expenseDateEnd = null,
        DateOnly? dueDateStart = null,
        DateOnly? dueDateEnd = null,
        string? vendorRef = null,
        string? title = null,
        string? currencyCode = null,
        double? pendingAmountMin = null,
        double? pendingAmountMax = null,
        string? vendor = null,
        double? grossMin = null,
        double? grossMax = null,
        double? netMin = null,
        double? netMax = null,
        string? documentNo = null,
        int? supplierId = null)
    {
        var parameters = new Dictionary<string, object>();

        if (limit is { } l) parameters["limit"] = l;
        if (page is { } p) parameters["page"] = p;
        if (!string.IsNullOrWhiteSpace(order)) parameters["order"] = order;
        if (!string.IsNullOrWhiteSpace(sort)) parameters["sort"] = sort;
        if (!string.IsNullOrWhiteSpace(searchTerm)) parameters["search_term"] = searchTerm;
        if (!string.IsNullOrWhiteSpace(status)) parameters["status"] = status;
        if (expenseDateStart is { } eds) parameters["expense_date_start"] = eds.ToString("yyyy-MM-dd");
        if (expenseDateEnd is { } ede) parameters["expense_date_end"] = ede.ToString("yyyy-MM-dd");
        if (dueDateStart is { } dds) parameters["due_date_start"] = dds.ToString("yyyy-MM-dd");
        if (dueDateEnd is { } dde) parameters["due_date_end"] = dde.ToString("yyyy-MM-dd");
        if (!string.IsNullOrWhiteSpace(vendorRef)) parameters["vendor_ref"] = vendorRef;
        if (!string.IsNullOrWhiteSpace(title)) parameters["title"] = title;
        if (!string.IsNullOrWhiteSpace(currencyCode)) parameters["currency_code"] = currencyCode;
        if (pendingAmountMin is { } pmin) parameters["pending_amount_min"] = pmin;
        if (pendingAmountMax is { } pmax) parameters["pending_amount_max"] = pmax;
        if (!string.IsNullOrWhiteSpace(vendor)) parameters["vendor"] = vendor;
        if (grossMin is { } gmin) parameters["gross_min"] = gmin;
        if (grossMax is { } gmax) parameters["gross_max"] = gmax;
        if (netMin is { } nmin) parameters["net_min"] = nmin;
        if (netMax is { } nmax) parameters["net_max"] = nmax;
        if (!string.IsNullOrWhiteSpace(documentNo)) parameters["document_no"] = documentNo;
        if (supplierId is { } sid) parameters["supplier_id"] = sid;

        QueryParameter = new(parameters);
    }

    /// <summary>
    /// Serializable query parameter dictionary forwarded to <c>ConnectionHandler</c>.
    /// </summary>
    public QueryParameter QueryParameter { get; }
}
