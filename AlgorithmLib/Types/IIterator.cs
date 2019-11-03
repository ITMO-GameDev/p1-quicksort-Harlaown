using System.Collections.Generic;

namespace AlgorithmLib.Types
{
    public interface IIterator<T> : IEnumerator<T>
    {
        bool HasNext();

        bool HasPrev();

        void Next();

        void Prev();

        void Set(T value);

        void Insert(T value);

        void Remove();
    }
}