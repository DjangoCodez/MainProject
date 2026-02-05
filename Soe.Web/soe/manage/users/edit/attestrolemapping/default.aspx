<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.users.edit.attestrolemapping._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSettings="true" runat="server">
	    <Tabs>
		    <SOE:Tab Type="Edit" TermID="5238" DefaultTerm="Koppla mot attestroller" runat="server">     
                <SOE:Instruction
                    TermID="5244" DefaultTerm="Inget från datum betyder per omgående"
                    FitInTable="true"
                    runat="server">
                </SOE:Instruction>
                <br />
                <SOE:Instruction
                    TermID="5245" DefaultTerm="Inget till datum betyder tillsvidare"
                    FitInTable="true"
                    runat="server">
                </SOE:Instruction>
                <div id="AttestRoleMapping" runat="server"></div>
            </SOE:Tab>
        </Tabs>
    </SOE:Form>
</asp:Content>
