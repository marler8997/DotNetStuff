using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

using More;

namespace More
{
    public class UnbufferedStreamReader : TextReader
    {
        Stream stream;
        Encoding encoding;

        ByteBuffer buffer;

        public UnbufferedStreamReader(Stream stream)
            : this(stream, Encoding.UTF8)
        {
        }
        public UnbufferedStreamReader(Stream stream, Encoding encoding)
        {
            this.stream = stream;
            this.encoding = encoding;
            this.buffer = new ByteBuffer(256, 256);
        }
        public override string ReadLine()
        {
            Int32 length = 0;

            while (true)
            {
                Int32 value = stream.ReadByte();
                if (value == -1)
                {
                    return (length == 0) ? null : encoding.GetString(buffer.array, 0, length);
                }

                if (value == '\n')
                {
                    if (length > 1 && buffer.array[length - 1] == '\r')
                    {
                        length--;
                    }
                    return encoding.GetString(buffer.array, 0, length);
                }


                buffer.EnsureCapacityCopyData(length + 1);
                buffer.array[length] = (Byte)value;
                length++;
            }
        }
        // Read works differently than the `Read()` method of a 
        // TextReader. It reads the next BYTE rather than the next character
        public override int Read()
        {
            return stream.ReadByte();
        }
        public override void Close()
        {
            stream.Close();
        }
        protected override void Dispose(bool disposing)
        {
            stream.Dispose();
        }
        public override int Peek()
        {
            throw new NotImplementedException();
        }
        public override int Read(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }
        public override int ReadBlock(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override string ReadToEnd()
        {
            throw new NotImplementedException();
        }
    }
    class ProgramOptions : CLParser
    {
        public readonly CLSwitch template;
        public ProgramOptions()
        {
            template = new CLSwitch('t', "template", "Generate a NpcClient template file");
            Add(template);
        }
        public override void PrintUsageHeader()
        {
            Console.Error.WriteLine("Create or update client: NpcClientGenerator.exe <file> [<host> <port>]");
            Console.Error.WriteLine("Generate template file : NpcClientGenerator.exe --template <file>");
        }
    }



    public class CustomObjectConfiguration : LfdConfiguration
    {
        public readonly ConfigString name;
        public readonly ConfigSwitch useObjectName;
        public readonly MultiConfigStrings interfaces;
        public readonly ConfigSwitch requireObjectWithMatchingInterfaces;

        readonly List<Config> configs = new List<Config>();
        void Add(Config config)
        {
            configs.Add(config);
            AddWithNameLowerInvariant(config.nameLowerInvarient, config.Handle);
        }
        public CustomObjectConfiguration()
        {
            this.name = new ConfigString("Name", "The name of the custom object", "custom-name");
            Add(name);

            this.useObjectName = new ConfigSwitch("UseObjectName", "Use to add object name to client methods and the NPC calls", false);
            Add(useObjectName);

            this.interfaces = new MultiConfigStrings("Interfaces", "List of interfaces to implement", "interface1 interface2...");
            Add(interfaces);

            this.requireObjectWithMatchingInterfaces = new ConfigSwitch("RequireObjectWithMatchingInterfaces",
                "When true, the generation will fail if there is not a remote object with the same interfaces", false);
            Add(requireObjectWithMatchingInterfaces);
        }
        public void WriteTemplate(TextWriter writer)
        {
            foreach (Config config in configs)
            {
                writer.WriteLine("// #");
                config.WriteTemplate("// #   ", writer);
            }
        }
    }

    
    public class NpcClientGenerationConfiguration : LfdConfiguration
    {
        //
        // Generic Configuration Options
        //
        public readonly ConfigString defaultServer;
        public readonly Config<UInt16> defaultPort;
        public readonly ConfigSwitch xmlCommments;
        public readonly ConfigStrings extraUsings;

        public readonly ConfigString objectNamespace;

        //
        // Interface Generation Options
        //
        public readonly ConfigSwitch generateInterfaces;

        //
        // Type Generation Options
        //
        public readonly ConfigSwitch generateTypes;
        public readonly ConfigString typeFilterType;
        public readonly MultiConfigStrings typeFilter;

        //
        // Method Object Generation Options
        //

        public readonly ConfigSwitch omitDefaultObjects;
        public readonly ConfigString defaultObjectFilterType;
        public readonly MultiConfigStrings defaultObjectFilter;

        readonly CustomObjectConfiguration customObjectConfiguration;
        public readonly List<CustomObjectConfiguration> customObjects = new List<CustomObjectConfiguration>();

        
        readonly List<Config> configs = new List<Config>();
        void Add(Config config)
        {
            configs.Add(config);
            AddWithNameLowerInvariant(config.nameLowerInvarient, config.Handle);
        }
        public NpcClientGenerationConfiguration()
        {
            //
            // Generic Configuration Options
            //
            defaultServer = new ConfigString("DefaultServer", "The default ip or hostname of the NPC server", "hostnameOrIP");
            Add(defaultServer);

            defaultPort = new Config<UInt16>("DefaultPort", "The default port of the NPC server", UInt16.Parse, true, 0);
            Add(defaultPort);

            xmlCommments = new ConfigSwitch("XmlComments", "Enables XML comments", false);
            Add(xmlCommments);

            extraUsings = new ConfigStrings("ExtraUsings", "Adds extra using statements", "namespace1 namespace2...");
            Add(extraUsings);

            objectNamespace = new ConfigString("ObjectNamespace", "Put the generated client objects in the given namespace", "namespace");
            Add(objectNamespace);

            //
            // Interface Generation Options
            //
            generateInterfaces = new ConfigSwitch("GenerateInterfaces", "Enables interface generation", false);
            Add(generateInterfaces);

            //
            // Type Generation Options
            //
            generateTypes = new ConfigSwitch("GenerateTypes", "Enables type generation", true);
            Add(generateTypes);

            typeFilterType = new ConfigString("TypeFilterType",
                "Indicates whether the type filter is 'Inclusive' or 'Exclusive'.  This is required if there is a TypeFilter.", "[Inclusive|Exclusive]");
            Add(typeFilterType);

            typeFilter = new MultiConfigStrings("TypeFilter", "Used to filter types", "regex1 regex2...");
            Add(typeFilter);

            //
            // Method Object Generation Objects
            //
            omitDefaultObjects = new ConfigSwitch("OmitDefaultObjects", "Use to omit all default objects", false);
            Add(omitDefaultObjects);

            defaultObjectFilterType = new ConfigString("DefaultObjectFilterType",
                "Indicates whether the default object filter is 'Inclusive' or 'Exclusive'.  This is required if there is a DefaultObjectFilter.", "[Inclusive|Exclusive]");
            Add(defaultObjectFilterType);

            defaultObjectFilter = new MultiConfigStrings("DefaultObjectFilter", "Used to filter which object to generate the default code for", "regex1 regex2...");
            Add(defaultObjectFilter);

            customObjectConfiguration = new CustomObjectConfiguration();
            AddWithNameLowerInvariant("CustomObject".ToLowerInvariant(), HandleCustomObject);
        }
        LfdLine HandleCustomObject(LfdReader reader, LfdLine line)
        {
            CustomObjectConfiguration customObjectConfiguration = new CustomObjectConfiguration();
            line = customObjectConfiguration.Handle(reader, line);
            customObjects.Add(customObjectConfiguration);
            return line;
        }
        public void WriteTemplate(TextWriter writer)
        {
            foreach (Config config in configs)
            {
                writer.WriteLine("// #");
                config.WriteTemplate("// # ", writer);
            }
            
            writer.WriteLine("// #");
            writer.WriteLine("// # CustomObject {");
            customObjectConfiguration.WriteTemplate(writer);
            writer.WriteLine("// # }");
            writer.WriteLine("// #");
            writer.WriteLine("// end");
        }
    }


    public class NpcClientConfigurationReader : ILineReader
    {
        readonly FileStream stream;
        readonly UnbufferedStreamReader reader;
        public NpcClientConfigurationReader(FileStream stream)
        {
            this.stream = stream;
            this.reader = new UnbufferedStreamReader(stream);
        }
        public String ReadLine()
        {
            while (true)
            {
                Int64 positionBeforeReader = stream.Position;

                String line = reader.ReadLine();
                if (line == null)
                {
                    throw new FormatException("Missing the 'end' to mark the end of the configuration");
                }
                line = line.Trim();
                if (String.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("//"))
                {
                    line = line.Substring(2).Trim();
                }
                if (String.IsNullOrEmpty(line)) continue;

                // Check if this is the end
                if (line.StartsWith("end", StringComparison.CurrentCultureIgnoreCase)) return null;

                // Check if line is a comment
                Int32 i = 0;
                while (true)
                {
                    if (i >= line.Length) break;
                    Char c = line[i];
                    if(c == '#') break;
                    if (!Char.IsWhiteSpace(c))
                    {
                        return line.Substring(i);
                    }
                    i++;
                }
            }
        }
        public void Dispose()
        {
            reader.Dispose();
        }
    }





    public class Filter
    {
        Boolean filterIsInclusive;
        Regex[] filterRegexes;
        public Filter(Boolean filterIsInclusive, Regex[] filterRegexes)
        {
            this.filterIsInclusive = filterIsInclusive;
            this.filterRegexes = filterRegexes;
        }
        public Boolean Exclude(String str)
        {
            if (filterIsInclusive)
            {
                for (int i = 0; i < filterRegexes.Length; i++)
                {
                    Regex regex = filterRegexes[i];
                    if (regex.IsMatch(str)) return false;
                }
                return true;
            }
            else
            {
                for (int i = 0; i < filterRegexes.Length; i++)
                {
                    Regex regex = filterRegexes[i];
                    if (regex.IsMatch(str)) return true;
                }
                return false;
            }
        }
    }

    public static class Extensions
    {
        public static RemoteNpcInterface[] IntefacesMatch(this RemoteNpcObject npcObject, List<String> interfaces)
        {
            if (npcObject.interfaces.Length != interfaces.Count) return null;

            RemoteNpcInterface[] remoteNpcInterfaces = new RemoteNpcInterface[interfaces.Count];
            Int32 remoteNpcInterfaceIndex = 0;

            foreach(String @interface in interfaces)
            {
                Boolean foundInterface = false;
                for(int i = 0; i < npcObject.interfaces.Length; i++)
                {
                    RemoteNpcInterface remoteNpcInterface = npcObject.interfaces[i];
                    if(@interface.Equals(remoteNpcInterface.name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        remoteNpcInterfaces[remoteNpcInterfaceIndex++] = remoteNpcInterface;
                        foundInterface = true;
                        break;
                    }
                }
                if (!foundInterface) return null;
            }

            return remoteNpcInterfaces;
        }

    }

    public class NpcClientGeneratorMain
    {
        const String Inclusive = "Inclusive", Exclusive = "Exclusive";

        public static String SplitName(String fullName, out String @namespace)
        {
            Int32 periodIndex = fullName.LastIndexOf('.');

            if (periodIndex < 0)
            {
                @namespace = "";
                return fullName;
            }

            @namespace = fullName.Remove(periodIndex);
            return fullName.Substring(periodIndex + 1);
        }
        static Int32 ConfigurationError(String fmt, params Object[] args)
        {
            Console.WriteLine(fmt, args);
            return 1;
        }
        static Filter GetFilterFromConfiguration(String option, String filterType, IList<String> filters)
        {
            if (filters == null || filters.Count <= 0) return null;

            if(filterType == null) throw new FormatException(String.Format(
                "Option '{0}Filter' requires that option '{0}FilterType' also be set", option));

            Boolean filterTypeIsInclusive;
            if (filterType.Equals(Inclusive))
            {
                filterTypeIsInclusive = true;
            }
            else if (filterType.Equals(Exclusive))
            {
                filterTypeIsInclusive = false;
            }
            else
            {
                throw new FormatException(String.Format("Expected {0}FilterType to be '{0}' or '{1}' but is '{2}'",
                    Inclusive, Exclusive, filterType));
            }

            Regex[] typeFilterRegexes = new Regex[filters.Count];
            for (int i = 0; i < typeFilterRegexes.Length; i++)
            {
                String typeFilterString = filters[i];
                typeFilterRegexes[i] = new Regex(typeFilterString, RegexOptions.Compiled);
            }
            return new Filter(filterTypeIsInclusive, typeFilterRegexes);
        }

        public static Int32 Main(String[] args)
        {
            //
            // Command line arguments
            //
            ProgramOptions options = new ProgramOptions();
            List<String> nonOptionArgs = options.Parse(args);


            NpcClientGenerationConfiguration configuration = new NpcClientGenerationConfiguration();
            if (options.template.set)
            {
                TextWriter writer = null;
                try
                {
                    if (nonOptionArgs.Count > 0)
                    {
                        writer = new StreamWriter(new FileStream(nonOptionArgs[0], FileMode.Create, FileAccess.Write));
                    }
                    else
                    {
                        writer = Console.Out;
                    }
                    
                    configuration.WriteTemplate(writer);
                    return 0;
                }
                finally
                {
                    if (writer != null) writer.Close();
                }
            }

            if (nonOptionArgs.Count < 1)
            {
                return options.ErrorAndUsage("Not enough non-option arguments");
            }

            String filename = nonOptionArgs[0];

            String serverHost = null;
            UInt16 port = 0;

            if (nonOptionArgs.Count >= 2)
            {
                serverHost = nonOptionArgs[1];
                if (nonOptionArgs.Count >= 3)
                {
                    port = UInt16.Parse(nonOptionArgs[2]);
                }
            }

            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            {
                NpcClientConfigurationReader reader = new NpcClientConfigurationReader(stream);
                LfdReader lfdReader = new LfdReader(reader);
                configuration.Parse(lfdReader);
            }

            //
            // Determine Server Hostname
            //
            if (serverHost == null)
            {
                serverHost = configuration.defaultServer.value;
                if (serverHost == null)
                    return ConfigurationError("No host was provided and the client configuration had no default host");
            }

            if (port == 0)
            {
                if (!configuration.defaultPort.set)
                    return ConfigurationError("No port was provided and the client configuration had no default port");

                port = configuration.defaultPort.value;
            }


            //
            //
            //
            // Check that Configuration is Valid
            //
            //
            //
            Filter typeFilter = GetFilterFromConfiguration("Type",
                configuration.typeFilterType.value,
                configuration.typeFilter.strings);

            Filter defaultObjectFilter = GetFilterFromConfiguration("DefaultObject",
                configuration.defaultObjectFilterType.value,
                configuration.defaultObjectFilter.strings);



            // Reset File Stream
            StreamWriter output = new StreamWriter(stream);

            //
            // Attempt to connect to Npc Server
            //
            EndPoint npcServerEndPoint = EndPoints.EndPointFromIPOrHost(serverHost, port);
            Socket npcServerSocket = new Socket(npcServerEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Console.WriteLine("Connecting to {0}:{1}...", serverHost, port);
                npcServerSocket.Connect(npcServerEndPoint);
                Console.WriteLine("Connected");
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to connect to npc server '{0}' on port {1}", serverHost, port);
                return 1;
            }

            try
            {
                Int32 returnValue = GenerateClient(configuration, output, npcServerSocket, typeFilter, defaultObjectFilter);
                if (returnValue == 0)
                {
                    stream.SetLength(stream.Position + 1);
                }
                return returnValue;
            }
            finally
            {
                output.Dispose();
            }
        }
        static String SwitchNamespace(TextWriter output, String currentNamespace, String newNamespace)
        {
            if (String.IsNullOrEmpty(newNamespace))
            {
                if (!String.IsNullOrEmpty(currentNamespace)) output.WriteLine("}");
            }
            else
            {
                if (!newNamespace.Equals(currentNamespace))
                {
                    if (!String.IsNullOrEmpty(currentNamespace)) output.WriteLine("}");
                    output.WriteLine("namespace {0}", newNamespace);
                    output.WriteLine("{");
                }
            }
            return newNamespace;
        }
        static Int32 GenerateClient(NpcClientGenerationConfiguration configuration, TextWriter output, Socket socket,
            Filter typeFilter, Filter defaultObjectFilter)
        {
            output.WriteLine();
            output.WriteLine();
            output.WriteLine("//");
            output.WriteLine("// This file is generated using the NpcClientGenerator.exe program.");
            output.WriteLine("// The NpcClientGenerator reads the configuration from the beginning of this file.");
            output.WriteLine("// The configuration is ended using the string 'end'");
            output.WriteLine("//");
            output.WriteLine("// Then the generator connects to the server and generates this client (the rest of the file) based on the configuration");
            output.WriteLine("//");

            Boolean xmlComments = configuration.xmlCommments.value;
            String objectNamespace = configuration.objectNamespace.value;
            
            //
            // Extra Using Statements
            //
            String[] extraUsings = null;
            if (configuration.extraUsings.strings != null)
            {
                extraUsings = configuration.extraUsings.strings;
            }

            //
            // Print file header info
            //
            output.WriteLine("using System;");
            output.WriteLine("using System.Net;");
            output.WriteLine("using System.Net.Sockets;");
            output.WriteLine();
            output.WriteLine("using More;");
            output.WriteLine();
            if (extraUsings != null)
            {
                for (int i = 0; i < extraUsings.Length; i++)
                {
                    output.WriteLine("using {0};", extraUsings[i]);
                }
                output.WriteLine();
            }
            output.WriteLine("using Pointer = System.Int64;");
            output.WriteLine();

            String currentNamespace = null;

            SocketLineReader socketLineReader = new SocketLineReader(socket, Encoding.ASCII, 512, 256);

            if (configuration.generateTypes.value)
            {
                socket.Send(Encoding.ASCII.GetBytes(":type\n"));
                //Console.WriteLine("[ToServer]   :type");
                
                while (true)
                {
                    String typeDefinitionLine = socketLineReader.ReadLine();
                    if (typeDefinitionLine == null) throw new InvalidOperationException("Server closed unexpectedly");
                    //Console.WriteLine("[FromServer] {0}", typeDefinitionLine);
                    if (typeDefinitionLine.Length == 0) break; // empty line

                    Int32 spaceIndex = typeDefinitionLine.IndexOf(' ');
                    String typeFullName = typeDefinitionLine.Remove(spaceIndex);
                    String typeDefinition = typeDefinitionLine.Substring(spaceIndex + 1);

                    String typeNamespace;
                    String typeName = SplitName(typeFullName, out typeNamespace);

                    if(typeFilter != null && typeFilter.Exclude(typeFullName))
                    {
                        Console.WriteLine("Type '{0}' was filtered out", typeFullName);
                        continue;
                    }

                    currentNamespace = SwitchNamespace(output, currentNamespace, typeNamespace);

                    if (typeDefinition.StartsWith("Enum"))
                    {
                        SosEnumDefinition enumDefinition = SosTypes.ParseSosEnumTypeDefinition(typeDefinition, 4);
                        if (xmlComments)
                        {
                            output.WriteLine("    /// <summary>The {0} enum</summary>", typeName);
                        }
                        output.WriteLine("    public enum {0}", typeName);
                        output.WriteLine("    {");
                        foreach(KeyValuePair<String,Int32> pair in enumDefinition.Values)
                        {
                            if (xmlComments)
                            {
                                output.WriteLine("        /// <summary>The {0} enum value</summary>", pair.Key);
                            }
                            output.WriteLine("        {0} = {1},", pair.Key, pair.Value);
                        }
                        output.WriteLine("    }");
                    }
                    else
                    {
                        SosObjectDefinition objectDefinition = SosTypes.ParseSosObjectTypeDefinition(typeDefinition, 0);

                        if (xmlComments)
                        {
                            output.WriteLine("    /// <summary>The {0} class</summary>", typeName);
                        }
                        output.WriteLine("    public class {0}", typeName);
                        output.WriteLine("    {");
                        foreach (KeyValuePair<String, String> pair in objectDefinition.fieldNamesToFieldTypes)
                        {
                            String fieldTypeName = pair.Value;
                            String fieldName = pair.Key;

                            if (fieldName.Equals("class") || fieldName.Equals("struct"))
                            {
                                fieldName = "@" + fieldName;
                            }

                            if (xmlComments)
                            {
                                output.WriteLine("        /// <summary>The {0} class field</summary>", fieldName);
                            }
                            output.WriteLine("        public {0} {1};", fieldTypeName, fieldName);
                        }

                        if (xmlComments)
                        {
                            output.WriteLine("         /// <summary>The empty constructor</summary>");
                        }
                        output.WriteLine("        public {0}()", typeName);
                        output.WriteLine("        {");
                        output.WriteLine("        }");


                        output.WriteLine("    }");
                    }
                }
            }

            //
            // Switch Namespace
            //
            currentNamespace = SwitchNamespace(output, currentNamespace, objectNamespace);


            Dictionary<String,RemoteNpcInterface> serverInterfaces;
            List<RemoteNpcObject> remoteNpcObjects = NpcClient.GetServerInterface(socketLineReader, out serverInterfaces);
            

            //
            // Generate Interfaces Definitions
            //
            if (xmlComments)
            {
                output.WriteLine("    /// <summary>Static class to hold interface definitions</summary>");
            }
            output.WriteLine("    public static class NpcInterfaces");
            output.WriteLine("    {");
            foreach (KeyValuePair<String, RemoteNpcInterface> pair in serverInterfaces)
            {
                String interfaceName = pair.Key;
                RemoteNpcInterface npcInterface = pair.Value;
                if (xmlComments)
                {
                    output.WriteLine("        /// <summary>Saved Interface Defintion for '{0}'</summary>", interfaceName);
                }
                output.Write("        public static readonly RemoteNpcInterface {0} = new RemoteNpcInterface(\"{0}\", ", interfaceName);
                if (npcInterface.parentInterfaceNames == null || npcInterface.parentInterfaceNames.Length <= 0)
                {
                    output.Write("null");
                }
                else
                {
                    output.Write("new String[]{");
                    for (int i = 0; i < npcInterface.parentInterfaceNames.Length; i++)
                    {
                        if (i > 0) output.Write(",");
                        output.Write("\"{0}\"", npcInterface.parentInterfaceNames[i]);
                    }
                    output.Write("}");
                }
                output.WriteLine(", new SosMethodDefinition[] {");
                for (int i = 0; i < npcInterface.methods.Length; i++)
                {
                    SosMethodDefinition methodDefinition = npcInterface.methods[i];
                    output.Write("            new SosMethodDefinition(\"{0}\",\"{1}\"",
                        methodDefinition.methodName, methodDefinition.returnSosTypeName);

                    if (methodDefinition.parameters != null)
                    {
                        foreach (SosMethodDefinition.Parameter parameter in methodDefinition.parameters)
                        {
                            output.Write(",\"{0}\",\"{1}\"", parameter.sosTypeName, parameter.name);
                        }
                    }

                    output.WriteLine("),");
                }
                output.WriteLine("        });");
            }

            output.WriteLine("        public static readonly RemoteNpcInterface[] All = new RemoteNpcInterface[] {");
            foreach (KeyValuePair<String, RemoteNpcInterface> pair in serverInterfaces)
            {
                output.WriteLine("            {0},", pair.Key);
            }
            output.WriteLine("        };");

            output.WriteLine("    }");

            //
            // Generate C# Interfaces
            //
            if (configuration.generateInterfaces.value)
            {
                foreach (KeyValuePair<String, RemoteNpcInterface> pair in serverInterfaces)
                {
                    String interfaceName = pair.Key;
                    RemoteNpcInterface npcInterface = pair.Value;
                    if (xmlComments)
                    {
                        output.WriteLine("    /// <summary>Interface Defintion for '{0}'</summary>", interfaceName);
                    }
                    output.Write("    public interface {0}", interfaceName);
                    if (npcInterface.parentInterfaceNames != null)
                    {
                        Boolean atFirstInterface = true;
                        foreach(String parentInterfaceName in npcInterface.parentInterfaceNames)
                        {
                            PrintInterface(output, ref atFirstInterface, parentInterfaceName);
                        }
                    }
                    output.WriteLine();
                    output.WriteLine("    {");
                    for (int i = 0; i < npcInterface.methods.Length; i++)
                    {
                        SosMethodDefinition methodDefinition = npcInterface.methods[i];
                        String clientMethodName = methodDefinition.methodName;
                        if (clientMethodName.Equals("Dispose"))
                        {
                            clientMethodName = "RemoteDispose";
                        }

                        Boolean returnTypeIsVoid = methodDefinition.returnSosTypeName.Equals("Void");
                        output.Write("        {0} {1}(", returnTypeIsVoid ? "void" : methodDefinition.returnSosTypeName, clientMethodName);

                        if (methodDefinition.parameters != null)
                        {
                            Boolean atFirst = true;
                            foreach (SosMethodDefinition.Parameter parameter in methodDefinition.parameters)
                            {
                                if (atFirst) { atFirst = false; } else { output.Write(", "); }
                                output.Write("{0} {1}", parameter.sosTypeName, parameter.name);
                            }
                        }
                        output.WriteLine(");");
                    }
                    output.WriteLine("    }");
                }
            }


            //
            // Generate Default Objects
            //
            if (!configuration.omitDefaultObjects.value)
            {
                for (int objectIndex = 0; objectIndex < remoteNpcObjects.Count; objectIndex++)
                {
                    RemoteNpcObject remoteNpcObject = remoteNpcObjects[objectIndex];

                    String objectFullName = remoteNpcObject.name;

                    if (defaultObjectFilter != null && defaultObjectFilter.Exclude(objectFullName))
                    {
                        Console.WriteLine("Default Object '{0}' was filtered out", objectFullName);
                        continue;
                    }

                    String @namespace;
                    String objectShortName = SplitName(objectFullName, out @namespace);

                    GenerateObject(output, xmlComments, objectShortName, objectFullName,
                        remoteNpcObject.interfaces, false, configuration.generateInterfaces.value);
                }
            }

            //
            // Generate Custom Objects
            //
            for (int customObjectIndex = 0; customObjectIndex < configuration.customObjects.Count; customObjectIndex++)
            {
                CustomObjectConfiguration customObjectConfig = configuration.customObjects[customObjectIndex];

                String objectFullName = customObjectConfig.name.value;
                if (objectFullName == null) ConfigurationError(
                    "Every CustomObject is required to have a 'Name' property");

                String @namespace;
                String objectShortName = SplitName(objectFullName, out @namespace);

                List<String> interfaceStrings = customObjectConfig.interfaces.strings;
                if(interfaceStrings.Count <= 0) ConfigurationError(
                    "Every CustomObject must have at least 1 interface");

                //
                // Check that there is a matching object
                //
                RemoteNpcInterface[] interfaces;
                if (customObjectConfig.requireObjectWithMatchingInterfaces.value)
                {
                    interfaces = null;
                    for (int npcObjectIndex = 0; npcObjectIndex < remoteNpcObjects.Count; npcObjectIndex++)
                    {
                        RemoteNpcObject npcObject = remoteNpcObjects[npcObjectIndex];

                        interfaces = npcObject.IntefacesMatch(interfaceStrings);
                        if (interfaces != null)
                        {
                            Console.WriteLine("CustomObject '{0}' Matches NpcServerObject '{1}'",
                                objectFullName, npcObject.name);
                            break;
                        }
                    }
                    if (interfaces == null)
                    {
                        Console.WriteLine("The NPC server does not have an object with the following interfaces:");
                        for (int i = 0; i < interfaceStrings.Count; i++)
                        {
                            Console.WriteLine("    {0}", interfaceStrings[i]);
                        }
                        return 1;
                    }
                }
                else
                {
                    interfaces = new RemoteNpcInterface[interfaceStrings.Count];
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        String interfaceString = interfaceStrings[i];
                        if(!serverInterfaces.TryGetValue(interfaceString, out interfaces[i]))
                        {
                            Console.WriteLine("Server does not have the '{0}' interface", interfaceString);
                            return 1;
                        }
                    }
                }

                GenerateObject(output, xmlComments, objectShortName, objectFullName,
                    interfaces, customObjectConfig.useObjectName.value, configuration.generateInterfaces.value);
            }

            // End all namespaces
            currentNamespace = SwitchNamespace(output, currentNamespace, null);

            return 0;
        }

        static void PrintInterface(TextWriter output, ref Boolean atFirstInterface, String @interface)
        {
            if (atFirstInterface)
            {
                output.Write(" : ");
                atFirstInterface = false;
            }
            else
            {
                output.Write(", ");
            }
            output.Write(@interface);
        }


        static void GenerateObject(TextWriter output, Boolean xmlComments, String className, String npcObjectName,
            RemoteNpcInterface[] interfaces, Boolean addObjectNameToMethods, Boolean generateInterfaces)
        {
            if (xmlComments)
            {
                output.WriteLine("    /// <summary>An NpcClient wrapper</summary>");
            }
            output.Write("    public class {0}", className);
            
            Boolean atFirstInterface = true;
            if (npcObjectName != null)
            {
                if (addObjectNameToMethods)
                {
                    PrintInterface(output, ref atFirstInterface, "INpcDynamicClient");
                }
                else
                {
                    PrintInterface(output, ref atFirstInterface, "INpcClient");
                }
            }
            if (generateInterfaces)
            {
                for (int i = 0; i < interfaces.Length; i++)
                {
                    PrintInterface(output, ref atFirstInterface, interfaces[i].name);
                }
            }

            output.WriteLine();
            output.WriteLine("    {");
            if (xmlComments)
            {
                output.WriteLine("        /// <summary>The NpcClient interface to perform npc calls</summary>");
            }
            output.WriteLine("        public INpcClientCaller npcClientCaller;");
            if (xmlComments)
            {
                output.WriteLine("        /// <summary>The empty constructor</summary>");
            }
            output.WriteLine("        public {0}(){{}}", className);
            if (xmlComments)
            {
                output.WriteLine("        /// <summary>The constructor with an NpcClient</summary>");
            }
            output.WriteLine("        public {0}(INpcClientCaller npcClientCaller)", className);
            output.WriteLine("        {");
            output.WriteLine("            this.npcClientCaller = npcClientCaller;");
            output.WriteLine("        }");
            if (xmlComments)
            {
                output.WriteLine("        /// <summary>The constructor with an endpoint</summary>");
            }
            output.WriteLine("        public {0}(Boolean verifyInterfaceMethods, EndPoint endPoint, Boolean threadSafe)", className);
            output.WriteLine("        {");
            output.WriteLine("            this.npcClientCaller = new NpcClient(endPoint, verifyInterfaceMethods ? ObjectInterfaces : null, threadSafe);");
            output.WriteLine("        }");
            if (xmlComments)
            {
                output.WriteLine("        /// <summary>The constructor with a socket</summary>");
            }
            output.WriteLine("        public {0}(Boolean verifyInterfaceMethods, Socket socket, Boolean threadSafe)", className);
            output.WriteLine("        {");
            output.WriteLine("            this.npcClientCaller = new NpcClient(socket, verifyInterfaceMethods ? ObjectInterfaces : null, threadSafe);");
            output.WriteLine("        }");
            if (xmlComments)
            {
                output.WriteLine("        /// <summary>Array of interfaces used by this client</summary>");
            }
            output.WriteLine("        public static readonly RemoteNpcInterface[] ObjectInterfaces = new RemoteNpcInterface[] {");
            for (int i = 0; i < interfaces.Length; i++)
            {
                RemoteNpcInterface npcInterface = interfaces[i];
                output.WriteLine("            NpcInterfaces.{0},", npcInterface.name);
            }
            output.WriteLine("        };");
            if (npcObjectName != null)
            {
                if (xmlComments)
                {
                    output.WriteLine("        /// <summary>RemoteNpcObject '{0}'</summary>", npcObjectName);
                }
                output.WriteLine("        public static readonly RemoteNpcObject Object = new RemoteNpcObject(\"{0}\", ObjectInterfaces);", npcObjectName);
            }
            /*
            //
            // Print Method Definitions
            //
            if (xmlComments)
            {
                /// <param name="args"></param>
                output.WriteLine("        /// <summary>Retrieve remote object and enum types from server and verify they are correct</summary>");
            }
            output.WriteLine("        public void UpdateAndVerifyEnumAndObjectTypes()");
            output.WriteLine("        {");
            output.WriteLine("            npcClientCaller.UpdateAndVerifyEnumAndObjectTypes();");
            output.WriteLine("        }");
            */
            /*
            if (xmlComments)
            {
                /// <param name="args"></param>
                output.WriteLine("        /// <summary>Verify that expected interface definitions are the same</summary>");
                output.WriteLine("        /// <param name=\"forceMethodUpdateFromServer\">True if you would like to update method defintions from server whether or not they have been cached</param>");
            }
            output.WriteLine("        public void VerifyInterfaceMethods(Boolean forceInterfaceUpdateFromServer)");
            output.WriteLine("        {");
            output.WriteLine("            npcClientCaller.VerifyInterfaceMethods(forceInterfaceUpdateFromServer, objectInterfaces);");
            output.WriteLine("        }");
            */
            if (npcObjectName != null)
            {
                if (addObjectNameToMethods)
                {
                    if (xmlComments)
                    {
                        /// <param name="args"></param>
                        output.WriteLine("        /// <summary>Verify that expected interface definitions are the same and the objects</summary>");
                        output.WriteLine("        /// <param name=\"forceInterfaceUpdateFromServer\">True if you would like to update method defintions from server whether or not they have been cached</param>");
                        output.WriteLine("        /// <param name=\"objectName\">The object name</param>");
                    }
                    output.WriteLine("        public void VerifyObject(Boolean forceInterfaceUpdateFromServer, String objectName)");
                    output.WriteLine("        {");
                    output.WriteLine("            npcClientCaller.VerifyObject(forceInterfaceUpdateFromServer, new RemoteNpcObject(objectName, ObjectInterfaces));");
                    output.WriteLine("        }");
                }
                else
                {
                    if (xmlComments)
                    {
                        /// <param name="args"></param>
                        output.WriteLine("        /// <summary>Verify that expected interface definitions are the same and the objects</summary>");
                        output.WriteLine("        /// <param name=\"forceInterfaceUpdateFromServer\">True if you would like to update method defintions from server whether or not they have been cached</param>");
                    }
                    output.WriteLine("        public void VerifyObject(Boolean forceInterfaceUpdateFromServer)");
                    output.WriteLine("        {");
                    output.WriteLine("            npcClientCaller.VerifyObject(forceInterfaceUpdateFromServer, Object);");
                    output.WriteLine("        }");
                }
            }
            /*
            if (xmlComments)
            {
                /// <param name="args"></param>
                output.WriteLine("        /// <summary>Verify that expected interface definitions are the same and the objects</summary>");
                output.WriteLine("        /// <param name=\"forceMethodUpdateFromServer\">True if you would like to update method defintions from server whether or not they have been cached</param>");
            }
            output.WriteLine("        public void VerifyObjectsAndInterfaceMethods(Boolean forceInterfaceUpdateFromServer, String objectName)");
            output.WriteLine("        {");
            output.WriteLine("            npcClientCaller.VerifyObjectsAndInterfaceMethods(forceInterfaceUpdateFromServer, , objectInterfaces);");
            output.WriteLine("        }");
            */
            if (xmlComments)
            {
                output.WriteLine("        /// <summary>Dispose the class</summary>");
            }
            output.WriteLine("        public void Dispose()");
            output.WriteLine("        {");
            output.WriteLine("            npcClientCaller.Dispose();");
            output.WriteLine("        }");

            //
            // Print Methods
            //
            for (int interfaceIndex = 0; interfaceIndex < interfaces.Length; interfaceIndex++)
            {
                RemoteNpcInterface npcInterface = interfaces[interfaceIndex];

                for (int methodIndex = 0; methodIndex < npcInterface.methods.Length; methodIndex++)
                {
                    SosMethodDefinition methodDefinition = npcInterface.methods[methodIndex];

                    String clientMethodName = methodDefinition.methodName;
                    
                    Boolean methodNameChanged = false;
                    if (methodDefinition.methodName.Equals("Dispose"))
                    {
                        clientMethodName = "RemoteDispose";
                        methodNameChanged = true;
                    }

                    Boolean returnTypeIsVoid = methodDefinition.returnSosTypeName.Equals("Void");
                    if (xmlComments)
                    {
                        output.WriteLine("        /// <summary>The {0} method.{1}</summary>", methodDefinition.methodName,
                            methodNameChanged ? " The method name was changed to prevent conflicts." : "");
                        if (methodDefinition.parameters != null)
                        {
                            foreach (SosMethodDefinition.Parameter parameter in methodDefinition.parameters)
                            {
                                output.WriteLine("        /// <param name=\"{0}\">The {0} parameter of type {1}</param>", parameter.name, parameter.sosTypeName);
                            }
                        }
                        if (!returnTypeIsVoid)
                        {
                            output.WriteLine("        /// <returns>Return type is {0}</returns>", methodDefinition.returnSosTypeName);
                        }
                    }

                    output.Write("        public {0} {1}(", returnTypeIsVoid ? "void" : methodDefinition.returnSosTypeName, clientMethodName);

                    Boolean atFirstParameter = true;
                    if (addObjectNameToMethods)
                    {
                        output.Write("String objectName");
                        atFirstParameter = false;
                    }

                    if (methodDefinition.parameters != null)
                    {
                        foreach (SosMethodDefinition.Parameter parameter in methodDefinition.parameters)
                        {
                            if (atFirstParameter) atFirstParameter = false; else output.Write(", ");

                            output.Write(parameter.sosTypeName);
                            output.Write(' ');
                            output.Write(parameter.name);
                        }
                    }
                    output.WriteLine(")");

                    output.WriteLine("        {");

                    String methodString;
                    String callMethodNameString;
                    if (addObjectNameToMethods)
                    {
                        methodString = String.Format("objectName, \"{0}\"", methodDefinition.methodName);
                        callMethodNameString = "CallOnObject";
                    }
                    else
                    {
                        callMethodNameString = "Call";
                        if (npcObjectName == null)
                        {
                            methodString = "\"" + methodDefinition.methodName + "\"";
                        }
                        else
                        {
                            methodString = "\"" + npcObjectName + "." + methodDefinition.methodName + "\"";
                        }
                    }

                    if (returnTypeIsVoid)
                    {
                        output.Write("            npcClientCaller.{0}(typeof(void), {1}", callMethodNameString, methodString);
                    }
                    else
                    {
                        output.Write("            return ({0})npcClientCaller.{1}(typeof({0}), {2}",
                            methodDefinition.returnSosTypeName, callMethodNameString, methodString);
                    }

                    if (methodDefinition.parameters != null)
                    {
                        foreach (SosMethodDefinition.Parameter parameter in methodDefinition.parameters)
                        {
                            output.Write(", ");
                            output.Write(parameter.name);
                        }
                    }
                    output.WriteLine(");");

                    output.WriteLine("        }");
                }
            }

            output.WriteLine("    }");
        }

    }
}
