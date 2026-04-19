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

namespace BexioApiNet.Abstractions.Models.Sales.InvoiceReminders;

/// <summary>
/// Invoice reminder as returned by the Bexio <c>/2.0/kb_invoice/{invoice_id}/kb_reminder</c>
/// endpoints. A reminder represents a dunning notice attached to an invoice.
/// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2ListInvoiceReminders"/>
/// </summary>
/// <param name="Id">Unique reminder identifier (read-only).</param>
/// <param name="KbInvoiceId">References an invoice object (read-only).</param>
/// <param name="Title">Reminder title/subject.</param>
/// <param name="IsValidFrom">Reminder validity start in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="IsValidTo">Reminder due date in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="ReminderPeriodInDays">Number of days the reminder period spans.</param>
/// <param name="ReminderLevel">Dunning level computed by Bexio (read-only).</param>
/// <param name="ShowPositions">When <see langword="true"/>, positions from the source invoice are reprinted on the reminder.</param>
/// <param name="RemainingPrice">Remaining open balance as a formatted decimal string (read-only).</param>
/// <param name="ReceivedTotal">Sum of received payments as a formatted decimal string (read-only).</param>
/// <param name="IsSent">Flag indicating whether the reminder has been sent.</param>
/// <param name="Header">Free-text header printed on top of the reminder.</param>
/// <param name="Footer">Free-text footer printed below the reminder body.</param>
public sealed record InvoiceReminder(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("kb_invoice_id")] int KbInvoiceId,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("is_valid_from")] string? IsValidFrom,
    [property: JsonPropertyName("is_valid_to")] string? IsValidTo,
    [property: JsonPropertyName("reminder_period_in_days")] int? ReminderPeriodInDays,
    [property: JsonPropertyName("reminder_level")] int? ReminderLevel,
    [property: JsonPropertyName("show_positions")] bool? ShowPositions,
    [property: JsonPropertyName("remaining_price")] string? RemainingPrice,
    [property: JsonPropertyName("received_total")] string? ReceivedTotal,
    [property: JsonPropertyName("is_sent")] bool? IsSent,
    [property: JsonPropertyName("header")] string? Header,
    [property: JsonPropertyName("footer")] string? Footer
);
