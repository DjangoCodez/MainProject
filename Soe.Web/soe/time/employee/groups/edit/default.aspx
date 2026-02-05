<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.employee.groups.edit._default" %>
<%@ Register Src="~/UserControls/TimeCodeTimeDeviationCause.ascx" TagPrefix="SOE" TagName="TimeCodeTimeDeviationCause" %>
<%@ Register Src="~/UserControls/AttestTransition.ascx" TagPrefix="SOE" TagName="AttestTransition" %>
<%@ Register Src="/UserControls/AccumulatorSetting.ascx" TagPrefix="SOE" TagName="AccumulatorSetting" %>

<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">

    <script type="text/javascript" language="javascript">
        var stdDimID = "<%=stdDimID%>";
        var companyID = "<%=SoeCompany.ActorCompanyId%>";
    </script>

    <script type="text/javascript"> 
        var dayTypeArraySelectCount =<%=selectedDayTypesDict.Count%>;
        var dayTypeArray = new Array();
    <%foreach (var dictionary in selectedDayTypesDict)
        {%>
        dayTypeArray.push(<%=dictionary.Value%>);
    <%}%>

        var dayTypeHolidaySalaryArraySelectCount =<%=selectedHolidaySalaryDayTypesDict.Count%>;
        var dayTypeHolidaySalaryArray = new Array();
    <%foreach (var dictionary in selectedHolidaySalaryDayTypesDict)
        {%>
        dayTypeHolidaySalaryArray.push(<%=dictionary.Value%>);
    <%}%>

        var accumulatorsArraySelectCount =<%=selectedTimeAccumulatorsDict.Count%>;
        var accumulatorsArray = new Array();
    <%foreach (var dictionary in selectedTimeAccumulatorsDict)
        {%>
        accumulatorsArray.push(<%=dictionary.Value%>);
    <%}%>

        var deviationCausesArraySelectCount =<%=selectedTimeDeviationCausesDict.Count%>;
        var deviationCausesArray = new Array();
    <%foreach (var dictionary in selectedTimeDeviationCausesDict)
        {%>
        deviationCausesArray.push(<%=dictionary.Value%>);
    <%}%>

        var deviationCausesRequestsArraySelectCount =<%=selectedTimeDeviationCauseRequestsDict.Count%>;
        var deviationCausesRequestsArray = new Array();
    <%foreach (var dictionary in selectedTimeDeviationCauseRequestsDict)
        {%>
        deviationCausesRequestsArray.push(<%=dictionary.Value%>);
    <%}%>

        var deviationCausesAbsenceAnnouncementsArraySelectCount =<%=selectedTimeDeviationCauseAbsenceAnnouncementsDict.Count%>;
        var deviationCausesAbsenceAnnouncementsArray = new Array();
    <%foreach (var dictionary in selectedTimeDeviationCauseAbsenceAnnouncementsDict)
        {%>
        deviationCausesAbsenceAnnouncementsArray.push(<%=dictionary.Value%>);
    <%}%>

        var timeCodesArraySelectCount =<%=selectedTimeCodesDict.Count%>;
        var timeCodesArray = new Array();
    <%foreach (var dictionary in selectedTimeCodesDict)
        {%>
        timeCodesArray.push(<%=dictionary.Value%>);
        <%}%>
    </script>
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true"
        runat="server">
        <tabs>
			<SOE:Tab Type="Edit" runat="server">
		        <div>
		            <fieldset> 
		                <legend><%=GetText(4408, "Tidavtal")%></legend>
	                    <table>
		                    <SOE:TextEntry
			                    ID="Name"
			                    TermID="5047" DefaultTerm="Namn"
			                    Validation="Required"
			                    InvalidAlertTermID="5048" InvalidAlertDefaultTerm="Du måste ange namn"
			                    MaxLength="100"
			                    runat="server">
		                    </SOE:TextEntry>
                            <SOE:TextEntry 
						        ID="ExternalCodes"
						        TermID="11842" DefaultTerm="Externa koder (vid flera separera med #)"
						        MaxLength="100"
						        runat="server">
					         </SOE:TextEntry>
                            <SOE:CheckBoxEntry
                                ID="AlsoAttestAdditionsFromTime" 
                                TermID="8592"
                                DefaultTerm="Attestera även resa/utlägg från Min tid" 
                                runat="server">
                            </SOE:CheckBoxEntry>
		               </table>
		           </fieldset>
                </div>
			    <div>
	                <fieldset> 
		                <legend><%=GetText(4435, "Tidavtal schemaläggs på följande dagtyper")%></legend>
                        <table>
                            <SOE:FormIntervalEntry
                                ID="DayTypes"
                                OnlyFrom="true"
                                NoOfIntervals="30"
                                DisableHeader="true"
                                DisableSettings="true"
                                HideLabel="true"
                                EnableDelete="true"
                                ContentType="2"
                                LabelType="1"
                                LabelWidth="1"
                                runat="server">
                            </SOE:FormIntervalEntry>
                        </table>
                    </fieldset>
			    </div>
                 <div>
	                <fieldset> 
		                <legend><%=GetText(8936, "Beräkna helglön på följande dagtyper")%></legend>
                        <table>
                            <SOE:FormIntervalEntry
                                ID="DayTypesHolidaySalary"
                                OnlyFrom="true"
                                NoOfIntervals="30"
                                DisableHeader="true"
                                DisableSettings="true"
                                HideLabel="true"
                                EnableDelete="true"
                                ContentType="2"
                                LabelType="1"
                                LabelWidth="1"
                                runat="server">
                            </SOE:FormIntervalEntry>
                        </table>
                    </fieldset>
			    </div>
                <div>
	                <fieldset> 
		                <legend><%=GetText(8053, "Tidavtal är kopplad till följande saldon")%></legend>
                        <table>
                            <SOE:FormIntervalEntry
                                ID="TimeAccumulators"
                                OnlyFrom="true"
                                NoOfIntervals="30"
                                DisableHeader="true"
                                DisableSettings="true"
                                HideLabel="true"
                                EnableDelete="true"
                                ContentType="2"
                                LabelType="1"
                                LabelWidth="1"
                                runat="server">
                            </SOE:FormIntervalEntry>
                        </table>
                    </fieldset>
			    </div>
                <div>
	                <fieldset> 
		                <legend><%=GetText(8054, "Tidavtal är kopplad till följande orsaker")%></legend>
                        <div>
                            <table>
                                <SOE:Text
			                        ID="TimeDeviationCausesText"
                                    TermID="11040" DefaultTerm="Markerade orsaker kan rapporteras av anställd"
                                    runat="server">
                                </SOE:Text>
                            </table>
                        </div>
                        <div class="col">
                             <table>
                                <SOE:FormIntervalEntry
                                    ID="TimeDeviationCauses"
                                    OnlyFrom="true"
                                    NoOfIntervals="50"
                                    DisableHeader="true"
                                    DisableSettings="true"
                                    HideLabel="true"
                                    EnableDelete="true"
                                    EnableCheck="true"
                                    ContentType="2"
                                    LabelType="1"
                                    LabelWidth="1"
                                    runat="server">
                                </SOE:FormIntervalEntry>
                            </table>                        
                        </div>
                        <div class="col">
                            &nbsp;
                            &nbsp;
                            &nbsp;
                        </div>
                        <div>
                            <table>
                                <SOE:SelectEntry
                                    ID="DefaultTimeDeviationCause"
                                    runat="server"
                                    TermID="4474"
                                    DefaultTerm="Standardorsak">
                                </SOE:SelectEntry>
                            </table>
                        </div>
                    </fieldset>
			    </div>
                <div>
	                <fieldset> 
		                <legend><%=GetText(8214, "Tidavtal kan ansöka om frånvaro med följande frånvaroorsaker")%></legend>
                        <div class="col">
                            <table>
                                <SOE:FormIntervalEntry
                                    ID="TimeDeviationCauseRequests"
                                    OnlyFrom="true"
                                    NoOfIntervals="30"
                                    DisableHeader="true"
                                    DisableSettings="true"
                                    HideLabel="true"
                                    EnableDelete="true"
                                    ContentType="2"
                                    LabelType="1"
                                    LabelWidth="1"
                                    runat="server">
                                </SOE:FormIntervalEntry>
                            </table>                        
                        </div>                        
                    </fieldset>
			    </div>
                <div>	                
                    <fieldset> 
		                <legend><%=GetText(8458, "Tidavtal kan sjukanmäla med följande frånvaroorsaker")%></legend>
                        <div class="col">
                            <table>
                                <SOE:FormIntervalEntry
                                    ID="TimeDeviationCauseAbsenceAnnouncements"
                                    OnlyFrom="true"
                                    NoOfIntervals="30"
                                    DisableHeader="true"
                                    DisableSettings="true"
                                    HideLabel="true"
                                    EnableDelete="true"
                                    ContentType="2"
                                    LabelType="1"
                                    LabelWidth="1"
                                    runat="server">
                                </SOE:FormIntervalEntry>
                            </table>                        
                        </div>                        
                    </fieldset>                    
			    </div>
                <div>	                
                    <fieldset> 
		                <legend><%=GetText(7201, "Tidavtal är kopplad till följande tidkoder")%></legend>
                        <div class="col">
                            <table>
                                <SOE:FormIntervalEntry
                                    ID="EmployeeGroupTimeCodes"
                                    OnlyFrom="true"
                                    NoOfIntervals="30"
                                    DisableHeader="true"
                                    DisableSettings="true"
                                    HideLabel="true"
                                    EnableDelete="true"
                                    ContentType="2"
                                    LabelType="1"
                                    LabelWidth="1"
                                    runat="server">
                                </SOE:FormIntervalEntry>
                            </table>                        
                        </div>                        
                    </fieldset>
			    </div>
                <div id="DivTransitions" runat="server">
   		            <SOE:AttestTransition ID="AttestTransitions" Runat="Server"></SOE:AttestTransition>
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
					            runat="server"  
						        ID="NoOfDaysTextEntry"
						        TermID="7159" DefaultTerm="antal dagar"
                                Width="40"
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
                 <div id="DivAccountsHierarchy" runat="server">
				    <fieldset> 
					    <legend><%=GetText(9961, "Inställningar för ekonomisk tillhörighet")%></legend>
                        <table>
                            <SOE:CheckBoxEntry 
                                ID="AllowShiftsWithoutAccount" 
                                TermID="9962"
                                DefaultTerm="Tillåt pass utan tillhörighet"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
			        </fieldset>
		        </div>
                <div>
                    <fieldset> 
				        <legend><%=GetText(3780, "Planering")%></legend>
                        <table>
                            <SOE:CheckBoxEntry 
                                ID="ExtraShiftAsDefault" 
                                TermID="13001"
                                DefaultTerm="Extrapass ibockad som standard vid skapa nytt pass i aktivt schema"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
            </SOE:Tab>
			<SOE:Tab Type="Setting" TermID="5413" DefaultTerm="Inställningar för tid" runat="server">
			    <div>
	                <fieldset> 
		                <legend><%=GetText(7430, "Typ av tidrapportering")%></legend>
                        <table>
		                    <SOE:SelectEntry
				                ID="TimeReportType"
				                DefaultTerm="Typ av tidrapportering"
				                TermID="7430"
                                OnChange = "changedTimeReportType();"
                                runat="server">
				            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry 
                                ID="NotifyChangeOfDeviations" 
                                TermID="11983"
                                DefaultTerm="Skicka meddelande till anställd vid förändring av ufall"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AutoGenTimeAndBreakForProject" 
                                TermID="7443"
                                DefaultTerm="Automatskapa tidblock/Rapportering utan klockslag"
                                runat="server">
                            </SOE:CheckBoxEntry>
                         </table>
		            </fieldset>
		        </div>
                 <div>
	                <fieldset> 
		                <legend><%=GetText(8824, "Karensavdrag")%></legend>
                        <table>
		                    <SOE:SelectEntry
				                ID="QualifyingDayCalculationRule"
				                DefaultTerm="Beräkningssätt"
				                TermID="8825"
                                Width="500"
                                runat="server">
				            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="QualifyingDayCalculationRuleLimitFirstDay" 
                                TermID="110675"
                                DefaultTerm="Begränsa karensavdrag till dag 1"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
		            </fieldset>
		        </div>
                 <div>
                    <fieldset> 
                        <legend><%=GetText(110683, "Arbetstidsförkortning (ATF)")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="TimeWorkReductionCalculationRule"
                                DefaultTerm="Beräkningssätt av sysselsättningsgrad"
                                TermID="110683"
                                Width="500"
                                runat="server">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
	                <fieldset> 
		                <legend><%=GetText(91986, "Periodberäkning")%></legend>
                        <table>
		                    <SOE:CheckBoxEntry
				                ID="CandidateForOvertimeOnZeroDayExcluded"
				                DefaultTerm="Beordrad tid på passfri dag exkluderas från periodberäkningen"
				                TermID="91987"
                                runat="server">
				            </SOE:CheckBoxEntry>
                        </table>
		            </fieldset>
		        </div>
                <div>
	                <fieldset> 
		                <legend><%=GetText(8272, "Arbetstidsregler")%></legend>
                        <table>
		                    <SOE:TextEntry
		                        ID="RuleWorkTimeWeek"
		                        runat="server"
		                        TermID="8273"                            
		                        DefaultTerm="Arbetstid (timmar per vecka)"
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>
                            <SOE:TextEntry
		                        ID="RuleWorkTimeDayMinimum"
		                        runat="server"
		                        TermID="8760"                            
		                        DefaultTerm="Min arbetstid per dag"
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>
                            <SOE:TextEntry
		                        ID="RuleWorkTimeDayMaximumWorkDay"
		                        runat="server"
		                        TermID="11516"                            
		                        DefaultTerm="Max arbetstid per dag"
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>
                            <SOE:TextEntry
		                        ID="RuleWorkTimeDayMaximumWeekend"
		                        runat="server"
		                        TermID="11517"                            
		                        DefaultTerm="Max arbetstid per helgdag"
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>
                            <SOE:TextEntry
		                        ID="MaxScheduleTimeFullTime"
		                        runat="server"
		                        TermID="11505"                            
		                        DefaultTerm="Max schematid (heltid)"
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>                            		   
                            <SOE:TextEntry
		                        ID="MinScheduleTimeFullTime"
		                        runat="server"
		                        TermID="11506"                            
		                        DefaultTerm="Min schematid (heltid)"
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>
                            <SOE:TextEntry
		                        ID="MaxScheduleTimePartTime"
		                        runat="server"
		                        TermID="11507"                            
		                        DefaultTerm="Max schematid (deltid)"
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">  
                            </SOE:TextEntry>
                            <SOE:TextEntry
		                        ID="MinScheduleTimePartTime"
		                        runat="server"
		                        TermID="11508"                            
		                        DefaultTerm="Min schematid (deltid)"
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>
                            <SOE:TextEntry
		                        ID="MaxScheduleTimeWithoutBreaks"
		                        runat="server"
		                        TermID="11690"                            
		                        DefaultTerm="Max schematid utan rast"
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>                            
                            <SOE:TextEntry
		                        ID="RuleResttimeDay"
		                        runat="server"
		                        TermID="8274"
		                        DefaultTerm="Dygnsvila"                            
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>
                            <SOE:TextEntry
		                        ID="RuleRestTimeDayStartTime"
		                        runat="server"
		                        TermID="11563"
		                        DefaultTerm="Dygnsvila startar klockan"                            
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>
                             <SOE:TextEntry
		                        ID="RuleResttimeWeek"
		                        runat="server"
		                        TermID="8472"
		                        DefaultTerm="Veckovila"                            
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>
                             <SOE:SelectEntry
				                ID="RestTimeWeekStartDaySelectEntry"
				                DefaultTerm="Veckovila startar på veckodag"
				                TermID="11561"
                                Width="250"
                                runat="server">
				            </SOE:SelectEntry>
                            <SOE:TextEntry
		                        ID="RuleRestTimeWeekStartTime"
		                        runat="server"
		                        TermID="11562"
		                        DefaultTerm="Veckovila startar klockan"                            
                                OnChange = "formatTimeEmptyIfPossible(this);"
                                OnFocus = "this.select();">                            
		                    </SOE:TextEntry>
                            <SOE:NumericEntry
                                ID="RuleScheduleFreeWeekendsMinimumYear"
                                TermID="9356" DefaultTerm="Min lediga helger per kalenderår"
                                AllowNegative="false"
                                AllowDecimals="false"
                                Width = "50"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:NumericEntry
                                ID="RuleScheduledDaysMaximumWeek"
                                TermID="9358" DefaultTerm="Max schemalagda dagar per vecka"
                                AllowNegative="false"
                                AllowDecimals="false"
                                Width ="50"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:CheckBoxEntry
                                ID="RuleRestDayIncludePresence" 
                                TermID="10137"
                                DefaultTerm="Kontrollera dygnsvila även på närvaro"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="RuleRestWeekIncludePresence" 
                                TermID="10138"
                                DefaultTerm="Kontrollera veckovila även på närvaro"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>

	                    <fieldset> 
		                    <legend><%=GetText(11833, "Planeringsperioder")%></legend>
                            <table>
                                <tr>
                                    <th width="200px"><label style="padding-left:5px"><%=GetText(11836, "Period") %></label></th>
                                    <th width="200px"><label style="padding-left:5px"><%=GetText(11835, "Arbetstid (timmar/period)")%></label></th>
                                </tr>
                                <SOE:FormIntervalEntry
                                    ID="PlanningPeriod"
                                    OnlyFrom="True"
                                    DisableHeader="true"
                                    DisableSettings="true"
                                    EnableDelete="true"
                                    ContentType="1"
                                    LabelType="2"
                                    LabelWidth="200"
                                    FromWidth="200"
                                    HideLabel="true"
                                    NoOfIntervals="20"
                                    runat="server">
                                </SOE:FormIntervalEntry>
                            </table>
		                </fieldset>
		            </fieldset>
		        </div>
			    <div>
	                <fieldset> 
		                <legend><%=GetText(4477, "Avvikelser")%></legend>
                        <table>
                            <SOE:NumericEntry
                                ID="DeviationTimeAxelHoursBeforeSchema"
                                runat="server"
                                TermID="4475"
                                DefaultTerm="Antal timmar som visas före schema">
                            </SOE:NumericEntry>
                            <SOE:NumericEntry
                                ID="DeviationTimeAxelHoursAfterSchema"
                                runat="server"
                                TermID="4476"
                                DefaultTerm="Antal timmar som visas efter schema">
                            </SOE:NumericEntry>
                        </table>
                    </fieldset>
                </div>
		        <div>
	                <fieldset> 
	                    <legend><%=GetText(4525, "Projekt")%></legend>
                	    <SOE:SelectEntry
				            ID="DefaultTimeCode"
				            DefaultTerm="Standard tidkod"
				            TermID="4450"
                            runat="server">
				        </SOE:SelectEntry>
					</fieldset>
				</div>	
                <div>
                    <SOE:TimeCodeTimeDeviationCause ID="TimeCodeTimeDeviationCauses" Runat="Server"></SOE:TimeCodeTimeDeviationCause>
                </div>
		        <div id="DivAccounts" runat="server">
                    <fieldset>
                        <legend><%=GetText(3099, "Konton")%></legend>
                        <div>
	                        <div>
                                <fieldset>
                                    <legend><%=GetText(5199, "Kostnad")%></legend>
                                    <table id="CostAccountTable" runat="server">
                                    </table>
                                </fieldset>
	                        </div>
	                        <div>
                                <fieldset>
                                    <legend><%=GetText(5200, "Intäkt")%></legend>
                                    <table id="IncomeAccountTable" runat="server">
                                    </table>
                                </fieldset>
	                        </div>
                        </div>
                    </fieldset>			    
			    </div>
				<div runat="server">
                    <SOE:AccumulatorSetting ID="EmployeeGroupAccumulatorSettings" Runat="Server"></SOE:AccumulatorSetting>
                </div>        
			</SOE:Tab>
		    <SOE:Tab Type="Setting" TermID=7006 DefaultTerm="Inställningar för stämpling" runat="server">
                <div>
                    <fieldset>   
                        <legend><%=GetText(7003, "Stämplingsavrundning")%></legend>
                        <table>
                            <tr>
                                <td>
                                    <asp:Label id="MinutesDummyLabel" Text="" CssClass="formText" Width="100" runat="server" /> 
                                </td>
                                <td>
                                    <asp:Label id="MinutesBeforeLabel" Text="Minuter före" CssClass="formText" Width="180" runat="server" /> 
                                </td>
                                <td>
                                    <asp:Label id="MinutesAfterLabel" Text="Minuter efter" CssClass="formText" Width="180" runat="server" /> 
                                </td>
                            </tr>
                            <SOE:FormIntervalEntry
                                ID="SchemaIn"
                                TermID="7007" DefaultTerm="Schema in:"
                                NoOfIntervals="1"
                                DisableHeader="false"
                                EnableCheck="false"
                                ContentType="4"
                                LabelType="1"
                                runat="server">
                            </SOE:FormIntervalEntry>
                            <SOE:FormIntervalEntry
                                ID="SchemaUt"
                                TermID="7008" DefaultTerm="Schema ut:"
                                NoOfIntervals="1"
                                DisableHeader="true"
                                EnableCheck="false"
                                ContentType="4"
                                LabelType="1"
                                runat="server">
                            </SOE:FormIntervalEntry>
                        </table>    
                    </fieldset>
                    <fieldset>   
                        <legend><%=GetText(3524, "Rasthantering")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="AlwaysDiscardBreakEvaluation" 
                                TermID="5754"
                                DefaultTerm="Skapa alltid rast enligt stämplingar"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="AutogenBreakOnStamping" 
                                TermID="3523"
                                DefaultTerm="Automatisk standardrast vid ej stämplad rast"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="MergeScheduleBreaksOnDay" 
                                TermID="5755"
                                DefaultTerm="Slå ihop rastsaldo på hela dagen vid flera raster"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:Text
			                    ID="BreakRoundingLabel"
                                TermID="3689" DefaultTerm="Rastavrundning"
                                runat="server">
                            </SOE:Text>
                            <SOE:NumericEntry
                                ID="BreakRoundingUp"
                                TermID="3690" DefaultTerm="Avrunda uppåt (minuter):"
                                AllowNegative="false"
                                AllowDecimals="false"
                                Width = "50"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:NumericEntry
                                ID="BreakRoundingDown"
                                TermID="3691" DefaultTerm="Avrunda nedåt (minuter):"
                                AllowNegative="false"
                                AllowDecimals="false"
                                Width = "50"
                                runat="server">
                            </SOE:NumericEntry>
                        </table>    
                    </fieldset>
                    <fieldset>   
                        <legend><%=GetText(6408, "Dygnsbryt")%></legend>
                        <table>
                            <SOE:NumericEntry
                                ID="BreakDayMinutesAfterMidnight"
                                TermID="6409" DefaultTerm="Förskjutning från midnatt (minuter):"
                                AllowNegative="true"
                                AllowDecimals="false"
                                Width = "50"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:NumericEntry
                                ID="KeepStampsTogetherWithinMinutes"
                                TermID="3872" DefaultTerm="Håll ihop stämpling över midnatt (minuter):"
                                AllowNegative="true"
                                AllowDecimals="false"
                                Width = "50"
                                runat="server">
                            </SOE:NumericEntry>
                        </table>    
                    </fieldset>
                </div>
			</SOE:Tab>
            <SOE:Tab Type="Setting" TermID="5406" DefaultTerm="Inställningar kontering" runat="server">
				<div>
					<fieldset> 
						<legend><%=GetText(3375, "Prioriteringsordning för konteringar")%></legend>
				        <div>
					        <fieldset> 
						        <legend><%=GetText(3368, "Löneartskonteringar")%></legend>
						        <table>
						            <SOE:SelectEntry
						                ID="PayrollProductAccountingPrio1"
						                TermID="3370"
						                DefaultTerm="Första"
						                runat="server">
						            </SOE:SelectEntry>
						            <SOE:SelectEntry
						                ID="PayrollProductAccountingPrio2"
						                TermID="3371"
						                DefaultTerm="Andra"
						                runat="server">
						            </SOE:SelectEntry>
						            <SOE:SelectEntry
						                ID="PayrollProductAccountingPrio3"
						                TermID="3372"
						                DefaultTerm="Tredje"
						                runat="server">
						            </SOE:SelectEntry>
						            <SOE:SelectEntry
						                ID="PayrollProductAccountingPrio4"
						                TermID="3373"
						                DefaultTerm="Fjärde"
						                runat="server">
						            </SOE:SelectEntry>
						            <SOE:SelectEntry
						                ID="PayrollProductAccountingPrio5"
						                TermID="3374"
						                DefaultTerm="Femte"
						                runat="server">
						            </SOE:SelectEntry>
						        </table>
					        </fieldset>
				        </div>
				        <div>
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
				        </div>
					</fieldset> 
				</div>
            </SOE:Tab>
        </tabs>
    </SOE:Form>
    <div class="searchTemplate">
        <div id="searchContainer" class="searchContainer">
        </div>
        <div id="accountSearchItem_$accountNr$">
            <div id="account_$id$" class="item" onmouseover="searchComponent.select();" onclick="searchComponent.choose();">
                <div class="id" id="extendNumWidth_$id$">
                    $accountNr$</div>
                <div class="name" id="extendNameWidth_$id$">
                    $accountName$</div>
            </div>
        </div>
    </div>
    <script type="text/javascript">
        for (var i = 0; i < dayTypeArraySelectCount; i++) {
            var inp = null;
            var sel = $$('DayTypes-from-' + (i + 1));
            if (sel != null) {
                for (var j = 0; j < sel.length; j++) {
                    if (sel[j].value == dayTypeArray[i])
                        sel[j].selected = true;
                }
            }
        }
        for (var i = 0; i < dayTypeHolidaySalaryArraySelectCount; i++) {
            var inp = null;
            var sel = $$('DayTypesHolidaySalary-from-' + (i + 1));
            if (sel != null) {
                for (var j = 0; j < sel.length; j++) {
                    if (sel[j].value == dayTypeHolidaySalaryArray[i])
                        sel[j].selected = true;
                }
            }
        }        
        for (var i = 0; i < accumulatorsArraySelectCount; i++) {
            var inp = null;
            var sel = $$('TimeAccumulators-from-' + (i + 1));
            if (sel != null) {
                for (var j = 0; j < sel.length; j++) {
                    if (sel[j].value == accumulatorsArray[i])
                        sel[j].selected = true;
                }
            }
        }
        for (var i = 0; i < deviationCausesArraySelectCount; i++) {
            var inp = null;
            var sel = $$('TimeDeviationCauses-from-' + (i + 1));
            if (sel != null) {
                for (var j = 0; j < sel.length; j++) {
                    if (sel[j].value == deviationCausesArray[i])
                        sel[j].selected = true;
                }
            }
        }
        for (var i = 0; i < deviationCausesRequestsArraySelectCount; i++) {
            var inp = null;
            var sel = $$('TimeDeviationCauseRequests-from-' + (i + 1));
            if (sel != null) {
                for (var j = 0; j < sel.length; j++) {
                    if (sel[j].value == deviationCausesRequestsArray[i])
                        sel[j].selected = true;
                }
            }
        }
        for (var i = 0; i < deviationCausesAbsenceAnnouncementsArraySelectCount; i++) {
            var inp = null;
            var sel = $$('TimeDeviationCauseAbsenceAnnouncements-from-' + (i + 1));
            if (sel != null) {
                for (var j = 0; j < sel.length; j++) {
                    if (sel[j].value == deviationCausesAbsenceAnnouncementsArray[i])
                        sel[j].selected = true;
                }
            }
        }
        for (var i = 0; i < timeCodesArraySelectCount; i++) {
            var inp = null;
            var sel = $$('EmployeeGroupTimeCodes-from-' + (i + 1));
            if (sel != null) {
                for (var j = 0; j < sel.length; j++) {
                    if (sel[j].value == timeCodesArray[i])
                        sel[j].selected = true;
                }
            }
        }
    </script>
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>
