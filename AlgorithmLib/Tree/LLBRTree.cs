using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AlgorithmLib.Types;

namespace AlgorithmLib.Tree
{
    /// <summary>Implements a left-leaning red-black tree.</summary>
    /// <remarks>
    ///     http://www.cs.princeton.edu/~rs/talks/LLRB/RedBlack.pdf
    ///     http://www.cs.princeton.edu/~rs/talks/LLRB/08Penn.pdf
    /// </remarks>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    public class LeftLeaningRedBlackTree<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
        where TKey : IComparable where TValue : IComparable

    {
        private readonly Comparison<TKey> keyComparison;
        private RBNode rootNode;

        public LeftLeaningRedBlackTree(Comparison<TKey> keyComparison)
        {
            this.keyComparison = keyComparison ?? throw new ArgumentNullException(nameof(keyComparison));
        }

        public TKey MinimumKey
        {
            get { return GetExtreme(rootNode, n => n.RBLeft, n => n.Key); }
        }

        public TKey MaximumKey
        {
            get { return GetExtreme(rootNode, n => n.RBRight, n => n.Key); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key, item.Value);
        }

        public int Count { get; private set; }

        public bool IsReadOnly { get; } = false;

        public void Add(TKey key, TValue value)
        {
            rootNode = Add(rootNode, key, value);
            rootNode.IsBlack = true;
        }

        public bool ContainsKey(TKey key)
        {
            return Contains(key);
        }

        public bool Remove(TKey key)
        {
            return Remove(key, default);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var node = GetNodeForKey(key);
            if (node == null)
            {
                value = default;
                return false;
            }

            value = node.Value;
            return true;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            rootNode = null;
            Count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var node = GetNodeForKey(item.Key);
            return node != null && node.Value.Equals(item.Value);
        }

        //WTF ??
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public TValue this[TKey key]
        {
            get => GetValueForKey(key);
            set => Add(key, value);
        }

        public ICollection<TKey> Keys => GetKeys().ToList();


        public ICollection<TValue> Values => GetValuesForAllKeys().ToList();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        private bool Remove(TKey key, TValue value)
        {
            var initialCount = Count;

            if (rootNode == null) return initialCount != Count;

            rootNode = Remove(rootNode, key);
            if (rootNode != null) rootNode.IsBlack = true;

            return initialCount != Count;
        }

        private IEnumerable<TKey> GetKeys()
        {
            var lastKey = default(TKey);
            var lastKeyValid = false;
            return Traverse(rootNode, n => !lastKeyValid || !Equals(lastKey, n.Key), n =>
            {
                lastKey = n.Key;
                lastKeyValid = true;
                return lastKey;
            });
        }

        private bool Contains(TKey key)
        {
            var node = GetNodeForKey(key);
            return node != null;
        }

        private TValue GetValueForKey(TKey key)
        {
            var node = GetNodeForKey(key);
            if (node != null) return node.Value;

            throw new KeyNotFoundException();
        }

        private IEnumerable<TValue> GetValuesForAllKeys()
        {
            return Traverse(rootNode, n => true, n => n.Value);
        }

        private static bool IsRed(RBNode node)
        {
            if (node == null) return false;

            return !node.IsBlack;
        }

        private RBNode Add(RBNode node, TKey key, TValue value)
        {
            if (node == null)
            {
                Count++;
                return new RBNode {Key = key, Value = value};
            }

            if (IsRed(node.RBLeft) && IsRed(node.RBRight)) FlipColor(node);

            var comparisonResult = KeyAndValueComparison(key, node.Key);
            if (comparisonResult < 0)
                node.RBLeft = Add(node.RBLeft, key, value);
            else if (comparisonResult > 0)
                node.RBRight = Add(node.RBRight, key, value);
            else
                node.Value = value;

            if (IsRed(node.RBRight)) node = RotateLeft(node);

            if (IsRed(node.RBLeft) && IsRed(node.RBLeft.RBLeft)) node = RotateRight(node);
            return node;
        }

        private RBNode Remove(RBNode node, TKey key)
        {
            var comparisonResult = KeyAndValueComparison(key, node.Key);
            if (comparisonResult < 0)
            {
                if (node.RBLeft == null) return FixUp(node);
                if (!IsRed(node.RBLeft) && !IsRed(node.RBLeft.RBLeft)) node = MoveRedLeft(node);
                node.RBLeft = Remove(node.RBLeft, key);
            }
            else
            {
                if (IsRed(node.RBLeft)) node = RotateRight(node);

                if (KeyAndValueComparison(key, node.Key) == 0 && node.RBRight == null)
                {
                    Count--;
                    return node;
                }

                if (node.RBRight == null) return FixUp(node);
                if (!IsRed(node.RBRight) && !IsRed(node.RBRight.RBLeft)) node = MoveRedRight(node);

                if (KeyAndValueComparison(key, node.Key) == 0)
                {
                    Count--;
                    var m = GetExtreme(node.RBRight, n => n.RBLeft, n => n);
                    node.Key = m.Key;
                    node.Value = m.Value;
                    node.RBRight = DeleteMinimum(node.RBRight);
                }
                else
                {
                    node.RBRight = Remove(node.RBRight, key);
                }
            }

            return FixUp(node);
        }

        private static void FlipColor(RBNode node)
        {
            node.IsBlack = !node.IsBlack;
            node.RBLeft.IsBlack = !node.RBLeft.IsBlack;
            node.RBRight.IsBlack = !node.RBRight.IsBlack;
        }

        private static RBNode RotateLeft(RBNode node)
        {
            var x = node.RBRight;
            node.RBRight = x.RBLeft;
            x.Left = node;
            x.IsBlack = node.IsBlack;
            node.IsBlack = false;
            return x;
        }

        private static RBNode RotateRight(RBNode node)
        {
            var x = node.RBLeft;
            node.RBLeft = x.RBRight;
            x.Right = node;
            x.IsBlack = node.IsBlack;
            node.IsBlack = false;
            return x;
        }

        private static RBNode MoveRedLeft(RBNode node)
        {
            FlipColor(node);
            if (!IsRed(node.RBRight.RBLeft)) return node;
            node.RBRight = RotateRight(node.RBRight);
            node = RotateLeft(node);
            FlipColor(node);

            if (IsRed(node.RBRight.RBRight)) node.RBRight = RotateLeft(node.RBRight);

            return node;
        }

        private static RBNode MoveRedRight(RBNode node)
        {
            FlipColor(node);
            if (!IsRed(node.RBLeft.RBLeft)) return node;
            node = RotateRight(node);
            FlipColor(node);

            return node;
        }

        private static RBNode DeleteMinimum(RBNode node)
        {
            if (node.RBLeft == null) return null;

            if (!IsRed(node.RBLeft) && !IsRed(node.RBLeft.RBLeft)) node = MoveRedLeft(node);

            node.RBLeft = DeleteMinimum(node.RBLeft);

            return FixUp(node);
        }

        private static RBNode FixUp(RBNode node)
        {
            if (IsRed(node.RBRight)) node = RotateLeft(node);

            if (IsRed(node.RBLeft) && IsRed(node.RBLeft.RBLeft)) node = RotateRight(node);

            if (IsRed(node.RBLeft) && IsRed(node.RBRight)) FlipColor(node);

            if (null == node.RBLeft || !IsRed(node.RBLeft.RBRight) || IsRed(node.RBLeft.RBLeft)) return node;
            node.RBLeft = RotateLeft(node.RBLeft);
            if (IsRed(node.RBLeft)) node = RotateRight(node);
            return node;
        }

        private RBNode GetNodeForKey(TKey key)
        {
            var node = rootNode;
            while (node != null)
            {
                var comparisonResult = keyComparison(key, node.Key);
                if (comparisonResult < 0)
                    node = node.RBLeft;
                else if (comparisonResult > 0)
                    node = node.RBRight;
                else
                    return node;
            }

            return null;
        }

        private static T GetExtreme<T>(RBNode node, Func<RBNode, RBNode> successor, Func<RBNode, T> selector)
        {
            var extreme = default(T);
            var current = node;
            while (current != null)
            {
                extreme = selector(current);
                current = successor(current);
            }

            return extreme;
        }

        private static IEnumerable<T> Traverse<T>(RBNode node, Func<RBNode, bool> condition, Func<RBNode, T> selector)
        {
            // Create a stack to avoid recursion
            var stack = new Stack<RBNode>();
            var current = node;
            while (current != null)
                if (current.Left != null)
                {
                    // Save current state and go left
                    stack.Push(current);
                    current = current.RBLeft;
                }
                else
                {
                    do
                    {
                        if (condition(current)) yield return selector(current);
                        current = current.RBRight;
                    } while (current == null && stack.Count > 0 && (current = stack.Pop()) != null);
                }
        }

        private int KeyAndValueComparison(TKey leftKey, TKey rightKey)
        {
            var comparisonResult = keyComparison(leftKey, rightKey);
            return comparisonResult;
        }

        public TreeIterator GetIterator()
        {
            return new TreeIterator(this);
        }
        
        [DebuggerDisplay("Key={Key}, Value={Value}")]
        public class RBNode : INode<TKey, TValue>
        {
            /// <summary>Gets or sets the color of the node.</summary>
            public bool IsBlack;

            internal RBNode RBLeft
            {
                get => (RBNode) Left;
                set => Left = value;
            }

            internal RBNode RBRight
            {
                get => (RBNode) Right;
                set => Right = value;
            }
            
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public INode<TKey, TValue> Left { get; set; }
            public INode<TKey, TValue> Right { get; set; }
        }

        public class TreeIterator
        {
            private readonly LeftLeaningRedBlackTree<TKey, TValue> tree;
            private readonly Stack<RBNode> nextStack = new Stack<RBNode>();
            private readonly Stack<RBNode> prevStack = new Stack<RBNode>();
            
            public TreeIterator(LeftLeaningRedBlackTree<TKey,TValue> tree)
            {
                this.tree = tree;
                Current = tree.rootNode;
                
                Reset();
            }
            
            public RBNode Current { get; private set; }

            public TKey Key => Current != null ? Current.Key : default(TKey);
            public TValue Value => Current != null ? Current.Value : default(TValue);

            public void Set(TValue value)
            {
                if (Current != null)
                {
                    Current.Value = value;
                }
            }
            
            public void Reset()
            {
                Current = tree.rootNode;
                if (Current == null)
                {
                    return;
                }
                
                nextStack.Clear();
                nextStack.Push(null);
                
                prevStack.Clear();
                prevStack.Push(tree.rootNode);
            }
            
            public bool HasNext()
            {
                return Current != null;
            }
            
            
            
            public void Next()
            {
                if (Current == null)
                {
                    // Iterating finished already.
                    return;
                }

                if(Current != tree.rootNode)
                    prevStack.Push(Current);
                
                if (Current.Right != null)
                {
                    // Right subtree in place.
                    // Push the right node. If the left node is missed, it would be processed next.
                    // Otherwise it will be processed after left subtree.
                    nextStack.Push(Current.RBRight);
                }

                if (Current.Left != null)
                {
                    // The left node in place.
                    // It would be the next current.
                    Current = Current.RBLeft;
                }
                else
                {
                    // Go one level up if there hadn't been right subtree,
                    // otherwise the right subtree would be processed.
                    Current = nextStack.Pop();
                }
            }

            public bool HasPrev()
            {
                return Current != null;
            }
            
            public void Prev()
            {
                if (Current != tree.rootNode)
                {
                    Current = prevStack.Pop();
                }
            }

            
        }
    }
    
    
}