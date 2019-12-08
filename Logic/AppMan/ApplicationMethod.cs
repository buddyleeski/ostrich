using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Ostrich.Logic.Attributes;
using System.Collections;

namespace Ostrich.Logic.AppMan
{
    public class ApplicationMethod
    {
        #region Properties

        public List<ApplicationMethodName> Names { get; private set; }
        public int ParameterCount { get; private set; }
        public string Description { get; private set; }
        public MethodInfo Method { get; private set; }
        public ApplicationInfo AppInfo { get; private set; }
        public List<int> ListParameterIndexes { get; private set; }

        public bool IsIntervalMethod { get; private set; }
        public int Interval { get; private set; }
        public DateTime LastCallTime { get; set; }

        public bool HasParamArray { get; private set; }
        public int Rank { get; set; }

        /// <summary>
        /// Determines if the BeforeCommand and AfterCommand events get fired
        /// </summary>
        public bool FireEvents { get; set; }

        #endregion // Properties

        #region Constructors
        public ApplicationMethod(ApplicationInfo app, MethodInfo method)
        {
            this.ListParameterIndexes = new List<int>();

            this.Method = method;
            this.AppInfo = app;

            var methodParams = method.GetParameters();
            if (methodParams.Any())
            {
                for (int i = 0; i < methodParams.Length; i++)
                {
                    var p = methodParams[i];
           
                    var paramArrayAttr = p.GetCustomAttributes(typeof(ParamArrayAttribute), false).SingleOrDefault() as ParamArrayAttribute;

                    if (paramArrayAttr != null || typeof(ICollection).IsAssignableFrom(p.ParameterType))
                    {
                        this.HasParamArray = true;
                        this.ListParameterIndexes.Add(i);
                    }
                }
            }

            var methodNames = method.GetCustomAttributes(typeof(MethodNameAttribute), false).Select(name => name as MethodNameAttribute).ToList();

            this.Names = new List<ApplicationMethodName>();

            methodNames.ForEach(methodName => 
            {
                if (methodName != null && methodName.Words != null && methodName.Words.Length != 0)
                    this.Names.Add(new ApplicationMethodName(methodName.Words, this.HasParamArray, this.ListParameterIndexes));
            });


            if (!this.Names.Any())
                this.Names.Add(new ApplicationMethodName(method.Name, this.HasParamArray, new List<int>()));

            var methodDescription = method.GetCustomAttributes(typeof(DescriptionAttribute), false).SingleOrDefault() as DescriptionAttribute;
            if (methodDescription != null)
                this.Description = methodDescription.Value;
            else
                this.Description = "";

            this.ParameterCount = method.GetParameters().Count();

            //Is this an interval method?
            var intervalInfo = method.GetCustomAttributes(typeof(IntervalAttribute), false).SingleOrDefault() as IntervalAttribute; 

            if (intervalInfo != null)
            {
                this.IsIntervalMethod = true;
                this.Interval = intervalInfo.Frequency;
                this.LastCallTime = DateTime.Now;
            }

            var rankAttr = method.GetCustomAttributes(typeof(MethodRankAttribute), false).SingleOrDefault() as MethodRankAttribute;
            if (rankAttr != null)
                this.Rank = rankAttr.Rank;
            else
                this.Rank = 9999999;
        }
        #endregion // Constructors
    }
}
