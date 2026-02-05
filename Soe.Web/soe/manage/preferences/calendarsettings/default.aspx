<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.preferences.calendarsettings.Default" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
		<Tabs>
			<SOE:Tab Type="Setting" TermID="3053" DefaultTerm="Kalenderinställningar" runat="server">
				<div class="col">
					<fieldset> 
						<legend><%=GetText(3118, "Generella kalenderinställningar")%></legend>
						<table>
						</table>
					</fieldset>
				</div>
			</SOE:Tab>		
		</Tabs>
	</SOE:Form>		
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

