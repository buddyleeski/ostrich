using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Logic.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IntervalAttribute : Attribute
    {
        public int Frequency { get; set; }
        public bool FireEvents { get; set; }

        public IntervalAttribute(int milliseconds)
        {
            this.Frequency = milliseconds;
            this.FireEvents = true;
        }

        public IntervalAttribute(int milliseconds, bool fireEvents)
        {
            this.Frequency = milliseconds;
            this.FireEvents = fireEvents;
        }
    }
}
