using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AlgorithmLib.Alloc
{
    internal static unsafe class WinAPI
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

    public unsafe class FixedSizeAllocator : IDisposable
    {
        private const uint PageSize = 4096;
        private IntPtr head = IntPtr.Zero;
        private uint free = 0;
        private int currentPage = -1;
        public uint BlockSize { get; private set; }

        private readonly List<IntPtr> pages = new List<IntPtr>();

        public FixedSizeAllocator(uint blockSize)
        {
            BlockSize = blockSize;
        }

        public bool IsInited { get; private set; } = false;

        public void Init()
        {
            for (int i = 0; i < 10; i++)
            {
                CreatePage();
            }

            currentPage = 0;
            head = pages[currentPage];
            free = PageSize;
            IsInited = true;
        }

        public IntPtr Alloc()
        {
            if(!IsInited) throw new ArgumentException();
            if (BlockSize > free)
            {
                head = GetPage();
                free = PageSize;
            }

            var tmp = head;
            head = IntPtr.Add(head, (int)BlockSize);
            free -= BlockSize;
            return tmp;
        }

        private IntPtr CreatePage()
        {
            var page = WinAPI.VirtualAlloc(IntPtr.Zero, PageSize,  WinAPI.AllocationType.Commit | WinAPI.AllocationType.Reserve, WinAPI.MemoryProtection.Readwrite);
            pages.Add(page);
            return page;
        }

        private static readonly double Goldenratio = (1 + Math.Sqrt(5)) / 2;

        private IntPtr GetPage()
        {
            if ( (currentPage+1) == pages.Count)
            {
                for (int i = 0; i < pages.Count * Goldenratio; i++)
                {
                    CreatePage();
                }
            }

            ++currentPage;
            return pages[currentPage];
        }

        ~FixedSizeAllocator()
        {
            Dispose(isDispose);
        }

#if DEBUG

        public void PrintCurrentPage()
        {
            var span = new Span<byte>(pages[currentPage].ToPointer(), (int)PageSize);
            
            Console.WriteLine("Current Page");
            for (int i = 0; i < span.Length / BlockSize; i++)
            {
                for (int j = 0; j < BlockSize; j++)
                {
                    int index = i * (int)BlockSize + j;
                    Console.Write($@"{span[index]} ");
                }

                Console.WriteLine();
            }
        }

        public void WriteCurrentPage()
        {
            var span = new Span<byte>(pages[currentPage].ToPointer(), (int)PageSize);

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = Byte.MaxValue;
            }
        }

#endif


        public void Destroy()
        {
            Dispose(isDispose);
        }

        private bool isDispose = false;

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) return;

            foreach (var ptr in pages) WinAPI.VirtualFree(ptr, 0, WinAPI.FreeType.MemRelease);
            head = IntPtr.Zero;
            free = 0;
            isDispose = true;
        }

    }

    public unsafe class Allocator : IDisposable
    {
        private readonly Dictionary<int, FixedSizeAllocator> allocators = new Dictionary<int, FixedSizeAllocator>();

        public Allocator()
        {
            
        }

        private static int Align(int size)
        {
            size--;
            var d = 512;
            for (; (d & size) == 0; d >>= 1);
            return (int)(d << 1);
        }

        public bool IsInited { get; private set; }
        public void Init()
        {
            uint start = 8;

            for (; start <= 512; start <<= 2)
            {
                var alocator = new FixedSizeAllocator(start);
                alocator.Init();

                allocators.Add((int)start, alocator);
            }

            IsInited = true;
        }

        public IntPtr Alloc(int size)
        {
            var alignSize = Align(size);

            if (alignSize > 512)
            {
                return WinAPI.VirtualAlloc(IntPtr.Zero, (uint)size,
                    WinAPI.AllocationType.Commit | WinAPI.AllocationType.Reserve, WinAPI.MemoryProtection.Readwrite);
            }

            var allocator = allocators[alignSize];
            return allocator.Alloc();
        }

        public void Destroy()
        {
            if(isDispose) return;

            foreach (var fixedSizeAllocator in allocators.Values)
            {
                fixedSizeAllocator.Destroy();
            }

            isDispose = true;
        }

        private bool isDispose = false;

        public void Dispose()
        {
            Destroy();
        }
    }
}