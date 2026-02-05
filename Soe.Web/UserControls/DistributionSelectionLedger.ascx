<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DistributionSelectionLedger.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.DistributionSelectionLedger" %>
<fieldset>
    <legend><%=SelectionTitle%></legend>
    <table>
        <SOE:SelectEntry
            ID="InvoiceSelection"
            TermID="1809" DefaultTerm="Fakturor"
            runat="server">
        </SOE:SelectEntry>
        <SOE:FormIntervalEntry
            ID="SupplierNr"
            TermID="1810" DefaultTerm="Leverantörsnr"
            NoOfIntervals="1"
            DisableHeader="false"
            EnableCheck="false"
            ContentType="1"
            LabelType="1"
            LabelAutoCompleteType = "2"
            runat="server">
        </SOE:FormIntervalEntry>
        <SOE:FormIntervalEntry
            ID="CustomerNr"
            TermID="1822" DefaultTerm="Kundnr"
            NoOfIntervals="1"
            DisableHeader="false"
            EnableCheck="false"
            ContentType="1"
            LabelType="1"
            LabelAutoCompleteType = "1"
            runat="server">
        </SOE:FormIntervalEntry>
        <SOE:FormIntervalEntry
            ID="InvoiceSeqNr"
            TermID="1811" DefaultTerm="Fakturalöpnr"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="1"
            LabelType="1"
            runat="server">
        </SOE:FormIntervalEntry>
        <SOE:FormIntervalEntry
            ID="Date"
            TermID="1812" DefaultTerm="Datum"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="3"
            LabelType="1"
            runat="server">
        </SOE:FormIntervalEntry>
        <SOE:SelectEntry
            ID="DateRegard"
            TermID="1813" DefaultTerm="Datum avseende på"
            runat="server">
        </SOE:SelectEntry>
        <SOE:SelectEntry
            ID="SortOrder"
            TermID="1814" DefaultTerm="Sortering"
            runat="server">
        </SOE:SelectEntry>
        <SOE:CheckBoxEntry 
            ID="ShowPreliminaryInvoices" 
            TermID="4820" DefaultTerm="Visa preliminära fakturor" 
            runat="server">
        </SOE:CheckBoxEntry>
        <SOE:CheckBoxEntry 
            ID="IncludeCashSalesInvoices" 
            TermID="4897" DefaultTerm="Inkludera kontantfakturor" 
            runat="server">
        </SOE:CheckBoxEntry>
        <SOE:CheckBoxEntry 
            ID="ShowVoucher" 
            TermID="1815" DefaultTerm="Redovisa bokföring" 
            runat="server">
        </SOE:CheckBoxEntry>
        <SOE:CheckBoxEntry
           ID="ShowPendingPaymentsInReport" 
           TermID="7188"
           DefaultTerm='Inkludera betalningar under avprickning'
           runat="server">
        </SOE:CheckBoxEntry>
    </table>
</fieldset>