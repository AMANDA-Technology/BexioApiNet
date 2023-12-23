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

namespace BexioApiNet.Tests.Tests.Accounting.ManualEntries;

public class CreateAndAddFile : TestBase
{
    /// <summary>
    ///
    /// </summary>
    [Test]
    public async Task CreateSingleEntry()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.AccountingManualEntries.Create(new(
            Type: ManualEntryType.manual_single_entry,
            Date: DateOnly.FromDateTime(DateTime.Now),
            ReferenceNr: "123",
            Entries: new []
            {
                new ManualEntryCreate(DebitAccountId: 89, CreditAccountId: 90, TaxId: 15, TaxAccountId: 90, Amount: 100m, CurrencyId: 1, Description: "Test entry", CurrencyFactor: 1)
            }));

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.Data!.ReferenceNr, Is.EqualTo("123"));
            Assert.That(res.Data!.Entries[0].Amount, Is.EqualTo(100m));
        });

        var res2 = await BexioApiClient!.AccountingManualEntries.AddAttachment(
            res.Data!.Id,
            res.Data.Entries[0].Id!.Value,
            new List<FileInfo>
            {
                new("Assets/letter.pdf"),
                new("Assets/letter2.pdf")
            });

        Assert.That(res2, Is.Not.Null);
        Assert.That(res2.Data![0].MimeType, Is.EqualTo("application/pdf"));
    }
}
