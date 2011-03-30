<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceDetails.ascx.cs" CodeBehind="AttendanceDetails.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.AttendanceDetails" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<link type="text/css" rel="stylesheet" href="css/personPopup.css" />

<asp:HiddenField ID="hfFilterTypeGroupID" runat="server" Value="-1" />
<asp:HiddenField ID="hfFilterService" runat="server" Value="" />
<asp:HiddenField ID="hfFilterName" runat="server" Value="" />
<asp:HiddenField ID="hfFilterTypeID" runat="server" Value="-1" />
<asp:HiddenField ID="hfFilterOccurrenceID" runat="server" Value="-1" />
<asp:HiddenField ID="hfFilterLocationID" runat="server" Value="-1" />

<div class="listFilter">
    <asp:Panel ID="pnlDataFilter" Visible="true" runat="server" DefaultButton="btnFilterApply">
        <table cellpadding="0" cellspacing="3" border="0" style="margin: 5px;">
            <tr>
                <td valign="top" rowspan="5" align="left" style="padding-left: 10px; padding-top: 10px;"><img src="images/filter.gif" alt="Filter" border="0" /></td>
                <td class="formLabel" align="right">Attendance Group</td>
                <td colspan="5"><asp:DropDownList ID="ddlFilterTypeGroup" runat="server" CssClass="formItem" OnSelectedIndexChanged="ddlFilterTypeGroup_Changed" AutoPostBack="true" /></td>
            </tr>
            <tr>
                <td class="formLabel" align="right">Service Time</td>
                <td colspan="5"><asp:DropDownList ID="ddlFilterService" runat="server" CssClass="formItem" OnSelectedIndexChanged="ddlFilterService_Changed" AutoPostBack="true" /></td>
            </tr>
            <tr>
                <td class="formLabel" align="right">Name</td>
                <td colspan="5"><asp:TextBox ID="tbFilterName" runat="server" CssClass="formItem" /></td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td class="formLabel" align="center">Attendance Type</td>
                <td class="formLabel" align="center">or Occurrence</td>
                <td class="formLabel" align="center">or Location</td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td><asp:DropDownList ID="ddlFilterType" runat="server" CssClass="formItem" OnSelectedIndexChanged="ddlFilterType_Changed" AutoPostBack="true" /></td>
                <td><asp:DropDownList ID="ddlFilterOccurrence" runat="server" CssClass="formItem" OnSelectedIndexChanged="ddlFilterOccurrence_Changed" AutoPostBack="true" /></td>
                <td><asp:DropDownList ID="ddlFilterLocation" runat="server" CssClass="formItem" OnSelectedIndexChanged="ddlFilterLocation_Changed" AutoPostBack="true" /></td>
            </tr>
            <tr>
                <td><asp:Button ID="btnFilterApply" runat="server" CssClass="smallText" Text="Apply Filter" OnClick="btnFilterApply_Click" /></td>
                <td>&nbsp;</td>
            </tr>
        </table>
    </asp:Panel>
</div>
<asp:Panel ID="pnlDataGrid" Visible="true" runat="server">
    <Arena:DataGrid ID="dgAttendance" runat="server" Width="100%" ShowFooter="true" OnItemDataBound="dgAttendance_ItemDataBound">
        <Columns>
            <asp:TemplateColumn HeaderText="Name" SortExpression="common_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:LinkButton ID="lbName" runat="server"><%# DataBinder.Eval(Container.DataItem, "first_name") %> <b><%# DataBinder.Eval(Container.DataItem, "last_name") %></b></asp:LinkButton>
                    <asp:LinkButton ID="lbAttendanceRePrint" runat="server" style="display: none;" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "occurrence_attendance_id") %>' OnCommand="btnRePrint_Click">Re-Print Labels</asp:LinkButton>
                    <asp:LinkButton ID="lbNumberBoard" runat="server" style="display: none;" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "occurrence_attendance_id") %>' OnCommand="btnNumberBoard_Click">Post Number</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="Name" SortExpression="common_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:Label ID="lbName2" runat="server"><%# DataBinder.Eval(Container.DataItem, "first_name") %> <b><%# DataBinder.Eval(Container.DataItem, "last_name") %></b></asp:Label>
                    <asp:LinkButton ID="lbAttendanceRePrint2" runat="server" style="display: none;" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "occurrence_attendance_id") %>' OnCommand="btnRePrint_Click">Re-Print Labels</asp:LinkButton>
                    <asp:LinkButton ID="lbNumberBoard2" runat="server" style="display: none;" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "occurrence_attendance_id") %>' OnCommand="btnNumberBoard_Click">Post Number</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="Gender" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# Enum.GetName(typeof(Arena.Enums.Gender), Convert.ToInt32(DataBinder.Eval(Container.DataItem, "gender"))) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="Age" SortExpression="birth_date" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# BirthDateToAgeString(DataBinder.Eval(Container.DataItem, "birth_date")) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="Grade" SortExpression="graduation_date" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# GraduationDateToGradeString(DataBinder.Eval(Container.DataItem, "graduation_date")) %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn HeaderText="Ability Level" DataField="ability_level" SortExpression="ability_level" Visible="false" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Check-in" DataField="check_in_time" SortExpression="check_in_time" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Attendance Type" DataField="common_attendance_name" SortExpression="common_attendance_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Occurrence" DataField="common_occurrence_name" SortExpression="common_occurrence_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Location" DataField="common_location_name" SortExpression="common_location_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
        </Columns>
    </Arena:DataGrid>
</asp:Panel>

<asp:Label ID="lbStatus" runat="server" />