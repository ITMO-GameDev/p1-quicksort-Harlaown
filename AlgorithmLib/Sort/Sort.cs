using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AlgorithmLib
{
    public static class Extension
    {
        public static void HybridSort<T>(this T[] array, IComparer<T> comparer)
        {
            HybridSort(array, 0, array.Length - 1, comparer);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSorted<T>(ref T[] arr, IComparer<T> comparer)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                if (comparer.Compare(arr[i - 1], arr[i]) > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static void HybridSort<T>(this T[] array, int index, int length, IComparer<T> comparer)
        {
            if (array == null) return;
            if (index < 0) return;
            if (length < 0) return;
            if (array.Length - index < length) return;
            if (length <= 1 || (comparer == null))
                return;
            if(IsSorted(ref array, comparer)) return;
            


            var span = new Span<T>(array);

            Sort(ref span, index, length, comparer.Compare);
        }

        private static void DownHeap<T>(ref Span<T> keys, int i, int n, int lo, Comparison<T> comparer)
        {
            var key = keys[lo + i - 1];
            int num;
            for (; i <= n / 2; i = num)
            {
                num = 2 * i;
                if (num < n && comparer(keys[lo + num - 1], keys[lo + num]) < 0)
                    ++num;
                if (comparer(key, keys[lo + num - 1]) < 0)
                    keys[lo + i - 1] = keys[lo + num - 1];
                else
                    break;
            }

            keys[lo + i - 1] = key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FloorLog2PlusOne(int n)
        {
            var num = 0;
            for (; n >= 1; n /= 2)
                ++num;
            return num;
        }

        //
        //https://ru.wikipedia.org/wiki/%D0%9F%D0%B8%D1%80%D0%B0%D0%BC%D0%B8%D0%B4%D0%B0%D0%BB%D1%8C%D0%BD%D0%B0%D1%8F_%D1%81%D0%BE%D1%80%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%BA%D0%B0
        //
        private static void HeapSort<T>(ref Span<T> keys, int lo, int hi, Comparison<T> comparer)
        {
            var n = hi - lo + 1;
            for (var i = n / 2; i >= 1; --i)
                DownHeap(ref keys, i, n, lo, comparer);
            for (var index = n; index > 1; --index)
            {
                Swap(ref keys, lo, lo + index - 1);
                DownHeap(ref keys, 1, index - 1, lo, comparer);
            }
        }

        //Кнутовская сортировка вставками
        //https://ru.wikipedia.org/wiki/%D0%A1%D0%BE%D1%80%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%BA%D0%B0_%D0%B2%D1%81%D1%82%D0%B0%D0%B2%D0%BA%D0%B0%D0%BC%D0%B8
        //
        private static void InsertionSort<T>(ref Span<T> keys, int lo, int hi, Comparison<T> comparer)
        {
            for (var i = lo; i < hi; ++i)
            {
                var index = i;
                T key;
                for (key = keys[i + 1]; index >= lo && comparer(key, keys[index]) < 0; --index)
                    keys[index + 1] = keys[index];
                keys[index + 1] = key;
            }
        }

        //
        //https://ru.wikipedia.org/wiki/Introsort
        //
        private static void IntroSort<T>(ref Span<T> keys, int lo, int hi, int depthLimit, Comparison<T> comparer)
        {
            int num1;
            for (; hi > lo; hi = num1 - 1)
            {
                var num2 = hi - lo + 1;
                if (num2 <= 16) //если элементов меньше 16 используем сортировку вставками
                {
                    if (num2 == 1) //если один элемент
                        break;
                    if (num2 == 2) //если два элемента
                    {
                        SwapIfGreater<T>(ref keys, comparer, lo, hi);
                        break;
                    }

                    if (num2 == 3) //если три элемента
                    {
                        SwapIfGreater<T>(ref keys, comparer, lo, hi - 1);
                        SwapIfGreater<T>(ref keys, comparer, lo, hi);
                        SwapIfGreater<T>(ref keys, comparer, hi - 1, hi);
                        break;
                    }

                    InsertionSort<T>(ref keys, lo, hi, comparer); //сортировка вставками
                    break;
                }

                if (depthLimit == 0) //если исчерпали глубину рекурсии
                {
                    HeapSort(ref keys, lo, hi, comparer); //используем пирамидальную сортировку
                    break;
                }

                // иначе используем разбиение быстрой сортировки
                --depthLimit;
                num1 = PickPivotAndPartition(ref keys, lo, hi, comparer);
                IntroSort(ref keys, num1 + 1, hi, depthLimit, comparer);
            }
        }

        private static int PickPivotAndPartition<T>(ref Span<T> keys, int lo, int hi, Comparison<T> comparer)
        {
            var index = lo + (hi - lo) / 2;
            SwapIfGreater(ref keys, comparer, lo, index);
            SwapIfGreater(ref keys, comparer, lo, hi);
            SwapIfGreater(ref keys, comparer, index, hi);
            var key = keys[index];
            Swap(ref keys, index, hi - 1);
            var i = lo;
            var j = hi - 1;
            while (i < j)
            {
                do
                    ;
                while (comparer(keys[++i], key) < 0);
                do
                    ;
                while (comparer(key, keys[--j]) < 0);
                if (i < j)
                    Swap(ref keys, i, j);
                else
                    break;
            }

            Swap(ref keys, i, hi - 1);
            return i;
        }

        private static void Sort<T>(ref Span<T> keys, int index, int length, Comparison<T> comparer)
        {
            //Глубина это логарифм от числа элементов + 1
            IntroSort(ref keys, index, length, FloorLog2PlusOne(length), comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(ref Span<T> a, int i, int j)
        {
            if (i == j)
                return;
            var obj = a[i];
            a[i] = a[j];
            a[j] = obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SwapIfGreater<T>(ref Span<T> keys, Comparison<T> comparer, int a, int b)
        {
            if (a == b || comparer(keys[a], keys[b]) <= 0)
                return;
            var key = keys[a];
            keys[a] = keys[b];
            keys[b] = key;
        }
    }
}