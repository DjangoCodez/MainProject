<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DistributionSelectionBilling.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.DistributionSelectionBilling" %>
<fieldset>
    <legend><%=title%></legend>
    <table>
        <SOE:CheckBoxEntry
            ID="ShowNotPrinted" 
            TermID="5206"
            DefaultTerm="Visa ej utskrivna"
            runat="server">
        </SOE:CheckBoxEntry>
        <SOE:CheckBoxEntry
            ID="ShowCopy" 
            TermID="5207"
            DefaultTerm="Visa kopior"
            OnClick = "disableEnableInvoiceCopyAsOriginal()"
            runat="server">
        </SOE:CheckBoxEntry>
        <SOE:CheckBoxEntry
            ID="InvoiceCopyAsOriginal" 
            TermID="7010"
            DefaultTerm="Visa kopior som original"                
            runat="server">
        </SOE:CheckBoxEntry>
        <SOE:CheckBoxEntry
            ID="IncludeClosedOrder" 
            TermID="7292"
            DefaultTerm="Inkludera stängda ordrar"                
            runat="server">
        </SOE:CheckBoxEntry>
        <SOE:FormIntervalEntry
            ID="ProjectNr"
            TermID="8025" DefaultTerm="Projektnr"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="1"
            LabelType="1"
            LabelAutoCompleteType = "3"
            runat="server">
        </SOE:FormIntervalEntry>            
        <SOE:SelectEntry
            ID="CustomerGroup"
            TermID="4595" DefaultTerm="Kundkategori"
            runat="server">
        </SOE:SelectEntry>
        <SOE:FormIntervalEntry
            ID="CustomerNr"
            TermID="1909" DefaultTerm="Kundnr"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="1"
            LabelType="1"
            LabelAutoCompleteType = "1"
            runat="server">
        </SOE:FormIntervalEntry>
        <SOE:FormIntervalEntry
            ID="EmployeeNr"
            TermID="8026" DefaultTerm="Anställningsnr"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="1"
            LabelType="1"
            LabelAutoCompleteType = "4"
            runat="server">
        </SOE:FormIntervalEntry>            
        <SOE:FormIntervalEntry
            ID="InvoiceNr"
            TermID="1910" DefaultTerm="Fakturanr"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="1"
            LabelType="1"
            runat="server">
        </SOE:FormIntervalEntry>
         <SOE:SelectEntry
            ID="StockInventory"
            TermID="9298" DefaultTerm="Inventeringsunderlag"
            runat="server">
        </SOE:SelectEntry>
        <SOE:FormIntervalEntry
            ID="ProductNr"
            TermID="9242" DefaultTerm="Artikelnummer"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="1"
            LabelType="1"
            runat="server">
        </SOE:FormIntervalEntry>
         <SOE:FormIntervalEntry
            ID="StockLocation"
            TermID="4602" DefaultTerm="Lagerplats"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="2"
            LabelType="1"
            runat="server">
        </SOE:FormIntervalEntry>
         <SOE:FormIntervalEntry
            ID="StockShelf"
            TermID="9297" DefaultTerm="Hyllplats"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="2"
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
        <SOE:FormIntervalEntry
            ID="PaymentDate"
            TermID="12049" DefaultTerm="Betaldatum"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="3"
            LabelType="1"
            Visible="false"
            runat="server">
        </SOE:FormIntervalEntry> 
        <SOE:SelectEntry
            ID="SortOrder"
            TermID="1911" DefaultTerm="Sortering"
            runat="server">
        </SOE:SelectEntry>      
        <SOE:FormIntervalEntry
            ID="Period"
            TermID="1137" DefaultTerm="Period"
            NoOfIntervals="1"
            DisableHeader="true"
            EnableCheck="false"
            ContentType="1"
            LabelType="1"
            runat="server">
        </SOE:FormIntervalEntry>
    </table>
</fieldset>