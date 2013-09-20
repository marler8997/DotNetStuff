using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.Build.Framework;

namespace More.Pdl
{
    class InputFileObject
    {
        public readonly String name;
        public readonly String contents;
        public InputFileObject(String name, Byte[] readBuffer, StringBuilder builder, Sha1 sha, Encoding encoding)
        {
            this.name = name;

            // Read the file
            builder.Length = 0;
            using (FileStream stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                while (true)
                {
                    Int32 bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                    if (bytesRead <= 0) break;
                    sha.Add(readBuffer, 0, bytesRead);

                    String str = encoding.GetString(readBuffer, 0, bytesRead);
                    builder.Append(str);
                }
            }
            this.contents = builder.ToString();
        }
    }

    public class PdlCodeGeneration : ITask
    {
        const Boolean TaskFailed = false;
        const Boolean TaskSucceeded = true;

        IBuildEngine buildEngine;
        ITaskHost taskHost;

        String[] inputFiles;
        String outputFile;
        String @namespace;

        Boolean forceCodeGeneration = false;

        public IBuildEngine BuildEngine
        {
            get { return buildEngine; }
            set { this.buildEngine = value; }
        }
        public ITaskHost HostObject
        {
            get { return taskHost; }
            set { this.taskHost = value; }
        }

        [Required]
        public String[] InputFiles
        {
            get { return inputFiles; }
            set { this.inputFiles = value; }
        }
        [Required]
        public String OutputFile
        {
            get { return outputFile; }
            set { this.outputFile = value; }
        }
        [Required]
        public String Namespace
        {
            get { return @namespace; }
            set { this.@namespace = value; }
        }
        public Boolean ForceCodeGeneration
        {
            get { return forceCodeGeneration; }
            set { this.forceCodeGeneration = value; }
        }
        
        void Log(MessageImportance importance, String message)
        {
            buildEngine.LogMessageEvent(new BuildMessageEventArgs(message, null, null, importance));
        }
        void Log(MessageImportance importance, String fmt, params Object[] obj)
        {
            Log(importance, String.Format(fmt, obj));
        }

        void LogError(String message)
        {
            buildEngine.LogErrorEvent(new BuildErrorEventArgs(null, null, null, 0, 0, 0, 0, message, null, null));
        }
        void LogError(String fmt, params Object[] obj)
        {
            LogError(String.Format(fmt, obj));
        }

        public UInt32[] TryGetSavedInputHash(String outputFile, out String savedInputHashString)
        {
            if (!File.Exists(outputFile)) { savedInputHashString = null; return null; }

            String firstLine;
            using (StreamReader reader = new StreamReader(new FileStream(outputFile, FileMode.Open, FileAccess.Read, FileShare.Read), true))
            {
                firstLine = reader.ReadLine();
            }
            if (firstLine == null) { savedInputHashString = null; return null; } // Output file is empty

            Int32 inputShaIndex = firstLine.IndexOf(PdlFile.InputShaPrefix);
            if (inputShaIndex < 0) { savedInputHashString = null; return null; } // First line does not contain hash

            inputShaIndex += PdlFile.InputShaPrefix.Length;
            savedInputHashString = firstLine.Substring(inputShaIndex);
            if (savedInputHashString.Length != PdlFile.ShaHexStringLength)
            {
                buildEngine.LogWarningEvent(new BuildWarningEventArgs(null, null, outputFile, 1, inputShaIndex, 1, firstLine.Length,
                    String.Format("Expected the InputSha of the output file to be 40 characters but it was {0}", savedInputHashString.Length), null, null));
                savedInputHashString = null;
                return null;
            }

            try
            {
                UInt32[] savedInputHash;
                Sha1.Parse(savedInputHashString, 0, out savedInputHash);
                return savedInputHash;
            }
            catch (Exception e)
            {
                buildEngine.LogWarningEvent(new BuildWarningEventArgs(null, null, outputFile, 1, inputShaIndex, 1, firstLine.Length,
                    String.Format("Exception occured while parsing InputSha of the output file '{0}': {1}", savedInputHashString, e.Message), null, null));
                return null;
            }
        }

        public Boolean Execute()
        {
            //
            // Check Options
            //
            if (buildEngine == null) throw new ArgumentNullException("BuildEngine");


            if (inputFiles == null || inputFiles.Length <= 0)
            {
                LogError("Missing InputFiles");
                return TaskFailed;
            }
            if (String.IsNullOrEmpty(outputFile))
            {
                LogError("Missing OutputFile");
                return TaskFailed;
            }
            if (String.IsNullOrEmpty(@namespace))
            {
                LogError("Missing Namespace");
                return TaskFailed;
            }

            //
            // Check that input files exist
            //
            Int32 missingInputFileCount = 0;
            for (int i = 0; i < inputFiles.Length; i++)
            {
                String inputFile = inputFiles[i];
                if (!File.Exists(inputFiles[i]))
                {
                    missingInputFileCount++;
                    LogError("Missing InputFile '{0}'", inputFile);
                }
            }
            if (missingInputFileCount > 0)
            {
                LogError("{0} of the input files {1} missing", missingInputFileCount, (missingInputFileCount == 1) ? "is" : "are");
                return TaskFailed;
            }

            //
            // Load the input files
            //
            InputFileObject[] inputFileObjects = new InputFileObject[inputFiles.Length];

            Byte[] fileBuffer = new Byte[1024];
            Sha1 inputSha = new Sha1();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < inputFileObjects.Length; i++)
            {
                inputFileObjects[i] = new InputFileObject(inputFiles[i], fileBuffer, builder, inputSha, Encoding.UTF8);
            }
            UInt32[] inputHash = inputSha.Finish();
            String inputHashString = Sha1.HashString(inputHash);


            if (forceCodeGeneration)
            {
                Log(MessageImportance.Normal, "Skipping the InputHash check because ForceCodeGeneration is set to true");
            }
            else
            {
                //
                // Try to get the saved hash from output file
                //
                String savedInputHashString;
                UInt32[] savedInputHash = TryGetSavedInputHash(outputFile, out savedInputHashString);
                if (savedInputHash != null)
                {
                    if (Sha1.Equals(inputHash, savedInputHash))
                    {
                        Log(MessageImportance.Normal, "Input hash matches saved input hash, no code generation done");
                        return TaskSucceeded;
                    }
                }
            }

            //
            // Parse Pdl Files
            //
            PdlFile pdlFile = new PdlFile();
            for (int i = 0; i < inputFileObjects.Length; i++)
            {
                InputFileObject inputFileObject = inputFileObjects[i];
                PdlFileParser.ParsePdlFile(pdlFile, new LfdReader(new StringReader(inputFileObject.contents)));
            }

            //
            // Generate the code
            //
            StringBuilder outputStringBuilder = new StringBuilder();

            using (StringWriter outputStringWriter = new StringWriter(outputStringBuilder))
            {
                // Save the hash first
                outputStringWriter.WriteLine("// {0}{1}", PdlFile.InputShaPrefix, inputHashString);
                PdlCodeGenerator.GenerateCode(outputStringWriter, pdlFile, @namespace);
            }

            String outputContents = outputStringBuilder.ToString();

            FileExtensions.SaveStringToFile(outputFile, FileMode.Create, outputContents);

            return TaskSucceeded;
        }
    }
}
