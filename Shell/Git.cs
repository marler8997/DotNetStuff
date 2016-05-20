using System;
using System.Diagnostics;

namespace More.Shell
{
    public static class Git
    {
        /*
        static ILogger logger;
        public static void Initialize(ILogger logger)
        {
            Git.logger = logger;
        }
        */
        static DataReceivedEventHandler StdOutHandler;
        static DataReceivedEventHandler StdErrHandler;
        public static void SetStdOut(DataReceivedEventHandler handler)
        {
            StdOutHandler = handler;
        }
        public static void SetStdErr(DataReceivedEventHandler handler)
        {
            StdErrHandler = handler;
        }


        static Process CreateGitProcess(String gitDirectory)
        {
            Process process = new Process();
            process.StartInfo.FileName = "git";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = gitDirectory;

            // For some reason this environment variable gets added and it messes things up with git
            process.StartInfo.EnvironmentVariables.Remove("GIT_DIR");

            if (StdOutHandler != null)
            {
                process.OutputDataReceived += StdOutHandler;
            }
            if (StdErrHandler != null)
            {
                process.OutputDataReceived += StdErrHandler;
            }
            return process;
        }
        static void RunGitProcess(this Process process)
        {
            //logger.Log("[Builder] Executing \"{0} {1}\"", process.StartInfo.FileName, process.StartInfo.Arguments);
            process.Start();
            if (StdOutHandler != null)
            {
                process.BeginOutputReadLine();
            }
            if (StdErrHandler != null)
            {
                process.BeginErrorReadLine();
            }
            process.WaitForExit();
        }
        static void RunGitProcessAndEnforceExitCode(this Process process)
        {
            RunGitProcess(process);
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(String.Format("Command '{0} {1}' exited with code {2}",
                    process.StartInfo.FileName, process.StartInfo.Arguments, process.ExitCode));
            }
        }

        public static void Status(String gitDirectory)
        {
            using (Process process = CreateGitProcess(gitDirectory))
            {
                process.StartInfo.Arguments = "status";
                process.RunGitProcess();
            }
        }
        public static void Checkout(String gitDirectory, String commit)
        {
            using (Process process = CreateGitProcess(gitDirectory))
            {
                process.StartInfo.Arguments = String.Format("checkout {0}", commit);
                process.RunGitProcessAndEnforceExitCode();
            }
        }

        /*
        public static void ProcessStandardOutputReceived(Object sender, DataReceivedEventArgs e)
        {
            logger.Log("stdout: " + e.Data);
        }
        public static void ProcessStandardErrorReceived(Object sender, DataReceivedEventArgs e)
        {
            logger.Log("stderr: " + e.Data);
        }
        */
    }
}