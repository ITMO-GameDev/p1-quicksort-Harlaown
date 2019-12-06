#include <cassert>   /* assert		*/
#include <limits>  /* limits_max */
#include "FreeListAllocator.h"
#include "Utils.h"
#include <windows.h>


#ifdef _DEBUG
#include <iostream>
#endif

FreeListAllocator::FreeListAllocator(const std::size_t totalSize, const PlacementPolicy pPolicy)
	: Allocator(totalSize)
{
	m_pPolicy = pPolicy;
}

void FreeListAllocator::Init()
{
	if (m_start_ptr != nullptr)
	{
		free(m_start_ptr);
		m_start_ptr = nullptr;
	}
	m_start_ptr = VirtualAlloc(nullptr, this->m_totalSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

	this->Reset();
}

FreeListAllocator::~FreeListAllocator()
{
	VirtualFree(m_start_ptr, 0, MEM_RELEASE);
	m_start_ptr = nullptr;
}

void* FreeListAllocator::Allocate(const std::size_t size, const std::size_t alignment)
{
	const std::size_t allocationHeaderSize = sizeof(AllocationHeader);
	const std::size_t freeHeaderSize = sizeof(FreeHeader);
	assert("Allocation size must be bigger" && size >= sizeof(Node));
	assert("Alignment must be 8 at least" && alignment >= 8);

	// Search through the free list for a free block that has enough space to allocate our data
	std::size_t padding;
	Node *affectedNode,
	     *previousNode;
	this->find(size, alignment, padding, previousNode, affectedNode);
	assert(affectedNode != nullptr && "Not enough memory");


	const std::size_t alignmentPadding = padding - allocationHeaderSize;
	const std::size_t requiredSize = size + padding;

	const std::size_t rest = affectedNode->data.blockSize - requiredSize;

	if (rest > 0)
	{
		// We have to split the block into the data block and a free block of size 'rest'
		Node* newFreeNode = (Node *)((std::size_t)affectedNode + requiredSize);
		newFreeNode->data.blockSize = rest;
		m_freeList.insert(affectedNode, newFreeNode);
	}
	m_freeList.remove(previousNode, affectedNode);

	// Setup data block
	const std::size_t headerAddress = (std::size_t)affectedNode + alignmentPadding;
	const std::size_t dataAddress = headerAddress + allocationHeaderSize;
	((AllocationHeader *)headerAddress)->blockSize = requiredSize;
	((AllocationHeader *)headerAddress)->padding = alignmentPadding;

	m_used += requiredSize;
	m_peak = max(m_peak, m_used);

#ifdef _DEBUG
	std::cout << "A" << "\t@H " << (void*)headerAddress << "\tD@ " << (void*)dataAddress << "\tS " << ((AllocationHeader
			*)headerAddress)->blockSize << "\tAP " << alignmentPadding << "\tP " << padding << "\tM " << m_used <<
		"\tR " <<
		rest << std::endl;
#endif

	return (void*)dataAddress;
}

void FreeListAllocator::find(const std::size_t size, const std::size_t alignment, std::size_t& padding,
                             Node*& previousNode, Node*& foundNode)
{
	switch (m_pPolicy)
	{
	case FIND_FIRST:
		find_first(size, alignment, padding, previousNode, foundNode);
		break;
	case FIND_BEST:
		find_best(size, alignment, padding, previousNode, foundNode);
		break;
	}
}

void FreeListAllocator::find_first(const std::size_t size, const std::size_t alignment, std::size_t& padding,
                                  Node*& previousNode, Node*& foundNode)
{
	//Iterate list and return the first free block with a size >= than given size
	Node *it = m_freeList.head,
	     *itPrev = nullptr;

	while (it != nullptr)
	{
		padding = Utils::CalculatePaddingWithHeader((std::size_t)it, alignment, sizeof(AllocationHeader));
		const std::size_t requiredSpace = size + padding;
		if (it->data.blockSize >= requiredSpace)
		{
			break;
		}
		itPrev = it;
		it = it->next;
	}
	previousNode = itPrev;
	foundNode = it;
}

void FreeListAllocator::find_best(const std::size_t size, const std::size_t alignment, std::size_t& padding,
                                 Node*& previousNode, Node*& foundNode)
{
	// Iterate WHOLE list keeping a pointer to the best fit
	//auto smallestDiff = std::numeric_limits<std::size_t>::max();

	auto smallestDiff = 18446744073709551615;
	Node* bestBlock = nullptr;
	Node *it = m_freeList.head,
	     *itPrev = nullptr;
	while (it != nullptr)
	{
		padding = Utils::CalculatePaddingWithHeader((std::size_t)it, alignment, sizeof(AllocationHeader));
		const std::size_t requiredSpace = size + padding;
		if (it->data.blockSize >= requiredSpace && (it->data.blockSize - requiredSpace < smallestDiff))
		{
			bestBlock = it;
		}
		itPrev = it;
		it = it->next;
	}
	previousNode = itPrev;
	foundNode = bestBlock;
}

void FreeListAllocator::Free(void* ptr)
{
	// Insert it in a sorted position by the address number
	const std::size_t currentAddress = (std::size_t)ptr;
	const std::size_t headerAddress = currentAddress - sizeof(AllocationHeader);
	const AllocationHeader* allocationHeader{(AllocationHeader *)headerAddress};

	Node* freeNode = (Node *)(headerAddress);
	freeNode->data.blockSize = allocationHeader->blockSize + allocationHeader->padding;
	freeNode->next = nullptr;

	Node* it = m_freeList.head;
	Node* itPrev = nullptr;
	while (it != nullptr)
	{
		if (ptr < it)
		{
			m_freeList.insert(itPrev, freeNode);
			break;
		}
		itPrev = it;
		it = it->next;
	}

	m_used -= freeNode->data.blockSize;

	// Merge contiguous nodes
	coalescence(itPrev, freeNode);

#ifdef _DEBUG
	std::cout << "F" << "\t@ptr " << ptr << "\tH@ " << static_cast<void*>(freeNode) << "\tS " << freeNode
	                                                                                             ->data.blockSize <<
		"\tM " << m_used << std::endl;
#endif
}

void FreeListAllocator::coalescence(Node* previousNode, Node* freeNode)
{
	if (freeNode->next != nullptr &&
		(std::size_t)freeNode + freeNode->data.blockSize == (std::size_t)freeNode->next)
	{
		freeNode->data.blockSize += freeNode->next->data.blockSize;
		m_freeList.remove(freeNode, freeNode->next);
#ifdef _DEBUG
		std::cout << "\tMerging(n) " << static_cast<void*>(freeNode) << " & " << static_cast<void*>(freeNode->next) <<
			"\tS " << freeNode->data.blockSize << std::endl;
#endif
	}

	if (previousNode != nullptr &&
		(std::size_t)previousNode + previousNode->data.blockSize == (std::size_t)freeNode)
	{
		previousNode->data.blockSize += freeNode->data.blockSize;
		m_freeList.remove(previousNode, freeNode);
#ifdef _DEBUG
		std::cout << "\tMerging(p) " << static_cast<void*>(previousNode) << " & " << static_cast<void*>(freeNode) <<
			"\tS " << previousNode->data.blockSize << std::endl;
#endif
	}
}

void FreeListAllocator::Reset()
{
	m_used = 0;
	m_peak = 0;
	Node* firstNode = static_cast<Node *>(m_start_ptr);
	firstNode->data.blockSize = m_totalSize;
	firstNode->next = nullptr;
	m_freeList.head = nullptr;
	m_freeList.insert(nullptr, firstNode);
}
