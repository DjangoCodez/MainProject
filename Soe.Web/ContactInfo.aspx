<%@ Page Trace="false" MasterPageFile="~/base.master" Language="C#" AutoEventWireup="true" CodeBehind="ContactInfo.aspx.cs" Inherits="SoftOne.Soe.Web._ContactInfo" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxControlToolkit" %>
<asp:Content ID="baseMasterBodyContent" ContentPlaceHolderID="baseMasterBody" runat="server">
    <div id="Login">
        <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" integrity="sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u" crossorigin="anonymous">
        <!---<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>--->
        <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js" integrity="sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa" crossorigin="anonymous"></script>

        <div class="logo">
            <img src="/img/login.png" alt="<%=GetText(3020, "SoftOne") %>" />
        </div>

        <form id="Form1" action="ContactInfo.aspx" method="post" defaultfocus="send">

            <div style="padding-left: 20px; padding-right: 20px; margin: 0px auto 0px auto; max-width: 600px;">
                <% if (showForm)
                    { %>
                <div class="info" style="margin-bottom: 15px;">
                    <div style="margin-bottom: 5px;" />
                    <span style="font-size: 16px; font-weight: bold;">Ditt användarkonto saknar e-postadress som numera är obligatoriskt för kontohantering</span>
                    <div style="margin: 5px 0 5px 0;" />
                    <div style="margin-bottom: 5px;" />
                    <span style="font-size: 16px; font-weight: bold;">Your user account has no e-mail address, which is now mandatory for account management</span>
                    <div style="margin: 5px 0 5px 0;" />
                    <div style="margin-bottom: 5px;" />
                    <span style="font-size: 16px; font-weight: bold;">Ange dina kontaktuppgifter nedan</span>
                    <div style="margin: 5px 0 5px 0;" />
                    <div style="margin-bottom: 5px;" />
                    <span style="font-size: 16px; font-weight: bold;">Enter your contact details below</span>
                    <div style="margin: 5px 0 5px 0;">
                        <span style="font-weight: lighter; font-style: italic;">Uppgifter kommer enbart att användas för att det ska möjligt för dig att själva hantera din användare. Detta gäller t ex för återställning av lösenord</span>
                    </div>
                    <div style="margin: 5px 0 5px 0;">
                        <span style="font-weight: lighter; font-style: italic;">Data will only be used to enable you to manage your own login. This applies, for example, to password reset</span>
                    </div>
                </div>


                <div class="form-group">
                    <label for="exampleInputEmail1">E-post/Email</label>
                    <input type="email" class="form-control" id="email" required aria-describedby="emailHelp" placeholder="Ange e-post / Enter email" runat="server">
                </div>
                <div class="form-group">
                    <label for="exampleInputPassword1">Telefonnummer/Phonenumber</label>
                    <input type="text" class="form-control" id="phone" placeholder="Ange telefonnummer / Enter phonenumber" runat="server">
                </div>
                <button type="submit" class="btn btn-primary pull-right" style="background-color: orange">Spara / Save</button>


                <% }
                    else
                    { %>

                <!--<div style="margin:0px auto 0px auto;">   -->
                <div style="margin-bottom: 5px;">
                    <span style="font-size: 16px; font-weight: bold;">Tack!</span>
                    <span style="font-size: 16px; font-weight: bold;">Du kan återgå till appen nu</span>
                </div>
                <!--</div>-->
            </div>
        </form>
        <% }  %>
    </div>
</asp:Content>
