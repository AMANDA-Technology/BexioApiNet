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

using BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Enums;
using BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Views;

namespace BexioApiNet.E2eTests.Tests.Accounting.ManualEntries;

/// <summary>
/// Live E2E coverage for <see cref="IManualEntryService.Put" />. Creates a manual entry, then
/// replaces it via PUT and verifies the server returns the updated payload.
/// </summary>
public class Put : BexioE2eTestBase
{
    /// <summary>
    /// Creates a manual entry, issues a PUT with a different reference number, and asserts the
    /// response reflects the update.
    /// </summary>
    [Test]
    public async Task UpdateSingleEntry()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var created = await BexioApiClient!.AccountingManualEntries.Create(new(
            Type: ManualEntryType.manual_single_entry,
            Date: DateOnly.FromDateTime(DateTime.Now),
            ReferenceNr: "PUT-SRC",
            Entries: new[]
            {
                new ManualEntryCreate(DebitAccountId: 89, CreditAccountId: 90, TaxId: 15, TaxAccountId: 90, Amount: 50m, CurrencyId: 1, Description: "Source entry", CurrencyFactor: 1)
            }));

        Assert.That(created.Data, Is.Not.Null);

        var updated = await BexioApiClient.AccountingManualEntries.Put(
            created.Data!.Id,
            new ManualEntryUpdate(
                Type: ManualEntryType.manual_single_entry,
                Date: DateOnly.FromDateTime(DateTime.Now),
                ReferenceNr: "PUT-DST",
                Entries: new[]
                {
                    new ManualEntryEntryUpdate(
                        DebitAccountId: 89,
                        CreditAccountId: 90,
                        TaxId: 15,
                        TaxAccountId: 90,
                        Description: "Updated entry",
                        Amount: 75m,
                        CurrencyId: 1,
                        CurrencyFactor: 1,
                        Id: created.Data.Entries[0].Id)
                },
                Id: created.Data.Id));

        Assert.That(updated, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updated.IsSuccess, Is.True);
            Assert.That(updated.Data!.ReferenceNr, Is.EqualTo("PUT-DST"));
            Assert.That(updated.Data!.Entries[0].Amount, Is.EqualTo(75m));
        });
    }
}
