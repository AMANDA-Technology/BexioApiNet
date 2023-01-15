﻿/*
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

namespace BexioApiNet.Abstractions.Models.Accounting.Accounts;

/// <summary>
/// Account object. <see href="https://docs.bexio.com/#tag/Accounts/operation/v2ListAccounts"/>
/// </summary>
/// <param name="Id"></param>
/// <param name="Uuid"></param>
/// <param name="AccountNo"></param>
/// <param name="Name"></param>
/// <param name="AccountType"></param>
/// <param name="TaxId"></param>
/// <param name="FibuAccountGroupId"></param>
/// <param name="IsActive"></param>
/// <param name="IsLocked"></param>
public sealed record Account(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("uuid")] string Uuid,
    [property: JsonPropertyName("account_no")] string AccountNo,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("account_type")] int AccountType,
    [property: JsonPropertyName("tax_id")] int? TaxId,
    [property: JsonPropertyName("fibu_account_group_id")] int FibuAccountGroupId,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("is_locked")] bool IsLocked
);
