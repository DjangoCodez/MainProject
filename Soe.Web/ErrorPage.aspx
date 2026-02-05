<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ErrorPage.aspx.cs" Inherits="SoftOne.Soe.Web.ErrorPage" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Error Page</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f2f2f2;
            margin: 0;
            padding: 0;
            text-align: center;
        }

        .error-container {
            margin: 50px auto;
            padding: 30px;
            background-color: #fff;
            border: 1px solid #ddd;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            width: 60%;
        }

        h1 {
            color: #d9534f;
        }

        p {
            font-size: 16px;
        }

        .error-details {
            display: none; 
            margin-top: 20px;
            background-color: #f9f9f9;
            border: 1px solid #e2e2e2;
            padding: 15px;
            text-align: left;
        }

        .links {
            margin-top: 20px;
        }

        .links a {
            color: #337ab7;
            text-decoration: none;
            margin: 0 10px;
        }

        .links a:hover {
            text-decoration: underline;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="error-container">
            <h1>Oops! Something went wrong.</h1>
            <p>We apologize for the inconvenience. Please try again later or contact support if the problem persists.</p>
            <div class="error-details">
                <strong>Error Details:</strong>
                <asp:Label ID="lblErrorMessage" runat="server" Text=""></asp:Label>
            </div>
            <div class="links">
                <a href="javascript:history.back()">Back</a>
                <a href="/logout.aspx">Logout</a>
            </div>
        </div>
    </form>
</body>
</html>
