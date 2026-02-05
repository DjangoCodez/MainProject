<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FieldSettings.aspx.cs" Inherits="SoftOne.Soe.Web.Settings.FieldSettings" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <div>
        <a href="javascript:history.go(-1)"><%= GetText(1549, "Gå tillbaka")%></a>
    </div>
    <br />
	<SOE:Form ID="Form1" TermID="30" DefaultTerm="Spara" runat="server" ActiveTab="1" DisableSettings="true" DisableSwitchLabelText="true">
		<Tabs>
			<SOE:Tab ID="RoleSettings" Type="Setting" TermID="1000" DefaultTerm="Fältinställningar för roll" runat="server">
				<div>
					<fieldset>
						<legend><%=GetText(1025, "Fältinställningar")%></legend>
						<table>
							<SOE:TextEntry 
							    ID="RoleFieldLabel" 
							    TermID="1005" DefaultTerm="Ledtext" 
							    DisableSettings="true" 
							    MaxLength="50" runat="server">
							</SOE:TextEntry>
							<SOE:SelectEntry
					            ID="RoleFieldVisible"
					            DisableSettings="true"
					            TermID="1006" DefaultTerm="Synlig"
					            runat="server">
					        </SOE:SelectEntry>
					        <SOE:SelectEntry
					            ID="RoleFieldTabStop"
					            DisableSettings="true"
					            TermID="1086" DefaultTerm="Skippa tabbstopp"
					            runat="server">
					        </SOE:SelectEntry>
					       <SOE:SelectEntry
					            ID="RoleFieldReadOnly"
					            DisableSettings="true"
					            TermID="1087" DefaultTerm="Bara läs"
					            runat="server">
					        </SOE:SelectEntry>
					        <SOE:SelectEntry
					            ID="RoleFieldBoldLabel"
					            DisableSettings="true"
					            TermID="1520" DefaultTerm="Fet ledtext"
					            runat="server">
					        </SOE:SelectEntry>
						</table>
					</fieldset>
				</div>	
			</SOE:Tab>
			<SOE:Tab ID="CompanySettings" Type="Setting" TermID="1001" DefaultTerm="Fältinställningar för företag" runat="server">
				<div>
					<fieldset>
						<legend>
						    <%=GetText(1025, "Fältinställningar")%>
						</legend>
						<table>
							<SOE:TextEntry 
							    ID="CompanyFieldLabel" 
							    TermID="1005" DefaultTerm="Ledtext" 
							    DisableSettings="true" 
							    MaxLength="50" runat="server">
							</SOE:TextEntry>
							<SOE:SelectEntry runat="server"
					            ID="CompanyFieldVisible"
					            DisableSettings="true" 
					            TermID="1006" DefaultTerm="Synlig">
					        </SOE:SelectEntry>
					        <SOE:SelectEntry
					            ID="CompanyFieldTabStop"
					            DisableSettings="true" 
					            TermID="1086" DefaultTerm="Skippa tabbstopp"
					            runat="server">
					        </SOE:SelectEntry>
					        <SOE:SelectEntry
					            ID="CompanyFieldReadOnly"
					            DisableSettings="true"
					            TermID="1087" DefaultTerm="Bara läs"
					            runat="server">
					        </SOE:SelectEntry>
					        <SOE:SelectEntry
					            ID="CompanyFieldBoldLabel"
					            DisableSettings="true"
					            TermID="1520" DefaultTerm="Fet ledtext"
					            runat="server">
					        </SOE:SelectEntry>
                        </table>
					</fieldset>
				</div>	
			</SOE:Tab>
		</Tabs>
	</SOE:Form>
</asp:Content>
