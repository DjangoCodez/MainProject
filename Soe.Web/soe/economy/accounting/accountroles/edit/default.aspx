<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.accounting.accountroles.edit._default" %>
<%@ Register Src="~/UserControls/Translations.ascx" TagPrefix="SOE" TagName="Translations" %>
<asp:Content ID="Content2" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
				<div>
					<fieldset>
						<legend><%=legendLabel%></legend>
						<table>
							<SOE:NumericEntry 
							    ID="AccountDimNr"
							    TermID="1070" DefaultTerm="Nummer" 
							    Validation="Required"
							    InvalidAlertTermID="1089" InvalidAlertDefaultTerm="Du måste ange nummer för konteringsnivån"
							    MaxLength="10" 
							    runat="server">
							</SOE:NumericEntry>
							<SOE:TextEntry 
							    ID="Name" 
							    TermID="1071" DefaultTerm="Namn" 
							    Validation="Required"
							    InvalidAlertTermID="1091" InvalidAlertDefaultTerm="Du måste ange namn"
							    MaxLength="100" 
							    runat="server">
							 </SOE:TextEntry>
							<SOE:TextEntry 
							    ID="ShortName" 
							    TermID="1072" DefaultTerm="Kortnamn" 
							    MaxLength="10" 
							    runat="server">
							</SOE:TextEntry>
                            <SOE:SelectEntry 
							    ID="ExternalAccounting" 
							    TermID="8045" DefaultTerm="Grundkontoplan" 
                                Visible = "false"                                
                                Width = "220"
							    runat="server">
							</SOE:SelectEntry>
		                    <SOE:SelectEntry 
					            ID="MinChar" 
					            TermID="1223" 
					            DefaultTerm="Minimum längd"
					            runat="server">
					        </SOE:SelectEntry>	
		                    <SOE:SelectEntry 
					            ID="MaxChar" 
					            TermID="1224" 
					            DefaultTerm="Max längd"
					            runat="server">
					        </SOE:SelectEntry>	
                            <SOE:CheckBoxEntry
                                ID="LinkedToProject" 
                                TermID="3353"
                                DefaultTerm="Länkad till projekt" 
                                runat="server">
                            </SOE:CheckBoxEntry>
                            <SOE:InstructionList
                                ID="LinkedToProjectExplanation"
                                runat="server">
                            </SOE:InstructionList>
						</table>
					</fieldset>
					<div id="DivSie" runat="server">
					    <fieldset>
						    <legend><%=GetText(1673, "SIE mappning")%></legend>
						    <table> 
							    <SOE:NumericSelectEntry
							        ID="SysSieDim"
							        TermID="1181" 
					                DefaultTerm="SIE dimension"
					                NumericMaxLength="2"
					                NumericWidth="30"
                                    SelectWidth="200"
							        runat="server">
							    </SOE:NumericSelectEntry>
						    </table>
				        </fieldset>
					</div>
                    <div>
                        <SOE:Translations ID="Translations" Runat="Server"></SOE:Translations>
                    </div>
				</div>
			</SOE:Tab>
		</Tabs>
	</SOE:Form>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
