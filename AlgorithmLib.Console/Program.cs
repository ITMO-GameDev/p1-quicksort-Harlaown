using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AlgorithmLib;
using AlgorithmLib.Sort;
using AlgorithmLib.Types;

namespace AlgorithmLib.Console
{
    class Program
    {
        

        static void Main(string[] args)
        {

            var array = new Array<int>(10);

            for (int i = 0; i < 15; i++)
            {
                array.Insert(i);
            }

            for (int i = 0; i < array.Size; i++)
            {
                array[i] = i + 1;
            }
            
            foreach (var element in array)
            {
                System.Console.WriteLine(element);
            }

            System.Console.Clear();
            
            array.Insert(5, 88);
            
            array.Remove(1);
            
            foreach (var element in array)
            {
                System.Console.WriteLine(element);
            }
        }
    }
}