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
                    builder.Append(encoding.GetString(readBuffer, 0, bytesRead));
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
        public String[] InputFiles
        {
            get { return inputFiles; }
            set { this.inputFiles = value; }
        }
        public String OutputFile
        {
            get { return outputFile; }
            set { this.outputFile = value; }
        }
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

        public Byte[] TryGetSavedInputHash(String outputFile, out String savedInputHashString)
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

            Byte[] savedInputHash = new Byte[20];
            try
            {
                savedInputHash.ParseHex(0, savedInputHashString, 0, savedInputHashString.Length);
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

            Byte[] readBuffer = new Byte[512];
            Sha1 inputSha = new Sha1();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < inputFileObjects.Length; i++)
            {
                inputFileObjects[i] = new InputFileObject(inputFiles[i], readBuffer, builder, inputSha, Encoding.UTF8);
            }
            Byte[] inputHash = inputSha.Finish();
            String inputHashString = inputHash.ToHexString(0, Sha1.HashByteLength);


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
                Byte[] savedInputHash = TryGetSavedInputHash(outputFile, out savedInputHashString);
                if (savedInputHash != null)
                {
                    if (Sha1.Equal(inputHash, savedInputHash))
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
            using (StreamWriter outputWriter = new StreamWriter(new FileStream(outputFile, FileMode.Create, FileAccess.Write)))
            {
                // Save the hash first
                outputWriter.WriteLine("// {0}{1}", PdlFile.InputShaPrefix, inputHashString);

                PdlCodeGenerator.GenerateCode(outputWriter, pdlFile, @namespace);
            }

            return TaskSucceeded;
        }
    }
}
