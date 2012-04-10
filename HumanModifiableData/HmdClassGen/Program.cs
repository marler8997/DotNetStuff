using System;
using System.IO;

namespace Marler.Hmd
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.WriteLine("Please supply an hmd properties file");
                return -1;
            }
            if (args.Length > 1)
            {
                Console.WriteLine("Too many arguments...please only supply an hmd properties file");
                return -1;
            }
            String hmdPropertiesFile = args[0];

            HmdProperties hmdProperties = HmdFileParser.ParsePropertiesFile(
                hmdPropertiesFile, Path.GetDirectoryName(hmdPropertiesFile));
            hmdProperties.ResolveChildParentReferences();

            CodeGenerator codeGenerator = new CodeGenerator(CSharpLanguageGenerator.Instance, Console.Out, 
                "DefaultHmdNamespace", "DefaultRootClassName", "HmdType");

            codeGenerator.Generate(hmdProperties);

            return 0;
        }
    }
}
