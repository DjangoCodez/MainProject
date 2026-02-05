<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DistributionSelectionFixedAssets.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.DistributionSelectionFixedAssets" %>
<fieldset>
    <legend><%=PageBase.GetText(4838, "Inventarieurval")%></legend>
    <table>
        <SOE:FormIntervalEntry
            ID="Inventories"
            TermID="4837" DefaultTerm="Inventarie"
            NoOfIntervals="1"
            DisableHeader="false"
            EnableCheck="false"
            ContentType="2"
            LabelType="1"
            runat="server">
        </SOE:FormIntervalEntry>
        <SOE:FormIntervalEntry
            ID="Categories"
            TermID="4839" DefaultTerm="Kategorier"
            NoOfIntervals="1"
            DisableHeader="false"
            EnableCheck="false"
            ContentType="2"
            LabelType="1"
            runat="server">
        </SOE:FormIntervalEntry>
        <SOE:SelectEntry
            ID="PrognosType"
            TermID="11966" DefaultTerm="Prognosintervall"
            runat="server">
        </SOE:SelectEntry>
    </table>
</fieldset>