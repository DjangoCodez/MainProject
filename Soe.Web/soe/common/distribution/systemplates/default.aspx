<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.distribution.systemplates._default" %>
<%@ outputcache Duration="1" Location="None" VaryByParam="*" %>
<%@ Register Src="~/UserControls/AngularHost.ascx" TagPrefix="SOE" TagName="AngularHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">	
    <SOE:AngularHost ID="AngularHost" ModuleName="Common" AppName="Soe.Common.Reports.ReportTemplates" runat="server" />
    <script type="text/javascript">
        if (!soeConfig)
            soeConfig = {};

        soeConfig.module = <%= (int)TargetSoeModule %>
        soeConfig.feature = <%= (int)FeatureEdit %>
        soeConfig.isSys = true
    </script>    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
