// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

[TestClass]
public class SafeArrayScopeTests
{
    [TestMethod]
    public void Constructor_IntSize_CreatesSafeArray()
    {
        using SafeArrayScope<int> array = new(3);
        unsafe { (array.Value is null).Should().BeFalse(); }
    }

    [TestMethod]
    public void Indexer_Int_RoundTripsValues()
    {
        using SafeArrayScope<int> array = new(3);
        array[0] = 10;
        array[1] = 20;
        array[2] = 30;
        array[0].Should().Be(10);
        array[1].Should().Be(20);
        array[2].Should().Be(30);
    }

    [TestMethod]
    public void Constructor_FromIntArray_PopulatesValues()
    {
        using SafeArrayScope<int> array = new([1, 2, 3, 4]);
        array[0].Should().Be(1);
        array[3].Should().Be(4);
    }

    [TestMethod]
    public void Indexer_String_RoundTripsValues()
    {
        using SafeArrayScope<string> array = new(2);
        array[0] = "alpha";
        array[1] = "beta";
        array[0].Should().Be("alpha");
        array[1].Should().Be("beta");
    }

    [TestMethod]
    public void Indexer_Double_RoundTripsValues()
    {
        using SafeArrayScope<double> array = new(2);
        array[0] = 1.5;
        array[1] = 2.5;
        array[0].Should().Be(1.5);
        array[1].Should().Be(2.5);
    }

    [TestMethod]
    public void Constructor_UnsupportedType_Throws()
    {
        FluentActions.Invoking(() => new SafeArrayScope<byte>(1)).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void Constructor_NintType_ThrowsWithComSafeArrayScopeMessage()
    {
        ArgumentException ex = FluentActions.Invoking(() => new SafeArrayScope<nint>(1))
            .Should().Throw<ArgumentException>().Which;
        ex.Message.Should().Contain("ComSafeArrayScope");
    }

    [TestMethod]
    public unsafe void Constructor_NullSafearrayPointer_ValueIsNull()
    {
        using SafeArrayScope<int> array = new((SAFEARRAY*)null);
        (array.Value is null).Should().BeTrue();
    }

    [TestMethod]
    public unsafe void Constructor_WrapVT_I4_Succeeds()
    {
        using SafeArrayScope<int> source = new(2);
        source[0] = 7;
        source[1] = 8;

        // Wrap the existing VT_I4 SAFEARRAY in a new scope — must not throw.
        SafeArrayScope<int> wrapped = new(source.Value);
        wrapped[0].Should().Be(7);
        wrapped[1].Should().Be(8);
    }

    [TestMethod]
    public unsafe void Constructor_TypeMismatch_ForInt_Throws()
    {
        // VT_BSTR SAFEARRAY wrapped as SafeArrayScope<int> must throw.
        using SafeArrayScope<string> source = new(1);
        SAFEARRAY* ptr = source.Value;
        ArgumentException ex = FluentActions.Invoking(() => new SafeArrayScope<int>(ptr))
            .Should().Throw<ArgumentException>().Which;
        ex.Message.Should().Contain("VarType=");
    }

    [TestMethod]
    public unsafe void Constructor_TypeMismatch_ForString_Throws()
    {
        using SafeArrayScope<int> source = new(1);
        SAFEARRAY* ptr = source.Value;
        FluentActions.Invoking(() => new SafeArrayScope<string>(ptr)).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public unsafe void Constructor_TypeMismatch_ForDouble_Throws()
    {
        using SafeArrayScope<int> source = new(1);
        SAFEARRAY* ptr = source.Value;
        FluentActions.Invoking(() => new SafeArrayScope<double>(ptr)).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void Length_ReturnsElementCount()
    {
        using SafeArrayScope<int> array = new(5);
        array.Length.Should().Be(5);
    }

    [TestMethod]
    public void IsEmpty_LengthZero_ReturnsTrue()
    {
        using SafeArrayScope<int> array = new(0);
        array.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public void IsEmpty_NonEmpty_ReturnsFalse()
    {
        using SafeArrayScope<int> array = new(1);
        array.IsEmpty.Should().BeFalse();
    }

    [TestMethod]
    public unsafe void IsNull_DefaultConstructed_ReturnsTrue()
    {
        using SafeArrayScope<int> array = new((SAFEARRAY*)null);
        array.IsNull.Should().BeTrue();
    }

    [TestMethod]
    public unsafe void ImplicitOperator_SafearrayStar_NonNull()
    {
        using SafeArrayScope<int> array = new(2);
        SAFEARRAY* ptr = array;
        (ptr is null).Should().BeFalse();
    }

    [TestMethod]
    public void ImplicitOperator_Nint_NonZero()
    {
        using SafeArrayScope<int> array = new(2);
        nint value = array;
        value.Should().NotBe(0);
    }

    [TestMethod]
    public unsafe void ExplicitOperator_Variant_HasArrayFlag()
    {
        using SafeArrayScope<int> array = new(2);
        VARIANT v = (VARIANT)array;
        (v.vt & VARENUM.VT_ARRAY).Should().Be(VARENUM.VT_ARRAY);
        (v.vt & VARENUM.VT_TYPEMASK).Should().Be(VARENUM.VT_I4);
    }

    [TestMethod]
    public unsafe void Constructor_ObjectType_VariantSafearray_Roundtrips()
    {
        using SafeArrayScope<object> array = new(2);
        array[0] = 42;
        array[1] = "hi";
        array[0].Should().Be(42);
        array[1].Should().Be("hi");
    }

    [TestMethod]
    public unsafe void ImplicitOperator_VoidDoublePointer_DereferencesToSafearrayPointer()
    {
        using SafeArrayScope<int> array = new(2);
        void** pp = array;
        (pp is null).Should().BeFalse();
        (*pp == array.Value).Should().BeTrue();
    }

    [TestMethod]
    public unsafe void ImplicitOperator_SafearrayDoublePointer_DereferencesToSafearrayPointer()
    {
        using SafeArrayScope<int> array = new(2);
        SAFEARRAY** pp = array;
        (pp is null).Should().BeFalse();
        (*pp == array.Value).Should().BeTrue();
    }
}
