using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ostrich.Logic.AppMan
{
    internal class ApplicationMessageBuffer
    {
        private StringBuilder Buffer { get; set; }
        internal int LineLimit { get; set; }
        internal int CharLimit { get; set; }
        private ApplicationUserWrapper User { get; set; }

        #region Constructors

        public ApplicationMessageBuffer(ApplicationUserWrapper user)
        {
            this.User = user;
            this.Buffer = new StringBuilder();
        }

        public ApplicationMessageBuffer(ApplicationUserWrapper user, int charLimit, int lineLimit)
        {
            this.User = user;
            this.CharLimit = charLimit;
            this.LineLimit = lineLimit;
            this.Buffer = new StringBuilder();
        }

        public ApplicationMessageBuffer(ApplicationUserWrapper user, int charLimit)
        {
            this.User = user;
            this.CharLimit = charLimit;
            this.Buffer = new StringBuilder();
        }

        #endregion // Constructors

        public void Flush()
        {
            this.User.SendUpdate(this.Buffer.ToString());
            this.Buffer.Clear();
        }

        public void SendLine(string msg)
        {
            if (this.CharLimit > 0)
            {
                if (this.Buffer.Length + msg.Length > this.CharLimit)
                    this.Flush();
            }
            else if (this.LineLimit > 0)
            {

                if (Regex.Match(this.Buffer.ToString(), Environment.NewLine).Length 
                    + Regex.Match(msg, Environment.NewLine).Length + 1>= this.LineLimit)
                    this.Flush();
            }

            this.Buffer.AppendLine(msg);
        }
    }
}
