using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace QuickCodeApp.Compiler
{
    public class QuickCodeCompiler
    {
        public string Entry { get; set; }
        public string Method { get; set; }
        public string Using { get; set; }
        public string Class { get; set; }
        public string AssemblySources { get; set; }
        public string Namespace { get; set; }
        public string CompilerVersion { get; set; }
        public string Language { get; set; }
        public string CompilerOptions { get; set; }
        public string SourceCode { get; set; }

        #region CS file template

        private const string DefaultTemplate =
@"/*USING*/
namespace /*NAMESPACEID*/
{
    public class /*CLASSID*/ 
    {
/*MainMethod*/
/*METHOD*/
    }
/*CLASS*/
}
/*NAMESPACE*/";

        private const string DefaultMainMethod =
@"      
        public static string result = """";
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
        }";

        private const string DefaultStaticMainMethod =
@"      
        public static string result = """";
        public static void Main(string[] args)
        {
            try
            {
               /*ENTRY*/
            }
            catch (Exception ex)
            {
                result = ex.Message + "":"" + ex.StackTrace;
            }
        }";

        #endregion

        public QuickCodeCompiler(bool useStaticMain = false)
        {
            SourceCode = GetDefaultTemplate(useStaticMain);
        }

        public string GetDefaultTemplate(bool useStaticMain, string nameSpaceId = "", string classId = "")
        {
            var id = Guid.NewGuid().ToString("N");
            var nameSpace = nameSpaceId != string.Empty ? nameSpaceId : "NameSpace" + id;
            var className = classId != string.Empty ? classId : "Class" + id;
            var template = Regex.Replace(DefaultTemplate, "/\\*NAMESPACEID\\*/", nameSpace);
            template = Regex.Replace(template, "/\\*CLASSID\\*/", className);
            template = useStaticMain
                              ? Regex.Replace(template, "/\\*MainMethod\\*/", DefaultStaticMainMethod)
                              : Regex.Replace(template, "/\\*MainMethod\\*/", DefaultMainMethod);


            return template;
        }

        public string BuildCSharpCode()
        {
            var sourceCode = SourceCode;
            sourceCode = Regex.Replace(sourceCode, "/\\*USING\\*/", Using ?? "");
            sourceCode = Regex.Replace(sourceCode, "/\\*NAMESPACE\\*/", Namespace ?? "");
            sourceCode = Regex.Replace(sourceCode, "/\\*CLASS\\*/", Class ?? "");
            sourceCode = Regex.Replace(sourceCode, "/\\*METHOD\\*/", Method ?? "");
            sourceCode = Regex.Replace(sourceCode, "/\\*ENTRY\\*/", Entry ?? "");
            return sourceCode;
        }

        public string GetFullClassQualifier(string sourceCode)
        {
            var ns = new Regex(@"namespace\s+(?<ns>\w+)", RegexOptions.Multiline).Match(sourceCode);
            var nameSpace = ns.Groups["ns"] != null ? ns.Groups["ns"].Value : " Unable to parse namespace";
            var cl = new Regex(@"class\s+(?<cl>\w+)", RegexOptions.Multiline).Match(sourceCode);
            var className = cl.Groups["cl"] != null ? cl.Groups["cl"].Value : " Unable to parse class";
            return nameSpace + "." + className;
        }

        public string CompileAndRun(string compileOptions)
        {
            var fullClassQualifier = GetFullClassQualifier(SourceCode);
            var generatedTemplate = string.Empty;
            var result = DotNetCompiler.CompileToAssembly(BuildCSharpCode(), Regex.Split(AssemblySources, Environment.NewLine), compileOptions: compileOptions);
            if (result.Errors.HasErrors)
            {
                generatedTemplate = result.Errors.Cast<CompilerError>().Aggregate(generatedTemplate,
                                                                                  (current, err) =>
                                                                                  current +
                                                                                  (err.ErrorText + " " + err.Line +
                                                                                   Environment.NewLine));
            }
            else
            {
                dynamic instance = result.CompiledAssembly.CreateInstance(fullClassQualifier);
                if (instance == null)
                {
                    generatedTemplate = "Failed to Call " + fullClassQualifier;
                }
                else
                {
                    var output = instance.Main();

                    if ((output is string))
                    {
                        generatedTemplate = output;
                    }
                    else if (output == null)
                    {
                        generatedTemplate = "null";
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
            }
            return generatedTemplate;
        }

        public string CompileToDll(string exeName, string compileOptions)
        {
            var msg = string.Empty;
            var result = DotNetCompiler.CompileToDll(BuildCSharpCode(), Regex.Split(AssemblySources, Environment.NewLine), exeName, compileOptions: compileOptions);
            msg = result.Errors.HasErrors ? result.Errors.Cast<CompilerError>().Aggregate(msg, (current, err) => current + (err.ErrorText + " " + err.Line + Environment.NewLine)) : "Successful";
            return msg;
        }

        public string CompileToExe(string exeName, string compileOptions)
        {
            var msg = string.Empty;
            var fullClassQualifier = GetFullClassQualifier(SourceCode);
            var result = DotNetCompiler.CompileToExe(BuildCSharpCode(), Regex.Split(AssemblySources, Environment.NewLine), exeName, false, fullClassQualifier, compileOptions: compileOptions);
            msg = result.Errors.HasErrors ? result.Errors.Cast<CompilerError>().Aggregate(msg, (current, err) => current + (err.ErrorText + " " + err.Line + Environment.NewLine)) : "Successful";
            return msg;
        }

        public string CompileAndRunExe(string compileOptions, string args = "")
        {
            var msg = string.Empty;
            var fullClassQualifier = GetFullClassQualifier(SourceCode);
            var result = DotNetCompiler.CompileToExe(BuildCSharpCode(), Regex.Split(AssemblySources, Environment.NewLine), Path.ChangeExtension(Path.GetTempFileName(), "exe"), false, fullClassQualifier, compileOptions: compileOptions);
            if (result.Errors.HasErrors)
            {
                msg = result.Errors.Cast<CompilerError>().Aggregate(msg, (current, err) => current + (err.ErrorText + " " + err.Line + Environment.NewLine));
            }
            else
            {
                RunWait(result.CompiledAssembly.Location, args);
                msg = "Successful";
            }
            return msg;
        }

        public static void RunWait(string filename, string args = "")
        {
            var pInfo = new ProcessStartInfo { FileName = filename, Arguments = args };
            Process p = Process.Start(pInfo);
            //p.WaitForInputIdle();
            p.WaitForExit();
            if (p.HasExited) return;
            if (p.Responding)
                p.CloseMainWindow();
            else
                p.Kill();
        }

        public static void Run(string filename, string args = "")
        {
            var pInfo = new ProcessStartInfo { FileName = filename, Arguments = args };
            Process p = Process.Start(pInfo);
            //p.WaitForInputIdle();
        }
    }
}
