using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Ostrich.Logic.Utilities
{
    public static class ExtensionMethods
    {
        #region String Extension
        
        public static string[] ToArgs(this string str)
        {
            if (str == null || !(str.Length > 0)) return new string[0];
            int idx = str.Trim().IndexOf(" ");
            if (idx == -1) return new string[] { str };
            int count = str.Length;
            ArrayList list = new ArrayList();
            while (count > 0)
            {
                if (str[0] == '"')
                {
                    int temp = str.IndexOf("\"", 1, str.Length - 1);
                    while (str[temp - 1] == '\\')
                    {
                        temp = str.IndexOf("\"", temp + 1, str.Length - temp - 1);
                    }
                    idx = temp + 1;
                }
                if (str[0] == '\'')
                {
                    int temp = str.IndexOf("\'", 1, str.Length - 1);
                    while (str[temp - 1] == '\\')
                    {
                        temp = str.IndexOf("\'", temp + 1, str.Length - temp - 1);
                    }
                    idx = temp + 1;
                }
                string s = str.Substring(0, idx);
                int left = count - idx;
                str = str.Substring(idx, left).Trim();
                list.Add(s.Trim('"'));
                count = str.Length;
                idx = str.IndexOf(" ");
                if (idx == -1)
                {
                    string add = str.Trim('"', ' ');
                    if (add.Length > 0)
                    {
                        list.Add(add);
                    }
                    break;
                }
            }
            return (string[])list.ToArray(typeof(string));
        }
        
        #endregion // String Extensions

        #region String[] Extensions
        
        public static bool StartsWith(this string[] array, string[] startsWith)
        {
            if (startsWith.Length > array.Length)
                return false;

            for (int i = 0; i < startsWith.Length; i++)
            {
                if (!array[i].Equals(startsWith[i], StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            return true;
        }

        public static bool StartsWith(this string[] array, List<string> startsWith)
        {
            if (startsWith.Count > array.Length)
                return false;

            for (int i = 0; i < startsWith.Count; i++)
            {
                if (!array[i].Equals(startsWith[i], StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            return true;
        }

        #endregion // String[] Extensions

        #region List<String> Extensions
        
        public static bool StartsWith(this List<string> array, String[] startsWith)
        {
            if (startsWith.Length > array.Count)
                return false;

            for (int i = 0; i < startsWith.Length; i++)
            {
                if (!array[i].Equals(startsWith[i], StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            return true;
        }

        public static bool StartsWith(this List<string> array, List<string> startsWith)
        {
            if (startsWith.Count > array.Count)
                return false;

            for (int i = 0; i < startsWith.Count; i++)
            {
                if (!array[i].Equals(startsWith[i], StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            return true;
        }

        #endregion // List<String> Extensions
    }
}
