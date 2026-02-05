<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.distribution.reports.permissions._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSettings="true" runat="server">
        <Tabs>
		    <SOE:Tab Type="Edit" TermID="5668" DefaultTerm="Koppla mot roller" runat="server">     
                <SOE:Instruction
                    TermID="5671" DefaultTerm="Om ingen roll väljs så har alla roller behörighet"
                    FitInTable="true"
                    runat="server">
                </SOE:Instruction>
                <div id="RoleMapping" runat="server"></div>
            </SOE:Tab>
        </tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
