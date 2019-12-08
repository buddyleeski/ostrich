using System;

using System.Collections.Generic;

using System.ComponentModel;

using System.Data;

using System.Text;

//using Microsoft.Office.Interop.UccApi;

using System.Runtime.InteropServices.ComTypes;

using System.Diagnostics;
using Microsoft.Office.Interop.UccApi;


namespace Communicator
{
    class Program 
    {
        #region Test
            #region hide
            private string _password = "Welcome!";
            #endregion
        #endregion

        private IUccSessionManager sessionManager = null;
        private IUccEndpoint endpoint = null;
        private UccApplicationSession _appBase = null;
        private IUccSession _session = null;
        private IUccUriManager _uriManager = null;
        

        static void Main(string[] args)
        {
            CommunicatorWrapper wrapper = new CommunicatorWrapper("ql1ocs2t.mi.corp.rockfin.com", "MI", "OstrichQLClient", "Welcome!", "sip:OstrichQLClient@quickenloans.com");
            wrapper.InitClient();
            wrapper.MessageReceived += wrapper_MessageReceived;
            
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        static void wrapper_MessageReceived(Microsoft.Office.Interop.UccApi.IUccInstantMessagingSession session, Microsoft.Office.Interop.UccApi.UccIncomingInstantMessageEvent data)
        {

            Console.WriteLine(data.ContentType);
            Console.WriteLine(data.Content);
            session.SendMessage("text/plain", "Response", null);
        }

    }
}
