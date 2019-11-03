using System.Collections;

namespace AlgorithmLib.Types
{
    internal class ArrayIterator<T> : IArrayIterator<T>
    {
        private Array<T> array;

        private int position = -1;
        
        
        internal ArrayIterator(Array<T> array)
        {
            this.array = array;
        }
        
        public void Dispose()
        {
            array = null;
        }

        public bool MoveNext()
        {
            if (position + 1 >= array.Size) return false;
            position++;
            return true;
        }

        public bool HasNext()
        {
            return position + 1 < array.Size;
        }

        public bool HasPrev()
        {
            return position >= 0;
        }
        
        public void Next()
        {
            position++;
        }

        public void Prev()
        {
            position--;
        }
        
        public void Reset()
        {
            position = -1;
        }

        public T Current => array[position];

        object IEnumerator.Current => Current;

        public void Set(T value)
        {
            array[position] = value;
        }

        public void Insert(T value)
        {
            array.Insert(position,value);
        }

        public void Remove()
        {
            array.Remove(position);
        }

        public void ToIndex(int index)
        {
            position = index;
        }
        
        
    }
}