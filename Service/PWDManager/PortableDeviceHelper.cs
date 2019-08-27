using System;
using System;
using System.Runtime.InteropServices;
using PortableDeviceApiLib;
    


namespace PhotoUploader.Service
{
    internal static class PortableDeviceConstants
    {
        // From PortableDevice.h in the windows SDK
        public const string WPD_DEVICE_OBJECT_ID = "DEVICE";

        public const int VT_LPWSTR = 31;
        public const int VT_CLSID = 72;
        public const int VT_DATE = 7;
        public const int VT_BOOL = 11;
        public const int VT_UI4 = 19;
        public const int VT_UI8 = 21;

        public const uint PORTABLE_DEVICE_DELETE_NO_RECURSION = 0;
        public const uint PORTABLE_DEVICE_DELETE_WITH_RECURSION = 1;

        public static Guid WPD_CONTENT_TYPE_FOLDER;
        public static Guid WPD_CONTENT_TYPE_FUNCTIONAL_OBJECT;
        public static Guid WPD_CONTENT_TYPE_GENERIC_FILE;
        public static Guid WPD_FUNCTIONAL_CATEGORY_STORAGE;

        static PortableDeviceConstants()
        {
            WPD_CONTENT_TYPE_FOLDER = new Guid(0x27E2E392, 0xA111, 0x48E0, 0xAB, 0x0C, 0xE1, 0x77, 0x05, 0xA0, 0x5F, 0x85);
            WPD_CONTENT_TYPE_FUNCTIONAL_OBJECT = new Guid(0x99ED0160, 0x17FF, 0x4C44, 0x9D, 0x98, 0x1D, 0x7A, 0x6F, 0x94, 0x19, 0x21);
            WPD_CONTENT_TYPE_GENERIC_FILE = new Guid(0x0085E0A6, 0x8D34, 0x45D7, 0xBC, 0x5C, 0x44, 0x7E, 0x59, 0xC7, 0x3D, 0x48);
            WPD_FUNCTIONAL_CATEGORY_STORAGE = new Guid(0x23F05BBC, 0x15DE, 0x4C2A, 0xA5, 0x5B, 0xA9, 0xAF, 0x5C, 0xE4, 0x12, 0xEF);
        }
    }


    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal struct PropVariant
    {
        [FieldOffset(0)]
        public short variantType;
        [FieldOffset(8)]
        public IntPtr pointerValue;
        [FieldOffset(8)]
        public byte byteValue;
        [FieldOffset(8)]
        public long intValue;
        [FieldOffset(8)]
        public long longValue;
        [FieldOffset(8)]
        public double dateValue;
        [FieldOffset(8)]
        public short boolValue;

        public static PropVariant FromValue(tag_inner_PROPVARIANT value)
        {
            IntPtr ptrValue = Marshal.AllocHGlobal(Marshal.SizeOf(value));
            Marshal.StructureToPtr(value, ptrValue, false);

            //
            // Marshal the pointer into our C# object
            //
            return (PropVariant)Marshal.PtrToStructure(ptrValue, typeof(PropVariant));
        }

        public string AsString()
        {
            switch (variantType)
            {
                case PortableDeviceConstants.VT_LPWSTR:
                    return Marshal.PtrToStringUni(pointerValue);

                case PortableDeviceConstants.VT_CLSID:
                    return ToGuid().ToString();

                case PortableDeviceConstants.VT_DATE:
                    return ToDate().ToString();

                case PortableDeviceConstants.VT_BOOL:
                    return ToBool().ToString();

                case PortableDeviceConstants.VT_UI4:
                    return intValue.ToString();

                case PortableDeviceConstants.VT_UI8:
                    return longValue.ToString();
            }

            return variantType.ToString();
        }

        public Guid ToGuid()
        {
            return (Guid)Marshal.PtrToStructure(pointerValue, typeof(Guid));
        }

        public DateTime ToDate()
        {
            return DateTime.FromOADate(dateValue);
        }

        public bool ToBool()
        {
            return Convert.ToBoolean(boolValue);
        }

        public static tag_inner_PROPVARIANT StringToPropVariant(string value)
        {
            // Tried using the method suggested here:
            // http://blogs.msdn.com/b/dimeby8/archive/2007/01/08/creating-wpd-propvariants-in-c-without-using-interop.aspx
            // However, the GetValue fails (Element Not Found) even though we've just added it.
            // So, I use the alternative (and I think more "correct") approach below.

            var pvSet = new PropVariant
                            {
                                variantType = PortableDeviceConstants.VT_LPWSTR, 
                                pointerValue = Marshal.StringToCoTaskMemUni(value)
                            };

            // Marshal our definition into a pointer
            var ptrValue = Marshal.AllocHGlobal(Marshal.SizeOf(pvSet));
            Marshal.StructureToPtr(pvSet, ptrValue, false);

            // Marshal pointer into the interop PROPVARIANT 
            return (tag_inner_PROPVARIANT)Marshal.PtrToStructure(ptrValue, typeof(tag_inner_PROPVARIANT));
        }
    }
}

