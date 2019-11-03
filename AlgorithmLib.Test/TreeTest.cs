using System;
using AlgorithmLib.Tree;
using Xunit;

namespace AlgorithmLib.Test
{
    public class TreeTest
    {
        private static LeftLeaningRedBlackTree<string, int> CreateTree()
        {
            return  new LeftLeaningRedBlackTree<string, int>((s, s1) =>
                string.Compare(s, s1, StringComparison.Ordinal));
        }

        public static LeftLeaningRedBlackTree<string, int> CreateInitTree()
        {
            var tree = CreateTree();
            const int Size = 5;
            for (int i = 0; i < Size; i++)
            {
                tree.Add($"{i}", i);
            }

            return tree;
        }
        
        [Fact]
        public void Create()
        {
            var tree = CreateTree();
            
            Assert.NotNull(tree);
        }

        [Fact]
        public void Add()
        {
            var tree = CreateTree();
            const int value = 123;
            const string key = "123";
            
            tree.Add(key, value);

            var condition = tree[key];
            
            Assert.Equal(value,condition);
            Assert.True(tree.ContainsKey(key));
        }

        [Fact]
        public void ChangeValue()
        {
            var tree = CreateTree();
            const int value = 123;
            const string key = "123";
            const int newValue = 1488;
            
            tree.Add(key, value);
            tree.Add(key, newValue);
            var condition = tree[key];
            
            
            Assert.True(tree.ContainsKey(key));
            Assert.NotEqual(condition,value);
            Assert.Equal(condition,newValue);
        }

        [Fact]
        public void CheckCount()
        {
            var tree = CreateTree();
            const int Size = 15000;
            for (int i = 0; i < Size; i++)
            {
                tree.Add($"test{i}", i);
            }

            var conditon = tree.Count;

            Assert.Equal(Size, conditon);
        }

        [Fact]
        public void ItreatorCreate()
        {
            var tree = CreateTree();

            var itr = tree.GetIterator();
            
            Assert.NotNull(itr);
        }

        [Fact]
        public void IteratorNext()
        {
            var tree = CreateInitTree();
            var iterator = tree.GetIterator();
            var prevValue = iterator.Current;

            iterator.Next();
            var currentValue = iterator.Current;
            
            Assert.NotEqual(prevValue.Value, currentValue.Value);
            Assert.Equal(0, currentValue.Value);
        }

        [Fact]
        public void IteratorPrev()
        {
            var tree = CreateInitTree();
            var iterator = tree.GetIterator();
            iterator.Next();
            var nextValue = iterator.Current.Value;
            
            iterator.Prev();
            var prevValue = iterator.Current.Value;
            
            Assert.NotEqual(nextValue,prevValue);
            Assert.Equal(1, prevValue);

        }
    }
}