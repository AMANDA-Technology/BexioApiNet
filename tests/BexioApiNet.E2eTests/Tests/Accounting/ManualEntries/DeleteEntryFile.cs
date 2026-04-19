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
/// Live E2E coverage for <see cref="IManualEntryService.DeleteEntryFile" />. Creates and
/// attaches a file to a specific line, then deletes the line-to-file association.
/// </summary>
public class DeleteEntryFile : BexioE2eTestBase
{
    /// <summary>
    /// Deletes the connection between a file and a manual entry line.
    /// </summary>
    [Test]
    public async Task DeleteManualEntryFile()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var created = await BexioApiClient!.AccountingManualEntries.Create(new(
            Type: ManualEntryType.manual_single_entry,
            Date: DateOnly.FromDateTime(DateTime.Now),
            ReferenceNr: "LINE-FILE-DEL",
            Entries: new[]
            {
                new ManualEntryCreate(DebitAccountId: 89, CreditAccountId: 90, TaxId: 15, TaxAccountId: 90, Amount: 25m, CurrencyId: 1, Description: "Delete line file source", CurrencyFactor: 1)
            }));

        Assert.That(created.Data, Is.Not.Null);
        var entryId = created.Data!.Entries[0].Id!.Value;

        var uploaded = await BexioApiClient.AccountingManualEntries.AddAttachment(
            created.Data.Id,
            entryId,
            new List<FileInfo> { new("Assets/letter.pdf") });

        Assert.That(uploaded.Data, Is.Not.Null.And.Not.Empty);

        var res = await BexioApiClient.AccountingManualEntries.DeleteEntryFile(
            created.Data.Id,
            entryId,
            uploaded.Data![0].Id);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.IsSuccess, Is.True);
    }
}
