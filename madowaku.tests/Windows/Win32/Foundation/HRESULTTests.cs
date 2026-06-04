// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Diagnostics.Debug;

namespace Windows.Win32.Foundation;

[TestClass]
public class HRESULTTests
{
    [TestMethod]
    public void Code_ReturnsLowSixteenBits()
    {
        HRESULT hr = (HRESULT)unchecked((int)0x80070005);
        hr.Code.Should().Be(0x0005);
    }

    [TestMethod]
    public void Facility_ReturnsExpectedFacility()
    {
        HRESULT hr = (HRESULT)unchecked((int)0x80070005);
        hr.Facility.Should().Be(FACILITY_CODE.FACILITY_WIN32);
    }

    [TestMethod]
    public void ExplicitCastFromWin32Error_Success_ReturnsZero()
    {
        HRESULT hr = (HRESULT)WIN32_ERROR.ERROR_SUCCESS;
        hr.Value.Should().Be(0);
    }

    [TestMethod]
    public void ExplicitCastFromWin32Error_NonZero_SetsFacilityWin32AndSeverityBit()
    {
        HRESULT hr = (HRESULT)WIN32_ERROR.ERROR_FILE_NOT_FOUND;
        hr.Value.Should().Be(unchecked((int)0x80070002));
        hr.Facility.Should().Be(FACILITY_CODE.FACILITY_WIN32);
        hr.Code.Should().Be((int)WIN32_ERROR.ERROR_FILE_NOT_FOUND);
    }

    [TestMethod]
    public void ImplicitCastToException_FailingHResult_ReturnsException()
    {
        Exception ex = HRESULT.COR_E_ARGUMENT;
        ex.Should().NotBeNull();
    }

    [TestMethod]
    public void ToStringWithDescription_Win32Facility_ContainsErrorName()
    {
        HRESULT hr = (HRESULT)WIN32_ERROR.ERROR_FILE_NOT_FOUND;
        string text = hr.ToStringWithDescription();
        text.Should().Contain("0x80070002");
        text.Should().Contain("ERROR_FILE_NOT_FOUND");
    }

    [TestMethod]
    public void ToStringWithDescription_NonWin32Facility_ContainsHexValue()
    {
        string text = HRESULT.COR_E_OBJECTDISPOSED.ToStringWithDescription();
        text.Should().Contain("0x80131622");
    }
}
