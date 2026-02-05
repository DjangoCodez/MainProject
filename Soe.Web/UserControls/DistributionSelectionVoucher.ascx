<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DistributionSelectionVoucher.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.DistributionSelectionVoucher" %>
<fieldset>
    <legend><%=PageBase.GetText(1318, "Verifikaturval")%></legend>
    <table>
        <SOE:FormIntervalEntry
            ID="VoucherSeries"
            TermID="1305" DefaultTerm="Verifikatserie"
            NoOfIntervals="1"
            DisableHeader="false"
            EnableCheck="false"
            ContentType="2"
            LabelType="1"
            runat="server">
        </SOE:FormIntervalEntry>
        <SOE:FormIntervalEntry
            ID="VoucherNr"
            TermID="1302" DefaultTerm="Verifikatnr"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="4"
            LabelType="1"
            runat="server">
        </SOE:FormIntervalEntry>
    </table>
</fieldset>