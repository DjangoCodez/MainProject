<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.categories.edit._default" Title="Untitled Page" %>
<%@ Register Src="~/UserControls/CompanyCategories.ascx" TagPrefix="SOE" TagName="CompanyCategories"%>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Form ID="Form1" EnableBack="true" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
			    <div>
					<fieldset> 
						<legend><%=GetText(4108, "Kategori")%></legend>
				        <table>
					        <SOE:TextEntry runat="server"
						        ID="Code"
						        TermID="3083" DefaultTerm="Kod"
						        MaxLength="50">
					        </SOE:TextEntry>	
					        <SOE:TextEntry runat="server"
						        ID="Name"
						        Validation="Required"
						        TermID="29" DefaultTerm="Namn"
						        InvalidAlertTermID="44" InvalidAlertDefaultTerm="Du måste ange namn"
						        MaxLength="100">
					        </SOE:TextEntry>	
					        <SOE:SelectEntry 
					            ID="CategoryGroup"
					            TermID="4664" DefaultTerm="Grupp"
    						    Width="100"
					            runat="server">
					        </SOE:SelectEntry>				
                            <SOE:SelectEntry 
					            ID="ParentCategory"
					            TermID="5582" DefaultTerm="Underkategori till"
    						    Width="200"
					            runat="server">
					        </SOE:SelectEntry>	
				        </table>
				    </fieldset>
                </div>
			</SOE:Tab>
		</Tabs>
	</SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
