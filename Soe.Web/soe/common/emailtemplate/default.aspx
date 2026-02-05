<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.emailtemplate._default" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
			    <div>
			        <div>
			            <div>
					        <fieldset> 
						        <legend><%=GetText(4108, "Kategori")%></legend>
				                <table>
					                <SOE:TextEntry 
						                ID="Name"
						                TermID="29" DefaultTerm="Namn"
                                        Validation="Required"
						                InvalidAlertTermID="44" InvalidAlertDefaultTerm="Du måste ange namn"
						                MaxLength="100"
                                        runat="server">
					                </SOE:TextEntry>
					                <SOE:TextEntry
                                        ID="Subject"
                                        TermID="4130"
                                        DefaultTerm="Meddelandets titel"
                                        Validation="Required" 
                                        InvalidAlertTermID="4140"
                                        InvalidAlertDefaultTerm="Meddelandet måste ha en titel"
                                        MaxLength="30"
                                        runat="server">
                                    </SOE:TextEntry>
                                    <SOE:BooleanEntry 
					                    runat="server"
					                    TermID="4154" 
					                    DefaultTerm="Skickas som HTML"
					                    id="IsHTML">
					                </SOE:BooleanEntry>	
					            </table>
					            <br />
				                <%=BodyText%>
				                <textarea id="Body" rows="20" cols="100" style="overflow-x: none; overflow-y:auto;" runat="server"></textarea>
				                <br />
				                <div id="preview" runat="server"></div>
				            </fieldset>
			            </div>
			        </div>
                </div>
			</SOE:Tab>
		</Tabs>
	</SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
