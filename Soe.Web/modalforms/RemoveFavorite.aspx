<%@ Page Language="C#" MasterPageFile="~/modalforms/ModalForm.Master" AutoEventWireup="true" CodeBehind="RemoveFavorite.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.RemoveFavorite" Title="Untitled Page" %>
<%@ OutputCache Duration="1" Location="Server" VaryByParam="*"%>
<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <SOE:SelectEntry
        ID="Favorites"
        TermID="2002" DefaultTerm="Favoriter"
        DisableSettings="true"
        Width="200" 
        runat="server">
    </SOE:SelectEntry>
</asp:Content>
