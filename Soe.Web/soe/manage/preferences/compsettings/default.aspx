<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.preferences.compsettings.Default" Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
            <SOE:Tab Type="Setting" TermID="5415" DefaultTerm="Företagsinställningar" runat="server">
                <div>
                    <fieldset>
                        <legend><%=GetText(3022, "Generella företagsinställningar")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="DefaultRole"
                                TermID="5561" DefaultTerm="Standardroll"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:NumericEntry
                                ID="CleanReportPrintoutAfterNrOfDays"
                                TermID="11787"
                                DefaultTerm="Rensa utskrivna rapporter efter antal dagar"
                                MaxLength="3"
                                AllowDecimals="false"
                                Width="20"
                                runat="server">
                            </SOE:NumericEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(11789, "Ekonomisk tillhörighet")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="UseAccountsHierarchy"
                                TermID="11776"  
                                DefaultTerm="Använd ekonomisk tillhörighet"
                                OnClick="useAccountsHierarchyClicked()"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <tr>
                                <td>
                                    <SOE:InstructionList
                                    ID="UseAccountHierarchyInstruction"
                                    runat="server">
                                    </SOE:InstructionList>
                                </td>
                            </tr>
                            <SOE:SelectEntry
                                ID="DefaultEmployeeAccountDimEmployee"
                                TermID="11777"
                                DefaultTerm="Standard konteringsnivå vid upplägg av anställd"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="UseLimitedEmployeeAccountDimLevels"
                                TermID="94046"
                                DefaultTerm="Använd ingen undernivå vid upplägg av anställd"
                                OnClick="useLimitedEmployeeAccountDimLevelsClicked()"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseExtendedEmployeeAccountDimLevels"
                                TermID="12154"
                                DefaultTerm="Använd en extra undernivå vid upplägg av anställd"
                                OnClick="useExtendedEmployeeAccountDimLevelsClicked()"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:SelectEntry
                                ID="DefaultEmployeeAccountDimSelector"
                                TermID="11782"
                                DefaultTerm="Lägsta konteringsnivå i väljare"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="FallbackOnEmployeeAccountInPrio"
                                TermID="12197"
                                DefaultTerm="Fall tillbaka på anställds tillhörighet om annan ej är satt vid vid konteringsprio"
                                runat="server">
                            </SOE:CheckBoxEntry>                            
                            <SOE:CheckBoxEntry
                                ID="BaseSelectableAccountsOnEmployeeInsteadOfAttestRole"
                                TermID="12198"
                                DefaultTerm="Basera valbara konton på anställds tillhörighet istället för aktuell användares attestroll"
                                runat="server">
                            </SOE:CheckBoxEntry>                            
                            <SOE:CheckBoxEntry
                                ID="SendReminderToExecutivesBasedOnEmployeeAccountOnly"
                                TermID="12199"
                                DefaultTerm="Skicka påminnelse till chefer baserat endast på anställds tillhörighet"
                                runat="server">
                             </SOE:CheckBoxEntry>                            
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(11788, "Inloggning")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="DoNotAddToSoftOneIdDirectlyOnSave"
                                TermID="11780"
                                DefaultTerm="Skicka inte inloggningsinformation till användaren direkt vid upplägg av och sparning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:NumericEntry
                                ID="BlockFromDateOnUserAfterNrOfDays"
                                TermID="11832"
                                DefaultTerm="Blockera användaren X antal dagar anställning upphört"
                                MaxLength="3"
                                AllowDecimals="false"
                                Width="20"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:CheckBoxEntry
                                ID="MandatoryContactInformation"
                                TermID="11770"
                                DefaultTerm="Tvingande kontaktuppgifter vid inloggning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AllowSupportLogin"
                                TermID="9291"
                                DefaultTerm="Tillåt supportinloggning"
                                OnClick="syncAllowSupportLoginChanged()"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:DateEntry
                                ID="SupportLoginTo"
                                TermID="9292"
                                DefaultTerm="Tillåt supportinloggning tom"
                                runat="server">
                            </SOE:DateEntry>
                            <SOE:TextEntry
                                ID="SupportLoginTimeTo"
                                TermID="9293"
                                DefaultTerm="Fram till klockan"
                                InfoText="HH:MM"
                                OnChange="formatTime(this);"
                                OnFocus="this.select();"
                                runat="server">
                            </SOE:TextEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3786, "Lösenord")%></legend>
                        <table>
                            <SOE:NumericEntry
                                ID="PasswordMinLength"
                                TermID="3787"
                                DefaultTerm="Min antal tecken"
                                MaxLength="2"
                                AllowDecimals="false"
                                Width="20"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:NumericEntry
                                ID="PasswordMaxLength"
                                TermID="3788"
                                DefaultTerm="Max antal tecken"
                                MaxLength="2"
                                AllowDecimals="false"
                                Width="20"
                                runat="server">
                            </SOE:NumericEntry>
                        </table>
                        <table>
                            <SOE:Instruction
                                ID="PasswordLengthInstruction"
                                runat="server">
                            </SOE:Instruction>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(5644, "Samtidig redigering av poster")%></legend>
                        <table>
                            <SOE:InstructionList
                                ID="EntityHistoryInstruction"
                                runat="server">
                            </SOE:InstructionList>
                            <SOE:NumericEntry
                                ID="EntityHistoryInterval"
                                TermID="5647"
                                DefaultTerm="Antal minuter bakåt"
                                MaxLength="3"
                                AllowDecimals="false"
                                Width="20"
                                runat="server">
                            </SOE:NumericEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(7067, "Avsändande e-postadress")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="UseDefaultEmailAddress"
                                TermID="7068"
                                DefaultTerm="Använd som standard"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:TextEntry
                                ID="DefaultEmailAddress"
                                TermID="7067"
                                DefaultTerm="Avsändande e-postadress"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:InstructionList
                                ID="EmailInstructions"
                                runat="server">
                            </SOE:InstructionList>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(6422, "Inkommande e-post")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="DisableMessageOnInboundEmailError"
                                TermID="6421"
                                DefaultTerm="Inaktivera e-postavisering vid misslyckad mottagning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(8281, "Externa länkar")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="ActivateExternalLinks"
                                TermID="8277"
                                DefaultTerm="Aktivera externa länkar"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:TextEntry
                                ID="ExternalLink1"
                                TermID="8278"
                                DefaultTerm="Länk 1"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="ExternalLink2"
                                TermID="8279"
                                DefaultTerm="Länk 2"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="ExternalLink3"
                                TermID="8280"
                                DefaultTerm="Länk 3"
                                runat="server">
                            </SOE:TextEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3572, "Översiktspanel")%></legend>
                        <table>
                            <SOE:NumericEntry
                                ID="DashboardRefreshInterval"
                                TermID="3882"
                                DefaultTerm="Omladdningsintervall (minuter)"
                                Width="50"
                                MaxLength="3"
                                AllowDecimals="false"
                                AllowNegative="false"
                                runat="server">
                            </SOE:NumericEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(7096, "Modulinställningar Personal")%></legend>
                        <table>
                            <SOE:TextEntry
                                ID="PersonellModuleHeader"
                                TermID="7097"
                                DefaultTerm="Modulnamn"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:SelectEntry
                                ID="ModuleIconImage"
                                TermID="7098" DefaultTerm="Modulikon"
                                runat="server">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3941, "Ärenden")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="CaseProjectAttestStateReceived"
                                TermID="3942" DefaultTerm="Attestnivå för mottaget"
                                runat="server">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(4719, "E-fakturainställningar för InExchange")%></legend>
                        <div>
                            <table>
                                <SOE:Instruction
                                    ID="FtpInstructionProduction"
                                    TermID="4723"
                                    runat="server">
                                </SOE:Instruction>
                                <SOE:TextEntry
                                    ID="InExchangeAddress"
                                    TermID="4720"
                                    DefaultTerm="FTP-adress"
                                    Width="350"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry
                                    ID="InExchangeUser"
                                    TermID="4721"
                                    DefaultTerm="Användarnamn"
                                    Width="150"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry
                                    ID="InExchangePasswd"
                                    TermID="4722"
                                    DefaultTerm="Lösenord"
                                    Width="150"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:Instruction
                                    ID="FtpInstructionTest"
                                    TermID="4724"
                                    runat="server">
                                </SOE:Instruction>
                                <SOE:TextEntry
                                    ID="InExchangeAddressTest"
                                    TermID="4720"
                                    DefaultTerm="FTP-adress"
                                    Width="350"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry
                                    ID="InExchangeUserTest"
                                    TermID="4721"
                                    DefaultTerm="Användarnamn"
                                    Width="150"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry
                                    ID="InExchangePasswdTest"
                                    TermID="4722"
                                    DefaultTerm="Lösenord"
                                    Width="150"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:Instruction
                                    ID="APIInstruction"
                                    TermID="4670"
                                    runat="server">
                                </SOE:Instruction>
                                <SOE:Instruction
                                    runat="server">
                                </SOE:Instruction>
                                <asp:Button
                                    ID="RegisterAPI"
                                    Text="Registrera API"
                                    UseSubmitBehavior="true"
                                    OnClick="ButtonRegister_Click"
                                    runat="server"></asp:Button>
                                <asp:Button
                                    ID="ActivateAPI"
                                    Text="Aktivera API"
                                    UseSubmitBehavior="true"
                                    OnClick="ButtonActivate_Click"
                                    runat="server"></asp:Button>
                                <SOE:CheckBoxEntry
                                    ID="InExchangeAPISendRegistered"
                                    TermID="7431"
                                    DefaultTerm="Skicka e-faktura"
                                    OnClick="inexchangeChanged()"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="InExchangeAPIReciveRegistered"
                                    TermID="7432"
                                    DefaultTerm="Ta emot e-faktura"
                                    OnClick="inexchangeChanged()"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:TextEntry
                                    ID="InexchangedRegisterDate"
                                    TermID="5493"
                                    DefaultTerm="Ändrad"
                                    Width="120"
                                    DisableSettings="true"
                                    ReadOnly="true"
                                    runat="server">
                                </SOE:TextEntry>
                            </table>
                        </div>

                    </fieldset>
                </div>
                <div ID="IntrumSettings" runat="server">
                    <fieldset>
                        <legend><%=GetText(7619, "Intrum inställningar")%></legend>
                        <table>
                            <SOE:TextEntry
                                ID="IntrumClientNo"
                                TermID="7620"
                                DefaultTerm="Intrum klientnummer"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="IntrumHubNo"
                                TermID="7621"
                                DefaultTerm="Intrum hubnummer"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="IntrumLedgerNo"
                                TermID="7622"
                                DefaultTerm="Intrum reskontranummer"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="IntrumUser"
                                TermID="9"
                                DefaultTerm="Användare"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="IntrumPwd"
                                TermID="3"
                                DefaultTerm="Lösenord"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:CheckBoxEntry
                                ID="IntrumTestMode"
                                TermID="7624"
                                DefaultTerm="Intrum - Testläge"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
                <div ID="ZetesSettings" runat="server">
                    <fieldset>
                        <legend><%=GetText(7706, "Zetes inställningar")%></legend>
                        <table>
                            <SOE:TextEntry
                                ID="ZetesClientCode"
                                TermID="7704"
                                DefaultTerm="Klientnummer"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="ZetesStakeholderCode"
                                TermID="7705"
                                DefaultTerm="Intressentnummer"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="ZetesUser"
                                TermID="9"
                                DefaultTerm="Användare"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="ZetesPwd"
                                TermID="3"
                                DefaultTerm="Lösenord"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:CheckBoxEntry
                                ID="ZetesTestMode"
                                TermID="7624"
                                DefaultTerm="Zetes - Testläge"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                     </fieldset>
                </div>
                <div id="FortnoxSettings" runat="server">
                    <fieldset>
                        <legend><%=GetText(9374, "Fortnox inställningar")%></legend>
                        <table>
                            <SOE:Instruction
                                ID="FortnoxInstruction"
                                runat="server">
                            </SOE:Instruction>
                        </table>
                        <table>
                            <a ID="FortnoxActivationUrl" runat="server" href="/"><%=GetText(4672, "Aktivera")%></a>
                        </table>
                        <table>
                            <button
                                ID="FortnoxDeactivate"
                                Name="FortnoxDeactivate"
                                runat="server"><%=GetText(9377, "Avaktivera")%></button>
                        </table>
                    </fieldset>
                </div>
                <div id="VismaEAccountingSettings" runat="server">
                    <fieldset>
                        <legend><%=GetText(9378, "Visma eEkonomi inställningar")%></legend>
                        <table>
                            <SOE:Instruction
                                ID="VismaEAccountingInstruction"
                                runat="server">
                            </SOE:Instruction>
                        </table>
                        <table>
                            <a ID="VismaEAccountingActivationUrl" runat="server" href="/"><%=GetText(4672, "Aktivera")%></a>
                        </table>
                        <table>
                            <button
                                ID="VismaEAccountingDeactivate"
                                Name="VismaEAccountingDeactivate"
                                runat="server"><%=GetText(9377, "Avaktivera")%></button>
                        </table>
                    </fieldset>
                </div>
                <div id="AzoraOneSettings" runat="server">
                    <fieldset>
                        <legend><%=GetText(9380, "Tolkning av dokument")%></legend>
                        <table>
                            <SOE:Instruction
                                ID="AzoraOneInstruction"
                                runat="server">
                            </SOE:Instruction>
                        </table>
                        <table>
                            <button
                                ID="AzoraOneActivate"
                                Name="AzoraOneActivate"
                                runat="server"><%=GetText(4672, "Aktivera")%></button>
                        </table>
                        <table>
                            <button
                                ID="AzoraOneSendTrainingData"
                                Name="AzoraOneSendTrainingData"
                                runat="server"><%=GetText(9381, "Skicka lärdata")%></button>
                        </table>
                        <table>
                            <button
                                ID="AzoraOneDeactivate"
                                Name="AzoraOneDeactivate"
                                runat="server"><%=GetText(9377, "Avaktivera")%></button>
                        </table>
                    </fieldset>
                </div>
                <div id="Finvoice" runat="server">
                    <fieldset>
                        <legend><%=GetText(7669, "Finvoice inställningar")%></legend>
                            <table>
                                <SOE:TextEntry
                                    ID="FinvoiceAddress"
                                    TermID="4718" DefaultTerm="Finvoice adress"
                                    MaxLength="30"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:TextEntry
                                    ID="FinvoiceOperator"
                                    TermID="4561" DefaultTerm="Finvoice operatör"
                                    MaxLength="30"
                                    runat="server">
                                </SOE:TextEntry>
                                <SOE:CheckBoxEntry
                                    ID="FinvoiceUseBankIntegration" 
                                    TermID="7670"
                                    DefaultTerm="Används av bankintegration" 
                                    runat="server">
                                </SOE:CheckBoxEntry>
                            </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(4733, "Använd attestpåminnelse")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="SupplierInvoiceAutoReminder"
                                TermID="4734"
                                DefaultTerm="Använd som standard"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend>Kivra</legend>
                        <table>
                            <SOE:TextEntry
                                ID="KivraTenentKey"
                                TermID="9999"
                                DefaultTerm="Apinyckel"
                                runat="server">
                            </SOE:TextEntry>
                        </table>
                    </fieldset>
                </div>
                <div ID="ChainSettings" runat="server">
                    <fieldset>
                        <legend><%=GetText(7751, "Kedjetillhörighet")%></legend>
                            <table>
                            <SOE:SelectEntry
                                ID="ChainAffiliation" 
                                TermID="7751"
                                DefaultTerm="Kedjetillhörighet" 
                                runat="server">
                            </SOE:SelectEntry>
                            </table>
                        </fieldset>
                </div>
            </SOE:Tab>
        </tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

