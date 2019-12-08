using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ostrich.Logic.AppMan;
using Ostrich.Logic.Enums;

namespace Ostrich.Logic
{

    #region InvalidCommand 

    public delegate void InvalidCommandHandler(object sender, InvalidCommandEventArgs e);
    public class InvalidCommandEventArgs : EventArgs
    {
        public InvalidCommandEventArgs(string input)
        {
            this.InputString = input;
        }

        public string InputString { get; private set; }
        public string ReturnString { get; set; }
    }

    #endregion // InvalidCommand

    #region SystemReply
    
    public delegate void SystemReplyHandler(object sender, SystemReplyEventArgs e);
    public class SystemReplyEventArgs : EventArgs
    {
        public SystemReplyEventArgs(string reply)
        {
            this.Reply = reply;
        }

        public string Reply { get; private set; }
    }

    #endregion // SystemReply

    #region BeforeCommand

    public delegate void BeforeCommandHandler(object sender, BeforeCommandEventArgs e);
    public class BeforeCommandEventArgs : EventArgs
    {
        public BeforeCommandEventArgs() {}
        public bool Continue { get; set; }
    }

    #endregion // BeforeCommand

    #region AfterCommand

    public delegate void AfterCommandHandler(object sender, AfterCommandEventArgs e);
    public class AfterCommandEventArgs : EventArgs
    {
        public AfterCommandEventArgs() {}
    }

    #endregion // AfterCommand

    #region ApplicationEvent
    public delegate void TriggerApplicationEventHandler(ApplicationUserWrapper sender, ApplicationEventArgs e);
    public delegate void ApplicationEventHandler(ApplicationUserWrapper sender, ApplicationEventArgs e);
    public class ApplicationEventArgs : EventArgs
    {
        public string EventName;
        public object Data;
        internal ApplicationUserWrapper Sender;
        internal string AssemblyName;
        public ApplicationEventArgs(ApplicationUserWrapper user, string eventName, object data) 
        {
            EventName = eventName;
            Data = data;

            AssemblyName = user.CurrentAppKey;
            Sender = user;
        }
    }
    #endregion ApplicationEvent

    #region Presence Event
    public delegate void PresenceEventHandler(object sender, PresenceEventArgs e);
    public class PresenceEventArgs : EventArgs
    {
        public string UserName;
        public string ShowValue;
        public string TypeValue;
        public PresenceEnum Presence;

        public PresenceEventArgs(string userName, string show, string type)
        {
            this.UserName = userName;
            this.ShowValue = (show ?? "").ToLower();
            this.TypeValue = (type ?? "").ToLower();

            if (!string.IsNullOrEmpty(this.TypeValue) && this.TypeValue == "unavailable") //* :unavailable (User has gone offline)
                this.Presence = PresenceEnum.Offline;
            else if (string.IsNullOrEmpty(this.ShowValue) //* nil (Available, no <show/> element)
                || this.ShowValue == "chat") //* :chat (Free for chat)
                this.Presence = PresenceEnum.Available;
            else if (this.ShowValue == "away") //* :away
                this.Presence = PresenceEnum.Away;
            else if (this.ShowValue == "dnd") //* :dnd (Do not disturb)
                this.Presence = PresenceEnum.DoNotDisturb;
            else if (this.ShowValue == "xa") //* :xa (Extended away)
                this.Presence = PresenceEnum.ExtendedAway;
            else
                this.Presence = PresenceEnum.Offline;
        }

        public PresenceEventArgs(string userName, PresenceEnum presence)
        {
            this.UserName = userName;
            this.Presence = presence;
        }
    }
    #endregion Presence Event
}
