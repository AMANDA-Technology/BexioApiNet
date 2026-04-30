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

namespace BexioApiNet.E2eTests.Tests.Accounting.Accounts;

/// <summary>
/// Live E2E tests for <c>BexioApiClient.Accounts</c> (the <c>v2ListAccounts</c> endpoint).
/// Tests assert the response payload structurally matches the <c>v2Account</c> schema in
/// <c>doc/openapi/bexio-v3.json</c>. Skipped automatically when no live credentials are set.
/// </summary>
public class TestGetAll : BexioE2eTestBase
{
    /// <summary>
    /// Fetches all accounts from the live tenant and asserts the response is successful and
    /// each returned record carries the schema-required identifier and metadata fields.
    /// </summary>
    [Test]
    public async Task GetAll()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.Accounts.Get(autoPage: true);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null.And.Not.Empty);
        });

        var first = res.Data!.First();
        Assert.Multiple(() =>
        {
            Assert.That(first.Id, Is.GreaterThan(0), "id is required by the v2Account schema");
            Assert.That(first.Uuid, Is.Not.Null.And.Not.Empty, "uuid is required by the v2Account schema");
            Assert.That(first.AccountNo, Is.Not.Null.And.Not.Empty, "account_no is required by the v2Account schema");
            Assert.That(first.Name, Is.Not.Null.And.Not.Empty, "name is required by the v2Account schema");
            Assert.That(first.AccountType, Is.InRange(1, 5), "account_type must be one of [1..5] per the v2Account schema");
            Assert.That(first.FibuAccountGroupId, Is.GreaterThan(0), "fibu_account_group_id is required by the v2Account schema");
        });
    }
}
