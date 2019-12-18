using System;
using System.Runtime.InteropServices;

namespace AlgorithmLib.Alloc
{
    internal static unsafe class WinApi
    {
        [Flags]
        public enum AllocationType : uint
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Reset = 0x80000,
            LargePages = 0x20000000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000
        }
        [Flags]
        public enum MemoryProtection : uint
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadwrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            Readonly = 0x02,
            Readwrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NocacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }
        [Flags]
        public enum FreeType : uint
        {
            MemDecommit = 0x4000,
            MemRelease = 0x8000,
        }

        [DllImport("kernel32.dll")]
        internal static extern bool VirtualFree(IntPtr lpAddress, UInt32 dwSize, FreeType dwFreeType);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(IntPtr lpAddress, UInt32 dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualProtect(IntPtr lpAddress, int dwSize, MemoryProtection flProtect, out MemoryProtection flOldProtect);
    }
}