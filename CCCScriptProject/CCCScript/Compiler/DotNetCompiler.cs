using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace QuickCodeApp.Compiler
{
    public class DotNetCompiler
    {
        public static CompilerResults Compile(string sourceCode,
                            string language,
                            string version,
                            string[] assemblyFiles,
                            string compileOptions,
                            string[] embeddedResource,
                            string[] linkedResources,
                            string mainClass,
                            bool generateExecutable,
                            bool generateInMemory,
                            string outputAssembly)
        {
            var param = new CompilerParameters
                            {
                                GenerateExecutable = generateExecutable,
                                GenerateInMemory = generateInMemory,
                                OutputAssembly = outputAssembly,
                                CompilerOptions = compileOptions,
                                MainClass = mainClass,
                            };
            param.EmbeddedResources.AddRange(embeddedResource);
            param.LinkedResources.AddRange(linkedResources);
            param.ReferencedAssemblies.AddRange(assemblyFiles);

            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", version } };
            var provider = CodeDomProvider.CreateProvider(language, providerOptions);
            return provider.CompileAssemblyFromSource(param, sourceCode);
        }

        public static CompilerResults CompileToExe(string sourceCode, string[] assemblyFiles, string output, bool generateInMemory = false, string mainClass = "Main", string language = "CSharp", string version = "v4.0", string compileOptions = "")
        {
            var param = new CompilerParameters
            {
                GenerateExecutable = true,
                GenerateInMemory = generateInMemory,
                OutputAssembly = output,
                MainClass = mainClass,
                CompilerOptions = compileOptions
            };
            param.ReferencedAssemblies.AddRange(assemblyFiles);

            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
            var provider = CodeDomProvider.CreateProvider("Csharp", providerOptions);
            return provider.CompileAssemblyFromSource(param, sourceCode);
        }

        public static CompilerResults CompileToDll(string sourceCode, string[] assemblyFiles, string output, bool generateInMemory = false, string language = "CSharp", string version = "v4.0", string compileOptions = "")
        {
            var param = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = generateInMemory,
                OutputAssembly = output,
                CompilerOptions = compileOptions
            };
            param.ReferencedAssemblies.AddRange(assemblyFiles);

            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
            var provider = CodeDomProvider.CreateProvider("Csharp", providerOptions);
            return provider.CompileAssemblyFromSource(param, sourceCode);
        }

        public static CompilerResults CompileToAssembly(string sourceCode, string[] assemblyFiles, string language = "CSharp", string version = "v4.0", string compileOptions = "")
        {
            var param = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = false,
                CompilerOptions = compileOptions
            };
            param.ReferencedAssemblies.AddRange(assemblyFiles);

            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
            var provider = CodeDomProvider.CreateProvider(language, providerOptions);
            return provider.CompileAssemblyFromSource(param, sourceCode);
        }
    }
}
