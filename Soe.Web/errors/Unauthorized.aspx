<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Unauthorized.aspx.cs" Inherits="SoftOne.Soe.Web.errors.Unauthorized" %>
<%@ Register Src="~/UserControls/SupportContactInfo.ascx" TagPrefix="SOE" TagName="SupportContactInfo" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <div class="error">
        <h1><%= GetText(1155, "Behörighet saknas")%></h1>
        <p class="description">
            <%--<%= GetText(1156, "Du saknar behörighet för att visa den här sidan")%> <br />--%>
            <%= UnauthorizedMessage%> <br /><br />
            <a href="javascript:history.go(-1)"><%= GetText(1549, "Gå tillbaka")%></a><%= Enc(" ") %><%= GetText(5475, "och försök igen. Om felet kvarstår kan du") %>
        </p>    
        <ul id="ArrangementList" runat="server">
            <li>
                <span><%=GetText(5480, "Kontakta en administratör") %></span>
            </li>
            <li>
                <SOE:SupportContactInfo ID="SupportContactInfo" Runat="Server"></SOE:SupportContactInfo>
            </li>
        </ul>
    </div>
</asp:Content>

