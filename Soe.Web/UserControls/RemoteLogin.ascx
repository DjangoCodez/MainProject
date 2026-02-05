<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RemoteLogin.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.RemoteLogin" %>
<%if (PageBase.SoeSupportCompany != null && PageBase.SoeSupportUserId != null) { %>
    <div id="RemoteLogin">
        <span class="tagline"><%=PageBase.GetText(5503, "Support")%></span>
        <span><%=PageBase.Enc(PageBase.SoeSupportCompany.ShortName)%></span>
        <span><%=PageBase.Enc(PageBase.SoeSupportUser.LoginName)%></span>
        <a href="/soe/manage/companies/edit/remote/?logout=1"><%=PageBase.GetText(1661, "Logga ut")%></a>
    </div>
<% } %>


