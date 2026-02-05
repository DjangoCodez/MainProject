<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.users.edit.email._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Form ID="Form1" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" TermID="1551" DefaultTerm="Byt lösenord" runat="server">
                <div>
                    <SOE:InstructionList ID="InstructionList" DefaultIdentifier=" " runat="server"></SOE:InstructionList>
                </div>
				<div>
                    <fieldset>
                        <legend><%= GetText(2252, "Epost") %></legend>
					    <table>	
				            <SOE:TextEntry 
					            ID="NewEmail"
					            TermID="2252" DefaultTerm="Epost"
					            Validation="Required"
					            InvalidAlertTermID="11673" InvalidAlertDefaultTerm="Du måste ange epost"
					            MaxLength="50"						        
					            runat="server">
				            </SOE:TextEntry>
				            <SOE:TextEntry 
					            ID="ConfirmEmail"
					            TermID="11662" DefaultTerm="Bekräfta epost"
					            Validation="Required"
					            InvalidAlertTermID="11664" InvalidAlertDefaultTerm="Du måste bekräfta epost"
					            MaxLength="50"						        
					            runat="server">
				            </SOE:TextEntry>
					    </table>
                    </fieldset>	
				</div>
			</SOE:Tab>					
		</Tabs>
	</SOE:Form>
</asp:Content>
