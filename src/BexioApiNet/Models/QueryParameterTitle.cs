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
///     Optional query parameters for the titles endpoint
///     (<c>GET /2.0/title</c> and <c>POST /2.0/title/search</c>). All values are
///     optional and only supplied parameters are appended to the request URL.
/// </summary>
public sealed record QueryParameterTitle
{
    /// <summary>
    ///     Initializes a new <see cref="QueryParameterTitle" />.
    /// </summary>
    /// <param name="limit">Maximum number of titles to return (Bexio default 500, max 2000).</param>
    /// <param name="offset">Number of records to skip for pagination.</param>
    /// <param name="orderBy">Field name to sort by (e.g. <c>name</c>). Append <c>_desc</c> for descending order.</param>
    public QueryParameterTitle(int? limit = null, int? offset = null, string? orderBy = null)
    {
        var parameters = new Dictionary<string, object>();

        if (limit is { } l) parameters["limit"] = l;
        if (offset is { } o) parameters["offset"] = o;
        if (!string.IsNullOrWhiteSpace(orderBy)) parameters["order_by"] = orderBy;

        QueryParameter = new(parameters);
    }

    /// <summary>
    ///     The wrapped query parameter dictionary serialized onto the URL.
    /// </summary>
    public QueryParameter QueryParameter { get; }
}
