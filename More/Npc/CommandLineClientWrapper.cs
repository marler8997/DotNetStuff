﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using More;

namespace More
{
    public static class CommandLineClientWrapper
    {
        static void Usage(String programName)
        {
            Console.WriteLine("<InteractiveMode> {0} <server>", programName);
            Console.WriteLine("<CommandLineMode> {0} <server> <interface> <method> <args>", programName);
        }
        public static Int32 Run(String programName, String[] args, UInt16 defaultPort, InterfaceMapping interfaceMapping)
        {
            if (args.Length < 1)
            {
                Usage(programName);
                return 1;
            }

            String serverIPOrHostAndOptionalPort = args[0];

            CommandLineClient client = new CommandLineClient(
                EndPoints.EndPointFromIPOrHostAndOptionalPort(serverIPOrHostAndOptionalPort, defaultPort), interfaceMapping);

            String line;

            //
            // Command line mode
            //
            if (args.Length > 1)
            {
                line = args[1];
                for (int i = 2; i < args.Length; i++)
                {
                    line += " " + args[i];
                }
                client.ProcessLine(line);
                return 0;
            }

            //
            // Interactive Mode
            //
            do
            {
                Console.Write("->");
                line = Console.ReadLine();

            } while (client.ProcessLine(line));

            return 0;
        }
    }
    public class InterfaceMap
    {
        public String userInterface;
        public String npcInterface;
        public InterfaceMap(String userInterface, String npcInterface)
        {
            this.userInterface = userInterface;
            this.npcInterface = npcInterface;
        }
    }
    public class InterfaceMapping
    {
        readonly InterfaceMap[] maps;
        public InterfaceMapping(InterfaceMap[] maps)
        {
            this.maps = maps;
        }
        public String UserToNpcInterface(String userInterface)
        {
            for (int i = 0; i < maps.Length; i++)
            {
                InterfaceMap map = maps[i];
                if (userInterface.Equals(map.userInterface, StringComparison.InvariantCultureIgnoreCase))
                {
                    return map.npcInterface;
                }
            }
            return null;
        }
        public String NpcToUserInterface(String npcInterface)
        {
            for (int i = 0; i < maps.Length; i++)
            {
                InterfaceMap map = maps[i];
                if (npcInterface.Equals(map.npcInterface, StringComparison.InvariantCultureIgnoreCase))
                {
                    return map.userInterface;
                }
            }
            return null;
        }
    }
    class CommandLineClient
    {
        readonly NpcClient client;
        readonly InterfaceMapping interfaceMapping;
        public CommandLineClient(EndPoint serverEndPoint, InterfaceMapping interfaceMapping)
        {
            this.client = new NpcClient(serverEndPoint, false);
            client.UpdateAndVerifyEnumAndObjectTypes();
            this.interfaceMapping = interfaceMapping;
        }

        // return false to discontinue interactive mode
        public Boolean ProcessLine(String line)
        {
            String interfaceOrCommand = line.Peel(out line);
            if (String.IsNullOrEmpty(interfaceOrCommand)) return true;

            String remoteNpcObjectName = interfaceMapping.UserToNpcInterface(interfaceOrCommand);

            //
            // If the first string was not an interface, it must have been a command
            //
            if (remoteNpcObjectName == null)
            {
                return ProcessInteractiveCommand(interfaceOrCommand);
            }
            else
            {
                //
                // Get the method name
                //
                String rawParameters;
                String methodName = line.Peel(out rawParameters);
                if (String.IsNullOrEmpty(methodName))
                {
                    Console.WriteLine("Error: Missing method name");
                    return true;
                }
                String fullMethodName = remoteNpcObjectName + "." + methodName;

                //
                // Execute
                //
                Object o;
                try
                {
                    o = client.CallWithRawParameters(fullMethodName, rawParameters);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return true;
                }

                //
                // Print return value
                //
                IEnumerable<String> strings = o as IEnumerable<String>;
                if (strings != null)
                {
                    foreach (String str in strings)
                    {
                        Console.WriteLine(str);
                    }
                }
                else
                {
                    Console.WriteLine(o);
                }

                return true;
            }
        }

        // returns false if interactive mode should stop
        Boolean ProcessInteractiveCommand(String command)
        {
            if (command.Equals("help", StringComparison.CurrentCultureIgnoreCase))
            {
                //
                // Request methods from server
                //
                Dictionary<String, List<SosMethodDefinition>> methodsGroupedByInterface = new Dictionary<string, List<SosMethodDefinition>>();

                List<SosMethodDefinition> methods = client.GetRemoteMethods(false);
                for (int i = 0; i < methods.Count; i++)
                {
                    SosMethodDefinition method = methods[i];

                    //
                    // Add to interface group
                    //
                    List<SosMethodDefinition> methodsByPrefix;
                    if (!methodsGroupedByInterface.TryGetValue(method.methodPrefix, out methodsByPrefix))
                    {
                        methodsByPrefix = new List<SosMethodDefinition>();
                        methodsGroupedByInterface.Add(method.methodPrefix, methodsByPrefix);
                    }
                    methodsByPrefix.Add(method);
                }

                //
                // Print methods to user
                //
                Console.WriteLine("Format: <Interface> <Method> [<Parameters>...]");
                foreach (KeyValuePair<String, List<SosMethodDefinition>> pair in methodsGroupedByInterface)
                {
                    String prefix = pair.Key;
                    List<SosMethodDefinition> methodsByPrefix = pair.Value;

                    String newPrefix = interfaceMapping.NpcToUserInterface(prefix);
                    if (newPrefix != null)
                    {
                        prefix = newPrefix;
                    }
                    Console.WriteLine(prefix);

                    foreach (SosMethodDefinition method in methodsByPrefix)
                    {
                        Console.Write("   {0}", method.fullMethodName.Substring(method.fullMethodName.LastIndexOf('.') + 1));
                        if (method.parameters != null)
                        {
                            foreach (SosMethodDefinition.Parameter parameter in method.parameters)
                            {
                                Console.Write(" {0}", parameter.name);
                            }
                        }
                        Console.WriteLine();
                    }
                }
            }
            else if (command.Equals("exit", StringComparison.CurrentCultureIgnoreCase) ||
                command.Equals("q", StringComparison.CurrentCultureIgnoreCase) ||
                command.Equals("quit", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }
            else
            {
                Console.WriteLine("Error: Unknown interface/command '{0}'", command);
            }
            return true;
        }
    }
}
