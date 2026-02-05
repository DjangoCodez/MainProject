<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.import.invoices.automaster._default" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="AutomasterForm" TermID="1174" DefaultTerm="Importera" EncType="multipart/form-data" runat="server">
        <Tabs>
		    <SOE:Tab Type="Setting" runat="server">
			    <div>
                    <fieldset>
                        <legend><%=GetText(5403, "Importera fil")%></legend>			            
			            <table>
                            <input type="hidden" name="action" value="upload" />
                            <SOE:FileEntry 
	                            ID="File" 
	                            TermID="4071" DefaultTerm="Fil"
	                            Width="200"
	                            runat="server">
	                        </SOE:FileEntry>
	                    </table>
                    </fieldset>
		        </div>
		    </SOE:Tab>
	    </Tabs>
    </SOE:Form>
  
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
