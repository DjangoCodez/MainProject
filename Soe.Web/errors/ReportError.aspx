<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportError.aspx.cs" Inherits="SoftOne.Soe.Web.errors.ReportError" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <div class="error">
        <h1><%= HeaderMessage%></h1>        
        <p class="description">
            <%= DetailMessage%>
            <br /><br /><a href="javascript:history.go(-1)"><%= GetText(1549, "Gå tillbaka")%></a>
        </p>    
    </div>
</asp:Content>
