<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrinterTest.ascx.cs" CodeBehind="PrinterTest.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.PrinterTest" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>


<asp:Panel ID="pnlPrintWhere" runat="server">
    Print To...<br />
    <table border="0">
        <tr>
            <td>
                <asp:RadioButton ID="rbSystemPrinters" runat="server" GroupName="PrinterType" Checked="true" Text="System Printers" />: 
            </td>
            <td>
                <asp:DropDownList ID="ddlSystemPrinters" runat="server" /><br />
            </td>
        </tr>
        <tr>
            <td>
                <asp:RadioButton ID="rbArenaPrinters" runat="server" GroupName="PrinterType" Checked="false" Text="Arena Printers" />: 
            </td>
            <td>
                <asp:DropDownList ID="ddlArenaPrinters" runat="server" /><br />
            </td>
        </tr>
        <tr>
            <td>
                <asp:RadioButton ID="rbLocations" runat="server" GroupName="PrinterType" Checked="false" Text="Arena Locations" />: 
            </td>
            <td>
                <asp:DropDownList ID="ddlLocations" runat="server" /><br />
            </td>
        </tr>
        <tr>
            <td>
                <asp:RadioButton ID="rbKiosks" runat="server" GroupName="PrinterType" Checked="false" Text="Arena Kiosks" />: 
            </td>
            <td>
                <asp:DropDownList ID="ddlKiosks" runat="server" /><br />
            </td>
        </tr>
    </table>
</asp:Panel>

<p>
<asp:Panel ID="pnlPrintWhat" runat="server">
    Print What...<br />
    <asp:RadioButton ID="rbPrintBasicText" runat="server" GroupName="PrintWhat" Checked="true" Text="Basic Test" /><br />
    <asp:RadioButton ID="rbPrintRSDirect" runat="server" GroupName="PrintWhat" Checked="false" Text="Reporting Services Direct Print" />
        : <asp:DropDownList ID="ddlRSDirectLabel" runat="server" />
        <asp:CheckBox ID="cbRSDirectLandscape" runat="server" Text="Landscape" /><br />
    <asp:RadioButton ID="rbPrintRSFramework" runat="server" GroupName="PrintWhat" Checked="false" Text="Reporting Services Framework Print" />
        : <asp:DropDownList ID="ddlRSFrameworkLabel" runat="server" />
        <asp:CheckBox ID="cbRSFrameworkLandscape" runat="server" Text="Landscape" /><br />
</asp:Panel>
</p>

<p>
<asp:Panel ID="pnlSubmit" runat="server" style="margin-top: 10px;">
    <asp:Button ID="btnPrint" runat="server" OnClick="PrintTestButton" Text="Print" />
</asp:Panel>
</p>

<p>
<asp:Panel ID="pnlResults" runat="server" style="margin-top: 10px; padding-left: 40px;">
    <asp:Label ID="lbResults" runat="server" />
</asp:Panel>
</p>