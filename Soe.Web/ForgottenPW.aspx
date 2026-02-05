<%@ Page Trace="false" MasterPageFile="~/base.master" Language="C#" AutoEventWireup="true" CodeBehind="ForgottenPW.aspx.cs" Inherits="SoftOne.Soe.Web._ForgottenPW" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxControlToolkit" %>
<asp:Content ID="baseMasterBodyContent" ContentPlaceHolderID="baseMasterBody" runat="server">
    <form id="Form2" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div id="Login">
        <div class="logo">
            <img src="/img/login.png" alt="<%=GetText(3020, "SoftOne") %>" />
        </div>
        <div>
            <form id="Form1" action="ForgottenPW.aspx" method="post" defaultfocus="send">
                <asp:UpdatePanel ID="UpdatePanelLogin" runat="server">
                    <ContentTemplate>
                        <div class="form">
                            <table>
                                <tr>
                                    <td colspan="2">
                                        <b><%= Enc(GetText(4544, "SoftOne, skapa nytt lösenord"))%></b> 
                                    </td>
                                </tr>
                                <tr>
                                    <th colspan="2">
                                         &nbsp;
                                    </th>
                                </tr>
                                <tr>
                                    <th>
                                        <label for="license"><%=GetText(1, "Avtal")%></label>
                                    </th>
                                    <td align="left">
                                        <input id="license" name="license" maxlength="50" value="<%=defaultLic%>" />
                                    </td>
                                </tr>
                                <tr>
                                    <th>
                                        <label for="login"><%=GetText(2, "Användarnamn")%></label>
                                    </th>
                                    <td align="left">
                                        <input id="login" name="login" maxlength="50" value="<%=defaultLogin%>"/>
                                    </td>
                                </tr>
                                <tr>
                                    <th>
                                        &nbsp;
                                    </th>
                                    <td align="left">
                                        <button type="submit" id="send"><%=GetText(4535, "Skicka nytt lösenord")%></button>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        &nbsp;
                                    </td>
                                </tr>
                                <tr>
                                    <th>
                                        &nbsp;
                                    </th>
                                    <td align="left">
                                        <b><%=message%></b>  
                                    </td>
                                </tr>
                                <tr>
                                    <th>
                                        &nbsp;
                                    </th>
                                    <td align="left">
                                        <a href="javascript:history.go(-1)" name="ForgottenPW"><%=GetText(5587, "Till inloggningssidan")%></a>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <asp:UpdateProgress DynamicLayout="false" ID="UpdateProgress1" runat="server">
                    <ProgressTemplate>
                        <div class="progress">
                            <img src="/img/mail_new.png" alt="" />
                            <%= Enc(GetText(4536, "Återställer lösenord")) %>
                        </div>
                    </ProgressTemplate>
                </asp:UpdateProgress>
            </form>
        </div>
    </div>
    </form>
    <script language="javascript" type="text/javascript">
// <![CDATA[

        function ForgottenPW_onclick() {
            window.location = "http://localhost:51079/Googlemaps.html";
        }

// ]]>
    </script>
</asp:Content>
