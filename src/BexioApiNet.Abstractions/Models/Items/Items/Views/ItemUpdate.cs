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

namespace BexioApiNet.Abstractions.Models.Items.Items.Views;

/// <summary>
///     Update view for an item — body of <c>POST /2.0/article/{article_id}</c>. The Bexio API
///     uses <c>POST</c> (not <c>PUT</c>) for full-replacement edits on this resource.
///     Read-only fields and <c>article_type_id</c> (read-only on update per the Bexio spec)
///     are excluded.
///     <see href="https://docs.bexio.com/#tag/Items/operation/v2EditItem" />
/// </summary>
/// <param name="UserId">References a user object. Currently has no impact regardless of which value is sent.</param>
/// <param name="InternCode">Internal article code.</param>
/// <param name="InternName">Internal article name.</param>
/// <param name="ContactId">References a contact object.</param>
/// <param name="DelivererCode">Deliverer code.</param>
/// <param name="DelivererName">Deliverer name.</param>
/// <param name="DelivererDescription">Deliverer description.</param>
/// <param name="InternDescription">Internal article description.</param>
/// <param name="PurchasePrice">Purchase price as a decimal string.</param>
/// <param name="SalePrice">Sale price as a decimal string.</param>
/// <param name="PurchaseTotal">Total purchase amount.</param>
/// <param name="SaleTotal">Total sale amount.</param>
/// <param name="CurrencyId">References a currency object.</param>
/// <param name="TaxIncomeId">References a tax object for income.</param>
/// <param name="TaxExpenseId">References a tax object for expenses.</param>
/// <param name="UnitId">References a unit object.</param>
/// <param name="IsStock">When <see langword="true" />, this item is tracked in stock. Requires <c>stock_edit</c> scope.</param>
/// <param name="StockId">References a stock location object.</param>
/// <param name="StockPlaceId">References a stock area object.</param>
/// <param name="StockNr">Stock number. Can only be updated if no bookings have been made for this product.</param>
/// <param name="StockMinNr">Minimum stock number threshold.</param>
/// <param name="Width">Width in millimetres.</param>
/// <param name="Height">Height in millimetres.</param>
/// <param name="Weight">Weight in grams.</param>
/// <param name="Volume">Volume in millilitres.</param>
/// <param name="HtmlText">HTML description text (deprecated).</param>
/// <param name="Remarks">Free-text remarks.</param>
/// <param name="DeliveryPrice">Delivery price.</param>
/// <param name="ArticleGroupId">References an article group.</param>
/// <param name="AccountId">References an account object for income.</param>
/// <param name="ExpenseAccountId">References an account object for expenses.</param>
public sealed record ItemUpdate(
    [property: JsonPropertyName("user_id")]
    int UserId,
    [property: JsonPropertyName("intern_code")]
    string InternCode,
    [property: JsonPropertyName("intern_name")]
    string InternName,
    [property: JsonPropertyName("contact_id")]
    int? ContactId = null,
    [property: JsonPropertyName("deliverer_code")]
    string? DelivererCode = null,
    [property: JsonPropertyName("deliverer_name")]
    string? DelivererName = null,
    [property: JsonPropertyName("deliverer_description")]
    string? DelivererDescription = null,
    [property: JsonPropertyName("intern_description")]
    string? InternDescription = null,
    [property: JsonPropertyName("purchase_price")]
    string? PurchasePrice = null,
    [property: JsonPropertyName("sale_price")]
    string? SalePrice = null,
    [property: JsonPropertyName("purchase_total")]
    double? PurchaseTotal = null,
    [property: JsonPropertyName("sale_total")]
    double? SaleTotal = null,
    [property: JsonPropertyName("currency_id")]
    int? CurrencyId = null,
    [property: JsonPropertyName("tax_income_id")]
    int? TaxIncomeId = null,
    [property: JsonPropertyName("tax_expense_id")]
    int? TaxExpenseId = null,
    [property: JsonPropertyName("unit_id")]
    int? UnitId = null,
    [property: JsonPropertyName("is_stock")]
    bool IsStock = false,
    [property: JsonPropertyName("stock_id")]
    int? StockId = null,
    [property: JsonPropertyName("stock_place_id")]
    int? StockPlaceId = null,
    [property: JsonPropertyName("stock_nr")]
    int StockNr = 0,
    [property: JsonPropertyName("stock_min_nr")]
    int StockMinNr = 0,
    [property: JsonPropertyName("width")] int? Width = null,
    [property: JsonPropertyName("height")] int? Height = null,
    [property: JsonPropertyName("weight")] int? Weight = null,
    [property: JsonPropertyName("volume")] int? Volume = null,
    [property: JsonPropertyName("html_text")]
    string? HtmlText = null,
    [property: JsonPropertyName("remarks")]
    string? Remarks = null,
    [property: JsonPropertyName("delivery_price")]
    double? DeliveryPrice = null,
    [property: JsonPropertyName("article_group_id")]
    int? ArticleGroupId = null,
    [property: JsonPropertyName("account_id")]
    int? AccountId = null,
    [property: JsonPropertyName("expense_account_id")]
    int? ExpenseAccountId = null
);