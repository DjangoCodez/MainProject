<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PageNotFound.aspx.cs" Inherits="SoftOne.Soe.Web.errors.PageNotFound" %>
<%@ Register Src="~/UserControls/SupportContactInfo.ascx" TagPrefix="SOE" TagName="SupportContactInfo" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <div class="error">
        <h1><%= GetText(5481, "Sidan du sökte finns inte på Softone.se")%></h1>        
        <p class="description">
            <%= GetText(1150, "Kanske har du skrivit fel? Kontrollera stavningen. Det kan också vara en sida som inte längre existerar.")%>
            <br /><br /><a href="javascript:history.go(-1)"><%= GetText(1549, "Gå tillbaka")%></a>
        </p>    
        <ul id="ArrangementList" runat="server">
            <li>
                <SOE:SupportContactInfo ID="SupportContactInfo" Runat="Server"></SOE:SupportContactInfo>
            </li>
        </ul>
    </div>
</asp:Content>
