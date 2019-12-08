using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Ostrich.Tracker.DataAccess.Helper
{
    public static class Request
    {
        public static string FormatData(string value)
        {
            return string.Format("Data={0}", value);
        }

        /// <summary>
        /// Create message sent to create new database object.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string PutMessage(string endPoint, string data, string key)
        {
            WebRequest request = HttpWebRequest.Create(endPoint);

            request.Headers.Add("OfficialKey", key);
            request.Method = "PUT";

            request.ContentLength = data.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            // Post to the login form.
            StreamWriter swRequestWriter = new
            StreamWriter(request.GetRequestStream());
            swRequestWriter.Write(data);
            swRequestWriter.Close();

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format("POST failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response  
                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                }
                return responseValue;
            }
        }

        /// <summary>
        /// Post message sent to update existing database object.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string PostMessage(string endPoint, string data, string key)
        {
            WebRequest request = HttpWebRequest.Create(endPoint);

            request.Headers.Add("OfficialKey", key);
            request.Method = "POST";

            request.ContentLength = data.Length;
            request.ContentType = "application/x-www-form-urlencoded";

            // Post to the login form.
            StreamWriter swRequestWriter = new
            StreamWriter(request.GetRequestStream());
            swRequestWriter.Write(data);
            swRequestWriter.Close();

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format("POST failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response  
                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                }
                return responseValue;
            }
        }

        public static string GetMessage(string endpoint, string key)
        {
            return GetMessage(endpoint, null, key);
        }

        public static string GetMessage(string endPoint, string parameters, string key)
        {

            string finalUrl;
            if (!string.IsNullOrEmpty(parameters))
                finalUrl = string.Format("{0}?{1}", endPoint, parameters);
            else
                finalUrl = endPoint;

            WebRequest request = HttpWebRequest.Create(finalUrl);
            request.Headers.Add("OfficialKey", key);
            request.Method = "GET";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format("GET failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response  
                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                }
                return responseValue;
            }
        }

        public static string DeleteMessage(string endPoint, string parameters, string key)
        {

            string finalUrl;
            if (!string.IsNullOrEmpty(parameters))
                finalUrl = string.Format("{0}?{1}", endPoint, parameters);
            else
                finalUrl = endPoint;

            WebRequest request = HttpWebRequest.Create(finalUrl);
            request.Headers.Add("OfficialKey", key);
            request.Method = "DELETE";

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format("DELETE failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response  
                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                }
                return responseValue;
            }
        }        
    }
}
