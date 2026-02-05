
<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.supplier.invoice.status._default" %>
<%@ OutputCache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">
       
        <%if (handleSupplierPayments)
        {%>
            <SOE:AngularHost ID="AngularHost2" ModuleName="Common,Economy" AppName="Soe.Economy.Supplier.Payments" runat="server" />            
            <script type="text/javascript">
                if (!soeConfig)
                    soeConfig = {};
                soeConfig.paymentId = <%= paymentId %>
                soeConfig.accountYearId = <%= accountYearId %>
                soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
            </script>        
        <%}
        else
        {%>
        <SOE:AngularHost ID="AngularHost" ModuleName="Common,Economy" AppName="Soe.Economy.Supplier.Invoices" runat="server" />
            <script type="text/javascript">
                if (!soeConfig)
                    soeConfig = {};
                soeConfig.accountYearId = <%= accountYearId %>
                soeConfig.accountYearIsOpen = <%= accountYearIsOpen.ToString().ToLower() %>
                soeConfig.invoiceId = <%= invoiceId %>
            </script>                
        <%}%>
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>
