using System;
using System.Collections.Generic;

namespace AlgorithmLib.Alloc
{
    public unsafe class Allocator : IDisposable
    {
        private readonly Dictionary<int, StackAllocator> allocators = new Dictionary<int, StackAllocator>();

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
                var alocator = new StackAllocator(start);
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
                return WinApi.VirtualAlloc(IntPtr.Zero, (uint)size,
                    WinApi.AllocationType.Commit | WinApi.AllocationType.Reserve, WinApi.MemoryProtection.Readwrite);
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