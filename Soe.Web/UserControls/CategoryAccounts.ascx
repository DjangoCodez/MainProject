<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CategoryAccounts.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.CategoryAccounts" %>

<script type="text/javascript">
    var array = new Array();
    <%foreach (var dictionary in selected){%>
        array.push(<%=dictionary.Value%>);
    <%}%>
    var selectCount=<%=selected.Count%>;
</script>

<fieldset>
    <legend id="LegendHeader" runat="server"></legend>
    <table>
        <SOE:FormIntervalEntry
            ID="Category"
            TermID="4107" 
            OnlyFrom="true"
            DefaultTerm="Kategori" 
            NoOfIntervals="100"
            DisableHeader="true"
            DisableSettings="true"
            EnableDelete="true"
            ContentType="2"
            LabelType="1"
            LabelWidth="10"
            HideLabel="true"
            runat="server">
        </SOE:FormIntervalEntry>
    </table>
</fieldset>

<script type="text/javascript">
    for (var i = 0; i < selectCount; i++) {
        var inp = null;
        var sel = $$('<%= CategoryID %>' + '-from-' + (i + 1));
    if (sel != null) {
        for (var j = 0; j < sel.length; j++) {
            if (sel[j].value == array[i])
                sel[j].selected = true;
        }
    }
}      

</script>
