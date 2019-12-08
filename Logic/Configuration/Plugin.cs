using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Ostrich.Logic.Configuration
{
    public class OstrichConfig : ConfigurationSection
    {
        [ConfigurationProperty("plugins", IsRequired = false, IsDefaultCollection=true)]
        [ConfigurationCollection(typeof(Plugin), AddItemName = "plugin")]
        public PluginCollection Plugins
        {
            get
            {
                return (PluginCollection)base["plugins"];
            }
        }


        public static OstrichConfig Get()
        {
            return (OstrichConfig)ConfigurationManager.GetSection("ostrichConfig");
        }
    }

    public class Plugin : ConfigurationElement
    {

        [ConfigurationProperty("name", IsRequired = true)]
        public String Name
        {
            get
            {
                return (String)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("assembly", IsRequired = true)]
        public string Assembly
        {
            get
            { return (string)this["assembly"]; }
            set
            { this["assembly"] = value; }
        }


        [ConfigurationProperty("jabberAccount", IsRequired = true)]
        public JabberAccount JabberAccount
        {
            get
            { return (JabberAccount)this["jabberAccount"]; }
            set
            { this["jabberAccount"] = value; }
        }

        [ConfigurationProperty("isDefault", DefaultValue = false, IsRequired = false)]
        public bool IsDefault
        {
            get
            {
                return (bool)this["isDefault"];
            }
            set
            {
                this["isDefault"] = value;
            }
        }
    }

    public class PluginCollection : ConfigurationElementCollection, IList<Plugin>
    {
        public PluginCollection()
        {
 
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Plugin();
        }

        protected override object GetElementKey(ConfigurationElement plugin)
        {
            return ((Plugin)plugin).Name;
        }

        protected override string ElementName
        {
            get
            {
                return "plugin";
            }
        }

        public new IEnumerator<Plugin> GetEnumerator()
        {
            foreach (Plugin plugin in this)
            {
                yield return plugin;
            }
        }

        public void Add(Plugin plugin)
        {
            BaseAdd(plugin, true);
        }

        public void Clear()
        {
            BaseClear();
        }

        public bool Contains(Plugin plugin)
        {
            return !(BaseIndexOf(plugin) < 0);
        }

        public void CopyTo(Plugin[] array, int index)
        {
            base.CopyTo(array, index);
        }

        public bool Remove(Plugin plugin)
        {
            BaseRemove(GetElementKey(plugin));
            return true;
        }

        bool ICollection<Plugin>.IsReadOnly
        {
            get { return IsReadOnly(); }
        }

        public int IndexOf(Plugin plugin)
        {
            return BaseIndexOf(plugin);
        }

        public void Insert(int index, Plugin plugin)
        {
            BaseAdd(index, plugin);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public Plugin this[int index]
        {
            get
            {
                return (Plugin)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

    }


 
    public class JabberAccount : ConfigurationElement
    {
        [ConfigurationProperty("server", IsRequired = false)]
        public string Server
        {
            get
            {
                return (string)this["server"];
            }
            set
            {
                this["server"] = value;
            }
        }

        [ConfigurationProperty("user", IsRequired = false)]
        public string User
        {
            get
            {
                return (string)this["user"];
            }
            set
            { this["user"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get
            {
                return (string)this["password"];
            }
            set
            { this["password"] = value; }
        }
    }
}
