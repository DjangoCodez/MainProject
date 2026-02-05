<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.economy.export.finnish_tax._default" %>
<%@ Register Src="~/UserControls/AngularSpaHost.ascx" TagPrefix="SOE" TagName="AngularSpaHost" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
	<%if (UseAngularSpa) {%>
		<SOE:AngularSpaHost ID="AngularSpaHost" runat="server" />
		<script type="text/javascript">
		if (!soeConfig)
			soeConfig = {};
		</script>    
	<%} else {%>
    <SOE:Form ID="Form1" TermID="1455" DefaultTerm="Exportera" EncType="multipart/form-data" runat="server">
	    <Tabs>
		    <SOE:Tab Type="Export" runat="server">				
                <fieldset>
                    <div class="col">
				        <table>
                            <SOE:SelectEntry 
					            ID="TaxEras_S"
					            TermID="4843" DefaultTerm="Skatteperiodens längd"
    					        Width="100"
					            runat="server">
					        </SOE:SelectEntry>		
					        <SOE:NumericEntry 
                                AllowDecimals="false"
					            ID="TaxPeriods_S" 
					            TermID="4844" DefaultTerm="Skatteperiod"
                                MaxLength="2"
    					        Width="100"
					            runat="server">
					        </SOE:NumericEntry>	
                            <SOE:NumericEntry 
                                AllowDecimals="false"
					            ID="TaxYear_S" 
					            TermID="4845" DefaultTerm="Året för skatteperioden"
                                MaxLength="4"
    					        Width="100"
					            runat="server"
                                Validation="Required"
						        InvalidAlertDefaultTerm="Måste anges"
						        InvalidAlertTermID="4174">
					        </SOE:NumericEntry>	
                        </table>
                    </div>
                    <div>
                        <table>
					        <SOE:CheckBoxEntry 
	                            ID="NoActivity_CB" 
	                            TermID="4846" DefaultTerm="Ingen verksamhet " 
	                            runat="server">
                            </SOE:CheckBoxEntry>      
                            <SOE:CheckBoxEntry 
	                            ID="Correction_CB" 
	                            TermID="4847" DefaultTerm="Korrigering" 
	                            runat="server">
                            </SOE:CheckBoxEntry>      
                            <SOE:SelectEntry 
					            ID="Reasons_S" 
					            TermID="4848" DefaultTerm="Orsak"
    					        Width="150"
					            runat="server">
					        </SOE:SelectEntry>	
                        </table>
                    </div>				
                </fieldset>
            </SOE:Tab>
        </Tabs>
    </SOE:Form>
	<%}%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>