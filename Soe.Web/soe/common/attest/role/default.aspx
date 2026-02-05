<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.attest.role._default" %>
<%@ Register Src="~/UserControls/CompanyCategories.ascx" TagPrefix="SOE" TagName="CompanyCategories" %>
<%@ Register Src="~/UserControls/AttestTransition.ascx" TagPrefix="SOE" TagName="AttestTransition" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
        <tabs>
			<SOE:Tab Type="Edit" runat="server">
		        <div>
				    <fieldset> 
					    <legend><%=GetText(5223, "Attestroll")%></legend>
			            <table>
				            <SOE:TextEntry
				                runat="server"  
					            ID="Name"
					            Validation="Required"
					            TermID="5224" DefaultTerm="Namn"
					            InvalidAlertTermID="44" InvalidAlertDefaultTerm="Du måste ange namn"
					            MaxLength="100">
				            </SOE:TextEntry>	
				            <SOE:TextEntry 
				                runat="server"
					            ID="Description"
					            TermID="5225" DefaultTerm="Beskrivning"
					            MaxLength="100">
				            </SOE:TextEntry>
                            <SOE:TextEntry 
						        ID="ExternalCodes"
						        TermID="11842" DefaultTerm="Externa koder (vid flera separera med #)"
						        MaxLength="100"
						        runat="server">
					         </SOE:TextEntry>
				            <SOE:NumericEntry 
				                runat="server"	
				                ID="DefaultMaxAmount"
					            TermID="5226" DefaultTerm="Maxbelopp"
                                MaxLength="8">  
				            </SOE:NumericEntry>
			            </table>
			        </fieldset>
		        </div>
                <div id="DivTransitions" runat="server">
                    <SOE:AttestTransition ID="AttestTransitions" Runat="Server"></SOE:AttestTransition>
                </div>
                <div id="DivVisibleAttestStates" runat="server" visible="false">
				    <fieldset> 
					    <legend><%=GetText(7426, "Synliga attestnivåer")%></legend>
                          <div class="col">
                              <table>
                                 <SOE:Instruction
                                    ID="VisibleAttestStatesInstruction"
                                    TermID="7427" DefaultTerm="Endast order med valda attestnivåer kommer vara synliga för rollen. Om ingen nivå är vald så är inställningen inaktiverad"
                                    runat="server">
                                </SOE:Instruction>
                              </table>
                            <table>
                            <SOE:FormIntervalEntry
                                ID="VisibleAttestStates" 
                                OnlyFrom="true"
                                DisableHeader="true"
                                DisableSettings="true"
                                EnableDelete="true"
                                ContentType="2"
                                LabelType="1"
                                FromWidth="100"
                                HideLabel="true"
                                NoOfIntervals="20"
                                FitInTable="true"
                                runat="server">
                            </SOE:FormIntervalEntry>
                        </table>
                       </div>
			        </fieldset>
		        </div>
                <div id="DivAttestReminders" runat="server">
				    <fieldset> 
					    <legend><%=GetText(7157, "Inställningar för påminnelse om klarmarkering")%></legend>
                        <SOE:SelectEntry
                                ID="AttestStateSelectEntry"
                                TermID="7158"
                                DefaultTerm="Attestnivå"
                                Width="160"
                                runat="server"
                                FitInTable="true">
                            </SOE:SelectEntry>
					    <SOE:TextEntry					            
						        ID="NoOfDaysTextEntry"
						        TermID="7159"
                                DefaultTerm="antal dagar"
                                Width="40"
                                runat="server"  
						        MaxLength="100"
                                FitInTable="true">
					        </SOE:TextEntry>
                            <SOE:SelectEntry
                                ID="PeriodSelectEntry"
                                TermID="7160"
                                DefaultTerm="efter att"
                                Width="160"
                                runat="server"
                                FitInTable="true">
                            </SOE:SelectEntry>
                        <SOE:Text ID="Passed" TermID="7161" DefaultTerm="har passerats." runat="server" FitInTable="true">
                        </SOE:Text>	
			        </fieldset>
		        </div>
			</SOE:Tab>
			<SOE:Tab ID="TabSettings" Type="Setting" runat="server">
	            <div id="DivCategories" runat="server">
                    <fieldset>
                        <legend><%=GetText(5412, "Generella inställningar") %></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="ShowUncategorized"                                
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ShowAllCategories"                                
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <%if (!UseAccountHierarchy())
                            {%>
                            <SOE:CheckBoxEntry
                                ID="ShowAllSecondaryCategories" 
                                TermID="8563"
                                DefaultTerm="Visa alla sekundära kategorier" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                             <%}%>
                            <SOE:CheckBoxEntry
                                ID="ShowTemplateSchedule" 
                                TermID="5758"
                                DefaultTerm="Visa grundschema i attestvy" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AlsoAttestAdditionsFromTime" 
                                TermID="91943"
                                DefaultTerm="Attestera även resa/utlägg från Attestera tid" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                             <%if (UseAccountHierarchy())
                            {%>
                            <SOE:CheckBoxEntry
                                ID="IsExecutive" 
                                TermID="8562"
                                DefaultTerm="Meddelandeavisering" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AttestByEmployeeAccount" 
                                TermID="9927"
                                DefaultTerm="Tillåt attestering av anställda baserat på tillhörighet" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="StaffingByEmployeeAccount" 
                                TermID="9928"
                                DefaultTerm="Tillåt planering av anställda baserat på tillhörighet" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                             <%}%>
                            <SOE:CheckBoxEntry
                                ID="HumanResourcesPrivacy" 
                                TermID="8615"
                                DefaultTerm="Sekretess HR" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                    <%if (!UseAccountHierarchy())
                    {%>
                    <fieldset>
                        <legend><%=GetText(5013, "Kategorier") %></legend>
                        <div><SOE:CompanyCategories ID="CompanyCategoriesPrimary" Runat="Server"></SOE:CompanyCategories></div> 
                        <div><SOE:CompanyCategories ID="CompanyCategoriesSecondary" Runat="Server"></SOE:CompanyCategories></div>
                    </fieldset>
                    <%} else
                    {%>
                    <%}%>
	            </div>
 	        </SOE:Tab>
		</tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
