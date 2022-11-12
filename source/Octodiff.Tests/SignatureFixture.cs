using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Octodiff.Tests.Util;

namespace Octodiff.Tests
{
    [TestFixture]
    public class SignatureFixture : CommandLineFixture
    {
        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void ShouldCreateSignature(string name, int numberOfFiles)
        {
            PackageGenerator.GeneratePackage(RegisterFile(name), numberOfFiles);

            string sigName = RegisterFile(name + ".sig");
            Run("signature " + name + " " + sigName);
            Assert.That(ExitCode, Is.EqualTo(0));

            var basisSize = new FileInfo(name).Length;
            var signatureSize = new FileInfo(sigName).Length;
            var signatureSizePercentageOfBasis = signatureSize / (double)basisSize;

            Trace.WriteLine(string.Format("Basis size: {0:n0}", basisSize));
            Trace.WriteLine(string.Format("Signature size: {0:n0}", signatureSize));
            Trace.WriteLine(string.Format("Signature ratio: {0:n3}", signatureSizePercentageOfBasis));
            Assert.IsTrue(0.012 <= signatureSizePercentageOfBasis && signatureSizePercentageOfBasis <= 0.014);
        }

        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void ShouldCreateDifferentSignaturesBasedOnChunkSize(string name, int numberOfFiles)
        {
            PackageGenerator.GeneratePackage(RegisterFile(name), numberOfFiles);

            string sig1 = RegisterFile(name + ".sig.1");
            string sig2 = RegisterFile(name + ".sig.2");
            string sig3 = RegisterFile(name + ".sig.3");
            string sig4 = RegisterFile(name + ".sig.4");
            string sig5 = RegisterFile(name + ".sig.5");

            Run("signature " + name + " " + sig1 + " --chunk-size=128");
            Run("signature " + name + " " + sig2 + " --chunk-size=256");
            Run("signature " + name + " " + sig3 + " --chunk-size=1024");
            Run("signature " + name + " " + sig4 + " --chunk-size=2048");
            Run("signature " + name + " " + sig5 + " --chunk-size=31744");

            Assert.That(Length(sig1) > Length(sig2));
            Assert.That(Length(sig2) > Length(sig3));
            Assert.That(Length(sig3) > Length(sig4));
            Assert.That(Length(sig4) > Length(sig5));
        }

        static long Length(string fileName)
        {
            return new FileInfo(fileName).Length;
        }
    }
}