using AlgorithmLib;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AlgorithmLib.Sort;
using Xunit;

namespace AlgorithmLib.Test
{
    public class SortTest
    {
        private const int SmallSize = 10;
        private const int MediumSize = 150;
        private const int BigSize = 1500;
        private const int VeryBigSize = sizeof(Int32) << 8 << 10;

        private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);

        private static void RandomArray(int[] array)
        {
            Parallel.For(0, array.Length, index => { array[index] = Rnd.Next(); });
        }

        private static void RandomArray<T>(IList<T> array, Func<T> func)
        {
            Parallel.For(0, array.Count, index => { array[index] = func(); });
        }

        private static void Shuffle<T>(ref Random rng, IList<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        private static bool IsSorted(int[] arr)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i - 1] > arr[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsSorted<T>(IReadOnlyList<T> arr, IComparer<T> comparer)
        {
            for (int i = 1; i < arr.Count; i++)
            {
                if (comparer.Compare(arr[i - 1], arr[i]) > 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static readonly Comparer<int> Comparer = Comparer<int>.Create((i, i1) => i.CompareTo(i1));

        [Fact]
        public void SmallArraySort()
        {
            int[] SmallArray = new int[SmallSize];
            RandomArray(SmallArray);

            SmallArray.HybridSort(Comparer<int>.Default);

            var condition = IsSorted(SmallArray);

            Assert.True(condition);
        }

        [Fact]
        public void MediumArraySort()
        {
            int[] MediumArray = new int[MediumSize];
            RandomArray(MediumArray);

            MediumArray.HybridSort(Comparer);

            var condition = IsSorted(MediumArray);

            Assert.True(condition);
        }

        [Fact]
        public void BigArraySort()
        {
            int[] BigArray = new int[BigSize];
            RandomArray(BigArray);

            BigArray.HybridSort(Comparer);

            var condition = IsSorted(BigArray);

            Assert.True(condition);
        }

        [Fact]
        public void VeryBigArraySort()
        {
            int[] VeryBigArray = new int[VeryBigSize];
            RandomArray(VeryBigArray);

            VeryBigArray.HybridSort(Comparer);

            var condition = IsSorted(VeryBigArray);

            Assert.True(condition);
        }

        [Fact]
        public void SmallSortedSortArray()
        {
            int[] SmallArray = new int[SmallSize];
            RandomArray(SmallArray);
            Array.Sort(SmallArray);

            SmallArray.HybridSort(Comparer);

            var condition = IsSorted(SmallArray);

            Assert.True(condition);
        }

        [Fact]
        public void MediumSortedSortArray()
        {
            int[] array = new int[MediumSize];
            RandomArray(array);
            Array.Sort(array);

            array.HybridSort(Comparer);

            var condition = IsSorted(array);

            Assert.True(condition);
        }

        [Fact]
        public void BigSortedSortArray()
        {
            int[] array = new int[BigSize];
            RandomArray(array);
            Array.Sort(array);

            array.HybridSort(Comparer);

            var condition = IsSorted(array);

            Assert.True(condition);
        }

        [Fact]
        public void VeryBigSortedSortArray()
        {
            int[] array = new int[VeryBigSize];
            RandomArray(array);
            Array.Sort(array);

            array.HybridSort(Comparer);

            var condition = IsSorted(array);

            Assert.True(condition);
        }

        [Fact]
        public void SmallCustomArraySort()
        {
            Vector2[] array = new Vector2[SmallSize];
            RandomArray(array, () =>
            {
                Vector2 temp = new Vector2(Rnd.Next(), Rnd.Next());
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

            var condition = IsSorted(array, comparer);

            Assert.True(condition);
        }

        [Fact]
        public void MediumCustomArraySort()
        {
            Vector2[] array = new Vector2[MediumSize];
            RandomArray(array, () =>
            {
                Vector2 temp = new Vector2(Rnd.Next(), Rnd.Next());
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

            var condition = IsSorted(array, comparer);

            Assert.True(condition);
        }

        [Fact]
        public void BigCustomArraySort()
        {
            Vector2[] array = new Vector2[BigSize];
            RandomArray(array, () =>
            {
                Vector2 temp = new Vector2(Rnd.Next(), Rnd.Next());
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

            var condition = IsSorted(array, comparer);

            Assert.True(condition);
        }

        [Fact]
        public void VeryBigCustomArraySort()
        {
            Vector2[] array = new Vector2[VeryBigSize];
            RandomArray(array, () =>
            {
                Vector2 temp = new Vector2(Rnd.Next(), Rnd.Next());
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

            var condition = IsSorted(array, comparer);

            Assert.True(condition);
        }

        [Fact]
        public void SmallReverseArraySort()
        {
            var array = new int[SmallSize];
            RandomArray(array);
            Array.Sort(array);
            Array.Reverse(array);

            array.HybridSort(Comparer);
            var condition = IsSorted(array);

            Assert.True(condition);
        }

        [Fact]
        public void MediumReverseArraySort()
        {
            var array = new int[MediumSize];
            RandomArray(array);
            Array.Sort(array);
            Array.Reverse(array);

            array.HybridSort(Comparer);
            var condition = IsSorted(array);

            Assert.True(condition);
        }

        [Fact]
        public void BigReverseArraySort()
        {
            var array = new int[BigSize];
            RandomArray(array);
            Array.Sort(array);
            Array.Reverse(array);

            array.HybridSort(Comparer);
            var condition = IsSorted(array);

            Assert.True(condition);
        }

        [Fact]
        public void VeryBigReverseArraySort()
        {
            var array = new int[VeryBigSize];
            RandomArray(array);
            Array.Sort(array);
            Array.Reverse(array);

            array.HybridSort(Comparer);
            var condition = IsSorted(array);

            Assert.True(condition);
        }
    }
}