using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Octopus.Platform.Util;

namespace Octodiff.Tests.Util
{
    public abstract class CommandLineFixture
    {
        private List<string> _filesToDelete = new();

        protected string StdErr { get; private set; }
        protected string StdOut { get; private set; }
        protected string Output { get; private set; }
        protected int ExitCode { get; set; }

        public void Run(string args)
        {
            var stdErrBuilder = new StringBuilder();
            var stdOutBuilder = new StringBuilder();
            var outputBuilder = new StringBuilder();
            var path = GetExePath();

            args = $"{path} {args}";
            path = "dotnet";

            var exit = SilentProcessRunner.ExecuteCommand(path,
                args,
                GetCurrentDirectory(),
                output =>
                {
                    stdOutBuilder.AppendLine(output);
                    outputBuilder.AppendLine(output);
                    Trace.WriteLine(output);
                },
                output =>
                {
                    stdErrBuilder.AppendLine(output);
                    outputBuilder.AppendLine(output);
                    Trace.WriteLine(output);
                });

            StdErr = stdErrBuilder.ToString();
            StdOut = stdOutBuilder.ToString();
            Output = outputBuilder.ToString();
            ExitCode = exit;
        }

        string GetExePath()
        {
            return Path.Combine(Path.GetDirectoryName(new Uri(typeof(CommandLineFixture).GetTypeInfo().Assembly.Location).LocalPath), "Octodiff.Tests.dll");
        }

        string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        protected string RegisterFile(string path)
        {
            _filesToDelete.Add(path);
            return path;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (string file in _filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }
        }
    }
}
