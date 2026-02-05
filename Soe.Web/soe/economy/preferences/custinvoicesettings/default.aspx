<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.custinvoicesettings._default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="5422" DefaultTerm="Inställningar kundreskontra" runat="server">
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
                                    ID="DefaultPaymentConditionClaimsAndInterest" 
                                    TermID="5749"
                                    DefaultTerm="Standard betalningsvillkor krav/ränta"
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
                                <SOE:TextEntry
                                    ID="DefaultCreditLimit"
                                    TermID="3865" 
                                    DefaultTerm="Standard kreditgräns"
                                    MaxLength="50"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry
                                    ID="OurReference"
                                    TermID="3291" DefaultTerm="Vår referens"
                                    MaxLength="50"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry
                                    ID="DnBNorClientId"
                                    TermID="9278" DefaultTerm="Klientnr hos DnB NOR Finans"
                                    MaxLength="5"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry
                                    ID="AutogiroClientId"
                                    TermID="9282" DefaultTerm="Kundnr hos BGC (Autogiro)"
                                    MaxLength="6"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:CheckBoxEntry
                                    ID="ApplyQuantitiesDuringInvoiceEntry"
                                    TermID="9382"
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
                                    ID="CustomerInvoiceAskPrintVoucherOnTransfer" 
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
                                    ID="CustomerPaymentAskPrintVoucherOnTransfer" 
                                    TermID="3295"
                                    DefaultTerm="Fråga om utskrift vid överföring av betalning till verifikat" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="CloseInvoicesWhenTransferredToVoucher" 
                                    TermID="5750"
                                    DefaultTerm="Stäng faktura när den är betald och bokförd" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="CloseInvoicesWhenExported" 
                                    TermID="9185"
                                    DefaultTerm="Stäng faktura när den är exporterad" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseDeliveryCustomerInvoicing" 
                                    TermID="4663"
                                    DefaultTerm="Använd betal och leveranskund" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TransferPaymentServiceOnlyToContract" 
                                    TermID="9281"
                                    DefaultTerm="Överför betaltjänst enbart till avtal" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TriangulationSales" 
                                    TermID="4884"
                                    DefaultTerm="Mellanmans försäljning varor EU" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AddCustomerNameToPaymentInternaDescr" 
                                    TermID="7528"
                                    DefaultTerm="Lägg till kundnamn från betalningsimporten till interntext" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseAutoAccountDistributionOnVoucher" 
                                    TermID="7617"
                                    DefaultTerm="Använd automatkontering vid överföring till verifikat" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AllowChangesToInternalAccountsOnPaidCustomerInvoice" 
                                    TermID="7681"
                                    DefaultTerm="Tillåt ändring av internkonton på betald men ej bokförd kundfaktura" 
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
                                    <SOE:NumericEntry
                                        ID="SegNbrStartCash" 
                                        TermID="4593"
                                        DefaultTerm="Startnummer kontantfakturor" 
                                        MaxLength="10"
                                        Width="70"                                
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:CheckBoxEntry
                                        ID="AutomaticLedgerInvoiceNrWhenImport" 
                                        TermID="7695"
                                        DefaultTerm="Automatiskt fakturanr vid import av reskontrafakturor" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
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
                                ID="DefaultCustomerBalanceList" 
                                TermID="4257"
                                DefaultTerm="Saldolista"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultReminderTemplate" 
                                TermID="7001"
                                DefaultTerm="Standard kravbrevsmall" 
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultInterestTemplate" 
                                TermID="5725"
                                DefaultTerm="Standard räntefakturamall" 
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultInterestRateCalculationTemplate" 
                                TermID="5783"
                                DefaultTerm="Standard ränteberäkningsmall" 
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
                <div id="DivAutomaster" runat="server">
                    <fieldset>
                        <legend><%=GetText(4873, "Automaster")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="CombineAccountingRows" 
                                TermID="4868"
                                DefaultTerm="Slå ihop konteringsrader" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseAutomaticDistribution" 
                                TermID="4869"
                                DefaultTerm="Använd automatkontering" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:TextEntry
                                ID="CashCustomerNumber"
                                TermID="4877" 
                                DefaultTerm="Kontant kund kundnummer"
                                MaxLength="10"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="TransferTypesToBeClosed"
                                TermID="4870" 
                                DefaultTerm="Överföringstyper som ska stängs"
                                MaxLength="50"
                                runat="server">
                            </SOE:TextEntry>
                            <tr>
                                <td colspan="2">
                                    <SOE:InstructionList 
                                        ID="TransferTypeInstruction"
                                        runat="server">
                                    </SOE:InstructionList>
                                </td>
                            </tr>
                        </table>
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
            </SOE:Tab>
            <SOE:Tab Type="Setting" TermID="5405" DefaultTerm="Inställningar krav och räntefakturering" runat="server">
                <div>
				    <fieldset> 
                        <legend><%=GetText(1882, "Krav och räntefakturering")%></legend>
                        <div>
				            <fieldset> 
					            <legend><%=GetText(1925, "Kravavgift")%></legend>
					            <table>
                                    <SOE:CheckBoxEntry
                                        ID="ReminderGenerateProductRow" 
                                        TermID="1926"
                                        DefaultTerm="Aktivera kravhantering" 
                                        OnClick = "ReminderGenerateProductRowChecked()"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="ReminderHandlingTypeNew" 
                                        TermID="1931"
                                        DefaultTerm="Faktureras separat som ny faktura" 
                                        OnClick = "ReminderHandlingTypeNewChecked()"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="ReminderHandlingTypeNext" 
                                        TermID="1927"
                                        DefaultTerm="Adderas till nästkommande faktura" 
                                        OnClick = "ReminderHandlingTypeNextChecked()"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                </table>
                                <table>
                                    <SOE:NumericEntry
                                        ID="MinNrOfDaysForNewClaim" 
                                        TermID="4707"
                                        DefaultTerm="Minsta antal dagar för att få skapa nytt kravbrev till samma kund" 
                                        Width="60"
                                        MaxLength="10"
                                        runat="server">
                                    </SOE:NumericEntry>
					            </table>
					        </fieldset>
			            </div>
                        <div>
				            <fieldset> 
					            <legend><%=GetText(4084, "Räntefakturering")%></legend>
					            <table>
                                    <SOE:CheckBoxEntry
                                        ID="InterestHandlingTypeNew" 
                                        TermID="1931"
                                        DefaultTerm="Faktureras separat som ny faktura" 
                                        OnClick = "InterestHandlingTypeNewChecked()"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="InterestHandlingTypeNext" 
                                        TermID="1927"
                                        DefaultTerm="Adderas till nästkommande faktura" 
                                        OnClick = "InterestHandlingTypeNextChecked()"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:NumericEntry
                                        ID="InterestPercent" 
                                        TermID="1933"
                                        DefaultTerm="Dröjsmålsränta %" 
                                        MaxLength="10"
                                        AllowDecimals="true"
                                        Width="60"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="InterestAccumulatedBeforeInvoice" 
                                        TermID="4090"
                                        DefaultTerm="Minsta räntebelopp för fakturering" 
                                        MaxLength="10"
                                        Width="60"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:NumericEntry
                                        ID="GracePeriodDays" 
                                        TermID="4087"
                                        DefaultTerm="Antal dagar innan ränta ackumuleras" 
                                        runat="server"
                                        MaxLength="3"
                                        Width="60">
                                    </SOE:NumericEntry>
                                </table>
                            </fieldset>
                        </div>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(4708, "Kravnivåer")%></legend>
                        <SOE:SelectEntry
                            ID="NrOfClaimLevels"
                            Width="100"
                            LabelWidth="200"
                            TermID="4714" 
                            DefaultTerm="Antal kravnivåer innan inkasso"
                            runat="server">
                        </SOE:SelectEntry> 

                        <fieldset>
                            <legend><%=GetText(4709, "Kravnivå 1")%></legend>
                            <asp:TextBox ID="ClaimLevelText1" runat="server" TextMode="MultiLine" Width="100%" Height="80px" Rows="10" />
                        </fieldset>
                        <fieldset>
                            <legend><%=GetText(4710, "Kravnivå 2")%></legend>
                            <asp:TextBox ID="ClaimLevelText2" runat="server" TextMode="MultiLine" Width="100%" Height="80px" Rows="10" />
                        </fieldset>
                        <fieldset>
                            <legend><%=GetText(4711, "Kravnivå 3")%></legend>
                            <asp:TextBox ID="ClaimLevelText3" runat="server" TextMode="MultiLine" Width="100%" Height="80px" Rows="10" />
                        </fieldset>
                        <fieldset>
                            <legend><%=GetText(4712, "Kravnivå 4")%></legend>
                            <asp:TextBox ID="ClaimLevelText4" runat="server" TextMode="MultiLine" Width="100%" Height="80px" Rows="10" />
                        </fieldset>
                        <fieldset>
                            <legend><%=GetText(4713, "Kravnivå INKASSO")%></legend>
                            <asp:TextBox ID="ClaimLevelText5" runat="server" TextMode="MultiLine" Width="100%" Height="80px" Rows="10" />
                        </fieldset>
                    </fieldset>
                </div>
            </SOE:Tab>
        </tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
