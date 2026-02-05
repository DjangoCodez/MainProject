<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.preferences.usersettings._default" %>

<asp:content id="Content1" contentplaceholderid="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
        <tabs>
		    <SOE:Tab Type="Setting" TermID="5414" DefaultTerm="Användarinställningar" runat="server">
                <div>
				    <fieldset>
					    <legend><%=GetText(3782, "Artikelrader")%></legend>
					    <table>
                            <SOE:SelectEntry
                                ID="ProductSearchFilterMode" 
                                TermID="3783"
                                DefaultTerm="Sökning i artikellista"
                                Width="120"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:NumericEntry
                                ID="ProductSearchMinPrefixLength" 
                                TermID="3784"
                                DefaultTerm="Antal tecken innan artikellista visas" 
                                MaxLength="2"
                                AllowDecimals="false"
                                Width="50"
                                runat="server">
                            </SOE:NumericEntry>
                            <SOE:NumericEntry
                                ID="ProductSearchMinPopulateDelay" 
                                TermID="3785"
                                DefaultTerm="Antal millisekunder innan artikellista visas" 
                                MaxLength="4"
                                AllowDecimals="false"
                                Width="50"
                                runat="server">
                            </SOE:NumericEntry>
                            <tr />
                            <SOE:CheckBoxEntry
                                ID="DisableWarningPopups"
                                TermID="7050"
                                DefaultTerm="Inaktivera varningar" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="ShowWarningBeforeInvoiceRowDeletion"
                                TermID="4592"
                                DefaultTerm="Visa kontrollfråga innan artikelraden tas bort" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:CheckBoxEntry
                                ID="HideIncomeRatioAndPercentage"
                                TermID="4732"
                                DefaultTerm="Visa inte intäktsrelation och procentandel" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                             <SOE:SelectEntry
	                            ID="DefaultStockPlace"
	                            DisableSettings="true"
                                Width="250"
	                            TermID="4617" DefaultTerm="Standard lager"
	                            runat="server">
                            </SOE:SelectEntry> 
					    </table>
				    </fieldset>
			    </div>
                <div>
				    <fieldset>
					    <legend><%=GetText(1833, "Försäljning")%></legend>
                        <div class="col">
					        <table>
                                 <SOE:SelectEntry
	                                ID="OurReference"
	                                DisableSettings="true"
                                    Width="250"
	                                TermID="3291" DefaultTerm="Vår referens"
	                                runat="server">
                                </SOE:SelectEntry>
                                <SOE:SelectEntry
	                                ID="DefaultOrderType"
	                                DisableSettings="true"
                                    Width="250"
	                                TermID="9333" DefaultTerm="Standard ordertyp"
	                                runat="server">
                                </SOE:SelectEntry>
                                 <SOE:CheckBoxEntry
                                ID="UseCashCustomerAsDefault"
                                TermID="7516"
                                DefaultTerm="Använd kontantkund som standard" 
                                runat="server">
                            </SOE:CheckBoxEntry>
					        </table>
                        </div>
				    </fieldset>
			    </div>
                <div id="DivOrderPlanning" runat="server">
					<fieldset>
						<legend><%=GetText(3967, "Orderplanering")%></legend>
						<table>
						    <SOE:SelectEntry
							    ID="OrderPlanningDefaultView"
							    TermID="3575" 
							    DefaultTerm="Standardvy" 
							    runat="server">
						    </SOE:SelectEntry>
						    <SOE:SelectEntry
							    ID="OrderPlanningDefaultInterval"
							    TermID="3408" 
							    DefaultTerm="Standardintervall för veckovy" 
							    runat="server">
						    </SOE:SelectEntry>
                        </table>
                        <fieldset>
						    <legend><%=GetText(4325, "Utseende uppdrag i planeringsvy")%></legend>
						    <table>
                                <tr>
                                    <td colspan="2">
                                        <SOE:Instruction ID="OrderPlanningShiftInfoTopLeftInfo"
                                            TermID="4330" DefaultTerm="I övre vänstra hörnet visas alltid uppdragets klockslag."
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Instruction>
                                    </td>
                                </tr>
						        <SOE:SelectEntry
							        ID="OrderPlanningShiftInfoTopRight"
							        TermID="4326" 
							        DefaultTerm="Information i övre högra hörnet"
							        runat="server">
						        </SOE:SelectEntry>
						        <SOE:SelectEntry
							        ID="OrderPlanningShiftInfoBottomLeft"
							        TermID="4327" 
							        DefaultTerm="Information i nedre vänstra hörnet" 
							        runat="server">
						        </SOE:SelectEntry>
						        <SOE:SelectEntry
							        ID="OrderPlanningShiftInfoBottomRight"
							        TermID="4328" 
							        DefaultTerm="Information i nedre högra hörnet" 
							        runat="server">
						        </SOE:SelectEntry>
                                <tr>
                                    <td colspan="3">
                                        <SOE:Instruction ID="OrderPlanningShiftInfoRightListInfo"
                                            TermID="4329" DefaultTerm="Ovanstående inställningar styr även vad som visas i högerlistan med 'Ej schemalagda ordrar'. Dock är det alltid en fast placering av informationen där."
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Instruction>
                                    </td>
                                </tr>
                            </table>
                        </fieldset>
                    </fieldset>
				</div>
		    </SOE:Tab>		
		</tabs>
    </SOE:Form>
</asp:content>
<asp:content id="Content2" contentplaceholderid="soeLeftContent" runat="server">
</asp:content>
