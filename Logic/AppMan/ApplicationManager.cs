using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Ostrich.Logic.Configuration;

namespace Ostrich.Logic
{
    public class ApplicationManager
    {
        #region Properties
        
        //Public
        public Dictionary<string, ApplicationInfo> Apps { get; set; }
        public Dictionary<string, ApplicationUser> AppUsers { get; set; }
        public string DefaultApplication = "";

        #endregion // Properties

        #region Methods

        public ApplicationUser AddUser(string id)
        {
            ApplicationUser usr = new ApplicationUser(this, id);
            this.AppUsers.Add(id, usr);
            return usr;
        }

        #endregion // Methods

        #region Constructors

        public ApplicationManager(List<Plugin> plugins)
        {
            this.Apps = new Dictionary<string, ApplicationInfo>();
            this.AppUsers = new Dictionary<string, ApplicationUser>();

            plugins.ForEach(plugin => 
            {
                FileInfo appFile = new FileInfo(plugin.Assembly);
                ApplicationInfo app = ApplicationInfo.NewApplicationInfo(appFile);

                if (app != null)
                {
                    if (this.Apps.ContainsKey(plugin.Name))
                    {
                        string key = string.Format("{0}{1}", plugin.Name, this.Apps.Where(a => a.Key.StartsWith(plugin.Name)).Count());
                        this.Apps.Add(key, app);
                        if (plugin.IsDefault)
                            this.DefaultApplication = key;
                    }
                    else
                    {
                        this.Apps.Add(plugin.Name, app);
                        if (plugin.IsDefault)
                            this.DefaultApplication = plugin.Name;
                    }
                }
            });
        }

        #endregion // Constructors
    }
}
