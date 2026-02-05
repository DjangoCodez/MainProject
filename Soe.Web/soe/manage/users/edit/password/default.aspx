<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.users.edit.password._default" %>
<%@ Register TagName="PasswordPolicy" TagPrefix="SOE" Src="/UserControls/PasswordPolicy.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Form ID="Form1" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" TermID="1551" DefaultTerm="Byt lösenord" runat="server">
                <SOE:Instruction 
                    ID="ChangePasswordInstruction"
                    TermID="5309" DefaultTerm="Du måste byta lösenord innan du kan använda systemet"
                    FitInTable="false"
                    runat="server">
                </SOE:Instruction>
				<div>
                    <fieldset>
                        <legend><%= GetText(4826, "Lösenordsinformation") %></legend>
					    <table>	
				            <SOE:PasswordEntry 
					            ID="OldPassword"
					            TermID="1529" DefaultTerm="Nuvarande lösenord"
					            Validation="Required"
					            InvalidAlertTermID="1575" InvalidAlertDefaultTerm="Du måste ange nuvarande lösenord"
					            MaxLength="50"						        
					            runat="server">
				            </SOE:PasswordEntry>
				            <SOE:PasswordEntry 
					            ID="NewPassword"
					            TermID="1569" DefaultTerm="Nytt lösenord"
					            Validation="Required"
					            InvalidAlertTermID="1570" InvalidAlertDefaultTerm="Du måste ange ett nytt lösenord"
					            MaxLength="50"						        
					            runat="server">
				            </SOE:PasswordEntry>
				            <SOE:PasswordEntry 
					            ID="ConfirmPassword"
					            TermID="1571" DefaultTerm="Bekräfta lösenord"
					            Validation="Required"
					            InvalidAlertTermID="1572" InvalidAlertDefaultTerm="Du måste bekräfta lösenordet"
					            MaxLength="50"						        
					            runat="server">
				            </SOE:PasswordEntry>
					    </table>
                    </fieldset>	
				</div>
			    <div>
			        <SOE:PasswordPolicy ID="PasswordPolicy" Runat="Server"></SOE:PasswordPolicy>
			    </div>
			</SOE:Tab>					
		</Tabs>
	</SOE:Form>
</asp:Content>
