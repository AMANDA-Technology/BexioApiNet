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

namespace BexioApiNet.Abstractions.Models.Items.Items;

/// <summary>
///     Item (article) as returned by the Bexio items endpoint.
///     <see href="https://docs.bexio.com/#tag/Items/operation/v2ListItems" />
/// </summary>
/// <param name="Id">Unique item identifier (read-only).</param>
/// <param name="UserId">References a user object. Currently has no impact regardless of which value is sent.</param>
/// <param name="ArticleTypeId"><c>1</c> for physical products, <c>2</c> for services.</param>
/// <param name="ContactId">References a contact object.</param>
/// <param name="DelivererCode">Deliverer code.</param>
/// <param name="DelivererName">Deliverer name.</param>
/// <param name="DelivererDescription">Deliverer description.</param>
/// <param name="InternCode">Internal article code.</param>
/// <param name="InternName">Internal article name.</param>
/// <param name="InternDescription">Internal article description.</param>
/// <param name="PurchasePrice">Purchase price as a decimal string.</param>
/// <param name="SalePrice">Sale price as a decimal string.</param>
/// <param name="PurchaseTotal">Total purchase amount.</param>
/// <param name="SaleTotal">Total sale amount.</param>
/// <param name="CurrencyId">References a currency object.</param>
/// <param name="TaxIncomeId">References a tax object for income.</param>
/// <param name="TaxId">References a tax object (read-only, derived).</param>
/// <param name="TaxExpenseId">References a tax object for expenses.</param>
/// <param name="UnitId">References a unit object.</param>
/// <param name="IsStock">When <see langword="true" />, this item is tracked in stock. Requires <c>stock_edit</c> scope.</param>
/// <param name="StockId">References a stock location object.</param>
/// <param name="StockPlaceId">References a stock area object.</param>
/// <param name="StockNr">Current stock number. Can only be set if no bookings have been made for this product.</param>
/// <param name="StockMinNr">Minimum stock number threshold.</param>
/// <param name="StockReservedNr">Reserved stock number (read-only).</param>
/// <param name="StockAvailableNr">Available stock number (read-only).</param>
/// <param name="StockPickedNr">Picked stock number (read-only).</param>
/// <param name="StockDisposedNr">Disposed stock number (read-only).</param>
/// <param name="StockOrderedNr">Ordered stock number (read-only).</param>
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
public sealed record Item(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("user_id")]
    int UserId,
    [property: JsonPropertyName("article_type_id")]
    int ArticleTypeId,
    [property: JsonPropertyName("contact_id")]
    int? ContactId,
    [property: JsonPropertyName("deliverer_code")]
    string? DelivererCode,
    [property: JsonPropertyName("deliverer_name")]
    string? DelivererName,
    [property: JsonPropertyName("deliverer_description")]
    string? DelivererDescription,
    [property: JsonPropertyName("intern_code")]
    string InternCode,
    [property: JsonPropertyName("intern_name")]
    string InternName,
    [property: JsonPropertyName("intern_description")]
    string? InternDescription,
    [property: JsonPropertyName("purchase_price")]
    string? PurchasePrice,
    [property: JsonPropertyName("sale_price")]
    string? SalePrice,
    [property: JsonPropertyName("purchase_total")]
    double? PurchaseTotal,
    [property: JsonPropertyName("sale_total")]
    double? SaleTotal,
    [property: JsonPropertyName("currency_id")]
    int? CurrencyId,
    [property: JsonPropertyName("tax_income_id")]
    int? TaxIncomeId,
    [property: JsonPropertyName("tax_id")] int? TaxId,
    [property: JsonPropertyName("tax_expense_id")]
    int? TaxExpenseId,
    [property: JsonPropertyName("unit_id")]
    int? UnitId,
    [property: JsonPropertyName("is_stock")]
    bool IsStock,
    [property: JsonPropertyName("stock_id")]
    int? StockId,
    [property: JsonPropertyName("stock_place_id")]
    int? StockPlaceId,
    [property: JsonPropertyName("stock_nr")]
    int StockNr,
    [property: JsonPropertyName("stock_min_nr")]
    int StockMinNr,
    [property: JsonPropertyName("stock_reserved_nr")]
    int StockReservedNr,
    [property: JsonPropertyName("stock_available_nr")]
    int StockAvailableNr,
    [property: JsonPropertyName("stock_picked_nr")]
    int StockPickedNr,
    [property: JsonPropertyName("stock_disposed_nr")]
    int StockDisposedNr,
    [property: JsonPropertyName("stock_ordered_nr")]
    int StockOrderedNr,
    [property: JsonPropertyName("width")] int? Width,
    [property: JsonPropertyName("height")] int? Height,
    [property: JsonPropertyName("weight")] int? Weight,
    [property: JsonPropertyName("volume")] int? Volume,
    [property: JsonPropertyName("html_text")]
    string? HtmlText,
    [property: JsonPropertyName("remarks")]
    string? Remarks,
    [property: JsonPropertyName("delivery_price")]
    double? DeliveryPrice,
    [property: JsonPropertyName("article_group_id")]
    int? ArticleGroupId,
    [property: JsonPropertyName("account_id")]
    int? AccountId,
    [property: JsonPropertyName("expense_account_id")]
    int? ExpenseAccountId
);