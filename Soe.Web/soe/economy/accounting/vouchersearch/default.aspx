<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.accounting.vouchersearch._default" %>
<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">
    <%if (UseAngularSpa) 
    {%>
        <SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
    <% } 
    else
    {%>
        <SOE:AngularHost ID="AngularHost" ModuleName="Common, Economy" AppName="Soe.Economy.Accounting.VoucherSearch" runat="server" />
     <% } %>
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};
        soeConfig.isTemplates = <%= Boolean.FalseString.ToLower() %>
        soeConfig.accountYearId = <%= accountYearId %>
        soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
        soeConfig.accountYearLastDate = '<%= accountYearLastDate %>'    
    </script>   
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>
