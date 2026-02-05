<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.suppinvoicesettings.paymentmethods.edit._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnableDelete="true" EnableCopy="true" EnablePrevNext="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
				<div>
                    <fieldset>
                        <legend><%=GetText(1770, "Betalningsmetod")%></legend>
                        <table>
	                        <SOE:TextEntry
	                            ID="Name"
	                            TermID="1793" 
                                DefaultTerm="Namn"
						        Validation="Required"
						        InvalidAlertTermID="1794" 
						        InvalidAlertDefaultTerm="Du måste ange namn"
	                            runat="server">
	                        </SOE:TextEntry>
                            <SOE:SelectEntry 
                                ID="SysPaymentMethod"
                                TermID="1771" 
                                DefaultTerm="Exporttyp"
                                OnChange="enableDisableFields()"
                                runat="server">
                            </SOE:SelectEntry>	 
                             <SOE:SelectEntry 
                                ID="PaymentInformation"
                                TermID="1773" 
                                DefaultTerm="Betalningsuppgifter"
                                runat="server">
                            </SOE:SelectEntry>	 
                            <SOE:TextEntry
                                ID="CustomerNr"
                                TermID="1772" 
                                DefaultTerm="Kundnummer"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="Account"
                                TermID="1776" 
                                DefaultTerm="Tillgångskonto"
						        Validation="Required"
						        InvalidAlertTermID="1777" 
						        InvalidAlertDefaultTerm="Du måste ange tillgångskonto"
                                runat="server">
                            </SOE:TextEntry>
                            <SOE:TextEntry
                                ID="PayerBankId"
                                TermID="4681" 
                                DefaultTerm="Bank Id"
                                runat="server">
                            </SOE:TextEntry>
                        </table>
                    </fieldset>
                </div>
            </SOE:Tab>
        </Tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
