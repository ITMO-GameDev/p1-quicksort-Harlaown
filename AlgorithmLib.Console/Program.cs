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
                NewMethod(ref alloc);
            }

            System.Console.Clear();

            System.Console.WriteLine(aligh( 8));
            System.Console.WriteLine(aligh( 11));

            alloc.Dispose();
            
        }

        private static int aligh(int size)
        {
            size--;
            var d = 512;
            for (; (d & size) == 0; d >>= 1) ;
            return (int) (d << 1);
        }

        private static void NewMethod(ref FixedSizeAllocator alloc)
        {
            var ptr = alloc.Alloc();

            var span = new Span<int>(ptr.ToPointer(), (int) (alloc.BlockSize / sizeof(int)));

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = rnd.Next(Int32.MinValue, Int32.MaxValue);
            }
            

            alloc.PrintCurrentPage();
        }
        public static Random rnd = new Random();
    }
}