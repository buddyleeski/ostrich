using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.UccApi;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Linq;
using System.Linq.Expressions;

namespace Communicator
{
    public class CommunicatorWrapper: _IUccEndpointEvents, _IUccSessionManagerEvents, _IUccSessionEvents, _IUccInstantMessagingSessionEvents, _IUccInstantMessagingSessionParticipantEvents
    {
        #region Private Properties

        private string _serverAddress { get; set; }
        private string _domain { get; set; }
        private string _userName { get; set; }
        private string _password { get; set; }
        private IUccEndpoint _mainEndpoint { get; set; }
        private string _sip { get; set; }
        private IUccSessionManager _sessionManager { get; set; }
        private UccUriManager _uriManager { get; set; }
        


        private Dictionary<string, IUccSession> _sessions;

        #endregion // Private Properties

        #region Public Properties

        public UCC_AUTHENTICATION_MODES AuthMode { get; set; }
        public UCC_TRANSPORT_MODE TransportMode { get; set; }
        public string AppName { get; set; }

        #endregion // Public Properties

        #region Constructor
        public CommunicatorWrapper(string server, string domain, string username, string password, string sip)
        {
            this._serverAddress = server;
            this._domain = domain;
            this._userName = username;
            this._password = password;
            this._sip = sip;

            this._sessions = new Dictionary<string, IUccSession>();

            this.AuthMode = UCC_AUTHENTICATION_MODES.UCCAM_NTLM;
            this.TransportMode = UCC_TRANSPORT_MODE.UCCTM_TLS;

            this.AppName = "DefaultAppName";

            this._uriManager = new UccUriManager();
        }
        #endregion // Constructors

        public void InitClient()
        {
            IUccPlatform platform = new UccPlatform();
            platform.Initialize(this.AppName, null);

            IUccUriManager _uriManager = new UccUriManager();
            UccUri uri = _uriManager.ParseUri(this._sip);

            //Create an endpoint object from the platform object
            _mainEndpoint = platform.CreateEndpoint(UCC_ENDPOINT_TYPE.UCCET_PRINCIPAL_SERVER_BASED, uri, null, null);
            IUccServerSignalingSettings serverSignalingSettings = (IUccServerSignalingSettings)_mainEndpoint;

            serverSignalingSettings.Server = serverSignalingSettings.CreateSignalingServer(this._serverAddress, this.TransportMode); //ac.rockfin.com:443 //qlocsfepool.mi.corp.rockfin.com
            serverSignalingSettings.AllowedAuthenticationModes = (int)this.AuthMode;

            UccCredential credential = serverSignalingSettings.CredentialCache.CreateCredential(this._userName, this._password, this._domain);
            serverSignalingSettings.CredentialCache.SetCredential("*", credential);

            //Advise for events
            Advise<_IUccEndpointEvents>(
               this._mainEndpoint,
               this);

            this._mainEndpoint.Enable();

            

            while (this._sessionManager == null)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void SendNewMessage(string user, string msg)
        {
            IUccSession session = null;
            if (!this._sessions.TryGetValue(user.ToLower(), out session))
            {
                var context = new UccContext();
                UccUri participantUri = this._uriManager.ParseUri("sip:MikeSteciuk@quickenloans.com");

                session = this._sessionManager.CreateSession(UCC_SESSION_TYPE.UCCST_INSTANT_MESSAGING, context);
                var p = session.CreateParticipant(participantUri, null);

                Advise<_IUccInstantMessagingSessionParticipantEvents>(p, this);
                session.AddParticipant(p, null);

                Advise<_IUccSessionEvents>(session, this);

                this._sessions.Add(user.ToLower(), session);
            }

            IUccInstantMessagingSession ims = session as IUccInstantMessagingSession;
            ims.SendMessage("text/plain", msg, null);
        }

        private void SendMessage()
        {
            
        }
        


        #region Helper Methods

        private static void Advise<T>(object source, T sink)
        {
            IConnectionPointContainer container = (IConnectionPointContainer)source;
            IConnectionPoint cp;
            int cookie;
            Guid guid = typeof(T).GUID;

            container.FindConnectionPoint(ref guid, out cp);

            cp.Advise(sink, out cookie);
        }

        #endregion // Helper Methods

        #region Interface Methods

        #region _IUccEndpointEvents

        public void OnDisable(IUccEndpoint pEventSource, IUccOperationProgressEvent pEventData)
        {
           
        }

        public void OnEnable(IUccEndpoint pEventSource, IUccOperationProgressEvent pEventData)
        {
            try
            {
                if (pEventData.IsComplete)
                {
                    if (pEventData.StatusCode >= 0)
                    {
                        // Sign in succeeded. Proceed with subscription and publication.     

   
                        // Create a session manager object to handle session related functionality and events.
                        this._sessionManager = pEventSource as IUccSessionManager;

                        // Advise session manager instance of this class as endpoint sink.
                        Advise<_IUccSessionManagerEvents>(this._sessionManager, this);
                        PublishAvailability("Available");

                    }
                    else
                    {
                        // Recover from failure to sign in.
                    }

                }
                else
                {
                    // Sign in failed. Make the endpoint null.
                    pEventSource = null;
                }
            }
            catch (COMException ex)
            {
                throw ex;
            }

        }

        #endregion // _IUccEndpointEvents

        #region _IUccSessionManagerEvents

        public void OnIncomingSession(IUccEndpoint pEventSource, UccIncomingSessionEvent pEventData)
        {
            pEventData.Accept();

            var session = pEventData.Session as IUccSession;

            if (!this._sessions.ContainsKey(pEventData.Inviter.Uri.Value.ToLower()))
            {
                this._sessions.Add(pEventData.Inviter.Uri.Value.ToLower(), session);
            }
            else
            {
                this._sessions[pEventData.Inviter.Uri.Value.ToLower()] = session;
            }

            foreach(var p in pEventData.Session.Participants)
            {
                Advise<_IUccInstantMessagingSessionParticipantEvents>(p, this);
            }
            

            //pEventData.Inviter.Uri.Value
            //if (IncomingSession != null)
            //    IncomingSession(pEventSource, pEventData);
        }


        public delegate void OutgoingSessionHandler(IUccEndpoint sender, UccOutgoingSessionEvent data);
        public event OutgoingSessionHandler OutgoingSession;
        public void OnOutgoingSession(IUccEndpoint pEventSource, UccOutgoingSessionEvent pEventData)
        {
            if (OutgoingSession != null)
                OutgoingSession(pEventSource, pEventData);
        }

        #endregion // _IUccSessionManagerEvents

        #region _IUccSessionEvents
        public delegate void AddParticipantHandler(IUccSession sender, IUccOperationProgressEvent data);
        public event AddParticipantHandler AddParticipant;
        public void OnAddParticipant(IUccSession pEventSource, IUccOperationProgressEvent pEventData)
        {
            if (AddParticipant != null)
                AddParticipant(pEventSource, pEventData);
        }


        public delegate void RemoveParticipantHandler(IUccSession sender, IUccOperationProgressEvent data);
        public event RemoveParticipantHandler RemoveParticipant;
        public void OnRemoveParticipant(IUccSession pEventSource, IUccOperationProgressEvent pEventData)
        {
            if (RemoveParticipant != null)
                RemoveParticipant(pEventSource, pEventData);
        }


        public delegate void TerminatedHandler(IUccSession sender, IUccOperationProgressEvent data);
        public event TerminatedHandler Terminated;
        public void OnTerminate(IUccSession pEventSource, IUccOperationProgressEvent pEventData)
        {
            if (Terminated != null)
                Terminated(pEventSource, pEventData);
        }

        #endregion // _IUccSessionEvents

        #region _IUccInstantMessagingSessionEvents


        public delegate void SendingMessageHandler(UccInstantMessagingSession sender, UccSessionOperationEvent data);
        public event SendingMessageHandler SendingMessage;
        public void OnSendMessage(UccInstantMessagingSession pEventSource, UccSessionOperationEvent pEventData)
        {
            if (SendingMessage != null)
                SendingMessage(pEventSource, pEventData);
        }

        #endregion // _IUccInstantMessagingSessionEvents

        #region _IUccInstantMessagingSessionParticipantEvents

        public void OnComposing(UccInstantMessagingSessionParticipant pEventSource, UccInstantMessagingComposingEvent pEventData)
        {
        }

        public void OnIdle(UccInstantMessagingSessionParticipant pEventSource, UccInstantMessagingComposingEvent pEventData)
        {
        }


        public delegate void MessageReceivedHandler(IUccInstantMessagingSession session, UccIncomingInstantMessageEvent data);
        public event MessageReceivedHandler MessageReceived;
        public void OnInstantMessageReceived(UccInstantMessagingSessionParticipant pEventSource, UccIncomingInstantMessageEvent pEventData)
        {
            IUccSession session = null;
            if (this._sessions.TryGetValue(pEventData.ParticipantEndpoint.Uri.Value.ToLower(), out session))
            {
                if (MessageReceived != null)
                    MessageReceived(session as IUccInstantMessagingSession, pEventData);
            }
            
        }

        #endregion // _IUccInstantMessagingSessionParticipantEvents

        #endregion // Interface Methods





        public void PublishAvailability(string strAvail)
        {
            // Create a publishable category instance for the "state" 
            // category to publish the user state. This is indicated by 
            // setting the Type to "machinestate". The availability for
            // userState is set to the availability specifed by 
            // the user.          
            IUccPublicationManager publicationManager = this._sessionManager as IUccPublicationManager;
            
            IUccCategoryInstance catStateForUserState =
                publicationManager.CreatePublishableCategoryInstance(
                  "state",                     // category name
                  2,                           // container ID
                  0x20000000,                  // instance ID
                  UCC_CATEGORY_INSTANCE_EXPIRE_TYPE.UCCCIET_USER,
                  0);
            IUccPresenceStateInstance userState =
                catStateForUserState as IUccPresenceStateInstance;
            userState.IsManual = true;
            userState.Type = UCC_PRESENCE_STATE_TYPE.UCCPST_USER_STATE;

            //Convert availablity string to integer for publication
            userState.Availability = AvailabilityStringToInt(strAvail);

            catStateForUserState.PublicationOperation = UCC_PUBLICATION_OPERATION_TYPE.UCCPOT_ADD;

            // Create a publishable category instance for "state" 
            // category to publish the machine state.
            // This is indicated by setting the Type to "machinestate".
            // The availability for machinestate is set to 3500 - "Available". 
            // The server aggregates the userstate and machinestate
            // to compute an aggregate state which is then 
            // published on behalf of the user. It is important to 
            // publish machinestate to make this aggregation possible.
            IUccCategoryInstance catStateForMachineState =
               publicationManager.CreatePublishableCategoryInstance(
                  "state",
                  2,
                  0x30000000,
                  UCC_CATEGORY_INSTANCE_EXPIRE_TYPE.UCCCIET_DEVICE,
                  10000);
            IUccPresenceStateInstance machineState = catStateForMachineState as IUccPresenceStateInstance;
            machineState.IsManual = true;
            machineState.Availability = 3500;
            machineState.Type = UCC_PRESENCE_STATE_TYPE.UCCPST_MACHINE_STATE;
            catStateForMachineState.PublicationOperation = UCC_PUBLICATION_OPERATION_TYPE.UCCPOT_ADD;
            // Create a publication and advise for publication events.
            IUccPublication pub = publicationManager.CreatePublication();
            //Advise<_IUccPublicationEvent>(pub, this);

            // Add the two publishable category instances 
            // to the publication.
            pub.AddPublishableCategoryInstance(
                                  catStateForUserState);
            pub.AddPublishableCategoryInstance(
                                  catStateForMachineState);
            // Publish the publication.
            pub.Publish(null);
        }

        /// <summary>
        /// Converts availability from a string to an integer.
        /// </summary>
        /// <param name="avail">String representation of availability</param>
        /// <returns>Integer representation of availability</returns>
        private int AvailabilityStringToInt(string avail)
        {
            if (avail == "Available")
                return 3500;
            else if (avail == "Busy")
                return 6500;
            else if (avail == "Do Not Disturb")
                return 9500;
            else if (avail == "Away")
                return 15500;
            else if (avail == "Appear Offline")
                return 18500;
            else
                return 0;
        }




    }
}
