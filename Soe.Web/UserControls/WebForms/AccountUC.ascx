<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccountUC.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.WebForms.AccountUC" %>
<%@ Register Src="~/UserControls/CategoryAccounts.ascx" TagPrefix="SOE" TagName="CategoryAccounts" %>
<div class="SoeTabView SoeForm">
                <div id="DivLinkedToProjectInstruction" visible="false" runat="server">
                    <fieldset>
					    <legend><%= PageBase.GetText(3353, "Länkad till projekt") %></legend>
				        <table>
                            <SOE:InstructionList
                                ID="LinkedToProjectInstruction"
                                runat="server">
                            </SOE:InstructionList>
                        </table>
				    </fieldset>
                </div>
				<div id="DivAccount" runat="server">
                    <fieldset>
					    <legend><%= PageBase.GetText(2068, "Generellt") %></legend>
                        <div class="col">
				            <table>
                                <SOE:CheckBoxEntry
                                    ID="Active" 
                                    TermID="1990"
                                    DefaultTerm="Aktivt"
                                    runat="server">
                                </SOE:CheckBoxEntry>
					            <SOE:NumericEntry
						            ID="AccountNr"
						            TermID="1038" DefaultTerm="Kontonr"
						            Validation="Required"
						            InvalidAlertTermID="1039" InvalidAlertDefaultTerm="Du måste ange kontonr"
						            MaxLength="50"
						            runat="server">
					            </SOE:NumericEntry>
					            <SOE:TextEntry
						            ID="ObjectCode"
						            TermID="1122" DefaultTerm="Objektkod"
						            Validation="Required"
						            InvalidAlertTermID="1123" InvalidAlertDefaultTerm="Du måste ange objektkod"
						            MaxLength="50"
						            runat="server">
					            </SOE:TextEntry>
					            <SOE:TextEntry 
						            ID="Name"
						            TermID="1040" DefaultTerm="Namn"
						            Validation="Required"
						            InvalidAlertTermID="1041" InvalidAlertDefaultTerm="Du måste ange namn"
						            MaxLength="100"
						            Width="194"
						            runat="server">
					            </SOE:TextEntry>	
                            </table>
                        </div>
                        <div>
                            <table>
					            <tbody id="GeneralAccountStd" runat="server"> 
					                <SOE:SelectEntry 
					                    ID="AccountType"
					                    TermID="1044" DefaultTerm="Kontotyp"
    						            Width="200"
					                    runat="server">
					                </SOE:SelectEntry>		
					                <SOE:SelectEntry 
					                    ID="VatAccount" 
					                    TermID="2067" 
					                    DefaultTerm="Momsredovisning"
    						            Width="200"
					                    runat="server">
					                </SOE:SelectEntry>	
                                    <SOE:TextEntry 
						                ID="VatAccountRate"
					                    TermID="3102" 
					                    DefaultTerm="Momssats"
						                MaxLength="3"
						                ReadOnly="true"
						                Border="0"
						                runat="server">
					                </SOE:TextEntry>
					            </tbody>
                            </table>
                        </div>
				    </fieldset>
                </div>
                <div id="DivAccountStd" runat="server">  
	                <div id="DivAccountMapping" runat="server">
	                    <fieldset>   
					        <legend><%= PageBase.GetText(2073, "Internkonton")%></legend>
				            <table id="TableAccountMapping" runat="Server">				        
				            </table>
	                    </fieldset>    
	                </div>
                    <div>
                        <fieldset>   
				            <legend><%= PageBase.GetText(1264, "SRU-koder enligt blankett") %></legend>
				            <table>			
                                <SOE:SelectEntry 
			                        ID="AccountSru1" 
			                        TermID="1265" DefaultTerm="SRU-kod 1"
            						Width="200"
                                    runat="server">
                                </SOE:SelectEntry> 	  
                                <SOE:SelectEntry 
			                        ID="AccountSru2" 
			                        TermID="1266" DefaultTerm="SRU-kod 2"
            						Width="200"
                                    runat="server">
                                </SOE:SelectEntry> 	  
				            </table>
	                    </fieldset> 
                    </div>
                    <div>
                        <fieldset> 
	                        <legend> <%= PageBase.GetText(2072, "Styrning Verifikatregistrering")%> </legend>
	                        <table>
	                            <SOE:SelectEntry 
					                ID="AmountStop" 
					                TermID="2069" DefaultTerm="Stanna i"
                                    runat="server">
                                </SOE:SelectEntry>
                                <SOE:TextEntry 
						            ID="Unit"						        
						            TermID="2070" DefaultTerm="Enhet kvantitet"			        
						            MaxLength="10"
						            runat="server">
                                </SOE:TextEntry>	
                                <SOE:CheckBoxEntry 
	                                ID="UnitStop" 
	                                TermID="2071" DefaultTerm="Stanna i kvantitet" 
	                                runat="server">
                                </SOE:CheckBoxEntry>
                            </table>
	                    </fieldset>
                    </div>
	                <div id="DivBalance" runat="server">
                        <fieldset> 
	                        <legend><%=PageBase.GetText(3048, "Saldon")%></legend>
	                        <table id="TableBalance" runat="server">						    	
	                        </table>
	                        <br />
	                        <table id="UpdateBalance" runat="server">
	                            <tr>
	                                <td>
        			                    <button name="CalcBalance" id="CalcBalance" type="submit"><%=PageBase.GetText(3049, "Räkna om saldon")%></button>
    			                    </td>
	                                <td>
						                <label id="CalcAllAccountsLabel" for="CalcAllAccounts"><%=PageBase.GetText(3051, "För alla konton")%></label>
	                                </td>
	                                <td>
                                        <SOE:CheckBoxEntry 
                                            ID="CalcAllAccounts"
                                            HideLabel="true"
                                            FitInTable="true"
                                            DisableSettings="true"
                                            Value="false"
                                            runat="server">
                                        </SOE:CheckBoxEntry>
        			                </td>
	                            </tr>
	                            <tr>
	                                <td colspan="3">
                                        <SOE:Instruction ID="Instruction1"
                                            TermID="3052" DefaultTerm="OBS! Att räkna om alla saldon kan ta några minuter"
                                            FitInTable="true"
                                            runat="server">
                                        </SOE:Instruction>
	                                </td>
                                </tr>
	                        </table>
                        </fieldset>
	                </div>
			    </div>
                <div id="DivAccountInternal" runat="server">
			        <fieldset> 
				        <legend><%=PageBase.GetText(3407, "Inställningar")%></legend>
		                <table>
                         <SOE:CategoryAccounts ID="AccountInternalCategories" Runat="Server"></SOE:CategoryAccounts>
                        </table>
                    </fieldset>
                </div>
    </div>