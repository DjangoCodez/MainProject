<%@ Page Language="C#" MasterPageFile="ModalForm.Master" AutoEventWireup="true" CodeBehind="RegFavorite.aspx.cs" Inherits="SoftOne.Soe.Web.modalforms.RegFavorite" %>
<%@ OutputCache Duration="1" Location="Server" VaryByParam="*"%>
<asp:Content ID="Content1" ContentPlaceHolderID="formContent" runat="server">
    <SOE:TextEntry 
        ID="RegFavoriteName" 
        TermID="2001" DefaultTerm="Namn" 
        Width="250"
        DisableSettings="true"
        runat="server">
    </SOE:TextEntry>
    <SOE:CheckBoxEntry
        ID="RemoteFavorite"
        TermID="5775" DefaultTerm="Supportinlogg"
        Value="false"
        Visible="false"
        Width="20"
        Border="0"
        runat="server">
    </SOE:CheckBoxEntry>
    <SOE:CheckBoxEntry
        ID="CompanyFavorite"
        TermID="3043" DefaultTerm="Företagsspecifik"
        Value="true"
        Width="20"
        Border="0"
        runat="server">
    </SOE:CheckBoxEntry>
    <SOE:CheckBoxEntry
        ID="UseAsDefaultPage"
        TermID="4451"
        DefaultTerm="Startsida"
        Value="false"
        Width="20"
        Border="0"
        runat="server">
    </SOE:CheckBoxEntry>
</asp:Content>
