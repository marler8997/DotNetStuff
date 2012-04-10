#include <stdio.h>

#include "parameters.h"


void __stdcall execute(void *functionAddress, struct Parameter *parameters)
{
	//
	// __stdcall means
	// 1. The callee (the function) cleans up the stack.
	// 2. Function Parameters are pushed onto the stack from right-to-left.
	// 3. EAX, ECX and EDX are designated for use within the function.
	// 4. Return value is stored in EAX
	//
	// Therefore, the first item on the stack will be the parameters variable, and the
	// second item will be the functionAddress variable.
	// 
	__asm {
		call functionAddress
	}

}