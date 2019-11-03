using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AlgorithmLib;
using AlgorithmLib.Tree;
using AlgorithmLib.Types;

namespace AlgorithmLib.Console
{
    static class Program
    {
        private static void Main(string[] args)
        {
            var dict = new Dictionary<string, int>();
            
            
            var tree = new LeftLeaningRedBlackTree<string, int>((s, s1) => string.Compare(s, s1, StringComparison.Ordinal));

            for (int i = 0; i < 1000; i++)
            {
                tree.Add($"test{i}", i);
            }

            System.Console.WriteLine(tree[@"test15"]);
        }
    }
}