#include <stdio.h>
#include <windows.h>

void *loadBinary(const char *dllFilename)
{
	return LoadLibrary(dllFilename);
}

void *getFunctionAddress(void *binaryHandle, const char *functionName)
{
	return GetProcAddress((HMODULE)binaryHandle, functionName);
}

void printFunctions(void *binaryHandle)
{
	



	printf("Not implemented\n");
}
