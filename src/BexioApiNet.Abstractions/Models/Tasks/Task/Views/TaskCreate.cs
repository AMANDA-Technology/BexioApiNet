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

namespace BexioApiNet.Abstractions.Models.Tasks.Task.Views;

/// <summary>
///     Create view for a Bexio task.
///     <see href="https://docs.bexio.com/#tag/Tasks/operation/v2CreateTask">Create Task</see>
/// </summary>
/// <param name="UserId">
///     The id of the user the task is assigned to. References a
///     <see href="https://docs.bexio.com/#tag/Users">user</see>.
/// </param>
/// <param name="Subject">The subject (title) of the task.</param>
/// <param name="FinishDate">The optional due date of the task.</param>
/// <param name="Info">Additional information or notes to attach to the task.</param>
/// <param name="ContactId">The id of the contact the task references, if any.</param>
/// <param name="SubContactId">The id of the sub-contact the task references, if any.</param>
/// <param name="PrProjectId">The id of the project the task is linked to, if any. Submitted as <c>pr_project_id</c>.</param>
/// <param name="EntryId">The id of the entry the task is linked to, if any.</param>
/// <param name="ModuleId">The id of the module the task is linked to, if any.</param>
/// <param name="TodoStatusId">The id of the task status to assign, if any.</param>
/// <param name="TodoPriorityId">The id of the task priority to assign, if any.</param>
/// <param name="HaveRemember">
///     Whether Bexio should set a reminder on the task. When <see langword="true" />,
///     <c>remember_type_id</c> and <c>remember_time_id</c> are required.
/// </param>
/// <param name="RememberTypeId">
///     The id of the reminder type. Required when <paramref name="HaveRemember" /> is
///     <see langword="true" />.
/// </param>
/// <param name="RememberTimeId">
///     The id of the reminder time. Required when <paramref name="HaveRemember" /> is
///     <see langword="true" />.
/// </param>
/// <param name="CommunicationKindId">The id of the communication kind, if any.</param>
public sealed record TaskCreate(
    [property: JsonPropertyName("user_id")]
    int UserId,
    [property: JsonPropertyName("subject")]
    string Subject,
    [property: JsonPropertyName("finish_date")]
    DateTimeOffset? FinishDate = null,
    [property: JsonPropertyName("info")] string? Info = null,
    [property: JsonPropertyName("contact_id")]
    int? ContactId = null,
    [property: JsonPropertyName("sub_contact_id")]
    int? SubContactId = null,
    [property: JsonPropertyName("pr_project_id")]
    int? PrProjectId = null,
    [property: JsonPropertyName("entry_id")]
    int? EntryId = null,
    [property: JsonPropertyName("module_id")]
    int? ModuleId = null,
    [property: JsonPropertyName("todo_status_id")]
    int? TodoStatusId = null,
    [property: JsonPropertyName("todo_priority_id")]
    int? TodoPriorityId = null,
    [property: JsonPropertyName("have_remember")]
    bool? HaveRemember = null,
    [property: JsonPropertyName("remember_type_id")]
    int? RememberTypeId = null,
    [property: JsonPropertyName("remember_time_id")]
    int? RememberTimeId = null,
    [property: JsonPropertyName("communication_kind_id")]
    int? CommunicationKindId = null
);