using System;
using System.IO;

namespace ScriptEngine
{
    /// <summary>
    /// Summary description for ScriptManager.
    /// </summary>
    public class ScriptManager : MarshalByRefObject, IScriptManager
    {

        public void CompileAndExecuteFile(string file, string[] args, IScriptManagerCallback callback)
        {
            var t = new Template();
            string extension = Path.GetExtension(file);

            if (extension == ".ccc")
            {
                if (Path.GetFileName(file).IndexOf(".debug.ccc") != -1)
                {
                    string filename = Path.GetFileName(file);
                    File.WriteAllText(filename + ".debug.cs", t.GenerateSourceFile(file));
                }

                string result = t.Compile(file);
                string postExecuteBlock = t.ExtractPostExecuteBlock(result);
                if (postExecuteBlock != string.Empty)
                {
                    result = t.RemovePostExecuteBlock(result);

                }
                var files = t.Split(result);

                foreach (var f in files)
                {
                    if (t.Values.ContainsKey("output" + f.FileId))
                    {
                        File.WriteAllText(t["output" + f.FileId], f.Content);
                    }
                }

                if (t.Values.ContainsKey("output"))
                {
                    if (t["output"] != string.Empty)
                    {
                        File.WriteAllText(t["output"], result);
                    }
                }
                else
                {
                    File.WriteAllText(Path.ChangeExtension(file, "ccc.txt"), result);
                }

                if (postExecuteBlock != string.Empty)
                {
                    if (Path.GetFileName(file).IndexOf(".debug.ccc") != -1)
                    {
                        string filename = Path.GetFileName(file);
                        File.WriteAllText(filename + ".debug.exe.cs", postExecuteBlock);
                    }

                    result = t.CompileCode(postExecuteBlock);
                    if (Path.GetFileName(file).IndexOf(".debug.ccc") != -1)
                    {
                        string filename = Path.GetFileName(file);
                        File.WriteAllText(filename + ".debug.exe.txt", result);
                    }
                }
            }else if (extension == ".ccs")
            {
                t.CompileCode(File.ReadAllText(file));
            }
        }

    }
}