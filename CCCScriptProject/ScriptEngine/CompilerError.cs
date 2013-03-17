using System;

namespace ScriptEngine
{
    /// <summary>
    /// Summary description for CompilerError.
    /// </summary>
    [Serializable]
    public class CompilerError
    {
        private int column;
        private string file;
        private int line;
        private string number;
        private string text;

        public CompilerError()
        {
        }

        public CompilerError(System.CodeDom.Compiler.CompilerError error)
        {
            column = error.Column;
            file = error.FileName;
            line = error.Line;
            number = error.ErrorNumber;
            text = error.ErrorText;
        }

        public int Line
        {
            get { return line; }
            set { line = value; }
        }

        public string File
        {
            get { return file; }
            set { file = value; }
        }

        public int Column
        {
            get { return column; }
            set { column = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public string Number
        {
            get { return number; }
            set { number = value; }
        }
    }
}