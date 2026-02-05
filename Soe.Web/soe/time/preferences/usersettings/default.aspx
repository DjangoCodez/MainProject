<%@ Page Title="" Language="C#" AutoEventWireup="True" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.preferences.usersettings._default" %>

<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
			<SOE:Tab Type="Setting" TermID="5414" DefaultTerm="Användarinställningar" runat="server">
				<div id="DivTimeSchedulePlanning" runat="server">
					<fieldset>
						<legend><%=GetText(3574, "Schemaplanering")%></legend>
						<table>
						    <SOE:SelectEntry
							    ID="TimeSchedulePlanningDefaultView"
							    TermID="3575" 
							    DefaultTerm="Standardvy" 
							    runat="server">
						    </SOE:SelectEntry>
						</table>                        
                        <table>
                            <SOE:CheckBoxEntry
                                ID="TimeSchedulePlanningDisableAutoLoad"
                                TermID="3989"
                                DefaultTerm="Stäng av initial laddning av pass" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeSchedulePlanningDisableTemplateScheduleWarning"
                                TermID="3261"
                                DefaultTerm="Stäng av varning i grundschemavy" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                        <div id="DivCalendarView" runat="server">
					        <fieldset>
						        <legend><%=GetText(3576, "Kalendervy")%></legend>
						        <table>
                                    <SOE:NumericEntry
                                        ID="TimeSchedulePlanningStartWeek" 
                                        TermID="3946"
                                        DefaultTerm="Startvecka" 
                                        MaxLength="3"
                                        Width="50"                                
                                        AllowDecimals="false"
                                        AllowNegative="true"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <tr style="height:15px">
                                        <td></td>
                                        <td>
                                            <SOE:Instruction FitInTable="true"
                                                ID="TimeSchedulePlanningStartWeekInstructionRow1"
                                                TermID="3947" DefaultTerm="Exempel:"
                                                runat="server">
                                            </SOE:Instruction>
                                        </td>
                                    </tr>
                                    <tr style="height:15px">
                                        <td></td>
                                        <td>
                                            <SOE:Instruction FitInTable="true" 
                                                ID="TimeSchedulePlanningStartWeekInstructionRow2"
                                                TermID="3948" DefaultTerm="0 = Aktuell vecka"
                                                runat="server">
                                            </SOE:Instruction>
                                        </td>
                                    </tr>
                                    <tr style="height:15px">
                                        <td></td>
                                        <td>
                                            <SOE:Instruction FitInTable="true"
                                                ID="TimeSchedulePlanningStartWeekInstructionRow3"
                                                TermID="3949" DefaultTerm="-1 = Föregående vecka"
                                                runat="server">
                                            </SOE:Instruction>
                                        </td>
                                    </tr>
                                    <tr style="height:15px">
                                        <td></td>
                                        <td>
                                            <SOE:Instruction FitInTable="true"
                                                ID="TimeSchedulePlanningStartWeekInstructionRow4"
                                                TermID="3950" DefaultTerm="1 = Nästa vecka"
                                                runat="server">
                                            </SOE:Instruction>
                                        </td>
                                    </tr>
                                </table>
                                <table>
                                    <SOE:BooleanEntry
                                        ID="TimeSchedulePlanningCalendarViewCountType" 
                                        TermID="3937"
                                        DefaultTerm="Siffran i respektive ruta anger"
                                        FalseTermId="3938"
                                        FalseDefaultTerm="Antal pass per dag"
                                        TrueTermId="3939"
                                        TrueDefaultTerm="Antal personer per dag"
                                        SeparateRows="true"
                                        runat="server">
                                    </SOE:BooleanEntry>
                                </table>
                                <table>
                                    <SOE:Instruction FitInTable="True"
                                        TermID="3940" DefaultTerm="För ledigt pass visas alltid antal pass oavsett inställningen ovan"
                                        runat="server">
                                    </SOE:Instruction>
                                    <tr/>
                                    <SOE:CheckBoxEntry
                                        ID="TimeSchedulePlanningCalendarViewShowToolTipInfo"
                                        TermID="3577"
                                        DefaultTerm="Visa information i tooltip" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
						        </table>
					        </fieldset>
                        </div>
                        <div id="DivDayView" runat="server">
					        <fieldset>
						        <legend><%=GetText(3889, "Dagvy")%></legend>
						        <table>
                                    <SOE:CheckBoxEntry
                                        ID="TimeSchedulePlanningShowEmployeeList"
                                        TermID="3962"
                                        DefaultTerm="Visa anställdalistan" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="TimeSchedulePlanningDisableCheckBreakTimesWarning"
                                        TermID="3888"
                                        DefaultTerm="Varna inte om ändring av raster vid 'drag och släpp'" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
						            <SOE:SelectEntry
							            ID="TimeSchedulePlanningDayViewGroupBy"
							            TermID="3648" 
							            DefaultTerm="Standardgruppering" 
							            runat="server">
						            </SOE:SelectEntry>
						            <SOE:SelectEntry
							            ID="TimeSchedulePlanningDayViewSortBy"
							            TermID="3649" 
							            DefaultTerm="Standardsortering" 
							            runat="server">
						            </SOE:SelectEntry>
						        </table>
					        </fieldset>
                        </div>
                        <div id="DivScheduleView" runat="server">
					        <fieldset>
						        <legend><%=GetText(3922, "Schemavy")%></legend>
						        <table>
			                        <SOE:SelectEntry
							            ID="TimeSchedulePlanningDefaultInterval"
							            TermID="3340" 
							            DefaultTerm="Standardintervall" 
							            runat="server">
						            </SOE:SelectEntry>
                                    <SOE:SelectEntry
							            ID="TimeSchedulePlanningDefaultShiftStyle"
							            TermID="3502" 
							            DefaultTerm="Standardutseende" 
							            runat="server">
						            </SOE:SelectEntry>
                                    <tr>
                                        <td colspan="3">
                                            <SOE:InstructionList ID="TimeSchedulePlanningDefaultShiftStyleInstruction"
                                                runat="server">
                                            </SOE:InstructionList>
                                        </td>
                                    </tr>
						            <SOE:SelectEntry
							            ID="TimeSchedulePlanningScheduleViewGroupBy"
							            TermID="3648" 
							            DefaultTerm="Standardgruppering" 
							            runat="server">
						            </SOE:SelectEntry>
						            <SOE:SelectEntry
							            ID="TimeSchedulePlanningScheduleViewSortBy"
							            TermID="3649" 
							            DefaultTerm="Standardsortering" 
							            runat="server">
						            </SOE:SelectEntry>
						        </table>
					        </fieldset>
                        </div>
                        <div id="DivEditShift" runat="server">
					        <fieldset>
						        <legend><%=GetText(3963, "Redigera pass")%></legend>
						        <table>
                                    <SOE:CheckBoxEntry
                                        ID="TimeSchedulePlanningDisableSaveOnNavigateWarning"
                                        TermID="3990"
                                        DefaultTerm="Spara ändringar automatiskt vid navigering till annan dag eller anställd" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="TimeSchedulePlanningDisableBreaksWithinHolesWarning"
                                        TermID="3964"
                                        DefaultTerm="Justera pass automatiskt så de sträcker sig över rasterna" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
						        </table>
					        </fieldset>
                        </div>
					</fieldset>
				</div>
                <div id="DivStaffingNeeds" runat="server">
					<fieldset>
						<legend><%=GetText(7216, "Behovsplanering")%></legend>
                        <div id="DivStaffingNeedsDayView" runat="server">
					        <fieldset>
						        <legend><%=GetText(3889, "Dagvy")%></legend>
						        <table>
                                    <SOE:CheckBoxEntry
                                        ID="StaffingNeedsDayViewShowDiagram"
                                        TermID="11658"
                                        DefaultTerm="Visa diagram" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="StaffingNeedsDayViewShowDetailedSummary"
                                        TermID="11659"
                                        DefaultTerm="Visa detaljerad summering" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                </table>
                            </fieldset>
                        </div>
                         <div id="DivStaffingNeedsScheduleView" runat="server">
					        <fieldset>
						        <legend><%=GetText(3922, "Schemavy")%></legend>
						        <table>
                                    <SOE:CheckBoxEntry
                                        ID="StaffingNeedsScheduleViewShowDetailedSummary"
                                        TermID="11659"
                                        DefaultTerm="Visa detaljerad summering" 
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                </table>
                            </fieldset>
                            </div>
                    </fieldset>
                </div>
                <div>
					<fieldset>
                        <legend><%=GetText(3178, "Anställd")%></legend>
						<table>
                            <SOE:CheckBoxEntry
                                ID="EmployeeGridDisableAutoLoad"
                                TermID="3179"
                                DefaultTerm="Stäng av initial laddning av anställdalistan" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
					</fieldset>
				</div>
                <div>
					<fieldset>
                        <legend><%=GetText(5280, "Attest")%></legend>
						<table>
                            <SOE:CheckBoxEntry
                                ID="AttestTreeDisableAutoLoad"
                                TermID="3582"
                                DefaultTerm="Stäng av initial laddning av attestträdet" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AttestDisableSaveAttestWarning"
                                TermID="5762"
                                DefaultTerm="Varna inte vid attest av tider" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AttestDisableApplyRestoreWarning"
                                TermID="5935"
                                DefaultTerm="Varna inte vid återställning av tider" 
                                runat="server">
                            </SOE:CheckBoxEntry>                            
						</table>
					</fieldset>
				</div>
                <div>
					<fieldset>
                        <legend><%=GetText(5950, "Löneberäkning")%></legend>
						<table>
                            <SOE:CheckBoxEntry
                                ID="PayrollCalculationTreeDisableAutoLoad"
                                TermID="3582"
                                DefaultTerm="Stäng av initial laddning av attestträdet" 
                                runat="server">
                            </SOE:CheckBoxEntry>
						</table>
					</fieldset>
				</div>
            </SOE:Tab>
        </tabs>
    </SOE:Form>
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>
