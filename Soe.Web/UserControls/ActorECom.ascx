<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ActorECom.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.ActorECom" %>
<div>
    <fieldset>
        <legend><%=PageBase.GetText(1386, "Telekomuppgifter")%></legend>
        <table>
            <SOE:FormIntervalEntry
                ID="ECom"
                TermID="1386" DefaultTerm="Telekomuppgifter" 
                OnlyFrom="true"
                NoOfIntervals="10"
                DisableHeader="true"
                DisableSettings="true"
                EnableDelete="true"
                ContentType="1"
                LabelType="2"
                LabelWidth="200"
                runat="server">
            </SOE:FormIntervalEntry>
        </table>
    </fieldset>
</div>