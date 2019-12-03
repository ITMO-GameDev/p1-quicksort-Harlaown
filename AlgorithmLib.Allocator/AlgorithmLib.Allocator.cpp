// AlgorithmLib.Allocator.cpp : Этот файл содержит функцию "main". Здесь начинается и заканчивается выполнение программы.
//

#include <iostream>
#include "windows.h"
#include "FixedSizeAllocator.h"
using std::cout;




int main()
{	
	void* p = nullptr;
	p = VirtualAlloc(nullptr, 4096, MEM_RESERVE, PAGE_READWRITE);
	if (p != nullptr)
	{
		cout << "Alloc" << std::endl;

		if (VirtualFree(p, 0, MEM_RELEASE))
		{
			cout << "Ok Free";
		}
		else
		{
			cout << "Not Free" << std::endl;
		}
	}

	auto allocator = new FixedSizeAllocator(sizeof(int));

	auto first = allocator->Alloc();
	auto second = allocator->Alloc();

	*(int*)first = 15;

	*(int*)second = 18;

	auto third = allocator->Alloc();

	*(int*)third = 48;

	delete allocator;
}



	
