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
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
/// Integration tests covering the full surface of <see cref="ManualEntryService"/> against
/// WireMock stubs. Verifies the path composed from <see cref="ManualEntryConfiguration"/>
/// (<c>3.0/accounting/manual_entries</c>) reaches the handler correctly, that the expected
/// HTTP verbs are used, and that fully populated JSON responses (matching the Bexio v3
/// OpenAPI <c>v3ManualEntryResponse</c>, <c>NextRefNr</c> and <c>FileResponse</c>/<c>FileDetailResponse</c>
/// schemas) deserialize into every field of the C# model.
/// </summary>
public sealed class ManualEntryServiceIntegrationTests : IntegrationTestBase
{
    private const string ManualEntriesPath = "/3.0/accounting/manual_entries";

    private const string ManualEntryResponse = """
        {
            "id": 1,
            "type": "manual_single_entry",
            "date": "2024-01-01",
            "reference_nr": "TEST-1",
            "created_by_user_id": 7,
            "edited_by_user_id": 8,
            "entries": [
                {
                    "id": 32,
                    "date": "2024-01-01",
                    "debit_account_id": 89,
                    "credit_account_id": 90,
                    "tax_id": 15,
                    "tax_account_id": 90,
                    "description": "Smoke entry",
                    "amount": 100.00,
                    "currency_id": 1,
                    "base_currency_id": 1,
                    "currency_factor": 1.0,
                    "base_currency_amount": 100.00,
                    "created_by_user_id": 7,
                    "edited_by_user_id": 8
                }
            ],
            "is_locked": false,
            "locked_info": "is_generated"
        }
        """;

    private const string ManualEntryListResponse = """
        [
            {
                "id": 1,
                "type": "manual_single_entry",
                "date": "2024-01-01",
                "reference_nr": "TEST-1",
                "created_by_user_id": 7,
                "edited_by_user_id": 8,
                "entries": [
                    {
                        "id": 32,
                        "date": "2024-01-01",
                        "debit_account_id": 89,
                        "credit_account_id": 90,
                        "tax_id": 15,
                        "tax_account_id": 90,
                        "description": "Smoke entry",
                        "amount": 100.00,
                        "currency_id": 1,
                        "base_currency_id": 1,
                        "currency_factor": 1.0,
                        "base_currency_amount": 100.00,
                        "created_by_user_id": 7,
                        "edited_by_user_id": 8
                    }
                ],
                "is_locked": false,
                "locked_info": "is_generated"
            }
        ]
        """;

    private const string FileResponseBody = """
        [
            {
                "id": 100,
                "uuid": "11111111-1111-4111-9111-111111111111",
                "name": "receipt.pdf",
                "size_in_bytes": 4096,
                "extension": "pdf",
                "mime_type": "application/pdf",
                "uploader_email": "user@example.com",
                "user_id": 7,
                "is_archived": false,
                "source_type": "web",
                "is_referenced": true,
                "created_at": "2024-01-01T08:30:00Z"
            }
        ]
        """;

    private const string FileDetailResponseBody = """
        {
            "id": 100,
            "uuid": "11111111-1111-4111-9111-111111111111",
            "name": "receipt.pdf",
            "size_in_bytes": 4096,
            "extension": "pdf",
            "mime_type": "application/pdf",
            "uploader_email": null,
            "user_id": 7,
            "is_archived": false,
            "source_type": null,
            "is_referenced": false,
            "created_at": "2024-01-01T08:30:00Z",
            "data": "JVBERi0xLjQK"
        }
        """;

    /// <summary>
    /// <c>ManualEntryService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/3.0/accounting/manual_entries</c> and deserialize a fully populated
    /// <c>v3ManualEntryResponse</c> payload — including nested entry-line fields
    /// such as <c>tax_id</c>, <c>currency_factor</c>, <c>base_currency_amount</c>,
    /// <c>created_by_user_id</c>, <c>edited_by_user_id</c>.
    /// </summary>
    [Test]
    public async Task ManualEntryService_Get_DeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(ManualEntriesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ManualEntryListResponse));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ManualEntriesPath));
            Assert.That(result.Data, Has.Count.EqualTo(1));

            var entry = result.Data![0];
            Assert.That(entry.Id, Is.EqualTo(1));
            Assert.That(entry.Type, Is.EqualTo("manual_single_entry"));
            Assert.That(entry.Date, Is.EqualTo(new DateOnly(2024, 1, 1)));
            Assert.That(entry.ReferenceNr, Is.EqualTo("TEST-1"));
            Assert.That(entry.CreatedByUserId, Is.EqualTo(7));
            Assert.That(entry.EditedByUserId, Is.EqualTo(8));
            Assert.That(entry.IsLocked, Is.False);
            Assert.That(entry.LockedInfo, Is.EqualTo("is_generated"));

            Assert.That(entry.Entries, Has.Count.EqualTo(1));
            var line = entry.Entries[0];
            Assert.That(line.Id, Is.EqualTo(32));
            Assert.That(line.Date, Is.EqualTo(new DateOnly(2024, 1, 1)));
            Assert.That(line.DebitAccountId, Is.EqualTo(89));
            Assert.That(line.CreditAccountId, Is.EqualTo(90));
            Assert.That(line.TaxId, Is.EqualTo(15));
            Assert.That(line.TaxAccountId, Is.EqualTo(90));
            Assert.That(line.Description, Is.EqualTo("Smoke entry"));
            Assert.That(line.Amount, Is.EqualTo(100.00m));
            Assert.That(line.CurrencyId, Is.EqualTo(1));
            Assert.That(line.BaseCurrencyId, Is.EqualTo(1));
            Assert.That(line.CurrencyFactor, Is.EqualTo(1.0m));
            Assert.That(line.BaseCurrencyAmount, Is.EqualTo(100.00m));
            Assert.That(line.CreatedByUserId, Is.EqualTo(7));
            Assert.That(line.EditedByUserId, Is.EqualTo(8));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="ManualEntryEntryCreate"/> payload, and must surface the returned
    /// <see cref="BexioApiNet.Abstractions.Models.Accounting.ManualEntries.ManualEntry"/> with
    /// every field populated from a fully populated 201 response.
    /// </summary>
    [Test]
    public async Task ManualEntryService_Create_SendsPostRequestAndDeserializes()
    {
        Server
            .Given(Request.Create().WithPath(ManualEntriesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ManualEntryResponse));

        var service = new ManualEntryService(ConnectionHandler);

        var payload = new ManualEntryEntryCreate(
            Type: ManualEntryType.manual_single_entry,
            Date: new DateOnly(2024, 1, 1),
            ReferenceNr: "TEST-1",
            Entries: new[]
            {
                new ManualEntryCreate(
                    DebitAccountId: 89,
                    CreditAccountId: 90,
                    TaxId: 15,
                    TaxAccountId: 90,
                    Description: "Smoke entry",
                    Amount: 100m,
                    CurrencyId: 1,
                    CurrencyFactor: 1m)
            });

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(result.Data!.Type, Is.EqualTo("manual_single_entry"));
            Assert.That(result.Data!.ReferenceNr, Is.EqualTo("TEST-1"));
            Assert.That(result.Data!.Entries, Has.Count.EqualTo(1));
            Assert.That(result.Data!.Entries[0].Amount, Is.EqualTo(100.00m));

            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ManualEntriesPath));
            Assert.That(request.Body, Does.Contain("\"reference_nr\":\"TEST-1\""));
            Assert.That(request.Body, Does.Contain("\"type\":\"manual_single_entry\""));
            Assert.That(request.Body, Does.Contain("\"date\":\"2024-01-01\""));
            Assert.That(request.Body, Does.Contain("\"debit_account_id\":89"));
            Assert.That(request.Body, Does.Contain("\"credit_account_id\":90"));
            Assert.That(request.Body, Does.Contain("\"tax_id\":15"));
            Assert.That(request.Body, Does.Contain("\"tax_account_id\":90"));
            Assert.That(request.Body, Does.Contain("\"description\":\"Smoke entry\""));
            Assert.That(request.Body, Does.Contain("\"amount\":100"));
            Assert.That(request.Body, Does.Contain("\"currency_id\":1"));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.Put</c> must issue a <c>PUT</c> request to
    /// <c>/3.0/accounting/manual_entries/{id}</c> with the serialized
    /// <see cref="ManualEntryUpdate"/> payload — including the entry-line <c>id</c> values
    /// required by the spec for line-level updates.
    /// </summary>
    [Test]
    public async Task ManualEntryService_Put_SendsPutRequestWithIdInPath()
    {
        const int manualEntryId = 1;
        var expectedPath = $"{ManualEntriesPath}/{manualEntryId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ManualEntryResponse));

        var service = new ManualEntryService(ConnectionHandler);

        var payload = new ManualEntryUpdate(
            Type: ManualEntryType.manual_single_entry,
            Date: new DateOnly(2024, 1, 1),
            ReferenceNr: "TEST-1",
            Entries: new[]
            {
                new ManualEntryEntryUpdate(
                    DebitAccountId: 89,
                    CreditAccountId: 90,
                    TaxId: 15,
                    TaxAccountId: 90,
                    Description: "Smoke entry updated",
                    Amount: 200m,
                    CurrencyId: 1,
                    CurrencyFactor: 1m,
                    Id: 32)
            },
            Id: manualEntryId);

        var result = await service.Put(manualEntryId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(manualEntryId));
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"reference_nr\":\"TEST-1\""));
            Assert.That(request.Body, Does.Contain("\"id\":1"));
            Assert.That(request.Body, Does.Contain("\"description\":\"Smoke entry updated\""));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.GetNextRefNr</c> must issue a <c>GET</c> against
    /// <c>/3.0/accounting/manual_entries/next_ref_nr</c> and deserialize the
    /// <c>NextRefNr</c> response object.
    /// </summary>
    [Test]
    public async Task ManualEntryService_GetNextRefNr_DeserializesPayload()
    {
        const string expectedPath = $"{ManualEntriesPath}/next_ref_nr";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"next_ref_nr\":\"42\"}"));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.GetNextRefNr(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.NextRefNr, Is.EqualTo("42"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.GetFiles</c> must issue a <c>GET</c> against
    /// <c>/3.0/accounting/manual_entries/{id}/files</c> and deserialize a fully populated
    /// <c>FileResponse</c> array — verifying every field of the schema.
    /// </summary>
    [Test]
    public async Task ManualEntryService_GetFiles_DeserializesAllFields()
    {
        const int manualEntryId = 1;
        var expectedPath = $"{ManualEntriesPath}/{manualEntryId}/files";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FileResponseBody));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.GetFiles(manualEntryId, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(result.Data, Has.Count.EqualTo(1));

            var file = result.Data![0];
            Assert.That(file.Id, Is.EqualTo(100));
            Assert.That(file.Uuid, Is.EqualTo("11111111-1111-4111-9111-111111111111"));
            Assert.That(file.Name, Is.EqualTo("receipt.pdf"));
            Assert.That(file.SizeInBytes, Is.EqualTo(4096));
            Assert.That(file.Extension, Is.EqualTo("pdf"));
            Assert.That(file.MimeType, Is.EqualTo("application/pdf"));
            Assert.That(file.UploaderEmail, Is.EqualTo("user@example.com"));
            Assert.That(file.UserId, Is.EqualTo(7));
            Assert.That(file.IsArchived, Is.False);
            Assert.That(file.SourceType, Is.EqualTo(ManualEntryFileSourceType.web));
            Assert.That(file.IsReferenced, Is.True);
            Assert.That(file.CreatedAt, Is.EqualTo(new DateTime(2024, 1, 1, 8, 30, 0, DateTimeKind.Utc)));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.GetFileById</c> must issue a <c>GET</c> against
    /// <c>/3.0/accounting/manual_entries/{id}/files/{fileId}</c> and deserialize the
    /// <c>FileDetailResponse</c> payload — including the base64-encoded <c>data</c> field.
    /// </summary>
    [Test]
    public async Task ManualEntryService_GetFileById_DeserializesDataField()
    {
        const int manualEntryId = 1;
        const int fileId = 100;
        var expectedPath = $"{ManualEntriesPath}/{manualEntryId}/files/{fileId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FileDetailResponseBody));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.GetFileById(manualEntryId, fileId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(fileId));
            Assert.That(result.Data!.Uuid, Is.EqualTo("11111111-1111-4111-9111-111111111111"));
            Assert.That(result.Data!.Name, Is.EqualTo("receipt.pdf"));
            Assert.That(result.Data!.SizeInBytes, Is.EqualTo(4096));
            Assert.That(result.Data!.Extension, Is.EqualTo("pdf"));
            Assert.That(result.Data!.MimeType, Is.EqualTo("application/pdf"));
            Assert.That(result.Data!.UploaderEmail, Is.Null);
            Assert.That(result.Data!.UserId, Is.EqualTo(7));
            Assert.That(result.Data!.IsArchived, Is.False);
            Assert.That(result.Data!.SourceType, Is.Null);
            Assert.That(result.Data!.IsReferenced, Is.False);
            Assert.That(result.Data!.Data, Is.EqualTo("JVBERi0xLjQK"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.GetEntryFiles</c> must issue a <c>GET</c> against
    /// <c>/3.0/accounting/manual_entries/{id}/entries/{entryId}/files</c> and deserialize
    /// the <c>FileResponse</c> array.
    /// </summary>
    [Test]
    public async Task ManualEntryService_GetEntryFiles_DeserializesPayload()
    {
        const int manualEntryId = 1;
        const int entryId = 32;
        var expectedPath = $"{ManualEntriesPath}/{manualEntryId}/entries/{entryId}/files";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FileResponseBody));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.GetEntryFiles(manualEntryId, entryId, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(100));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.GetEntryFileById</c> must issue a <c>GET</c> against
    /// <c>/3.0/accounting/manual_entries/{id}/entries/{entryId}/files/{fileId}</c> and
    /// deserialize the <c>FileDetailResponse</c> payload (including base64 data).
    /// </summary>
    [Test]
    public async Task ManualEntryService_GetEntryFileById_DeserializesPayload()
    {
        const int manualEntryId = 1;
        const int entryId = 32;
        const int fileId = 100;
        var expectedPath = $"{ManualEntriesPath}/{manualEntryId}/entries/{entryId}/files/{fileId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FileDetailResponseBody));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.GetEntryFileById(manualEntryId, entryId, fileId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(fileId));
            Assert.That(result.Data!.Data, Is.EqualTo("JVBERi0xLjQK"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.Delete</c> must issue a <c>DELETE</c> request that includes the
    /// target id in the URL path and must surface the <c>2xx</c> response as a successful
    /// <c>ApiResult</c>.
    /// </summary>
    [Test]
    public async Task ManualEntryService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
        var expectedPath = $"{ManualEntriesPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.Delete(idToDelete, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.DeleteFile</c> must issue a <c>DELETE</c> against
    /// <c>/3.0/accounting/manual_entries/{id}/files/{fileId}</c>.
    /// </summary>
    [Test]
    public async Task ManualEntryService_DeleteFile_SendsDeleteRequest()
    {
        const int manualEntryId = 1;
        const int fileId = 100;
        var expectedPath = $"{ManualEntriesPath}/{manualEntryId}/files/{fileId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.DeleteFile(manualEntryId, fileId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>ManualEntryService.DeleteEntryFile</c> must issue a <c>DELETE</c> against
    /// <c>/3.0/accounting/manual_entries/{id}/entries/{entryId}/files/{fileId}</c>.
    /// </summary>
    [Test]
    public async Task ManualEntryService_DeleteEntryFile_SendsDeleteRequest()
    {
        const int manualEntryId = 1;
        const int entryId = 32;
        const int fileId = 100;
        var expectedPath = $"{ManualEntriesPath}/{manualEntryId}/entries/{entryId}/files/{fileId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new ManualEntryService(ConnectionHandler);

        var result = await service.DeleteEntryFile(manualEntryId, entryId, fileId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
