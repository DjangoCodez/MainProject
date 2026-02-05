<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.preferences.compsettings._default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">

    <script type="text/javascript" language="javascript">
        var stdDimID = "<%=stdDimID%>";
        var companyID = "<%=SoeCompany.ActorCompanyId%>";
    </script>

    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
            <SOE:Tab Type="Setting" TermID="5415" DefaultTerm="Företagsinställningar" runat="server">
                <div>
                    <fieldset>
                        <legend><%=GetText(3022, "Generella företagsinställningar")%></legend>
                        <div class="col">
                            <table>
                                <SOE:SelectEntry
                                    runat="server"
                                    ID="DefaultTimeCode"
                                    DefaultTerm="Standard tidkod"
                                    TermID="4450">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    runat="server"
                                    ID="DefaultTimeDeviationCause"
                                    DefaultTerm="Standard orsak"
                                    TermID="9324">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    runat="server"
                                    ID="DefaultEmployeeGroup"
                                    DefaultTerm="Standard tidavtal"
                                    TermID="4510">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    runat="server"
                                    ID="DefaultPayrollGroup"
                                    DefaultTerm="Standard löneavtal"
                                    TermID="91921">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    runat="server"
                                    ID="DefaultVacationGroup"
                                    DefaultTerm="Standard semesteravtal"
                                    TermID="91922">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    runat="server"
                                    ID="DefaultTimePeriodHead"
                                    DefaultTerm="Standard perioduppsättning"
                                    TermID="5304">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    runat="server"
                                    ID="DefaultPlanningPeriod"
                                    DefaultTerm="Standard planeringsperiod"
                                    TermID="11834">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    runat="server"
                                    ID="TimeDefaultTimeCodeEarnedHoliday"
                                    DefaultTerm="Standard tillägg för intjänade röda dagar"
                                    TermID="8737">
                                </SOE:SelectEntry>
                                <SOE:NumericEntry
                                    ID="EmployeeSeqNbrStart"
                                    TermID="9290"
                                    DefaultTerm="Nästa lediga anställningsnummer"
                                    MaxLength="10"
                                    Width="50"
                                    AllowDecimals="false"
                                    AllowNegative="false"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:NumericEntry
                                    ID="EmployeeKeepNbrOfYearsAfterEnd"
                                    TermID="3663"
                                    DefaultTerm="Behåll anställd efter anställningsslut (antal år)"
                                    MaxLength="2"
                                    Width="50"
                                    AllowDecimals="false"
                                    AllowNegative="false"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:SelectEntry
                                    ID="EmployeeIncludeNbrOfMonthsAfterEnded"
                                    TermID="10129"
                                    DefaultTerm="Visa avslutade anställda efter avslutad anställning (antal mån)"
                                    runat="server">
                                </SOE:SelectEntry>
                            </table>
                        </div>
                        <div>
                            <table>
                                <SOE:CheckBoxEntry
                                    ID="DefaultPreviousTimePeriod"
                                    TermID="5392"
                                    DefaultTerm="Visa föregående period som standard i Attest/Min tid"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="DoNotUseMessageGroupInAttest"
                                    TermID="110676"
                                    DefaultTerm="Använd ej mottagargrupp i attestera tid"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeAttestTreeIncludeAdditionalEmployees"
                                    TermID="11981"
                                    DefaultTerm="Visa fler kollegor i attestera tid"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeCodeBreakShowInvoiceProducts"
                                    TermID="5174"
                                    DefaultTerm="Visa artiklar med faktor för rasttyper"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeCodeBreakShowPayrollProducts"
                                    TermID="5175"
                                    DefaultTerm="Visa lönearter med faktor för rasttyper"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseSimplifiedEmployeeRegistration"
                                    TermID="12158"
                                    DefaultTerm="Aktivera förenklat anställningsflöde"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="SuggestEmployeeNrAsUsername"
                                    TermID="7170"
                                    DefaultTerm="Föreslå anställningsnummer som användarnamn"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="ForceSocialSecurityNbr"
                                    TermID="9136"
                                    DefaultTerm="Personnummer obligatoriskt"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="DontValidateSecurityNbr"
                                    TermID="8613"
                                    DefaultTerm="Validera inte personnummer"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="SetEmploymentPercentManually"
                                    TermID="5982"
                                    DefaultTerm="Ange sysselsättningsgrad manuellt"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="SetNextFreePersonNumberAutomatically"
                                    TermID="9286"
                                    DefaultTerm="Ange nästa anställningsnummer automatiskt"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseEmploymentExperienceAsStartValue"
                                    TermID="9322"
                                    DefaultTerm="Använd branschvana på anställning som ingående balans"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeSplitBreakOnAccount"
                                    TermID="91889"
                                    DefaultTerm="Rastutfyllnad och rastavdrag per tillhörighet istället för per dag"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseHibernatingEmployment"
                                    TermID="9347"
                                    DefaultTerm="Använd vilande anställning"
                                    OnClick="useHibernatingEmploymentClicked()"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="DontAllowIdenticalSSN"
                                    TermID="9352"
                                    DefaultTerm="Tilllåt ej identiska personnummer"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="UseIsNearestManagerOnAttestRoleUser"
                                    TermID="13126"
                                    DefaultTerm="Använd chef på attestroll kopplad till anställd"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                            </table>
                        </div>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3387, "Utskrift")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="DefaultTimeMonthlyReport"
                                TermID="5391"
                                DefaultTerm="Standard månadsrapportmall"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultEmployeeScheduleDayReport"
                                TermID="5776"
                                DefaultTerm="Standard dagschemamall"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultEmployeeScheduleWeekReport"
                                TermID="3267"
                                DefaultTerm="Standard veckoschemamall"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultEmployeeTemplateScheduleDayReport"
                                TermID="5883"
                                DefaultTerm="Standard daggrundschemamall"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultEmployeeTemplateScheduleWeekReport"
                                TermID="5894"
                                DefaultTerm="Standard veckogrundschemamall"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultEmployeePostTemplateScheduleDayReport"
                                TermID="11691"
                                DefaultTerm="Standard dagschemamall tjänster"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultEmployeePostTemplateScheduleWeekReport"
                                TermID="11692"
                                DefaultTerm="Standard veckoschemamall tjänster"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultScenarioScheduleDayReport"
                                TermID="11924"
                                DefaultTerm="Standard dagschemamall scenario"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultScenarioScheduleWeekReport"
                                TermID="11925"
                                DefaultTerm="Standard veckoschemamall scenario"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultTimeScheduleTasksAndDeliverysDayReport"
                                TermID="11660"
                                DefaultTerm="Standard arbetsuppgifter och leveranser dag"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultTimeScheduleTasksAndDeliverysWeekReport"
                                TermID="11661"
                                DefaultTerm="Standard arbetsuppgifter och leveranser vecka"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultTimeSalarySpecificationReport"
                                TermID="5570"
                                DefaultTerm="Standard lönespecifikation (Classic Lön)"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultTimeSalaryControlInfoReport"
                                TermID="9094"
                                DefaultTerm="Standard Kontrolluppgiftsrapport"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultTimeKU10Report"
                                TermID="11022"
                                DefaultTerm="Standard KU10 rapport"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultTimeSalarySettingReport"
                                TermID="9285"
                                DefaultTerm="Standard löninställningar rapport"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultXEPayrollSlipReport"
                                TermID="8619"
                                DefaultTerm="Standard lönespecifikation"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultEmployeeVacationDebtReport"
                                TermID="8706"
                                DefaultTerm="Standard semesterskuldsrapport"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="DefaultEmploymentContractShortSubstituteReport"
                                TermID="8751"
                                DefaultTerm="Standard anställningsbevis, kortare vikariat"
                                runat="server">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3304, "Schemainställningar")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="MaxNoOfBrakes"
                                TermID="3305"
                                DefaultTerm="Max antal raster per arbetspass"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="StartOnFirstDayOfWeek"
                                TermID="3366"
                                DefaultTerm="Föreslå starta på en måndag"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseStopDateOnTemplate"
                                TermID="3916"
                                DefaultTerm="Använd slutdatum på schemamall/grundschema"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="CreateShiftsThatStartsAfterMidnigtInMobile"
                                TermID="8512"
                                DefaultTerm="Kunna skapa pass som startar efter midnatt i mobilen"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3875, "Aktivera schema")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="PlacementDefaultPreliminary"
                                TermID="3876"
                                DefaultTerm="Preliminär aktivering av schema som standard"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PlacementHideShiftTypes"
                                TermID="5983"
                                DefaultTerm="Visa inte passtyp"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PlacementHideAccountDims"
                                TermID="5984"
                                DefaultTerm="Visa inte internkontonivåer"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PlacementHidePreliminary"
                                TermID="5985"
                                DefaultTerm="Visa inte preliminär"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(5369, "Attestnivåer")%></legend>
                        <table>
                            <SOE:SelectEntry
                                runat="server"
                                ID="ExportSalaryMinimumAttestStatus"
                                DefaultTerm="Lägsta status för lönebearbetning eller export av löneartstransaktioner"
                                TermID="4497">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                runat="server"
                                ID="ExportSalaryResultingAttestStatus"
                                DefaultTerm="Status efter lönebearbetning eller export av löneartstransaktioner"
                                TermID="4498">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                runat="server"
                                ID="MobileTimeAttestResultingAttestStatus"
                                DefaultTerm="Status efter klarmarkering av tid via mobil"
                                TermID="8258">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                runat="server"
                                ID="ExportInvoiceMinimumAttestStatus"
                                DefaultTerm="Lägsta status för export för fakturatransaktioner"
                                TermID="4499">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                runat="server"
                                ID="ExportInvoiceResultingAttestStatus"
                                DefaultTerm="Status efter export för fakturatransaktioner"
                                TermID="4500">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(3911, "Stämpling")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="IgnoreOfflineTerminals"
                                TermID="3912"
                                DefaultTerm="Ignorera terminaler som är offline vid stämpling"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeDoNotModifyTimeStampEntryType"
                                TermID="8571"
                                DefaultTerm="Justera inte in och ut automatiskt"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseTimeScheduleTypeFromTime"
                                TermID="3699"
                                DefaultTerm="Använd schematyp från passtyp vid stämpling"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="LimitAttendanceViewToStampedTerminal"
                                TermID="3685"
                                DefaultTerm="Visa endast anställd i närvarotablå på stämplad terminal"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PossibilityToRegisterAdditionsInTerminal"
                                TermID="8575"
                                DefaultTerm="Möjlighet att registrera tillägg i samband med stämpling"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
            </SOE:Tab>
            <SOE:Tab Type="Setting" TermID="3779" DefaultTerm="Inställningar planering" runat="server">
                <div>
                    <fieldset>
                        <legend><%=GetText(3780, "Planering")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="UseStaffing"
                                TermID="5579"
                                DefaultTerm="Aktivera planering"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseVacant"
                                TermID="3902"
                                DefaultTerm="Använd vakanta anställda"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="OrderPlanningIgnoreScheduledBreaksOnAssignment"
                                TermID="7530"
                                DefaultTerm="Ignorera raster vid utplanering"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:SelectEntry
                                ID="StaffingShiftAccountDimId"
                                TermID="3781"
                                DefaultTerm="Konteringsnivå för kontering på pass"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="TimeSchedulePlanningDayViewMinorTickLength"
                                TermID="3871"
                                DefaultTerm="Intervall (minuter)"
                                Width="100"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="IncludeSecondaryEmploymentInWorkTimeWeek"
                                TermID="12533"
                                DefaultTerm="Inkludera sekundära anställningar i veckoarbetstiden"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeSchedulePlanningInactivateLending"
                                TermID="12528"
                                DefaultTerm="Inaktivera visualisering och validering av in- och utlåning i schema och attest"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <tr>
                                <td colspan="2">
                                    <SOE:InstructionList ID="TimeSchedulePlanningInactivateLendingInstruction"
                                        runat="server">
                                    </SOE:InstructionList>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                    <fieldset>
                        <legend><%=GetText(3574, "Schemaplanering")%></legend>
                        <table>
                            <SOE:NumericEntry
                                ID="TimeSchedulePlanningClockRounding"
                                TermID="3915"
                                DefaultTerm="Avrunda klockslag (minuter)"
                                MaxLength="4"
                                Width="50"
                                AllowDecimals="false"
                                AllowNegative="false"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeDefaultDoNotKeepShiftsTogether"
                                TermID="3891"
                                DefaultTerm="Håll inte ihop dag"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeSchedulePlanningSendXEMailOnChange"
                                TermID="7138"
                                DefaultTerm="Skicka meddelande vid ändring av pass"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeSchedulePlanningSetShiftAsExtra"
                                TermID="8822"
                                DefaultTerm="Kunna markera pass som extrapass"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeSchedulePlanningSetShiftAsSubstitute"
                                TermID="8823"
                                DefaultTerm="Kunna markera pass som vikariat"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="HideRecipientsInShiftRequest"
                                TermID="3440"
                                DefaultTerm="Dölj mottagare i passförfrågan"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeSchedulePlanningSortQueueByLas"
                                TermID="3672"
                                DefaultTerm="Sortera kö för önskade pass enligt LAS"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="CreateEmployeeRequestWhenDeniedWantedShift"
                                TermID="3996"
                                DefaultTerm="Kopiera tider till tillgänglighet när anställd inte får önskat pass"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ShowTemplateScheduleForEmployeesInApp"
                                TermID="8799"
                                DefaultTerm="Visa grundschema för de anställda i mobilen"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseMultipleScheduleTypes"
                                TermID="11971"
                                DefaultTerm="Aktivera flera schematyper på pass"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="SubstituteShiftIsAssignedDueToAbsenceOnlyIfSameBatch"
                                TermID="10930"
                                DefaultTerm="Tolka ej pass skapade från annan anställd innan dennes frånvaro som vikariat på grund av frånvaro"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="SubstituteShiftDontIncludeCopiedOrMovedShifts"
                                TermID="8857"
                                DefaultTerm="Ta inte med kopierade och flyttade pass till kortare vikariat"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ExtraShiftAsDefaultOnHidden"
                                TermID="13002"
                                DefaultTerm="Extrapass ibockad som standard på pass som skapas på 'ledigt pass' i aktivt schema"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PrintAgreementOnAssignFromFreeShift"
                                TermID="13128"
                                DefaultTerm="Skriv ut anställningsbevis vid tilldelning av ledigt pass"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeSchedulePlanningDragDropMoveAsDefault"
                                TermID="12556"
                                DefaultTerm="Flytta istället för kopiera som standard vid dra och släpp pass"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseLeisureCodes"
                                TermID="12534"
                                DefaultTerm="Använd fridagar"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="UseAnnualLeave"
                                TermID="13124"
                                DefaultTerm="Använd årsledighet"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeSchedulePlanningSaveCopyOnPublish"
                                TermID="13127"
                                DefaultTerm="Spara kopia av schema vid publicering (Preliminär till definitiv)"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                        <fieldset>
                            <legend><%=GetText(8272, "Arbetstidsregler")%></legend>
                            <table>
                                <SOE:CheckBoxEntry
                                    ID="TimeSchedulePlanningSkipWorkRules"
                                    TermID="3991"
                                    DefaultTerm="Möjlighet att stänga av kontroll av arbetstidsregler"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeSchedulePlanningUseWorkRulesForMinors"
                                    TermID="8734"
                                    DefaultTerm="Använd arbetstidsregler för minderåriga"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeSchedulePlanningOverrideWorkRuleWarningsForMinors"
                                    TermID="8723"
                                    DefaultTerm="Möjlighet att gå förbi arbetstidsregler för minderåriga"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeSchedulePlanningRuleRestTimeDayMandatory"
                                    TermID="11548"
                                    DefaultTerm="Tillåt ej att passera varning för dygnsvila i grundschema/tjänster"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeSchedulePlanningRuleRestTimeWeekMandatory"
                                    TermID="11549"
                                    DefaultTerm="Tillåt ej att passera varning för veckovila i grundschema/tjänster"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeSchedulePlanningRuleRuleWorkTimeWeekDontEvaluateInSchedule"
                                    TermID="11550"
                                    DefaultTerm="Stäng av arbetstidsregel veckoarbetstid +/- i schema dag- och veckovy"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeSchedulePlanningUseRuleWorkTimeWeekForParttimeWorkersInSchedule"
                                    TermID="8776"
                                    DefaultTerm="Använd arbetstidsregel veckoarbetstid för deltidsarbetare i schema dag- och veckovy"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:NumericEntry
                                    ID="TimeSchedulePlanningRuleWorkTimeHoursBeforeAssignShift"
                                    TermID="11559"
                                    DefaultTerm="Varna vid tilldelning (kopiera/flytta) av pass innan angiven tidsgräns i timmar"
                                    MaxLength="2"
                                    Width="50"
                                    AllowDecimals="false"
                                    AllowNegative="false"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:CheckBoxEntry
                                    ID="TimeSchedulePlanningShiftRequestPreventTooEarly"
                                    TermID="12515"
                                    DefaultTerm="Kontrollera att passförfrågan inte skickas för tidigt"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:NumericEntry
                                    ID="TimeSchedulePlanningShiftRequestPreventTooEarlyWarnHoursBefore"
                                    TermID="12516"
                                    DefaultTerm="Varna om passförfrågan skickas mer än NN timmar före passets starttid"
                                    MaxLength="3"
                                    Width="50"
                                    AllowDecimals="false"
                                    AllowNegative="false"
                                    runat="server">
                                </SOE:NumericEntry>
                                <SOE:NumericEntry
                                    ID="TimeSchedulePlanningShiftRequestPreventTooEarlyStopHoursBefore"
                                    TermID="12517"
                                    DefaultTerm="Stoppa om passförfrågan skickas mer än NN timmar före passets starttid"
                                    MaxLength="3"
                                    Width="50"
                                    AllowDecimals="false"
                                    AllowNegative="false"
                                    runat="server">
                                </SOE:NumericEntry>
                                <tr>
                                    <td colspan="2">
                                        <SOE:InstructionList ID="TimeSchedulePlanningShiftRequestPreventTooEarlyInstruction"
                                            runat="server">
                                        </SOE:InstructionList>
                                    </td>
                                </tr>
                            </table>
                        </fieldset>
                        <div id="DivCalendarView" runat="server">
                            <fieldset>
                                <legend><%=GetText(3576, "Kalendervy")%></legend>
                                <table>
                                    <SOE:CheckBoxEntry
                                        ID="TimeSchedulePlanningCalendarViewShowDaySummary"
                                        TermID="3944"
                                        DefaultTerm="Visa summering per dag"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                </table>
                            </fieldset>
                        </div>
                        <div id="DivDayView" runat="server">
                            <fieldset>
                                <legend><%=GetText(3889, "Dagvy")%></legend>
                                <table>
                                    <SOE:TextEntry
                                        ID="TimeSchedulePlanningDayViewStartTime"
                                        runat="server"
                                        TermID="3892"
                                        DefaultTerm="Dagen börjar kl."
                                        Width="50"
                                        OnChange="formatTimeEmptyIfPossible(this, true);"
                                        OnFocus="this.select();">
                                    </SOE:TextEntry>
                                    <SOE:TextEntry
                                        ID="TimeSchedulePlanningDayViewEndTime"
                                        runat="server"
                                        TermID="3893"
                                        DefaultTerm="Dagen slutar kl."
                                        Width="50"
                                        OnChange="formatTimeEmptyIfPossible(this, true);"
                                        OnFocus="this.select();">
                                    </SOE:TextEntry>
                                    <tr>
                                        <td colspan="2">
                                            <SOE:InstructionList ID="DayViewEndTimeInstruction"
                                                runat="server">
                                            </SOE:InstructionList>
                                        </td>
                                    </tr>
                                    <tr>
                                    </tr>
                                    <SOE:SelectEntry
                                        ID="TimeSchedulePlanningBreakVisibility"
                                        TermID="3890"
                                        DefaultTerm="Visa raster grafiskt"
                                        runat="server">
                                    </SOE:SelectEntry>
                                </table>
                            </fieldset>
                        </div>
                        <div id="DivEditShift" runat="server">
                            <fieldset>
                                <legend><%=GetText(3955, "Redigera pass")%></legend>
                                <table>
                                    <SOE:CheckBoxEntry
                                        ID="TimeEditShiftShowEmployeeInGridView"
                                        TermID="3956"
                                        DefaultTerm="Visa anställd per pass"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="TimeEditShiftShowDateInGridView"
                                        TermID="3957"
                                        DefaultTerm="Visa datum per pass"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="TimeShiftTypeMandatory"
                                        TermID="3840"
                                        DefaultTerm="Passtyp obligatorisk"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="TimeEditShiftAllowHoles"
                                        TermID="3992"
                                        DefaultTerm="Tillåt hål utan raster"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                </table>
                            </fieldset>
                        </div>
                        <div id="DivCosts" runat="server">
                            <fieldset>
                                <legend><%=GetText(5930, "Kostnader")%></legend>
                                <table>
                                    <SOE:CheckBoxEntry
                                        ID="StaffingUseTemplateCost"
                                        TermID="5933"
                                        DefaultTerm="Använd schablonkostnad"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:TextEntry
                                        ID="StaffingTemplateCost"
                                        TermID="5931"
                                        DefaultTerm="Schablonkonstnad per timme"
                                        runat="server">
                                    </SOE:TextEntry>
                                </table>
                                <table>
                                    <SOE:Instruction ID="TemplateInstruction"
                                        TermID="5936" DefaultTerm="Om ej schablonkostnad används så används lön/timme hämtat från anställd"
                                        runat="server">
                                    </SOE:Instruction>
                                </table>
                            </fieldset>
                        </div>
                        <div id="DivAvailability" runat="server">
                            <fieldset>
                                <legend><%=GetText(3036, "Tillgänglighet")%></legend>
                                <table>
                                    <SOE:NumericEntry
                                        ID="TimeAvailabilityLockDaysBefore"
                                        TermID="3037"
                                        DefaultTerm="Lås tillgänglighet ett antal dagar innan"
                                        MaxLength="3"
                                        Width="50"
                                        AllowDecimals="false"
                                        AllowNegative="false"
                                        runat="server">
                                    </SOE:NumericEntry>
                                    <tr>
                                        <td colspan="2">
                                            <SOE:InstructionList
                                                ID="TimeAvailabilityLockDaysBeforeInstruction"
                                                runat="server">
                                            </SOE:InstructionList>
                                        </td>
                                    </tr>
                                </table>
                            </fieldset>
                        </div>
                        <div id="DivPlanningContactInformation" runat="server">
                            <fieldset>
                                <legend><%=GetText(12032, "Kontaktuppgifter")%></legend>
                                <SOE:InstructionList ID="PlanningContactInformationInstruction"
                                    runat="server">
                                </SOE:InstructionList>
                                <div class="col">
                                    <table>
                                        <SOE:Text
                                            ID="PlanningContactInformationAddressTypesLabel"
                                            TermID="12035" DefaultTerm="Adresser"
                                            runat="server">
                                        </SOE:Text>
                                    </table>
                                </div>
                                <div class="col">
                                    <table>
                                        <SOE:FormIntervalEntry
                                            ID="PlanningContactInformationAddressTypes"
                                            OnlyFrom="true"
                                            NoOfIntervals="20"
                                            DisableHeader="true"
                                            DisableSettings="true"
                                            HideLabel="true"
                                            EnableDelete="true"
                                            EnableCheck="false"
                                            ContentType="2"
                                            LabelType="1"
                                            LabelWidth="1"
                                            runat="server">
                                        </SOE:FormIntervalEntry>
                                    </table>
                                </div>
                                <div class="col">
                                    <table>
                                        <SOE:Text
                                            ID="PlanningContactInformationEComTypesLabel"
                                            TermID="12036" DefaultTerm="Telefon/webb"
                                            runat="server">
                                        </SOE:Text>
                                    </table>
                                </div>
                                <div class="col">
                                    <table>
                                        <SOE:FormIntervalEntry
                                            ID="PlanningContactInformationEComTypes"
                                            OnlyFrom="true"
                                            NoOfIntervals="20"
                                            DisableHeader="true"
                                            DisableSettings="true"
                                            HideLabel="true"
                                            EnableDelete="true"
                                            EnableCheck="false"
                                            ContentType="2"
                                            LabelType="1"
                                            LabelWidth="1"
                                            runat="server">
                                        </SOE:FormIntervalEntry>
                                    </table>
                                </div>
                            </fieldset>
                        </div>
                        <div id="DivMinors" runat="server">
                            <fieldset>
                                <legend><%=GetText(8759, "Minderåriga")%></legend>
                                <table>
                                    <SOE:TextEntry
                                        ID="MinorsSchoolDayStartMinutes"
                                        runat="server"
                                        TermID="8731"
                                        DefaultTerm="Skoldagen börjar kl."
                                        Width="50"
                                        OnChange="formatTimeEmptyIfPossible(this, true);"
                                        OnFocus="this.select();">
                                    </SOE:TextEntry>
                                    <SOE:TextEntry
                                        ID="MinorsSchoolDayStopMinutes"
                                        runat="server"
                                        TermID="8732"
                                        DefaultTerm="Skoldagen slutar kl."
                                        Width="50"
                                        OnChange="formatTimeEmptyIfPossible(this, true);"
                                        OnFocus="this.select();">
                                    </SOE:TextEntry>
                                    <tr>
                                        <td colspan="2">
                                            <SOE:InstructionList ID="InstructionsMinors"
                                                runat="server">
                                            </SOE:InstructionList>
                                        </td>
                                    </tr>
                                </table>
                            </fieldset>
                        </div>
                        <div id="DivPlanningPeriods" runat="server">
                            <fieldset>
                                <legend><%=GetText(12181, "Periodsammanställning")%></legend>
                                <table>
                                    <SOE:CheckBoxEntry
                                        ID="CalculatePlanningPeriodScheduledTime"
                                        TermID="3924"
                                        DefaultTerm="Beräkna periodsammanställning"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:Instruction ID="CalculatePlanningPeriodScheduledTimeInstruction"
                                        TermID="10943" DefaultTerm="Denna funktion påverkar prestanda. Bocka endast i om ni verkligen använder den och vet vad den innebär."
                                        runat="server">
                                    </SOE:Instruction>
                                    <SOE:CheckBoxEntry
                                        ID="CalculatePlanningPeriodScheduledTimeIncludeExtraShift"
                                        TermID="11841"
                                        DefaultTerm="Inkludera extrapass i periodsammanställning"
                                        Indent="true"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:CheckBoxEntry
                                        ID="CalculatePlanningPeriodScheduledTimeUseAveragingPeriod"
                                        TermID="12532"
                                        DefaultTerm="Använd medelvärdesperiod"
                                        Indent="true"
                                        runat="server">
                                    </SOE:CheckBoxEntry>
                                    <SOE:Text
                                        ID="PlanningPeriodColorLabel"
                                        TermID="12182"
                                        DefaultTerm="Färger"
                                        runat="server">
                                    </SOE:Text>
                                    <SOE:TextEntry
                                        ID="PlanningPeriodColorOver"
                                        runat="server"
                                        TermID="12175"
                                        DefaultTerm="Över (planerad för mycket)"
                                        CssClass="color"
                                        Indent="true"
                                        Width="50">
                                    </SOE:TextEntry>
                                    <SOE:TextEntry
                                        ID="PlanningPeriodColorEqual"
                                        runat="server"
                                        TermID="12176"
                                        DefaultTerm="Exakt (planerad exakt rätt)"
                                        CssClass="color"
                                        Indent="true"
                                        Width="50">
                                    </SOE:TextEntry>
                                    <SOE:TextEntry
                                        ID="PlanningPeriodColorUnder"
                                        runat="server"
                                        TermID="12177"
                                        DefaultTerm="Under (planerad för lite)"
                                        CssClass="color"
                                        Indent="true"
                                        Width="50">
                                    </SOE:TextEntry>
                                </table>
                                <table>
                                    <tr>
                                        <td>
                                            <SOE:InstructionList ID="PlanningPeriodColorInstructions"
                                                runat="server">
                                            </SOE:InstructionList>
                                        </td>
                                    </tr>
                                </table>
                            </fieldset>
                        </div>
                        <div id="DivDashboard" runat="server">
                            <fieldset>
                                <legend><%=GetText(3652, "Indikatorer")%></legend>
                                <SOE:InstructionList ID="InstructionsGaugeThresholds"
                                    runat="server">
                                </SOE:InstructionList>
                                <div class="col">
                                    <table>
                                        <SOE:TextEntry
                                            ID="GaugeSalesThreshold1"
                                            runat="server"
                                            TermID="3653"
                                            DefaultTerm="Försäljning"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeHoursThreshold1"
                                            runat="server"
                                            TermID="3654"
                                            DefaultTerm="Timmar"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeSalaryCostThreshold1"
                                            runat="server"
                                            TermID="3655"
                                            DefaultTerm="Lönekostnad"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeSalaryPercentThreshold1"
                                            runat="server"
                                            TermID="11757"
                                            DefaultTerm="Löneprocent"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeLPATThreshold1"
                                            runat="server"
                                            TermID="3657"
                                            DefaultTerm="LPAT"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeFPATThreshold1"
                                            runat="server"
                                            TermID="3656"
                                            DefaultTerm="FPAT"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeBPATThreshold1"
                                            runat="server"
                                            TermID="3660"
                                            DefaultTerm="BPAT"
                                            Visible="false"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                    </table>
                                </div>
                                <div>
                                    <table>
                                        <SOE:TextEntry
                                            ID="GaugeSalesThreshold2"
                                            runat="server"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeHoursThreshold2"
                                            runat="server"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeSalaryCostThreshold2"
                                            runat="server"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeSalaryPercentThreshold2"
                                            runat="server"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeLPATThreshold2"
                                            runat="server"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeFPATThreshold2"
                                            runat="server"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="GaugeBPATThreshold2"
                                            runat="server"
                                            Visible="false"
                                            Width="50"
                                            OnChange="validatePercent(this);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                    </table>
                                </div>
                            </fieldset>
                        </div>
                    </fieldset>
                    <fieldset>
                        <legend><%=GetText(3837, "Kompetenser")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="TimeNbrOfSkillLevels"
                                TermID="3838"
                                DefaultTerm="Antal kompetensnivåer (stjärnor)"
                                Width="60"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeSkillLevelHalfPrecision"
                                TermID="3839"
                                DefaultTerm="Dubbel noggrannhet (halva stjärnor)"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeSkillCantBeOverridden"
                                TermID="3974"
                                DefaultTerm="Kompetenser måste vara uppfyllda"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <tr>
                                <td colspan="2">
                                    <SOE:InstructionList ID="TimeSkillCantBeOverriddenInstruction"
                                        runat="server">
                                    </SOE:InstructionList>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                    <div id="DivStaffingNeeds" runat="server">
                        <fieldset>
                            <legend><%=GetText(3919, "Behov")%></legend>
                            <table>
                                <SOE:SelectEntry
                                    ID="StaffingNeedsChartType"
                                    TermID="3920"
                                    DefaultTerm="Standard graftyp"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="StaffingNeedsFrequencyAccountDim"
                                    TermID="12170"
                                    DefaultTerm="Undre kontodimension Frekvensdata"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="StaffingNeedsFrequencyParentAccountDim"
                                    TermID="12171"
                                    DefaultTerm="Övre kontodimension Frekvensdata"
                                    runat="server">
                                </SOE:SelectEntry>
                            </table>
                            <div id="DivStaffingNeedsRatios" runat="server">
                                <fieldset>
                                    <legend><%=GetText(3951, "Nyckeltal")%></legend>
                                    <table>
                                        <SOE:CheckBoxEntry
                                            ID="StaffingNeedsRatioSalesPerScheduledHour"
                                            TermID="3952"
                                            DefaultTerm="Försäljning per schemalagd timme"
                                            runat="server">
                                        </SOE:CheckBoxEntry>
                                        <SOE:CheckBoxEntry
                                            ID="StaffingNeedsRatioSalesPerWorkHour"
                                            TermID="3953"
                                            DefaultTerm="Försäljning per arbetad timme"
                                            runat="server">
                                        </SOE:CheckBoxEntry>
                                        <SOE:CheckBoxEntry
                                            ID="StaffingNeedsRatioFrequencyAverage"
                                            TermID="3954"
                                            DefaultTerm="Frekvenssnitt"
                                            runat="server">
                                        </SOE:CheckBoxEntry>
                                    </table>
                                </fieldset>
                            </div>
                            <div>
                                <fieldset>
                                    <legend><%=GetText(8514, "Skapa arbetspass")%></legend>
                                    <table>
                                        <SOE:TextEntry
                                            ID="StaffingNeedsWorkingPeriodMaxLength"
                                            runat="server"
                                            TermID="8515"
                                            DefaultTerm="Max längd för arbetspass"
                                            OnChange="formatTimeEmptyIfPossible(this, true);"
                                            OnFocus="this.select();">
                                        </SOE:TextEntry>
                                        <SOE:TextEntry
                                            ID="StaffingNeedRoundUp"
                                            runat="server"
                                            TermID="11527"
                                            DefaultTerm="Avrunda upp till nästa heltal vid (0,X)">
                                        </SOE:TextEntry>
                                    </table>
                                </fieldset>
                            </div>
                            <div>
                                <fieldset>
                                    <legend><%=GetText(11696, "Skapa tjänster")%></legend>
                                    <table>
                                        <SOE:TextEntry
                                            ID="EmployeePostPrefix"
                                            runat="server"
                                            TermID="11697"
                                            DefaultTerm="Prefix till tjänst namn">
                                        </SOE:TextEntry>
                                    </table>
                                </fieldset>
                            </div>
                        </fieldset>
                    </div>
                    <fieldset>
                        <legend><%=GetText(8431, "Godkänna ledighet")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="SetApprovedYesAsDefault"
                                TermID="8432"
                                DefaultTerm="Godkänn ska vara förvalt"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="OnlyNoReplacementIsElectable"
                                TermID="8433"
                                DefaultTerm="Endast Ingen Ersättare ska kunna väljas"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="IncludeNoteInMessages"
                                TermID="8844"
                                DefaultTerm="Inkludera notering i meddelanden"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ValidateVacationWholeDay"
                                TermID="10932"
                                DefaultTerm="Kontrollera att semester har lagts på alla pass på dagen vid sparning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <tr>
                                <td colspan="2">
                                    <SOE:InstructionList ID="ValidateVacationWholeDayInstructionList"
                                        runat="server">
                                    </SOE:InstructionList>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                    <fieldset>
                        <legend><%=GetText(3150, "Frånvaro")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="RemoveScheduleTypeOnAbsence"
                                TermID="12554"
                                DefaultTerm="Ta bort schematyp på pass som skapas för ersättare vid frånvaro"
                                runat="server">
                            </SOE:CheckBoxEntry>
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
                    <fieldset>
                        <legend><%=GetText(11973, "Semesterskuld kontering")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="VacationValueDaysCreditAccountId"
                                TermID="11974"
                                DefaultTerm="Upplupna semesterlöner"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="VacationValueDaysDebitAccountId"
                                TermID="11975"
                                DefaultTerm="Förändring av semesterskuld"
                                runat="server">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                    <fieldset>
                        <legend><%=GetText(12547, "Beräkningar")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="RecalculateFutureAccountingWhenChangingMainAllocation"
                                TermID="12548"
                                DefaultTerm="Räkna om kontering på schema och ev. utfall vid förändring av huvudtillhörighet"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>

                </div>
            </SOE:Tab>
            <SOE:Tab Type="Setting" TermID="3508" DefaultTerm="Inställningar automatattest" runat="server">
                <div>
                    <fieldset>
                        <legend><%=GetText(3509, "Automatattest")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="TimeAutoAttestRunService"
                                TermID="3510"
                                DefaultTerm="Kör automatiskt (med fördefinierat intervall)"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:SelectEntry
                                ID="TimeAutoAttestSourceAttestStateId"
                                TermID="3511"
                                DefaultTerm="Automatattest körs på transaktioner med attestnivå"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="TimeAutoAttestSourceAttestStateId2"
                                TermID="9089"
                                DefaultTerm="Automatattest körs även på transaktioner med attestnivå"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="TimeAutoAttestTargetAttestStateId"
                                TermID="3512"
                                DefaultTerm="Efter lyckad körning får transaktionerna attestnivå"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="TimeAutoAttestEmployeeManuallyAdjustedTimeStamps"
                                TermID="8595"
                                DefaultTerm="Automatattest körs även på dagar med justerade stämplingar"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <tr>
                                <td colspan="2">
                                    <SOE:InstructionList ID="EmployeeManuallyAdjustedTimeStampsInstructionList"
                                        runat="server">
                                    </SOE:InstructionList>
                                </td>
                            </tr>
                            <tr>
                            </tr>
                        </table>
                    </fieldset>
                </div>
            </SOE:Tab>
            <SOE:Tab Type="Setting" TermID="9122" DefaultTerm="Inställningar lön" runat="server">
                <div id="DivPayroll" runat="server">
                    <fieldset>
                        <legend><%=GetText(3071, "Lön")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="UsePayroll"
                                TermID="3072"
                                DefaultTerm="Aktivera lön"
                                OnClick="usePayrollClicked()"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:DateEntry
                                ID="UsedPayrollSince"
                                TermID="11511"
                                DefaultTerm="Lön aktiverat från"
                                runat="server">
                            </SOE:DateEntry>
                            <SOE:DateEntry
                                ID="CalculateExperienceFrom"
                                TermID="10260"
                                DefaultTerm="Räkna branschvana från datum"
                                runat="server">
                            </SOE:DateEntry>
                        </table>
                    </fieldset>
                </div>
                <div id="DivPayrollGroup" runat="server">
                    <fieldset>
                        <legend><%=GetText(9121, "Löneavtal")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="PayrollGroupMandatory"
                                TermID="10132"
                                DefaultTerm="Löneavtal obligatoriskt på anställd"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollAgreementUseOvertimeCompensation"
                                TermID="9123"
                                DefaultTerm="Använd övertidskompensation"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollAgreementUseExeption2to6"
                                TermID="9124"
                                DefaultTerm="Använd undantag från §§ 2-6 i arbetstidsavtalet"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollAgreementUseTravelCompensation"
                                TermID="9125"
                                DefaultTerm="Använd restidsersättning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollAgreementUseWorkTimeShiftCompensation"
                                TermID="9126"
                                DefaultTerm="Använd ersättning för förskjuten ordinarie arbetstid"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollAgreementUseVacationRightsDays"
                                TermID="9127"
                                DefaultTerm="Använd semesterrätt"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollAgreementUseGrossNetTimeInStaffing"
                                TermID="5929"
                                DefaultTerm="Använd brutto-/nettotid i planering"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollAgreementUsePayrollTax"
                                TermID="3271"
                                DefaultTerm="Använd egenavgift"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
                <div id="DivPayrollEmploymentTypes" runat="server">
                    <fieldset>
                        <legend><%=GetText(5910, "Anställningsformer")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_Probationary"
                                TermID="5900"
                                DefaultTerm="Använd provanställning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_Substitute"
                                TermID="5901"
                                DefaultTerm="Använd vikariat"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_SubstituteVacation"
                                TermID="5915"
                                DefaultTerm="Använd semestervikariat"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_Permanent"
                                TermID="5902"
                                DefaultTerm="Använd tillsvidareanställning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_FixedTerm"
                                TermID="5903"
                                DefaultTerm="Använd allmän visstidsanställning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_SpecialFixedTerm"
                                TermID="12157"
                                DefaultTerm="Använd särskild visstidsanställning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_Seasonal"
                                TermID="5904"
                                DefaultTerm="Använd säsongsarbete"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_SpecificWork"
                                TermID="5905"
                                DefaultTerm="Använd visst arbete"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_Trainee"
                                TermID="5906"
                                DefaultTerm="Använd praktikantanställning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_NormalRetirementAge"
                                TermID="5907"
                                DefaultTerm="Använd tjänsteman som uppnått den ordinarie pensionsåldern enligt ITP-planen"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_CallContract"
                                TermID="5908"
                                DefaultTerm="Använd behovsanställning"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_LimitedAfterRetirementAge"
                                TermID="5909"
                                DefaultTerm="Använd tidsbegränsad anställning för personer fyllda 69 år (enligt lag)"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_FixedTerm14days"
                                TermID="11526"
                                DefaultTerm="Använd allmän visstidsanställning 14 dagar"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="PayrollEmploymentTypeUse_SE_Apprentice"
                                TermID="11882"
                                DefaultTerm="Lärling"
                                runat="server">
                            </SOE:CheckBoxEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(10004, "Attestnivåer")%></legend>
                        <table>
                            <SOE:SelectEntry
                                runat="server"
                                ID="PayrollCalculationLockedStatus"
                                DefaultTerm="Status efter löneberäkning låst"
                                TermID="10000">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                runat="server"
                                ID="PayrollCalculationApproved1Status"
                                DefaultTerm="Status efter lön godkänd, nivå 1"
                                TermID="10001">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                runat="server"
                                ID="PayrollCalculationApproved2Status"
                                DefaultTerm="Status efter lön godkänd, nivå 2"
                                TermID="10002">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                runat="server"
                                ID="PayrollCalculationPaymentFileCreated"
                                DefaultTerm="Status efter utbetalningsfil skapats"
                                TermID="10003">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(8599, "Löneutbetalning")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="SalaryPaymentExportType"
                                DefaultTerm="Utbetalningsformat"
                                TermID="8600"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:TextEntry
                                ID="SalaryPaymentExportSenderIdentification"
                                DefaultTerm="Kundnummer (SUS/BGC KI/NORDEA/ISO20022)"
                                TermID="8608"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="SalaryPaymentExportDivisionName"
                                DefaultTerm="Divisions namn (ISO20022)"
                                TermID="8871"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="SalaryPaymentExportAgreementNumber"
                                DefaultTerm="Avtalsnummer (ISO20022)"
                                TermID="8856"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="SalaryPaymentExportSenderBankGiro"
                                DefaultTerm="Bankgiro (BGC KI)"
                                TermID="8609"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:CheckBoxEntry
                                ID="SalaryPaymentExportCompanyIsRegisterHolder"
                                TermID="8610"
                                DefaultTerm="Företaget är registerhållare (SUS)"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <%-- <SOE:SelectEntry
				                ID="SalaryPaymentExportBank"
				                DefaultTerm="Bank (ISO20022)"
				                TermID="9964"
                                runat="server">
				            </SOE:SelectEntry>--%>
                            <SOE:SelectEntry
                                ID="SalaryPaymentExportPaymentAccount"
                                DefaultTerm="Utbetalningskonto (ISO20022)"
                                TermID="9967"
                                runat="server">
                            </SOE:SelectEntry>

                            <%-- <SOE:CheckBoxEntry
                                ID="SalaryPaymentExportUseAccountNrAsBBAN" 
                                TermID="9965"
                                DefaultTerm="Använd kontonr i BBAN fältet (ISO20022)"
                                runat="server">
                            </SOE:CheckBoxEntry>  --%>
                            <SOE:CheckBoxEntry
                                ID="SalaryPaymentExportUsePaymentDateAsExecutionDate"
                                TermID="9966"
                                DefaultTerm="Använd execution date som den dag som lönen når mottagaren (ISO20022)"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="SalaryPaymentExportUseIBANOnEmployee"
                                TermID="10140"
                                DefaultTerm="Använd BIC/IBAN från anställd (ISO20022)"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="SalaryPaymentExportUseExtendedCurrencyNOK"
                                TermID="9348"
                                DefaultTerm="Skapa bankfil - Utökat urval för Norsk valuta"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:TextEntry
                                ID="SalaryPaymentExportExtendedAgreementNumber"
                                DefaultTerm="Utökat avtalsnummer"
                                TermID="9349"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="SalaryPaymentExportExtendedSenderIdentification"
                                DefaultTerm="Utökat kundnummer"
                                TermID="9350"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:SelectEntry
                                ID="SalaryPaymentExportExtendedPaymentAccount"
                                DefaultTerm="Utökat utbetalningskonto"
                                TermID="9351"
                                runat="server">
                            </SOE:SelectEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(8598, "Löneexport")%></legend>
                        <table>
                            <SOE:SelectEntry
                                ID="ExportTarget"
                                DefaultTerm="Exportformat"
                                TermID="4496"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:TextEntry
                                ID="ExternalExportID"
                                DefaultTerm="Externt exportid"
                                TermID="8035"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="ExternalExportSubId"
                                DefaultTerm="Externt exportid 2"
                                TermID="12531"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="SalaryExportEmail"
                                DefaultTerm="E-post till löneadministratör"
                                TermID="9101"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:CheckBoxEntry
                                ID="SalaryExportNoComments"
                                TermID="8548"
                                DefaultTerm="Ta inte med kommentarer i exportfilen"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:TextEntry
                                ID="SalaryExportEmailCopy"
                                DefaultTerm="E-post att skicka kopia till"
                                TermID="7178"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:SelectEntry
                                ID="SalaryExportUseSocSecFormat"
                                DefaultTerm="Ersätt anställningsnummer med personnummer (format)"
                                TermID="12085"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="ExportVatProductId"
                                DefaultTerm="Löneart vid moms"
                                TermID="9076"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="SalaryExportLockPeriod"
                                TermID="9975"
                                DefaultTerm="Tillåt ej förändringar efter löneexport"
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="SalaryExportAllowPreliminary"
                                TermID="10139"
                                DefaultTerm="Tillåt preliminär löneexport"
                                runat="server">
                            </SOE:CheckBoxEntry>


                        </table>
                    </fieldset>
                </div>
                <div id="DivPensionAndreports">
                    <fieldset>
                        <legend><%=GetText(5997, "Pension och rapporter")%></legend>
                        <table>
                            <tr>
                                <td>
                                    <div id="DivPension" runat="server">
                                        <fieldset>
                                            <legend><%=GetText(4772, "Pension")%></legend>
                                            <table>
                                                <SOE:TextEntry
                                                    ID="ForaAgreementNumber"
                                                    TermID="5994"
                                                    DefaultTerm="Fora Avtalsnummer"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="ITP1Number"
                                                    TermID="5995"
                                                    DefaultTerm="ITP1 - Nummer"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="ITP2Number"
                                                    TermID="5996"
                                                    DefaultTerm="ITP2 - Nummer"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="KPAAgreementNumber"
                                                    TermID="4773"
                                                    DefaultTerm="KPA Avtalsnummer"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="KPAManagementNumber"
                                                    TermID="11953"
                                                    DefaultTerm="KPA Förvaltningsnummer"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="SkandiaSortingConcept"
                                                    TermID="8947"
                                                    DefaultTerm="Skandia sorteringsbegrepp"
                                                    runat="server">
                                                </SOE:TextEntry>
                                            </table>
                                        </fieldset>
                                    </div>
                                </td>
                                <td>
                                    <div id="DivSNKFO" runat="server">
                                        <fieldset>
                                            <legend><%=GetText(4774, "SN/KFO")%></legend>
                                            <table>
                                                <SOE:TextEntry
                                                    ID="SNKFOMemberNumber"
                                                    TermID="4775"
                                                    DefaultTerm="Medlemsnummer"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="SNKFOWorkPlaceNumber"
                                                    TermID="4776"
                                                    DefaultTerm="Arbetsplatsnummer"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="SNKFOAffiliateNumber"
                                                    TermID="4777"
                                                    DefaultTerm="Förbundsnummer"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="SNKFOAgreementNumber"
                                                    TermID="4778"
                                                    DefaultTerm="Avtalskod"
                                                    runat="server">
                                                </SOE:TextEntry>
                                            </table>
                                        </fieldset>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <div id="DivBygg" runat="server">
                                        <fieldset>
                                            <legend><%=GetText(4779, "Bygglösen")%></legend>
                                            <table>
                                                <SOE:SelectEntry
                                                    ID="CommunityCode"
                                                    TermID="4780"
                                                    DefaultTerm="Kommunkod"
                                                    runat="server">
                                                </SOE:SelectEntry>
                                            </table>
                                        </fieldset>
                                    </div>
                                </td>
                                <td>
                                    <div id="DivSCB" runat="server">
                                        <fieldset>
                                            <legend><%=GetText(4781, "SCB")%></legend>
                                            <table>
                                                <SOE:TextEntry
                                                    ID="SCBWorkSite"
                                                    TermID="4782"
                                                    DefaultTerm="Arbetställe"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="CFARNumber"
                                                    TermID="4783"
                                                    DefaultTerm="CFAR-Nummer"
                                                    runat="server">
                                                </SOE:TextEntry>
                                            </table>
                                        </fieldset>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <div id="ArbetsgivarintygnuDiv" runat="server">
                                        <fieldset>
                                            <legend>Arbetsgivarintyg.nu</legend>
                                            <table>
                                                <SOE:TextEntry
                                                    ID="Apinyckel"
                                                    TermID="9999"
                                                    DefaultTerm="Apinyckel"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="ArbetsgivarId"
                                                    TermID="91897"
                                                    DefaultTerm="ArbetsgivarId"
                                                    runat="server">
                                                </SOE:TextEntry>
                                            </table>
                                        </fieldset>
                                    </div>
                                </td>
                                <td>
                                    <div id="DivFolksam" runat="server">
                                        <fieldset>
                                            <legend><%=GetText(12084, "Folksam")%></legend>
                                            <table>
                                                <SOE:NumericEntry
                                                    ID="FolksamCustomerNumber"
                                                    TermID="1772"
                                                    DefaultTerm="Kundnummer"
                                                    MaxLength="6"
                                                    Width="50"
                                                    AllowDecimals="false"
                                                    AllowNegative="false"
                                                    runat="server">
                                                </SOE:NumericEntry>
                                            </table>
                                        </fieldset>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <div id="DivSkatteverket" runat="server">
                                        <fieldset>
                                            <legend><%=GetText(10025, "Skatteverket")%></legend>
                                            <table>
                                                <SOE:TextEntry
                                                    ID="PlaceOfEmploymentAddress"
                                                    TermID="8860"
                                                    DefaultTerm="Tjänsteställe adress"
                                                    runat="server">
                                                </SOE:TextEntry>
                                                <SOE:TextEntry
                                                    ID="PlaceOfEmploymentCity"
                                                    TermID="8861"
                                                    DefaultTerm="Tjänsteställe ort"
                                                    runat="server">
                                                </SOE:TextEntry>
                                            </table>
                                        </fieldset>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                </div>
                <div id="DivPayrollSupportArea" runat="server">
                    <fieldset>
                        <legend><%=GetText(3074, "Stödområde")%></legend>
                        <table>
                            <SOE:TextEntry
                                ID="PayrollMaxRegionalSupportAmount"
                                TermID="3075"
                                DefaultTerm="Max regionalstöd (belopp)"
                                Width="100"
                                LabelWidth="200"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="PayrollMaxRegionalSupportPercent"
                                TermID="3076"
                                DefaultTerm="Max regionalstöd (%)"
                                MaxLength="3"
                                Width="40"
                                LabelWidth="200"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="PayrollMaxRegionalSupportAccountDim1"
                                OnChange="accountSearch.searchField('PayrollMaxRegionalSupportAccountDim1')"
                                OnKeyUp="accountSearch.keydown('PayrollMaxRegionalSupportAccountDim1')"
                                Width="100"
                                runat="server">
                            </SOE:TextEntry>
                        </table>
                        <table id="PayrollMaxRegionalSupportAccountTable" runat="server"></table>
                        <table>
                            <SOE:TextEntry
                                ID="PayrollMaxResearchSupportAmount"
                                TermID="3077"
                                DefaultTerm="Max forskarstöd (belopp)"
                                Width="100"
                                LabelWidth="200"
                                runat="server">
                            </SOE:TextEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(8624, "Provision")%></legend>
                        <div class="col">
                            <table>
                                <SOE:SelectEntry
                                    ID="AccountProvisionTimeCode"
                                    TermID="8623"
                                    DefaultTerm="Tidkod för service provision"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="AccountProvisionAccountDim"
                                    TermID="8627"
                                    DefaultTerm="Kontodimension för provisionsunderlag"
                                    runat="server">
                                </SOE:SelectEntry>
                            </table>
                        </div>
                    </fieldset>
                </div>
                <div>
                    <fieldset>
                        <legend><%=GetText(8620, "Övriga inställningar")%></legend>
                        <div class="col">
                            <table>
                                <SOE:SelectEntry
                                    ID="AccountingDistributionPayrollProduct"
                                    TermID="8621"
                                    DefaultTerm="Fördelning av kontering efter löneart"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
                                    ID="GrossalaryRoundingPayrollProduct"
                                    TermID="8941"
                                    DefaultTerm="Löneart för avrundning av den totala bruttolönen"
                                    runat="server">
                                </SOE:SelectEntry>
                                <tr>
                                    <td colspan="2">
                                        <SOE:InstructionList ID="GrossalaryRoundingPayrollProductInstructionList"
                                            runat="server">
                                        </SOE:InstructionList>
                                    </td>
                                </tr>
                                <SOE:CheckBoxEntry
                                    ID="PublishPayrollSlipWhenLockingPeriod"
                                    TermID="8829"
                                    DefaultTerm="Publicera lönespecifikation i samband med låsning av period"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry
                                    ID="SendNoticeWhenPayrollSlipPublished"
                                    TermID="8863"
                                    DefaultTerm="Skicka meddelande vid publicering av lönespecifikation"
                                    runat="server">
                                </SOE:CheckBoxEntry>
                            </table>
                        </div>
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
