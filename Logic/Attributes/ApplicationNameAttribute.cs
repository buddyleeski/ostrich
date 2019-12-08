using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Logic.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApplicationNameAttribute : Attribute
    {
        public string Name { get; set; }
        public string Prefix { get; set; }

        public ApplicationNameAttribute(string name)
        {
            this.Name = name;
            this.Prefix = "";
        }

        public ApplicationNameAttribute(string prefix, string name)
        {
            this.Prefix = prefix;
            this.Name = name;
        }
    }
}
