#include <stdio.h>

__declspec(dllexport) void __stdcall NoArgumentsFunction()
{
	printf("Succesfully Called 'NoArgumentsFunction'\n");
}

__declspec(dllexport) void __stdcall OneArgumentFunction(unsigned char c)
{
	printf("Succesfully Called 'OneArgumentFunction' with '%c' (%d)\n", c, c);
}

__declspec(dllexport) void __stdcall TwoArgumentsFunction(unsigned char c, unsigned char b)
{
	printf("Succesfully Called 'TwoArgumentsFunction' with '%c' (%d) and '%c' (%d)\n", c, c, b, b);
}