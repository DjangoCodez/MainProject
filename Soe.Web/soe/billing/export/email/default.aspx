<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.export.email._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
<SOE:Form ID="Form1" runat="server" TermID="4132" DefaultTerm="Välj en eller flera fakturor">
    <tabs>
		<SOE:Tab Type="Edit" runat="server">
			<div id="DivAccount" runat="server">
                <fieldset>
					<legend><%= GetText(4133, "Välj vad som skall ingå i utskicket")%></legend>
                    <table>		
                        <tr>
                            <th>
			                    <SOE:Instruction
			                        ID="From"
                                    TermID="4131"
                                    DefaultTerm="Skickas från"
                                    FitInTable="true"
                                    runat="server">
                                </SOE:Instruction>
                                <SOE:Text
			                        ID="FromAddress"
			                        FitInTable="true"
                                    runat="server">
                                </SOE:Text>
                            </th>
                        </tr>
                    </table>
                    <br />
                    <table>
                        <%--
                        <SOE:TextEntry
                            ID="Title"
                            TermID="4130"
                            DefaultTerm="Meddelandets titel"
                            MaxLength="30"
                            Validation="Required" 
                            InvalidAlertDefaultTerm="Meddelandet måste ha en titel"
                            InvalidAlertTermID="4140"
                            runat="server">
                        </SOE:TextEntry>--%>
                        <SOE:SelectEntry
                            ID="Template"
                            TermID="4136"
                            MaxLength="30"
                            DefaultTerm="Mall för e-post innehåll"
                            InvalidAlertDefaultTerm="Meddelandet måste ha ett innehåll"
                            InvalidAlertTermID="4139"
                            runat="server">                                
                        </SOE:SelectEntry>
					</table>
				</fieldset>
			</div>
		</SOE:Tab>
	</tabs>
</SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
