<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.productgroup._default" %>
<%@ Register Src="~/UserControls/CompanyCategories.ascx" TagPrefix="SOE" TagName="CompanyCategories"%>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
			    <div>
				    <fieldset> 
					    <legend><%=GetText(1874, "Artikeluppgifter")%></legend>
			            <table>
	                        <SOE:TextEntry 
	                            runat="server"
		                        ID="Code"
		                        Validation="Required"
		                        TermID="4244" DefaultTerm="Kod"
		                        MaxLength="100"
		                        Width="200">
	                        </SOE:TextEntry>
	                        <SOE:TextEntry 
	                            runat="server"
		                        ID="Name"
		                        Validation="Required"
		                        TermID="23" DefaultTerm="Namn"
		                        InvalidAlertTermID="4243" InvalidAlertDefaultTerm="Du måste ange en kod för gruppen"
		                        MaxLength="256"
		                        Width="200">
	                        </SOE:TextEntry>
                        </table>                                
                    </fieldset>
                </div>
            </SOE:Tab>
        </Tabs>
    </SOE:Form>
     
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
