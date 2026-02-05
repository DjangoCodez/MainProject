<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.system.admin.licenses.users._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<SOE:Grid ID="SoeGrid1" ItemType="SoftOne.Soe.Data.User" runat="server" AutoGenerateColumns="false">
		<Columns>             
			<SOE:BoundField 
			    DataField="Name" 
			    TermID="1307" DefaultTerm="Namn" 
			    Filterable="Contains" Sortable="Text">
			</SOE:BoundField>        
			<SOE:BoundField 
			    DataField="LoginName" 
			    TermID="2007" DefaultTerm="Användarnamn" 
			    Filterable="Contains" Sortable="Text">			
            </SOE:BoundField>
			<SOE:BoundField 
			    DataField="LoggedIn" 
			    TermID="5150" DefaultTerm="Loggade in" 
			    Filterable="Contains" Sortable="Text">			
            </SOE:BoundField>
            <%--<SOE:TemplateField HeaderStyle-CssClass="action" ItemStyle-CssClass="action">
                <HeaderTemplate></HeaderTemplate>
                <ItemTemplate>
                    <SOE:Link ID="Link2" runat="server"
			            Href='<%# String.Format("?license={0}&user={1}&logout=1", licenseId, Item.UserId) %>'
                        Alt='<%#GetText(5149, "Logga ut")%>'
                        ImageSrc='<%#"/img/navigate_right.png" %>'
                        Permission='Modify'
                        Feature='Common'>
                    </SOE:Link>
                </ItemTemplate>
            </SOE:TemplateField>--%>
		</Columns>
	</SOE:Grid>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
