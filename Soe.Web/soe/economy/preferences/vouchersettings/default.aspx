<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.vouchersettings._default" %>

<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="5420" DefaultTerm="Inställningar redovisning" runat="server">
                <div>
					<fieldset> 
						<legend><%=GetText(1342, "Verifikatregistrering")%></legend>
						<table>
                            <SOE:CheckBoxEntry
                                ID="UseQuantityInVoucher" 
                                TermID="3017"
                                DefaultTerm="Använd kvantiteter vid verifikatregistrering" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AllowEditVoucher" 
                                TermID="3016"
                                DefaultTerm="Tillåt ändringar i verifikat" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AllowEditVoucherDate" 
                                TermID="3558"
                                DefaultTerm="Tillåt ändring av verifikatdatum" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="DisableInlineValidation" 
                                TermID="3350"
                                DefaultTerm="Inaktivera direkt validering" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                        <table>
                            <tr>
                                <td>
                                    <SOE:InstructionList
                                        ID="DisableInlineValidationExplanation"
                                        runat="server">
                                    </SOE:InstructionList>
                                </td>
                            </tr>
                        </table>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="AllowUnbalancedVoucher" 
                                TermID="3015"
                                DefaultTerm="Tillåt icke balanserade verifikat" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AllowUnbalancedAccountDistribution" 
                                TermID="4806"
                                DefaultTerm="Tillåt icke balanserade automatkontering" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AutomaticAccountDistribution" 
                                TermID="3475"
                                DefaultTerm="Generera automatkontering utan fråga" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="SeparateVouchersInPeriodAccounting" 
                                TermID="4828"
                                DefaultTerm="Separata verifikationer för periodisering" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="CreateVouchersForStockTransactions" 
                                TermID="7489"
                                DefaultTerm="Skapa verifikat för lagertransaktioner" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
					</fieldset>
				</div>
			    <div>
					<fieldset> 
						<legend><%=GetText(3021, "Redovisning")%></legend>
						<table>
                            <SOE:SelectEntry
                                ID="MaxYearOpen"
                                TermID="3032"
                                DefaultTerm="Max antal öppna redovisningsår"
                                Width="50"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="MaxPeriodOpen"
                                TermID="3005"
                                DefaultTerm="Max antal öppna perioder"
                                Width="50"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="AllowMultiPeriodChange" 
                                TermID="3006"
                                DefaultTerm="Tillåt ändringar i flera perioder" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseDimsInRegistration" 
                                TermID="7265"
                                DefaultTerm="Stanna som standard i internkonto vid verifikatregistrering/kontering" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="CreateDiffRowOnBalanceTransfer" 
                                TermID="7492"
                                DefaultTerm="Skapa rad för differens vid överföring av ingående balanser från föregående år" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:SelectEntry
                                ID="DefaultVatCodeAccounting" 
                                TermID="3906"
                                DefaultTerm="Standard momskod" 
                                runat="server">
                            </SOE:SelectEntry>
						</table>
					</fieldset>
                </div>
			    <div>
					<fieldset> 
						<legend><%=GetText(2074, "Verifikatserier")%></legend>
						<table>
                            <SOE:SelectEntry
                                ID="VoucherSeriesTypeManual"
                                TermID="3133"
                                DefaultTerm="Standardserie för manuella verifikat"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="VoucherSeriesTypeVat"
                                TermID="5505"
                                DefaultTerm="Standardserie för momsavräkningsverifikat"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="SupplierInvoiceVoucherSeries"
                                TermID="3104"
                                DefaultTerm="Standardserie för lev.fakturor"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="SupplierPaymentVoucherSeries"
                                TermID="3134"
                                DefaultTerm="Standardserie för lev.betalningar"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="CustomerInvoiceVoucherSeries"
                                TermID="3105"
                                DefaultTerm="Standardserie för kundfakturor"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="CustomerPaymentVoucherSeries"
                                TermID="3135"
                                DefaultTerm="Standardserie för kundbetalningar"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="StockVoucherSeries"
                                TermID="4660"
                                DefaultTerm="Standardserie för lager verifikat"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="PayrollAccountExportVoucherSeries"
                                TermID="4662"
                                DefaultTerm="Standardserie för löneexport verifikat"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="AccountdistributionVoucherSeries"
                                TermID="11823"
                                DefaultTerm="Standardserie för periodkontering"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
						</table>
					</fieldset>
                </div>
                <div>
				    <fieldset> 
					    <legend><%=GetText(3387, "Utskrift")%></legend>
					    <table>
                            <SOE:SelectEntry
                                ID="DefaultAccountingOrder" 
                                TermID="5675"
                                DefaultTerm="Standard bokföringsorder"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultVoucherList" 
                                TermID="5678"
                                DefaultTerm="Standard verifikatlista"
                                runat="server">
                            </SOE:SelectEntry>
                              <SOE:SelectEntry
                                ID="DefaultAccountAnalysis" 
                                TermID="4784"
                                DefaultTerm="Kontoanalys rapport"
                                runat="server">
                            </SOE:SelectEntry>
					    </table>
                    </fieldset>
                </div>
                <div>
				    <fieldset> 
					    <legend><%=GetText(3861, "Valuta")%></legend>
					    <table>
                            <SOE:CheckBoxEntry
                                ID="ShowEnterpriseCurrency" 
                                TermID="3863"
                                DefaultTerm="Visa koncernvaluta"
                                runat="server">
                            </SOE:CheckBoxEntry>
					    </table>
                    </fieldset>
                </div>
                <div>
				    <fieldset> 
					    <legend><%=GetText(1803, "Import")%></legend>
					    <table>
                            <SOE:SelectEntry
                                ID="VoucherImportVoucherSerie"
                                TermID="7247"
                                DefaultTerm="Standardserie för verifikatimport"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:TextEntry 
                                    ID="VoucherImportDefaultAccount" 
                                    TermID="7248" 
                                    DefaultTerm="Standardkonto för verifikatimport" 
                                    OnChange="accountSearch.searchField('VoucherImportDefaultAccount')"
                                    OnKeyUp="accountSearch.keydown('VoucherImportDefaultAccount')"
                                    Width="40"
                                    runat="server">
                                </SOE:TextEntry>
					    </table>
                    </fieldset>
                </div>
                <div id="DivConsolidatedAccounting" runat="server">
				    <fieldset> 
					    <legend><%=GetText(7274, "Koncernredovisning")%></legend>
					    <table>
                            <SOE:SelectEntry
                                ID="MapCompanyToAccount"
                                TermID="7478"
                                DefaultTerm="Företagsnamn från dotterbolag mappas mot konteringsdimension"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
					    </table>
                    </fieldset>
                </div>
                <div id="DivIntrastat" runat="server">
				    <fieldset> 
					    <legend><%=GetText(7633, "Intrastat")%></legend>
					    <table>
                            <SOE:SelectEntry
                                ID="IntrastatImportOriginType"
                                TermID="7641"
                                DefaultTerm="Intrastat för införsel"
                                Width="150"
                                runat="server">
                            </SOE:SelectEntry>
					    </table>
                    </fieldset>
                </div>
            </SOE:Tab>
        </tabs>
    </SOE:Form>

    <div class="searchTemplate">
	    <div id="searchContainer" class="searchContainer"></div>    
        <div id="accountSearchItem_$accountNr$">
            <div id="account_$id$" class="item" onmouseover="searchComponent.select();" onclick="searchComponent.choose();">
                <div class="id" id="extendNumWidth_$id$">$accountNr$</div>
                <div class="name" id="extendNameWidth_$id$">$accountName$</div>
            </div>
        </div>
    </div>

</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>
