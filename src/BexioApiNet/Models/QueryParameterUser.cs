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
///     Pagination query parameters for <c>GET /3.0/users</c>.
/// </summary>
/// <param name="Limit">Maximum number of users to return. Bexio default is 500, maximum is 2000.</param>
/// <param name="Offset">Number of users to skip over before starting the result page.</param>
public sealed record QueryParameterUser(
    int Limit,
    int Offset
)
{
    /// <summary>
    ///     Wraps the pagination fields in the generic <see cref="Models.QueryParameter" />
    ///     dictionary consumed by <c>BexioConnectionHandler</c>.
    /// </summary>
    public QueryParameter? QueryParameter { get; } =
        new(new Dictionary<string, object>
        {
            { "limit", Limit },
            { "offset", Offset }
        });
}
