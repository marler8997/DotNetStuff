#include <stdio.h>

#include "binary-interface.h"
#include "parameters.h"

void printUsage()
{
	printf("Usage:\n");
	printf("   run.exe binary-file function arguments");
}


int main(int argc, const char *argv[]) {
	int result;
	const char *binaryFilename;
	const char *functionName = "main";
	void *binaryHandle,*functionHandle;
	struct Parameter *parameters;
	
	//
	// Get Command Line Arguments
	//	
	if(argc < 3)
	{
		printf("Need a binary file and a function name.\n");
		return -1;
	}	
	binaryFilename = argv[1];
	
	if(argc >= 3)
	{
		functionName = argv[2];
	}
	
	result = ParseParameters(argc-3,(argv+3), &parameters);
	if(result < 0)
	{
		printf("Could not parase function parameters, error code %d\n", result);
		return -1;
	}
	
	//
	// Print Command Line Options
	//
	printf("Binary File    : %s\n", binaryFilename);
	printf("Function Name: : %s\n", functionName);	
	
	//
	// Execute
	//	
	
	// Load Binary
	binaryHandle = loadBinary(binaryFilename);
	if(binaryHandle == NULL)
	{
		printf("Error: loadBinary(\"%s\") returned NULL\n", binaryFilename);
		return -1;
	}
	// Get Function Address
	functionHandle = getFunctionAddress(binaryHandle, functionName);
	if(functionHandle == NULL)
	{
		printf("Error: getFunctionAddress(%d, \"%s\") returned NULL\n", binaryHandle, functionName);
		return -1;	
	}
	execute(functionHandle, parameters);

	return 0;
}