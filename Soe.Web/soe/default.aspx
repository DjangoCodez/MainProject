<%@ Page Language="C#" MasterPageFile="~/soe/start.master" AutoEventWireup="True" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe._default" Title="SoftOne" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">    
    <SOE:AngularHost ID="AngularHost" ModuleName="Common" AppName="Soe.Common.Start" runat="server" />        
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
