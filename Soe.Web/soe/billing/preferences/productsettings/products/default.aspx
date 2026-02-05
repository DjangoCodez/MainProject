<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs"
    Inherits="SoftOne.Soe.Web.soe.billing.preferences.productsettings.products._default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">

    <script type="text/javascript" language="javascript">
        var defaultFreight = '';
        var defaultInvoiceFee = '';
        var defaultCentRounding = '';
        var defaultReminderFee = '';
        var defaultInterestInvoicing = '';
    </script>

    <SOE:Form ID="Form1" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="3266" DefaultTerm="Basartiklar" runat="server">
                <div id="Invoice" runat="server">
                    <fieldset>
                        <legend><%=GetText(3276, "Faktura")%></legend>
                        <div>
                            <SOE:Instruction 
                                ID="Instruction1"
                                TermID="3269" DefaultTerm="Konton administreras på respektive artikel"
                                FitInTable="true"
                                runat="server">
                            </SOE:Instruction>
                            <br />
                            <br />
                            <SOE:InstructionList 
                                ID="InvoiceExplanation" 
                                runat="server">
                            </SOE:InstructionList>
                            <table>
                                <tr>
                                    <td>
                                        &nbsp;
                                    </td>
                                    <td>
                                        &nbsp;
                                        <SOE:Text
			                                ID="InvoiceProductNumberLabel"
                                            TermID="3270"
                                            DefaultTerm="Artikel"
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="FreightLabel"
                                            TermID="3162"
                                            DefaultTerm="Frakt"
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="Freight" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('Freight');"
                                            OnKeyUp = "invoiceProductSearch.keydown('Freight');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="InvoiceFeeLabel"
                                            TermID="3163"
                                            DefaultTerm="Fakt. avgift"
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="InvoiceFee" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('InvoiceFee');"
                                            OnKeyUp = "invoiceProductSearch.keydown('InvoiceFee');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="CentRoundingLabel"
                                            TermID="3192" 
                                            DefaultTerm="Öresutjämning" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="CentRounding" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('CentRounding');"
                                            OnKeyUp = "invoiceProductSearch.keydown('CentRounding');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="HouseholdTaxDeductionLabel"
                                            TermID="3287" 
                                            DefaultTerm="ROT-avdrag" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="HouseholdTaxDeduction" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('HouseholdTaxDeduction');"
                                            OnKeyUp = "invoiceProductSearch.keydown('HouseholdTaxDeduction');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="HouseholdTaxDeductionDeniedLabel"
                                            TermID="3551" 
                                            DefaultTerm="Fakturering av avslaget ROT-avdrag" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="HouseholdTaxDeductionDenied" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('HouseholdTaxDeductionDenied');"
                                            OnKeyUp = "invoiceProductSearch.keydown('HouseholdTaxDeductionDenied');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
                                            ID="Household50TaxDeductionLabel"
                                            TermID="7884" 
                                            DefaultTerm="ROT-avdrag 50%" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="Household50TaxDeduction" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('Household50TaxDeduction');"
                                            OnKeyUp = "invoiceProductSearch.keydown('Household50TaxDeduction');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
                                            ID="Household50TaxDeductionDeniedLabel"
                                            TermID="7885" 
                                            DefaultTerm="Fakturering av avslaget ROT-avdrag 50%" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="Household50TaxDeductionDenied" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('Household50TaxDeductionDenied');"
                                            OnKeyUp = "invoiceProductSearch.keydown('Household50TaxDeductionDenied');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="RUTTaxDeductionLabel"
                                            TermID="7251" 
                                            DefaultTerm="RUT-avdrag" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="RUTTaxDeduction" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('RUTTaxDeduction');"
                                            OnKeyUp = "invoiceProductSearch.keydown('RUTTaxDeduction');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="RUTTaxDeductionDeniedLabel"
                                            TermID="7252" 
                                            DefaultTerm="Fakturering av avslaget RUT-avdrag" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="RUTTaxDeductionDenied" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('RUTTaxDeductionDenied');"
                                            OnKeyUp = "invoiceProductSearch.keydown('RUTTaxDeductionDenied');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="Green15TaxDeductionLabel"
                                            TermID="7495" 
                                            DefaultTerm="Grön teknik-avdrag 15 %" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="Green15TaxDeduction" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('Green15TaxDeduction');"
                                            OnKeyUp = "invoiceProductSearch.keydown('Green15TaxDeduction');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="Green15TaxDeductionDeniedLabel"
                                            TermID="7496" 
                                            DefaultTerm="Fakturering av avslaget Grön teknik-avdrag 15 %" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="Green15TaxDeductionDenied" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('Green15TaxDeductionDenied');"
                                            OnKeyUp = "invoiceProductSearch.keydown('Green15TaxDeductionDenied');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="Green20TaxDeductionLabel"
                                            TermID="7655" 
                                            DefaultTerm="Grön teknik-avdrag 20 %" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="Green20TaxDeduction" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('Green20TaxDeduction');"
                                            OnKeyUp = "invoiceProductSearch.keydown('Green20TaxDeduction');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="Green20TaxDeductionDeniedLabel"
                                            TermID="7656" 
                                            DefaultTerm="Fakturering av avslaget Grön teknik-avdrag 20 %" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="Green20TaxDeductionDenied" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('Green20TaxDeductionDenied');"
                                            OnKeyUp = "invoiceProductSearch.keydown('Green20TaxDeductionDenied');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="Green50TaxDeductionLabel"
                                            TermID="7497" 
                                            DefaultTerm="Grön teknik-avdrag 50 %" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="Green50TaxDeduction" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('Green50TaxDeduction');"
                                            OnKeyUp = "invoiceProductSearch.keydown('Green50TaxDeduction');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="Green50TaxDeductionDeniedLabel"
                                            TermID="7498" 
                                            DefaultTerm="Fakturering av avslaget Grön teknik-avdrag 50 %" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="Green50TaxDeductionDenied" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('Green50TaxDeductionDenied');"
                                            OnKeyUp = "invoiceProductSearch.keydown('Green50TaxDeductionDenied');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="FlatPriceLabel"
                                            TermID="7012" 
                                            DefaultTerm="Artikel för fast pris" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="FlatPrice" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('FlatPrice');"
                                            OnKeyUp = "invoiceProductSearch.keydown('FlatPrice');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="MiscProductLabel"
                                            TermID="3552" 
                                            DefaultTerm="Ströartikel" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="MiscProduct" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('MiscProduct');"
                                            OnKeyUp = "invoiceProductSearch.keydown('MiscProduct');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>             
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="GuaranteeProductLabel"
                                            TermID="9154" 
                                            DefaultTerm="Garantibeloppsartikel" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="GuaranteeProduct" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('GuaranteeProduct');"
                                            OnKeyUp = "invoiceProductSearch.keydown('GuaranteeProduct');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <SOE:Text
			                                ID="FlatPriceKeepPricesLabel"
                                            TermID="7223" 
                                            DefaultTerm="Artikel för fastpris, spara priser" 
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Text>
                                    </td>
                                    <td>
                                        <SOE:TextEntry 
                                            ID="FlatPriceKeepPrices" 
                                            HideLabel="true"
                                            Width="100"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            OnChange = "invoiceProductSearch.searchField('FlatPriceKeepPrices');"
                                            OnKeyUp = "invoiceProductSearch.keydown('FlatPriceKeepPrices');"
                                            runat="server">
                                        </SOE:TextEntry>
                                    </td>
                                    <td colspan="2">
                                        <SOE:Instruction 
                                            ID="FlatPriceKeepPricesInstruction"
                                            TermID="7224" DefaultTerm="Artikel för fastpris men priser på övriga rader ligger kvar utan att räknas i totalen"
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Instruction>
                                    </td>
                                </tr>                                       
                            </table>
                        </div>
                    </fieldset>
                </div>
                <div id="Reminder" runat="server">
                    <fieldset>
                        <legend><%=GetText(3277, "Krav och ränta")%></legend>
                        <table>
                            <SOE:Instruction
                                TermID="3269" DefaultTerm="Konton administreras på respektive artikel"
                                FitInTable="true"
                                runat="server">
                            </SOE:Instruction>
                            <tr>
                                <td>
                                    &nbsp;
                                </td>
                                <td>
                                    &nbsp;
                                    <SOE:Text
		                                ID="ReminderProductNumberLabel"
                                        TermID="3270"
                                        DefaultTerm="Artikel"
                                        FitInTable="true"
                                        runat="server">
                                    </SOE:Text>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <SOE:Text
		                                ID="ReminderFeeLabel"
                                        TermID="3274" 
                                        DefaultTerm="Kravavgift" 
                                        FitInTable="true"
                                        runat="server">
                                    </SOE:Text>
                                </td>
                                <td>
                                    <SOE:TextEntry 
                                        ID="ReminderFee" 
                                        HideLabel="true"
                                        Width="100"
                                        FitInTable="true"
                                        DisableSettings="true"
                                        OnChange = "invoiceProductSearch.searchField('ReminderFee');"
                                        OnKeyUp = "invoiceProductSearch.keydown('ReminderFee');"
                                        runat="server">
                                    </SOE:TextEntry>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <SOE:Text
		                                ID="InterestInvoicingLabel"
                                        TermID="3275" 
                                        DefaultTerm="Räntefakturering" 
                                        FitInTable="true"
                                        runat="server">
                                    </SOE:Text>
                                </td>
                                <td>
                                    <SOE:TextEntry 
                                        ID="InterestInvoicing" 
                                        HideLabel="true"
                                        Width="100"
                                        FitInTable="true"
                                        DisableSettings="true"
                                        OnChange = "invoiceProductSearch.searchField('InterestInvoicing');"
                                        OnKeyUp = "invoiceProductSearch.keydown('InterestInvoicing');"
                                        runat="server">
                                    </SOE:TextEntry>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
			    </div>                       
            </SOE:Tab>
        </tabs>
    </SOE:Form>
    <div class="searchTemplate">
        <div id="searchContainer" class="searchContainer">
        </div>
        <div id="invoiceProductSearchItem_$number$">
            <div id="product_$id$" class="item" onmouseover="searchComponent.select();" onclick="searchComponent.choose();">
                <div class="id" id="extendNumWidth_$id$">
                    $number$</div>
                <div class="name" id="extendNameWidth_$id$">
                    $name$</div>
            </div>
            <div class="C" />
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
