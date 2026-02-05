<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.attest.transition._default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true"
        runat="server">
        <tabs>
			<SOE:Tab Type="Edit" runat="server">
			    <div>
					<fieldset> 
						<legend><%=GetText(3330, "Övergång")%></legend>
				        <table>
                            <SOE:SelectEntry
                                ID="Entity"
                                TermID="3325"
                                DefaultTerm="Typ"
                                Width="156"
                                runat="server">
                            </SOE:SelectEntry>
					        <SOE:TextEntry
					            runat="server"  
						        ID="Name"
						        Validation="Required"
						        TermID="3313" DefaultTerm="Namn"
						        InvalidAlertTermID="44" InvalidAlertDefaultTerm="Du måste ange namn"
                                Width="150"
						        MaxLength="100">
					        </SOE:TextEntry>	
                            <SOE:SelectEntry
                                ID="StateFrom"
                                TermID="3331"
                                DefaultTerm="Från nivå"
                                OnChange="SuggestTransitionName()"
                                Width="156"
                                runat="server">
                            </SOE:SelectEntry>
                            <SOE:SelectEntry
                                ID="StateTo"
                                TermID="3332"
                                DefaultTerm="Till nivå"
                                OnChange="SuggestTransitionName()"
                                Width="156"
                                runat="server">
                            </SOE:SelectEntry>
				        </table>
                        <table>
                            <SOE:CheckBoxEntry 
                                ID="NotifyChangeOfAttestState" 
                                TermID="11982"
                                DefaultTerm="Skicka meddelande till anställd vid förändring av atteststatus"
                                runat="server">                                
                            </SOE:CheckBoxEntry>
                        </table>
				    </fieldset>
                </div>
			</SOE:Tab>
		</tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
