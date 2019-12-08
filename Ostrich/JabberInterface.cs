using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Ostrich.Logic;
using jabber.client;
using System.Threading;
using System.Configuration;
using Ostrich.Logic.AppMan;
using System.Collections;
using Ostrich.Logic.Configuration;

namespace Ostrich
{
    public partial class JabberInterface
    {
        public Dictionary<string, User> Clients = new Dictionary<string, User>();
        private JabberClient BotClient = null;
        private ApplicationManager AppMan;
        private List<Plugin> Plugins;

        public JabberInterface()
        {
            
        }

        public static void Start(object plugins)
        {
            while (true)
            {
                var jabber = new JabberInterface();
                jabber.Execute((List<Logic.Configuration.Plugin>)plugins);
            }
        }

        protected void OnStop()
        {
            this.Close();
        }

        public void Execute(List<Logic.Configuration.Plugin> plugins)
        {
            try
            {
                this.Plugins = plugins;
                var account = plugins.First().JabberAccount;

                this.AppMan = new ApplicationManager(plugins);

                this.BotClient = new JabberClient();

                this.BotClient.User = account.User;
                this.BotClient.Server = account.Server;
                this.BotClient.Password = account.Password;

                this.BotClient.AutoPresence = true;
                this.BotClient.AutoRoster = true;
                this.BotClient.AutoReconnect = 10;

                // listen for errors.  Always do this!
                this.BotClient.OnError += new bedrock.ExceptionHandler(BotClient_OnError);

                this.BotClient.OnAuthenticate += new bedrock.ObjectHandler(BotClient_OnAuthenticate);

                this.BotClient.OnMessage += new MessageHandler(BotClient_OnMessage);
                this.BotClient.OnIQ += new IQHandler(BotClient_OnIQ);
                this.BotClient.OnPresence += new PresenceHandler(BotClient_OnPresence);
                this.BotClient.OnReadText += new bedrock.TextHandler(BotClient_OnReadText);

                RosterManager rosterManager = new RosterManager();
                rosterManager.Client = this.BotClient;
                rosterManager.AutoAllow = AutoSubscriptionHanding.AllowAll;

                if (VERBOSE)
                    this.BotClient.OnWriteText += new bedrock.TextHandler(BotClient_OnWriteText);

                this.BotClient.Connect();

                Thread.Sleep(2000);

                try
                {
                    this.BotClient.GetAgents();
                }
                catch (NullReferenceException ex)
                {
                    EventLog.WriteEntry("JabberInterface", ex.Message);
                }

                while (true)
                {
                    Thread.Sleep(60000);
                }


            }
            catch (ThreadAbortException)
            {
                this.Close();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("JabberInterface", ex.Message);
                this.Close();
            }
        }

        private void BotClient_OnReadText(object sender, string txt)
        {
            Console.WriteLine(txt);
        }

        private void BotClient_OnPresence(object sender, jabber.protocol.client.Presence pres)
        {
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            Console.WriteLine(pres.ToString());
            Console.WriteLine("***********************************");
            Console.WriteLine(pres.Type.ToString()); // Available/Unavailable
            Console.WriteLine(pres.Value);
            Console.WriteLine(pres.Show); // dnd / away / ""
            Console.WriteLine("------------------------");
            Console.WriteLine(pres.HasAttribute("type") ? pres.GetAttribute("type").ToString() : "");
            Console.WriteLine("***********************************");

            JabberClient temp = (JabberClient)sender;

            ApplicationUser usr;
            string usrname = string.Format("{0}@{1}", pres.From.User.ToString(), pres.From.Server.ToString());
            if (this.AppMan.AppUsers.TryGetValue(usrname, out usr))
            {
                PresenceEventArgs presenceArgs = new PresenceEventArgs(usr.UserId, pres.Show, pres.Type.ToString());
                usr.Presence = presenceArgs.Presence;
                usr.OnPresenceEvent(presenceArgs);

            }
            else
            {
                if (!string.IsNullOrEmpty(this.AppMan.DefaultApplication))
                {
                    var app = this.AppMan.Apps[this.AppMan.DefaultApplication];
                    var method = app.AppType.GetMethod("InitiateConversation");
                    
                    if (method != null && (bool)method.Invoke(null, new object[] { usrname }))
                    {
                        usr = this.AppMan.AddUser(usrname);
                        usr.SystemReply += new SystemReplyHandler(usr_SystemReply);
                        usr.TriggerApplicationEvent += new TriggerApplicationEventHandler(usr_TriggerApplicationEvent);
                        usr.Tag = pres.From.ToString();
                        
                        PresenceEventArgs presenceArgs = new PresenceEventArgs(usr.UserId, pres.Show, pres.Type.ToString());
                        usr.Presence = presenceArgs.Presence;
                    }       
                }
            }

                // SHOW VALUE
                //* nil (Available, no <show/> element)
                //* :away
                //* :chat (Free for chat)
                //* :dnd (Do not disturb)
                //* :xa (Extended away)

                //Type Values
                //* :error
                //* :probe (Servers send this to request presence information)
                //* :subscribe (Subscription request)
                //* :subscribed (Subscription approval)
                //* :unavailable (User has gone offline)
                //* :unsubscribe (Unsubscription request)
                //* :unsubscribed (Unsubscription approval)
                //* [nil] (available)

        }

        private void BotClient_OnIQ(object sender, jabber.protocol.client.IQ iq)
        {
            Console.WriteLine(iq.ToString());
        }

        private void BotClient_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            if (msg.Body == "" || msg.Body == null) return;

            JabberClient temp = (JabberClient)sender;

            ApplicationUser usr;
            string usrname = string.Format("{0}@{1}", msg.From.User.ToString(), msg.From.Server.ToString());
            if (!this.AppMan.AppUsers.TryGetValue(usrname, out usr))
            {
                usr = this.AppMan.AddUser(usrname);
                usr.SystemReply += new SystemReplyHandler(usr_SystemReply);
                usr.TriggerApplicationEvent += new TriggerApplicationEventHandler(usr_TriggerApplicationEvent);
                usr.Tag = msg.From.ToString();
            }

            usr.Input = msg.Body;
        }

        void usr_TriggerApplicationEvent(ApplicationUserWrapper sender, ApplicationEventArgs e)
        {
            var userList = this.AppMan.AppUsers.ToList();
            userList.ForEach(user => 
            {
                user.Value.NextEvent = e;
            });
        }

        void usr_SystemReply(object sender, SystemReplyEventArgs e)
        {
            ApplicationUserWrapper usr = sender as ApplicationUserWrapper;
            if (usr == null)
                throw new NullReferenceException("User is null");
            this.BotClient.Message((string)usr.Tag, e.Reply);
        }

        void auth_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        private void BotClient_OnError(object sender, Exception ex)
        {
            Console.WriteLine("Error: " + ex.ToString());
        }

        const bool VERBOSE = true;

        private void BotClient_OnWriteText(object sender, string txt)
        {
            if (txt == " ") return;  // ignore keep-alive spaces
            Console.WriteLine("SEND: " + txt);
        }

        private void BotClient_OnAuthenticate(object sender) { }

        public void Close()
        {
            this.BotClient.Close();
            this.AppMan.AppUsers.ToList().ForEach(usr => 
            {
                usr.Value.Exit = true;
            });
        }
    }
}