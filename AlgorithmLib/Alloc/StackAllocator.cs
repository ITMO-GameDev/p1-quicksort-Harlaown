using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AlgorithmLib.Tree;

namespace AlgorithmLib.Alloc
{
    public unsafe class StackAllocator : IDisposable
    {
        private const uint PageSize = 4096;
        private IntPtr head = IntPtr.Zero;
        private uint free = 0;
        private int currentPage = -1;
        public uint BlockSize { get; private set; }

        private readonly List<IntPtr> pages = new List<IntPtr>();

        public StackAllocator(uint blockSize)
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
            var page = WinApi.VirtualAlloc(IntPtr.Zero, PageSize,  WinApi.AllocationType.Commit | WinApi.AllocationType.Reserve, WinApi.MemoryProtection.Readwrite);
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

        ~StackAllocator()
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

            foreach (var ptr in pages) WinApi.VirtualFree(ptr, 0, WinApi.FreeType.MemRelease);
            head = IntPtr.Zero;
            free = 0;
            isDispose = true;
        }

    }
}