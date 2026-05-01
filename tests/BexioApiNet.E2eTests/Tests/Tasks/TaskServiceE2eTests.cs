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

using BexioApiNet.Abstractions.Models.Tasks.Task.Views;

namespace BexioApiNet.E2eTests.Tests.Tasks;

/// <summary>
///     Live E2E coverage for <c>/2.0/task</c>. Verifies the list endpoint and the full
///     Create → Read → Update → Delete lifecycle against the live Bexio API. Skipped automatically
///     when Bexio credentials are missing via <see cref="BexioE2eTestBase" />.
/// </summary>
public class TaskServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    ///     Retrieves the task list from the live Bexio API and asserts the call is successful.
    ///     Performs structural assertions against the OpenAPI <c>Task</c> schema fields when at
    ///     least one task exists.
    /// </summary>
    [Test]
    public async Task GetAll_StructurallyMatchesOpenApiSchema()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.Tasks.Get();

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });

        if (res.Data is null || res.Data.Count is 0)
            return;

        var task = res.Data[0];
        Assert.Multiple(() =>
        {
            Assert.That(task.Id, Is.GreaterThan(0));
            Assert.That(task.UserId, Is.GreaterThan(0));
            Assert.That(task.Subject, Is.Not.Null);
            Assert.That(task.TodoStatusId, Is.GreaterThan(0));
        });
    }

    /// <summary>
    ///     Exercises the full Create → Read → Update → Delete lifecycle against the live Bexio API.
    ///     The task is uniquely tagged with an <c>"E2E-Task-"</c> subject prefix so any orphan
    ///     records are trivial to spot in the Bexio UI. The <c>finally</c> block guarantees cleanup
    ///     even when an assertion fails so the test tenant is not polluted.
    /// </summary>
    [Test]
    public async Task CreateReadUpdateDelete_LifecycleSucceeds()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        // Discover a valid user_id to satisfy the Bexio "user_id" required field on create.
        var users = await BexioApiClient!.Users.GetAll();
        Assert.Multiple(() =>
        {
            Assert.That(users.IsSuccess, Is.True);
            Assert.That(users.Data, Is.Not.Null);
            Assert.That(users.Data, Is.Not.Empty);
        });
        var userId = users.Data!.First().Id;

        // Discover a valid todo_status_id from the read-only lookup.
        var statuses = await BexioApiClient.TaskStatuses.Get();
        Assert.That(statuses.IsSuccess, Is.True);
        Assert.That(statuses.Data, Is.Not.Null.And.Not.Empty);
        var statusId = statuses.Data![0].Id;

        var subject = $"E2E-Task-Lifecycle-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var created = await BexioApiClient.Tasks.Create(new TaskCreate(
            UserId: userId,
            Subject: subject,
            Info: "Lifecycle test — created by automated E2E suite.",
            TodoStatusId: statusId));

        Assert.Multiple(() =>
        {
            Assert.That(created.IsSuccess, Is.True);
            Assert.That(created.Data, Is.Not.Null);
            Assert.That(created.Data!.Subject, Is.EqualTo(subject));
        });

        var taskId = created.Data!.Id;
        try
        {
            // Read back via GetById and confirm the persisted task matches the create payload.
            var fetched = await BexioApiClient.Tasks.GetById(taskId);
            Assert.Multiple(() =>
            {
                Assert.That(fetched.IsSuccess, Is.True);
                Assert.That(fetched.Data, Is.Not.Null);
                Assert.That(fetched.Data!.Id, Is.EqualTo(taskId));
                Assert.That(fetched.Data.Subject, Is.EqualTo(subject));
            });

            // Update the subject to verify the POST {id} edit endpoint round-trips.
            var updatedSubject = subject + "-Updated";
            var updated = await BexioApiClient.Tasks.Update(taskId, new TaskUpdate(
                UserId: userId,
                Subject: updatedSubject,
                Info: "Lifecycle test — updated by automated E2E suite.",
                TodoStatusId: statusId));
            Assert.Multiple(() =>
            {
                Assert.That(updated.IsSuccess, Is.True);
                Assert.That(updated.Data, Is.Not.Null);
                Assert.That(updated.Data!.Subject, Is.EqualTo(updatedSubject));
            });
        }
        finally
        {
            // Best-effort cleanup so the tenant is left clean even if assertions failed.
            await BexioApiClient.Tasks.Delete(taskId);
        }
    }
}
