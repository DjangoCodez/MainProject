<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.users.edit.usermapping._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">    
<SOE:Form ID="Form1" DisableSettings="true" runat="server">
	<Tabs>
		<SOE:Tab Type="Edit" TermID="2065" DefaultTerm="Koppla mot företag/roller" runat="server">
            <div id="CompanyRoleMapping" runat="server"></div>
        </SOE:Tab>
    </Tabs>
</SOE:Form>
</asp:Content>
