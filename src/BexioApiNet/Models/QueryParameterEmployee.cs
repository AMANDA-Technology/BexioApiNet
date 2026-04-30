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
/// Query parameters for <c>GET /4.0/payroll/employees/{employeeId}</c>. The Bexio
/// v4.0 spec requires the GET-by-id endpoint to be scoped to a single date via the
/// <c>date</c> query parameter (ISO <c>yyyy-MM-dd</c> format).
/// <see href="https://docs.bexio.com/#tag/Employees">Employees</see>
/// </summary>
public sealed record QueryParameterEmployee
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParameterEmployee"/> record.
    /// </summary>
    /// <param name="date">Date to retrieve the employee snapshot on (required by the API).</param>
    public QueryParameterEmployee(DateOnly date)
    {
        QueryParameter = new(new Dictionary<string, object>
        {
            ["date"] = date.ToString("yyyy-MM-dd")
        });
    }

    /// <summary>
    /// Serializable query parameter dictionary forwarded to <c>ConnectionHandler</c>.
    /// </summary>
    public QueryParameter QueryParameter { get; }
}
