using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.BuildEngine;

using More;

namespace CopyProject
{
    class PropertyChange
    {
        public readonly String name;
        public readonly String value;

        public Int32 changeCount;
        public PropertyChange(String name, String value)
        {
            this.name = name;
            this.value = value;
            changeCount = 0;
        }
    }


    class CopyProjectOptions : CLParser
    {
        public readonly CLStringArgument namespaceChange;
        public readonly CLStringArgument assemblyName;
        public readonly CLStringArgument propChanges;
        public readonly CLStringArgument templateFile;

        public CopyProjectOptions()
        {
            namespaceChange = new CLStringArgument('n', "namespace", "Change the namespace (OldNamespace.Library:NewNamespace.NewLibrary)");
            Add(namespaceChange);

            assemblyName = new CLStringArgument('a', "assembly", "Change the assembly name");
            Add(assemblyName);

            propChanges = new CLStringArgument('p', "props", "Change the semicolon list of properties 'name:value;name:value;...'");
            Add(propChanges);

            templateFile = new CLStringArgument('t', "template", "src:name or dst:name Use a csproj template file ....");
            Add(templateFile);
        }

        public override void PrintUsageHeader()
        {
            Console.WriteLine("CopyProject [options] <source-csproj> <dest-dir-or-csproj>");
        }
    }

    class CopyProjectMain
    {
        static Int32 Main(string[] args)
        {
            CopyProjectOptions options = new CopyProjectOptions();
            List<String> nonOptionArgs = options.Parse(args);

            if (nonOptionArgs.Count != 2)
            {
                return options.ErrorAndUsage("Expected 2 non-option arguments but got {0}", nonOptionArgs.Count);
            }

            //
            // Setup source and dest proj names/paths
            //
            String sourceProj = nonOptionArgs[0];
            if (!File.Exists(sourceProj))
            {
                Console.WriteLine("Source csproj file '{0}' does not exist", sourceProj);
                return 1;
            }
            String sourceProjPath = Path.GetDirectoryName(sourceProj);
            String sourceProjName = Path.GetFileName(sourceProj);

            
            String destProjOrPath = nonOptionArgs[1];
            String destProj, destProjPath;

            if(destProjOrPath.EndsWith(".csproj"))
            {
                destProj = destProjOrPath;
                destProjPath = Path.GetDirectoryName(destProjOrPath);
            }
            else
            {
                destProj = Path.Combine(destProjOrPath, sourceProjName);
                destProjPath = destProjOrPath;
            }

            if (!Directory.Exists(destProjPath))
            {
                Console.WriteLine("Destination Directory '{0}' doesn't exist", destProjPath);
                return 1;
            }


            //
            // Print Settings
            //
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.WriteLine("Settings");
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.WriteLine("Source Proj File : {0}", sourceProj);
            Console.WriteLine("Dest Proj File   : {0}", destProj);





            //
            //
            //
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.WriteLine("Processing source csproj file...");
            Console.WriteLine("-----------------------------------------------------------------------------");

            HashSet<FileResource> compileFiles = new HashSet<FileResource>();

            Project sourceProject = new Project();
            sourceProject.Load(sourceProj);

            BuildItemGroupCollection itemGroups = sourceProject.ItemGroups;

            foreach (BuildItemGroup itemGroup in itemGroups)
            {
                //Console.WriteLine("ItemGroup Count='{0}' Condition='{1}'", itemGroup.Count, itemGroup.Condition);

                foreach (BuildItem item in itemGroup)
                {
                    if (item.Name.Equals("Compile"))
                    {
                        String include = item.Include;
                        String exclude = item.Exclude;

                        if(!String.IsNullOrEmpty(include))
                        {
                            FileResource resource = new FileResource(include, sourceProjPath, destProjPath);
                            if (!File.Exists(resource.sourceRelativeToCWDNameAndPath))
                            {
                                Console.WriteLine("Error: ItemGroup 'Compile' Include '{0}' does not exist (name relative to current working directory is '{1}')",
                                    resource.sourceRelativeToCWDNameAndPath, resource.sourceRelativeToCWDNameAndPath);
                                return 1;
                            }

                            Console.WriteLine("Compile Include '{0}'", resource.relativeToProjNameAndPath);
                            compileFiles.Add(resource);
                        }
                        if(!String.IsNullOrEmpty(exclude))
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Ignoring '{0}'", item.Include);
                    }
                }
            }

            //
            // Load template file if applicable
            //
            Project destProject;
            if (!options.templateFile.set)
            {
                destProject = sourceProject;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine("Loading csproj template file...");
                Console.WriteLine("-----------------------------------------------------------------------------");
                String templateFile = options.templateFile.ArgValue;
                if(templateFile.StartsWith("src:"))
                {
                    templateFile = Path.Combine(sourceProjPath, templateFile.Substring("src:".Length));
                }
                else if(templateFile.StartsWith("dst:"))
                {
                    templateFile = Path.Combine(destProjPath, templateFile.Substring("dst:".Length));
                }

                if (!File.Exists(templateFile))
                {
                    Console.WriteLine("Error: Template file '{0}' does not exist ('{1}')", options.templateFile.ArgValue,
                        templateFile);
                    return 1;
                }

                destProject = new Project();
                destProject.Load(templateFile);

                //
                // Add compile files to template
                //
                BuildItemGroup compileItemGroup = destProject.AddNewItemGroup();
                foreach (FileResource resource in compileFiles)
                {
                    compileItemGroup.AddNewItem("Compile", resource.relativeToProjNameAndPath);
                }
            }



            //
            //
            //
            List<PropertyChange> propertyChanges = new List<PropertyChange>();

            if (options.assemblyName.set)
            {
                propertyChanges.Add(new PropertyChange("AssemblyName", options.assemblyName.ArgValue));
            }

            if (options.propChanges.set)
            {
                String[] changes = options.propChanges.ArgValue.Split(new Char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < changes.Length; i++)
                {
                    String change = changes[i];

                    Int32 colonIndex = change.IndexOf(':');
                    if (colonIndex < 0)
                    {
                        Console.WriteLine("Invalid -p or --props value '{0}', each semi-colon separated change must be a <name>:<value> pair", options.propChanges.ArgValue);
                        return 1;
                    }
                    propertyChanges.Add(new PropertyChange(change.Remove(colonIndex), change.Substring(colonIndex + 1)));
                }
            }



            if (propertyChanges.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine("Modifying source csproj file...");
                Console.WriteLine("-----------------------------------------------------------------------------");

                // Print changes
                for (int i = 0; i < propertyChanges.Count; i++)
                {
                    PropertyChange change = propertyChanges[i];
                    Console.WriteLine("Property '{0}', new value '{1}'", change.name, change.value);
                }


                foreach (BuildPropertyGroup propertyGroup in destProject.PropertyGroups)
                {
                    foreach (BuildProperty property in propertyGroup)
                    {
                        for (int i = 0; i < propertyChanges.Count; i++)
                        {
                            PropertyChange change = propertyChanges[i];

                            if (property.Name.Equals(change.name))
                            {
                                if (property.IsImported)
                                {
                                    Console.WriteLine("WARNING: Property <{0}>{1}</{0}> is imported and cannot be changed", property.Name, property.Value);
                                }
                                else
                                {
                                    Console.WriteLine("Change: Property '{0}' from '{1}' to '{2}'", change.name, property.Value, change.value);
                                    property.Value = change.value;
                                    change.changeCount++;
                                }
                            }
                        }
                    }
                }


                // Check changes
                for (int i = 0; i < propertyChanges.Count; i++)
                {
                    PropertyChange change = propertyChanges[i];
                    if (change.changeCount <= 0)
                    {
                        Console.WriteLine("Error: Did not find any non-imported <{0}> properties to change", change.name);
                        return 1;
                    }

                    Console.WriteLine("Changed {0} <{1}> properties", change.changeCount, change.name);
                }
            }



            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.WriteLine("Saving new csproj file...");
            Console.WriteLine("-----------------------------------------------------------------------------");
            destProject.Save(destProj);



            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.WriteLine("Copying Files...");
            Console.WriteLine("-----------------------------------------------------------------------------");
            

            //
            // Setup copy variables
            //
            String oldDottedNamespace, newDottedNamespace;
            String oldNamespace, newNamespace;

            if(options.namespaceChange.set)
            {
                String namespaceChangeString = options.namespaceChange.ArgValue;
                Int32 colonIndex = namespaceChangeString.IndexOf(':');
                if(colonIndex < 0)
                {
                    Console.WriteLine("Namespace change option should contain a colon to seperate the namespaces.");
                    return 1;
                }
                oldNamespace = namespaceChangeString.Remove(colonIndex);
                newNamespace = namespaceChangeString.Substring(colonIndex + 1);

                oldDottedNamespace = oldNamespace + ".";
                newDottedNamespace = newNamespace + ".";
            }
            else
            {
                oldDottedNamespace = null;
                newDottedNamespace = null;
                oldNamespace = null;
                newNamespace = null;
            }



            //
            // Copy the project file
            //
            foreach (FileResource compileFile in compileFiles)
            {
                //
                // Make any destination directories
                //
                String directory = Path.GetDirectoryName(compileFile.relativeToProjNameAndPath);
                if (!String.IsNullOrEmpty(directory))
                {
                    String destinationRelativeToCWDDirectory = Path.Combine(destProjPath, directory);
                    if (!Directory.Exists(destinationRelativeToCWDDirectory))
                    {
                        Console.WriteLine("Creating destination directory '{0}'", directory);
                        Directory.CreateDirectory(destinationRelativeToCWDDirectory);
                    }
                }

                if (!options.namespaceChange.set)
                {
                    File.Copy(compileFile.sourceRelativeToCWDNameAndPath, compileFile.destRelativeToCWDNameAndPath, true);
                }
                else
                {
                    String fileContents = FileExtensions.ReadFileToString(compileFile.sourceRelativeToCWDNameAndPath);

                    //
                    // Make Replacements
                    //
                    fileContents = fileContents.Replace("namespace " + oldDottedNamespace, "namespace " + newDottedNamespace);
                    fileContents = fileContents.Replace("namespace " + oldNamespace, "namespace " + newNamespace);

                    fileContents = fileContents.Replace("using " + oldDottedNamespace, "using " + newDottedNamespace);
                    fileContents = fileContents.Replace("using " + oldNamespace, "using " + newNamespace);


                    FileExtensions.SaveStringToFile(compileFile.destRelativeToCWDNameAndPath, FileMode.Create, fileContents);
                }
            }



            return 0;
        }
    }
    class FileResource
    {
        public readonly String relativeToProjNameAndPath;
        public readonly String sourceRelativeToCWDNameAndPath;
        public readonly String destRelativeToCWDNameAndPath;

        public FileResource(String relativeToProjNameAndPath, String sourceProjPath, String destProjPath)
        {
            this.relativeToProjNameAndPath = relativeToProjNameAndPath;
            this.sourceRelativeToCWDNameAndPath = Path.Combine(sourceProjPath, relativeToProjNameAndPath);
            this.destRelativeToCWDNameAndPath = Path.Combine(destProjPath, relativeToProjNameAndPath);
        }
    }

}
