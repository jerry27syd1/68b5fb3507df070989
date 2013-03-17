using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CCCScriptProject
{

    public class CodeBlock
    {
        private const string StringPattern = @"(?imsx-n)@!(?<value1>\w+)(?<value2>.*?)!@";
        private const string DefintionPattern = @"(?imsx-n)\#using(?<value1>.*?)\#endusing";
        private const string AssemblyPattern = @"(?imsx-n)\#dll(?<value1>.*?)\#enddll";
        private const string ClassPattern = @"(?imsx-n)\#class(?<value1>.*?)\#endclass";
        private const string MethodPattern = @"(?imsx-n)\#method(?<value1>.*?)\#endmethod";
        private const string FieldPattern = @"(?imsx-n)\#field(?<value1>.*?)\#endfield";
        private const string ReplacePattern = @"\[(?<Value1>.*?)\]\s*\=\s*(?<Value2>.*)";
        private const string ImportclassPattern = "\\#using@\\\"(?<Value1>.*)\\\"";
        private const string ImportPattern = "\\#@\\\"(?<Value1>.*)\\\"";
        private const string ReplaceExPattern = @"\#\[(?<Value1>.*?)\]\s*\=\s*(?<Value2>.*)\s*;";
        private SourceTypeOptions SourceType { get; set; }

        public string Content { get; set; }

        public static CodeBlockFile GenerateSourceFile(string rootFile)
        {
            return GenerateSourceFile(rootFile, null);
        }

        public static CodeBlockFile GenerateSourceFileFromString(string textString)
        {
            return GenerateSourceFile(null, textString);
        }

        private static CodeBlockFile GenerateSourceFile(string rootFile, string textString)
        {
            IEnumerable<CodeBlock> list = rootFile != null
                                              ? SplitCodeAndText(rootFile)
                                              : SplitCodeAndTextFromString(textString);
            string code = SplitCodeAndContent(list, "obj");
            code = OpenDefintionLinkInCode(code);

            string usingBlock = ExtractDefintionBlock(code);
            string dllBlock = ExtractAssemblyBlock(code);
            string methodBlock = ExtractMethodBlock(code);
            string classBlock = ExtractClassBlock(code);
            string fieldBlock = ExtractFieldBlock(code);
            string strings = ExtractStringBlock(code);
            code = RemoveAllPatternBlocks(code);
            code = ProcessAllInlinePatterns(code, "obj");

            var codeFile = new CodeBlockFile();
            codeFile.Definition = usingBlock;
            codeFile.SourceCode = strings + code;
            codeFile.Reference = dllBlock;
            codeFile.Class = classBlock;
            codeFile.Method =methodBlock ;
            codeFile.Field = fieldBlock;

            return codeFile;
        }

        /// <summary>
        /// Handles @!variable 
        ///          xxxxx !@
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static string ExtractStringBlock(string code)
        {
            string result = string.Empty;
            MatchCollection collection = Parse(code, StringPattern);
            foreach (Match item in collection)
            {
                result += "string " + item.Groups["value1"].Value.Trim() + "=@\"" + item.Groups["value2"].Value.Replace("\"", "\"\"") + "\";" + Environment.NewLine;
            }
            return result;

        }

        /// <summary>
        /// Handles #using@"xxx.cs"
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static string OpenDefintionLinkInCode(string code)
        {
            string newCode = String.Empty;


            using (var reader = new StringReader(code))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimedLine = line.Replace("\r", "").Replace("\n", "").Trim();
                    if (trimedLine.StartsWith("#using@"))
                    {
                        Match match = ParseSingle(trimedLine, ImportclassPattern);
                        string value1 = match.Groups["Value1"].Value;
                        try
                        {
                            string importCode = File.ReadAllText(value1);

                            int idx = importCode.IndexOf("namespace");
                            if (idx == -1)
                            {
                                idx = importCode.IndexOf("class");
                            }
                            if (idx != -1)
                            {
                                importCode = importCode.Substring(idx, importCode.Length - idx);
                            }

                            newCode += "#using" + Environment.NewLine + importCode + Environment.NewLine +
                                       "#endusing" +
                                       Environment.NewLine;
                        }
                        catch
                        {
                            throw new Exception("throw new Exception(\"Reference:" + FormatCode(value1) +
                                                " File Not Found.\");");
                            ;
                        }
                    }
                    else
                    {
                        newCode += trimedLine + Environment.NewLine;
                    }
                }
            }
            return newCode;
        }

        /// <summary>
        /// Handles #using 
        ///            using System.IO;
        ///         #endusing
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ExtractDefintionBlock(string str)
        {
            string result = string.Empty;
            MatchCollection collection = Parse(str, DefintionPattern);
            foreach (Match item in collection)
            {
                result += item.Groups["value1"].Value.Trim() + Environment.NewLine;
            }
            return result;
        }

        /// <summary>
        /// Handles #dll
        ///         System.IO.dll
        ///         #enddll
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ExtractAssemblyBlock(string str)
        {
            string result = string.Empty;
            MatchCollection collection = Parse(str, AssemblyPattern);
            foreach (Match item in collection)
            {
                result += item.Groups["value1"].Value.Trim() + Environment.NewLine;
            }
            return result;
        }

        /// <summary>
        /// Handles #class
        ///         public class XXX{}
        ///         #endclass
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ExtractClassBlock(string str)
        {
            string result = string.Empty;
            MatchCollection collection = Parse(str, ClassPattern);
            foreach (Match item in collection)
            {
                result += item.Groups["value1"].Value.Trim() + Environment.NewLine;
            }
            return result;
        }

        /// <summary>
        /// Handles #method
        ///         public void Save(){}
        ///         #endmethod
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ExtractMethodBlock(string str)
        {
            string result = string.Empty;
            MatchCollection collection = Parse(str, MethodPattern);
            foreach (Match item in collection)
            {
                result += item.Groups["value1"].Value.Trim() + Environment.NewLine;
            }
            return result;
        }
        /// <summary>
        /// Handles #field
        ///         public void Save(){}
        ///         #endfield
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string ExtractFieldBlock(string str)
        {
            string result = string.Empty;
            MatchCollection collection = Parse(str, FieldPattern);
            foreach (Match item in collection)
            {
                result += item.Groups["value1"].Value.Trim() + Environment.NewLine;
            }
            return result;
        }
        public static string RemoveAllPatternBlocks(string str)
        {
            str = Replace(str, "", DefintionPattern);
            str = Replace(str, "", AssemblyPattern);
            str = Replace(str, "", StringPattern);
            str = Replace(str, "", ClassPattern);
            str = Replace(str, "", MethodPattern);
            str = Replace(str, "", FieldPattern);
            return str;
        }

        private static string ProcessAllInlinePatterns(string code, string textOutputFuncName)
        {
            var r = new Random();
            var appendCodes = new List<string>();
            string newCode = String.Empty;
            using (var reader = new StringReader(code))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimedLine = line.Replace("\r", "").Replace("\n", "").Trim();
                    if (trimedLine.Length > 0)
                    {
                        if (trimedLine.StartsWith("="))
                        {
                            trimedLine = trimedLine.Replace("=", textOutputFuncName + "+=") + ";";
                            newCode += trimedLine + Environment.NewLine;
                        }
                        //else if (trimedLine.StartsWith("["))
                        //{
                        //    Match match = ParseSingle(trimedLine, ReplacePattern);
                        //    string value1 = match.Groups["Value1"].Value;
                        //    string value2 = match.Groups["Value2"].Value;

                        //    string newVar = "value" + r.Next();
                        //    string s = "string " + newVar + " = " + value2;
                        //    newCode += s + Environment.NewLine;
                        //    string replaceCall = textOutputFuncName + "=" + textOutputFuncName + ".Replace(\"" +
                        //                         FormatCode(value1) + "\", " + newVar + ")" + ";" +
                        //                         Environment.NewLine;
                        //    appendCodes.Add(replaceCall);
                        //}
                        else if (trimedLine.StartsWith("#["))
                        {
                            Match match = ParseSingle(trimedLine, ReplaceExPattern);
                            string value1 = match.Groups["Value1"].Value;
                            string value2 = match.Groups["Value2"].Value;

                            string newVar = "value" + r.Next();
                            string s = "string " + newVar + " = " + value2 + ";";
                            newCode += s + Environment.NewLine;
                            string replaceCall = textOutputFuncName + "= T.ReplaceEx(" + textOutputFuncName + ", \"" +
                                                 FormatCode(value1) + "\", " + newVar + ")" + ";" +
                                                 Environment.NewLine;
                            appendCodes.Add(replaceCall);
                        }
                        else if (trimedLine.StartsWith("#@"))
                        {
                            Match match = ParseSingle(trimedLine, ImportPattern);
                            string value1 = match.Groups["Value1"].Value;
                            try
                            {
                                newCode += File.ReadAllText(value1);
                            }
                            catch
                            {
                                throw new Exception("throw new Exception(\"Reference:" + FormatCode(value1) +
                                                    " File Not Found.\");");
                                ;
                            }
                        }
                        else
                        {
                            newCode += trimedLine + Environment.NewLine;
                        }
                    }
                }
            }
            string result = newCode;
            foreach (string appendCode in appendCodes)
                result = result + appendCode;
            return result;
        }

        private static string SplitCodeAndContent(IEnumerable<CodeBlock> splitList, string textOutputFuncName)
        {
            bool sourceTypeFound = false;
            string code = String.Empty;
            foreach (CodeBlock codeBlock in splitList)
            {
                if (codeBlock.SourceType == SourceTypeOptions.Code && codeBlock.Content.Length > 0)
                {
                    code += codeBlock.Content.TrimStart() + Environment.NewLine;
                    sourceTypeFound = true;
                }
                else if (codeBlock.Content.Length > 0)
                {
                    //bool validate = codeBlock.Content.Any(c => c != '\r' && c != '\n');
                    //if (validate)
                    //{

                    if (sourceTypeFound)
                    {
                        //!!!!!!!!!!!!!!!!!!!!!!!!! warning 
                        if (codeBlock.Content.Length >= 2 && codeBlock.Content[0] == '\r' &&
                            codeBlock.Content[1] == '\n')
                        {
                            codeBlock.Content = ReplaceFirst(codeBlock.Content, Environment.NewLine, "");
                        }
                    }

                    code += textOutputFuncName + " += \"" + FormatCode(codeBlock.Content) + "\";" +
                            Environment.NewLine;
                    //}
                }
            }
            return code;
        }

        private static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string FormatCode(string code)
        {
            return code.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r")
                       .Replace("\n", "\\n").Replace("\t", "\\t");
        }

        private static IEnumerable<CodeBlock> SplitCodeAndTextFromString(string textString)
        {
            var list = new List<CodeBlock>();
            int state = 0;
            var codeContext = new[] { '$', '!', '!', '$' };
            string text = String.Empty;
            using (var reader = new StringReader(textString))
            {
                while (reader.Peek() >= 0)
                {
                    var c = (char) reader.Read();
                    if (state == 0) //$
                    {
                        if (c == codeContext[state])
                        {
                            state++;
                        }
                        else
                        {
                            text += c;
                        }
                    }
                    else if (state == 1) //!
                    {
                        if (c == codeContext[state])
                        {
                            var block = new CodeBlock { Content = text, SourceType = SourceTypeOptions.Text };
                            list.Add(block);
                            text = String.Empty;
                            state++;
                        }
                        else
                        {
                            state = 0;
                            text += codeContext[state];
                            text += c;
                        }
                    }
                    else if (state == 2) // code space !
                    {
                        if (c == codeContext[state])
                        {
                            state++;
                        }
                        else
                        {
                            text += c;
                        }
                    }
                    else if (state == 3) // $
                    {
                        if (c == codeContext[state])
                        {
                            var block = new CodeBlock { Content = text, SourceType = SourceTypeOptions.Code };
                            list.Add(block);
                            text = String.Empty;
                            state = 0;
                        }
                        else
                        {
                            state = 2;
                            text += codeContext[state];
                            text += c;
                        }
                    }
                }
            }
            if (state < 2)
            {
                var block = new CodeBlock { Content = text, SourceType = SourceTypeOptions.Text };
                list.Add(block);
            }
            else
            {
                var block = new CodeBlock { Content = text, SourceType = SourceTypeOptions.Code };
                list.Add(block);
            }

            return list;
        }

        private static IEnumerable<CodeBlock> SplitCodeAndText(string rootFile)
        {
            var list = new List<CodeBlock>();
            var codeContext = new[] { '$', '!', '!', '$' };
            int state = 0;
            string text = String.Empty;
            using (var reader = new StreamReader(new FileStream(rootFile, FileMode.Open)))
            {

                char c = '\n';
                while (reader.Peek() >= 0)
                {
                    c = (char) reader.Read();

                    if (state == 0) //$
                    {
                        if (c == codeContext[state])
                        {
                            state++;
                        }
                        else
                        {
                            text += c;
                        }
                    }
                    else if (state == 1) //!
                    {
                        if (c == codeContext[state])
                        {
                            var block = new CodeBlock { Content = text, SourceType = SourceTypeOptions.Text };
                            list.Add(block);
                            text = String.Empty;
                            state++;
                        }
                        else
                        {
                            state = 0;
                            text += codeContext[state];
                            text += c;
                        }
                    }
                    else if (state == 2) // code space !
                    {
                        if (c == codeContext[state])
                        {
                            state++;
                        }
                        else
                        {
                            text += c;
                        }
                    }
                    else if (state == 3) // $
                    {
                        if (c == codeContext[state])
                        {
                            var block = new CodeBlock { Content = text, SourceType = SourceTypeOptions.Code };
                            list.Add(block);
                            text = String.Empty;
                            state = 0;
                        }
                        else
                        {
                            state = 2;
                            text += codeContext[state];
                            text += c;
                        }
                    }
                }
                if (state < 2)
                {
                    if (state == 1)
                    {
                        text += c;
                    }

                    var block = new CodeBlock { Content = text, SourceType = SourceTypeOptions.Text };
                    list.Add(block);
                }
                else
                {
                    var block = new CodeBlock { Content = text, SourceType = SourceTypeOptions.Code };
                    list.Add(block);
                }
            }

            return list;
        }

        public static Match ParseSingle(string input, string pattern)
        {
            var regex = new Regex(pattern,
                                  RegexOptions.IgnoreCase
                                  | RegexOptions.CultureInvariant
                                  | RegexOptions.IgnorePatternWhitespace
                                  | RegexOptions.Compiled
                );
            MatchCollection matchCollection = regex.Matches(input);
            return matchCollection.Count > 0 ? matchCollection[0] : null;
        }

        public static MatchCollection Parse(string input, string pattern)
        {
            var regex = new Regex(pattern,
                                  RegexOptions.IgnoreCase
                                  | RegexOptions.CultureInvariant
                                  | RegexOptions.IgnorePatternWhitespace
                                  | RegexOptions.Compiled
                );
            return regex.Matches(input);
        }

        public static string Replace(string input, string replace, string pattern)
        {
            var regex = new Regex(pattern,
                                  RegexOptions.IgnoreCase
                                  | RegexOptions.CultureInvariant
                                  | RegexOptions.IgnorePatternWhitespace
                                  | RegexOptions.Compiled
                );
            return regex.Replace(input, replace);
        }

        #region Nested type: SourceTypeOptions

        private enum SourceTypeOptions
        {
            CodeDefintion,
            Code,
            Text
        }

        #endregion
    }
}
