using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ostrich.Logic;
using Ostrich.Logic.AppMan;

namespace Ostrich.Logic.Base
{
    public abstract class ApplicationBase
    {
        public ApplicationUserWrapper User { get; set; }
        public string Response { get; set; }
        public ApplicationInfo AppInfo { get; set; }

        public virtual void AppLoad()
        {
        }

        public virtual void Help()
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            if (AppInfo != null)
            {
                sb.AppendLine(string.Format("You are currently using the {0} plugin.", this.AppInfo.Name));
                sb.AppendLine("The following methods are available:");

                this.AppInfo.Methods.Where(m => !string.IsNullOrEmpty(m.Description)).ToList().ForEach(m =>
                {
                    count++;
                    sb.AppendLine(string.Format("\n{0} - {1}", string.Join(" ", m.Names.First().Name),
                                            (m.Description != "" ? m.Description : "[No Description]")));
                    
                    if (m.Names.Count > 1)
                    {
                        sb.AppendLine("Aliases Include:");
                        
                        m.Names.Skip(1).ToList().ForEach(name =>
                        {
                            sb.AppendLine(string.Join(" ", name.Name));
                        });
                    }

                    if (count % 8 == 0)
                    {
                        SendUpdate(sb.ToString());
                        sb.Clear();
                    }
                });

                if (sb.Length != 0)
                    SendUpdate(sb.ToString());
            }
        }

        public void SendUpdate(string msg)
        {
            this.User.SendUpdate(msg);
        }

        public void TriggerEvent(string eventName, object data)
        {
            this.User.TriggerEvent(eventName, data);
        }

        public string GetInput()
        {
            return this.User.Input;
        }

        public string GetInput(int milliseconds)
        {
            return this.User.GetInput(milliseconds);
        }

        public string GetValue(string prompt)
        {
            if (!this.User.HasParameters())
                SendUpdate(prompt);
            return this.User.Parameter;
        }

        #region Static Methods
        public static bool InitiateConversation(string user)
        {
            return false;
        }
        #endregion Static Methods

        #region Events

        public event BeforeCommandHandler BeforeCommand;
        public void OnBeforeCommand(BeforeCommandEventArgs args)
        {
            if (BeforeCommand != null)
                BeforeCommand(this, args);
        }

        public event AfterCommandHandler AfterCommand;
        public void OnAfterCommand(AfterCommandEventArgs args)
        {
            if (AfterCommand != null)
                AfterCommand(this, args);
        }

        public event ApplicationEventHandler ApplicationEvent;
        public void OnApplicationEvent(ApplicationEventArgs args)
        {
            if (ApplicationEvent != null)
                ApplicationEvent(args.Sender, args);
        }
        #endregion //Events

    }
}
