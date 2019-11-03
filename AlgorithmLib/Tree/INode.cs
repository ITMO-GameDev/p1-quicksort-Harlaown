namespace AlgorithmLib.Tree
{
    public interface INode<TKey, TValue>
    {
        /// <summary>Gets or sets the node's key.</summary>
        TKey Key { get; set; }

        /// <summary>Gets or sets the node's value.</summary>
        TValue Value { get; set; }

        /// <summary>Gets or sets the left node.</summary>
        INode<TKey, TValue> Left { get; set; }

        /// <summary>Gets or sets the right node.</summary>
        INode<TKey, TValue> Right { get; set; }
    }
}