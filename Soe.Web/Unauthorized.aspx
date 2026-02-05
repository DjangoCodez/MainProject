<%@ Page MasterPageFile="~/base.master" Language="C#" AutoEventWireup="true" CodeBehind="Unauthorized.aspx.cs" Inherits="SoftOne.Soe.Web.unauthorized" %>

<asp:Content ID="baseMasterBodyContent" ContentPlaceHolderID="baseMasterBody" runat="server">
    <div class="error" style="margin: 20px">
        <h1><%= GetText(1155, "Behörighet saknas")%></h1>
        <p class="description">
            <%= unauthorizedMessage%>
            <br />
            <br />
            <a href="<%= loginUrl %>"><%= GetText(11556, "Klicka här för at försöka logga in i SoftOne online igen")%></a>
        </p>
    </div>
</asp:Content>
