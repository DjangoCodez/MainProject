<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SoeFormFooter.ascx.cs" Inherits="SoftOne.Soe.Web.UserControls.SoeFormFooter" %>
<div class="row formFooter">
    <div class="col-sm-12 messageFooter">
        <% if (!String.IsNullOrEmpty(Message)) { %>
            <span class="fal fa-info-circle"></span><span class="message"><%=Message%></span>
        <% } %>
    </div>
    <div class="col-sm-6 statusFooter">
        <span><%=PageBase.Enc(PageBase.GetText(1946, "Ingen historik"))%></span>
    </div>
    <div class="col-sm-6 buttonsFooter">
        <div class="pull-right">
            <asp:Button ID="ButtonDelete" OnClick="ButtonDelete_Click" CssClass="btn btn-default" Visible="false" runat="server"/>
            <asp:Button ID="ButtonSave" OnClick="ButtonSave_Click" CssClass="btn submit" runat="server"/>
        </div>
    </div>
    <div class="col-sm-8 linksFooter">        
        <% if (Links != null && Links.Count > 0) { %>
        <ul id="actionLinks">
            <% foreach (var link in Links) { %>
	            <li>
                    <a href="<%=link.Href %>" title="<%=link.Alt %>"><%=PageBase.Enc(link.Value) %></a>
                </li>		        
	        <% } %>
        </ul>
        <% } %>
    </div>
</div>