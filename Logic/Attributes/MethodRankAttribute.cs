using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Logic.Attributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MethodRankAttribute : Attribute
    {
        public int Rank {get; set;}

        public MethodRankAttribute(int rank)
        {
            Rank = rank;
        }
    }
}
