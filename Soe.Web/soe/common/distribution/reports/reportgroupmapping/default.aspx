<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.distribution.reports.reportgroupmapping._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnableCopy="false" DisableSave="true" runat="server">	
		<Tabs>
			<SOE:Tab Type="Edit" TermID="2176" DefaultTerm="Koppla rapport till rapportgrupper" runat="server">
				<div>
					<fieldset> 
                        <legend><%=Enc(GetText(2177, "Rapportgrupp"))%></legend>								    						
				        <table>
				            <SOE:SelectEntry 
				                ID="ReportGroups" 
				                TermID="2178" DefaultTerm="Rapportgrupper"
				                Width="320"
				                runat="server" >
				            </SOE:SelectEntry>						
			                <tr>
                                <td>&nbsp;</td>
                                <td align="right">
                                    <input type="submit" value=<%=Enc(GetText(2125, "Lägg till"))%>>
                                </td>
                            </tr>
			            </table>                 																		    												
						<table id="Groups" runat="server"></table>						
					</fieldset>
				</div>				
			</SOE:Tab>		
		</Tabs>	
		</SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>