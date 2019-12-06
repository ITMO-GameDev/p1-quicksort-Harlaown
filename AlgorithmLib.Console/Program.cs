using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AlgorithmLib;
using AlgorithmLib.Alloc;
using AlgorithmLib.Tree;
using AlgorithmLib.Types;

namespace AlgorithmLib.Console
{
    static unsafe class Program
    {
        private static void Main(string[] args)
        {
            var alloc = new FixedSizeAllocator(512);
            
            alloc.Init();
            
            
            for (int i = 0; i < 4096/512; i++)
            {
                var ptr = alloc.Alloc();
                NewMethod(ref ptr, (int) alloc.BlockSize);
                alloc.PrintCurrentPage();
            }

            System.Console.Clear();

            System.Console.WriteLine(aligh( 8));
            System.Console.WriteLine(aligh( 11));

            alloc.Dispose();
            

            var listal = new FreeListAllocator(6144, FreeListAllocator.FindPlacementPolicy.First);

            listal.Init();

            var ptr1 = listal.Allocate(512, 8);
            NewMethod(ref ptr1, 512);
            listal.Free(ptr1);
            
        }

        private static int aligh(int size)
        {
            size--;
            var d = 512;
            for (; (d & size) == 0; d >>= 1) ;
            return (int) (d << 1);
        }

        private static void NewMethod(ref IntPtr alloc, int size)
        {
            var span = new Span<int>(alloc.ToPointer(), (int)size / sizeof(int));

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = rnd.Next(Int32.MinValue, Int32.MaxValue);
            }
        }
        public static Random rnd = new Random();
    }
}