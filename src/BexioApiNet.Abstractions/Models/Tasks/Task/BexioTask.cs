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

namespace BexioApiNet.Abstractions.Models.Tasks.Task;

/// <summary>
///     A Bexio task. Renamed to <c>BexioTask</c> to avoid collision with <see cref="System.Threading.Tasks.Task" />.
///     <see href="https://docs.bexio.com/#tag/Tasks/operation/v2ListTasks">List Tasks</see>
/// </summary>
/// <param name="Id">The id of the task.</param>
/// <param name="UserId">
///     The id of the user the task is assigned to. References a
///     <see href="https://docs.bexio.com/#tag/Users">user</see>.
/// </param>
/// <param name="FinishDate">The optional due date of the task.</param>
/// <param name="Subject">The subject (title) of the task.</param>
/// <param name="Place">The place id associated with the task, if any.</param>
/// <param name="Info">Additional information or notes attached to the task.</param>
/// <param name="ContactId">
///     The id of the contact the task references. See
///     <see href="https://docs.bexio.com/#tag/Contacts">contacts</see>.
/// </param>
/// <param name="SubContactId">The id of the sub-contact the task references, if any.</param>
/// <param name="ProjectId">The id of the project the task is linked to, if any.</param>
/// <param name="EntryId">The id of the entry the task is linked to, if any.</param>
/// <param name="ModuleId">The id of the module the task is linked to, if any.</param>
/// <param name="TodoStatusId">
///     The id of the task status. See
///     <see href="https://docs.bexio.com/#tag/Tasks/operation/v2ListTaskStatus">List Task Status</see>.
/// </param>
/// <param name="TodoPriorityId">
///     The id of the task priority. See
///     <see href="https://docs.bexio.com/#tag/Tasks/operation/v2ListTaskPriority">List Task Priority</see>.
/// </param>
/// <param name="HasReminder">Whether the task has a reminder set. Read-only.</param>
/// <param name="RememberTypeId">
///     The id of the reminder type. Required when <c>have_remember</c> is submitted as
///     <see langword="true" />.
/// </param>
/// <param name="RememberTimeId">
///     The id of the reminder time. Required when <c>have_remember</c> is submitted as
///     <see langword="true" />.
/// </param>
/// <param name="CommunicationKindId">The id of the communication kind, if any.</param>
public sealed record BexioTask(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("user_id")]
    int UserId,
    [property: JsonPropertyName("finish_date")]
    DateTimeOffset? FinishDate,
    [property: JsonPropertyName("subject")]
    string Subject,
    [property: JsonPropertyName("place")] int? Place,
    [property: JsonPropertyName("info")] string? Info,
    [property: JsonPropertyName("contact_id")]
    int? ContactId,
    [property: JsonPropertyName("sub_contact_id")]
    int? SubContactId,
    [property: JsonPropertyName("project_id")]
    int? ProjectId,
    [property: JsonPropertyName("entry_id")]
    int? EntryId,
    [property: JsonPropertyName("module_id")]
    int? ModuleId,
    [property: JsonPropertyName("todo_status_id")]
    int TodoStatusId,
    [property: JsonPropertyName("todo_priority_id")]
    int? TodoPriorityId,
    [property: JsonPropertyName("has_reminder")]
    bool? HasReminder,
    [property: JsonPropertyName("remember_type_id")]
    int? RememberTypeId,
    [property: JsonPropertyName("remember_time_id")]
    int? RememberTimeId,
    [property: JsonPropertyName("communication_kind_id")]
    int? CommunicationKindId
);