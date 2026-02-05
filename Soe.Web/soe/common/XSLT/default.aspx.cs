using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Text;
using System.IO;
using System.Xml.XPath;
using System.Xml.Xsl;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System.Xml.Linq;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Util.Config;

namespace SoftOne.Soe.Web.soe.common.XSLT
{
    public partial class _default : System.Web.UI.Page
    {
        #region Variables
        
        private string xslPath;
        private string pathOnServer;
        private string xmlContent;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["templatetype"] != null && Request.QueryString["id"] != null && Request.QueryString["c"] != null)
            {
                int templateTypeId = Convert.ToInt32(Request.QueryString["templatetype"]);
                int id = Convert.ToInt32(Request.QueryString["id"]);
                int companyId = Convert.ToInt32(Request.QueryString["c"]);

                switch (templateTypeId)
                {
                    case (int)SoeReportTemplateType.FinvoiceEdiSupplierInvoice:
                        #region FinvoiceEdiSupplierInvoice

                        var em = new EdiManager(null);
                        EdiEntry edientry = em.GetEdiEntry(id, companyId, ignoreState: true);

                        xslPath = HttpContext.Current.Server.MapPath("~") + "reports/Finvoice.xsl";
                        xmlContent = edientry.XML;                        
                        pathOnServer = ConfigSettings.SOE_SERVER_DIR_TEMP_FINVOICE_REPORT_PHYSICAL + edientry.FileName;   
                        
                        XDocument xdoc = XDocument.Parse(xmlContent);
                        //xdoc.Declaration.Encoding = "ISO-8859-1";
                        xdoc.Save(pathOnServer);
                        
                        #endregion
                        break;
                    default:
                        break;
                }

                XmlTextReader xmlReader = null;
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                               
                try
                {                    
                    xmlReader = new XmlTextReader(pathOnServer);
                    XPathDocument doc = new XPathDocument(xmlReader);

                    //TODO: XslTransform is obsolete, use XslCompiledTransform if possible
                    XslCompiledTransform transform = new XslCompiledTransform();
                    transform.Load(xslPath);
                    transform.Transform(doc, null, sw);

                    this.form1.InnerHtml = sb.ToString();
                }
                catch (Exception excp)
                {
                    Response.Write(excp.ToString());
                }
                finally
                {
                    if(xmlReader != null)
                        xmlReader.Close();
                    if(sw != null)
                        sw.Close();
                    
                    RemoveFileFromServer(pathOnServer);
                }
            }
        }
        private void RemoveFileFromServer(string pathOnServer)
        {
            if (System.IO.File.Exists(pathOnServer))
                System.IO.File.Delete(pathOnServer);
        }
    }
}