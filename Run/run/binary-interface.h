#ifndef DLL_INTERFACE_H
#define DLL_INTERFACE_H

#include "parameters.h"

void *loadBinary(const char *filename);
void *getFunctionAddress(void *binaryHandle, const char *functionName);
void printFunctions(void *binaryHandle);
void __stdcall execute(void *functionHandle, struct Parameter *parameterList);

#endif