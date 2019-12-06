using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Node = AlgorithmLib.Alloc.SinglyLinkedList.Node;
using size_t = System.Int64;
namespace AlgorithmLib.Alloc
{

    internal unsafe class SinglyLinkedList
    {
        internal unsafe struct Node
        {
            public FreeHeader Data;
            public Node* Next;
        }

        internal Node* Head;

        public SinglyLinkedList() { }

        public void Insert(Node* previousNode, Node* newNode)
        {
            if (previousNode == null)
            {
                // Is the first node
                if (Head != null)
                {
                    // The list has more elements
                    newNode->Next = Head;
                }
                else
                {
                    newNode->Next = null;
                }

                Head = newNode;
            }
            else
            {
                if (previousNode->Next == null)
                {
                    // Is the last node
                    previousNode->Next = newNode;
                    newNode->Next = null;
                }
                else
                {
                    // Is a middle node
                    newNode->Next = previousNode->Next;
                    previousNode->Next = newNode;
                }
            }
        }

        public void Remove(Node* previousNode, Node* deleteNode)
        {
            if (previousNode == null)
            {
                // Is the first node
                if (deleteNode->Next == null)
                {
                    // List only has one element
                    Head = null;
                }
                else
                {
                    // List has more elements
                    Head = deleteNode->Next;
                }
            }
            else
            {
                previousNode->Next = deleteNode->Next;
            }
        }
    };

    internal struct FreeHeader
    {
        public size_t BlockSize;
    };

    internal struct AllocationHeader
    {
        public size_t BlockSize;
        public char Padding;
    };

    public unsafe class FreeListAllocator
    {
        public FreeListAllocator(size_t size, FindPlacementPolicy policy)
        {
            this.policy = policy;
            totalSize = size;
            used = 0;
        }

        private void Coalescence(Node* previousNode, Node* freeNode)
        {
            if (freeNode->Next != null &&
                (size_t) freeNode + freeNode->Data.BlockSize == (size_t) freeNode->Next)
            {
                freeNode->Data.BlockSize += freeNode->Next->Data.BlockSize;
                freeList.Remove(freeNode, freeNode->Next);
            }

            if (previousNode != null &&
                (size_t) previousNode + previousNode->Data.BlockSize == (size_t) freeNode)
            {
                previousNode->Data.BlockSize += freeNode->Data.BlockSize;
                freeList.Remove(previousNode, freeNode);
            }
        }

        private void Find(int size, int alignment, ref size_t padding,ref  Node* previousNode,ref Node* foundNode)
        {
            switch (policy)
            {
                case FindPlacementPolicy.First:
                    FindFirst(size, alignment, ref padding,ref  previousNode,ref  foundNode);
                    break;
                case FindPlacementPolicy.Best:
                    FindBest(size, alignment, ref padding,ref  previousNode,ref  foundNode);
                    break;
            }
        }

        private static size_t CalculatePadding(size_t baseAddress, int alignment)
        {
            var multiplier = ((baseAddress) / alignment) + 1;
            var alignedAddress = multiplier * alignment;
            var padding = alignedAddress - baseAddress;
            return padding;
        }

        private static size_t CalculatePaddingWithHeader(size_t baseAddress, int alignment, int headerSize)
        {
            size_t padding = CalculatePadding(baseAddress, alignment);
            size_t neededSpace = headerSize;

            if (padding < neededSpace)
            {
                // Header does not fit - Calculate next aligned address that header fits
                neededSpace -= padding;

                // How many alignments I need to fit the header        
                if (neededSpace % alignment > 0)
                {
                    padding += alignment * (1 + (neededSpace / alignment));
                }
                else
                {
                    padding += alignment * (neededSpace / alignment);
                }
            }

            return padding;
        }

        private void FindBest(int size, int alignment, ref size_t padding,ref  Node* previousNode,
             ref Node* foundNode)
        {
            // Iterate WHOLE list keeping a pointer to the best fit
            size_t smallestDiff = size_t.MaxValue;
            Node* bestBlock = null;
            Node* it = freeList.Head;
            Node* itPrev = null;
            while (it != null)
            {
                padding = CalculatePaddingWithHeader((size_t)it, alignment, sizeof(AllocationHeader));
                size_t requiredSpace = size + padding;
                if (it->Data.BlockSize >= requiredSpace && (it->Data.BlockSize - requiredSpace < smallestDiff))
                {
                    bestBlock = it;
                }
                itPrev = it;
                it = it->Next;
            }
            previousNode = itPrev;
            foundNode = bestBlock;
        }

        private void FindFirst(int size, int alignment, ref size_t padding,ref Node* previousNode,
             ref Node* foundNode)
        {
            //Iterate list and return the first free block with a size >= than given size
            Node* it = freeList.Head;
            Node* itPrev = null;

            while (it != null)
            {
                padding = CalculatePaddingWithHeader((size_t)it, alignment, sizeof(AllocationHeader));
                size_t requiredSpace = size + padding;
                if (it->Data.BlockSize >= requiredSpace)
                {
                    break;
                }
                itPrev = it;
                it = it->Next;
            }
            previousNode = itPrev;
            foundNode = it;
        }

        public IntPtr Allocate(int size, int alignment = 0)
        {
             size_t allocationHeaderSize = sizeof(AllocationHeader);
             size_t freeHeaderSize = sizeof(FreeHeader);
            

            // Search through the free list for a free block that has enough space to allocate our data
            size_t padding = 0;
            Node* affectedNode = null;
            Node* previousNode = null;
            Find(size, alignment, ref padding, ref previousNode,ref affectedNode);
            


             size_t alignmentPadding = padding - allocationHeaderSize;
             size_t requiredSize = size + padding;

             size_t rest = affectedNode->Data.BlockSize - requiredSize;

            if (rest > 0)
            {
                // We have to split the block into the data block and a free block of size 'rest'
                var newFreeNode = (Node*)((size_t)affectedNode + requiredSize);
                newFreeNode->Data.BlockSize = rest;
                freeList.Insert(affectedNode, newFreeNode);
            }
            freeList.Remove(previousNode, affectedNode);

            // Setup data block
             size_t headerAddress = (size_t)affectedNode + alignmentPadding;
             size_t dataAddress = headerAddress + allocationHeaderSize;
            ((AllocationHeader*)headerAddress)->BlockSize = requiredSize;
            ((AllocationHeader*)headerAddress)->Padding = (char)alignmentPadding;

            used += requiredSize;
            peak = Math.Max(peak, used);


            return new IntPtr(dataAddress);
        }

        public void Free(IntPtr ptr)
        {
            // Insert it in a sorted position by the address number
             size_t currentAddress = (size_t)ptr;
             size_t headerAddress = currentAddress - sizeof(AllocationHeader);
             AllocationHeader* allocationHeader =  (AllocationHeader*)headerAddress;

            Node* freeNode = (Node*)(headerAddress);
            freeNode->Data.BlockSize = allocationHeader->BlockSize + allocationHeader->Padding;
            freeNode->Next = null;

            Node* it = freeList.Head;
            Node* itPrev = null;
            while (it != null)
            {
                if (ptr.ToPointer() < it)
                {
                    freeList.Insert(itPrev, freeNode);
                    break;
                }
                itPrev = it;
                it = it->Next;
            }

            used -= freeNode->Data.BlockSize;

            // Merge contiguous nodes
            Coalescence(itPrev, freeNode);

        }

        public void Init()
        {
            if (startPtr != IntPtr.Zero)
            {
                startPtr = IntPtr.Zero;
            }

            startPtr = WinApi.VirtualAlloc(IntPtr.Zero, (uint)totalSize,
                WinApi.AllocationType.Commit | WinApi.AllocationType.Reserve, WinApi.MemoryProtection.Readwrite);

            Reset();
        }


        void Reset()
        {
            used = 0;
            peak = 0;
            Node* firstNode = (Node*)startPtr;
            firstNode->Data.BlockSize = totalSize;
            firstNode->Next = null;
            freeList.Head = null;
            freeList.Insert(null, firstNode);
        }

        public enum FindPlacementPolicy
        {
            First,
            Best
        }


        readonly SinglyLinkedList freeList = new SinglyLinkedList();

        private IntPtr startPtr = IntPtr.Zero;

        private readonly FindPlacementPolicy policy;

        private readonly size_t totalSize;
        private size_t used;
        private size_t peak;
    }
}