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

namespace BexioApiNet.Abstractions.Models.MasterData.Users;

/// <summary>
///     Bexio user account returned by the v3.0 <c>/users</c> endpoints
///     (GET list, GET <c>/users/me</c>, GET by id).
///     <see href="https://docs.bexio.com/#tag/User-Management">User Management</see>
/// </summary>
/// <param name="Id">Unique user identifier.</param>
/// <param name="SalutationType">Salutation type. One of <c>male</c> or <c>female</c>.</param>
/// <param name="Firstname">First name of the user. Read-only. Maximum 80 characters.</param>
/// <param name="Lastname">Last name of the user. Read-only. Maximum 80 characters.</param>
/// <param name="Email">Email address of the user. Email addresses must be unique across users.</param>
/// <param name="IsSuperadmin">
///     Whether the user is a superadmin. Only included when the caller has the necessary admin rights.
/// </param>
/// <param name="IsAccountant">
///     Whether the user is an accountant. Only included when the caller has the necessary admin rights.
/// </param>
public sealed record User(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("salutation_type")]
    string? SalutationType,
    [property: JsonPropertyName("firstname")]
    string? Firstname,
    [property: JsonPropertyName("lastname")]
    string? Lastname,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("is_superadmin")]
    bool? IsSuperadmin,
    [property: JsonPropertyName("is_accountant")]
    bool? IsAccountant
);
