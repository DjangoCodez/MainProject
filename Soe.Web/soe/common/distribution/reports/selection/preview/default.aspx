<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.distribution.reports.selection.preview._default" %>
<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form runat="server">
    <div>
        <CR:CrystalReportViewer ID="viewer" runat="server" AutoDataBind="true" 
            HasCrystalLogo="False" HasDrilldownTabs="False" HasDrillUpButton="False" 
            HasGotoPageButton="False" HasRefreshButton="False" HasSearchButton="False" 
            HasToggleGroupTreeButton="False" HasToggleParameterPanelButton="False" 
            HasZoomFactorList="False" ToolPanelView="None" />
    </div>
    </form>
</body>
</html>
