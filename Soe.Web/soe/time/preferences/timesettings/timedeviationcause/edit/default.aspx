<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.preferences.timesettings.timedeviationcause.edit._default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
        <tabs>
			<SOE:Tab Type="Edit" runat="server">		        
			    <fieldset> 
				    <legend><%=GetText(4314, "Avvikelseorsaksuppgifter")%></legend>
		            <table>
			            <SOE:TextEntry
			                ID="Name"
				            TermID="23" DefaultTerm="Namn"
                            Validation="Required"
				            InvalidAlertTermID="2088" InvalidAlertDefaultTerm="Du måste ange ett namn"
				            MaxLength="100"
				            runat="server">
			            </SOE:TextEntry>
			            <SOE:TextEntry
			                ID="Description"
				            TermID="1461" DefaultTerm="Beskrivning"
				            MaxLength="512"
				            runat="server">
			            </SOE:TextEntry>
			            <SOE:TextEntry
			                ID="ExtCode"
				            TermID="3559" DefaultTerm="Extern kod"
				            MaxLength="50"
				            runat="server">
			            </SOE:TextEntry>
			            <SOE:TextEntry
			                ID="ImageSource"
				            TermID="3367" DefaultTerm="Ikon"
				            MaxLength="100"
				            runat="server">
			            </SOE:TextEntry>
			            <SOE:SelectEntry
			                ID="Type"
			                TermID="4505"
			                DefaultTerm="Orsakstyp"
			                MaxLength="100"
			                runat="server">
			            </SOE:SelectEntry>			            
			            <SOE:SelectEntry
			                ID="ResultingTimeCode"
			                TermID="4509"
			                DefaultTerm="Orsak ger tidkod"
			                MaxLength="100"
			                runat="server">
			            </SOE:SelectEntry>
                        <SOE:NumericEntry
						        ID="EmployeeRequestPolicyNbrOfDaysBefore"						        
						        TermID="8787" 
                                DefaultTerm="Ledigheten ska sökas senast XX dagar före"						       
						        MaxLength="3"
						        runat="server">
					    </SOE:NumericEntry>
                        <SOE:CheckBoxEntry
                            ID="EmployeeRequestPolicyNbrOfDaysBeforeCanNotOverride"
                            TermID="8788"
                            DefaultTerm="Ansökan ska inte kunna skickas in om den infaller efter inställning ovan"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>
                        <SOE:CheckBoxEntry
                            ID="OnlyWholeDay"
                            TermID="8057"
                            DefaultTerm="Endast heldag"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>                            
                        <SOE:CheckBoxEntry
                            ID="ShowZeroDaysInAbsencePlanning"
                            TermID="8470"
                            DefaultTerm="Visa lediga dagar i frånvaroplaneringen"
                            Width="20"   
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>  
                        <SOE:NumericEntry
                            ID="AttachZeroDaysNbrOfDaysBefore"
                            TermID="11790"
                            DefaultTerm="Kontrollera och lägg till antal lediga dagar före period"
                            Width="20"   
                            Border="0"
                            runat="server">
                        </SOE:NumericEntry>
                        <SOE:NumericEntry
                            ID="AttachZeroDaysNbrOfDaysAfter"
                            TermID="11791"
                            DefaultTerm="Kontrollera och lägg till antal lediga dagar efter period"
                            Width="20"   
                            Border="0"
                            runat="server">
                        </SOE:NumericEntry>
                        <SOE:CheckBoxEntry
                            ID="ChangeDeviationCauseAccordingToPlannedAbsence"
                            TermID="11817"
                            DefaultTerm="Använd avvikelseorsak från planerad frånvaro"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>                        
                       <SOE:NumericEntry
                            ID="ChangeCauseOutsideOfPlannedAbsence"
                            TermID="11818"
                            DefaultTerm="Justera standardorsak om tid utanför planerad frånvaro understiger"
                            Width="20"   
                            Border="0"
                            runat="server">
                        </SOE:NumericEntry>
                       <SOE:NumericEntry
                            ID="ChangeCauseInsideOfPlannedAbsence"
                            TermID="11819"
                            DefaultTerm="Justera standardorsak om tid inom planerad frånvaro understiger"
                            Width="20"   
                            Border="0"
                            runat="server">
                        </SOE:NumericEntry>
                       <SOE:NumericEntry
                            ID="AdjustTimeOutsideOfPlannedAbsence"
                            TermID="11820"
                            DefaultTerm="Justera stämplingstid till planerad tid om tid utanför planerad frånvaro understiger"
                            Width="20"   
                            Border="0"
                            runat="server">
                        </SOE:NumericEntry>
                       <SOE:NumericEntry
                            ID="AdjustTimeInsideOfPlannedAbsence"
                            TermID="11821"
                            DefaultTerm="Justera stämplingstid till planerad tid om tid inom planerad frånvaro understiger"
                            Width="20"   
                            Border="0"
                            runat="server">
                        </SOE:NumericEntry>
                        <SOE:CheckBoxEntry
                            ID="AllowGapToPlannedAbsence"
                            TermID="11822"
                            DefaultTerm="Skapa hål mellan planerad frånvaro och sista eller första stämplingen"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry> 
                        <SOE:CheckBoxEntry
                            ID="IsVacation"
                            TermID="3301"
                            DefaultTerm="Hanteras som semester"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>                            
                        <SOE:CheckBoxEntry
                            ID="SpecifyChild"
                            TermID="8533"
                            DefaultTerm="Ange barn"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>
                        <SOE:CheckBoxEntry
                            ID="Payed"
                            TermID="5765"
                            DefaultTerm="Betald tid"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>
                        <SOE:CheckBoxEntry
                            ID="NotChargeable"
                            TermID="7440"
                            DefaultTerm="Ej debiterbar"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>
                        <SOE:CheckBoxEntry
                            ID="ValidForStandby"
                            TermID="11996"
                            DefaultTerm="Kan användas under beredskap"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>
                         <SOE:CheckBoxEntry
                            ID="MandatoryNote"
                            TermID="10257"
                            DefaultTerm="Tvingande notering (endast avvikelserapportering)"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>
                        <SOE:CheckBoxEntry
                            ID="MandatoryTime"
                            TermID="7526"
                            DefaultTerm="Ange klockslag utanför schema"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>
                        <SOE:CheckBoxEntry
                            ID="ExcludeFromPresenceWorkRules"
                            TermID="9963"
                            DefaultTerm="Ingår ej i kontroll av dygnsvila/veckovila på närvaro enligt tidavtalet"
                            Width="20"
                            Border="0"
                            runat="server">
                        </SOE:CheckBoxEntry>
                    </table>
                </fieldset>                
			</SOE:Tab>
        </tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
