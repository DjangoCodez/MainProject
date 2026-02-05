<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AttestTransition.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.AttestTransition" %>
    <script type="text/javascript">
    var arrayTransitions = new Array();
    <%foreach (var dictionary in selected){%>
        arrayTransitions.push(<%=dictionary.Value%>);
    <%}%>
    var attestTransitionCount=<%=selected.Count%>;
    </script>
<fieldset> 
    <legend><%=PageBase.GetText(3343, "Behöriga attestövergångar")%></legend>
    <table>
        <tr>
            <th width="200px"><label style="padding-left:5px"><%=PageBase.GetText(3325, "Typ") %></label></th>
            <th width="200px"><label style="padding-left:5px"><%=PageBase.GetText(3344, "Övergång")%></label></th>
        </tr>
        <SOE:FormIntervalEntry
            ID="AttestTransitions"
            OnlyFrom="true"
            DisableHeader="true"
            DisableSettings="true"
            EnableDelete="true"
            ContentType="2"
            LabelType="2"
            LabelWidth="200"
            FromWidth="200"
            HideLabel="true"
            NoOfIntervals="100"
            runat="server">
        </SOE:FormIntervalEntry>
    </table>
</fieldset>

