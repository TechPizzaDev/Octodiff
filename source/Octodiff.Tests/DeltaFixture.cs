using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Octodiff.Tests.Util;

namespace Octodiff.Tests
{
    [TestFixture]
    public class DeltaFixture : CommandLineFixture
    {
        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void DeltaOfUnchangedFileShouldResultInJustCopySegment(string name, int numberOfFiles)
        {
            PackageGenerator.GeneratePackage(RegisterFile(name), numberOfFiles);

            string sigName = RegisterFile(name + ".sig");
            Run("signature " + name + " " + sigName);
            Assert.That(ExitCode, Is.EqualTo(0));

            string deltaName = RegisterFile(name + ".delta");
            Run("delta " + sigName + " " + name + " " + deltaName);
            Assert.That(ExitCode, Is.EqualTo(0));

            Run("explain-delta " + name + ".delta");
            Assert.That(Regex.IsMatch(Output, $"^Copy: 0 to ([0-9A-F]+){Environment.NewLine}$"));
            Assert.That(Output, Does.Not.Contain("Data:"));
        }

        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void DeltaOfChangedFileShouldResultInNewDataSegments(string name, int numberOfFiles)
        {
            PackageGenerator.GeneratePackage(RegisterFile(name), numberOfFiles);

            string sigName = RegisterFile(name + ".sig");
            Run("signature " + name + " " + sigName);
            Assert.That(ExitCode, Is.EqualTo(0));

            string newName = RegisterFile(Path.ChangeExtension(name, "2.zip"));
            PackageGenerator.ModifyPackage(name, newName, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));

            string deltaName = RegisterFile(name + ".delta");
            Run("delta " + sigName + " " + newName + " " + deltaName);
            Assert.That(ExitCode, Is.EqualTo(0));

            Run("explain-delta " + deltaName);
            Assert.That(Regex.IsMatch(Output, $"Copy: ([0-9A-F]+) to ([0-9A-F]+){Environment.NewLine}"));
            Assert.That(Regex.IsMatch(Output, "Data: \\(([0-9]+) bytes\\)"));

            var originalSize = new FileInfo(name).Length;
            var newSize = new FileInfo(newName).Length;
            var deltaSize = new FileInfo(deltaName).Length;
            var actualDifference = Math.Abs(newSize - originalSize);
            var deltaToActualRatio = (double)deltaSize / actualDifference;
            Trace.WriteLine(string.Format("Delta ratio: {0:n3}", deltaToActualRatio));
            Assert.IsTrue(deltaSize * 2 < newSize, "Delta should be at least half the new file size");
            Assert.IsTrue(0.80 <= deltaToActualRatio && deltaToActualRatio <= 1.60, "Delta should be pretty close to the actual file differences");
        }
    }
}