<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.manage.system.admin.tasks._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" runat="server">
        <Tabs>
	        <SOE:Tab Type="Setting" runat="server">
	            <div id="DivTask" runat="server">
	                <fieldset>
                        <legend><%="Information"%></legend>
		                <table>
		                    <SOE:TextEntry 
	                            ID="TaskId"
                                TermID="5130" DefaultTerm="Id"
                                ReadOnly="true"
	                            MaxLength="300"
	                            Width="100"
	                            runat="server">
                            </SOE:TextEntry>
		                    <SOE:TextEntry 
	                            ID="Name"
                                TermID="5131" DefaultTerm="Namn"
                                ReadOnly="true"
	                            MaxLength="600"
	                            Width="600"
	                            runat="server">
                            </SOE:TextEntry>	
		                    <SOE:TextEntry 
	                            ID="Description"
                                TermID="5132" DefaultTerm="Beskrivning"
                                ReadOnly="true"
	                            MaxLength="500"
	                            Width="600"
	                            runat="server">
                            </SOE:TextEntry>
                            <%--
	                        <SOE:NumericEntry 
	                            ID="NoOfExecutions"
	                            TermID="5133" DefaultTerm="Antal utförda"
	                            MaxLength="10" 
                                ReadOnly="true"
	                            Width="100"
	                            runat="server">
	                        </SOE:NumericEntry>		
		                    <SOE:TextEntry 
	                            ID="LastExecutionState"
                                TermID="5134" DefaultTerm="Senaste status"
                                ReadOnly="true"
	                            MaxLength="100"
	                            Width="100"
	                            runat="server">
                            </SOE:TextEntry>
                            --%>
                        </table>
                    </fieldset>
                </div>
            </SOE:Tab>
        </Tabs>
    </SOE:Form>   
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
