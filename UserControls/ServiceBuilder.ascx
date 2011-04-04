<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceBuilder.ascx.cs" CodeBehind="ServiceBuilder.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.ServiceBuilder" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<link type="text/css" href="include/scripts/jqueryui/css/custom-theme/jquery-ui-.custom.css" rel="Stylesheet" />	
<link type="text/css" href="UserControls/Custom/HDC/CheckIn/Include/Servicebuilder.css" rel="Stylesheet" />	
<script type="text/javascript" src="include/scripts/jquery.1.3.2.min.js"></script>
<script type="text/javascript" src="include/scripts/jqueryui/js/jquery-ui-1.7.3.custom.min.js"></script>
<script type="text/javascript" src="UserControls/Custom/HDC/CheckIn/Scripts/ServiceBuilder.js"></script>

<script type="text/javascript">
var jsonDataField = '<%= jsonData.ClientID %>';
</script>

<asp:Panel runat="server" ID="pnlServiceInformation">
    <h3>Service Information</h3>
    <table border="0" style="margin-bottom: 10px;">
        <tr><td class="smallText">Name:</td><td><asp:TextBox ID="tbName" CssClass="smallText" runat="server" Width="200px" /></td></tr>
        <tr>
            <td class="smallText">Start Date:</td>
            <td><Arena:DateTextBox ID="tbStartDate" Runat="server" style="width:70px" CssClass="smallText" Required="true" EmptyValueMessage="Start Date is required!" InvalidValueMessage="Start Date must be a valid date!" />
	            &nbsp;&nbsp;<span class="smallText">Time:</span>
                <Arena:DateTextBox ID="tbStartTime" runat="server" style="width:80px" CssClass="smallText" Required="true" Format="time" InvalidValueMessage="Start Time must be a valid time (hh:mm am/pm)!"></Arena:DateTextBox>
            </td>
        </tr>
        <tr>
            <td class="smallText">End Date:</td>
            <td><Arena:DateTextBox ID="tbEndDate" Runat="server" style="width:70px" CssClass="smallText" Required="true" EmptyValueMessage="End Date is required!" InvalidValueMessage="End Date must be a valid date!" />
	            &nbsp;&nbsp;<span class="smallText">Time:</span>
                <Arena:DateTextBox ID="tbEndTime" runat="server" style="width:80px" CssClass="smallText" Required="true" Format="time" InvalidValueMessage="End Time must be a valid time (hh:mm am/pm)!"></Arena:DateTextBox>
            </td>
        </tr>
        <tr>
            <td class="smallText">Check-In Start:</td>
            <td><Arena:DateTextBox ID="tbCheckInStartDate" Runat="server" style="width:70px" CssClass="smallText" Required="true" EmptyValueMessage="Start Date is required!" InvalidValueMessage="Start Date must be a valid date!" />
	            &nbsp;&nbsp;<span class="smallText">Time:</span>
                <Arena:DateTextBox ID="tbCheckInStartTime" runat="server" style="width:80px" CssClass="smallText" Required="true" Format="time" InvalidValueMessage="Start Time must be a valid time (hh:mm am/pm)!"></Arena:DateTextBox>
            </td>
        </tr>
        <tr>
            <td class="smallText">Check-In End:</td>
            <td><Arena:DateTextBox ID="tbCheckInEndDate" Runat="server" style="width:70px" CssClass="smallText" Required="false" InvalidValueMessage="End Date must be a valid date!" />
	            &nbsp;&nbsp;<span class="smallText">Time:</span>
                <Arena:DateTextBox ID="tbCheckInEndTime" runat="server" style="width:80px" CssClass="smallText" Required="false" Format="time" InvalidValueMessage="End Time must be a valid time (hh:mm am/pm)!"></Arena:DateTextBox>
            </td>
        </tr>
        <tr>
            <td class="smallText">Membership Required for Check-In</td>
            <td><Arena:ArenaCheckBox ID="cbMembershipRequired" runat="server" Text="" /></td>
        </tr>
        <tr>
            <td class="smallText" style="width: 200px;">Link To Tag<br />(Only used when attendance type is not already linked to a tag/group)</td>
            <td><Arena:ProfilePicker ID="ppProfile" runat="server" AllowRemove="true" /> <Arena:ArenaCheckBox ID="cbUseForAll" runat="server" CssClass="smallText" Text="Use For All" ToolTip="If checked, all occurrences will be linked to this tag, overriding any attendance type setting." /></td>
        </tr>
    </table>

    <asp:Button ID="btnCreate" runat="server" Text="Create" OnClick="btnCreate_Click" />
</asp:Panel>

<asp:Panel ID="pnlServiceLayout" runat="server">
    <h3>Service Layout</h3>
    <asp:Label ID="lbErrors" runat="server" ForeColor="Red" Text="" />
    <asp:Label ID="lbStatus" runat="server" Text="" />
    <table border="0">
        <tr style="vertical-align: top;">
            <td>
                <div id="sbAttendanceTypes" class="sbDraggableList sbListContainer">
                    <h3 class="sbHeader ui-widget-header">Attendance Types</h3>
                    <div class="ui-widget-content">
                        <ol>
                            <asp:DropDownList ID="ddlOccurrenceTypeGroup" runat="server" Width="100%" AutoPostBack="true" OnSelectedIndexChanged="ddlOccurrenceTypeGroup_SelectedIndexChanged" style="margin-bottom: 10px;" />
                            <asp:Literal ID="ltAttendanceTypes" runat="server" />
                        </ol>
                    </div>
                </div>
            </td>
            <td>
                <div id="roomList" class="sbDraggableList sbListContainer">
                    <h3 class="sbHeader ui-widget-header">Locations</h3>
                    <div class="ui-widget-content">
                        <ol>
                            <asp:Literal ID="ltLocations" runat="server" />
                        </ol>
                    </div>
                </div>
            </td>
            <td>
                <div id="service" class="sbListContainer">
                    <h3 class="sbHeader ui-widget-header">Build Service</h3>
                    <div class="ui-widget-content">
                        <ol>
                        </ol>
                    </div>
                </div>
            </td>
        </tr>
    </table>
</asp:Panel>

<asp:HiddenField ID="jsonData" runat="server" Value="{}" />
