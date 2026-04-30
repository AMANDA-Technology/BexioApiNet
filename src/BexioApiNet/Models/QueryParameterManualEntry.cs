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
/// Optional query parameters for the <c>3.0/accounting/manual_entries</c> endpoints.
/// Per the Bexio v3 OpenAPI spec only <c>limit</c> and <c>offset</c> are supported.
/// </summary>
/// <param name="Limit">Limit the number of results (max is 2000).</param>
/// <param name="Offset">Skip over a number of elements by specifying an offset value.</param>
public sealed record QueryParameterManualEntry(
    int? Limit = null,
    int? Offset = null
)
{
    /// <summary>
    /// The wrapped <see cref="QueryParameter"/> built from the supplied options. Returns
    /// <see langword="null"/> when no optional value was provided so the connection handler can
    /// skip query string composition entirely.
    /// </summary>
    public QueryParameter? QueryParameter { get; } = Build(Limit, Offset);

    private static QueryParameter? Build(int? limit, int? offset)
    {
        var parameters = new Dictionary<string, object>();
        if (limit is { } l) parameters["limit"] = l;
        if (offset is { } o) parameters["offset"] = o;
        return parameters.Count is 0 ? null : new QueryParameter(parameters);
    }
}
