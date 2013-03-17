using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QuickCodeApp.Compiler
{
    public class ScriptParser
    {
        public ScriptParser()
        {

        }

        public string[] Split(string parse)
        {
            var result = new string[2];
            result[0] = string.Empty;
            result[1] = string.Empty;
            var idx = 0;
            using (var sr = new StringReader(parse))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (idx == 0 && line.Trim() == "#")
                    {
                        idx = 1;//skip this char as well
                    }
                    else
                    {
                        result[idx] += line.TrimEnd() + Environment.NewLine;
                    }

                }
            }

            result[0] = Regex.Replace(result[0], Environment.NewLine, Environment.NewLine + "\t\t\t\t");

            return result;
        }




    }
}
