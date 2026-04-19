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

namespace BexioApiNet.Abstractions.Models.Sales.Invoices.Views;

/// <summary>
/// Body for <c>POST /2.0/kb_invoice/{invoice_id}/send</c>. Sends the invoice via Bexio's network
/// mail service. The <c>[Network Link]</c> placeholder must be present in <see cref="Message"/>.
/// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2SendInvoice"/>
/// </summary>
/// <param name="RecipientEmail">Recipient email address (required). During trial the recipient is limited to the token owner.</param>
/// <param name="Subject">Email subject (required).</param>
/// <param name="Message">Email body (required). Must contain the <c>[Network Link]</c> placeholder.</param>
/// <param name="MarkAsOpen">When <see langword="true"/>, marks the invoice as open after sending.</param>
/// <param name="AttachPdf">When <see langword="true"/>, attaches the PDF directly to the email.</param>
public sealed record InvoiceSendRequest(
    [property: JsonPropertyName("recipient_email")] string RecipientEmail,
    [property: JsonPropertyName("subject")] string Subject,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("mark_as_open")] bool? MarkAsOpen = null,
    [property: JsonPropertyName("attach_pdf")] bool? AttachPdf = null
);
