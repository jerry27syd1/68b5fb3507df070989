using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using CCCScriptProject;

public class Template
{
    private const string DefaultTemplate =
@"/*USING*/
namespace /*NAMESPACEID*/
{
    public class /*CLASSID*/ 
    {
/*FIELD*/
/*MainMethod*/
/*METHOD*/
    }
/*CLASS*/
}
/*NAMESPACE*/";
    #region template
    private const string DefaultClass =
@"
public static class Extensions
{
    public static string R(this String str, string v1, string v2)
    {
        return str.Replace(v1,v2);
    }
}
";
    private const string DefaultMethod =
@"
public void RunWait(string filename, string args = """")
{
    //Get path to system folder.
    string sysFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
    //Create a new process info structure.
    var pInfo = new ProcessStartInfo();
    //Set file name to open.
    pInfo.FileName = filename;
    pInfo.Arguments = args;

    //Start the process.
    var p = Process.Start(pInfo);
    //Wait for window to finish loading.
    p.WaitForInputIdle();
    //Wait for the process to exit or time out.
    p.WaitForExit();
    //Check to see if the process is still running.
    if (p.HasExited == false)
        //Process is still running.
        //Test to see if the process is hung up.
        if (p.Responding)
            //Process was responding; close the main window.
            p.CloseMainWindow();
        else
            //Process was not responding; force the process to close.
            p.Kill();

}

public void Run(string filename, string args = """")
{
    //Get path to system folder.
    string sysFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
    //Create a new process info structure.
    var pInfo = new ProcessStartInfo();
    //Set file name to open.
    pInfo.FileName = filename;
    pInfo.Arguments = args;

    //Start the process.
    var p = Process.Start(pInfo);
    //Wait for window to finish loading.
    p.WaitForInputIdle();
}

public void Region(string destionatFileName, string regionName, string scriptFileName = """", string prefix = ""\\#"", string postfix = """"){

    var dest = File.ReadAllText(destionatFileName).Trim();
    var source = """"; 
    if (scriptFileName == """")
    {
       scriptFileName = new System.IO.FileInfo( Environment.GetCommandLineArgs()[1]).Name;
       source = File.ReadAllText(scriptFileName +"".txt"").Trim();
    }else{
       source = File.ReadAllText(scriptFileName).Trim();
    }
    dest = Regex.Replace(dest, ""(?imsx-n)"" + prefix + ""region\\s""  + regionName + postfix + "".*?"" + prefix + ""endregion"" + postfix, source);
    File.WriteAllText(destionatFileName, dest);
}

 ";

    private static string DefalultUsingBlock =
 @"
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
";

    private const string DefaultMainMethod =
@"      
        public static string obj = """";
        public string Start(Template T)
        {
            try
            {
               /*ENTRY*/
            }
            catch (Exception ex)
            {
                obj = ex.Message + "" : "" + ex.StackTrace;
            }
            return obj;
        }
";
    #endregion

    public string GetDefaultTemplate(CodeBlockFile source, string nameSpaceId, string classId)
    {
        return DefaultTemplate.Replace("/*NAMESPACEID*/", nameSpaceId)
                               .Replace("/*CLASSID*/", classId)
                               .Replace("/*USING*/", DefalultUsingBlock + source.Definition)
                               .Replace("/*FIELD*/", source.Field)
                               .Replace("/*CLASS*/", DefaultClass + source.Class)
                               .Replace("/*METHOD*/", DefaultMethod + source.Method)
                               .Replace("/*MainMethod*/", DefaultMainMethod)
                               .Replace("/*ENTRY*/", source.SourceCode);
    }
    public Template()
    {
        Values = new Dictionary<string, string>();
        Name = GetType().Name;
        TypeName = GetType().Name;
    }

    public string Name { get; set; }

    public string TypeName { get; set; }

    public Dictionary<string, string> Values { get; set; }

    public string this[string data]
    {
        get { return Values[data]; }
        set { Values[data] = value; }
    }

    public string Compile(string path)
    {
        return Complie(path, null, null);
    }

    public string CompileFromString(string textString)
    {
        return Complie(null, textString, null);
    }

    public string CompileCode(string codes)
    {
        return Complie(null, null, codes);
    }

    public string ReadAsCodeString(string path)
    {
        if (File.Exists(path))
        {
            string file = File.ReadAllText(path);
            return CodeBlock.FormatCode(file);
        }
        throw new Exception("\"\";\nthrow Exception(\"T.ReadAsCodeString() File:" + path + " is not found.\");");
    }

    public List<TemplateFile> Split(string str)
    {
        var result = new List<TemplateFile>();
        MatchCollection collection = CodeBlock.Parse(str, @"(?imsx-n)\$(?<value1>\d)\~(?<value2>.*?)\~\d\$");
        foreach (Match item in collection)
        {
            var f = new TemplateFile();
            f.FileId = item.Groups["value1"].Value;
            f.Content = item.Groups["value2"].Value.Trim();
            result.Add(f);
        }
        return result;
    }

    public string ExtractPostExecuteBlock(string str)
    {
        string result = string.Empty;
        MatchCollection collection = CodeBlock.Parse(str, @"(?imsx-n)\$\~\~(?<value1>.*?)\~\~\$");
        foreach (Match item in collection)
        {
            result += item.Groups["value1"].Value.Trim();
        }
        return result;
    }

    public string RemovePostExecuteBlock(string str)
    {
        return CodeBlock.Replace(str, "", @"(?imsx-n)\$\~\~(?<value1>.*?)\~\~\$");
    }

    public string GenerateSourceFile(string path)
    {
        CodeBlockFile source = CodeBlock.GenerateSourceFile(path);
        return GetDefaultTemplate(source, "CCCScript", "RuntimeProgram");
    }

    public string Format(string textString)
    {
        return CodeBlock.FormatCode(textString);
    }

    public void RunWait(string filename, string args = "")
    {
        //Get path to system folder.
        string sysFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
        //Create a new process info structure.
        var pInfo = new ProcessStartInfo();
        //Set file name to open.
        pInfo.FileName = filename;
        pInfo.Arguments = args;

        //Start the process.
        Process p = Process.Start(pInfo);
        //Wait for window to finish loading.
        p.WaitForInputIdle();
        //Wait for the process to exit or time out.
        p.WaitForExit();
        //Check to see if the process is still running.
        if (p.HasExited == false)
            //Process is still running.
            //Test to see if the process is hung up.
            if (p.Responding)
                //Process was responding; close the main window.
                p.CloseMainWindow();
            else
                //Process was not responding; force the process to close.
                p.Kill();
    }

    public void Run(string filename, string args = "")
    {
        //Get path to system folder.
        string sysFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
        //Create a new process info structure.
        var pInfo = new ProcessStartInfo();
        //Set file name to open.
        pInfo.FileName = filename;
        pInfo.Arguments = args;

        //Start the process.
        Process p = Process.Start(pInfo);
        //Wait for window to finish loading.
        p.WaitForInputIdle();
    }

    private string Complie(string path, string textString, string codes)
    {
        string rtResult = String.Empty;
        CodeBlockFile source = null;
        if (path != null)
        {
            source = CodeBlock.GenerateSourceFile(path);
        }
        else if (textString != null)
        {
            source = CodeBlock.GenerateSourceFileFromString(textString);
        }
        else
        {
            source = new CodeBlockFile();
            source.SourceCode = codes;
        }
        string finalCode = GetDefaultTemplate(source, "CCCScript", "RuntimeProgram");

        CompilerResults result = CscCompile(finalCode,
                                            source.Reference.Split(new[] { Environment.NewLine },
                                                                   StringSplitOptions.RemoveEmptyEntries));

        if (result.Errors.Count == 0)
        {
            dynamic t = result.CompiledAssembly.CreateInstance("CCCScript.RuntimeProgram");
            if (t != null)
            {
                rtResult = t.Start(this);
            }
            else
            {
                rtResult = "RuntimeProgram is null";
                throw new Exception(rtResult);
            }
        }
        else
        {
            foreach (CompilerError err in result.Errors)
            {
                rtResult += err.ErrorText + " " + err.Line.ToString() + Environment.NewLine;
            }
            throw new Exception(rtResult);
        }

        return rtResult;
    }

    public static CompilerResults CscCompile(string source, string[] assemblyFiles)
    {
        var providerOptions = new Dictionary<string, string>();
        providerOptions.Add("CompilerVersion", "v4.0");
        CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp", providerOptions);

        var parms = new CompilerParameters();
        parms.GenerateExecutable = false;
        parms.GenerateInMemory = true;
        parms.IncludeDebugInformation = false;
        parms.ReferencedAssemblies.Add("Accessibility.dll");
        parms.ReferencedAssemblies.Add("System.Configuration.Install.dll");
        parms.ReferencedAssemblies.Add("System.Data.dll");
        parms.ReferencedAssemblies.Add("System.Design.dll");
        parms.ReferencedAssemblies.Add("System.DirectoryServices.dll");
        parms.ReferencedAssemblies.Add("System.dll");
        parms.ReferencedAssemblies.Add("System.Drawing.Design.dll");
        parms.ReferencedAssemblies.Add("System.Drawing.dll");
        parms.ReferencedAssemblies.Add("System.EnterpriseServices.dll");
        parms.ReferencedAssemblies.Add("System.Management.dll");
        parms.ReferencedAssemblies.Add("System.Messaging.dll");
        parms.ReferencedAssemblies.Add("System.Runtime.Remoting.dll");
        parms.ReferencedAssemblies.Add("System.Runtime.Serialization.Formatters.Soap.dll");
        parms.ReferencedAssemblies.Add("System.Security.dll");
        parms.ReferencedAssemblies.Add("System.ServiceProcess.dll");
        parms.ReferencedAssemblies.Add("System.Web.dll");
        parms.ReferencedAssemblies.Add("System.Web.RegularExpressions.dll");
        parms.ReferencedAssemblies.Add("System.Web.Services.dll");
        parms.ReferencedAssemblies.Add("System.Windows.Forms.dll");
        parms.ReferencedAssemblies.Add("System.XML.dll");
        parms.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll");
        parms.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
        parms.ReferencedAssemblies.Add("System.Core.dll");
        parms.ReferencedAssemblies.Add("System.Xml.Linq.dll");
        parms.ReferencedAssemblies.Add("System.dll");

        parms.ReferencedAssemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CCCScript.dll"));
        parms.ReferencedAssemblies.AddRange(assemblyFiles);

        return provider.CompileAssemblyFromSource(parms, source);
    }

    public string ReplaceEx(string source, string value1, string value2)
    {
        List<string> v1 = GenerateReplaceCritiera(value1);
        List<string> v2 = GenerateReplaceCritiera(value2);
        v1.AddRange(GenerateReplaceCritiera("_" + value1));
        v2.AddRange(GenerateReplaceCritiera("_" + value2));

        for (int i = 0; i < v1.Count; i++)
        {
            source = source.Replace(v1[i], v2[i]);
        }
        return source;
    }

    public List<string> GenerateReplaceCritiera(string value)
    {
        var result = new List<string>();

        result.Add(value.ToUpper());
        result.Add(value.ToUpper().Replace(" ", "_"));
        result.Add(value.ToUpper().Replace(" ", ""));

        result.Add("_" + value.ToUpper().Replace(" ", "_"));
        result.Add("_" + value.ToUpper().Replace(" ", ""));

        result.Add(value.ToLower());
        result.Add(value.ToLower().Replace(" ", "_"));
        result.Add(value.ToLower().Replace(" ", ""));

        result.Add("_" + value.ToLower().Replace(" ", "_"));
        result.Add("_" + value.ToLower().Replace(" ", ""));

        result.Add(AllFirstCharUppercase(value, ' '));
        result.Add(AllFirstCharUppercase(value, ' ').Replace(" ", "_"));
        result.Add(AllFirstCharUppercase(value, ' ').Replace(" ", ""));

        result.Add("_" + AllFirstCharUppercase(value, ' ').Replace(" ", "_"));
        result.Add("_" + AllFirstCharUppercase(value, ' ').Replace(" ", ""));

        result.Add(FirstCharLowercase(value).Replace(" ", "_"));
        result.Add(FirstCharLowercase(value).Replace(" ", ""));

        result.Add("_" + FirstCharLowercase(value).Replace(" ", "_"));
        result.Add("_" + FirstCharLowercase(value).Replace(" ", ""));

        return result;
    }

    public string FirstCharLowercase(string s)
    {
        char[] c = s.ToCharArray();

        for (int i = 0; i < c.Length; i++)
        {
            if (Char.IsLetter(c[i]))
            {
                c[i] = Char.ToLower(c[i]);
                break;
            }
        }
        return new string(c);
    }

    public string AllFirstCharUppercase(string s, char delimit)
    {
        char[] c = s.ToCharArray();
        int state = 0;
        for (int i = 0; i < c.Length; i++)
        {
            if (state == 0)
            {
                if (Char.IsLetter(c[i]))
                {
                    c[i] = Char.ToUpper(c[i]);
                    state = 1;
                }
            }
            else if (state == 1)
            {
                if (delimit == c[i])
                {
                    state = 0;
                }
                else
                {
                    c[i] = Char.ToLower(c[i]);
                }
            }
        }
        return new string(c);
    }

    public void CreateDirectory(DirectoryInfo dirInfo)
    {
        if (dirInfo == null) return;
        if (dirInfo.Parent != null) CreateDirectory(dirInfo.Parent);
        if (!dirInfo.Exists) dirInfo.Create();
    }

    public class TemplateFile
    {
        public string FileId { get; set; }
        public string Content { get; set; }
    }
}