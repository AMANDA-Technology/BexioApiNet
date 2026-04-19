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
/// Dictionary for optional query parameters for the accounting journal endpoint.
/// </summary>
/// <param name="From">Filter for entries after this date.</param>
/// <param name="To">Filter for entries until this date.</param>
/// <param name="AccountUuid">Filter for entries with account with this uuid.</param>
/// <param name="Limit">Limit the number of results (max is 2000).</param>
/// <param name="Offset">Skip over a number of elements by specifying an offset value.</param>
public sealed record QueryParameterJournal(
    DateOnly? From = null,
    DateOnly? To = null,
    string? AccountUuid = null,
    int? Limit = null,
    int? Offset = null
)
{
    /// <summary>
    /// Wrapped dictionary of query parameters to serialize onto the URL. Only parameters with a value are emitted.
    /// </summary>
    public QueryParameter? QueryParameter { get; } = Build(From, To, AccountUuid, Limit, Offset);

    private static QueryParameter? Build(DateOnly? from, DateOnly? to, string? accountUuid, int? limit, int? offset)
    {
        var parameters = new Dictionary<string, object>();
        if (from is { } f) parameters["from"] = f.ToString("yyyy-MM-dd");
        if (to is { } t) parameters["to"] = t.ToString("yyyy-MM-dd");
        if (!string.IsNullOrWhiteSpace(accountUuid)) parameters["account_uuid"] = accountUuid;
        if (limit is { } l) parameters["limit"] = l;
        if (offset is { } o) parameters["offset"] = o;
        return parameters.Count is 0 ? null : new QueryParameter(parameters);
    }
}
