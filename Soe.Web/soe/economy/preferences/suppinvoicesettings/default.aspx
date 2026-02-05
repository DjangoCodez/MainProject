<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.suppinvoicesettings._default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="5421" DefaultTerm="Inställningar leverantörsreskontra" runat="server">
			    <div>
				    <fieldset> 
					    <legend><%=GetText(3067, "Registrering")%></legend>
                        <div class="col">
                            <table>
                                <SOE:SelectEntry
                                    ID="DefaultVatType" 
                                    TermID="3519"
                                    DefaultTerm="Standard momstyp" 
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultPaymentCondition" 
                                    TermID="3012"
                                    DefaultTerm="Standard betalningsvillkor" 
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultPaymentMethod"
                                    TermID="9044"
                                    DefaultTerm="Standard betalmetod"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="SettlePaymentMethod"
                                    TermID="9299"
                                    DefaultTerm="Utjämningsbetalmetod"
                                    runat="server">
                                </SOE:SelectEntry>
                                 <SOE:SelectEntry
                                    ID="ObservationMethod"
                                    TermID="9190"
                                    DefaultTerm="Bevakning kreditnotor"
                                    runat="server">
                                </SOE:SelectEntry>
                                 <SOE:NumericEntry
                                        ID="ObservationDays" 
                                        TermID="9191"
                                        DefaultTerm="Bevakning antal dagar" 
                                        MaxLength="3"
                                        Width="50"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                </SOE:NumericEntry>
                                   <SOE:SelectEntry
                                    ID="BankCode"
                                    TermID="9263"
                                    DefaultTerm="Bank för utlandsbetalning"
                                    runat="server">
                                </SOE:SelectEntry>                                                
                                <SOE:CheckBoxEntry
                                    ID="AggregatePaymentsInSEPAExportFile" 
                                    TermID="4830"
                                    DefaultTerm="Anslut betalningar i SEPA exportfil"
                                    runat="server">
                                </SOE:CheckBoxEntry>          
                                <SOE:CheckBoxEntry
                                    ID="UseInternalAccountsWithBalanceSheetAccounts" 
                                    TermID="4865"
                                    DefaultTerm="Internkonto på balanskonton"
                                    runat="server">
                                </SOE:CheckBoxEntry>          
                                <SOE:CheckBoxEntry
                                    ID="UseQuantityInSupplierInvoiceAccountingRows" 
                                    TermID="7725"
                                    DefaultTerm="Använd kvantiteter vid fakturaregistrering"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                            </table>
                        </div>
                        <div>
					        <table>
                                <SOE:CheckBoxEntry
                                    ID="DefaultDraft" 
                                    TermID="3079"
                                    DefaultTerm="Spara som preliminär förvald"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="KeepSupplier" 
                                    TermID="4864"
                                    DefaultTerm="Behåll leverantör när ny faktura registreras"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AllowEditOrigin"
                                    TermID="3089"
                                    DefaultTerm="Tillåt redigering av definitiva fakturor"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AllowEditAccountingRows"
                                    TermID="4821"
                                    DefaultTerm="Tillåt redigering av konteringsrader även om verifikat har skapats"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AutomaticAccountDistribution" 
                                    TermID="3475"
                                    DefaultTerm="Generera automatkontering utan fråga" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="InvoiceTransferToVoucher" 
                                    TermID="3073"
                                    DefaultTerm="Överför faktura automatiskt till verifikat"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="SupplierInvoiceAskPrintVoucherOnTransfer" 
                                    TermID="3317"
                                    DefaultTerm="Fråga om utskrift vid överföring till verifikat" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="PaymentManualTransferToVoucher" 
                                    TermID="1828"
                                    DefaultTerm="Överför betalning automatiskt till verifikat" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="SupplierPaymentAskPrintVoucherOnTransfer" 
                                    TermID="3296"
                                    DefaultTerm="Fråga om utskrift vid överföring av betalning till verifikat" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="CloseInvoicesWhenTransferredToVoucher" 
                                    TermID="5782"
                                    DefaultTerm="Stäng faktura när den är bokförd och betald" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="SetPaymentDefaultPayDateAsDueDate" 
                                    TermID="5694"
                                    DefaultTerm="Föreslå förfallodatum som betaldatum för betalningar"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UsePayementSuggestions" 
                                    TermID="7108"
                                    DefaultTerm="Använd betalningsförslag"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="FICheckOCRValidity" 
                                    TermID="9246"
                                    DefaultTerm="Finsk OCR validering"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseTimeDiscount" 
                                    TermID="4805"
                                    DefaultTerm="Använd tid rabatt"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="HideAutogiroInvoicesFromUnpaid" 
                                    TermID="7291"
                                    DefaultTerm="Dölj autogirofakturor i obetalda"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="RoundVatOnSupplerInvoice"
                                    TermID="7294"
                                    DefaultTerm="Moms avrundas till närmaste heltal"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="GetInternalAccountsFromOrder"
                                    TermID="7295"
                                    DefaultTerm="Hämta internkonton från order"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AutoTransferAutogiroInvoices"
                                    TermID="7297"
                                    DefaultTerm="Automatiskt skapande av betalning för autogirofakturor"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AutoTransferAutogiroPaymentsToVoucher"
                                    TermID="7299"
                                    DefaultTerm="Automatisk avprickning av autogirofakturor"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseAutoAccountDistributionOnVoucher" 
                                    TermID="7617"
                                    DefaultTerm="Använd automatkontering vid överföring till verifikat" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
					        </table>
                        </div>
                    </fieldset>
                </div>
			    <div>
				    <fieldset> 
					    <legend><%=GetText(3136, "Löpnummer")%></legend>
                        <div>
				            <fieldset> 
					            <legend><%=GetText(3798, "Faktura")%></legend>
					            <table>
                                    <tr>
                                        <td colspan="2">
                                                <SOE:InstructionList ID="SeqNbrStartWarning"
                                                runat="server">
                                            </SOE:InstructionList>
                                        </td>
                                    </tr>
                                    <SOE:BooleanEntry
                                        ID="SeqNbrPerType" 
                                        TermID="3137"
                                        DefaultTerm="En nummerserie per fakturatyp" 
                                        OnClick ="SeqNbrPerTypeChanged()"
                                        runat="server">
                                    </SOE:BooleanEntry>
                                    <tr>
                                        <td colspan="2">
                                            <SOE:InstructionList ID="SeqNbrPerTypeInstruction"
                                                runat="server">
                                            </SOE:InstructionList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            &nbsp;
                                        </td>
                                        <td style="padding-left:85px">
                                            <SOE:Text 
			                                    ID="LastUsedSeqNbr"
                                                TermID="3507"
                                                DefaultTerm="Senast använda"
                                                FitInTable="true"
                                                runat="server">
                                            </SOE:Text>
                                        </td>
                                    </tr>
                                    <SOE:NumericEntry
                                        ID="SeqNbrStart" 
                                        TermID="3306"
                                        DefaultTerm="Startnummer" 
                                        MaxLength="10"
                                        Width="70"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="SeqNbrStartDebit" 
                                        TermID="3307"
                                        DefaultTerm="Startnummer debetfakturor" 
                                        MaxLength="10"
                                        Width="70"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="SeqNbrStartCredit" 
                                        TermID="3308"
                                        DefaultTerm="Startnummer kreditfakturor" 
                                        MaxLength="10"
                                        Width="70"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="SeqNbrStartInterest" 
                                        TermID="3309"
                                        DefaultTerm="Startnummer räntefakturor" 
                                        MaxLength="10"
                                        Width="70"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
					            </table>
                            </fieldset>
                        </div>
                    </fieldset>
                </div>
                <div>
				    <fieldset> 
					    <legend><%=GetText(3387, "Utskrift")%></legend>
					    <table>
                            <SOE:SelectEntry
                                ID="DefaultSupplierBalanceList" 
                                TermID="4257"
                                DefaultTerm="Saldolista"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultPaymentSuggestionList" 
                                TermID="7109"
                                DefaultTerm="Betalningsförslag"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultChecklistPayments" 
                                TermID="7525"
                                DefaultTerm="Avstämningslista betalningar"
                                runat="server">
                            </SOE:SelectEntry>
					    </table>
                    </fieldset>
                </div>
                <div>
				    <fieldset> 
					    <legend><%=GetText(3861, "Valuta")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="CurrencySource"
                                TermID="5702" DefaultTerm="Hämta valutakurser från"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="CurrencyIntervalType"
                                TermID="5703" DefaultTerm="Uppdatering kurser"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="ShowTransactionCurrency" 
                                TermID="3862"
                                DefaultTerm="Visa transaktionsvaluta"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ShowEnterpriseCurrency" 
                                TermID="3863"
                                DefaultTerm="Visa koncernvaluta"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ShowLedgerCurrency" 
                                TermID="3864"
                                DefaultTerm="Visa reskontravaluta"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
				    <fieldset> 
					    <legend><%=GetText(1758, "Hopslagning till verifikat")%></legend>
                        <div class="col">
                            <fieldset> 
					            <legend><%=GetText(5680, "Faktura")%></legend>
					            <table>
                                    <SOE:CheckBoxEntry
                                        ID="InvoiceMergeVoucherOnVoucherDate" 
                                        TermID="1759"
                                        DefaultTerm="Slå ihop verifikat per bokföringsdatum" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="InvoiceMergeVoucherRowsOnAccount" 
                                        TermID="1760"
                                        DefaultTerm="Slå ihop verifikatrader" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
					            </table>
                            </fieldset>
                        </div>
                        <div class="col">
                            <fieldset> 
					            <legend><%=GetText(5681, "Betalning")%></legend>
					            <table>
                                    <SOE:CheckBoxEntry
                                        ID="PaymentMergeVoucherOnVoucherDate" 
                                        TermID="1759"
                                        DefaultTerm="Slå ihop verifikat per bokföringsdatum" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="PaymentMergeVoucherRowsOnAccount" 
                                        TermID="1760"
                                        DefaultTerm="Slå ihop verifikatrader" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
					            </table>
                            </fieldset>
                        </div>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <div class="col">
				            <fieldset> 
					            <legend><%=GetText(3851, "Åldersfördelning")%></legend>
					            <table>
                                    <SOE:SelectEntry
                                        ID="AgeDistNbrOfIntervals" 
                                        TermID="3852"
                                        DefaultTerm="Antal fördelningar"
                                        Width="56"   
                                        OnChange="AgeDistNbrOfIntervalsChanged()"                             
                                        runat="server">
                                    </SOE:SelectEntry>
                                    <SOE:NumericEntry
                                        ID="AgeDistInterval1" 
                                        TermID="3853"
                                        DefaultTerm="Brytpunkt 1" 
                                        MaxLength="3"
                                        Width="50"
                                        OnChange="SetAgeDistributionExample()"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="AgeDistInterval2" 
                                        TermID="3854"
                                        DefaultTerm="Brytpunkt 2" 
                                        MaxLength="3"
                                        Width="50"                                
                                        OnChange="SetAgeDistributionExample()"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="AgeDistInterval3" 
                                        TermID="3855"
                                        DefaultTerm="Brytpunkt 3" 
                                        MaxLength="3"
                                        Width="50"                                
                                        OnChange="SetAgeDistributionExample()"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="AgeDistInterval4" 
                                        TermID="3856"
                                        DefaultTerm="Brytpunkt 4" 
                                        MaxLength="3"
                                        Width="50"                                
                                        OnChange="SetAgeDistributionExample()"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="AgeDistInterval5" 
                                        TermID="3857"
                                        DefaultTerm="Brytpunkt 5" 
                                        MaxLength="3"
                                        Width="50"                                
                                        OnChange="SetAgeDistributionExample()"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <tr>
                                        <td colspan=2>
                                            <asp:Label ID="AgeDistExample" runat="server" Text="" style="margin-left:5px" />
                                        </td>
                                    </tr>
                                </table>
                            </fieldset>
                        </div>
                        <!--<div class="col">
				            <fieldset> 
					            <legend><%=GetText(3860, "Likviditetsplanering")%></legend>
					            <table>
                                    <SOE:SelectEntry
                                        ID="LiqPlanNbrOfIntervals" 
                                        TermID="3852"
                                        DefaultTerm="Antal fördelningar"
                                        Width="56"   
                                        OnChange="LiqPlanNbrOfIntervalsChanged()"                             
                                        runat="server">
                                    </SOE:SelectEntry>
                                    <SOE:NumericEntry
                                        ID="LiqPlanInterval1" 
                                        TermID="3853"
                                        DefaultTerm="Brytpunkt 1" 
                                        MaxLength="3"
                                        Width="50"
                                        OnChange="SetLiquidityPlanningExample()"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="LiqPlanInterval2" 
                                        TermID="3854"
                                        DefaultTerm="Brytpunkt 2" 
                                        MaxLength="3"
                                        Width="50"                                
                                        OnChange="SetLiquidityPlanningExample()"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="LiqPlanInterval3" 
                                        TermID="3855"
                                        DefaultTerm="Brytpunkt 3" 
                                        MaxLength="3"
                                        Width="50"                                
                                        OnChange="SetLiquidityPlanningExample()"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="LiqPlanInterval4" 
                                        TermID="3856"
                                        DefaultTerm="Brytpunkt 4" 
                                        MaxLength="3"
                                        Width="50"                                
                                        OnChange="SetLiquidityPlanningExample()"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="LiqPlanInterval5" 
                                        TermID="3857"
                                        DefaultTerm="Brytpunkt 5" 
                                        MaxLength="3"
                                        Width="50"                                
                                        OnChange="SetLiquidityPlanningExample()"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <tr>
                                        <td colspan=2>
                                            <asp:Label ID="LiqPlanExample" runat="server" Text="" style="margin-left:5px" />
                                        </td>
                                    </tr>
                                </table>
                            </fieldset>
                        </div>-->
                    </fieldset>
                </div>
			    <div>
				    <fieldset> 
					    <legend><%=GetText(4563, "Fakturaattest")%></legend>
					    <table>
                            <SOE:SelectEntry
                                ID="AttestFlowState" 
                                TermID="4564"
                                DefaultTerm="Nivå för överföring till reskontra" 
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="AttestFlowProjectLeader" 
                                TermID="9153"
                                DefaultTerm="Standard attestövergång för projektledare" 
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="AttestGroupSelect" 
                                TermID="4661"
                                DefaultTerm="Standard attestgrupp" 
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="AttestFlowSelect" 
                                TermID="4565"
                                DefaultTerm="Standardmall" 
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:NumericEntry
                                ID="AttestFlowMinAmount" 
                                TermID="4566"
                                DefaultTerm="Obligatorisk användare vid belopp över" 
                                MaxLength="10"
                                Width="70"                                
                                AllowDecimals="false"
                                AllowNegative="false"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:SelectEntry
                                ID="AttestFlowAmountUserId" 
                                TermID="4567"
                                DefaultTerm="Användare enligt ovan" 
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:NumericEntry
                                ID="DaysToWarnBeforeInvoiceIsDue" 
                                TermID="4571"
                                DefaultTerm="Varna antal dagar innan förfallodatum vid ej utförd attest"
                                MaxLength="10"
                                Width="50"                                
                                AllowDecimals="false"
                                AllowNegative="false"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:CheckBoxEntry
                                ID="ShowNonAttestedInvoices" 
                                TermID="7185"
                                DefaultTerm="Visa personligt attesterade men ej slutattesterade fakturor under 'Mina attesterade'"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ShowOnlyAttestedInvoicesAtUnPayed" 
                                TermID="9119"
                                DefaultTerm="Visa endast slutattesterade fakturor under 'Obetalda leverantörsfakturor'"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="CreateAutoAttestFromSupplierOnEDI" 
                                TermID="7229"
                                DefaultTerm="Skapa attest automatisk från leverantören på EDI-fakturor"
                                runat="server">
                            </SOE:CheckBoxEntry>
                             <%-- <SOE:CheckBoxEntry
                                ID="SaveSupplierInvoiceAsOrigin" 
                                TermID="2286"
                                DefaultTerm="Spara ny leverantörsfaktura som underlag vid attest"
                                runat="server">
                            </SOE:CheckBoxEntry>--%>
                             <SOE:SelectEntry
                                ID="SelectEntryAttest" 
                                TermID="4010"
                                DefaultTerm="Status under attest" 
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="SupplierInvoiceTransferToVoucherOnAcceptedAttest" 
                                TermID="9262"
                                DefaultTerm="Överför automatiskt till verifikat vid godkänd slutattest"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:SelectEntry
                                ID="SelectAttestGroupSuggestionPrio1" 
                                TermID="4831"
                                DefaultTerm="Prioritet 1 att attestgrupp automatiskt föreslås om den finns angiven för"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="SelectAttestGroupSuggestionPrio2" 
                                TermID="4832"
                                DefaultTerm="Prioritet 2 att attestgrupp automatiskt föreslås om den finns angiven för"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="SelectAttestGroupSuggestionPrio3" 
                                TermID="4833"
                                DefaultTerm="Prioritet 3 att attestgrupp automatiskt föreslås om den finns angiven för"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="SelectAttestGroupSuggestionPrio4" 
                                TermID="4834"
                                DefaultTerm="Prioritet 4 att attestgrupp automatiskt föreslås om den finns angiven för"
                                runat="server">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                </div>
			    <div>
				    <fieldset> 
					    <legend><%=GetText(7663, "Massvidarefakturering av leverantörsfakturor")%></legend>
					    <table>
                            <SOE:SelectEntry
                                ID="BatchOnwardInvoiceingOrderTemplate" 
                                TermID="7665"
                                DefaultTerm="Ordermall för massvidarefakturering"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="BatchOnwardInvoiceingAttachImage" 
                                TermID="7668"
                                DefaultTerm="Bifoga bild vid massvidarefakturering" 
                                runat="server">
                            </SOE:CheckBoxEntry>
					    </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3098, "Betalningar")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="SupplierPaymentNotificationRecipientGroup"
                                TermID="3362"
                                DefaultTerm="Mottagargrupp för notifieringar"
                                runat="server">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
					    <legend><%=GetText(3501, "Inventarier")%></legend>
					    <table>
                            <SOE:FormIntervalEntry
                                ID="InventoryEditTriggerAccounts"
                                TermID="3502" DefaultTerm="Konton för registrering av inventarie" 
                                OnlyFrom="true"
                                NoOfIntervals="10"
                                DisableHeader="false"
                                DisableSettings="true"
                                EnableCheck="false"
                                EnableDelete="true"
                                ContentType="1"
                                LabelType="2"
                                LabelWidth="150"
                                FromWidth="40"
                                runat="server">
                            </SOE:FormIntervalEntry>
                        </table>
                    </fieldset>
                </div>
			    <div>
				    <fieldset> 
					    <legend><%=GetText(7482, "Tolkning av fakturor")%></legend>
					    <table>
                            <SOE:CheckBoxEntry
                                ID="ScanningTransferToInvoice" 
                                TermID="7483"
                                DefaultTerm="Överför tolkning automatiskt till leverantörsfaktura" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ScanningCloseWhenTransferedToInvoice" 
                                TermID="7484"
                                DefaultTerm="Stäng tolkad faktura vid överföring till leverantörsfaktura" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:SelectEntry
                                    ID="ScanningReferenceTargetField" 
                                    TermID="4882"
                                    DefaultTerm="Hämta från 'Ref. nr' vid scanning av faktura" 
                                    runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                    ID="ScanningCodeTargetField" 
                                    TermID="4883"
                                    DefaultTerm="Hämta från 'Er referens' vid scanning av faktura (# är separator)" 
                                    runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="ScanningCalcDueDateFromSupplier"
                                TermID="7700"
                                DefaultTerm="Beräkna förfallodatum från leverantör"
                                runat="server">
                            </SOE:CheckBoxEntry>
		                </table>
		            </fieldset>
                </div>
                <div>
				    <fieldset> 
					    <legend><%=GetText(8172, "Finvoice")%></legend>
					    <table>
                            <SOE:CheckBoxEntry
                                ID="FinvoiceImportOnlyForCompany" 
                                TermID="7506"
                                DefaultTerm="Importera endast fakturor som tillhör företaget"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="FinvoiceUseTransferToOrder" 
                                TermID="8845"
                                DefaultTerm="Använd överföring av orderrader"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="FinvoiceTransferToInvoice" 
                                TermID="8191"
                                DefaultTerm="Överför Finvoice fakturor automatiskt till leverantörsfaktura"
                                runat="server">
                            </SOE:CheckBoxEntry>
		                </table>
		            </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(7653, "Artikelrader")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="ProductRowsImport" 
                                TermID="7652"
                                DefaultTerm="Importera artikelrader från e-faktura"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="DetailedCodingRows" 
                                TermID="7880"
                                DefaultTerm="Skapa detaljerade konteringsrader baserade på produktrader"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
				    <fieldset> 
					    <legend><%=GetText(7189, "Rapporter")%></legend>
					    <table>
                            <SOE:CheckBoxEntry
                                ID="ShowPendingPaymentsInReport" 
                                TermID="7188"
                                DefaultTerm='Inkludera betalningar under avprickning'
                                runat="server">
                            </SOE:CheckBoxEntry>
		                </table>
		            </fieldset>
                </div>
            </SOE:Tab>
        </tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
