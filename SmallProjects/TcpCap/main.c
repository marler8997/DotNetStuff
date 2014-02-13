#include <stdio.h>
#include <stdlib.h>

#include <io.h>

#define WPCAP
#define HAVE_REMOTE

#include <pcap.h>

char errbuf[PCAP_ERRBUF_SIZE];

#define LINE_LEN 16

pcap_t *user_select_adapter()
{
	int i;
	pcap_if_t *deviceList, *device;
	int deviceCount;
	int userResponse;
	pcap_t *handle;

    printf("\nNo adapter selected: printing the device list:\n");
    /* The user didn't provide a packet source: Retrieve the local device list */
    if (pcap_findalldevs_ex(PCAP_SRC_IF_STRING, NULL, &deviceList, errbuf) == -1)
    {
        fprintf(stderr,"Error in pcap_findalldevs_ex: %s\n", errbuf);
        return NULL;
    }
    
    /* Print the list */
	deviceCount = 0;
    for(device = deviceList; device; device = device->next)
    {
        printf("%d. %s\n", deviceCount + 1, device->name);

        if (device->description)  printf(" (%s)\n", device->description);
        else printf(" (No description available)\n");

		deviceCount++;
    }
    
    if (deviceCount == 0)
    {
        fprintf(stderr,"No interfaces found! Exiting.\n");
        return NULL;
    }
    
    printf("Enter the interface number (1-%d):", deviceCount);
    scanf_s("%d", &userResponse);
    
    if (userResponse < 1 || userResponse > deviceCount)
    {
        printf("\nInterface number out of range.\n");

        /* Free the device list */
        pcap_freealldevs(deviceList);
        return -1;
    }
    
    /* Jump to the selected adapter */
    for (device = deviceList, i = 0; i < userResponse - 1 ; device = device->next, i++);
    
    /* Open the device */
    if ( (handle= pcap_open(device->name,
                        100 /*snaplen*/,
                        PCAP_OPENFLAG_PROMISCUOUS /*flags*/,
                        20 /*read timeout*/,
                        NULL /* remote authentication */,
                        errbuf)
                        ) == NULL)
    {
		fprintf(stderr,"\nError opening adapter: %s\n", errbuf);
        return NULL;
    }
	return handle;
}
pcap_if_t *getInterface(int interface_num)
{
	int i;
	pcap_if_t *ifaceList, *iface;

    if (pcap_findalldevs_ex(PCAP_SRC_IF_STRING, NULL, &ifaceList, errbuf) == -1)
    {
        fprintf(stderr,"Error in pcap_findalldevs_ex: %s\n", errbuf);
        return NULL;
    }
    
    // Find to the selected interface
	i = 1;
	for (iface = ifaceList, i=1; iface && i < interface_num; iface = iface->next, i++);

	if(iface == NULL) {
		fprintf(stderr, "Error: interface_num %d is out of range\n", interface_num);
	}
	return iface;
}

void printInterfaces()
{
	pcap_if_t *ifaceList, *iface;
	int ifaceCount;
	int userResponse;
	pcap_t *handle;

    if (pcap_findalldevs_ex(PCAP_SRC_IF_STRING, NULL, &ifaceList, errbuf) == -1)
    {
		fprintf(stderr, "Error: could not print network interfaces because pcap_findalldevs_ex failed: %s\n", errbuf);
        return;
    }
    
    /* Print the list */
	ifaceCount = 0;
    for(iface = ifaceList; iface; iface = iface->next)
    {
		ifaceCount++;
		fprintf(stderr, "%d.", ifaceCount);
        if (iface->description)  fprintf(stderr, iface->description);
		else fprintf(stderr, iface->name);
		fprintf(stderr, "\n");
    }    
    if (ifaceCount == 0)
    {
        fprintf(stderr, "No network interfaces\n");
        return;
	}
}

void usage()
{
	fprintf(stderr, "\n");
	fprintf(stderr, "Usage: PacketCap.exe [-f filter] [-s snaplen] interface_num\r\n");
	fprintf(stderr, "\n");
	printInterfaces();
}

//
// TODO: Implement binary and text mode
//
int main(int argc, char *argv[])
{
	int i;

	pcap_if_t *iface;
	pcap_t *handle;

	int interface_num = 0;
	char *filterText;
	int snaplen = 65536; // ensures the entire packet will be captured


	struct bpf_program filterCode;
	bpf_u_int32 filterNetmask;

	if(argc <= 1) {
		usage();
		return 0;
	}
	
	filterText = NULL;

	//
	// Parse Command Line Arguments
	//
	for(i = 1; i < argc; i++) {
		char *arg = argv[i];
		if(arg[0] == '-') {
			switch(arg[1]) {
				case 'f':
					i++;
					if(i >= argc) {
						printf("-f option missing argument\n");
						return 1;
					}
					filterText = argv[i];
					break;
				case 's':
					i++;
					if(i >= argc) {
						printf("-s option missing argument\n");
						return 1;
					}
					snaplen = atoi(argv[i]);
					break;
				default:
					printf("Unknown option -%c\n", arg[1]);
					return 1;
			}
		} else {
			if(interface_num != 0) {
				printf("Error: too many arguments\n");
				usage();
				return 1;
			} else {
				interface_num = atoi(arg);
				if(interface_num == 0) {
					fprintf(stderr, "Error: invalid interface_num: %s\n", arg);
					return 1;
				}
			}
		}
	}

	//
	// Select network interface and open it
	//
	//handle = user_select_adapter();
	iface = getInterface(interface_num);
	if(iface == NULL) return 1;

    if ( (handle= pcap_open(iface->name,
                        snaplen,
                        PCAP_OPENFLAG_PROMISCUOUS /*flags*/,
                        20 /*read timeout*/,
                        NULL /* remote authentication */,
                        errbuf)
                        ) == NULL)
    {
		fprintf(stderr,"\nError opening adapter: %s\n", errbuf);
        return NULL;
    }



	//
	// Compile and set filter
	//
	if(filterText) {
		// TODO: determine the netmask correctly
		filterNetmask = 0xFFFFFFFF;

		if(pcap_compile(handle, &filterCode, filterText, 1, filterNetmask) < 0) {
			fprintf(stderr, "Failed to compile filter: %s\n", pcap_geterr(handle));
			return 1;
		}
		if(pcap_setfilter(handle, &filterCode) < 0) {
			fprintf(stderr, "Failed to set the filter: %s\n", pcap_geterr(handle));
			return 1;
		}
	}


	return captureLoop(handle);
}

int captureLoop(pcap_t *handle)
{
	int i;
	int result;
	struct pcap_pkthdr *header;
	const u_char *pkt_data;

    /* Read the packets */
    while((result = pcap_next_ex( handle, &header, &pkt_data)) >= 0)
    {
        if(result == 0) continue; // timeout elapsed

		if(header->len != header->caplen) {
			fprintf(stderr, "Error: header len %d != caplen %d\n", header->len, header->caplen);
			return 1;
		}

		/* print pkt timestamp and pkt len */
		//printf("%ld:%ld (%ld)\n", header->ts.tv_sec, header->ts.tv_usec, header->len);   
		printf("%08X", header->len);           
    
		/* Print the packet */
		for(i = 0; i < header->caplen; i++) {
			printf("%02X", pkt_data[i]);
		}
		fflush(stdout);
    }

    if(result == -1)
    {
        fprintf(stderr, "Error reading the packets: %s\n", pcap_geterr(handle));
        return -1;
    }
	return 0;
}