// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Diagnostics.Debug;

namespace Windows.Win32.Foundation;

public class HRESULTTests
{
    [Fact]
    public void Code_ReturnsLowSixteenBits()
    {
        HRESULT hr = (HRESULT)unchecked((int)0x80070005);
        Assert.Equal(0x0005, hr.Code);
    }

    [Fact]
    public void Facility_ReturnsExpectedFacility()
    {
        HRESULT hr = (HRESULT)unchecked((int)0x80070005);
        Assert.Equal(FACILITY_CODE.FACILITY_WIN32, hr.Facility);
    }

    [Fact]
    public void ExplicitCastFromWin32Error_Success_ReturnsZero()
    {
        HRESULT hr = (HRESULT)WIN32_ERROR.ERROR_SUCCESS;
        Assert.Equal(0, hr.Value);
    }

    [Fact]
    public void ExplicitCastFromWin32Error_NonZero_SetsFacilityWin32AndSeverityBit()
    {
        HRESULT hr = (HRESULT)WIN32_ERROR.ERROR_FILE_NOT_FOUND;
        Assert.Equal(unchecked((int)0x80070002), hr.Value);
        Assert.Equal(FACILITY_CODE.FACILITY_WIN32, hr.Facility);
        Assert.Equal((int)WIN32_ERROR.ERROR_FILE_NOT_FOUND, hr.Code);
    }

    [Fact]
    public void ImplicitCastToException_FailingHResult_ReturnsException()
    {
        Exception ex = HRESULT.COR_E_ARGUMENT;
        Assert.NotNull(ex);
    }

    [Fact]
    public void ToStringWithDescription_Win32Facility_ContainsErrorName()
    {
        HRESULT hr = (HRESULT)WIN32_ERROR.ERROR_FILE_NOT_FOUND;
        string text = hr.ToStringWithDescription();
        Assert.Contains("0x80070002", text);
        Assert.Contains("ERROR_FILE_NOT_FOUND", text);
    }

    [Fact]
    public void ToStringWithDescription_NonWin32Facility_ContainsHexValue()
    {
        string text = HRESULT.COR_E_OBJECTDISPOSED.ToStringWithDescription();
        Assert.Contains("0x80131622", text);
    }
}
