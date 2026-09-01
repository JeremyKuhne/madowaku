// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Com.StructuredStorage;

namespace Windows.Win32.System.Variant;

public unsafe partial struct VARIANT
{
    // Add other data types from PROPVARIANT

#pragma warning disable TOUKI0041 // Naming rule violation
    /// <summary>
    ///  Contains the overlapping fields of a <see cref="VARIANT"/>.
    /// </summary>
    public partial struct _Anonymous_e__Union
    {
        /// <summary>
        ///  Contains the value fields of a <see cref="VARIANT"/>.
        /// </summary>
        public partial struct _Anonymous_e__Struct
        {
            /// <summary>
            ///  Contains the overlapping value storage of a <see cref="VARIANT"/>.
            /// </summary>
            public partial struct _Anonymous_e__Union
            {
#pragma warning restore TOUKI0041 // Naming rule violation
                /// <summary>
                ///  A pointer to a <see cref="Guid"/> value.
                /// </summary>
                [FieldOffset(0)]
                public Guid* puuid;

                /// <summary>
                ///  A file time value.
                /// </summary>
                [FieldOffset(0)]
                public FILETIME filetime;

                /// <summary>
                ///  A counted array value.
                /// </summary>
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