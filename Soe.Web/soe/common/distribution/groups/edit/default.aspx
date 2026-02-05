<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.distribution.groups.edit._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">    
    <SOE:Form ID="Form1" EnableCopy="false" EnableDelete="true" DisableSave="true" runat="server">	
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
				<div>
					<fieldset> 	
                        <legend><%=GetText(5404, "Grupp")%></legend>
                        <div>
					        <table>						    
					            <SOE:TextEntry   
				                    ID="Name"						            
				                    TermID="2228" DefaultTerm="Namn"						        
					                InvalidAlertTermID="1543" InvalidAlertDefaultTerm="Du måste ange namn"
				                    MaxLength="100"
				                    Width="250"
				                    runat="server">
				                </SOE:TextEntry>		
					            <SOE:TextEntry   
				                    ID="Description"						            
				                    TermID="1544" DefaultTerm="Beskrivning"						        
				                    MaxLength="255"
				                    Width="250"
				                    runat="server">
				                </SOE:TextEntry>				                                    
                                <SOE:SelectEntry 
					                runat="server" 
					                ID="TemplateType" 
					                Width="255"
					                TermID="2251" DefaultTerm="Rapporttyp">
					            </SOE:SelectEntry>					        
					            <SOE:SelectEntry 
					                runat="server" 
					                ID="ReportHeaders" 
					                Width="255"
					                TermID="2221" DefaultTerm="Rubriker">
					            </SOE:SelectEntry>
					            <SOE:CheckBoxEntry 
                                    ID="ShowLabel" 
                                    TermID="2234" DefaultTerm="Visa rubrik"                              
                                    runat="server">
                                </SOE:CheckBoxEntry>
                                <SOE:CheckBoxEntry 
                                    ID="ShowSum" 
                                    TermID="2235" DefaultTerm="Visa summa"                                                                
                                    runat="server">
                                </SOE:CheckBoxEntry>						
                                <SOE:CheckBoxEntry 
                                    ID="InvertRow" 
                                    TermID="1934" DefaultTerm="Vänd tecken"                                                                
                                    runat="server">
                                </SOE:CheckBoxEntry>
			                    <tr>
			                        <td/>
			                        <td align="left">
			                            <input name="add" id="add" type="submit" value="<%=reportGroup == null ? GetText(1232, "Registrera") : GetText(2125, "Lägg till")%>"/>
			                            <% if (reportGroup != null){ %>
			                                <input name="upd" id="upd" type="submit" value="<%=GetText(1437, "Uppdatera")%>"/>
			                            <%} %>
			                        </td>
			                    </tr>
				            </table>
                        </div>    																		    												
						<table id="Headers" runat="server"></table>						
					</fieldset>
				</div>				
			</SOE:Tab>		
		</Tabs>	
		</SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
