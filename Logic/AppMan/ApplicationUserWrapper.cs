using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ostrich.Logic.Enums;

namespace Ostrich.Logic.AppMan
{
    public class ApplicationUserWrapper
    {
       
        #region Properties

        //Private
        private ApplicationUser user;

        //Public
        public string UserId
        {
            get { return this.user.UserId; }
        }
        public object Tag 
        {
            get { return this.user.Tag; }
            set { this.user.Tag = value; }
            
        }
        public string Input
        {
            get
            {
                return this.user.Input;
            }
        }

        public PresenceEnum Presence
        {
            get
            {
                return this.user.Presence;
            }
        }

        public string Parameter
        {
            get
            {
                return this.user.Parameter;
            }
        }
        public void ClearParameters()
        {
            this.user.ClearParameters();
        }
        public bool HasParameters()
        {
            return this.user.HasParameters();
        }

        internal string CurrentAppKey
        {
            get { return this.user.CurrentAppKey; }
        }

        #endregion // Properties

        #region Methods
        
        public void SendUpdate(string reply)
        {
            this.user.OnSystemReply(new SystemReplyEventArgs(reply));
        }

        public void TriggerEvent(string eventName, object data)
        {
            ApplicationEventArgs args = new ApplicationEventArgs(this, eventName, data);
            this.user.OnTriggerApplicationEvent(args);
        }

        public string GetInput(int milliseconds)
        {
            return this.user.GetInput(milliseconds);
        }
        #endregion // Methods

        #region Events

        public event PresenceEventHandler PresenceEvent
        {
            add
            {
                this.user.PresenceEvent += value;
            }
            remove
            {
                this.user.PresenceEvent -= value;
            }
        }

        #endregion Events

        #region Constructor

        public ApplicationUserWrapper(ApplicationUser u)
        {
            this.user = u;
        }

        #endregion // Constructor


    }
}
