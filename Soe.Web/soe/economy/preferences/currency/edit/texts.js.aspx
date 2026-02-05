<%@ Page Language="C#" AutoEventWireup="true" Inherits="SoftOne.Soe.Web.PageBase" %>
<%@ Import Namespace="SoftOne.Soe.Business.Core" %>
<%@ Import Namespace="SoftOne.Soe.Data" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/javascript";
        Response.Write("var actorCompanyId=" + SoeCompany.ActorCompanyId + ";");
    }
</script>