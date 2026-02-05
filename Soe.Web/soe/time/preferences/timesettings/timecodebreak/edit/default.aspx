<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SoftOne.Soe.Web.soe.time.preferences.timesettings.timecodebreak.edit._default" %>
<%@ Register TagName="TimeCodeBase" TagPrefix="SOE" Src="~/UserControls/TimeCodeBase.ascx" %>
<%@ Register Src="~/UserControls/TimeCodeTimeDeviationCause.ascx" TagPrefix="SOE" TagName="TimeCodeTimeDeviationCause" %>
<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <SOE:Form ID="Form1" EnablePrevNext="true" EnableDelete="true" EnableCopy="true" runat="server">
        <tabs>
			<SOE:Tab Type="Edit" runat="server">
                <SOE:TimeCodeBase ID="TimeCodeBase" Runat="Server"></SOE:TimeCodeBase>
		    </SOE:Tab>
		    <SOE:Tab Type="Setting" runat="server">
                <div>
                    <SOE:TimeCodeTimeDeviationCause ID="TimeCodeTimeDeviationCauses" Runat="Server"></SOE:TimeCodeTimeDeviationCause>
                </div>
			</SOE:Tab>
	    </tabs>
    </SOE:Form>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
