using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Ipc.General.Json.Net;

namespace IPC.CommonLibrary
{
    public static class RestHelper
    {
        #region Create Request
        
        public static HttpWebRequest CreateRestRequest<T>(T t, string requestString, HttpMethod method, string sessionToken, bool json)
        {
            HttpWebRequest request = WebRequest.Create(requestString) as HttpWebRequest;
            request.Method = method.ToString();
            request.Credentials = new NetworkCredential(sessionToken, "");

            if (method == HttpMethod.POST || method == HttpMethod.PUT)
            {
                string postData = "";

                if (json)
                {
                    request.ContentType = "application/json";
                    // Attempt to serialize the data in 2 different formats. 
                    try
                    {
                        var typeString = t.GetType().ToString();
                        if (typeString.Contains("Authorize") || typeString.Contains("Adjust") || typeString.Contains("Undo") || typeString.Contains("Capture") || typeString.Contains("Return"))
                            postData = JsonObjectSerializer.SerializeJson(t, true);
                        else
                        {
                            JsonQueryStringConverter converter = new JsonQueryStringConverter();
                            postData = converter.ConvertValueToString(t, typeof (T));
                        }
                    }
                    catch (Exception)
                    {
                        postData = JsonObjectSerializer.SerializeJson(t, true);
                    }

                    if (postData.Contains("\\/Date("))
                    {
                        var dates = new List<string>();
                        GetDateFromObject<T>(t, ref dates);
                        ReplaceDateValues<T>(ref postData, dates, 0);
                    }

                    FormatJsonString(ref postData);
                }
                else
                {  
                    request.ContentType = "application/xml";
                    postData = GetXMLFromContractObject<T>(t);
                    FormatXmlString(ref postData);
                }

                byte[] bytes = new ASCIIEncoding().GetBytes(postData);
                request.ContentLength = bytes.Length;
                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(bytes, 0, bytes.Length);
                    postStream.Close();
                }
            }

            return request;
        }

        private static void FormatJsonString(ref string postData) 
        {
            postData = postData.Replace(", CWS-CSharp", "");  
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Rest.AuthorizeTransaction", "AuthorizeTransaction,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Rest.AuthorizeAndCaptureTransaction", "AuthorizeAndCaptureTransaction,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Rest.Adjust", "Adjust,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Rest.Undo", "Undo,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Rest.CaptureAll", "CaptureAll,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Rest.Capture", "Capture,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Rest.ReturnById", "ReturnById,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Rest.ReturnTransaction", "ReturnTransaction,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Bankcard.BankcardUndo", "BankcardUndo,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Bankcard");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Bankcard.Pro.BankcardTransactionPro", "BankcardTransactionPro,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Bankcard/Pro");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Bankcard.Pro.BankcardTransactionDataPro", "BankcardTransactionDataPro,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Bankcard/Pro");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Bankcard.BankcardTransaction", "BankcardTransaction,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Bankcard");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Bankcard.BankcardTransactionData", "BankcardTransactionData,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Bankcard");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Bankcard.BankcardCapture", "BankcardCapture,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Bankcard");
            postData = postData.Replace("schemas.ipcommerce.com.CWS.v2._0.Transactions.Bankcard.BankcardReturn", "BankcardReturn,http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Bankcard");
        }

        private static void FormatXmlString(ref string postData)
        {
            // Transactions need to include their iType in the first element of the body. 
            postData = postData.Replace("AuthorizeTransaction xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest\"", "AuthorizeTransaction xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest\" i:type=\"AuthorizeTransaction\"");
            postData = postData.Replace("AuthorizeAndCaptureTransaction xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest\"", "AuthorizeAndCaptureTransaction xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest\" i:type=\"AuthorizeAndCaptureTransaction\"");
            postData = postData.Replace("Adjust xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest\"", "Adjust xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest\" i:type=\"Adjust\"");
            postData = postData.Replace("Undo xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest\"", "Undo xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.ipcommerce.com/CWS/v2.0/Transactions/Rest\" i:type=\"Undo\"");
        }

        private static void ReplaceDateValues<T>(ref string postData, List<string> dates, int count)
        {
            var index = postData.IndexOf("\\/Date(", 0);
            var endIndex = postData.IndexOf(")\\/", index) + 3; // 3 characters
            postData = postData.Remove(index, endIndex - index); // Remove this chunk in the string
            postData = postData.Insert(index, dates.ElementAt(count));

            if (postData.Contains("\\/Date(") && count + 1 <= dates.Count)
            {
                ReplaceDateValues<T>(ref postData, dates, ++count);
            }
        }

        private static void GetDateFromObject<T>(T t, ref List<string> dates, Type type = null )
        {
            type = type ?? typeof(T);

            if (t.GetType().IsArray || t is ICollection)
                foreach (var obj in (IEnumerable)t)
                {
                    type = obj.GetType();
                    GetDateFromObject(obj, ref dates, type);
                }
            else
            {
                foreach (var propertyInfo in type.GetProperties())
                {
                    if (propertyInfo.PropertyType == typeof (DateTime))
                    {
                        var propValue = propertyInfo.GetValue(t, null);
                        dates.Add(propValue.ToString());
                    }
                    else if (propertyInfo.PropertyType.IsClass && !propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType != typeof(string) && !propertyInfo.PropertyType.IsArray)
                    {
                        var propValue = propertyInfo.GetValue(t, null);
                        if(propValue != null)
                            GetDateFromObject(propValue, ref dates, propValue.GetType());
                    }
                }
            }
        }

        public static string GetXMLFromContractObject<T>(T t)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            XmlWriter xmlWriter = XmlWriter.Create(sb, settings);

            DataContractSerializer ser = new DataContractSerializer(typeof(T));
            ser.WriteObject(xmlWriter, t);
            xmlWriter.Flush();

            string xml = sb.ToString();
            return xml;
        }

        
        public static HttpWebRequest CreateRequest(string requestString, HttpMethod method, string sessionToken, bool json, XmlDocument d)
        {
            HttpWebRequest request = WebRequest.Create(requestString) as HttpWebRequest;
            request.Method = method.ToString();
            request.Credentials = new NetworkCredential(sessionToken, "");

            string postData = "";

            if (json)
            {
                request.ContentType = "application/json";
                JsonQueryStringConverter converter = new JsonQueryStringConverter();
                postData = d.InnerXml;
            }
            else
            {
                request.ContentType = "application/xml";
                postData = d.InnerXml;
            }
            // MY STUFF
            XDocument temp = new XDocument();
            byte[] bro = new ASCIIEncoding().GetBytes(temp.ToString());
            
            // END MY STUFF

            byte[] bytes = new ASCIIEncoding().GetBytes(postData);
            request.ContentLength = bytes.Length;
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(bytes, 0, bytes.Length);
                postStream.Close();
            }

            return request;
        }

        
        public static HttpWebRequest CreateRequest<T>(T t, string requestString, HttpMethod method, string sessionToken, bool json, XmlDocument d)
        {
            HttpWebRequest request = WebRequest.Create(requestString) as HttpWebRequest;
            request.Method = method.ToString();
            request.Credentials = new NetworkCredential(sessionToken, "");

            string postData = "";

            if (json)
            {
                request.ContentType = "application/json";
                JsonQueryStringConverter converter = new JsonQueryStringConverter();
                postData = converter.ConvertValueToString(t, typeof(T));
            }
            else
            {
                request.ContentType = "application/xml";
                postData = GetXMLFromCWSObject<T>(t);
            }

            byte[] bytes = new ASCIIEncoding().GetBytes(postData);
            request.ContentLength = bytes.Length;
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(bytes, 0, bytes.Length);
                postStream.Close();
            }

            return request;
        }

        public static HttpWebRequest CreateRequest<T>(T t, string requestString, HttpMethod method, string sessionToken, bool json)
        {
            HttpWebRequest request = WebRequest.Create(requestString) as HttpWebRequest;
            request.Method = method.ToString();
            request.Credentials = new NetworkCredential(sessionToken, "");

            string postData = "";

            if (json)
            {
                request.ContentType = "application/json";
                JsonQueryStringConverter converter = new JsonQueryStringConverter();
                postData = converter.ConvertValueToString(t, typeof(T));
            }
            else
            {
                request.ContentType = "application/xml";
                postData = GetXMLFromCWSObject<T>(t);
            }

            byte[] bytes = new ASCIIEncoding().GetBytes(postData);
            request.ContentLength = bytes.Length;
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(bytes, 0, bytes.Length);
                postStream.Close();
            }

            return request;
        }

        public static HttpWebRequest CreateRequest<T>(List<T> tlist, string requestString, HttpMethod method, string sessionToken, bool json)
        {
            HttpWebRequest request = WebRequest.Create(requestString) as HttpWebRequest;
            request.Method = method.ToString();
            request.Credentials = new NetworkCredential(sessionToken, "");

            string postData = "";

            if (json)
            {
                request.ContentType = "application/json";
                JsonQueryStringConverter converter = new JsonQueryStringConverter();
                postData = converter.ConvertValueToString(tlist, typeof(List<T>));
            }
            else
            {
                request.ContentType = "application/xml";
                postData = GetXMLFromCWSObjectList<T>(tlist);
            }

            byte[] bytes = new ASCIIEncoding().GetBytes(postData);
            request.ContentLength = bytes.Length;
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(bytes, 0, bytes.Length);
                postStream.Close();
            }

            return request;
        }

        #endregion

        #region Get Response
        
        public static string GetResponse(HttpWebRequest request, bool json, bool format = true)
        {
            string responseStr = string.Empty;

            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        throw new Exception("Details logged on server!");
                    }

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                    response.Close();
                }
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse r = (HttpWebResponse)we.Response;
                    StreamReader reader = new StreamReader(r.GetResponseStream());
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                    r.Close();

                    throw new Exception(responseStr);
                }
                else if(we.Status == WebExceptionStatus.ServerProtocolViolation)
                {
                    throw new Exception(we.Message + " " + we.Status);
                }
                else
                    Console.WriteLine(we.Message + " " + we.Status);
                throw we;
                
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (!format) { return responseStr; }

            if (json)
            {
                responseStr = responseStr.Replace("\"", "");

                if(responseStr.Contains("id") && responseStr.Contains("appProfile"))
                    return responseStr.Split(new string[]{":", ","}, StringSplitOptions.RemoveEmptyEntries)[1];
                

                return responseStr;
            }
            else if (!string.IsNullOrEmpty(responseStr))
            {
                XDocument response = XDocument.Parse(responseStr); 

                if(response.Root.Elements().Count() == 0)
                    return response.Document.Root.Value;

                if(response.ToString().Contains("ApplicationProfileId"))
                    return response.Descendants(XName.Get("id", "http://schemas.ipcommerce.com/CWS/v2.0/ServiceInformation/Rest")).FirstOrDefault().Value;

                return "";
            }
            return null;
        }

        //public static string GetResponse(HttpWebRequest request, bool json)
        //{
        //    return GetResponse(request, json, false);

        //}
        
        public static T GetResponse<T>(HttpWebRequest request, bool json)
        {
            string responseStr = GetResponse(request, json, false);

            if (json)
            {
                return JsonObjectSerializer.DeserializeJson<T>(responseStr);
                //JsonQueryStringConverter converter = new JsonQueryStringConverter();
                //return (T)converter.ConvertStringToValue(responseStr, typeof(T));
            }
            else
            {
                return GetCWSObjectFromXml<T>(responseStr);
            }
        }
        
        public static List<T> GetResponseList<T>(HttpWebRequest request, bool json)
        {
            string responseStr = GetResponse(request, json, false);

            if (json) 
            {
                //JsonQueryStringConverter converter = new JsonQueryStringConverter();
                //var list = new List<T>();
                //var list = (List<T>) converter.ConvertStringToValue(responseStr, typeof (List<T>));
                var list = JsonObjectSerializer.DeserializeJson<List<T>>(responseStr);
                return list;
            }
            else
            {
                return GetCWSObjectListFromXml<T>(responseStr);
            }
        }

        public static List<T> GetCWSObjectListFromJson<T>(string json)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            return ser.Deserialize<List<T>>(json);
        }

        public static T GetCWSObjectFromJson<T>(string json)
        {
            return (T)JsonObjectSerializer.DeserializeJson<T>(json);
        }

        public static string GetXMLFromCWSObject<T>(T t)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            XmlWriter xmlWriter = XmlWriter.Create(sb, settings);

            DataContractSerializer ser = new DataContractSerializer(typeof(T));
            ser.WriteObject(xmlWriter, t);
            xmlWriter.Flush();

            string xml = sb.ToString();
            return xml;
        }
        public static string GetXMLFromCWSObjectList<T>(List<T> tlist)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            XmlWriter xmlWriter = XmlWriter.Create(sb, settings);

            DataContractSerializer ser = new DataContractSerializer(typeof(List<T>));
            ser.WriteObject(xmlWriter, tlist);
            xmlWriter.Flush();

            string xml = sb.ToString();
            return xml;
        }
        public static T GetCWSObjectFromXml<T>(string xml)
        {
            XmlReader read = XmlReader.Create(new StringReader(xml));
            DataContractSerializer ser = new DataContractSerializer(typeof(T));

            return (T)ser.ReadObject(read);
        }
        
        public static List<T> GetCWSObjectListFromXml<T>(string xml)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            DataContractSerializer ser = new DataContractSerializer(typeof(List<T>));
            return (List<T>) ser.ReadObject(ms);
        }

        #endregion
    }
}
