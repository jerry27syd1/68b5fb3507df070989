namespace ScriptEngine
{
    /// <summary>
    /// Summary description for IScriptManager.
    /// </summary>
    public interface IScriptManager
    {
        void CompileAndExecuteFile(string file, string[] args, IScriptManagerCallback callback);
    }
}