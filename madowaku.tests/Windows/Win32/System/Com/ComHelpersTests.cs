// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

#if NET
namespace Windows.Win32;

[TestClass]
public class ComHelpersTests
{
    [TestMethod]
    public unsafe void PopulateIUnknown_EmptyVtable_PopulatesIUnknownSlots()
    {
        System.Com.IUnknown.Vtbl vtable = default;

        ComHelpers.PopulateIUnknown<System.Com.IUnknown>(&vtable);

        nint* entries = (nint*)&vtable;
        entries[0].Should().NotBe(0);
        entries[1].Should().NotBe(0);
        entries[2].Should().NotBe(0);
    }
}
#endif