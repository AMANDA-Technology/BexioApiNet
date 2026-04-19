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

using BexioApiNet.Abstractions.Models.Sales.Positions;

namespace BexioApiNet.Abstractions.Models.Sales.Invoices;

/// <summary>
/// Invoice as returned by the Bexio <c>/2.0/kb_invoice</c> endpoint. Covers both the plain
/// <c>Invoice</c> schema returned by list and the <c>InvoiceWithDetails</c> variant returned by
/// show/create/update (which additionally populates <see cref="Positions"/>).
/// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2ListInvoices"/>
/// </summary>
/// <param name="Id">Unique invoice identifier (read-only).</param>
/// <param name="DocumentNr">Invoice number. Must be omitted when automatic numbering is enabled, required otherwise.</param>
/// <param name="Title">Invoice title/subject.</param>
/// <param name="ContactId">References a contact object.</param>
/// <param name="ContactSubId">Optional additional contact reference (e.g. addressee within a company).</param>
/// <param name="UserId">References the user that owns the invoice.</param>
/// <param name="ProjectId">Read-only reference to a project object.</param>
/// <param name="PrProjectId">Write-only reference to a project object, accepted on create/update.</param>
/// <param name="LogopaperId">References a logo-paper object (deprecated by Bexio but still accepted).</param>
/// <param name="LanguageId">References a language object.</param>
/// <param name="BankAccountId">References a bank account object.</param>
/// <param name="CurrencyId">References a currency object.</param>
/// <param name="PaymentTypeId">References a payment type object.</param>
/// <param name="Header">Free-text header printed on top of the invoice.</param>
/// <param name="Footer">Free-text footer printed below the positions.</param>
/// <param name="TotalGross">Gross total as a formatted decimal string (read-only).</param>
/// <param name="TotalNet">Net total as a formatted decimal string (read-only).</param>
/// <param name="TotalTaxes">Total of applicable taxes as a formatted decimal string (read-only).</param>
/// <param name="TotalReceivedPayments">Sum of applied payments as a formatted decimal string (read-only).</param>
/// <param name="TotalCreditVouchers">Sum of applied credit vouchers as a formatted decimal string (read-only).</param>
/// <param name="TotalRemainingPayments">Remaining open balance as a formatted decimal string (read-only).</param>
/// <param name="Total">Grand total as a formatted decimal string (read-only).</param>
/// <param name="TotalRoundingDifference">Rounding difference applied to the total (read-only).</param>
/// <param name="MwstType">VAT/MwSt. handling flag: <c>0</c> including taxes, <c>1</c> excluding taxes, <c>2</c> exempt.</param>
/// <param name="MwstIsNet">When <see langword="true"/> and <see cref="MwstType"/> is <c>0</c>, totals are interpreted as net.</param>
/// <param name="ShowPositionTaxes">When <see langword="true"/>, per-position taxes are shown on the printed document.</param>
/// <param name="IsValidFrom">Invoice validity start in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="IsValidTo">Invoice due date in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="ContactAddress">Composed contact address string (read-only).</param>
/// <param name="ContactAddressManual">Manually overridden contact address (write-only).</param>
/// <param name="KbItemStatusId">Read-only invoice status id (7 Draft, 8 Pending, 9 Paid, 15 Partial, 16 Cancelled, 17 Unpaid).</param>
/// <param name="Reference">Free-text reference displayed on the invoice.</param>
/// <param name="ApiReference">Caller-supplied reference accessible only via the API.</param>
/// <param name="ViewedByClientAt">Timestamp when the invoice was first opened by the recipient (read-only).</param>
/// <param name="UpdatedAt">Timestamp of the last update in Bexio's <c>yyyy-MM-dd HH:mm:ss</c> format (read-only).</param>
/// <param name="EsrId">Read-only reference to an ESR id.</param>
/// <param name="QrInvoiceId">Read-only reference to a QR invoice id.</param>
/// <param name="TemplateSlug">References a document template slug.</param>
/// <param name="Taxs">Read-only aggregated tax summary lines.</param>
/// <param name="NetworkLink">Read-only network link used by the Bexio document viewer.</param>
/// <param name="Positions">Optional polymorphic positions array populated on show/create/update responses. Each element is deserialized into the concrete <see cref="Position"/> subtype identified by its <c>type</c> discriminator.</param>
public sealed record Invoice(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("document_nr")] string? DocumentNr,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("contact_id")] int? ContactId,
    [property: JsonPropertyName("contact_sub_id")] int? ContactSubId,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("project_id")] int? ProjectId,
    [property: JsonPropertyName("pr_project_id")] int? PrProjectId,
    [property: JsonPropertyName("logopaper_id")] int? LogopaperId,
    [property: JsonPropertyName("language_id")] int? LanguageId,
    [property: JsonPropertyName("bank_account_id")] int? BankAccountId,
    [property: JsonPropertyName("currency_id")] int? CurrencyId,
    [property: JsonPropertyName("payment_type_id")] int? PaymentTypeId,
    [property: JsonPropertyName("header")] string? Header,
    [property: JsonPropertyName("footer")] string? Footer,
    [property: JsonPropertyName("total_gross")] string? TotalGross,
    [property: JsonPropertyName("total_net")] string? TotalNet,
    [property: JsonPropertyName("total_taxes")] string? TotalTaxes,
    [property: JsonPropertyName("total_received_payments")] string? TotalReceivedPayments,
    [property: JsonPropertyName("total_credit_vouchers")] string? TotalCreditVouchers,
    [property: JsonPropertyName("total_remaining_payments")] string? TotalRemainingPayments,
    [property: JsonPropertyName("total")] string? Total,
    [property: JsonPropertyName("total_rounding_difference")] decimal? TotalRoundingDifference,
    [property: JsonPropertyName("mwst_type")] int? MwstType,
    [property: JsonPropertyName("mwst_is_net")] bool? MwstIsNet,
    [property: JsonPropertyName("show_position_taxes")] bool? ShowPositionTaxes,
    [property: JsonPropertyName("is_valid_from")] string? IsValidFrom,
    [property: JsonPropertyName("is_valid_to")] string? IsValidTo,
    [property: JsonPropertyName("contact_address")] string? ContactAddress,
    [property: JsonPropertyName("contact_address_manual")] string? ContactAddressManual,
    [property: JsonPropertyName("kb_item_status_id")] int? KbItemStatusId,
    [property: JsonPropertyName("reference")] string? Reference,
    [property: JsonPropertyName("api_reference")] string? ApiReference,
    [property: JsonPropertyName("viewed_by_client_at")] string? ViewedByClientAt,
    [property: JsonPropertyName("updated_at")] string? UpdatedAt,
    [property: JsonPropertyName("esr_id")] int? EsrId,
    [property: JsonPropertyName("qr_invoice_id")] int? QrInvoiceId,
    [property: JsonPropertyName("template_slug")] string? TemplateSlug,
    [property: JsonPropertyName("taxs")] IReadOnlyList<InvoiceTax>? Taxs,
    [property: JsonPropertyName("network_link")] string? NetworkLink,
    [property: JsonPropertyName("positions")] IReadOnlyList<Position>? Positions
);
