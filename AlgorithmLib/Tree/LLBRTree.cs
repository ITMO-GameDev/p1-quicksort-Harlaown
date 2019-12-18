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
        where TKey : IComparable

    {
        private readonly Comparison<TKey> keyComparison;
        private Node rootNode;

        public LeftLeaningRedBlackTree(Comparison<TKey> keyComparison)
        {
            this.keyComparison = keyComparison ?? throw new ArgumentNullException(nameof(keyComparison));
        }

        public TKey MinimumKey
        {
            get { return GetExtreme(rootNode, n => n.Left, n => n.Key); }
        }

        public TKey MaximumKey
        {
            get { return GetExtreme(rootNode, n => n.Right, n => n.Key); }
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

        private static bool IsRed(Node node)
        {
            if (node == null) return false;

            return !node.IsBlack;
        }

        private Node Add(Node node, TKey key, TValue value)
        {
            if (node == null)
            {
                Count++;
                return new Node {Key = key, Value = value};
            }

            if (IsRed(node.Left) && IsRed(node.Right)) FlipColor(node);

            var comparisonResult = KeyAndValueComparison(key, node.Key);
            if (comparisonResult < 0)
                node.Left = Add(node.Left, key, value);
            else if (comparisonResult > 0)
                node.Right = Add(node.Right, key, value);
            else
                node.Value = value;

            if (IsRed(node.Right)) node = RotateLeft(node);

            if (IsRed(node.Left) && IsRed(node.Left.Left)) node = RotateRight(node);
            return node;
        }

        private Node Remove(Node node, TKey key)
        {
            var comparisonResult = KeyAndValueComparison(key, node.Key);
            if (comparisonResult < 0)
            {
                if (node.Left == null) return FixUp(node);
                if (!IsRed(node.Left) && !IsRed(node.Left.Left)) node = MoveRedLeft(node);
                node.Left = Remove(node.Left, key);
            }
            else
            {
                if (IsRed(node.Left)) node = RotateRight(node);

                if (KeyAndValueComparison(key, node.Key) == 0 && node.Right == null)
                {
                    Count--;
                    return node;
                }

                if (node.Right == null) return FixUp(node);
                if (!IsRed(node.Right) && !IsRed(node.Right.Left)) node = MoveRedRight(node);

                if (KeyAndValueComparison(key, node.Key) == 0)
                {
                    Count--;
                    var m = GetExtreme(node.Right, n => n.Left, n => n);
                    node.Key = m.Key;
                    node.Value = m.Value;
                    node.Right = DeleteMinimum(node.Right);
                }
                else
                {
                    node.Right = Remove(node.Right, key);
                }
            }

            return FixUp(node);
        }

        private static void FlipColor(Node node)
        {
            node.IsBlack = !node.IsBlack;
            node.Left.IsBlack = !node.Left.IsBlack;
            node.Right.IsBlack = !node.Right.IsBlack;
        }

        private static Node RotateLeft(Node node)
        {
            var x = node.Right;
            node.Right = x.Left;
            x.Left = node;
            x.IsBlack = node.IsBlack;
            node.IsBlack = false;
            return x;
        }

        private static Node RotateRight(Node node)
        {
            var x = node.Left;
            node.Left = x.Right;
            x.Right = node;
            x.IsBlack = node.IsBlack;
            node.IsBlack = false;
            return x;
        }

        private static Node MoveRedLeft(Node node)
        {
            FlipColor(node);
            if (!IsRed(node.Right.Left)) return node;
            node.Right = RotateRight(node.Right);
            node = RotateLeft(node);
            FlipColor(node);

            if (IsRed(node.Right.Right)) node.Right = RotateLeft(node.Right);

            return node;
        }

        private static Node MoveRedRight(Node node)
        {
            FlipColor(node);
            if (!IsRed(node.Left.Left)) return node;
            node = RotateRight(node);
            FlipColor(node);

            return node;
        }

        private static Node DeleteMinimum(Node node)
        {
            if (node.Left == null) return null;

            if (!IsRed(node.Left) && !IsRed(node.Left.Left)) node = MoveRedLeft(node);

            node.Left = DeleteMinimum(node.Left);

            return FixUp(node);
        }

        private static Node FixUp(Node node)
        {
            if (IsRed(node.Right)) node = RotateLeft(node);

            if (IsRed(node.Left) && IsRed(node.Left.Left)) node = RotateRight(node);

            if (IsRed(node.Left) && IsRed(node.Right)) FlipColor(node);

            if (null == node.Left || !IsRed(node.Left.Right) || IsRed(node.Left.Left)) return node;
            node.Left = RotateLeft(node.Left);
            if (IsRed(node.Left)) node = RotateRight(node);
            return node;
        }

        private Node GetNodeForKey(TKey key)
        {
            var node = rootNode;
            while (node != null)
            {
                var comparisonResult = keyComparison(key, node.Key);
                if (comparisonResult < 0)
                    node = node.Left;
                else if (comparisonResult > 0)
                    node = node.Right;
                else
                    return node;
            }

            return null;
        }

        private static T GetExtreme<T>(Node node, Func<Node, Node> successor, Func<Node, T> selector)
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

        private static IEnumerable<T> Traverse<T>(Node node, Func<Node, bool> condition, Func<Node, T> selector)
        {
            // Create a stack to avoid recursion
            var stack = new Stack<Node>();
            var current = node;
            while (current != null)
                if (current.Left != null)
                {
                    // Save current state and go left
                    stack.Push(current);
                    current = current.Left;
                }
                else
                {
                    do
                    {
                        if (condition(current)) yield return selector(current);
                        current = current.Right;
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
        public class Node
        {
            /// <summary>Gets or sets the color of the node.</summary>
            public bool IsBlack;
            
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }
        }

        public class TreeIterator
        {
            private readonly LeftLeaningRedBlackTree<TKey, TValue> tree;
			
            private readonly Stack<Node> nextStack = new Stack<Node>();
            private readonly Stack<Node> prevStack = new Stack<Node>();
            
            public TreeIterator(LeftLeaningRedBlackTree<TKey,TValue> tree)
            {
                this.tree = tree;
                Current = tree.rootNode;
                
                Reset();
            }
            
            public Node Current { get; private set; }

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
                    nextStack.Push(Current.Right);
                }

                if (Current.Left != null)
                {
                    // The left node in place.
                    // It would be the next current.
                    Current = Current.Left;
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
                return Current != tree.rootNode;
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