using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Json;

namespace Ostrich.Tracker.DataAccess.Helper
{
    public static class JSON
    {
        public static T Deserialise<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T)serializer.ReadObject(ms); // <== Your missing line
                return obj;
            }
        }

        public static string Serialize<T>(T data)
        {
            //T obj = Activator.CreateInstance<T>();
            MemoryStream ms = new MemoryStream();

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(data.GetType());
            serializer.WriteObject(ms, data);


            ms.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(ms);
            return reader.ReadToEnd();
        }
    }
}
