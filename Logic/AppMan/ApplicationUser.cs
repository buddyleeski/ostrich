using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Ostrich.Logic.Base;
using System.Threading;
using Ostrich.Logic.AppMan;
using Ostrich.Logic.Utilities;
using System.Text.RegularExpressions;
using Ostrich.Logic.Enums;
using System.Diagnostics;
using System.Collections;

namespace Ostrich.Logic
{

    /// <summary>
    /// This class will contain user information and state. This will probably parse ... stuff
    /// </summary>
    public class ApplicationUser
    {
        #region Properties

        //Public
        public string UserId { get; set; }
        public object Tag { get; set; }
        public PresenceEnum Presence { get; set; }
        public string Input
        {
            get
            {
                Monitor.Enter(this.InputQueue);
                if (this.InputQueue.Count <= 0)
                    Monitor.Wait(this.InputQueue);

                string result = this.InputQueue.Dequeue();

                Monitor.Exit(this.InputQueue);

                return result;

            }
            set
            {
                Monitor.Enter(this.InputQueue);

                this.InputQueue.Enqueue(value);
                Monitor.Pulse(this.InputQueue);
                Monitor.Exit(this.InputQueue);
                
            }
        }

        public string GetInput(int milliseconds)
        {
            Monitor.Enter(this.InputQueue);
            if (this.InputQueue.Count <= 0)
            {
                Monitor.Wait(this.InputQueue, milliseconds);
                if (this.InputQueue.Count <= 0)
                    return "";
            }

            string result = this.InputQueue.Dequeue();

            Monitor.Exit(this.InputQueue);

            return result;
        }

        public bool Exit = false;

        public ApplicationEventArgs NextEvent
        {
            get
            {
                ApplicationEventArgs result = null;
                Monitor.Enter(this.EventQueue);

                if (this.EventQueue.Any())
                    result = this.EventQueue.Dequeue();

                Monitor.Exit(this.EventQueue);

                return result;
            }
            set
            {
                Monitor.Enter(this.EventQueue);
                this.EventQueue.Enqueue(value);
                Monitor.Exit(this.EventQueue);
            }
        }

        public string Parameter
        {
            get
            {
                Monitor.Enter(this.InputQueue);
                
                if (!this.ParameterQueue.Any())
                {
                    if (this.InputQueue.Count <= 0)
                        Monitor.Wait(this.InputQueue);

                    string input = this.InputQueue.Dequeue();
                    var args = input.ToArgs().ToList();
                    args.ForEach(arg => { this.ParameterQueue.Enqueue(arg); });
                }

                string result = this.ParameterQueue.Dequeue();

                Monitor.Exit(this.InputQueue);

                return result;
            }
        }
        public void ClearParameters()
        {
            this.ParameterQueue.Clear();
        }
        public bool HasParameters()
        {
            if (this.ParameterQueue.Any() || this.InputQueue.Any())
                return true;
            return false;
        }

        //Private
        private ApplicationManager AppMan { get; set; }
        private Dictionary<ApplicationInfo, object> AppInstances { get; set; }
        private KeyValuePair<ApplicationInfo, object> CurrentApp { get; set; }
        private Queue<string> InputQueue { get; set; }
        private Queue<string> ParameterQueue { get; set; }
        private Queue<ApplicationEventArgs> EventQueue { get; set; }

        private ApplicationUserWrapper _userWrapper;
        private ApplicationUserWrapper UserWrapper 
        {
            get
            {
                if (_userWrapper == null)
                    _userWrapper = new ApplicationUserWrapper(this);
                return _userWrapper;
            }

            set { _userWrapper = value; }
        }

        private MethodInfo _getArray;
        private MethodInfo _getList;

        //Internal
        internal string CurrentAppKey { get; set; }

        #endregion // Properties

        #region Events

        public event InvalidCommandHandler InvalidCommand;
        public void OnInvalidCommand(InvalidCommandEventArgs args)
        {
            if (InvalidCommand != null)
                InvalidCommand(this.UserWrapper, args);
        }

        public event SystemReplyHandler SystemReply;
        public void OnSystemReply(SystemReplyEventArgs args)
        {
            if (SystemReply != null)
                SystemReply(this.UserWrapper, args);
        }

        public event TriggerApplicationEventHandler TriggerApplicationEvent;
        public void OnTriggerApplicationEvent(ApplicationEventArgs args)
        {
            if (TriggerApplicationEvent != null)
                TriggerApplicationEvent(args.Sender, args);
        }

        public event PresenceEventHandler PresenceEvent;
        public void OnPresenceEvent(PresenceEventArgs args)
        {
            if (PresenceEvent != null)
            {
                PresenceEvent(this.UserWrapper, args);
            }
        }

        #endregion //Events

        #region Methods

        /// <summary>
        /// Entry point for user executed commands.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public void ParseInput(string input)
        {
            if (input.ToLower() == "exit")
            {
                if (this.CurrentAppKey == "" || this.CurrentAppKey == null)
                    SendReply("No application is running.");
                else
                    this.SendReply(this.CloseApp());

            }
            else if (this.CurrentAppKey == null || this.CurrentAppKey == "")
                SendReply(this.OpenApp(input));
            else
                this.Execute(input);
        }

        private string CloseApp()
        {
            string temp = this.CurrentAppKey;
            this.CurrentAppKey = "";

            return string.Format("{0} has been closed.", temp);
        }

        private string OpenApp(string appKey)
        {
            ApplicationInfo app;
            if (this.AppMan.Apps.TryGetValue(appKey, out app))
            {
                object instance;
                if (!this.AppInstances.TryGetValue(app, out instance))
                {
                    
                    instance = app.Asm.CreateInstance(app.AppType.ToString());
                    ApplicationBase appbase = (ApplicationBase)instance;
                    appbase.User = this.UserWrapper;
                    appbase.AppLoad();

                    this.AppInstances.Add(app, instance);
                }

                this.CurrentApp = new KeyValuePair<ApplicationInfo, object>(app, instance);
                this.CurrentAppKey = appKey;

                return string.Format("{0} has been loaded.", appKey);
            }

            return string.Format("Unable to find appliation \"{0}\"", appKey);
        }

        private void Execute(string input)
        {

            if (input.Length > 0)
            {
               
                if (!this.ExecuteMethod(input))
                {
                    InvalidCommandEventArgs e = new InvalidCommandEventArgs(input);
                    OnInvalidCommand(e);
                    if (e.ReturnString != null || e.ReturnString != "")
                        SendReply(e.ReturnString);
                }
            }

            //// Null results means that we have an invalid command

        }

        private bool ExecuteMethod(string input)
        {
            // If this application is in the system...
            ApplicationInfo app = this.CurrentApp.Key;
            object[] parameters = null;
            var args = input.ToArgs();

            ApplicationMethod method = app.Methods.FirstOrDefault(m => 
                                            !m.IsIntervalMethod
                                            && !m.HasParamArray
                                            && m.Names.Where(name => args.Length == m.ParameterCount + name.NameWordCount
                                                && Regex.IsMatch(input, name.RegEx, RegexOptions.IgnoreCase)
                                                && (parameters = this.ParseArgs(args.ToList(), m, name)) != null).FirstOrDefault() != null);

            if (method == null) //try looking for a parameter method
            {

                method = app.Methods.FirstOrDefault(m =>
                                            !m.IsIntervalMethod && m.HasParamArray
                                            && m.Names.Where(name => args.Length >= m.ParameterCount + name.NameWordCount
                                                && Regex.IsMatch(input, name.RegEx, RegexOptions.IgnoreCase)
                                                && (parameters = this.ParseArgs(args.ToList(), m, name)) != null).FirstOrDefault() != null);
            }

            if (method != null)
            {
                // Match the number of params

                object instance;
                if (!this.AppInstances.TryGetValue(app, out instance))
                {
                    instance = app.Asm.CreateInstance(app.AppType.ToString());
                    this.AppInstances.Add(app, instance);
                }

                ((ApplicationBase)instance).AppInfo = app;

                //Fire BeforeCommand Event
                BeforeCommandEventArgs bcArgs = new BeforeCommandEventArgs() { Continue = true };
                ((ApplicationBase)instance).OnBeforeCommand(bcArgs);

                if (bcArgs.Continue)
                {
                    object result = null;

                    try
                    {
                        result = method.Method.Invoke(instance, parameters);
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry("Ostrich ApplicationUser class", ex.Message);
                    }

                    if (result != null && result.ToString() != "")
                        SendReply(result.ToString());
                    this.ClearParameters();
                }

                //Fire AfterCommand Event
                AfterCommandEventArgs acArgs = new AfterCommandEventArgs();
                ((ApplicationBase)instance).OnAfterCommand(acArgs);

                return true;
            }

            return false;
        }

        private void ExecuteIntervalMethods()
        {
            // If this application is in the system...
            ApplicationInfo app = this.CurrentApp.Key;

            List<ApplicationMethod> methods = app.Methods.Where(m => m.IsIntervalMethod
                                            && (DateTime.Now - m.LastCallTime).TotalMilliseconds >= m.Interval).ToList();
            app.Methods.ForEach(m => 
            {
                
            });

            methods.ForEach(method =>
            {
                if (method != null)
                {
                    object instance;
                    if (!this.AppInstances.TryGetValue(app, out instance))
                    {
                        instance = app.Asm.CreateInstance(app.AppType.ToString());
                        this.AppInstances.Add(app, instance);
                    }

                    ((ApplicationBase)instance).AppInfo = app;

                    if (method.FireEvents)
                    {
                        //Fire BeforeCommand Event
                        BeforeCommandEventArgs bcArgs = new BeforeCommandEventArgs() { Continue = true };
                        ((ApplicationBase)instance).OnBeforeCommand(bcArgs);

                        if (bcArgs.Continue)
                        {
                            object result = method.Method.Invoke(instance, null);
                            if (result != null && result.ToString() != "")
                                SendReply(result.ToString());
                            this.ClearParameters();
                        }
                    }
                    else
                    {
                        object result = method.Method.Invoke(instance, null);
                        if (result != null && result.ToString() != "")
                            SendReply(result.ToString());
                        this.ClearParameters();
                    }
                    

                    if (method.FireEvents)
                    {
                        //Fire AfterCommand Event
                        AfterCommandEventArgs acArgs = new AfterCommandEventArgs();
                        ((ApplicationBase)instance).OnAfterCommand(acArgs);
                    }

                    method.LastCallTime = DateTime.Now;
                }
            });
        }

        private object[] ParseArgs(List<string> argsIn, ApplicationMethod method, ApplicationMethodName name)
        {
            var argsTemp = new List<string>();

            var commandLine = string.Join(" ", argsIn);
            var parsedParams = Regex.Match(commandLine, name.RegEx, RegexOptions.IgnoreCase).Groups;

            if (parsedParams.Count > 1)
            {
                for (int i = 1; i < parsedParams.Count; i++)
                {
                    argsTemp.Add(parsedParams[i].Value);
                }
            }
            else
            {
                for (int i = 0; i < argsIn.Count; i++)
                {
                    if (i >= name.Name.Count)
                        argsTemp.Add(argsIn[i]);
                    else if (Regex.IsMatch(name.Name[i].Trim(), @"\[\S*\]", RegexOptions.IgnoreCase))
                        argsTemp.Add(argsIn[i]);
                }
            }

            var parameters = method.Method.GetParameters().ToList();
            List<object> result = new List<object>();

            for (int i = 0; i < parameters.Count; i++)
            {

                var type = parameters[i].ParameterType;

                try
                {
                    if ((type.BaseType != null && type.BaseType.FullName == "System.Array") 
                        || typeof(System.Collections.ICollection).IsAssignableFrom(type))
                    {

                        string[] tempArray = Regex.Split(argsTemp[i].Trim(), "\\s+", RegexOptions.IgnoreCase);
                        if (!type.HasElementType || type.GetElementType().FullName != "System.String") //&& !typeof(List<string>).IsAssignableFrom(type)
                        {
                            if (typeof(IList).IsAssignableFrom(type))
                            {
                                var genMethod = this._getList.MakeGenericMethod(type.GetGenericArguments());
                                result.Add(genMethod.Invoke(this, new object[] { tempArray }));
                            }
                            else
                            {
                                var genMethod = this._getArray.MakeGenericMethod(new Type[] { type.GetElementType() });
                                result.Add(genMethod.Invoke(this, new object[] { tempArray }));
                            }
                        }
                        else
                            result.Add(tempArray);
                    }
                    else
                    {

                        result.Add(Convert.ChangeType(argsTemp[i], type));
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            return result.ToArray();
        }

        private T[] GetArray<T>(string[] args)
        {
            T[] result = args.Select(s => (T)Convert.ChangeType(s, typeof(T))).ToArray();

            return result;
        }

        private List<T> GetList<T>(string[] args)
        {
            List<T> result = args.Select(s => (T)Convert.ChangeType(s, typeof(T))).ToList();

            return result;
        }

        public void SendReply(string reply)
        {
            OnSystemReply(new SystemReplyEventArgs(reply));
        }

        private void Start()
        {
            bool isReady = false;

            //Set default applications
            if (this.AppMan.DefaultApplication != "")
                this.OpenApp(this.AppMan.DefaultApplication);

            //TODO: Rework the interval methods
            //Add a timer that sleeps until the next method needs to get executed then executes the method

            while (true && !this.Exit)
            {
                
                if (!(this.CurrentAppKey == "" || this.CurrentAppKey == null))
                {
                    ApplicationEventArgs appEventArgs;
                    while ((appEventArgs = this.NextEvent) != null)
                    {
                        //Fire App Event
                        if (this.CurrentAppKey == appEventArgs.AssemblyName)
                        {
                            var instance = this.CurrentApp.Value as ApplicationBase;
                            instance.OnApplicationEvent(appEventArgs);
                        }
                    }

                    //TODO: Add Error handling here
                    ExecuteIntervalMethods();
                }

                var input = this.GetInput(1000);
                if (!string.IsNullOrEmpty(input))
                    ParseInput(input);
                isReady = false;
            }
        }

        #endregion // Methods

        #region Constructors

        /// <summary>
        /// Developers should create new users via the ApplicationManager
        /// </summary>
        /// <param name="appMan"></param>
        /// <param name="id"></param>
        internal ApplicationUser(ApplicationManager appMan, string id)
        {
            this.AppMan = appMan;
            this.UserId = id;
            this.AppInstances = new Dictionary<ApplicationInfo, object>();
            this.InputQueue = new Queue<string>();
            this.EventQueue = new Queue<ApplicationEventArgs>();
            this.ParameterQueue = new Queue<string>();

            this.UserWrapper = new ApplicationUserWrapper(this);
            this._getArray = this.GetType().GetMethod("GetArray", BindingFlags.NonPublic | BindingFlags.Instance);
            this._getList = this.GetType().GetMethod("GetList", BindingFlags.NonPublic | BindingFlags.Instance);


            ThreadStart ts = delegate() { Start(); };
            Thread msgThread = new Thread(ts);
            msgThread.Start();
        }

        public ApplicationUser() { }
        #endregion // Constructors
    }
}
