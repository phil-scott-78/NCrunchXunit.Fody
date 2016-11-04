using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NCrunch.Framework;
using Shouldly;
using Xunit;

namespace Tests
{
    public class WeaverTests : IClassFixture<FodyFixture>
    {
        readonly Assembly _assembly;
        readonly string _newAssemblyPath;
        readonly string _assemblyPath;

        public WeaverTests(FodyFixture fixture)
        {
            _assembly = fixture.Assembly;
            _newAssemblyPath = fixture.NewAssemblyPath;
            _assemblyPath = fixture.AssemblyPath;
        }

        [Fact]
        public void Classes_with_facts_are_marked_as_exclusive_to_themself()
        {
            var type = _assembly.GetType("AssemblyToProcess.ClassWithFacts");
            var attribute = type.GetCustomAttributes(true).OfType<ExclusivelyUsesAttribute>().First();
            attribute.ResourceNames.First().ShouldBe("AssemblyToProcess.ClassWithFacts");
        }

        [Fact]
        public void Classes_with_collection_are_marked_as_exclusive_to_the_collection()
        {
            var type = _assembly.GetType("AssemblyToProcess.ClassWithCollection1");
            var attribute = type.GetCustomAttributes(true).OfType<ExclusivelyUsesAttribute>().First();
            attribute.ResourceNames.First().ShouldBe("SomeCollection");
        }

#if(DEBUG)
        [Fact]
        public void PeVerify()
        {
            Verifier.Verify(_assemblyPath,_newAssemblyPath);
        }
#endif
    }

    public class FodyFixture : IDisposable
    {

        public Assembly Assembly;
        public string NewAssemblyPath;
        public string AssemblyPath;
    
        public FodyFixture()
        {
            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
            AssemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif

            NewAssemblyPath = AssemblyPath.Replace(".dll", "2.dll");
            File.Copy(AssemblyPath, NewAssemblyPath, true);

            var moduleDefinition = ModuleDefinition.ReadModule(NewAssemblyPath);
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = new DefaultAssemblyResolver()
            };

            weavingTask.Execute();
            moduleDefinition.Write(NewAssemblyPath);

            Assembly = Assembly.LoadFile(NewAssemblyPath);
        }

        public void Dispose()
        {
        }
    }
}