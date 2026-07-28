// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Com.StructuredStorage;

namespace Windows.Win32.System.Variant;

public unsafe partial struct VARIANT
{
    // Add other data types from PROPVARIANT

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable TOUKI0041 // Naming rule violation
    public partial struct _Anonymous_e__Union
    {
        public partial struct _Anonymous_e__Struct
        {
            public partial struct _Anonymous_e__Union
            {
#pragma warning restore TOUKI0041 // Naming rule violation
#pragma warning restore CS1591
                /// <inheritdoc cref="PROPVARIANT._Anonymous_e__Union._Anonymous_e__Struct._Anonymous_e__Union.puuid"/>
                [FieldOffset(0)]
                public Guid* puuid;

                /// <inheritdoc cref="PROPVARIANT._Anonymous_e__Union._Anonymous_e__Struct._Anonymous_e__Union.filetime"/>
                [FieldOffset(0)]
                public FILETIME filetime;

                /// <inheritdoc cref="PROPVARIANT._Anonymous_e__Union._Anonymous_e__Struct._Anonymous_e__Union.cabool"/>
                /// <remarks>
                ///  <para>
                ///   Any of the CA* types are valid
                ///  </para>
                /// </remarks>
                [FieldOffset(0)]
                public CAUB ca;
            }
        }
    }
}