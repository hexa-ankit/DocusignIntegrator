using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Net;
using System.Data;
using System.Xml.Linq;
using DocuSignIntegrator;
using System.Configuration;


namespace DocuSignIntegrator
{
    public class GetEnvelopeDocs
    {

        public static string UpdateDocumentsByListen(string envelopeID, string envelopeStatus, string declineReason)
        {
            try
            {
                DownloadEnvelopeDocs.Main(envelopeID, envelopeStatus, declineReason);
                return "success";
            }
            catch (Exception ex)
            {
                String errMess = "Exception: " + ex.Message;
                if ((ex.InnerException != null) && (!String.IsNullOrEmpty(ex.InnerException.Message)))
                {
                    errMess += ", Inner Exception: " + ex.InnerException.Message;
                }
                return "failed";
            }
           
        }
        public static string GetAllDocuments()
        {
            try
            {
                string XMLEnvelopeStatus = GetEnvelopeStatuses.Main();
                List<Docusignflow> envelopes = requiredDocuments.getEnvelopeIdList(XMLEnvelopeStatus);
                List<StatusData> statusEnvelopes = requiredDocuments.getXMLItemsList(XMLEnvelopeStatus);
                List<StatusData> itemsToDownloadUpdate = requiredDocuments.ItemsChanged(envelopes, statusEnvelopes);
                string result = requiredDocuments.DownloadAndUpdateDocs(itemsToDownloadUpdate);
                return null;
            }
            catch (Exception ex)
            {
                 String errMess = "Exception: " + ex.Message;
                if ((ex.InnerException!=null)&&(!String.IsNullOrEmpty(ex.InnerException.Message)))
                {
                    errMess += ", Inner Exception: " + ex.InnerException.Message;
                }
                return null;                
            }           
        }
        internal static void SaveDocumentsToDB(byte[] SignatureFile, string envelopeId, string status, string declineReason)
        {
            try
            {
                Manage obj = new Manage();
                obj.UpdateDocument(SignatureFile, envelopeId, status, declineReason);
            }
            catch (Exception ex)
            { }     
        }
    }


    public class requiredDocuments
    {
        public static List<Docusignflow> getEnvelopeIdList(string XMLData)
        {
            Manage obj = new Manage();

            DataTable dt = obj.GetUnsignedDocument();
            List<Docusignflow> EnvelopeList = new List<Docusignflow>();
            foreach (DataRow row in dt.Rows)
            {
                Docusignflow DocusignFlowObj = new Docusignflow()
                {
                    EnvelopeId = row["EnvelopeID"].ToString(),
                    Status = row["Status"].ToString(),
                    StatusChangedDT = row["StatusChangedTime"].ToString()

                };
                EnvelopeList.Add(DocusignFlowObj);
            }            
            return EnvelopeList;
        }

        public static List<StatusData> getXMLItemsList(string xmlData)
        {
            XDocument doc = XDocument.Parse(xmlData);
            XNamespace ns = doc.Root.Name.Namespace;
            var elements = doc.Root.Descendants(ns + "envelopes").Elements(ns + "envelope");
            List<StatusData> List = new List<StatusData>();
            foreach (var item in elements)
            {
                if (item.Element(ns + "status").Value != "sent")
                {
                    StatusData status = new StatusData()
                    {
                        envelopeID = item.Element(ns + "envelopeId").Value,
                        Status = item.Element(ns + "status").Value,
                        StatusChangedDT = Convert.ToDateTime(item.Element(ns + "statusChangedDateTime").Value)
                    };
                    List.Add(status);
                }
            }
            return List;
        }
        public static List<StatusData> ItemsChanged(List<Docusignflow> dbList, List<StatusData> statusList)
        {
            List<StatusData> obj = new List<StatusData>();
            foreach (var item in dbList)
            {
                foreach (var statusItem in statusList)
                {
                    if (item.EnvelopeId == statusItem.envelopeID)
                    {
                        StatusData EntityObj = new StatusData()
                        {
                            envelopeID = item.EnvelopeId,
                            Status = statusItem.Status,
                            StatusChangedDT = statusItem.StatusChangedDT
                        };
                        obj.Add(EntityObj);
                    }
                }
            }

            return obj;
        }
        public static string DownloadAndUpdateDocs(List<StatusData> updatedStatus)
        {

            //testEntities container = new testEntities();
            foreach (var item in updatedStatus)
            {
                DownloadEnvelopeDocs.Main(item.envelopeID, item.Status, "");
            }
            return "success";
        }

        public static bool itemExists(string envelopeID) { return true; }

    }
    public class GetEnvelopeStatuses
    {
        // Enter your info here:
        static string email = ConfigurationSettings.AppSettings["DocusignLogin"].ToString();	// your account email
        static string password = ConfigurationSettings.AppSettings["DocusignPassword"].ToString();	// your account password
        static string integratorKey = ConfigurationSettings.AppSettings["DocusignIntegratorKey"].ToString();		// your account Integrator Key (found on Preferences -> API page)
        static string baseURL = "";			// - we will retrieve this

        //***********************************************************************************************
        // main()
        //***********************************************************************************************
        public static string Main()
        {

            try
            {
                //============================================================================
                //  STEP 1 - Login API Call (used to retrieve your baseUrl)
                //============================================================================

                // Endpoint for Login api call (in demo environment):
                string url = "https://demo.docusign.net/restapi/v2/login_information";

                // set request url, method, and headers.  No body needed for login api call
                HttpWebRequest request = initializeRequest(url, "GET", null, email, password);

                // read the http response
                string response = getResponseBody(request);

                // parse baseUrl from response body
                baseURL = parseDataFromResponse(response, "baseUrl");

                //--- display results
                Console.WriteLine("\nAPI Call Result: \n\n" + prettyPrintXml(response));

                //============================================================================
                //  STEP 2 - Get Statuses of a set of envelopes
                //============================================================================

                //*** This example gets statuses of all envelopes in your account going back 1 month...

                int curr_month = System.DateTime.Now.Month;
                int curr_day = System.DateTime.Now.Day;
                int curr_year = System.DateTime.Now.Year;
                if (curr_month != 1)
                {
                    curr_month -= 1;
                }
                else
                { // special case for january
                    curr_month = 12;
                    curr_year -= 1;
                }

                // append "/envelopes?from_date=MONTH/DAY/YEAR" and use in get statuses api call
                // we need to URL encode the slash (/) chars, whos URL encoding is: %2F
                url = baseURL + "/envelopes?from_date=" + curr_month.ToString() + "%2F" + curr_day.ToString() + "%2F" + curr_year.ToString();

                // set request url, method, and headers.  No request body for this api call...
                request = initializeRequest(url, "GET", null, email, password);

                // read the http response
                response = getResponseBody(request);

                //--- display results
                return prettyPrintXml(response);
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        return prettyPrintXml(text);
                    }
                }
            }
        } // end main()

        //***********************************************************************************************
        // --- HELPER FUNCTIONS ---
        //***********************************************************************************************
        public static HttpWebRequest initializeRequest(string url, string method, string body, string email, string password)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            addRequestHeaders(request, email, password);
            if (body != null)
                addRequestBody(request, body);
            return request;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void addRequestHeaders(HttpWebRequest request, string email, string password)
        {
            // authentication header can be in JSON or XML format.  XML used for this walkthrough:
            string authenticateStr =
                "<DocuSignCredentials>" +
                    "<Username>" + email + "</Username>" +
                    "<Password>" + password + "</Password>" +
                    "<IntegratorKey>" + integratorKey + "</IntegratorKey>" + // global (not passed)
                    "</DocuSignCredentials>";
            request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
            request.Accept = "application/xml";
            request.ContentType = "application/xml";
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void addRequestBody(HttpWebRequest request, string requestBody)
        {
            // create byte array out of request body and add to the request object
            byte[] body = System.Text.Encoding.UTF8.GetBytes(requestBody);
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(body, 0, requestBody.Length);
            dataStream.Close();
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string getResponseBody(HttpWebRequest request)
        {
            // read the response stream into a local string
            HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(webResponse.GetResponseStream());
            string responseText = sr.ReadToEnd();
            return responseText;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string parseDataFromResponse(string response, string searchToken)
        {
            // look for "searchToken" in the response body and parse its value
            using (XmlReader reader = XmlReader.Create(new StringReader(response)))
            {
                while (reader.Read())
                {
                    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == searchToken))
                        return reader.ReadString();
                }
            }
            return null;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string prettyPrintXml(string xml)
        {
            // print nicely formatted xml
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                return xml;
            }
        }
    } // end class
    
    public class DownloadEnvelopeDocs
    {
        // Enter your info here:
        static string email = ConfigurationSettings.AppSettings["DocusignLogin"].ToString();	// your account email
        static string password = ConfigurationSettings.AppSettings["DocusignPassword"].ToString();	// your account password
        static string integratorKey = ConfigurationSettings.AppSettings["DocusignIntegratorKey"].ToString();		// your account Integrator Key (found on Preferences -> API page)
        // valid envelope id from an existing envelope in your account
        static string baseURL = "";			// - we will retrieve this

        //***********************************************************************************************
        // main()
        //***********************************************************************************************
        public static void Main(string envelopeId, string status, string declineReason)
        {

            try
            {
                byte[] SignatureFile = null;
                byte[] CertificateFile = null;
                //============================================================================
                //  STEP 1 - Login API Call (used to retrieve your baseUrl)
                //============================================================================

                // Endpoint for Login api call (in demo environment):
                string url = "https://demo.docusign.net/restapi/v2/login_information";

                // set request url, method, and headers.  No body needed for login api call
                HttpWebRequest request = initializeRequest(url, "GET", null, email, password);

                // read the http response
                string response = getResponseBody(request);

                // parse baseUrl from response body
                baseURL = parseDataFromResponse(response, "baseUrl");

                //--- display results
                Console.WriteLine("\nAPI Call Result: \n\n" + prettyPrintXml(response));

                //============================================================================
                //  STEP 2 - Get Envelope Document(s) List and Info
                //============================================================================

                // append "/envelopes/{envelopeId}/documents" to to baseUrl and use for next endpoint
                url = baseURL + "/envelopes/" + envelopeId + "/documents";

                // set request url, method, body, and headers
                request = initializeRequest(url, "GET", null, email, password);

                // read the http response
                response = getResponseBody(request);

                // store each document name and uri locally, so that we can subsequently download each one
                Dictionary<string, string> docsList = new Dictionary<string, string>();
                string uri, name;
                using (XmlReader reader = XmlReader.Create(new StringReader(response)))
                {
                    while (reader.Read())
                    {
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "envelopeDocument"))
                        {
                            XmlReader reader2 = reader.ReadSubtree();
                            uri = ""; name = "";
                            while (reader2.Read())
                            {
                                if ((reader2.NodeType == XmlNodeType.Element) && (reader2.Name == "name"))
                                {
                                    name = reader2.ReadString();
                                }
                                if ((reader2.NodeType == XmlNodeType.Element) && (reader2.Name == "uri"))
                                {
                                    uri = reader2.ReadString();
                                }
                            }// end while
                            docsList.Add(name, uri);
                        }
                    }
                }

                //--- display results
                Console.WriteLine("\nAPI Call Result: \n\n" + prettyPrintXml(response));

                //============================================================================
                //  STEP 3 - Download the Document(s)
                //============================================================================

                foreach (KeyValuePair<string, string> kvp in docsList)
                {
                    if (kvp.Key != "Summary")
                    {
                        // append document uri to baseUrl and use to download each document(s)
                        url = baseURL + kvp.Value;
                        // set request url, method, body, and headers
                        request = initializeRequest(url, "GET", null, email, password);
                        request.Accept = "application/pdf";	// documents are converted to PDF in the DocuSign cloud

                        // read the response and store into a local file:
                        HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                        string path = HttpContext.Current.Server.MapPath("~/Documents/") + envelopeId + ".pdf";
                        using (MemoryStream ms = new MemoryStream())
                        {
                            webResponse.GetResponseStream().CopyTo(ms);
                            SignatureFile = ms.ToArray();

                            //if (ms.Length > int.MaxValue)
                            //{

                            //    throw new NotSupportedException("Cannot write a file larger than 2GB.");
                            //}
                            //   outfile.Write(ms.GetBuffer(), 0, (int)ms.Length);

                        }
                    }
                    if (kvp.Key == "Summary")
                    {
                        // append document uri to baseUrl and use to download each document(s)
                        url = baseURL + kvp.Value;
                        // set request url, method, body, and headers
                        request = initializeRequest(url, "GET", null, email, password);
                        request.Accept = "application/pdf";	// documents are converted to PDF in the DocuSign cloud

                        // read the response and store into a local file:
                        HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                        string path = HttpContext.Current.Server.MapPath("~/EnvelopeCerts/") + envelopeId + "_cert.pdf";
                        using (MemoryStream ms = new MemoryStream())
                        {
                            webResponse.GetResponseStream().CopyTo(ms);

                            CertificateFile = ms.ToArray();                            

                        }
                    }
                }
                GetEnvelopeDocs.SaveDocumentsToDB(SignatureFile, envelopeId, status, declineReason);
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        Console.WriteLine(prettyPrintXml(text));
                    }
                }
            }
        } // end main()

        //***********************************************************************************************
        // --- HELPER FUNCTIONS ---
        //***********************************************************************************************
        public static HttpWebRequest initializeRequest(string url, string method, string body, string email, string password)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            addRequestHeaders(request, email, password);
            if (body != null)
                addRequestBody(request, body);
            return request;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void addRequestHeaders(HttpWebRequest request, string email, string password)
        {
            // authentication header can be in JSON or XML format.  XML used for this walkthrough:
            string authenticateStr =
                "<DocuSignCredentials>" +
                    "<Username>" + email + "</Username>" +
                    "<Password>" + password + "</Password>" +
                    "<IntegratorKey>" + integratorKey + "</IntegratorKey>" + // global (not passed)
                    "</DocuSignCredentials>";
            request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
            request.Accept = "application/xml";
            request.ContentType = "application/xml";
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void addRequestBody(HttpWebRequest request, string requestBody)
        {
            // create byte array out of request body and add to the request object
            byte[] body = System.Text.Encoding.UTF8.GetBytes(requestBody);
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(body, 0, requestBody.Length);
            dataStream.Close();
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string getResponseBody(HttpWebRequest request)
        {
            // read the response stream into a local string
            HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(webResponse.GetResponseStream());
            string responseText = sr.ReadToEnd();
            return responseText;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string parseDataFromResponse(string response, string searchToken)
        {
            // look for "searchToken" in the response body and parse its value
            using (XmlReader reader = XmlReader.Create(new StringReader(response)))
            {
                while (reader.Read())
                {
                    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == searchToken))
                        return reader.ReadString();
                }
            }
            return null;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string prettyPrintXml(string xml)
        {
            // print nicely formatted xml
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                return xml;
            }
        }
    } // end class
    
    public class StatusData
    {
        public string envelopeID { get; set; }
        public string Status { get; set; }
        public DateTime StatusChangedDT { get; set; }
    }
}
