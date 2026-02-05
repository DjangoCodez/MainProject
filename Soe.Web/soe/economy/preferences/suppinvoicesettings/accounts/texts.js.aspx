<%@ Page Language="C#" AutoEventWireup="true" Inherits="SoftOne.Soe.Web.PageBase" %>
<%@ Import Namespace="SoftOne.Soe.Business.Core" %>
<%@ Import Namespace="SoftOne.Soe.Data" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/javascript";

        AccountManager am = new AccountManager(ParameterObject);
        AccountDim dim = am.GetAccountDimStd(SoeCompany.ActorCompanyId);
        if (dim != null)
            Response.Write("var stdDimID = eval('" + dim.AccountDimId + "');");
        else
            Response.Write("var stdDimID ='';");
        
        Response.Write("eval('accountSearch.init(false)');");
    }
</script>

var companyID = <%=SoeCompany.ActorCompanyId%>;