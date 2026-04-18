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

namespace BexioApiNet.UnitTests;

/// <summary>
/// Base class for offline unit tests of connector services. Provides a fresh
/// <see cref="IBexioConnectionHandler"/> substitute (via NSubstitute) for every
/// test so each test operates in isolation. Inheriting test fixtures should use
/// <see cref="ConnectionHandler"/> to configure return values and to verify calls
/// with <c>ConnectionHandler.Received()</c>.
/// </summary>
[Category("Unit")]
public abstract class ServiceTestBase
{
    /// <summary>
    /// NSubstitute mock of <see cref="IBexioConnectionHandler"/> created fresh for each test.
    /// Use this to arrange return values (<c>ConnectionHandler.GetAsync&lt;T&gt;(...).Returns(...)</c>)
    /// and to assert calls (<c>await ConnectionHandler.Received(1).GetAsync(...)</c>).
    /// </summary>
    protected IBexioConnectionHandler ConnectionHandler { get; private set; } = null!;

    /// <summary>
    /// Creates a new substitute per test to guarantee isolation and to capture call
    /// history for assertions. Called automatically by NUnit before every test.
    /// </summary>
    [SetUp]
    public void SetUpConnectionHandler()
    {
        ConnectionHandler = Substitute.For<IBexioConnectionHandler>();
    }

    /// <summary>
    /// Disposes the current substitute after each test. The NSubstitute proxy
    /// implements <see cref="IDisposable"/> because <see cref="IBexioConnectionHandler"/>
    /// does, so explicit disposal keeps the NUnit analyzer (NUnit1032) happy.
    /// </summary>
    [TearDown]
    public void TearDownConnectionHandler()
    {
        ConnectionHandler.Dispose();
    }
}
