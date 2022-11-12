using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using Octodiff.Tests.Util;

namespace Octodiff.Tests
{
    [TestFixture]
    public class PatchFixture : CommandLineFixture
    {
        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void PatchingShouldResultInPerfectCopy(string name, int numberOfFiles)
        {
            string newName = RegisterFile(Path.ChangeExtension(name, "2.zip"));
            string copyName = RegisterFile(Path.ChangeExtension(name, "2_out.zip"));
            PackageGenerator.GeneratePackage(RegisterFile(name), numberOfFiles);
            PackageGenerator.ModifyPackage(name, newName, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));

            string sigName = RegisterFile(name + ".sig");
            string deltaName = RegisterFile(name + ".delta");
            Run("signature " + name + " " + sigName);
            Run("delta " + sigName + " " + newName + " " + deltaName);
            Run("patch " + name + " " + deltaName + " " + copyName);
            Assert.That(ExitCode, Is.EqualTo(0));

            Assert.That(Sha1(newName), Is.EqualTo(Sha1(copyName)));
        }

        [Test]
        [TestCase("SmallPackage1mb.zip", 10)] // temp disable this passes locally but fails in appveyor?
        [TestCase("SmallPackage10mb.zip", 100)]
        public void PatchVerificationShouldFailWhenFilesModified(string name, int numberOfFiles)
        {
            string newBasis = RegisterFile(Path.ChangeExtension(name, "1.zip"));
            string newName = RegisterFile(Path.ChangeExtension(name, "2.zip"));
            string copyName = RegisterFile(Path.ChangeExtension(name, "2_out.zip"));
            PackageGenerator.GeneratePackage(RegisterFile(name), numberOfFiles);
            PackageGenerator.ModifyPackage(name, newBasis, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));
            PackageGenerator.ModifyPackage(name, newName, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));

            string sigName = RegisterFile(name + ".sig");
            string deltaName = RegisterFile(name + ".delta");
            Run("signature " + name + " " + sigName);
            Run("delta " + sigName + " " + newName + " " + deltaName);
            Run("patch " + newBasis + " " + deltaName + " " + copyName);
            Assert.That(ExitCode, Is.EqualTo(4));
            Assert.That(Output, Does.Contain("Error: Verification of the patched file failed"));
        }

        [Test]
        [TestCase("SmallPackage10mb.zip", 100)]
        public void PatchVerificationCanBeSkipped(string name, int numberOfFiles)
        {
            var newBasis = RegisterFile(Path.ChangeExtension(name, "1.zip"));
            var newName = RegisterFile(Path.ChangeExtension(name, "2.zip"));
            var copyName = RegisterFile(Path.ChangeExtension(name, "2_out.zip"));
            PackageGenerator.GeneratePackage(RegisterFile(name), numberOfFiles);
            PackageGenerator.ModifyPackage(name, newBasis, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));
            PackageGenerator.ModifyPackage(name, newName, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));

            string sigFile = RegisterFile(name + ".sig");
            string deltaFile = RegisterFile(name + ".delta");
            Run("signature " + name + " " + sigFile);
            Run("delta " + sigFile + " " + newName + " " + deltaFile);
            Run("patch " + newBasis + " " + deltaFile + " " + copyName + " --skip-verification");
            Assert.That(ExitCode, Is.EqualTo(0));
            Assert.That(Sha1(newName), Is.Not.EqualTo(Sha1(copyName)));
        }

        static string Sha1(string fileName)
        {
            using (var s = new FileStream(fileName, FileMode.Open))
            using (var sha = SHA1.Create())
            {
                return Convert.ToHexString(sha.ComputeHash(s));
            }
        }
    }
}