<%@ Page Language="C#" AutoEventWireup="true" Inherits="SoftOne.Soe.Web.PageBase" %>
<%@ Import Namespace="SoftOne.Soe.Business.Core" %>
<%@ Import Namespace="SoftOne.Soe.Data" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/javascript";

        Response.Write("var defaultFreight = '';");
        Response.Write("var defaultInvoiceFee = '';");
        Response.Write("var defaultCentRounding = '';");
        Response.Write("var defaultReminderFee = '';");
        Response.Write("var defaultInterestInvoicing = '';");
        Response.Write("eval('invoiceProductSearch.init(false)');");
    }
</script>

var companyID = <%=SoeCompany.ActorCompanyId%>;