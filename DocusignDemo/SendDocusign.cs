using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;

namespace DocuSignIntegrator
{
    public class SendDocusign
    {
        Manage Man = new Manage();

        public string SendDocument(string name, string email)
        {
            try
            {
                string Result = RequestSignatureOnDocument.Main(name,email);
                string EnvelopeID = extractEnvelopeId(Result);
                Man.InsertDocument(EnvelopeID);
                    return "success";
            }
            catch (Exception ex)
            {
                return "failed";
            }
           
        }
        public static string extractEnvelopeId(string XmlData)
        {
            string envelopeId = string.Empty;
            XDocument xdoc = new XDocument();
            xdoc = XDocument.Parse(XmlData);

            var query = xdoc.Descendants()
                .Where(a => a.Name.LocalName == "envelopeId")
                .FirstOrDefault().Value;
            envelopeId = query;
            return envelopeId;

        }
    }
    public class RequestSignatureOnDocument
	{
		public static string Main (string Name, string Email)
		{
			//---------------------------------------------------------------------------------------------------
			// ENTER VALUES FOR THE FOLLOWING 6 VARIABLES:
			//---------------------------------------------------------------------------------------------------
			string username = ConfigurationSettings.AppSettings["DocusignLogin"].ToString();;			// your account email
			string password = ConfigurationSettings.AppSettings["DocusignPassword"].ToString();;			// your account password
			string integratorKey = ConfigurationSettings.AppSettings["DocusignIntegratorKey"].ToString();			// your account Integrator Key (found on Preferences -> API page)
            string recipientName = Name;			// recipient (signer) name
            string recipientEmail = Email;			// recipient (signer) email
			string documentName = AppDomain.CurrentDomain.BaseDirectory + "CONTRACT.pdf";			// copy document with same name and extension into project directory (i.e. "test.pdf")
			string contentType = "application/pdf";		// default content type is PDF
			//---------------------------------------------------------------------------------------------------
 
			// additional variable declarations
			string baseURL = "";			// - we will retrieve this through the Login API call
 
			try {
				//============================================================================
				//  STEP 1 - Login API Call (used to retrieve your baseUrl)
				//============================================================================
				
				// Endpoint for Login api call (in demo environment):
				string url = "https://demo.docusign.net/restapi/v2/login_information";
				
				// set request url, method, and headers.  No body needed for login api call
				HttpWebRequest request = initializeRequest( url, "GET", null, username, password, integratorKey);
				
				// read the http response
				string response = getResponseBody(request);
				
				// parse baseUrl from response body
				baseURL = parseDataFromResponse(response, "baseUrl");
				
				//--- display results
				Console.WriteLine("\nAPI Call Result: \n\n" + prettyPrintXml(response));
				
				//============================================================================
				//  STEP 2 - Send Signature Request from Template
				//============================================================================
 
				/*
					This is the only DocuSign API call that requires a "multipart/form-data" content type.  We will be 
					constructing a request body in the following format (each newline is a CRLF):

					--AAA
					Content-Type: application/xml
					Content-Disposition: form-data

					<XML BODY GOES HERE>
					--AAA
					Content-Type:application/pdf
					Content-Disposition: file; filename="document.pdf"; documentid=1 

					<DOCUMENT BYTES GO HERE>
					--AAA--
				 */
 
				// append "/envelopes" to baseURL and use for signature request api call
				url = baseURL + "/envelopes";
				
				// construct an outgoing XML formatted request body (JSON also accepted)
				// .. following body adds one signer and places a signature tab 100 pixels to the right
				// and 100 pixels down from the top left corner of the document you supply
				string xmlBody = 
					"<envelopeDefinition xmlns=\"http://www.docusign.com/restapi\">" + 
					"<emailSubject>DocuSign API - Signature Request on Document</emailSubject>" + 
					"<status>sent</status>" + 	// "sent" to send immediately, "created" to save as draft in your account
						// add document(s)
					"<documents>" +
					"<document>" + 
					"<documentId>1</documentId>" + 
					"<name>" + documentName + "</name>" + 
					"</document>" + 
					"</documents>" +
						// add recipient(s)
					"<recipients>" + 
					"<signers>" + 
					"<signer>" + 
					"<recipientId>1</recipientId>" + 
					"<email>" + recipientEmail + "</email>" +	
					"<name>" + recipientName + "</name>" + 		
					"<tabs>" + 
					"<signHereTabs>" + 
					"<signHere>" + 
					"<xPosition>100</xPosition>" + // default unit is pixels
					"<yPosition>100</yPosition>" + // default unit is pixels
					"<documentId>1</documentId>" + 
					"<pageNumber>1</pageNumber>" + 
					"</signHere>" +
					"</signHereTabs>" + 
					"</tabs>" + 
					"</signer>" + 
					"</signers>" + 
					"</recipients>" +  
					"</envelopeDefinition>";
				
				// set request url, method, headers.  Don't set the body yet, we'll set that separelty after
				// we read the document bytes and configure the rest of the multipart/form-data request
				request = initializeRequest( url, "POST", null, username, password, integratorKey);
 
				// some extra config for this api call
				configureMultiPartFormDataRequest(request, xmlBody, documentName, contentType);
 
				// read the http response
				response = getResponseBody(request);
				
				//--- display results
				return prettyPrintXml(response);
			}
			catch (WebException e) {
				using (WebResponse response = e.Response) {
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
		public static HttpWebRequest initializeRequest(string url, string method, string body, string email, string password, string intKey)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (url);
			request.Method = method;
			addRequestHeaders( request, email, password, intKey );
			if( body != null )
				addRequestBody(request, body);
			return request;
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static void addRequestHeaders(HttpWebRequest request, string email, string password, string intKey)
		{
			// authentication header can be in JSON or XML format.  XML used for this walkthrough:
			string authenticateStr = 
				"<DocuSignCredentials>" + 
					"<Username>" + email + "</Username>" +
					"<Password>" + password + "</Password>" + 
					"<IntegratorKey>" + intKey + "</IntegratorKey>" + 
					"</DocuSignCredentials>";
			request.Headers.Add ("X-DocuSign-Authentication", authenticateStr);
			request.Accept = "application/xml";
			request.ContentType = "application/xml";
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static void addRequestBody(HttpWebRequest request, string requestBody)
		{
			// create byte array out of request body and add to the request object
			byte[] body = System.Text.Encoding.UTF8.GetBytes (requestBody);
			Stream dataStream = request.GetRequestStream ();
			dataStream.Write (body, 0, requestBody.Length);
			dataStream.Close ();
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static void configureMultiPartFormDataRequest(HttpWebRequest request, string xmlBody, string docName, string contentType)
		{
			// overwrite the default content-type header and set a boundary marker
			request.ContentType = "multipart/form-data; boundary=BOUNDARY";
			
			// start building the multipart request body
			string requestBodyStart = "\r\n\r\n--BOUNDARY\r\n" +
				"Content-Type: application/xml\r\n" +
					"Content-Disposition: form-data\r\n" +
					"\r\n" +
					xmlBody + "\r\n\r\n--BOUNDARY\r\n" + 	// our xml formatted envelopeDefinition
					"Content-Type: " + contentType + "\r\n" +
					"Content-Disposition: file; filename=\"" + docName + "\"; documentId=1\r\n" +
					"\r\n";
			string requestBodyEnd = "\r\n--BOUNDARY--\r\n\r\n";
			
			// read contents of provided document into the request stream
			FileStream fileStream = File.OpenRead(docName);
			
			// write the body of the request
			byte[] bodyStart = System.Text.Encoding.UTF8.GetBytes(requestBodyStart.ToString());
			byte[] bodyEnd = System.Text.Encoding.UTF8.GetBytes(requestBodyEnd.ToString());
			Stream dataStream = request.GetRequestStream();
			dataStream.Write(bodyStart, 0, requestBodyStart.ToString().Length);
			
			// Read the file contents and write them to the request stream.  We read in blocks of 4096 bytes
			byte[] buf = new byte[4096];
			int len;
			while ((len = fileStream.Read(buf, 0, 4096) ) > 0) {
				dataStream.Write(buf, 0, len);
			}
			dataStream.Write(bodyEnd, 0, requestBodyEnd.ToString().Length);
			dataStream.Close();
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static string getResponseBody(HttpWebRequest request)
		{
			// read the response stream into a local string
			HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse ();
			StreamReader sr = new StreamReader(webResponse.GetResponseStream());
			string responseText = sr.ReadToEnd();
			return responseText;
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static string parseDataFromResponse(string response, string searchToken)
		{
			// look for "searchToken" in the response body and parse its value
			using (XmlReader reader = XmlReader.Create(new StringReader(response))) {
				while (reader.Read()) {
					if((reader.NodeType == XmlNodeType.Element) && (reader.Name == searchToken))
						return reader.ReadString();
				}
			}
			return null;
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static string prettyPrintXml(string xml)
		{
			// print nicely formatted xml
			try {
				XDocument doc = XDocument.Parse(xml);
				return doc.ToString();
			}
			catch (Exception) {
				return xml;
			}
		}
	} // end class
}
