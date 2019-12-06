// AlgorithmLib.Allocator.cpp : Этот файл содержит функцию "main". Здесь начинается и заканчивается выполнение программы.
//

#include <iostream>
#include "windows.h"
#include "FixedSizeAllocator.h"
#include "FreeListAllocator.h"
using std::cout;


int main()
{	
	auto allocator = new FixedSizeAllocator(sizeof(int));

	auto first = allocator->Alloc();
	auto second = allocator->Alloc();

	*static_cast<int*>(first) = 15;

	*static_cast<int*>(second) = 18;

	auto third = allocator->Alloc();

	*static_cast<int*>(third) = 48;

	delete allocator;


	auto listAllocator = new FreeListAllocator(4096 * 40, FreeListAllocator::FIND_FIRST);

	listAllocator->Init();
	
	auto ptr = listAllocator->Allocate(512, 8);

	listAllocator->Free(ptr);

	delete listAllocator;
	
}
