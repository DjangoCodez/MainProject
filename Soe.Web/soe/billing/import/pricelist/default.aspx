<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.billing.import.pricelist._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">

    <SOE:Form ID="Form1" TermID="1174" DefaultTerm="Importera" EncType="multipart/form-data" runat="server">
	    <Tabs>
		    <SOE:Tab Type="Import" runat="server">
			    <div>
                    <fieldset>
				        <legend><%=Legend %></legend>
				        <table>
                            <input type="hidden" name="action" value="upload" />
                            <SOE:FileEntry 
		                        ID="FileInput" 
		                        TermID="4071" DefaultTerm="Fil"
		                        Width="200"
		                        runat="server">
		                    </SOE:FileEntry>
                            
                                <SOE:FileEntry 
		                            ID="FileInput2" 
		                            TermID="4071" DefaultTerm="Fil"
		                            Width="200"
                                    Visible="true"
		                            runat="server">
		                        </SOE:FileEntry>
                            
		                    <SOE:SelectEntry
						        ID="Provider" 
						        TermID="4165" DefaultTerm="Tillhandahållare"
                                OnChange="providerChanged()"
						        runat="server">
						    </SOE:SelectEntry>
		                </table>
                        <div id="importingDiv" style="visibility: hidden">
                            <img src="../../../../img/loading.gif" /> &nbsp Importerar...
                        </div>
				    </fieldset>
		        </div>
		    </SOE:Tab> 
	    </Tabs>
    </SOE:Form>    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
