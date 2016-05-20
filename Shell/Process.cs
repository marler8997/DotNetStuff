using System;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace More.Shell
{
    public static class Proc
    {
        static TextWriter log;
        public static void SetLog(TextWriter log)
        {
            Proc.log = log;
        }

        public static void Exec(String workingDirectory, String program, String args)
        {
            int exitCode = TryExec(workingDirectory, program, args);
            if (exitCode != 0)
            {
                throw new InvalidOperationException(String.Format("Process '{0} {1}' failed with exit code {2}",
                    program, args, exitCode));
            }
        }
        public static int TryExec(String workingDirectory, String program, String args)
        {
            Process process = new Process();
            process.StartInfo.FileName = program;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            if (!workingDirectory.Equals(Environment.CurrentDirectory))
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
                if (log != null)
                {
                    log.WriteLine("Exec Path: {0}'", workingDirectory);
                }
            }

            SubProcessStandardOutputHandler outputHandler = new SubProcessStandardOutputHandler();
            process.OutputDataReceived += outputHandler.StandardOutputReceived;
            process.ErrorDataReceived += outputHandler.StandardErrorReceived;
            if (log != null)
            {
                log.WriteLine("Executing: {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
                log.Flush();
            }
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            outputHandler.Finish();
            return process.ExitCode;
        }
        public static String ExecAndGetOutput(String workingDirectory, String program, String args)
        {
            Process process = new Process();
            process.StartInfo.FileName = program;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            if (!workingDirectory.Equals(Environment.CurrentDirectory))
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
                if (log != null)
                {
                    log.WriteLine("Exec Path: {0}'", workingDirectory);
                }
            }

            SubProcessOutputBuilderHandler outputHandler = new SubProcessOutputBuilderHandler();
            process.OutputDataReceived += outputHandler.StandardOutputReceived;
            process.ErrorDataReceived += outputHandler.StandardErrorReceived;
            if (log != null)
            {
                log.WriteLine("Executing: {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);
                log.Flush();
            }
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            String result = outputHandler.Finish();
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(String.Format("Process '{0} {1}' failed with exit code {2}",
                    program, args, process.ExitCode));
            }
            return result;
        }
        public class SubProcessStandardOutputHandler
        {
            Boolean subProcessHasOutput;
            public void StandardOutputReceived(Object sender, DataReceivedEventArgs e)
            {
                if (!subProcessHasOutput)
                {
                    if (log != null) log.WriteLine("-----------------------------------------------------------");
                    subProcessHasOutput = true;
                }
                if (log != null) log.WriteLine("out: " + e.Data);
            }
            public void StandardErrorReceived(Object sender, DataReceivedEventArgs e)
            {
                if (!subProcessHasOutput)
                {
                    if (log != null) log.WriteLine("-----------------------------------------------------------");
                    subProcessHasOutput = true;
                }
                if (log != null) log.WriteLine("err: " + e.Data);
            }
            public void Finish()
            {
                if (subProcessHasOutput)
                {
                    if (log != null) log.WriteLine("-----------------------------------------------------------");
                }
            }
        }
        public class SubProcessOutputBuilderHandler
        {
            Boolean subProcessHasOutput;
            StringBuilder outputBuilder = new StringBuilder();
            public void StandardOutputReceived(Object sender, DataReceivedEventArgs e)
            {
                if (!subProcessHasOutput)
                {
                    if (log != null) log.WriteLine("-----------------------------------------------------------");
                    subProcessHasOutput = true;
                }
                if (log != null) log.WriteLine("out: " + e.Data);
                outputBuilder.Append(e.Data);
            }
            public void StandardErrorReceived(Object sender, DataReceivedEventArgs e)
            {
                if (!subProcessHasOutput)
                {
                    if (log != null) log.WriteLine("-----------------------------------------------------------");
                    subProcessHasOutput = true;
                }
                if (log != null) log.WriteLine("err: " + e.Data);
            }
            public String Finish()
            {
                if (subProcessHasOutput)
                {
                    if (log != null) log.WriteLine("-----------------------------------------------------------");
                }
                return outputBuilder.ToString();
            }
        }
    }

}