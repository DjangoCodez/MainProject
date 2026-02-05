<%@ Page Language="C#" MasterPageFile="~/soe/soe.master" AutoEventWireup="True" CodeBehind="defaultHtml.aspx.cs" Inherits="SoftOne.Soe.Web.soe._defaultHtml" Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="soeMainContent" runat="server">
    <script>
        $(document).ready(function () {
            $('.growImage').mouseover(function () {
                //moving the div left a bit is completely optional
                //but should have the effect of growing the image from the middle.
                $(this).stop().animate({ "width": "85px", "height": "85px", "left": "0px", "top": "0px" }, 150, 'swing');
                $("#selectedImage").attr("src", $(this).attr("src"));
            }).mouseout(function () {
                $(this).stop().animate({ "width": "60px", "height": "60px", "left": "0px", "top": "0px" }, 150, 'swing');
            });;
        });
    </script>
    <style>
        #startPage {
            min-height: 565px;
            border: 1px solid gray;
            box-shadow: 3px 3px 5px gray;
        }
        #modules {
            height: 400px;
            background: url('/img/silverlight/startpage/SoftOneBackground.jpg') repeat;
            position: relative;
        }
        #news {
            height: 165px;
            background: url('/img/silverlight/startpage/NewsMarginBackground.png') repeat;
        }
        #selectedModule {
            text-align: center;
            width: 100%;
            position: absolute;
            top: 80px;
        }
        #selectedImage {
        }
        #softOneLogo {
            position: absolute;
            top: 30px;
            left: 30px;
        }
        #moduleSelector {
            text-align: center;
            width: 100%;
            position: absolute;
            bottom: 30px;
        }
        #moduleSelector a {
            text-decoration: none;
        }
        .growImage {
            width: 60px;
            height: 60px;
        }
        .shadowed {
                -webkit-filter: drop-shadow(5px 5px 5px #222);
                filter: drop-shadow(5px 5px 5px #222); 
        }
    </style>
    <div id="startPage">
        <div id="modules">
            <div id="softOneLogo">
                <img class="shadowed" src="../img/silverlight/startpage/SoftOneLogo.png" />
            </div>
            <div id="selectedModule">
                <img id="selectedImage" src="../img/silverlight/startpage/billing.png" />
            </div>
            <div id="moduleSelector">
                <a id="billing" runat="server" href="/soe/billing"><img class="growImage" alt="billing" src="../img/silverlight/startpage/billing.png" /></a>
                <a id="economy" runat="server" href="/soe/economy"><img class="growImage" alt="economy" src="../img/silverlight/startpage/economy.png" /></a>
                <a id="time" runat="server" href="/soe/time"><img class="growImage" alt="billing" src="../img/silverlight/startpage/Personell.png" /></a>
                <a id="communication" runat="server" href="/soe/communication"><img class="growImage" alt="billing" src="../img/silverlight/startpage/communication.png" /></a>
            </div>
         </div>
        <div id="news">

        </div>
    </div>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="soeLeftContent" runat="server">
</asp:Content>
