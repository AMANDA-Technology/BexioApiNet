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

namespace BexioApiNet.Abstractions.Models.Payroll.Employees;

/// <summary>
/// Address block of a payroll employee. Same shape on requests (POST/PATCH) and
/// responses for the Bexio v4.0 <c>/payroll/employees</c> endpoints.
/// <see href="https://docs.bexio.com/#tag/Employees">Employees</see>
/// </summary>
/// <param name="ComplementaryLine">Optional supplementary address line.</param>
/// <param name="Street">Deprecated combined street name + house number; prefer <see cref="StreetName"/> and <see cref="HouseNumber"/>.</param>
/// <param name="StreetName">Street name. Requires <see cref="HouseNumber"/> when not <see langword="null"/>.</param>
/// <param name="HouseNumber">House number. Requires <see cref="StreetName"/> when not <see langword="null"/>.</param>
/// <param name="Postbox">PO box number.</param>
/// <param name="Locality">Locality / district.</param>
/// <param name="ZipCode">Postal code.</param>
/// <param name="City">City name.</param>
/// <param name="Country">ISO Alpha-2 country code (e.g. <c>CH</c>).</param>
/// <param name="Canton">Swiss canton / state.</param>
/// <param name="MunicipalityId">Municipality identifier (Bexio internal).</param>
public sealed record EmployeeAddress(
    [property: JsonPropertyName("complementary_line")] string? ComplementaryLine = null,
    [property: JsonPropertyName("street")] string? Street = null,
    [property: JsonPropertyName("street_name")] string? StreetName = null,
    [property: JsonPropertyName("house_number")] string? HouseNumber = null,
    [property: JsonPropertyName("postbox")] string? Postbox = null,
    [property: JsonPropertyName("locality")] string? Locality = null,
    [property: JsonPropertyName("zip_code")] string? ZipCode = null,
    [property: JsonPropertyName("city")] string? City = null,
    [property: JsonPropertyName("country")] string? Country = null,
    [property: JsonPropertyName("canton")] string? Canton = null,
    [property: JsonPropertyName("municipality_id")] string? MunicipalityId = null
);
