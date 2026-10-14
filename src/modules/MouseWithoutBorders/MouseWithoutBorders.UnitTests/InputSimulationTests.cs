// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using MouseWithoutBorders.Class;

namespace MouseWithoutBorders.UnitTests;

// The tests pass their own stand-ins for the input injector and SendInput, so they never send real input.
[TestClass]
public sealed class InputSimulationTests
{
    // The input injector fails with these now and then (#43292): HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER), which
    // arrives as a COMException, and E_ACCESSDENIED, which arrives as an UnauthorizedAccessException.
    private const int InsufficientBuffer = unchecked((int)0x8007007A);
    private const int AccessDenied = unchecked((int)0x80070005);

    private int injections;
    private int sendInputs;

    [TestMethod]
    public void InjectOrSendInputShouldInjectAMouseMoveWithoutSendInput()
    {
        uint result = InputSimulation.InjectOrSendInput(true, Inject, SendInput);

        Assert.AreEqual(1, injections);
        Assert.AreEqual(0, sendInputs);
        Assert.AreEqual(0u, result);
    }

    [TestMethod]
    public void InjectOrSendInputShouldSendAMoveTheInjectorRefuses()
    {
        uint result = InputSimulation.InjectOrSendInput(true, () => throw Marshal.GetExceptionForHR(InsufficientBuffer)!, SendInput);

        Assert.AreEqual(1, sendInputs);
        Assert.AreEqual(1u, result);
    }

    [TestMethod]
    public void InjectOrSendInputShouldSendAMoveTheInjectorIsNotAllowedToInject()
    {
        uint result = InputSimulation.InjectOrSendInput(true, () => throw Marshal.GetExceptionForHR(AccessDenied)!, SendInput);

        Assert.AreEqual(1, sendInputs);
        Assert.AreEqual(1u, result);
    }

    [TestMethod]
    public void InjectOrSendInputShouldSendOtherInputWithoutTheInjector()
    {
        uint result = InputSimulation.InjectOrSendInput(false, Inject, SendInput);

        Assert.AreEqual(0, injections);
        Assert.AreEqual(1, sendInputs);
        Assert.AreEqual(1u, result);
    }

    private void Inject() => injections++;

    private uint SendInput()
    {
        sendInputs++;
        return 1;
    }
}
