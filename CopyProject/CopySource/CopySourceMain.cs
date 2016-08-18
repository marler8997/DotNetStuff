using System;
using System.Collections.Generic;
using System.IO;

using More;

namespace CopySource
{
    class CopySourceOptions : CLParser
    {
        public readonly CLStringArgument namespaceChange;

        public CopySourceOptions()
        {
            namespaceChange = new CLStringArgument('n', "namespace", "Change the namespace (OldNamespace.Library:NewNamespace.NewLibrary)");
            Add(namespaceChange);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("CopySource [options] <source-file> <dest-file>");
        }
    }
    class CopySourceMain
    {
        static Int32 Main(String[] args)
        {
            CopySourceOptions options = new CopySourceOptions();
            List<String> nonOptionArgs = options.Parse(args);

            if (nonOptionArgs.Count != 2)
            {
                return options.ErrorAndUsage("Expected 2 non-option arguments but got {0}", nonOptionArgs.Count);
            }
            String sourceFile = nonOptionArgs[0];
            String destFile = nonOptionArgs[1];

            if (!File.Exists(sourceFile))
            {
                Console.WriteLine("Error: source file '{0}' does not exist", sourceFile);
                return 1;
            }

            // TODO: how to handle if the destination file/directory do or do not exist

            if (options.namespaceChange.set)
            {
                String oldNamespace, newNamespace;
                String oldNamespaceWithTrailingDot, newNamespaceWithTrailingDot;

                String namespaceChangeString = options.namespaceChange.ArgValue;
                Int32 colonIndex = namespaceChangeString.IndexOf(':');
                if (colonIndex < 0)
                {
                    Console.WriteLine("Namespace change option should contain a colon to seperate the namespaces.");
                    return 1;
                }
                oldNamespace = namespaceChangeString.Remove(colonIndex);
                newNamespace = namespaceChangeString.Substring(colonIndex + 1);

                oldNamespaceWithTrailingDot = oldNamespace + ".";
                newNamespaceWithTrailingDot = newNamespace + ".";

                String fileContents = FileExtensions.ReadFileToString(sourceFile);

                //
                // Make Replacements
                //
                fileContents = fileContents.Replace("namespace " + oldNamespaceWithTrailingDot, "namespace " + newNamespaceWithTrailingDot);
                fileContents = fileContents.Replace("namespace " + oldNamespace, "namespace " + newNamespace);

                fileContents = fileContents.Replace("using " + oldNamespaceWithTrailingDot, "using " + newNamespaceWithTrailingDot);
                fileContents = fileContents.Replace("using " + oldNamespace, "using " + newNamespace);

                fileContents = fileContents.Replace(oldNamespaceWithTrailingDot, newNamespaceWithTrailingDot);

                FileExtensions.SaveStringToFile(destFile, FileMode.Create, fileContents);
            }
            else
            {
                File.Copy(sourceFile, destFile, true);
            }

            return 0;
        }
    }
}
