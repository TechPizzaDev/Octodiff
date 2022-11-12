using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Octodiff.Tests.Util;

namespace Octodiff.Tests
{
    [TestFixture]
    public class TimingsFixture : CommandLineFixture
    {
        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void ExecuteWithTimings(string name, int numberOfFiles)
        {
            string newName = RegisterFile(Path.ChangeExtension(name, "2.zip"));
            string copyName = RegisterFile(Path.ChangeExtension(name, "2_out.zip"));
            string sigName = RegisterFile(name + ".sig");
            string deltaName = RegisterFile(name + ".delta");

            Time("Package creation", () => PackageGenerator.GeneratePackage(RegisterFile(name), numberOfFiles));
            Time("Package modification", () => PackageGenerator.ModifyPackage(name, newName, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles)));
            Time("Signature creation", () => Run("signature " + name + " " + sigName));
            Time("Delta creation", () => Run("delta " + sigName + " " + newName + " " + deltaName));
            Time("Patch application", () => Run("patch " + name + " " + deltaName + " " + copyName));
            Time("Patch application (no verify)", () => Run("patch " + name + " " + deltaName + " " + copyName + " --skip-verification"));
        }

        static void Time(string task, Action callback)
        {
            var watch = Stopwatch.StartNew();
            callback();
            Trace.WriteLine(task.PadRight(30, ' ') + ": " + watch.ElapsedMilliseconds + "ms");
        }
    }
}