<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TimeCodeTimeDeviationCause.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.TimeCodeTimeDeviationCause" %>
    <script type="text/javascript">
    var arrayFrom = new Array();
    var arrayTo = new Array();
    <%foreach (var dictionary in selectedFrom){%>
        arrayFrom.push(<%=dictionary.Value%>);
    <%}%>
    <%foreach (var dictionary in selectedTo){%>
        arrayTo.push(<%=dictionary.Value%>);
    <%}%>
    
    var selectMappingsCount=<%=selectedFrom.Count%>;
    </script>
<fieldset> 
    <legend><%=PageBase.GetText(4486, "Kopplingar mellan tidkod och orsak")%></legend>
    <table>
        <tr>
            <th width="1px"></th>
            <th width="200px"><label style="padding-left:5px"><%=PageBase.GetText(4488, "Orsak")%></label></th>
            <th width="200px"><label style="padding-left:5px"><%=PageBase.GetText(4487, "Tidkod") %></label></th>
        </tr>
        <SOE:FormIntervalEntry
            ID="Mappings"
            HideLabel="true"
            DisableHeader="true"
            DisableSettings="true"
            EnableDelete="true"
            ContentType="2"
            LabelType="1"
            LabelWidth="1"
            FromWidth="200"
            ToWidth="200"
            NoOfIntervals="10"
            runat="server">
        </SOE:FormIntervalEntry>
    </table>
</fieldset>

    <script type="text/javascript">
        for(var i=0;i<selectMappingsCount;i++) {
            var inp=null;
            var sel=$$('Mappings-from-'+(i+1));
            if(sel!=null) {
                for(var j=0;j<sel.length;j++) {
                    if(sel[j].value==arrayFrom[i]) {
                        sel[j].selected=true;
                    }
                }
            }
            var selTo=$$('Mappings-to-'+(i+1));
            if(selTo!=null) {
                for(var j=0;j<selTo.length;j++) {
                    if(selTo[j].value==arrayTo[i]) {
                        selTo[j].selected=true;
                    }
                }
            }
        }       
    </script>
