using System;
using Xunit;

namespace AlgorithmLib.Types.Test
{
    public class ArrayAndIteratorTest
    {
        [Fact]
        public void CreateArray()
        {
            int size = 40;

            var array = new Array<string>(size);
            
            Assert.NotNull(array);
            Assert.Equal(size, array.Size);
        }
        
        
        [Fact]
        public void Insert()
        {
            var str = "test";
            var array = new Array<string>();
            var size = array.Size;
            
            array.Insert(str);
            
            Assert.Equal(str, array[array.Size - 1]);
            Assert.Equal(size + 1, array.Size);
        }
        
        
        [Fact]
        public void InsertByIndex()
        {
            var str = "test";
            var index = 3;
            var array = new Array<string>();
            var size = array.Size;
            
            array.Insert(index, str);
            
            Assert.Equal(str, array[index]);
            Assert.Equal(size +1, array.Size);
        }

        
        [Fact]
        public void Remove()
        {
            int index = 2;
            var str = "test";
            var array = new Array<string>();
            var size = array.Size;
            for (int i = 0; i < size; i++)
            {
                array[i] = str + i;
            }
            
            array.Remove(index);
            
            Assert.NotEqual(size, array.Size);
            Assert.Equal(str + (index + 1), array[index]);
            Assert.Equal(str + (index + 2), array[index+1]);
        }

        [Fact]
        public void IteratorInit()
        {
            var array = new Array<string>();

            var iterator = array.GetIterator();
            
            Assert.NotNull(iterator);
        }

        private static Array<string> InitArray()
        {
            var array = new Array<string>();
            for (int i = 0; i < array.Size; i++)
            {
                array[i] = $"test{i}";
            }
            return array;
        }
        
        [Fact]
        public void IteratorGetItem()
        {
            var array = InitArray();
            var iterator = array.GetIterator();

            iterator.Next();
            var item = iterator.Current;
            
            Assert.NotNull(item);
            Assert.NotNull(iterator);
        }
        
        [Fact]
        public void IteratorSetItem()
        {
            var str = "anime";
            var array = InitArray();
            var iterator = array.GetIterator();

            iterator.Next();
            iterator.Next();
            iterator.Set(str);

            var item = iterator.Current;
            
            
            Assert.NotNull(iterator);
            Assert.NotNull(item);
            Assert.Equal(str, item);
        }
        
        [Fact]
        public void IteratorInsertItem()
        {
            var str = "anime";
            var array = InitArray();
            var iterator = array.GetIterator();
            iterator.Next();
            
            var currnetItem = iterator.Current;
            iterator.Insert(str);
            var item = iterator.Current;
            
            
            Assert.NotNull(iterator);
            Assert.NotNull(item);
            Assert.NotNull(currnetItem);
            Assert.NotEqual(currnetItem, item);
            Assert.Equal(item, str);
        }
        
        [Fact]
        public void IteratorMoveForward()
        {
            var array = InitArray();
            var iterator = array.GetIterator();

            var condition = iterator.HasNext();
            
            Assert.NotNull(iterator);
            Assert.True(condition);
        }
        
        [Fact]
        public void IteratorMoveBackward()
        {
            var array = InitArray();
            var iterator = array.GetIterator();

            var condition = iterator.HasPrev();
            
            Assert.NotNull(iterator);
            Assert.False(condition);
        }
    }
}