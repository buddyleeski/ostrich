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

namespace Ostrich
{
    public partial class Test
    {
        public Dictionary<string, User> Clients = new Dictionary<string, User>();
        private JabberClient BotClient = null;
        private ApplicationManager AppMan;

        public Test()
        {
            
        }

        static void Main(string[] args)
        {
            //Test t = new Test();
            //t.Execute();
             
            var config = Logic.Configuration.OstrichConfig.Get();
            var accounts = config.Plugins.ToList().GroupBy(plugin => new { plugin.JabberAccount.Server, plugin.JabberAccount.User }).ToList();

            List<Thread> jabberThreads = new List<Thread>();

            accounts.ForEach(account =>
            {
                ParameterizedThreadStart threadStart = new ParameterizedThreadStart(JabberInterface.Start);
                
                Thread t = new Thread(threadStart);
                t.Start(account.ToList());

                jabberThreads.Add(t);
            });

            Thread.Sleep(1200000);

            jabberThreads.ForEach(t => { t.Abort(); });

            jabberThreads.ForEach(t => { t.Join();  });
        }

       
    }
}
