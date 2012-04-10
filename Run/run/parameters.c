#include <stdio.h>

#include "parameters.h"

//
// Return Value: Number of parameters on success, Negative if an error occurred
//
int ParseParameters(int argc, const char *argv[], const struct Parameter **parameters)
{
	int i;

	if(argc < 0) return -1;
	if(argc == 0) {
		*parameters = NULL;
		return 0;
	}
	
	*parameters = (struct Parameter*)malloc(sizeof(struct Parameter) * argc);
	
	for(i = 0; i < argc; i++)
	{
		
	}	
	
	return 0;
}