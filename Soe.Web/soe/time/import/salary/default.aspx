<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.import.salary._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" TermID="1174" DefaultTerm="Importera" EncType="multipart/form-data" runat="server">
	    <Tabs>
		    <SOE:Tab Type="Import" runat="server">
                <div>
                    <fieldset>
				        <legend><%=GetText(5562, "Importera lönespecifikation")%></legend>
                        <table>
                            <input type="hidden" name="action" value="upload"  />
                            <input type="file" name="File" value="upload" multiple="multiple" style="width:475px;margin-bottom:10px" />                          
                            <SOE:SelectEntry
                                ID="SalaryTimePeriod"
                                TermID="5564" DefaultTerm="Löneperiod"
                                Width="300"
                                Validation="NotEmpty" 
                                InvalidAlertDefaultTerm="Du måste ange löneperiod" InvalidAlertTermID="5565"                                
                                runat="server">
                            </SOE:SelectEntry> 
                            <SOE:SelectEntry
                                ID="ImportType"
                                TermID="9090" DefaultTerm="Typ av import"
                                Width="300"
                                Validation="NotEmpty" 
                                InvalidAlertDefaultTerm="Du måste ange importtyp" InvalidAlertTermID="9091"                                
                                runat="server">
                            </SOE:SelectEntry>                             
                        </table>                      
                    </fieldset>
                </div>
            </SOE:Tab>
	    </Tabs>
    </SOE:Form>
</asp:Content>