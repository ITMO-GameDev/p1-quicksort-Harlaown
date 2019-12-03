#pragma once

#include <vector>

inline size_t align(size_t x, size_t a) { return ((x - 1) | (a - 1)) + 1; }

class FixedSizeAllocator
{
public:
	FixedSizeAllocator(const size_t block_size) : head(nullptr), free(0), blockSize(block_size)
	{
	}

	void* Alloc();
	size_t GetBlockSize() const;
	~FixedSizeAllocator();

private:
	void* GetPage();

	const static size_t PageSize = 4096;
	
	std::vector<void*> pages;
	void* head;
	size_t free;
	size_t blockSize;
};

