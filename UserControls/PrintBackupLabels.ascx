<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrintBackupLabels.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.PrintBackupLabels" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

Starting security code: <asp:TextBox ID="tbStartingNumber" runat="server"></asp:TextBox><br />
Number of sets to print: <asp:TextBox ID="tbPrintCount" runat="server"></asp:TextBox><br />
Print to: <asp:DropDownList ID="ddlPrinter" runat="server"></asp:DropDownList><br />

<asp:Button ID="btnPrint" Text="Print" OnClick="btnPrint_Click" runat="server" />
