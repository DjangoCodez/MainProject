<%@ Page Language="C#" Trace="false" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.preferences.schedulesettings.halfdays.edit._default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <script type="text/javascript">
    var clockValue1=<%=clockValue1 %>;
    var clockValue2=<%=clockValue2 %>;
    var clockValue3=<%=clockValue3 %>;
    var array = new Array();
    <%foreach (var dictionary in selected){%>
        array.push(<%=dictionary.Value%>);
    <%}%>
    var selectCount=<%=selected.Count%>;    
    var timehalfdayid=<%=modalTimeHalfdayId%>;
    var timedaytypeid=<%=modalDaytypeId%>;    
        var type=<%=modalType%>;
    if (type > 0)
    {
        var href="/modalforms/UpdateSchedules.aspx?type="+type+"&halfday="+timehalfdayid+"&daytype="+timedaytypeid;
        PopLink.modalWindowShow(href);
    }
    </script>
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
        <tabs>
			<SOE:Tab Type="Edit" runat="server">
                <div>
		            <fieldset> 
			            <legend><%=GetText(4418, "Halvdag")%></legend>
	                    <table>
		                    <SOE:TextEntry 
			                    ID="Name"
			                    TermID="3070" DefaultTerm="Namn"
			                    Validation="Required"
			                    InvalidAlertTermID="1041" InvalidAlertDefaultTerm="Du måste ange namn"
			                    MaxLength="255"
			                    runat="server">
		                    </SOE:TextEntry>	
		                    <SOE:TextEntry 
			                    ID="Description"
			                    TermID="4003" DefaultTerm="Beskrivning"
			                    MaxLength="255"
			                    runat="server">
		                    </SOE:TextEntry>	
	                    	<SOE:SelectEntry
				                ID="DayType"
				                TermID="4370" DefaultTerm="Dagtyp"
				                Validation="NotEmpty"
				                InvalidAlertDefaultTerm="Dagtyp måste anges"
				                InvalidAlertTermID="4292"
				                MaxLength="255"
				                runat="server">
				            </SOE:SelectEntry>
				            <SOE:SelectEntry
				                ID="Type"
				                TermID="4416" DefaultTerm="Typ"
				                Validation="NotEmpty"
				                InvalidAlertDefaultTerm="Typ måste anges"
				                InvalidAlertTermID="4415"
				                MaxLength="255"
				                runat="server">
				            </SOE:SelectEntry>
				            <SOE:TextEntry
				                ID="Value"
				                TermID="4413" DefaultTerm="Klockslag/procent"
				                MaxLength="300"
				                OnChange="ToggleInputFields(this)" 
				                runat="server">
				            </SOE:TextEntry>
                        </table>
                    </fieldset>
                </div>
                <div>
		                <fieldset> 
			                <legend><%=GetText(4417, "Raster som inte dras på halvdag")%></legend>
			                <table>
	                        <SOE:FormIntervalEntry
                                    ID="Breaks"
                                    OnlyFrom="true"
                                    NoOfIntervals="10"
                                    DisableHeader="true"
                                    DisableSettings="true"
                                    HideLabel="true"
                                    EnableDelete="true"
                                    ContentType="2"
                                    LabelType="1"
                                    LabelWidth="1"
                                    runat="server">
                                </SOE:FormIntervalEntry>
                            </table>
                        </fieldset>
                    </div>
			</SOE:Tab>
		</tabs>
    </SOE:Form>
    <script type="text/javascript">
        try {
            for(var i=0;i<selectCount;i++) {
                var inp=null;
                var sel=$$('Breaks-from-'+(i+1));
                if(sel!=null) {
                    for(var j=0;j<sel.length;j++) {
                        if(sel[j].value==array[i])
                            sel[j].selected=true;
                    }
                }
            }
            ToggleInputFields(null);
        }
        catch(err){ }            
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>

