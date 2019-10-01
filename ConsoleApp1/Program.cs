using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using QuickSortProject;

namespace ConsoleApp1
{
    class Program
    {
        
        private static int[] Array = new  int[ sizeof(int) << 8 << 10 ];

        private static readonly int[] SmallArray = new int[10];

        private static void RandomArray<T>(T[] array, Func<T> func)
        {
            Parallel.For(0, array.Length, index => { array[index] = func(); });
        }

        static void Main(string[] args)
        {
           var random = new Random();

           Parallel.For(0, Array.Length, index => { Array[index] = random.Next(Int32.MinValue, Int32.MaxValue); });

           Parallel.For(0, SmallArray.Length, index => { SmallArray[index] = random.Next(Int32.MinValue, Int32.MaxValue); });

           Console.WriteLine(Array.Length);

           QuickSortProject.Extension.HybridSort(Array, 0, Array.Length - 1, Comparer<int>.Create((i, i1) => i.CompareTo(i1) ));

           QuickSortProject.Extension.HybridSort(SmallArray, Comparer<int>.Create((i, i1) => i.CompareTo(i1)));

           foreach (var i in SmallArray)
           {
               Console.WriteLine(i);
           }

           Vector2[] array = new Vector2[50];
           RandomArray(array, () =>
           {
               Vector2 temp = new Vector2(random.Next(), random.Next());
               return temp;
           });
           var comparer = Comparer<Vector2>.Create(((self, other) =>
           {
               if (self.X > other.X) return 1;
               if (self.X < other.X) return -1;
               if (self.Y > other.Y) return 1;
               if (self.Y < other.Y) return -1;
               return 0;
           }));

           array.HybridSort(comparer);

           foreach (var vector2 in array)
           {
               Console.WriteLine($"X {vector2.X} : Y {vector2.Y}");
           }
        }
    }
}