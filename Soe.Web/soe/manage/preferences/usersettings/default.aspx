<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.preferences.usersettings.Default" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
		<Tabs>
			<SOE:Tab Type="Setting" TermID="5414" DefaultTerm="Användarinställningar" runat="server">
				<div>
					<fieldset> 
						<legend><%=GetText(3023, "Generella användarinställningar")%></legend>
						<table>
						    <SOE:SelectEntry
							    ID="UserLangId"
							    TermID="3010" 
							    DefaultTerm="Standardspråk" 
							    runat="server">
						    </SOE:SelectEntry>
						    <SOE:SelectEntry
							    ID="UserCompanyId"
							    TermID="3011" 
							    DefaultTerm="Standardföretag" 
							    runat="server">
						    </SOE:SelectEntry>
                            <SOE:CheckBoxEntry
                                ID="UserShowAnimations"
                                TermID="3120"
                                DefaultTerm="Visa animeringar" 
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

