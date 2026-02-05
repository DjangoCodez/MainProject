<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.preferences.systeminfosettings.Default" Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="7089" DefaultTerm="Systeminfoinställningar" runat="server">
				 <div>
					<fieldset>
						<legend><%=GetText(5002, "Personal")%></legend>
                        <fieldset style="border:hidden; margin:0px;">
                            <div class="col">
                                <fieldset> 
					            <legend><%=GetText(7092, "Utgående kompetenser")%></legend>
					            <table>
                                    <SOE:CheckBoxEntry 
                                        ID="UseSkill"
                                        TermID="7090"
                                        DefaultTerm="Använd" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:NumericEntry
                                        ID="SkillDaysInAdvance" 
                                        TermID="7091"
                                        DefaultTerm="Dagar i förväg" 
                                        MaxLength="3"
                                        AllowDecimals="false"
                                        Width="20"
                                        runat="server">
                                    </SOE:NumericEntry>
					            </table>
                            </fieldset>
                            </div>
                            <div class="col">
                                <fieldset> 
					                <legend><%=GetText(7093, "Utgående aktiverade scheman")%></legend>
					                <table>
                                        <SOE:CheckBoxEntry 
                                            ID="UsePlacement"
                                            TermID="7090"
                                            DefaultTerm="Använd" 
                                            runat="server">
                                        </SOE:CheckBoxEntry>
                                        <SOE:NumericEntry
                                            ID="PlacementDaysInAdvanced" 
                                            TermID="7091"
                                            DefaultTerm="Dagar i förväg" 
                                            MaxLength="3"
                                            AllowDecimals="false"
                                            Width="20"
                                            runat="server">  
                                        </SOE:NumericEntry>
					                </table>
                                </fieldset>
                            </div>
                            <div class="col">
                                <fieldset> 
					            <legend><%=GetText(9088, "Nära Preliminära pass")%></legend>
					            <table>
                                    <SOE:CheckBoxEntry 
                                        ID="UseClosePreliminaryTimeScheduleTemplateBlocks"
                                        TermID="9086"
                                        DefaultTerm="Använd" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:NumericEntry
                                        ID="PreliminaryTimeScheduleTemplateBlocksDaysInAdvanced" 
                                        TermID="9087"
                                        DefaultTerm="Dagar i förväg" 
                                        MaxLength="3"
                                        AllowDecimals="false"
                                        Width="20"
                                        runat="server">
                                    </SOE:NumericEntry>
					            </table>
                            </fieldset>
                            </div>
                            <div class="col">
                                <fieldset> 
					                <legend><%=GetText(7156, "Påminnelse klarmarkering")%></legend>
					                <table>
                                        <SOE:CheckBoxEntry 
                                            ID="UseAttestReminder"
                                            TermID="9086"
                                            DefaultTerm="Använd" 
                                            runat="server">
                                        </SOE:CheckBoxEntry>
					                </table>
                                </fieldset>
                            </div>
                            <div class="col">
                                <fieldset> 
					                <legend><%=GetText(9289, "Påminn om sjukintyg")%></legend>
					                <table>
                                        <SOE:CheckBoxEntry 
                                            ID="UseIllnessReminder"
                                            TermID="91903"
                                            DefaultTerm="Använd" 
                                            runat="server">
                                        </SOE:CheckBoxEntry>
                                        <SOE:NumericEntry
                                          ID="ReminderDaysAfterIlnessStarted" 
                                          TermID="9288"    
                                          DefaultTerm="Dagar efter sjukfallets början" 
                                          MaxLength="3"
                                          AllowDecimals="false"
                                          Width="20"
                                          runat="server">
                                       </SOE:NumericEntry>
					                </table>
                                </fieldset>
                            </div>
                            <div class="col">
                                <fieldset> 
					                <legend><%=GetText(9287, "Påminn om anmälan till Försäkringskassan")%></legend>
					                <table>
                                       <SOE:CheckBoxEntry 
                                          ID="UseIllnessReminderSIA"
                                            TermID="7090"
                                            DefaultTerm="Använd" 
                                          runat="server">
                                       </SOE:CheckBoxEntry>
                                       <SOE:NumericEntry
                                          ID="ReminderDaysAfterIlnessStartedSIA" 
                                          TermID="9288"
                                          DefaultTerm="Dagar efter sjukfallets början" 
                                          MaxLength="3"
                                          AllowDecimals="false"
                                          Width="20"
                                          runat="server">
                                       </SOE:NumericEntry>
                                       <SOE:TextEntry 
							                ID="ReminderIllnessEmailSocialInsuranceAgency" 
							                TermID="4127" DefaultTerm="E-post"        							                
							                runat="server">
							            </SOE:TextEntry>
					                </table>
                                </fieldset>
                            </div>
                            <div class="col">
                                <fieldset> 
					                <legend><%=GetText(10039, "Påminn om anställningar som närmar sig slutdatum")%></legend>
					                <table>
                                       <SOE:CheckBoxEntry 
                                          ID="UseEmploymentReminder"
                                          TermID="7090"
                                          DefaultTerm="Använd" 
                                          runat="server">
                                       </SOE:CheckBoxEntry>
                                       <SOE:NumericEntry
                                          ID="ReminderDaysBeforeEmploymentEnds" 
                                          TermID="10040"
                                          DefaultTerm="Dagar före anställningen slutar" 
                                          MaxLength="3"
                                          AllowDecimals="false"
                                          Width="20"
                                          runat="server">
                                       </SOE:NumericEntry>
					                </table>
                                </fieldset>
                            </div>
                            <div class="col">
                                <fieldset> 
					                <legend><%=GetText(9318, "Påminn om uppnådd ålder")%></legend>
					                <table>
                                       <SOE:CheckBoxEntry 
                                            ID="UseEmployeeAgeReminder"
                                            TermID="7090"
                                            DefaultTerm="Använd" 
                                            runat="server">
                                       </SOE:CheckBoxEntry>
                                       <SOE:TextEntry
                                            ID="EmployeeAgeReminderAges" 
                                            TermID="9320"
                                            DefaultTerm="Åldrar" 
                                            runat="server">
                                       </SOE:TextEntry>
                                       <SOE:NumericEntry
                                            ID="ReminderDaysBeforeEmployeeAgeReached" 
                                            TermID="9087"
                                            DefaultTerm="Dagar i förväg" 
                                            MaxLength="3"
                                            AllowDecimals="false"
                                            Width="20"
                                            runat="server">
                                       </SOE:NumericEntry>
					                </table>
                                </fieldset>
                            </div>
                            <div class="col">
                                 <fieldset> 
					                <legend><%=GetText(9319, "Påminn om uppnådd branschvana (månader)")%></legend>
					                <table>
                                       <SOE:CheckBoxEntry 
                                            ID="UseEmployeeExperienceReminder"
                                            TermID="7090"
                                            DefaultTerm="Använd" 
                                            runat="server">
                                       </SOE:CheckBoxEntry>
                                        <SOE:TextEntry
                                            ID="EmployeeExperienceReminderMonths" 
                                            TermID="9321"
                                            DefaultTerm="Månader" 
                                            runat="server">
                                       </SOE:TextEntry>
                                       <SOE:NumericEntry
                                            ID="ReminderDaysBeforeEmployeeExperienceReached" 
                                            TermID="9087"
                                            DefaultTerm="Dagar i förväg" 
                                            MaxLength="3"
                                            AllowDecimals="false"
                                            Width="20"
                                            runat="server">
                                       </SOE:NumericEntry>
					                </table>
                                </fieldset>
                            </div>
                            <div class="col">
                                 <fieldset> 
					                <legend><%=GetText(10935, "Påminn om uppdatera branschvana")%></legend>
					                <table>
                                       <SOE:CheckBoxEntry 
                                            ID="UseUpdateExperienceReminder"
                                            TermID="7090"
                                            DefaultTerm="Använd" 
                                            runat="server">
                                       </SOE:CheckBoxEntry>                                        
					                </table>
                                </fieldset>
                            </div>
                        </fieldset>
                        <fieldset style="border:hidden; margin:0px;">
                        <div class="col">
                            <fieldset> 
					            <legend><%=GetText(9105, "Publicera scheman automatiskt")%></legend>
					            <table>
                                    <SOE:CheckBoxEntry 
                                        ID="UsePublishScheduleAutomaticly"
                                        TermID="9086"
                                        DefaultTerm="Använd" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:NumericEntry
                                        ID="PublishScheduleAutomaticlyDaysInAdvanced" 
                                        TermID="9087"
                                        DefaultTerm="Dagar i förväg" 
                                        MaxLength="3"
                                        AllowDecimals="false"
                                        Width="20"
                                        runat="server">
                                    </SOE:NumericEntry>
					            </table>
                            </fieldset>
                        </div>
                        <div class="col">
                            <fieldset> 
					            <legend><%=GetText(7211, "Påminnelse orderplanering")%></legend>
					            <table>
                                    <SOE:CheckBoxEntry 
                                        ID="UseReminderOrderSchedule"
                                        TermID="9086"
                                        DefaultTerm="Använd" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:NumericEntry
                                        ID="ReminderOrderScheduleDaysInAdvance" 
                                        TermID="9087"
                                        DefaultTerm="Dagar i förväg" 
                                        MaxLength="3"
                                        AllowDecimals="false"
                                        Width="20"
                                        runat="server">
                                    </SOE:NumericEntry>
					            </table>
                            </fieldset>
                        </div>
                        <div class="col">
                            <fieldset> 
					            <legend><%=GetText(11992, "Påminnelse efter längre frånvaro")%></legend>
					            <table>
                                    <SOE:CheckBoxEntry 
                                        ID="ReminderAfterLongAbsence"
                                        TermID="9086"
                                        DefaultTerm="Använd" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:NumericEntry
                                        ID="ReminderAfterLongAbsenceDaysInAdvance" 
                                        TermID="11993"
                                        DefaultTerm="Dagar innan frånvaro avslutas" 
                                        MaxLength="2"
                                        AllowDecimals="false"
                                        Width="20"
                                        runat="server">
                                    </SOE:NumericEntry>
                                        <SOE:NumericEntry
                                        ID="IsReminderAfterLongAbsenceAfterDays" 
                                        TermID="11994"
                                        DefaultTerm="Antal dagar för att det ska anses som längre frånvaro" 
                                        MaxLength="3"
                                        AllowDecimals="false"
                                        Width="20"
                                        runat="server">
                                    </SOE:NumericEntry>
					            </table>
                            </fieldset>
                        </div>
                        </fieldset>        
				</div>
			</SOE:Tab>
		</tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

