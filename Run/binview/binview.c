//
// Documentation:
// http://webster.cs.ucr.edu/Page_TechDocs/pe.txt
//


#include <stdio.h>
#include <windows.h>


static int freadError(size_t expected, size_t actual);

static int freadError(size_t expected, size_t actual)
{
	printf("ERROR: expected %d bytes, but fread returned %d bytea\n", expected, actual);
	return -1;
}

int main(int argc, char *argv[])
{
	IMAGE_DOS_HEADER dosHeader;
	IMAGE_FILE_HEADER fileHeader;
	IMAGE_OPTIONAL_HEADER optionalHeader;
	char *binaryFilename;
	FILE *fp;
	size_t size;
	unsigned int offset;
	DWORD ntSignature;
	
	//
	// Command Line Options
	//
	if(argc < 2)
	{
		printf("Supply a binary file\n");
		return -1;
	}
	
	binaryFilename = argv[1];
	
	//
	// Print Options
	//
	printf("Binary Filename : %s\n", binaryFilename);
	
	
	//
	// Open File
	//
	fp = fopen(binaryFilename, "rb");	
	
	//
	// The DOS Header
	//	
	
	// Read Dos Header
	size = fread((void*)&dosHeader, 1, sizeof(IMAGE_DOS_HEADER), fp);
	if(size != sizeof(IMAGE_DOS_HEADER)) {
		return freadError(sizeof(IMAGE_DOS_HEADER), size);
	}
	
	// Print Dos Header
	printf("\nDOS Header:\n");
	printf("0000: MZ Header signature               WORD e_magic     = 0x%04x\n", dosHeader.e_magic);
	printf("0002: Bytes on last page of file        WORD e_cblp      = 0x%04x\n", dosHeader.e_cblp);
	printf("0004: Pages in file                     WORD e_cp        = 0x%04x\n", dosHeader.e_cp);
	printf("0006: Relocations                       WORD e_crlc      = 0x%04x\n", dosHeader.e_crlc);
	printf("0008: Size of header in paragraphs      WORD e_cparhdr   = 0x%04x\n", dosHeader.e_cparhdr);
	printf("000a: Minimum extra paragraphs needed   WORD e_minalloc  = 0x%04x\n", dosHeader.e_minalloc);
	printf("000c: Maximum extra paragraphs needed   WORD e_maxalloc  = 0x%04x\n", dosHeader.e_maxalloc);
	printf("000e: Initial (relative) SS value       WORD e_ss        = 0x%04x\n", dosHeader.e_ss);
	printf("0010: Initial SP value                  WORD e_sp        = 0x%04x\n", dosHeader.e_sp);
	printf("0012: Checksum                          WORD e_csum      = 0x%04x\n", dosHeader.e_csum);
	printf("0014: Initial IP value                  WORD e_ip        = 0x%04x\n", dosHeader.e_ip);
	printf("0016: Initial (relative) CS value       WORD e_cs        = 0x%04x\n", dosHeader.e_cs);
	printf("0018: File address of relocation table  WORD e_lfarlc    = 0x%04x\n", dosHeader.e_lfarlc);
	printf("001a: Overlay number                    WORD e_ovno      = 0x%04x\n", dosHeader.e_ovno);
	printf("001c: Reserved words                    WORD e_res[4]    = 0x%04x\n", dosHeader.e_res[0]);
	printf("001e:                                                    = 0x%04x\n", dosHeader.e_res[1]);
	printf("0020:                                                    = 0x%04x\n", dosHeader.e_res[2]);
	printf("0022:                                                    = 0x%04x\n", dosHeader.e_res[3]);
	printf("0024: OEM identifier (for e_oeminfo)    WORD e_oemid     = 0x%04x\n", dosHeader.e_oemid);
	printf("0026: OEM information; e_oemid specific WORD e_oeminfo   = 0x%04x\n", dosHeader.e_oeminfo);
	printf("0028: Reserved words                    WORD e_res2[10]  = 0x%04x\n", dosHeader.e_res2[0]);
	printf("002a:                                                    = 0x%04x\n", dosHeader.e_res2[1]);
	printf("002c:                                                    = 0x%04x\n", dosHeader.e_res2[2]);
	printf("002e:                                                    = 0x%04x\n", dosHeader.e_res2[3]);
	printf("0030:                                                    = 0x%04x\n", dosHeader.e_res2[4]);
	printf("0032:                                                    = 0x%04x\n", dosHeader.e_res2[5]);
	printf("0034:                                                    = 0x%04x\n", dosHeader.e_res2[6]);
	printf("0036:                                                    = 0x%04x\n", dosHeader.e_res2[7]);
	printf("0038:                                                    = 0x%04x\n", dosHeader.e_res2[8]);
	printf("003a:                                                    = 0x%04x\n", dosHeader.e_res2[9]);	
	
	// Check Dos Header
	if(dosHeader.e_magic != IMAGE_DOS_SIGNATURE) {
		printf("e_magic should be 0x%04x, but it was 0x%04x\n", IMAGE_DOS_SIGNATURE, dosHeader.e_magic);
		return -1;
	}
	
	// Find the NT_Signature
	offset = 0x3c;
	ntSignature = dosHeader.e_lfanew;	
	while(ntSignature != IMAGE_NT_SIGNATURE) {	
		printf("%04x:                                                    = 0x%02x\n", offset, (unsigned char)ntSignature);
		ntSignature >>= 8;
		size = fread((void*)(((char*)&ntSignature)+3), 1, sizeof(char), fp);
		offset ++;		
		if(size != sizeof(char)) {
			return freadError(sizeof(char), size);
		}		
	}	
	
	//
	// The File Header
	//	
	
	// Read File Header
	size = fread((void*)&fileHeader, 1, sizeof(IMAGE_FILE_HEADER), fp);
	if(size != sizeof(IMAGE_FILE_HEADER)) {
		return freadError(sizeof(IMAGE_FILE_HEADER), size);
	}
	
	// Print File Header
	printf("%04x:                         WORD Machine               = 0x%04x\n", offset, fileHeader.Machine);
	offset += 2;
	printf("%04x:                         WORD NumberOfSections      = 0x%04x\n", offset, fileHeader.NumberOfSections);
	offset += 2;
	printf("%04x:                         DWORD TimeDateStamp        = 0x%08x\n", offset, fileHeader.TimeDateStamp);
	offset += 4;
	printf("%04x:                         DWORD PointerToSymbolTable = 0x%08x\n", offset, fileHeader.PointerToSymbolTable);
	offset += 4;
	printf("%04x:                         DWORD NumberOfSymbols      = 0x%08x\n", offset, fileHeader.NumberOfSymbols);
	offset += 4;
	printf("%04x:                         WORD SizeOfOptionalHeader  = 0x%04x\n", offset, fileHeader.SizeOfOptionalHeader);
	offset += 2;
	printf("%04x:                         WORD Characteristics       = 0x%04x\n", offset, fileHeader.Characteristics);
	offset += 2;
	
	// Check File Header
	if(fileHeader.SizeOfOptionalHeader != sizeof(IMAGE_OPTIONAL_HEADER))
	{
		printf("SizeOfOptionalHeader should be 0x%04x, but it was 0x%04x\n", sizeof(IMAGE_OPTIONAL_HEADER), fileHeader.SizeOfOptionalHeader);
		return -1;		
	}
	
	//
	// 'Characteristics' is 16 bits and consists of a collection of flags, most
	// of them being valid only for object files and libraries:
	//
	// Bit 0 (IMAGE_FILE_RELOCS_STRIPPED) is set if there is no relocation
	// information in the file. This refers to relocation information per
	// section in the sections themselves; it is not used for executables,
	// which have relocation information in the 'base relocation' directory
	// described below.
	//
	// Bit 1 (IMAGE_FILE_EXECUTABLE_IMAGE) is set if the file is
	// executable, i.e. it is not an object file or a library. This flag
	// may also be set if the linker attempted to create an executable but
	// failed for some reason, and keeps the image in order to do e.g.
	// incremental linking the next time.
	//
	// Bit 2 (IMAGE_FILE_LINE_NUMS_STRIPPED) is set if the line number
	// information is stripped; this is not used for executable files.
	//
	// Bit 3 (IMAGE_FILE_LOCAL_SYMS_STRIPPED) is set if there is no
	// information about local symbols in the file (this is not used
	// for executable files).
	//
	// Bit 4 (IMAGE_FILE_AGGRESIVE_WS_TRIM) is set if the operating system
	// is supposed to trim the working set of the running process (the
	// amount of RAM the process uses) aggressivly by paging it out. This
	// should be set if it is a demon-like application that waits most of
	// the time and only wakes up once a day, or the like.
	//
	// Bits 7 (IMAGE_FILE_BYTES_REVERSED_LO) and 15
	// (IMAGE_FILE_BYTES_REVERSED_HI) are set if the endianess of the file is
	// not what the machine would expect, so it must swap bytes before
	// reading. This is unreliable for executable files (the OS expects
	// executables to be correctly byte-ordered).
	//
	// Bit 8 (IMAGE_FILE_32BIT_MACHINE) is set if the machine is expected
	// to be a 32 bit machine. This is always set for current
	// implementations; NT5 may work differently.
	//
	// Bit 9 (IMAGE_FILE_DEBUG_STRIPPED) is set if there is no debugging
	// information in the file. This is unused for executable files.
	// According to other information ([6]), this bit is called "fixed" and
	// is set if the image can only run if it is loaded at the preferred
	// load address (i.e. it is not relocatable).
	//
	// Bit 10 (IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP) is set if the application
	// may not run from a removable medium such as a floppy or a CD-ROM. In
	// this case, the operating system is advised to copy the file to the
	// swapfile and execute it from there.
	//
	// Bit 11 (IMAGE_FILE_NET_RUN_FROM_SWAP) is set if the application may
	// not run from the network. In this case, the operating system is
	// advised to copy the file to the swapfile and execute it from there.
	//
	// Bit 12 (IMAGE_FILE_SYSTEM) is set if the file is a system file such
	// as a driver. This is unused for executable files; it is also not
	// used in all the NT drivers I inspected.
	//
	// Bit 13 (IMAGE_FILE_DLL) is set if the file is a DLL.
	//
	// Bit 14 (IMAGE_FILE_UP_SYSTEM_ONLY) is set if the file is not
	// designed to run on multiprocessor systems (that is, it will crash
	// there because it relies in some way on exactly one processor).
	
	
	/*
	Relative Virtual Addresses
	--------------------------

	The PE format makes heavy use of so-called RVAs. An RVA, aka "relative
	virtual address", is used to describe a memory address if you don't know
	the base address. It is the value you need to add to the base address to
	get the linear address.
	The base address is the address the PE image is loaded to, and may vary
	from one invocation to the next.

	Example: suppose an executable file is loaded to address 0x400000 and
	execution starts at RVA 0x1560. The effective execution start will then
	be at the address 0x401560. If the executable were loaded to 0x100000,
	the execution start would be 0x101560.

	Things become complicated because the parts of the PE-file (the
	sections) are not necessarily aligned the same way the loaded image is.
	For example, the sections of the file are often aligned to
	512-byte-borders, but the loaded image is perhaps aligned to
	4096-byte-borders. See 'SectionAlignment' and 'FileAlignment' below.

	So to find a piece of information in a PE-file for a specific RVA,
	you must calculate the offsets as if the file were loaded, but skip
	according to the file-offsets.
	As an example, suppose you knew the execution starts at RVA 0x1560, and
	want to diassemble the code starting there. To find the address in the
	file, you will have to find out that sections in RAM are aligned to 4096
	bytes and the ".code"-section starts at RVA 0x1000 in RAM and is 16384
	bytes long; then you know that RVA 0x1560 is at offset 0x560 in that
	section. Find out that the sections are aligned to 512-byte-borders in
	the file and that ".code" begins at offset 0x800 in the file, and you
	know that the code execution start is at byte 0x800+0x560=0xd60 in the
	file.

	Then you disassemble and find an access to a variable at the linear
	address 0x1051d0. The linear address will be relocated upon loading the
	binary and is given on the assumption that the preferred load address is
	used. You find out that the preferred load address is 0x100000, so we
	are dealing with RVA 0x51d0. This is in the data section which starts at
	RVA 0x5000 and is 2048 bytes long. It begins at file offset 0x4800.
	Hence. the veriable can be found at file offset
	0x4800+0x51d0-0x5000=0x49d0.
	*/
	
	//
	// The Optional Header (not really 'optional')
	// Contains information about how to treat the PE-file
	//
	
	// Read Optional Header
	size = fread((void*)&optionalHeader, 1, sizeof(IMAGE_OPTIONAL_HEADER), fp);
	if(size != sizeof(IMAGE_OPTIONAL_HEADER)) {
		return freadError(sizeof(IMAGE_OPTIONAL_HEADER), size);
	}
	
	// Print Optional Header
	printf("%04x:                         WORD Machine               = 0x%04x\n", offset, fileHeader.Machine);
	offset += 2;
	printf("%04x:                         WORD NumberOfSections      = 0x%04x\n", offset, fileHeader.NumberOfSections);
	offset += 2;
	printf("%04x:                         DWORD TimeDateStamp        = 0x%08x\n", offset, fileHeader.TimeDateStamp);
	offset += 4;
	printf("%04x:                         DWORD PointerToSymbolTable = 0x%08x\n", offset, fileHeader.PointerToSymbolTable);
	offset += 4;
	printf("%04x:                         DWORD NumberOfSymbols      = 0x%08x\n", offset, fileHeader.NumberOfSymbols);
	offset += 4;
	printf("%04x:                         WORD SizeOfOptionalHeader  = 0x%04x\n", offset, fileHeader.SizeOfOptionalHeader);
	offset += 2;
	printf("%04x:                         WORD Characteristics       = 0x%04x\n", offset, fileHeader.Characteristics);
	offset += 2;
	
	return 0;
}