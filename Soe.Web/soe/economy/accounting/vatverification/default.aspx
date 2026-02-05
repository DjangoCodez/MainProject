<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.accounting.vatverification._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">        
    <SOE:AngularHost ID="AngularHost" ModuleName="Common, Economy" AppName="Soe.Economy.Accounting.VatVerification" runat="server" />
    <script type="text/javascript">
            if (!soeConfig)
            soeConfig = {};
        soeConfig.isTemplates = <%= Boolean.FalseString.ToLower() %>
        soeConfig.accountYearId = <%= accountYearId %>
        soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
        soeConfig.accountYearLastDate = '<%= accountYearLastDate %>'            
    </script>    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
