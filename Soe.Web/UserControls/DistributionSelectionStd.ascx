<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DistributionSelectionStd.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.DistributionSelectionStd" %>
<fieldset>
    <legend><%=PageBase.GetText(1314, "Standardurval")%></legend>
    <div id="DivDateSelection" runat="server">
        <table>
            <SOE:CheckBoxEntry 
                ID="DateSelection" 
                TermID="1322" DefaultTerm="Datumurval" 
                OnClick = "showHideDateSelection()"
                runat="server">
             </SOE:CheckBoxEntry>
        </table>
    </div>
    <div id="DivDate" style="display:none">
        <table>
            <SOE:FormIntervalEntry
                ID="Date"
                TermID="1301" DefaultTerm="Datum"
                NoOfIntervals="1"
                DisableHeader="false"
                EnableCheck="false"
                ContentType="3"
                LabelType="1"
                runat="server">
            </SOE:FormIntervalEntry>
        </table>
    </div>
    <div id="DivYearAndPeriod">
        <table>
            <SOE:FormIntervalEntry
                ID="AccountYear"
                TermID="1303" DefaultTerm="Redovisningsår"
                NoOfIntervals="1"
                DisableHeader="false"
                EnableCheck="false"
                ContentType="2"
                LabelType="1"
                runat="server">
            </SOE:FormIntervalEntry>
            <SOE:FormIntervalEntry
                ID="AccountPeriod"
                TermID="1304" DefaultTerm="Period"
                NoOfIntervals="1"
                DisableHeader="true"
                EnableCheck="false"
                ContentType="2"
                LabelType="1"
                runat="server">
            </SOE:FormIntervalEntry>
        </table>
    </div>
        <div id="DivProjectReport" runat="server">
        <table>
            <SOE:CheckBoxEntry 
                ID="ProjectReport" 
                TermID="4867" DefaultTerm="Projektrapport"                 
                runat="server"
                Visible="false">
             </SOE:CheckBoxEntry>
        </table>
    </div>
    <div>
        <table>
            <SOE:SelectEntry
                ID="AccountDim"
                TermID="7411" 
                DefaultTerm="Konteringsdimension"
                Visible="false"
                runat="server">
            </SOE:SelectEntry>
            <SOE:CheckBoxEntry 
                ID="IncludeMissingAccountDim" 
                TermID="7424" DefaultTerm="Saknar konteringsdimension"
                runat="server"
                Visible="false">
             </SOE:CheckBoxEntry>
             <SOE:CheckBoxEntry 
                ID="SeparateAccountDim" 
                TermID="7425" DefaultTerm="Enskild rapport per internkonto"
                runat="server"
                Visible="false">
             </SOE:CheckBoxEntry>
        </table>
    </div>
    <div id="DivTaxAudit">
        <table>
            <input type="hidden" id="CreateVatVoucherFlag" value="0" runat="server" />
            <SOE:CheckBoxEntry 
                ID="CreateVatVoucher" 
                TermID="5504" DefaultTerm="Skapa momsavräkningsverifikat"
                runat="server"
                Visible="false">
             </SOE:CheckBoxEntry>
        </table>
    </div>
    <div id="DivYearEndAndExternalVoucherSeries">
        <table>
            <SOE:CheckBoxEntry 
                ID="IncludeYearEndVoucherSeries" 
                TermID="4824" DefaultTerm="Inkludera årsskifte verifikat"
                runat="server"
                Visible="false">
             </SOE:CheckBoxEntry>
            <SOE:CheckBoxEntry 
                ID="IncludeExternalVoucherSeries" 
                TermID="4825" DefaultTerm="Inkludera externa verifikat"
                runat="server"
                Visible="false">
             </SOE:CheckBoxEntry>
        </table>
    </div>
    <div id="DivBudget">
        <table>
            <input type="hidden" id="ShowBudget" value="0" runat="server" />
            <SOE:SelectEntry
            ID="Budget"
            TermID="9161" DefaultTerm="Budget/Prognos"
            runat="server">
        </SOE:SelectEntry>
        </table>
    </div>
</fieldset>

