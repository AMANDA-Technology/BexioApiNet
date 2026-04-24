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
///     Typed query parameter wrapper for the Bexio files endpoints
///     (<c>GET /3.0/files</c> and <c>POST /3.0/files/search</c>).
///     Each constructor argument is optional so callers can supply only the parameters they need;
///     <see langword="null" /> values are skipped and not serialized onto the URL.
/// </summary>
/// <param name="Limit">
///     Maximum number of results to return (accepted by <c>/search</c>, Bexio default <c>500</c>, maximum
///     <c>2000</c>).
/// </param>
/// <param name="Offset">Number of results to skip for pagination.</param>
/// <param name="OrderBy">
///     Sort clause — one of <c>id</c>, <c>created_at</c>, <c>source_id</c>, <c>uuid</c>, <c>name</c>,
///     <c>size_in_bytes</c>, optionally with <c>_asc</c>/<c>_desc</c> suffixes.
/// </param>
/// <param name="ArchivedState">Archived filter — one of <c>all</c>, <c>archived</c>, <c>not_archived</c>.</param>
public sealed record QueryParameterFile(
    int? Limit = null,
    int? Offset = null,
    string? OrderBy = null,
    string? ArchivedState = null
)
{
    /// <summary>
    ///     The underlying <see cref="Models.QueryParameter" /> passed to <see cref="Interfaces.IBexioConnectionHandler" />.
    ///     <see langword="null" /> when every input is <see langword="null" /> so the handler appends no query string.
    /// </summary>
    public QueryParameter? QueryParameter { get; } = BuildQueryParameter(Limit, Offset, OrderBy, ArchivedState);

    private static QueryParameter? BuildQueryParameter(int? limit, int? offset, string? orderBy, string? archivedState)
    {
        var parameters = new Dictionary<string, object>();

        if (limit is { } l)
            parameters["limit"] = l;

        if (offset is { } o)
            parameters["offset"] = o;

        if (!string.IsNullOrWhiteSpace(orderBy))
            parameters["order_by"] = orderBy;

        if (!string.IsNullOrWhiteSpace(archivedState))
            parameters["archived_state"] = archivedState;

        return parameters.Count is 0 ? null : new QueryParameter(parameters);
    }
}
