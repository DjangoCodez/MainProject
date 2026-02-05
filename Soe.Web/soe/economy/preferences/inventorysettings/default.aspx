<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.preferences.inventorysettings._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" DisableSave="false" runat="server">
		<Tabs>
			<SOE:Tab Type="Setting" TermID="3486" DefaultTerm="Inställningar inventarier" runat="server">
                <div>
                    <fieldset>
                        <legend><%=GetText(3476, "Inventarier")%></legend>
                        <table>
                            <SOE:CheckBoxEntry
                                ID="SeparateVouchersInWriteOffs" 
                                TermID="4859"
                                DefaultTerm="Separata verifikationer för avskrivning" 
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
