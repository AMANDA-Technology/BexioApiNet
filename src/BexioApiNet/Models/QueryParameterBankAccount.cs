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
/// Optional query parameters for <c>GET /3.0/banking/accounts</c>. All parameters are optional
/// per the Bexio v3.0 spec; only the values supplied by the caller are added to the request URI.
/// <see href="https://docs.bexio.com/#tag/Bank-Accounts/operation/ListBankAccounts">List Bank Accounts</see>
/// </summary>
/// <param name="Limit">Maximum number of results (1–2000). Bexio default is 500.</param>
/// <param name="Offset">Offset to skip from the start of the result set.</param>
public sealed record QueryParameterBankAccount(
    int? Limit = null,
    int? Offset = null
)
{
    /// <summary>
    /// Underlying <see cref="BexioApiNet.Models.QueryParameter"/> forwarded to
    /// <see cref="BexioApiNet.Interfaces.IBexioConnectionHandler.GetAsync{TResult}"/>.
    /// Only properties that have been set by the caller are added to the dictionary.
    /// </summary>
    public QueryParameter? QueryParameter { get; } = Build(Limit, Offset);

    private static QueryParameter? Build(int? limit, int? offset)
    {
        var parameters = new Dictionary<string, object>();

        if (limit is { } l)
            parameters["limit"] = l;

        if (offset is { } o)
            parameters["offset"] = o;

        return parameters.Count is 0 ? null : new QueryParameter(parameters);
    }
}
