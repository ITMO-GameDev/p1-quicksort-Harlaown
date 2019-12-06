#include "FixedSizeAllocator.h"
#include "windows.h"

void* FixedSizeAllocator::Alloc()
{
	if (blockSize > free)
	{
		free = PageSize;
		head = GetPage();
	}
	auto tmp = head;
	head = static_cast<char*>(head) + blockSize;
	free -= blockSize;
	return tmp;
}

size_t FixedSizeAllocator::GetBlockSize() const
{
	return this->blockSize;
}

FixedSizeAllocator::~FixedSizeAllocator()
{
	for (auto i = pages.begin(); i != pages.end(); ++i)
	{
		VirtualFree(*i, 0, MEM_RELEASE);
	}
}

void* FixedSizeAllocator::GetPage()
{
	const auto page = VirtualAlloc(nullptr, PageSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	pages.push_back(page);
	return page;
}
