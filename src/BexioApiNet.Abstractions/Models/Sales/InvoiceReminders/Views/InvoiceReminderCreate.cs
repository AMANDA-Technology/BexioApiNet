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

namespace BexioApiNet.Abstractions.Models.Sales.InvoiceReminders.Views;

/// <summary>
/// Create view for an invoice reminder — body of
/// <c>POST /2.0/kb_invoice/{invoice_id}/kb_reminder</c>. Read-only fields (id, kb_invoice_id,
/// reminder_level, remaining_price, received_total) are intentionally omitted.
/// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2CreateInvoiceReminder"/>
/// </summary>
/// <param name="Title">Reminder title/subject.</param>
/// <param name="IsValidFrom">Reminder validity start in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="IsValidTo">Reminder due date in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="ReminderPeriodInDays">Number of days the reminder period spans.</param>
/// <param name="ShowPositions">When <see langword="true"/>, positions from the source invoice are reprinted on the reminder.</param>
/// <param name="IsSent">Flag indicating whether the reminder should be created as already sent.</param>
/// <param name="Header">Free-text header printed on top of the reminder.</param>
/// <param name="Footer">Free-text footer printed below the reminder body.</param>
public sealed record InvoiceReminderCreate(
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("is_valid_from")] string? IsValidFrom = null,
    [property: JsonPropertyName("is_valid_to")] string? IsValidTo = null,
    [property: JsonPropertyName("reminder_period_in_days")] int? ReminderPeriodInDays = null,
    [property: JsonPropertyName("show_positions")] bool? ShowPositions = null,
    [property: JsonPropertyName("is_sent")] bool? IsSent = null,
    [property: JsonPropertyName("header")] string? Header = null,
    [property: JsonPropertyName("footer")] string? Footer = null
);
