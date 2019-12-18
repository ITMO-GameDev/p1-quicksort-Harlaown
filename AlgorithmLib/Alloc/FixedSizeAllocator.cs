using System;
using System.Collections.Generic;
using System.Text;
using size_t = System.Int64;


namespace AlgorithmLib.Alloc
{

    public class FixedSizeAllocator : IDisposable
    {
        private const size_t PageSize = 4096;
        private readonly size_t chunkSize = 0;
        private bool isDispose = false;
        private readonly StackLinkedList freeList = new StackLinkedList();
        private readonly List<IntPtr> pages = new List<IntPtr>();

        public FixedSizeAllocator(size_t chunkSize)
        {
            this.chunkSize = chunkSize;
            IsInited = false;
        }

        public bool IsInited { get; private set; }
        public IntPtr Alloc()
        {
            if (!IsInited) throw new ArgumentException();
            if (freeList.IsHeadNull())
            {
                GetPage();
            }
            return freeList.Pop();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Free(IntPtr ptr)
        {
            if (!IsInited) throw new ArgumentException();

            freeList.Push(ref ptr);
        }

        public void Init()
        {
            if(IsInited) return;

            GetPage();
            IsInited = true;
        }

        private void Dispose(bool disposing)
        {
            if (disposing) return;

            foreach (var ptr in pages) WinApi.VirtualFree(ptr, 0, WinApi.FreeType.MemRelease);
            pages.Clear();
            IsInited = false;
            isDispose = true;
        }

        private void GetPage()
        {
            var page = WinApi.VirtualAlloc(IntPtr.Zero, (int)PageSize, WinApi.AllocationType.Commit | WinApi.AllocationType.Reserve, WinApi.MemoryProtection.Readwrite);
            pages.Add(page);
            SlicePage(page);
        }

        private void SlicePage(IntPtr page)
        {
            var chunks = PageSize / chunkSize;

            for (int i = 0; i < chunks; ++i)
            {
                var address = IntPtr.Add(page, (int)(i * chunkSize));
                freeList.Push(ref address);
            }
        }
    }

   

    internal unsafe class StackLinkedList
    {
        unsafe ref struct Node
        {
            public Node* Next;
        };

        public bool IsHeadNull()
        {
            return head == null;
        }

        private Node* head;

        public IntPtr Pop()
        {
            if (head == null) throw new NullReferenceException();

            ref var top = ref head;
            head = head->Next;
            var ptr = new IntPtr(top);
            return ptr;

        }

        private void Push(ref Node* newNode)
        {
            newNode->Next = head;
            head = newNode;
        }

        public void Push(ref IntPtr newNode)
        {
            var node = (Node*)newNode;
            Push(ref node);
        }
        
    };
}
