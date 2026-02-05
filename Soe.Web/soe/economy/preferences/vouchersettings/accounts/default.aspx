<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.vouchersettings.accounts._default" %>
<%@ Import Namespace="SoftOne.Soe.Business.Core" %>
<%@ Import Namespace="SoftOne.Soe.Data" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">

   <script type="text/javascript" language="javascript">
       var stdDimID = "<%=stdDimID%>";
       var companyID = "<%=SoeCompany.ActorCompanyId%>";
    </script>

    <SOE:Form ID="Form1" runat="server">
		<Tabs>
			<SOE:Tab Type="Setting" TermID="3222" DefaultTerm="Baskonton redovisning" runat="server">
				<div>
                    <fieldset>
                        <legend><%=GetText(3021, "Redovisning")%></legend>
                        <div class="col">
                            <table>
                                <SOE:TextEntry 
                                    ID="AccountCommonCheck" 
                                    TermID="3188" 
                                    DefaultTerm="Kassa" 
                                    OnChange="accountSearch.searchField('AccountCommonCheck')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonCheck')"
                                    Width="40"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry 
                                    ID="AccountCommonPG" 
                                    TermID="3189" 
                                    DefaultTerm="PlusGiro" 
                                    OnChange="accountSearch.searchField('AccountCommonPG')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonPG')"
                                    Width="40"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry 
                                    ID="AccountCommonBG" 
                                    TermID="3190" 
                                    DefaultTerm="BankGiro" 
                                    OnChange="accountSearch.searchField('AccountCommonBG')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonBG')"
                                    Width="40"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry 
                                    ID="AccountCommonAG" 
                                    TermID="3191" 
                                    DefaultTerm="Autogiro" 
                                    OnChange="accountSearch.searchField('AccountCommonAG')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonAG')"
                                    Width="40"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry 
                                    ID="AccountCommonCentRounding" 
                                    TermID="3192" 
                                    DefaultTerm="Öresutjämning"
                                    OnChange="accountSearch.searchField('AccountCommonCentRounding')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonCentRounding')"
                                    Width="40" 
                                    runat="server">
                                </SOE:TextEntry>
                            </table>
                        </div>
                        <div>
                            <table>
                                <SOE:TextEntry 
                                    ID="AccountCommonCurrencyProfit" 
                                    TermID="3193" 
                                    DefaultTerm="Valutavinst"
                                    OnChange="accountSearch.searchField('AccountCommonCurrencyProfit')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonCurrencyProfit')"
                                    Width="40" 
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry 
                                    ID="AccountCommonCurrencyLoss" 
                                    TermID="3194" 
                                    DefaultTerm="Valutaförlust"
                                    OnChange="accountSearch.searchField('AccountCommonCurrencyLoss')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonCurrencyLoss')"
                                    Width="40" 
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry 
                                    ID="AccountCommonDiff" 
                                    TermID="3225" 
                                    DefaultTerm="Differens"
                                    OnChange="accountSearch.searchField('AccountCommonDiff')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonDiff')"
                                    Width="40" 
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry 
                                    ID="AccountCommonBankFee" 
                                    TermID="3195" 
                                    DefaultTerm="Bankavgifter"
                                    OnChange="accountSearch.searchField('AccountCommonBankFee')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonBankFee')"
                                    Width="40" 
                                    runat="server">
                                </SOE:TextEntry>
                            </table>
                        </div>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3604, "Inställningar för Periodiseringar")%></legend>
                        <div class="col">
                            <table>
                                <SOE:TextEntry
                                    ID="AccountCommonAccrualCostAccount"
                                    TermID="3605"
                                    DefaultTerm="Standardkonto för periodiserad kostnad"
                                    OnChange="accountSearch.searchField('AccountCommonAccrualCostAccount')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonAccrualCostAccount')"
                                    Width="40"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry
                                    ID="AccountCommonAccrualRevenueAccount"
                                    TermID="3606"
                                    DefaultTerm="Standardkonto för periodiserad intäkt"
                                    OnChange="accountSearch.searchField('AccountCommonAccrualRevenueAccount')"
                                    OnKeyUp="accountSearch.keydown('AccountCommonAccrualRevenueAccount')"
                                    Width="40"
                                    runat="server">
                                </SOE:TextEntry>
                            </table>
                            <fieldset>
                                <legend><%=GetText(3607, "Kontokoppling")%></legend>
                                <div class="col">
                                    <table>
                                        <tr>
                                            <th></th>
                                            <%-- Empty header for balance spacing with FormIntervalEntry --%>
                                            <th width="200px">
                                                <label style="padding-left: 5px"><%=GetText(3504, "Account") %></label></th>
                                            <th width="200px">
                                                <label style="padding-left: 5px"><%=GetText(9345, "Accrual Account")%></label></th>
                                        </tr>
                                        <SOE:FormIntervalEntry
                                            ID="AccrualAccountMapping"
                                            LabelAutoCompleteType="5"
                                            OnlyFrom="false"
                                            DisableHeader="true"
                                            DisableSettings="true"
                                            EnableDelete="true"
                                            ContentType="1"
                                            LabelType="1"
                                            LabelWidth="0"
                                            FromWidth="200"
                                            HideLabel="true"
                                            NoOfIntervals="20"
                                            runat="server">
                                        </SOE:FormIntervalEntry>
                                    </table>

                                </div>
                            </fieldset>
                        </div>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3129, "Moms")%></legend>
                        <table>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable1" 
                                TermID="3180" 
                                DefaultTerm="Utgående moms 1"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable1')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable1')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable2" 
                                TermID="3181" 
                                DefaultTerm="Utgående moms 2"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable2')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable2')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable3" 
                                TermID="3182" 
                                DefaultTerm="Utgående moms 3"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable3')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable3')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                             <SOE:TextEntry 
                                ID="AccountCommonMixedVat" 
                                TermID="8543" 
                                DefaultTerm="Utgående moms Blandad"
                                OnChange="accountSearch.searchField('AccountCommonMixedVat')"
                                OnKeyUp="accountSearch.keydown('AccountCommonMixedVat')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatReceivable" 
                                TermID="3183" 
                                DefaultTerm="Ingående moms"
                                OnChange="accountSearch.searchField('AccountCommonVatReceivable')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatReceivable')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatAccountingKredit" 
                                TermID="5507" 
                                DefaultTerm="Momsredovisning kredit"
                                OnChange="accountSearch.searchField('AccountCommonVatAccountingKredit')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatAccountingKredit')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatAccountingDebet" 
                                TermID="7485" 
                                DefaultTerm="Momsredovisning debet"
                                OnChange="accountSearch.searchField('AccountCommonVatAccountingDebet')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatAccountingDebet')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                        </table>
                    </fieldset>
				</div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3130, "Moms omvänd skattskyldighet")%></legend>
                        <table>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable1Reversed" 
                                TermID="3184" 
                                DefaultTerm="Utgående moms 1"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable1Reversed')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable1Reversed')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable2Reversed" 
                                TermID="3185" 
                                DefaultTerm="Utgående moms 2"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable2Reversed')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable2Reversed')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable3Reversed" 
                                TermID="3186" 
                                DefaultTerm="Utgående moms 3"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable3Reversed')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable3Reversed')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatReceivableReversed" 
                                TermID="3187" 
                                DefaultTerm="Ingående moms"
                                OnChange="accountSearch.searchField('AccountCommonVatReceivableReversed')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatReceivableReversed')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonReverseVatSales" 
                                TermID="6022" 
                                DefaultTerm="Försäljning omvänd moms"
                                OnChange="accountSearch.searchField('AccountCommonReverseVatSales')"
                                OnKeyUp="accountSearch.keydown('AccountCommonReverseVatSales')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>     
                            <SOE:TextEntry 
                                ID="AccountCommonReverseVatPurchase" 
                                TermID="5508" 
                                DefaultTerm="Inköp omvänd moms"
                                OnChange="accountSearch.searchField('AccountCommonReverseVatPurchase')"
                                OnKeyUp="accountSearch.keydown('AccountCommonReverseVatPurchase')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>                           
                        </table>
                    </fieldset>
                </div>
                 <div>
                    <fieldset>
                        <legend><%=GetText(9280, "Moms EU import")%></legend>
                        <table>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable1EUImport" 
                                TermID="3184" 
                                DefaultTerm="Utgående moms 1"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable1EUImport')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable1EUImport')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable2EUImport" 
                                TermID="3185" 
                                DefaultTerm="Utgående moms 2"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable2EUImport')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable2EUImport')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable3EUImport" 
                                TermID="3186" 
                                DefaultTerm="Utgående moms 3"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable3EUImport')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable3EUImport')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatReceivableEUImport" 
                                TermID="3187" 
                                DefaultTerm="Ingående moms"
                                OnChange="accountSearch.searchField('AccountCommonVatReceivableEUImport')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatReceivableEUImport')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPurchaseEUImport" 
                                TermID="7301" 
                                DefaultTerm="Inköp EU moms"
                                OnChange="accountSearch.searchField('AccountCommonVatPurchaseEUImport')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPurchaseEUImport')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>                           
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(7303, "Moms ej EU import")%></legend>
                        <table>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable1NonEUImport" 
                                TermID="3184" 
                                DefaultTerm="Utgående moms 1"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable1NonEUImport')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable1NonEUImport')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable2NonEUImport" 
                                TermID="3185" 
                                DefaultTerm="Utgående moms 2"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable2NonEUImport')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable2NonEUImport')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPayable3NonEUImport" 
                                TermID="3186" 
                                DefaultTerm="Utgående moms 3"
                                OnChange="accountSearch.searchField('AccountCommonVatPayable3NonEUImport')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPayable3NonEUImport')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatReceivableNonEUImport" 
                                TermID="3187" 
                                DefaultTerm="Ingående moms"
                                OnChange="accountSearch.searchField('AccountCommonVatReceivableNonEUImport')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatReceivableNonEUImport')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCommonVatPurchaseNonEUImport" 
                                TermID="7302" 
                                DefaultTerm="Inköp ej EU moms"
                                OnChange="accountSearch.searchField('AccountCommonVatPurchaseNonEUImport')"
                                OnKeyUp="accountSearch.keydown('AccountCommonVatPurchaseNonEUImport')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>                           
                        </table>
                    </fieldset>
                </div>
            </SOE:Tab>
        </Tabs>
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

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
