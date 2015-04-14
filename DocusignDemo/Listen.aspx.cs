using System;
using Microsoft.VisualBasic;
using System.IO;
using System.Xml;
using System.Text;

public class Listen : System.Web.UI.Page
{

    protected void Page_Load(object sender, System.EventArgs e)
    {
        //  string customName = "";

        byte[] bytes = new byte[Request.InputStream.Length];
        Request.InputStream.Read(bytes, 0, bytes.Length);
        Request.InputStream.Position = 0;
        
        try
        {
            // Using Xml DOM Example
            string input = UTF8Encoding.UTF8.GetString(bytes);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(input);

            // save XML
            XmlNamespaceManager xmlNamespace = new XmlNamespaceManager(doc.NameTable);
            xmlNamespace.AddNamespace("dsx", "http://www.docusign.net/API/3.0");
            XmlNode envelopeNode = doc.SelectSingleNode("//dsx:DocuSignEnvelopeInformation/dsx:EnvelopeStatus/dsx:EnvelopeID", xmlNamespace);
            string envelopeId = envelopeNode.InnerText;
            XmlNode envelopeStatusNode = doc.SelectSingleNode("//dsx:DocuSignEnvelopeInformation/dsx:EnvelopeStatus/dsx:Status", xmlNamespace);
            string envelopeStatus = envelopeStatusNode.InnerText.ToLower();
            //DeclineReason
            string DeclineReason = string.Empty;
            if (envelopeStatus == "declined")
            {
                XmlNode envelopeDeclinedReasonNode = doc.SelectSingleNode("//dsx:DocuSignEnvelopeInformation/dsx:EnvelopeStatus/dsx:RecipientStatuses/dsx:RecipientStatus/dsx:DeclineReason", xmlNamespace);
                DeclineReason = envelopeDeclinedReasonNode.InnerText.ToLower();
            }
            //  If (envelopeStatus = "completed") Then
            DocuSignIntegrator.GetEnvelopeDocs.UpdateDocumentsByListen(envelopeId, envelopeStatus, DeclineReason);
            // End If


        }
        catch (Exception ex)
        {
            // could deserialize
            string errMess = "Exception: " + ex.Message;
            if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
            {
                errMess += ", Inner Exception: " + ex.InnerException.Message;
            }            
        }
    }
}