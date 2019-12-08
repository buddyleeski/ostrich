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

namespace Ostrich
{
    public partial class Ostrich : ServiceBase
    {
        //public Dictionary<string, User> Clients = new Dictionary<string, User>();
        //private JabberClient BotClient = null;
        //private ApplicationManager AppMan;

        private List<Thread> jabberThreads = new List<Thread>();

        public Ostrich()
        {
            InitializeComponent();
        }

        static void Main(string[] args)
        {
            ServiceBase.Run(new Ostrich());
        }

        protected override void OnStart(string[] args)
        {
            this.Execute();
        }

        protected override void OnStop()
        {
            this.Close();
        }

        public void Execute()
        {

            var config = Logic.Configuration.OstrichConfig.Get();
            var accounts = config.Plugins.ToList().GroupBy(plugin => new { plugin.JabberAccount.Server, plugin.JabberAccount.User }).ToList();

            this.jabberThreads = new List<Thread>();

            accounts.ForEach(account =>
            {
                ParameterizedThreadStart threadStart = new ParameterizedThreadStart(JabberInterface.Start);
                
                Thread t = new Thread(threadStart);
                t.Start(account.ToList());
                
                this.jabberThreads.Add(t);
            });
        }

        //private void BotClient_OnReadText(object sender, string txt)
        //{
        //    Console.WriteLine(txt);
        //}

        //private void BotClient_OnPresence(object sender, jabber.protocol.client.Presence pres)
        //{
        //    Console.WriteLine(pres.ToString());
        //}

        //private void BotClient_OnIQ(object sender, jabber.protocol.client.IQ iq)
        //{
        //    Console.WriteLine(iq.ToString());
        //}

        //private void BotClient_OnMessage(object sender, jabber.protocol.client.Message msg)
        //{
        //    if (msg.Body == "" || msg.Body == null) return;

        //    JabberClient temp = (JabberClient)sender;

        //    ApplicationUser usr;
        //    string usrname = string.Format("{0}@{1}", msg.From.User.ToString(), msg.From.Server.ToString());
        //    if (!this.AppMan.AppUsers.TryGetValue(usrname, out usr))
        //    {
        //        usr = this.AppMan.AddUser(usrname);
        //        usr.SystemReply += new SystemReplyHandler(usr_SystemReply);
        //        usr.TriggerApplicationEvent += new TriggerApplicationEventHandler(usr_TriggerApplicationEvent);
        //        usr.Tag = msg.From.ToString();
        //    }

        //    usr.Input = msg.Body;
        //}

        //void usr_TriggerApplicationEvent(ApplicationUserWrapper sender, ApplicationEventArgs e)
        //{
        //    var userList = this.AppMan.AppUsers.ToList();
        //    userList.ForEach(user => 
        //    {
        //        user.Value.NextEvent = e;
        //    });
        //}

        //void usr_SystemReply(object sender, SystemReplyEventArgs e)
        //{
        //    ApplicationUserWrapper usr = sender as ApplicationUserWrapper;
        //    if (usr == null)
        //        throw new NullReferenceException("User is null");
        //    this.BotClient.Message((string)usr.Tag, e.Reply);
        //}

        //void auth_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //}

        //private void BotClient_OnError(object sender, Exception ex)
        //{
        //    Console.WriteLine("Error: " + ex.ToString());
        //}

        //const bool VERBOSE = true;

        //private void BotClient_OnWriteText(object sender, string txt)
        //{
        //    if (txt == " ") return;  // ignore keep-alive spaces
        //    Console.WriteLine("SEND: " + txt);
        //}

        //private void BotClient_OnAuthenticate(object sender) { }

        public void Close()
        {
            this.jabberThreads.ForEach(t => 
            {
                t.Abort();
            });
        }
    }
}
