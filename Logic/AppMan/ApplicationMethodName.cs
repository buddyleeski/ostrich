using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Logic.AppMan
{
    public class ApplicationMethodName
    {
        public List<string> Name { get; set; }
        public int NameWordCount { get; set; }
        public String RegEx { get; set; }

        public ApplicationMethodName(string[] words, bool hasParamArray, List<int> listParameterIndexes)
        {
            Name = words.ToList();
            this.ProcessName(hasParamArray, listParameterIndexes);
        }

        public ApplicationMethodName(string name, bool hasParamArray, List<int> listParameterIndexes)
        {
            Name = new List<string>() { name };
            this.ProcessName(hasParamArray, listParameterIndexes);
        }

        private void ProcessName(bool hasParamArray, List<int> listParameterIndexes)
        {
            StringBuilder regBuilder = new StringBuilder();

            int paramCount = this.Name.Count(word => word.StartsWith("["));
            int count = 0;
            this.Name.ForEach(word => 
            {
                if (word.StartsWith("[") && word.EndsWith("]"))
                {
                    count++;
                    if (hasParamArray && listParameterIndexes.Contains(count-1))
                        regBuilder.Append("\\s+([\\s*\\S]*)");
                    else
                        regBuilder.AppendFormat("\\s+(\\S+)\\s*");
                }
                else
                {
                    regBuilder.AppendFormat("\\s+{0}", word);
                    this.NameWordCount++;
                }
            });

            this.RegEx = regBuilder.ToString().Trim();

            this.RegEx = "^" + this.RegEx.Substring(3);
        }
    }
}
