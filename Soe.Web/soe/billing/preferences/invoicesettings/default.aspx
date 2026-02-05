<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.preferences.invoicesettings._default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
            <SOE:Tab Type="Setting" TermID="5430" DefaultTerm="Inställningar försäljning" runat="server">
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
                                    ID="DefaultVatCode"
                                    TermID="3906"
                                    DefaultTerm="Standard momskod"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultPriceListType"
                                    TermID="4104" DefaultTerm="Standard prislista"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultDeliveryType"
                                    TermID="3262"
                                    DefaultTerm="Standard leveranssätt"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultDeliveryCondition"
                                    TermID="3263"
                                    DefaultTerm="Standard leveransvillkor"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultPaymentCondition"
                                    TermID="3012"
                                    DefaultTerm="Standard betalningsvillkor"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultWholeSeller"
                                    TermID="7011"
                                    DefaultTerm="Standardgrossist"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultOneTimeCustomer"
                                    TermID="7517"
                                    DefaultTerm="Engångskund"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultPaymentConditionHouseholdDeduction"
                                    TermID="7051"
                                    DefaultTerm="Betalningsvillkor skattereduktion"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:TextEntry
                                    ID="OurReference"
                                    TermID="3291" DefaultTerm="Vår referens"
                                    MaxLength="50"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:NumericEntry
                                    ID="InvoiceFeeLimitAmount"
                                    TermID="7131"
                                    DefaultTerm="Beloppsgräns för faktureringsavgift"
                                    MaxLength="10"
                                    Width="70"
                                    AllowDecimals="false"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:NumericEntry
                                    ID="InvoiceNumberLength"
                                    TermID="4234"
                                    DefaultTerm="Antal tecken i fakturanummer"
                                    MaxLength="2"
                                    Width="20"
                                    AllowDecimals="false"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:NumericEntry
                                    ID="OfferValidNoOfDays"
                                    TermID="7724"
                                    DefaultTerm="Antal dagar offert är giltig"
                                    MaxLength="5"
                                    Width="20"
                                    AllowDecimals="false"
                                    runat="server">
                                </SOE:NumericEntry>
                            </table>
                        </div>
                        <div>
                            <table>
                                <SOE:CheckBoxEntry
                                    ID="CopyInvoiceNrToOcr"
                                    TermID="3106"
                                    DefaultTerm="Använd fakturanr som OCR"
                                    OnClick="FakturaOCRChecked()"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="FormReferenceToOcr"
                                    TermID="4702"
                                    DefaultTerm="Skapa RF-referens som OCR"
                                    OnClick="RFOCRChecked()"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="FormFIReferenceToOcr"
                                    TermID="4715"
                                    DefaultTerm="Använd FI-referens utan RF som OCR"
                                    OnClick="FIOCRChecked()"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseFreightAmount"
                                    TermID="3841"
                                    DefaultTerm="Använd frakt"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseInvoiceFee"
                                    TermID="3264"
                                    DefaultTerm="Använd faktureringsavgift"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseCentRounding"
                                    TermID="3279"
                                    DefaultTerm="Använd öresutjämning"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseInvoiceFeeLimit"
                                    TermID="7130"
                                    DefaultTerm="Använd beloppsgräns för faktureringsavgift"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseCashSales"
                                    TermID="4576"
                                    DefaultTerm="Använd kontantförsäljning"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="HideVatWarnings"
                                    TermID="3894"
                                    DefaultTerm="Dölj momsvarningar"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="CloseInvoicesWhenTransferredToVoucher"
                                    TermID="5750"
                                    DefaultTerm="Stäng faktura när den är betald och bokförd"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="ShowWarningOnZeroRows"
                                    TermID="7167"
                                    DefaultTerm="Visa varning vid negativt TB"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="MandatoryChecklist"
                                    TermID="9137"
                                    DefaultTerm="Krav på att checklistan är ifylld vid klarmarkering"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AutomaticCustomerOwner"
                                    TermID="7203"
                                    DefaultTerm="Sätt automatiskt ägare på kund"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="HideTaxDeductionContacts"
                                    TermID="6150"
                                    DefaultTerm="Dölj skattereduktion"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UsePartialInvoicingOnOrderRow"
                                    TermID="7219"
                                    DefaultTerm="Använd delleverans på orderrad"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="HideVatRate"
                                    TermID="7237"
                                    DefaultTerm="Dölj momssats"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AskOpenInvoiceWhenCreateInvoiceFromOrder"
                                    TermID="3281"
                                    DefaultTerm="Fråga om öppna faktura som skapas från order"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseDeliveryAdressAsInvoiceAddress"
                                    TermID="4666"
                                    DefaultTerm="Levadress ger fakturaadress"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AllowInvoiceOfCreditOrders"
                                    TermID="7429"
                                    DefaultTerm="Tillåt fakturering av negativa ordrar"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="SetOurReferenceOnMergedInvoices"
                                    TermID="7645"
                                    DefaultTerm="Ange vår referens på ordernivå vid samfakturering"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseQuantityPrices"
                                    TermID="7680"
                                    DefaultTerm="Använd stafflad prissättning"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseExternalInvoiceNr"
                                    TermID="7599"
                                    DefaultTerm="Använd externt fakturanummer"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                            </table>
                        </div>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3136, "Löpnummer")%></legend>
                        <table>
                            <tr>
                                <td>
                                    <SOE:InstructionList
                                        ID="SeqNbrStartWarning"
                                        runat="server">
                                    </SOE:InstructionList>
                                </td>
                            </tr>
                        </table>
                        <div id="DivOfferSeqNbr" runat="server">
                            <fieldset>
                                <legend><%=GetText(5321, "Offert")%></legend>
                                <table>
                                    <tr>
                                        <td>&nbsp;
                                        </td>
                                        <td style="padding-left: 85px">
                                            <SOE:Text
                                                ID="OfferLastUsedSeqNbr"
                                                TermID="3507"
                                                DefaultTerm="Senast använda"
                                                FitInTable="true"
                                                runat="server">
                                            </SOE:Text>
                                        </td>
                                    </tr>
                                    <SOE:NumericEntry
                                        ID="OfferSeqNbrStart"
                                        TermID="3306"
                                        DefaultTerm="Startnummer"
                                        MaxLength="10"
                                        Width="70"
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                </table>
                            </fieldset>
                        </div>
                        <div id="DivOrderSeqNbr" runat="server">
                            <fieldset>
                                <legend><%=GetText(3797, "Order")%></legend>
                                <table>
                                    <tr>
                                        <td>&nbsp;
                                        </td>
                                        <td style="padding-left: 85px">
                                            <SOE:Text
                                                ID="OrderLastUsedSeqNbr"
                                                TermID="3507"
                                                DefaultTerm="Senast använda"
                                                FitInTable="true"
                                                runat="server">
                                            </SOE:Text>
                                        </td>
                                    </tr>
                                    <SOE:NumericEntry
                                        ID="OrderSeqNbrStart"
                                        TermID="3306"
                                        DefaultTerm="Startnummer"
                                        MaxLength="10"
                                        Width="70"
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <SOE:BooleanEntry
                                        ID="UseOrderSeqNbrInternal"
                                        TermID="7761"
                                        DefaultTerm="Använd nummerserie för internorder"
                                        OnClick="UseOrderSeqNbrInternalChanged()"
                                        runat="server">
                                    </SOE:BooleanEntry>
                                    <SOE:NumericEntry
                                        ID="OrderSeqNbrStartInternal"
                                        TermID="7760"
                                        DefaultTerm="Startnummer internorder"
                                        MaxLength="10"
                                        Width="70"
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                </table>
                            </fieldset>
                        </div>
                        <div>
                            <fieldset>
                                <legend><%=GetText(3798, "Faktura")%></legend>
                                <table>
                                    <SOE:BooleanEntry
                                        ID="SeqNbrPerType"
                                        TermID="3137"
                                        DefaultTerm="En nummerserie per fakturatyp"
                                        OnClick="SeqNbrPerTypeChanged()"
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
                                        <td>&nbsp;
                                        </td>
                                        <td style="padding-left: 85px">
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
                                </table>
                            </fieldset>
                        </div>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3571, "Automatspara")%></legend>
                        <div class="col">
                            <table>
                                <SOE:SelectEntry
                                    ID="AutoSaveOfferInterval"
                                    TermID="3567"
                                    DefaultTerm="Spara offert automatiskt"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="AutoSaveOrderInterval"
                                    TermID="3568"
                                    DefaultTerm="Spara order automatiskt"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="AutoSaveContractInterval"
                                    TermID="3570"
                                    DefaultTerm="Spara avtal automatiskt"
                                    runat="server">
                                </SOE:SelectEntry>
                            </table>
                        </div>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3286, "Fakturatext")%></legend>
                        <textarea
                            id="InvoiceText"
                            rows="5"
                            cols="143"
                            style="overflow-x: none; overflow-y: auto;"
                            onkeypress="return checkTextAreaMaxLength(this, 1024);"
                            runat="server">
			            </textarea>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3378, "Artikelrader")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="MergeInvoiceRowsMerchandise"
                                TermID="3379"
                                DefaultTerm="Slå ihop rader med samma artiklar (varor)"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="MergeInvoiceRowsService"
                                TermID="3380"
                                DefaultTerm="Slå ihop rader med samma artiklar (tjänster)"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultHouseholdDeductionType"
                                TermID="7263"
                                DefaultTerm="Standard skattereduktionstyp på artikelrad"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:NumericEntry
                                ID="ProductRowMarginalLimit"
                                TermID="3550"
                                DefaultTerm="Varna vid täckningsgrad under (%)"
                                MaxLength="6"
                                AllowDecimals="true"
                                Width="70"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:CheckBoxEntry
                                ID="CalculateMarginalIncomeForRowsWithZeroPurchasePrice"
                                TermID="7519"
                                DefaultTerm="Beräkna TB/TG för artikelrader med 0 i inköpspris"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="OrderAskForWholeseller"
                                TermID="3555"
                                DefaultTerm="Fråga alltid efter grossist vid orderregistrering"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="InvoiceAskForWholeseller"
                                TermID="3556"
                                DefaultTerm="Fråga alltid efter grossist vid fakturaregistrering"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ProductShowOnlyProductNumber"
                                TermID="4584"
                                DefaultTerm="Visa endast artikelnummer i radens artikelnummer fält"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ExtendedInfoOnMergeOrders"
                                TermID="9118"
                                DefaultTerm="Använd utökad information på samlingsfaktura"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseAdditionalDiscount"
                                TermID="9507"
                                DefaultTerm="Använd tilläggsrabatt"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseCustomerCategorieAndArticleGroupsDiscount"
                                TermID="9271"
                                DefaultTerm="Använd kundkategori och artikelgrupp för beräkning av rabatt"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ProductShowProductPictureLink"
                                TermID="4669"
                                DefaultTerm="Visa länk till extern produktinformation"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseExtendedInfoInExternalSearch"
                                TermID="7747"
                                DefaultTerm="Visa kategorier och utökad produktinformation i extern sök"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AutoCreateDateOnProductRows"
                                TermID="7475"
                                DefaultTerm="Automatisk datumsättning på artikelrader"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="CreateSubtotalRowInConsolidatedInvoices"
                                TermID="7886"
                                DefaultTerm="Skapa delsumma vid samfakturering"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ShowImportProductRows"
                                TermID="7376"
                                DefaultTerm="Importera artikelrader"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(5361, "Status")%></legend>
                        <div class="col">
                            <table>
                                <SOE:SelectEntry
                                    ID="StatusForTransferOfferToOrder"
                                    TermID="5370"
                                    DefaultTerm="Status överfört offert till order"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="StatusForTransferOfferToInvoice"
                                    TermID="5371"
                                    DefaultTerm="Status överfört offert till faktura"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="StatusForTransferOrderToInvoice"
                                    TermID="5372"
                                    DefaultTerm="Status överfört order till faktura"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="StatusForTransferOrderToContract"
                                    TermID="7241"
                                    DefaultTerm="Status överfört order till avtal"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="StatusOrderReadyMobile"
                                    TermID="5652"
                                    DefaultTerm="Status klarmarkerad order via mobil"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="StatusOrderDeliverFromStock"
                                    TermID="7409"
                                    DefaultTerm="Status plocka order från lager"
                                    runat="server">
                                </SOE:SelectEntry>
                            </table>
                        </div>
                        <div>
                            <table>
                                <SOE:CheckBoxEntry
                                    ID="HideRowsTransferredToOrderOrInvoiceFromOffer"
                                    TermID="8230"
                                    DefaultTerm="Dölj rader överförda till order/faktura från offert"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="HideRowsTransferredToInvoiceFromOrder"
                                    TermID="8231"
                                    DefaultTerm="Dölj rader överförda till faktura från order"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="HideStatusOrderReadyForMobile"
                                    TermID="7250"
                                    DefaultTerm="Dölj klarmarkerade ordrar i mobilen"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="AskCreateInvoiceWhenOrderReady"
                                    TermID="3280"
                                    DefaultTerm="Fråga om skapa faktura vid klarmarkering av order"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                            </table>
                        </div>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3387, "Utskrift")%></legend>
                        <div class="col">
                            <table>
                                <SOE:SelectEntry
                                    ID="DefaultOfferTemplate"
                                    TermID="8024"
                                    DefaultTerm="Standard offertmall"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultContractTemplate"
                                    TermID="3457"
                                    DefaultTerm="Standard avtalsmall"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultOrderTemplate"
                                    TermID="8023"
                                    DefaultTerm="Standard ordermall"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultWorkingOrderTemplate"
                                    TermID="5751"
                                    DefaultTerm="Standard arbetsordermall"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultInvoiceTemplate"
                                    TermID="1991"
                                    DefaultTerm="Standard fakturamall"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultTimeProjectReportTemplate"
                                    TermID="8011"
                                    DefaultTerm="Standard tidbokmall"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultHouseholdDeductionTemplate"
                                    TermID="7254"
                                    DefaultTerm="Standard skattereduktionsmall"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultExpenseReportTemplate"
                                    TermID="11943"
                                    DefaultTerm="Standard utläggsmall"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultPrintTemplateCashSales"
                                    TermID="7514"
                                    DefaultTerm="Standard utskriftsmall kontantförsäljning"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultTemplatePurchaseOrder"
                                    TermID="7581"
                                    DefaultTerm="Standard beställningsmall"
                                    runat="server">
                                </SOE:SelectEntry>
                            </table>
                        </div>
                        <div>
                            <table>
                                <SOE:NumericEntry
                                    ID="NbrOfOfferCopies"
                                    TermID="8154"
                                    DefaultTerm="Antal offertkopior"
                                    MaxLength="1"
                                    Width="20"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:NumericEntry
                                    ID="NbrOfContractCopies"
                                    TermID="8155"
                                    DefaultTerm="Antal avtalskopior"
                                    MaxLength="1"
                                    Width="20"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:NumericEntry
                                    ID="NbrOfOrderCopies"
                                    TermID="8153"
                                    DefaultTerm="Antal orderkopior"
                                    MaxLength="1"
                                    Width="20"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:NumericEntry
                                    ID="NbrOfInvoiceCopies"
                                    TermID="3388"
                                    DefaultTerm="Antal fakturakopior"
                                    MaxLength="1"
                                    Width="20"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:CheckBoxEntry
                                    ID="PrintTaxBill"
                                    TermID="8037"
                                    DefaultTerm="Godkänd för F-skatt"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="ShowOrdernrOnInvoiceReport"
                                    TermID="8052"
                                    DefaultTerm="Ordernr på faktura"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <!--
                                <SOE:CheckBoxEntry
                                    ID="CCInvoiceMailToSelf" 
                                    TermID="8205"
                                    DefaultTerm="Kopia på faktura till eget företag" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                -->
                                <SOE:CheckBoxEntry
                                    ID="ShowCOLabelOnReport"
                                    TermID="8303"
                                    DefaultTerm="Inkludera C/O rubrik på faktura"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseInvoiceReportDataHistory"
                                    TermID="5693"
                                    DefaultTerm="Använd ursprungligt data på fakturautskrift"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="IncludeWorkingDescriptionOnInvoice"
                                    TermID="7180"
                                    DefaultTerm="Ta med arbetsbeskrivning på faktura"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <%--TODO--%>
                                <SOE:CheckBoxEntry
                                    ID="PrintCheckListWithOrder"
                                    TermID="9139"
                                    DefaultTerm="Skriv alltid ut checklista med order"
                                    Visible="false"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="IncludeRemainingAmountOnInvoice"
                                    TermID="7212"
                                    DefaultTerm="Visa kvar att fakturera på fakturautskrift vid ej lyftfaktura"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <!--<SOE:CheckBoxEntry
                                    ID="BillingIncludeTimeProjectinReport" 
                                    TermID="2297"
                                    DefaultTerm="Inkludera tidbok i samma rapport som order (ej som bilaga)" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="BillingOrderIncludeTimeProjectinReport" 
                                    TermID="2298"
                                    DefaultTerm="Inkludera tidbok i samma rapport som faktura (ej som bilaga)" 
                                    runat="server">
                                </SOE:CheckBoxEntry>-->
                                <SOE:CheckBoxEntry
                                    ID="BillingShowStartStopInTimeReport"
                                    TermID="7444"
                                    DefaultTerm="Visa start- och stopptid på tidbok"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="BillingShowPurchaseDateOnInvoice"
                                    TermID="7582"
                                    DefaultTerm="Best.datum på faktura endast om angivet på order"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                            </table>
                        </div>
                    </fieldset>
                </div>
                <div id="DivEmail" runat="server">
                    <fieldset>
                        <legend><%=GetText(4142, "E-postmallar")%></legend>
                        <div class="col">
                            <table>
                                <SOE:SelectEntry
                                    ID="DefaultEmailTemplate"
                                    TermID="4530"
                                    DefaultTerm="Standard e-postmall"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultEmailTemplateOffer"
                                    TermID="7721"
                                    DefaultTerm="Standard e-postmall offert"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultEmailTemplateOrder"
                                    TermID="7722"
                                    DefaultTerm="Standard e-postmall order"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultEmailTemplateContract"
                                    TermID="7723"
                                    DefaultTerm="Standard e-postmall avtal"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="DefaultEmailTemplateCashSales"
                                    TermID="7515"
                                    DefaultTerm="Standard e-postmall kontantförsäljning"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:TextEntry
                                    ID="CCInvoiceMailAddress"
                                    TermID="7554"
                                    DefaultTerm="E-post fakturakopia (CC)"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:SelectEntry
                                    ID="DefaultEmailTemplatePurchase"
                                    TermID="7661"
                                    DefaultTerm="Standard e-postmall beställning"
                                    runat="server">
                                </SOE:SelectEntry>
                                <!--
                                <SOE:TextEntry
                                    ID="BCCInvoiceMailAddress"
                                    TermID="7555" DefaultTerm="E-post fakturakopia (BCC)"
                                    MaxLength="30"
                                    runat="server">
                                </SOE:TextEntry>
                                -->
                            </table>
                        </div>
                    </fieldset>
                </div>
                <div id="DivEInvoice" runat="server">
                    <fieldset>
                        <legend><%=GetText(4559, "E-faktura")%></legend>
                        <div class="col">
                            <table>
                                <%--<SOE:SelectEntry
                                    ID="EInvoiceDistributor" 
                                    TermID="4716"
                                    DefaultTerm="E-Fakturadistributör" 
                                    runat="server">
                                </SOE:SelectEntry>--%>
                                <SOE:SelectEntry
                                    ID="EInvoiceFormat"
                                    TermID="4717"
                                    DefaultTerm="E-Fakturaformat"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:CheckBoxEntry
                                    ID="SveFakturaToFile"
                                    TermID="4785"
                                    DefaultTerm="Svefaktura - skapa till fil"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="SveFakturaToAPITestMode"
                                    TermID="2184"
                                    DefaultTerm="InExchange - Testläge API"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="BillingHideArticleNrOnSvefaktura"
                                    TermID="7249"
                                    DefaultTerm="Dölj artikelnummer på Svefaktura"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="BillingUseInvoiceDeliveryProvider"
                                    TermID="7754"
                                    DefaultTerm="Använd fakturadistributör"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="BillingUseInExchangeDeliveryProvider"
                                    TermID="7775"
                                    DefaultTerm="Använd InExchange som fakturadistributör för samtliga fakturor"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:InstructionList ID="BillingUseInExchangeDeliveryProviderInstruction"
                                    runat="server">
                                </SOE:InstructionList>
                            </table>
                        </div>
                        <div>
                            <table>
                                <SOE:CheckBoxEntry
                                    ID="FinvoiceSingleInvoicePerFile"
                                    TermID="7659"
                                    DefaultTerm="Varje Finvoice faktura i sin egen fil"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="FinvoiceInvoiceLabelToOrderIdentifier"
                                    TermID="7740"
                                    DefaultTerm="Lägg Märkning i fältet för OrderIdentifier"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                            </table>
                        </div>
                    </fieldset>
                </div>
            </SOE:Tab>
            <SOE:Tab Type="Setting" TermID="5406" DefaultTerm="Inställningar kontering" runat="server">
                <div>
                    <fieldset>
                        <legend><%=GetText(3375, "Prioriteringsordning för konteringar")%></legend>
                        <fieldset>
                            <legend><%=GetText(3369, "Artikelkonteringar")%></legend>
                            <table>
                                <SOE:SelectEntry
                                    ID="InvoiceProductAccountingPrio1"
                                    TermID="3370"
                                    DefaultTerm="Första"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="InvoiceProductAccountingPrio2"
                                    TermID="3371"
                                    DefaultTerm="Andra"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="InvoiceProductAccountingPrio3"
                                    TermID="3372"
                                    DefaultTerm="Tredje"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="InvoiceProductAccountingPrio4"
                                    TermID="3373"
                                    DefaultTerm="Fjärde"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="InvoiceProductAccountingPrio5"
                                    TermID="3374"
                                    DefaultTerm="Femte"
                                    runat="server">
                                </SOE:SelectEntry>
                            </table>
                        </fieldset>
                    </fieldset>
                </div>
            </SOE:Tab>
        </tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
