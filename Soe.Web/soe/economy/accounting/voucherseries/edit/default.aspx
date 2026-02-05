<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.accounting.voucherseries.edit._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
				<div>
					<fieldset> 
						<legend><%=GetText(2080, "Verifikatserie")%></legend>
						<table>							
					        <SOE:NumericEntry
						        ID="VoucherSerieTypeNr"						        
						        TermID="2075" DefaultTerm="SerieNr"						        
						        Validation="Required"
						        InvalidAlertTermID="1666" InvalidAlertDefaultTerm="Du måste ange serienr"
						        MaxLength="3"
						        runat="server">
					        </SOE:NumericEntry>
					        <SOE:TextEntry 
						        ID="Name"
						        TermID="2076" DefaultTerm="Benämning"
						        Validation="Required"
						        InvalidAlertTermID="2088" InvalidAlertDefaultTerm="Du måste ange ett namn"
						        MaxLength="50"
						        runat="server">
					        </SOE:TextEntry>
					        <SOE:NumericEntry
						        ID="StartNr"						        
						        TermID="2077" DefaultTerm="Startnummer"						        
						        Validation="Required"
						        InvalidAlertTermID="1667" InvalidAlertDefaultTerm="Du måste ange startnummer"
						        MaxLength="8"
						        runat="server">
					        </SOE:NumericEntry>				
                            <SOE:CheckBoxEntry
                                    ID="YearEndSerie" 
                                    TermID="4822"
                                    DefaultTerm="Serie för årsskifte"
                                    runat="server">
                            </SOE:CheckBoxEntry>			
                            <SOE:CheckBoxEntry
                                    ID="ExternalSerie" 
                                    TermID="4823"
                                    DefaultTerm="Serie för externa verifikat"
                                    runat="server">
                            </SOE:CheckBoxEntry>	
						</table>
					</fieldset>
				</div>
			</SOE:Tab>		
		</Tabs>
	</SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>