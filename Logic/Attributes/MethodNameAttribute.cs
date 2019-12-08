using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Logic.Attributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MethodNameAttribute : Attribute
    {
        public string[] Words { get; set; }

        public MethodNameAttribute(params string[] words)
        {
            if (words.Length == 1)
                this.Words = words[0].Split(' ');
            else
                this.Words = words;
        }
    }
}
