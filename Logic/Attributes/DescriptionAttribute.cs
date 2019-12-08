using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Logic.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionAttribute : Attribute
    {
        public string Value {get; private set;}

        public DescriptionAttribute(string val)
        {
            this.Value = val;
        }
    }
}
