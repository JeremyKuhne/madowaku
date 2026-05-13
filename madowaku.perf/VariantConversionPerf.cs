// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Variant;

namespace madowaku.perf;

/// <summary>
///  Benchmarks core <see cref="VARIANT"/> conversion paths covered by Variant tests.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess, warmupCount: 1, iterationCount: 3, launchCount: 1)]
public class VariantConversionPerf
{
    private VARIANT _intVariant;
    private object _boxedInt = null!;

    /// <summary>
    ///  Initializes benchmark inputs used across all conversion benchmarks.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _intVariant = (VARIANT)42;
        _boxedInt = 42;
    }

    /// <summary>
    ///  Reads an int payload directly through the explicit VARIANT cast operator.
    /// </summary>
    [Benchmark]
    public int ExplicitIntCast()
    {
        return (int)_intVariant;
    }

    /// <summary>
    ///  Converts through <see cref="VARIANT.ToObject"/> and unboxes to <see cref="int"/>.
    /// </summary>
    [Benchmark]
    public int ToObjectThenUnbox()
    {
        object value = _intVariant.ToObject()!;
        return (int)value;
    }

    /// <summary>
    ///  Converts a boxed int through <see cref="VARIANT.FromObject"/> and returns the variant type.
    /// </summary>
    [Benchmark]
    public VARENUM FromObjectInt()
    {
        VARIANT value = VARIANT.FromObject(_boxedInt);
        VARENUM type = value.vt;
        return type;
    }

    /// <summary>
    ///  Converts an int through the explicit variant conversion operator and returns the variant type.
    /// </summary>
    [Benchmark]
    public VARENUM ExplicitVariantInt()
    {
        VARIANT value = (VARIANT)42;
        VARENUM type = value.vt;
        return type;
    }
}
