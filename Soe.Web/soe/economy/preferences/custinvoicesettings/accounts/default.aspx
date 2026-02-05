<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.custinvoicesettings.accounts._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">

     <script type="text/javascript" language="javascript">
       var stdDimID = "<%=stdDimID%>";
       var companyID = "<%=SoeCompany.ActorCompanyId%>";
    </script>

    <SOE:Form ID="Form1" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="3224" DefaultTerm="Baskonton kundreskontra" runat="server">
				<div>
                    <fieldset>
                        <legend><%=GetText(3127, "Kundreskontra")%></legend>
                        <table>
                            <SOE:TextEntry 
                                ID="AccountCustomerSalesVat" 
                                TermID="7684" 
                                DefaultTerm="Försäljning - momspliktig"
                                OnChange="accountSearch.searchField('AccountCustomerSalesVat')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerSalesVat')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerSalesNoVat" 
                                TermID="7685" 
                                DefaultTerm="Försäljning - momsfri"
                                OnChange="accountSearch.searchField('AccountCustomerSalesNoVat')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerSalesNoVat')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerSalesWithinEU" 
                                TermID="7686" 
                                DefaultTerm="Försäljning - inom EU (varor)"
                                OnChange="accountSearch.searchField('AccountCustomerSalesWithinEU')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerSalesWithinEU')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerSalesWithinEUService" 
                                TermID="7689" 
                                DefaultTerm="Försäljning - inom EU (tjänster)"
                                OnChange="accountSearch.searchField('AccountCustomerSalesWithinEUService')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerSalesWithinEUService')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerSalesOutsideEU" 
                                TermID="7687" 
                                DefaultTerm="Försäljning - utanför EU (varor)"
                                OnChange="accountSearch.searchField('AccountCustomerSalesOutsideEU')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerSalesOutsideEU')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerSalesOutsideEUService" 
                                TermID="7690" 
                                DefaultTerm="Försäljning - utanför EU (tjänster)"
                                OnChange="accountSearch.searchField('AccountCustomerSalesOutsideEUService')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerSalesOutsideEUService')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerSalesTripartiteTrade" 
                                TermID="7688" 
                                DefaultTerm="Försäljning - trepartshandel"
                                OnChange="accountSearch.searchField('AccountCustomerSalesTripartiteTrade')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerSalesTripartiteTrade')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerFreight" 
                                TermID="3162" 
                                DefaultTerm="Frakt" 
                                OnChange="accountSearch.searchField('AccountCustomerFreight')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerFreight')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerOrderFee" 
                                TermID="3163" 
                                DefaultTerm="Fakt. avgift"
                                OnChange="accountSearch.searchField('AccountCustomerOrderFee')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerOrderFee')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerInsurance" 
                                TermID="3164" 
                                DefaultTerm="Försäkring"
                                OnChange="accountSearch.searchField('AccountCustomerInsurance')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerInsurance')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerClaim" 
                                TermID="3165" 
                                DefaultTerm="Kundfordran"
                                OnChange="accountSearch.searchField('AccountCustomerClaim')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerClaim')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountUncertainCustomerClaim" 
                                TermID="8312" 
                                DefaultTerm="Osäkra kundfordringar"
                                OnChange="accountSearch.searchField('AccountUncertainCustomerClaim')"
                                OnKeyUp="accountSearch.keydown('AccountUncertainCustomerClaim')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerUnderpay" 
                                TermID="3166" 
                                DefaultTerm="Differens" 
                                OnChange="accountSearch.searchField('AccountCustomerUnderpay')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerUnderpay')"
                                Width="40"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerOverpay" 
                                TermID="3167" 
                                DefaultTerm="Överbetalning"
                                OnChange="accountSearch.searchField('AccountCustomerOverpay')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerOverpay')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerPenaltyInterest" 
                                TermID="3168" 
                                DefaultTerm="Dröjsmålsränta"
                                OnChange="accountSearch.searchField('AccountCustomerPenaltyInterest')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerPenaltyInterest')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerClaimCharge" 
                                TermID="3169" 
                                DefaultTerm="Kravavgift"
                                OnChange="accountSearch.searchField('AccountCustomerClaimCharge')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerClaimCharge')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerPaymentFromTaxAgency" 
                                TermID="3827" 
                                DefaultTerm="Inbetalning från Skatteverket"
                                OnChange="accountSearch.searchField('AccountCustomerPaymentFromTaxAgency')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerPaymentFromTaxAgency')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerDiscount" 
                                TermID="4186" 
                                DefaultTerm="Kundrabatt"
                                OnChange="accountSearch.searchField('AccountCustomerDiscount')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerDiscount')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry 
                                ID="AccountCustomerDiscountOffset" 
                                TermID="4187" 
                                DefaultTerm="Motkonto rabatter"
                                OnChange="accountSearch.searchField('AccountCustomerDiscountOffset')"
                                OnKeyUp="accountSearch.keydown('AccountCustomerDiscountOffset')"
                                Width="40" 
                                runat="server">
                            </SOE:TextEntry>
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

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
