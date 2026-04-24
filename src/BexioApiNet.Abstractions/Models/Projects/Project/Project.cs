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

namespace BexioApiNet.Abstractions.Models.Projects.Project;

/// <summary>
/// Project as returned by the Bexio projects endpoint.
/// <see href="https://docs.bexio.com/#tag/Projects/operation/v2ShowProject"/>
/// </summary>
/// <param name="Id">Unique project identifier (read-only).</param>
/// <param name="Uuid">Stable uuid of the project (read-only).</param>
/// <param name="Nr">Project number assigned by Bexio (read-only).</param>
/// <param name="Name">Display name of the project.</param>
/// <param name="StartDate">Project start date in Bexio's <c>yyyy-MM-dd HH:mm:ss</c> format.</param>
/// <param name="EndDate">Project end date in Bexio's <c>yyyy-MM-dd HH:mm:ss</c> format.</param>
/// <param name="Comment">Free-text comment for the project.</param>
/// <param name="PrStateId">Reference to a project state object.</param>
/// <param name="PrProjectTypeId">Reference to a project type object.</param>
/// <param name="ContactId">Reference to the primary contact object.</param>
/// <param name="ContactSubId">Reference to a secondary (sub) contact object.</param>
/// <param name="PrInvoiceTypeId">Invoice type id. <c>1</c> type_hourly_rate_service, <c>2</c> type_hourly_rate_employee, <c>3</c> type_hourly_rate_project, <c>4</c> type_fix.</param>
/// <param name="PrInvoiceTypeAmount">Invoice type amount as a decimal string. Only editable with supporting <see cref="PrInvoiceTypeId"/> (<c>type_hourly_rate_project</c> or <c>type_fix</c>).</param>
/// <param name="PrBudgetTypeId">Budget type id. <c>1</c> type_budgeted_costs, <c>2</c> type_budgeted_hours, <c>3</c> type_service_budget, <c>4</c> type_service_employees.</param>
/// <param name="PrBudgetTypeAmount">Budget type amount as a decimal string. Only editable with supporting <see cref="PrBudgetTypeId"/> (<c>type_budgeted_costs</c> or <c>type_budgeted_hours</c>).</param>
/// <param name="UserId">Reference to the user that owns the project.</param>
public sealed record Project(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("uuid")] string? Uuid,
    [property: JsonPropertyName("nr")] string? Nr,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("start_date")] string? StartDate,
    [property: JsonPropertyName("end_date")] string? EndDate,
    [property: JsonPropertyName("comment")] string? Comment,
    [property: JsonPropertyName("pr_state_id")] int PrStateId,
    [property: JsonPropertyName("pr_project_type_id")] int PrProjectTypeId,
    [property: JsonPropertyName("contact_id")] int ContactId,
    [property: JsonPropertyName("contact_sub_id")] int? ContactSubId,
    [property: JsonPropertyName("pr_invoice_type_id")] int? PrInvoiceTypeId,
    [property: JsonPropertyName("pr_invoice_type_amount")] string? PrInvoiceTypeAmount,
    [property: JsonPropertyName("pr_budget_type_id")] int? PrBudgetTypeId,
    [property: JsonPropertyName("pr_budget_type_amount")] string? PrBudgetTypeAmount,
    [property: JsonPropertyName("user_id")] int UserId
);
