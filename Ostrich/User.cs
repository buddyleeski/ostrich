using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Logic
{
    public class User
    {
        public string ScreenName { get; internal set; }

        public object Context { get; set; }

        public User(string screenName)
        {
            this.ScreenName = screenName;
        }

    }
}
