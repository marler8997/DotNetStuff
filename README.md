More
====

This repository contains alot of the C# development I do.

This repository is called "More" referring to the namespace used for the common C# classes.


The More.dll Assembly
====

The More.dll .NET assembly contains many usefull C# classes that include the following:

* AnsiEscape Decoding
* Serialization/Deserialization via reflection
* ASN.1 Encoding/Decoding
* Provides a BitArray class that performs better than the Microsoft Common Framewor BitArray
* Provides Extension methods for native integer types for BigEndian and LittleEndian * Serialization/Deserialization.
* Provides array wrapper classes to facilitate changing the size
* A CommandLineParsing class
* A class for “smarter” type searching (Used by the NPC library to find types exposed by functions from a remote NPC server)
* Provides EndPoint Resolution and Parsing.  Also provides the DnsEndPoint class.
* Provides some Enumerator classes for empty lists and single object lists.
* Provides a vast amount of extension methods such as
  * Hex Parsing and Generation from binary data
  * Some extra string methods
  * Some Garbage Collector extension methods for counting GarbageCollection runs
  * An ArrayBuilder class
  * A StringLiteral Parser
  * CamelCase to UnderscoreCase Conversion
  * List to String methods
  * Dictionary Extensions to create strings or help getting values
  * Conversion of DateTime to Unix Time
  * Stopwatch methods to convert ticks to human time
  * Socket extensions to safely get end points, send files, ShutdownAndDispose, ReadUntilClosed
  * TextWriter extensions to prefix a line with n spaces
  * Stream extensions to read the entire stream
  * FileExtensions to read the entire file or write a string to a file
* An HTTP client and some Http helper methods
* A JSON parser
* An Ini-format parser
* An LFD parser/reader (another file format)
* A line parser that can be used by readers who aren’t guaranteed to receive data on line boundaries
* Various classes to enhance the WindowsCE Framework to function like the regular framework
* Some methods to help with platform specific file paths such as: IsValidUnixFileName, LocalCombine, LocalPathDiff, LocalToUnixPath, UnixToLocalPath
* A convenient method to run an external process
* A method to open/roll a rolling log
* A set of classes to run servers using select
* Some classes to tunnel sockets and streams to each other
* A SHA1 hasher
* Some SNMP helper methods
* A SortedList implementation (different than the Microsoft one)
* SOS serialization/deserialization methods (SOS = “Simple Object Serialization”)
* A StringBuilderWriter
* Some Thread helper methods
* A special type of dictionary called UniqueIndexObjectDictionary.
* A class to facilitate WaitActions (actions that should be executed later)


The More.Net Assmebly
====

The More.Net.dll assembly contains many usefull C# classes that include the following:
* TLS Helper Methods
* BufferPool
* Dns functionality
* Ftp helper methods
* Http helper classes
* Pcap helper classes
* SOCKS4/SOCKS5/HTTP proxy helper classes
* Select Server helper classes
* Telnet Stream classes
* XDR serialization classes

The Net Applications and Libraries
====
The Net applications and libraries are located in the Net directory. It includes the following:

* HTTP Server
* A SOCKS4/5 and HTTP proxy server
* An extension of UDP called CDP library and documentation
* An alternative to HTTP called CTP that uses CDP
* HTTP to CTP protocol converter
* DNS client / server
* FTP server
* A custom protocol for remote procedure calls called NPC
* RPC library
* NFS Server (NFS client for testing)
* Netcat
* Network Adapter (Used to forward tcp/udp connections/packets)
* SNMP client
* Telnet client
* Custom protocol TMP used for tunneling into private networks




