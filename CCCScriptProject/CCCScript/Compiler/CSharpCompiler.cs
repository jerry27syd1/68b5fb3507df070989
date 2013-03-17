using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace QuickCodeApp.Compiler
{
    public class CSharpCompiler 
    {
        private List<string> AssemblySourceList { get; set; }
        public string Entry { get; set; }
        public string Method { get; set; }
        public string Using { get; set; }
        public string Class { get; set; }
        public string AssemblySources { get; set; }
        public string Namespace { get; set; }
        public string CompilerVersion { get; set; }
        public string Language { get; set; }

        public string SourceCode
        {
            get { return _sourceCode; }
            set { _sourceCode = value; }
        }

        private string _sourceCode =
@"/*USING*/
/*NAMESPACE*/
namespace EntryPointNameSpace
{
    /*CLASS*/
    public class EntryPoint 
    {
        public static string result = """";
        /*METHOD*/
        public object Main()
        {
            try
            {
               /*ENTRY*/
            }
            catch (Exception ex)
            {
                return ex.Message + "":"" + ex.StackTrace;
            }
            return result;
        }

        public string clip{
            get{ return (String)Clipboard.GetDataObject().GetData(DataFormats.Text); } 
        }
    }
}";
      
        public CSharpCompiler()
        {
            Language = "CSharp";
        }

        public static CompilerResults Compile(string source, string[] assemblySources, string language = "CSharp", string version = "v4.0")
        {
            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", version } };
            var provider = CodeDomProvider.CreateProvider(language, providerOptions);
            var parms = new CompilerParameters { GenerateExecutable = false, GenerateInMemory = true, IncludeDebugInformation = false };
       
            foreach (var sources in assemblySources)
            {
                parms.ReferencedAssemblies.Add(sources);
            }
            return provider.CompileAssemblyFromSource(parms, source);
        }

        public string BuildCSharpCode()
        {
            _sourceCode = Regex.Replace(_sourceCode, "/\\*USING\\*/", Using ?? "");
            _sourceCode = Regex.Replace(_sourceCode, "/\\*NAMESPACE\\*/", Namespace ?? "");
            _sourceCode = Regex.Replace(_sourceCode, "/\\*CLASS\\*/", Class ?? "");
            _sourceCode = Regex.Replace(_sourceCode, "/\\*METHOD\\*/", Method ?? "");
            _sourceCode = Regex.Replace(_sourceCode, "/\\*ENTRY\\*/", Entry ?? "");
            return _sourceCode;
        }


        public string CompileAndRun()
        {
            var generatedTemplate = string.Empty;
            var result = Compile(BuildCSharpCode(), Regex.Split(AssemblySources, Environment.NewLine));
            if (result.Errors.Count == 0)
            {
                dynamic t = result.CompiledAssembly.CreateInstance("EntryPointNameSpace.EntryPoint");
                if (t != null)
                {
                    var output = t.Main();

                    if (output is string)
                    {
                        generatedTemplate = output;
                    }
                    else
                    {
                        using (var log = new StringWriter())
                        {
                            ObjectDumper.Write(output, int.MaxValue, log);
                            generatedTemplate = log.ToString();
                        }
                    }
                }
                else
                {
                    generatedTemplate = "EntryPointNameSpace.EntryPoint";
                    throw new Exception(generatedTemplate);
                }
            }
            else
            {
                foreach (CompilerError err in result.Errors)
                {
                    generatedTemplate += err.ErrorText + " " + err.Line.ToString() + Environment.NewLine;
                }

            }
            return generatedTemplate;
        }

        private static string ReadComplierVersionFromEntryPointCs(string fileName)
        {
            var result = ReadFile(fileName,"/*COMPILER_VERSION", "COMPILER_VERSION*/");
            return result.Count > 0 ? result[0] : null;
        }

        private static List<string> ReadAssemblySourcesFromEntryPointCs(string fileName)
        {
            return ReadFile(fileName, "/*DLL", "DLL*/");
        }

        public static List<string> ReadFile(string fileName, string startBlock, string endBlock)
        {
            var resultList = new List<string>();
            var file = File.ReadAllText(fileName);
            var startBlockWithNewLine = startBlock + Environment.NewLine;
            var fileStart = file.IndexOf(startBlockWithNewLine) + startBlockWithNewLine.Length;
            if (fileStart != -1)
            {
                using (var reader = new StringReader(file.Substring(fileStart, file.Length - fileStart)))
                {
                    for (; ; )
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        line = line.Trim();
                        if (line.EndsWith(endBlock))
                        {
                            break;
                        }
                        resultList.Add(line.Trim());
                    }
                }
                return resultList;
            }
            return null;
        }

    }
}
