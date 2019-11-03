using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using AlgorithmLib.Sort;

namespace AlgorithmLib.Types
{
    public class Array<T> : IArray<T>
    {

        private Memory<T> array;


        private static readonly double Goldenratio = (1 + Math.Sqrt(5)) / 2;

        public Array() : this(16)
        {
            
        }

        public int Size { get; private set; } = 0;

        public Array(int size)
        {
            Size = size;
            array = new T[size];
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return (IsDisposable ? null : new ArrayIterator<T>(this)) ?? throw new ObjectDisposedException("Object has removed");
        }

        public IArrayIterator<T> GetIterator()
        {
            return (IsDisposable ? null : new ArrayIterator<T>(this)) ?? throw new ObjectDisposedException("Object has removed");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool IsDisposable { get; set; }
        
        
        public void Dispose()
        {
            IsDisposable = true;
            array = null;
            Size = 0;
        }
 
        
        public void Insert(T value)
        {
            if(IsDisposable) return;

            ResizeIfBiggestOrEqual();
            
            InsertAndIncrement( value);
        }
      
        private void InsertAndIncrement(T value)
        {
            NativeInsert(value, Size );
            ++Size;
        }

        
        private void InsertAndIncrement( T value, int pos)
        {
            var length = Size - pos;
            var slice = array.Span.Slice(pos, length);
            var destinationSpanWithOffset = array.Span.Slice(pos + 1, length);
            slice.CopyTo(destinationSpanWithOffset);
            NativeInsert(value, pos);
            
            
//            var item = span[0];
//            for (int i = 0; i < span.Length - 1; i++)
//            {
//                var nextItem = span[i + 1];
//                span[i + 1] = item;
//                item = nextItem;
//            }
//            NativeInsert(value, pos);

            ++Size;
        }
        
        private void NativeInsert(T value, int pos)
        {
            array.Span[pos] = value;
        }

        private void Resize()
        {
            var newArray = new Memory<T>(new T[(int) (array.Length * Goldenratio)]);
                
            array.CopyTo(newArray);
                
            array = newArray;
        }

        private void ResizeIfBiggestOrEqual()
        {
            if (Size >= Capacity)
            {
                Resize();
            }
        }
     
        public void Insert(int position,  T value)
        {
            if(IsDisposable) return;

            if(position >= Size)
                 return;
            
            
            ResizeIfBiggestOrEqual();

            InsertAndIncrement(value, position);
        }

        public void Remove(int position)
        {
            if(IsDisposable) return;
          
            if(position >= Size) return;

            if (position == Size - 1)
            {
                Size--;
                return;
            }

            var length = Size - position;
            
            var span = array.Span.Slice(position, length);

            var offsetSpan = array.Span.Slice(position + 1, length -  1);
            
            offsetSpan.CopyTo(span);

            array.Span[Size - 1] = default(T);
            
            --Size;
        }

        public T this[int index]
        {
            get
            {
                if (index >= Size)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return array.Span[index];
            }
            set
            {
                if (index >= Size)
                {
                    throw new ArgumentOutOfRangeException();
                }
                
                NativeInsert( value, index);
            }
        }
        
        protected int Capacity => array.Length;
    }
}