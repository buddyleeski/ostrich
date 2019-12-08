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
using Communicator;

namespace Ostrich
{
    public partial class CommunicatorInterface
    {
        public Dictionary<string, User> Clients = new Dictionary<string, User>();
        private CommunicatorWrapper BotClient = null;
        private ApplicationManager AppMan;
        private List<Plugin> Plugins;

        public CommunicatorInterface()
        {

        }

        public static void Start(object plugins)
        {
            while (true)
            {
                var jabber = new CommunicatorInterface();
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

                this.BotClient = new CommunicatorWrapper("ql1ocs2t.mi.corp.rockfin.com", "MI", "OstrichQLClient", "Welcome!", "sip:OstrichQLClient@quickenloans.com");


                this.BotClient.MessageReceived += BotClient_MessageReceived;




                this.BotClient.InitClient();
               

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
                EventLog.WriteEntry("CommunicatorInterface", ex.Message);
                this.Close();
            }
        }

        void BotClient_MessageReceived(Microsoft.Office.Interop.UccApi.IUccInstantMessagingSession session, Microsoft.Office.Interop.UccApi.UccIncomingInstantMessageEvent data)
        {
            
            Console.WriteLine(data.ContentType);
            Console.WriteLine(data.Content);
            session.SendMessage("text/plain", "Response", null);

            if (string.IsNullOrEmpty(data.Content)) return;

            //JabberClient temp = (JabberClient)sender;

            ApplicationUser usr;
            string usrname = data.ParticipantEndpoint.Uri.Value;
            if (!this.AppMan.AppUsers.TryGetValue(usrname, out usr))
            {
                usr = this.AppMan.AddUser(usrname);
                usr.SystemReply += new SystemReplyHandler(usr_SystemReply);
                usr.TriggerApplicationEvent += new TriggerApplicationEventHandler(usr_TriggerApplicationEvent);
                //usr.Tag = data.;
            }

            usr.Input = data.Content;
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
            this.BotClient.SendNewMessage((string)usr.Tag, e.Reply);
        }

        void auth_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        public void Close()
        {
            //this.BotClient.
            this.AppMan.AppUsers.ToList().ForEach(usr =>
            {
                usr.Value.Exit = true;
            });
        }
    }
}