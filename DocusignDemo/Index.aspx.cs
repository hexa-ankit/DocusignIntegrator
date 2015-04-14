using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Data;
using System.Text;
using System.Web.UI.WebControls;
using DocuSignIntegrator;

namespace WebApplication1
{
    public partial class Index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            binddata();
        }
        public void binddata()
        {
            Manage obj = new Manage();

            DataTable dt = obj.GetDocuments();

            GridDocument.DataSource = dt;
            GridDocument.DataBind();
        }

        protected void New_Click(object sender, EventArgs e)
        {
            SendDocusign NewDocument = new SendDocusign();
            if (NewDocument.SendDocument(Name.Text, Email.Text) == "success")
            {
                Status.Text = "Sent.";
                Name.Text = "";
                Email.Text = "";
            }
            else
                Status.Text = "Failed.";
            binddata();

        }

        protected void Update_Click(object sender, EventArgs e)
        {
            GetEnvelopeDocs.GetAllDocuments();
            binddata();
        }

        protected void GridDocument_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var Status = DataBinder.Eval(e.Row.DataItem, "Status");
                Button btnDownload = e.Row.FindControl("btnDownload") as Button;
                if (Status.ToString().ToLower()== "completed")
                {
                    btnDownload.Visible = true;
                }
                else
                    btnDownload.Visible = false;
            }
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            Manage obj = new Manage();
            byte[] SignedPdf;
            SignedPdf = obj.GetDocumentData(btn.CommandName);
                Response.Clear();

                Response.ClearHeaders();

                Response.ClearContent();
                
                
                Response.AddHeader("Content-Disposition", "attachment; filename=SignedDocument.pdf");
                Response.ContentType = "application/pdf";
                
                Response.AddHeader("Content-Length", SignedPdf.Length.ToString());
                
                Response.BinaryWrite(SignedPdf) ;

                HttpContext.Current.ApplicationInstance.CompleteRequest();
            Response.Flush();
            Response.End();
            
        }
        
    }
}