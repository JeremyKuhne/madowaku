// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public unsafe partial struct IDispatch
{
    /// <summary>
    ///  Get the dispatch IDs of the specified names.
    /// </summary>
    public int[] GetIdsOfNames(params string[] names)
    {
        ArgumentNull.ThrowIfNull(names);

        if (names.Length == 0)
        {
            return [];
        }

        using StringParameterArray namesArg = new(names);
        int[] ids = new int[names.Length];
        fixed (int* i = ids)
        {
            HRESULT hr = GetIDsOfNames(IID.Empty(), (PWSTR*)(char**)namesArg, (uint)names.Length, lcid: 0, i);
            if (hr.Failed && hr != HRESULT.DISP_E_UNKNOWNNAME)
            {
                hr.ThrowOnFailure();
            }
        }

        return ids;
    }

    /// <summary>
    ///  Get the dispatch ID of the specified name.
    /// </summary>
    public int GetIdOfName(string name)
    {
        ArgumentNull.ThrowIfNull(name);

        int id = PInvokeMadowaku.DISPID_UNKNOWN;
        fixed (char* n = name)
        {
            PWSTR* p = (PWSTR*)n;
            HRESULT hr = GetIDsOfNames(IID.Empty(), (PWSTR*)&p, 1, lcid: 0, &id);
            if (hr.Failed && hr != HRESULT.DISP_E_UNKNOWNNAME)
            {
                hr.ThrowOnFailure();
            }
        }

        return id;
    }

    /// <summary>
    ///  Get the value of the specified property.
    /// </summary>
    public VARIANT GetPropertyValue(string name)
    {
        int dispid = GetIdOfName(name);
        if (dispid == PInvokeMadowaku.DISPID_UNKNOWN)
        {
            return default;
        }

        Guid guid = Guid.Empty;
        EXCEPINFO pExcepInfo = default;
        DISPPARAMS dispParams = default;
        VARIANT value = default;

        HRESULT hr = Invoke(
            dispid,
            &guid,
            PInvokeMadowaku.GetThreadLocale(),
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &dispParams,
            &value,
            &pExcepInfo,
            null);

        Debug.Assert(hr.Succeeded);

        return value;
    }

    /// <summary>
    ///  Get the value of the specified property by dispatch ID.
    /// </summary>
    public VARIANT GetPropertyValue(int dispatchId)
    {
        Guid guid = Guid.Empty;
        EXCEPINFO pExcepInfo = default;
        DISPPARAMS dispParams = default;
        VARIANT value = default;

        Invoke(
            dispatchId,
            &guid,
            PInvokeMadowaku.GetThreadLocale(),
            DISPATCH_FLAGS.DISPATCH_PROPERTYGET,
            &dispParams,
            &value,
            &pExcepInfo,
            null);

        return value;
    }

    /// <summary>
    ///  Sets the value of the specified property by dispatch ID.
    /// </summary>
    public HRESULT SetPropertyValue(int dispatchId, VARIANT value)
    {
        Guid guid = Guid.Empty;
        EXCEPINFO pExcepInfo = default;
        VARIANT* arg = &value;
        int putDispatchID = PInvokeMadowaku.DISPID_PROPERTYPUT;

        DISPPARAMS dispParams = new()
        {
            cArgs = 1,
            cNamedArgs = 1,
            // You HAVE to name the put argument or you'll get DISP_E_PARAMNOTFOUND
            rgdispidNamedArgs = &putDispatchID,
            rgvarg = arg
        };

        uint argumentError;

        HRESULT hr = Invoke(
            dispatchId,
            &guid,
            PInvokeMadowaku.GetThreadLocale(),
            DISPATCH_FLAGS.DISPATCH_PROPERTYPUT,
            &dispParams,
            null,
            &pExcepInfo,
            &argumentError);

        return hr;
    }
}
