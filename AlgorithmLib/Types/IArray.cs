using System;
using System.Collections.Generic;

namespace AlgorithmLib.Types
{
    internal interface IArray<T> : IEnumerable<T>, IDisposable
    {

        void Insert( T value);

        void Insert(int position,  T value);

        void Remove(int position);
        
        T this[int index] { get; set; }
        
        int Size { get;  }

        
    }
}