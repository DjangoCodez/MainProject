<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.contracts.edit._default" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Form ID="Form1" EnableDelete="true" EnableCopy="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
				<div>
				    <SOE:InstructionList ID="LicenseInstructions" runat="server"></SOE:InstructionList>
				</div>
				<div>
					<fieldset> 
						<legend><%=GetText(2021, "Licensuppgifter")%></legend>
						<div class="col">
                            <table>
							    <SOE:NumericEntry 
							        ID="LicenseNr" 
							        TermID="2022" DefaultTerm="License nr" 
							        Validation="Required"
							        InvalidAlertTermID="1234" InvalidAlertDefaultTerm="Du måste ange licensnummer"
							        MaxLength="50" 
							        runat="server">
							    </SOE:NumericEntry>
							    <SOE:TextEntry 
							        ID="Name" 
							        TermID="2134" DefaultTerm="Företagsnamn" 
							        InvalidAlertTermID="1235" InvalidAlertDefaultTerm="Du måste ange företagsnamn"
							        Validation="Required" 
							        MaxLength="100" 
							        runat="server">
							    </SOE:TextEntry>
                                <SOE:TextEntry 
							        ID="LegalName" 
							        TermID="9151" DefaultTerm="Juridiskt namn" 
							        MaxLength="100" 
							        runat="server">
							    </SOE:TextEntry>
							    <SOE:TextEntry 
							        ID="OrgNr" 
							        TermID="2135" DefaultTerm="Org nr" 
							        Validation="Required" 
							        InvalidAlertTermID="1236" InvalidAlertDefaultTerm="Du måste ange orgnummer"
							        MaxLength="50" 
							        runat="server">
							    </SOE:TextEntry>
							    <SOE:NumericEntry 
							        ID="NrOfCompanies" 
							        TermID="2025" DefaultTerm="Antal företag" 
							        Validation="Required" 
							        InvalidAlertTermID="1239" InvalidAlertDefaultTerm="Du måste ange antal företag"
							        MaxLength="10" 
							        runat="server">
							    </SOE:NumericEntry>
    <%--                            <SOE:CheckBoxEntry 
                                    ID="Support"
							        TermID="3029" DefaultTerm="Supportlicens" 
                                    Value="false"
                                    runat="server">
                                </SOE:CheckBoxEntry>--%>
					            <SOE:DateEntry runat="server"
					                ID="TerminationDate"
					                TermID="5518" DefaultTerm="Slutdatum"
					                MaxLength="100">
				                </SOE:DateEntry>
					            <SOE:SelectEntry 
					                ID="BaseCurrency" 
					                TermID="4160" 
					                DefaultTerm="Basvaluta"
					                runat="server" >
					            </SOE:SelectEntry>
                            </table>
                        </div>
                        <div>
                            <table>
							    <SOE:NumericEntry 
							        ID="ConcurrentUsers" 
							        TermID="2023" DefaultTerm="Samtidiga användare" 
							        Validation="Required" 
							        InvalidAlertTermID="1237" InvalidAlertDefaultTerm="Du måste ange samtidiga användare"
							        MaxLength="10" 
							        runat="server">
							    </SOE:NumericEntry>
							    <SOE:NumericEntry 
							        ID="MaxNrOfUsers" 
							        TermID="2024" DefaultTerm="Max antal användare" 
							        Validation="Required" 
							        InvalidAlertTermID="1238" InvalidAlertDefaultTerm="Du måste ange max antal användare"
							        MaxLength="10" 
							        runat="server">
							    </SOE:NumericEntry>
							    <SOE:NumericEntry 
							        ID="MaxNrOfEmployees" 
							        TermID="5193" DefaultTerm="Max antal anställda" 
							        Validation="Required" 
							        InvalidAlertTermID="5194" InvalidAlertDefaultTerm="Du måste ange max antal anställda"
							        MaxLength="10" 
							        runat="server">
							    </SOE:NumericEntry>
							    <SOE:NumericEntry 
							        ID="MaxNrOfMobileUsers" 
							        TermID="3814" DefaultTerm="Max antal mobilanvändare" 
							        Validation="Required" 
							        InvalidAlertTermID="3815" InvalidAlertDefaultTerm="Du måste ange max antal mobilanvändare"
							        MaxLength="10" 
							        runat="server">
							    </SOE:NumericEntry>
                                <SOE:SelectEntry
                                    ID="SysServer"
                                    TermID="5125" DefaultTerm="Server"
                                    runat="server">
                                </SOE:SelectEntry>
                            </table>
                        </div>
					</fieldset>
				</div>	
			</SOE:Tab>	
            <SOE:Tab Type="Setting" TermID="1998" DefaultTerm="Artiklar och behörigheter"  runat="server">			
                <table>
	                <SOE:SelectEntry 
                        ID="TemplateLicense" 
                        TermID="1689" DefaultTerm="Kopiera behörigheter från licens"
                        Visible="false"
                        runat="server" >
                    </SOE:SelectEntry>                    
                </table>
                <div>
					<fieldset> 
						<legend><%=GetText(5001, "SoftOne Artiklar")%></legend>
                        <table>
                            <SOE:CheckBoxEntry 
                                ID="ModifySysXEArticles" 
                                TermID="5055" 
                                DefaultTerm="Uppdatera SoftOne Artiklar"                                
                                OnClick="modifyArticlesChecked()"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry 
                                ID="DeleteSysXEArticles" 
                                TermID="5968" 
                                DefaultTerm="Återställ behörigheter enligt SoftOne Artiklar"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:Instruction
                                ID="CannotModifyOwnLicenseWarning"
                                TermID="5056" 
                                DefaultTerm="Kan inte ändra SoftOne Artiklar på den nu inloggade licensen"
                                runat="server">
                            </SOE:Instruction>
                            <tr>
                                <td>
                                    &nbsp;
                                </td>
                            </tr>
                        </table>
                        <table id="SysXEArticleTable" runat="server">
                        </table>
                    </fieldset>
                </div>
            </SOE:Tab>
		</Tabs>
	</SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
