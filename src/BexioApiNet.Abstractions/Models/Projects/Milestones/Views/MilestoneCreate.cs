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

namespace BexioApiNet.Abstractions.Models.Projects.Milestones.Views;

/// <summary>
///     Create view for a milestone — body of <c>POST /3.0/projects/{project_id}/milestones</c>. The
///     read-only <c>id</c> field is intentionally omitted.
///     <see href="https://docs.bexio.com/#tag/Projects/operation/CreateMilestone" />
/// </summary>
/// <param name="Name">Name of the milestone (required, max 255 characters).</param>
/// <param name="EndDate">End date for the milestone.</param>
/// <param name="Comment">Free-text description for the milestone (max 10000 characters).</param>
/// <param name="ParentMilestoneId">References a higher-level milestone (parent milestone).</param>
public sealed record MilestoneCreate(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("end_date")]
    DateOnly? EndDate = null,
    [property: JsonPropertyName("comment")]
    string? Comment = null,
    [property: JsonPropertyName("pr_parent_milestone_id")]
    int? ParentMilestoneId = null
);
