<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.common.holidays.edit._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <script type="text/javascript">
        var type=<%=modalType%>;
        if (type > 0)
        {
            var href="/modalforms/UpdateSchedules.aspx?type="+type+"&holiday="+<%=modalHolidayId%>+"&daytype="+<%=modalDaytypeId%>+"&oldDate=" + <%=deleteDate%>;
            PopLink.modalWindowShow(href);
        }
    </script>
	<SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" DisableSave="false" EnableCopy="true" runat="server">
		<Tabs>
			<SOE:Tab Type="Edit" runat="server">
			    <div>
					<fieldset> 
						<legend><%=GetText(3057, "Avvikelsedag")%></legend>
				        <table>
				            <SOE:TextEntry runat="server"
				                ID="Name"
				                Validation="Required"
				                TermID="29" DefaultTerm="Namn"
				                InvalidAlertTermID="4279" InvalidAlertDefaultTerm="Avvikelsedagen måste namnges"
				                MaxLength="100">
				            </SOE:TextEntry>
					        <SOE:DateEntry runat="server"
					            ID="Date"
					            Validation="Required"
					            TermID="4195" DefaultTerm="Datum"
					            InvalidAlertTermID="4280" InvalidAlertDefaultTerm="Datum som avvikelsedagen faller ut på måste anges"
					            MaxLength="100">
				            </SOE:DateEntry>	
                           <SOE:CheckBoxEntry
                                ID="IsRedDay" 
                                TermID="8746"
                                DefaultTerm="Röd dag" 
                                runat="server">
                            </SOE:CheckBoxEntry>
				            <SOE:TextEntry runat="server"
					            ID="Description"						                
					            TermID="4003" DefaultTerm="Beskrivning"
					            MaxLength="255">
					        </SOE:TextEntry>
				            <SOE:SelectEntry runat="server"
					            ID="DayType"						                
					            Validation="NotEmpty"
					            TermID="3058" DefaultTerm="Dagtyp"
					            InvalidAlertTermID="4298" InvalidAlertDefaultTerm="Dagtyp måste anges"
					            MaxLength="100">
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