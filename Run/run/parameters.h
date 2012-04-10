#ifndef PARAMETER_H
#define PARAMETER_H

struct Parameter
{
	unsigned char byteLength;
	void *addressOfValue;
};

int ParseParameters(int argc, const char *argv[], const struct Parameter **parameters);

#endif