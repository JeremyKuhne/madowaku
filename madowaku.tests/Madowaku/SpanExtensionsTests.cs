// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;

namespace Madowaku;

[TestClass]
public unsafe class SpanExtensionsTests
{
    [TestMethod]
    public void CopyToNative_ReadOnlySpan_NoNullTerminate_CopiesData()
    {
        int[] data = [1, 2, 3, 4, 5];
        HGLOBAL global = ((ReadOnlySpan<int>)data).CopyToNative();

        try
        {
            void* memory = PInvoke.GlobalLock(global);
            try
            {
                Span<int> result = new(memory, data.Length);
                result.ToArray().Should().Equal(data);
            }
            finally
            {
                PInvoke.GlobalUnlock(global);
            }
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void CopyToNative_ReadOnlySpan_NullTerminate_AddsDefaultValue()
    {
        int[] data = [1, 2, 3];
        HGLOBAL global = ((ReadOnlySpan<int>)data).CopyToNative(nullTerminate: true);

        try
        {
            void* memory = PInvoke.GlobalLock(global);
            try
            {
                Span<int> result = new(memory, data.Length + 1);
                result[..data.Length].ToArray().Should().Equal(data);
                result[^1].Should().Be(0);
            }
            finally
            {
                PInvoke.GlobalUnlock(global);
            }
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void CopyToNative_Span_NoNullTerminate_CopiesData()
    {
        int[] data = [10, 20, 30];
        Span<int> span = data;
        HGLOBAL global = span.CopyToNative();

        try
        {
            void* memory = PInvoke.GlobalLock(global);
            try
            {
                Span<int> result = new(memory, data.Length);
                result.ToArray().Should().Equal(data);
            }
            finally
            {
                PInvoke.GlobalUnlock(global);
            }
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void CopyToNative_EmptySpan_NullTerminate_AllocatesTerminatorOnly()
    {
        ReadOnlySpan<int> span = default;
        HGLOBAL global = span.CopyToNative(nullTerminate: true);

        try
        {
            global.IsNull.Should().BeFalse();

            void* memory = PInvoke.GlobalLock(global);
            try
            {
                Span<int> result = new(memory, 1);
                result[0].Should().Be(0);
            }
            finally
            {
                PInvoke.GlobalUnlock(global);
            }
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void CopyToNative_ReadOnlySpan_ToExistingHGLOBAL_CopiesData()
    {
        int[] data = [1, 2, 3, 4];
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, (nuint)(data.Length * sizeof(int)));

        try
        {
            ((ReadOnlySpan<int>)data).CopyToNative(global);

            void* memory = PInvoke.GlobalLock(global);
            try
            {
                Span<int> result = new(memory, data.Length);
                result.ToArray().Should().Equal(data);
            }
            finally
            {
                PInvoke.GlobalUnlock(global);
            }
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void CopyToNative_ReadOnlySpan_ToExistingHGLOBAL_NullTerminate_AddsDefaultValue()
    {
        int[] data = [1, 2, 3];
        HGLOBAL global = PInvoke.GlobalAlloc(
            GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE,
            (nuint)((data.Length + 1) * sizeof(int)));

        try
        {
            ((ReadOnlySpan<int>)data).CopyToNative(global, nullTerminate: true);

            void* memory = PInvoke.GlobalLock(global);
            try
            {
                Span<int> result = new(memory, data.Length + 1);
                result[..data.Length].ToArray().Should().Equal(data);
                result[^1].Should().Be(0);
            }
            finally
            {
                PInvoke.GlobalUnlock(global);
            }
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void CopyToNative_Span_ToExistingHGLOBAL_CopiesData()
    {
        int[] data = [5, 6, 7];
        Span<int> span = data;
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, (nuint)(data.Length * sizeof(int)));

        try
        {
            span.CopyToNative(global);

            void* memory = PInvoke.GlobalLock(global);
            try
            {
                Span<int> result = new(memory, data.Length);
                result.ToArray().Should().Equal(data);
            }
            finally
            {
                PInvoke.GlobalUnlock(global);
            }
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void CopyToNative_ToExistingHGLOBAL_DestinationTooSmall_ThrowsArgumentException()
    {
        int[] data = [1, 2, 3];
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(int));

        try
        {
            FluentActions.Invoking(() => ((ReadOnlySpan<int>)data).CopyToNative(global))
                .Should().Throw<ArgumentException>();
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void CopyToNative_ToExistingHGLOBAL_DestinationTooSmallForNullTerminator_ThrowsArgumentException()
    {
        int[] data = [1, 2, 3];
        HGLOBAL global = PInvoke.GlobalAlloc(
            GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE,
            (nuint)(data.Length * sizeof(int)));

        try
        {
            FluentActions.Invoking(() => ((ReadOnlySpan<int>)data).CopyToNative(global, nullTerminate: true))
                .Should().Throw<ArgumentException>();
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }
}
