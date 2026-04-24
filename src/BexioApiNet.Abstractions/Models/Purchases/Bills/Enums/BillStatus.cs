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

// ReSharper disable InconsistentNaming
namespace BexioApiNet.Abstractions.Models.Purchases.Bills.Enums;

/// <summary>
/// Bill status as returned by the Bexio v4.0 <c>/purchase/bills</c> endpoints.
/// <see href="https://docs.bexio.com/#tag/Bills">Bills</see>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillStatus
{
    /// <summary>
    /// Bill is a draft and has not yet been booked.
    /// </summary>
    DRAFT,

    /// <summary>
    /// Bill has been booked.
    /// </summary>
    BOOKED,

    /// <summary>
    /// Payment has been partially created for this bill.
    /// </summary>
    PARTIALLY_CREATED,

    /// <summary>
    /// Payment has been fully created for this bill.
    /// </summary>
    CREATED,

    /// <summary>
    /// Payment has been partially sent.
    /// </summary>
    PARTIALLY_SENT,

    /// <summary>
    /// Payment has been sent.
    /// </summary>
    SENT,

    /// <summary>
    /// Payment has been partially downloaded.
    /// </summary>
    PARTIALLY_DOWNLOADED,

    /// <summary>
    /// Payment has been downloaded.
    /// </summary>
    DOWNLOADED,

    /// <summary>
    /// Bill has been partially paid.
    /// </summary>
    PARTIALLY_PAID,

    /// <summary>
    /// Bill has been fully paid.
    /// </summary>
    PAID,

    /// <summary>
    /// Payment has partially failed.
    /// </summary>
    PARTIALLY_FAILED,

    /// <summary>
    /// Payment has failed.
    /// </summary>
    FAILED
}
