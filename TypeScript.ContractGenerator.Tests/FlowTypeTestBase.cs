using System;
using System.IO;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using SkbKontur.TypeScript.ContractGenerator.CodeDom;
using SkbKontur.TypeScript.ContractGenerator.Internals;

namespace SkbKontur.TypeScript.ContractGenerator.Tests
{
    [TestFixture(JavaScriptTypeChecker.Flow)]
    [TestFixture(JavaScriptTypeChecker.TypeScript)]
    public abstract class FlowTypeTestBase
    {
        protected FlowTypeTestBase(JavaScriptTypeChecker javaScriptTypeChecker)
        {
            filesGenerationContext = FilesGenerationContext.Create(javaScriptTypeChecker);
        }

        protected string[] GenerateCode(Type rootType)
        {
            return GenerateCode(CustomTypeGenerator.Null, rootType);
        }

        protected string[] GenerateCode(ICustomTypeGenerator customTypeGenerator, Type rootType)
        {
            return GenerateCode(FlowTypeGenerationOptions.Default, customTypeGenerator, rootType);
        }

        protected string[] GenerateCode(FlowTypeGenerationOptions options, ICustomTypeGenerator customTypeGenerator, Type rootType)
        {
            var generator = new FlowTypeGenerator(options, customTypeGenerator, new RootTypesProvider(rootType));
            return generator.Generate().Select(x => x.GenerateCode(new DefaultCodeGenerationContext(JavaScriptTypeChecker))).ToArray();
        }

        protected void GenerateFiles(ICustomTypeGenerator customTypeGenerator, string folderName, params Type[] rootTypes)
        {
            var path = $"{TestContext.CurrentContext.TestDirectory}/{folderName}/{JavaScriptTypeChecker}";
            if (Directory.Exists(path))
                Directory.Delete(path, recursive : true);
            Directory.CreateDirectory(path);

            var generator = new FlowTypeGenerator(FlowTypeGenerationOptions.Default, customTypeGenerator, new RootTypesProvider(rootTypes));
            generator.GenerateFiles(path, JavaScriptTypeChecker);
        }

        protected void CheckDirectoriesEquivalence(string expectedDirectory, string actualDirectory)
        {
            expectedDirectory = $"{TestContext.CurrentContext.TestDirectory}/{expectedDirectory}/{JavaScriptTypeChecker}";
            actualDirectory = $"{TestContext.CurrentContext.TestDirectory}/{actualDirectory}/{JavaScriptTypeChecker}";

            CheckDirectoriesEquivalenceInner(expectedDirectory, actualDirectory);
        }

        private static void CheckDirectoriesEquivalenceInner(string expectedDirectory, string actualDirectory)
        {
            if (!Directory.Exists(expectedDirectory) || !Directory.Exists(actualDirectory))
                Assert.Fail("Both directories should exist");

            var expectedFiles = Directory.EnumerateFiles(expectedDirectory).Select(Path.GetFileName).ToArray();
            var actualFiles = Directory.EnumerateFiles(actualDirectory).Select(Path.GetFileName).ToArray();

            actualFiles.Should().BeEquivalentTo(expectedFiles);

            foreach (var filename in expectedFiles)
            {
                var expected = File.ReadAllText($"{expectedDirectory}/{filename}").Replace("\r\n", "\n");
                var actual = File.ReadAllText($"{actualDirectory}/{filename}").Replace("\r\n", "\n");
                actual.Should().Be(expected);
            }

            var expectedDirectories = Directory.EnumerateDirectories(expectedDirectory).Select(Path.GetFileName).ToArray();
            var actualDirectories = Directory.EnumerateDirectories(actualDirectory).Select(Path.GetFileName).ToArray();

            actualDirectories.Should().BeEquivalentTo(expectedDirectories);

            foreach (var directory in expectedDirectories)
                CheckDirectoriesEquivalenceInner($"{expectedDirectory}/{directory}", $"{actualDirectory}/{directory}");
        }

        protected string GetExpectedCode(string expectedCodeFilePath)
        {
            return File.ReadAllText(GetFilePath(expectedCodeFilePath)).Replace("\r\n", "\n");
        }

        private string GetFilePath(string filename)
        {
            return $"{TestContext.CurrentContext.TestDirectory}/Files/{filename}.{filesGenerationContext.FileExtension}";
        }

        private readonly FilesGenerationContext filesGenerationContext;
        private JavaScriptTypeChecker JavaScriptTypeChecker => filesGenerationContext.JavaScriptTypeChecker;
    }
}