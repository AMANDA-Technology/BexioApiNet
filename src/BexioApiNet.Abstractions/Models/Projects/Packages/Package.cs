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

namespace BexioApiNet.Abstractions.Models.Projects.Packages;

/// <summary>
///     Work package as returned by the Bexio projects packages endpoint. Packages are nested under
///     a project and accessed via <c>/3.0/projects/{project_id}/packages</c>.
///     <see href="https://docs.bexio.com/#tag/Projects/operation/ListWorkPackages" />
/// </summary>
/// <param name="Id">Unique work package identifier (read-only).</param>
/// <param name="Name">Name of the work package (max 255 characters).</param>
/// <param name="SpentTimeInHours">Time spent on the work package, in hours.</param>
/// <param name="EstimatedTimeInHours">Estimated time for the work package, in hours.</param>
/// <param name="Comment">Free-text description for the work package (max 10000 characters).</param>
/// <param name="MilestoneId">References a milestone object.</param>
public sealed record Package(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("spent_time_in_hours")]
    decimal? SpentTimeInHours,
    [property: JsonPropertyName("estimated_time_in_hours")]
    decimal? EstimatedTimeInHours,
    [property: JsonPropertyName("comment")]
    string? Comment,
    [property: JsonPropertyName("pr_milestone_id")]
    int? MilestoneId
);
