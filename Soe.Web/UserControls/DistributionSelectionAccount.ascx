<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DistributionSelectionAccount.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.DistributionSelectionAccount" %>
<fieldset>
    <legend><%=PageBase.GetText(1319, "Konto- och internkontourval")%></legend>
    <table>
        <SOE:FormIntervalEntry
            ID="Account"
            NoOfIntervals="10"
            DisableHeader="false"
            DisableSettings="true"
            EnableDelete="true"
            ContentType="1"
            LabelType="2"
            runat="server">
        </SOE:FormIntervalEntry>
    </table>
</fieldset>