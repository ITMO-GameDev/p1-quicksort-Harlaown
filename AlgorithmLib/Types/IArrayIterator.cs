namespace AlgorithmLib.Types
{
    public interface IArrayIterator<T> : IIterator<T>
    {
        void ToIndex(int index);
    }
}