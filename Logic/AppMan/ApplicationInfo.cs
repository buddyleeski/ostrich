using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Ostrich.Logic.Attributes;
using System.IO;
using Ostrich.Logic.AppMan;
using Ostrich.Logic.Base;

namespace Ostrich.Logic
{
    public class ApplicationInfo
    {
        #region Properties 

        public Assembly Asm { get; set; }
        public Type AppType { get; set; }
        public string Prefix { get; set; }
        public string Name { get; set; }
        public List<ApplicationMethod> Methods { get; set; }

        #endregion // Properties

        #region Constructors

        //Not to be used
        private ApplicationInfo() { }
        private ApplicationInfo(string filePath)
        {
            this.Asm = Assembly.LoadFile(filePath);
            this.AppType = this.Asm.GetTypes().FirstOrDefault(tp => tp.IsAssignableFrom(typeof(ApplicationBase)));

            if (this.AppType == null)
                throw new NotImplementedException();
            //this.Methods = this.AppType.GetMethods().ToDictionary(m => m.Name, n =>n);
        }

        public static ApplicationInfo NewApplicationInfo(FileInfo file)
        {
            string filePath = file.FullName;
            ApplicationInfo info = new ApplicationInfo();
            info.Asm = Assembly.LoadFile(filePath);

            var appType = info.Asm.GetTypes().FirstOrDefault(t => typeof(ApplicationBase).IsAssignableFrom(t));

            if (appType == null) 
                return null;
            
            var nameAttribute = appType.GetCustomAttributes(false).SingleOrDefault(attr => attr is ApplicationNameAttribute) as ApplicationNameAttribute;
            if (nameAttribute != null)
            {
                info.Name = nameAttribute.Name;
                info.Prefix = nameAttribute.Prefix;
            }
            else
                info.Name = System.IO.Path.GetFileNameWithoutExtension(file.Name);
            
            info.AppType = appType;

            info.Methods = appType.GetMethods().Select(method => new ApplicationMethod(info, method)).OrderBy(i => i.Rank).ToList();

            return info;
        }

        #endregion // Constructors

        
    }
}
