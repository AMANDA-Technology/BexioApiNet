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

namespace BexioApiNet.Services.Connectors.Payroll;

/// <summary>
/// Absence endpoint configuration. Bexio <c>4.0/payroll/employees/{employeeId}/absences</c>.
/// Absences are nested under employees, so the <see cref="EndpointRoot"/> covers only the
/// employee path segment — callers append <c>/{employeeId}/absences</c> at the call site.
/// </summary>
public static class AbsenceConfiguration
{
    /// <summary>
    /// Current api version of the endpoint.
    /// </summary>
    public const string ApiVersion = "4.0";

    /// <summary>
    /// The request path prefix for the parent payroll employee resource.
    /// </summary>
    public const string EndpointRoot = "payroll/employees";
}
