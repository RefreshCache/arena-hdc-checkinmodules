<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ManageAttendance.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.ManageAttendance" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<asp:HiddenField ID="hfFilterAttendanceTypeGroupID" runat="server" Value="-1" />
<asp:HiddenField ID="hfFilterAttendanceTypeID" runat="server" Value="-1" />
<asp:HiddenField ID="hfFilterOccurrenceID" runat="server" Value="-1" />
<asp:HiddenField ID="hfFilterLocationID" runat="server" Value="-1" />

<div class="listFilter">
    <asp:Panel ID="pnlDataFilter" Visible="true" runat="server" DefaultButton="btnFilterApply">
        <table cellpadding="0" cellspacing="3" border="0" style="margin: 5px;">
            <tr>
                <td valign="top" rowspan="4" align="left" style="padding-left: 10px; padding-top: 10px;"><img src="images/filter.gif" alt="Filter" border="0" /></td>
                <td class="formLabel" align="right">Attendance Type</td>
                <td><asp:DropDownList ID="ddlFilterAttendanceTypeGroup" runat="server" CssClass="formItem" OnSelectedIndexChanged="ddlFilterAttendanceTypeGroup_Changed" AutoPostBack="true"/> <asp:DropDownList ID="ddlFilterAttendanceType" runat="server" CssClass="formItem" OnSelectedIndexChanged="ddlFilterAttendanceType_Changed" AutoPostBack="true" /></td>
            </tr>
            <tr>
                <td class="formLabel" align="right">Occurrence</td>
                <td><asp:DropDownList ID="ddlFilterOccurrence" runat="server" CssClass="formItem" /></td>
            </tr>
            <tr>
                <td class="formLabel" align="right">Location</td>
                <td><asp:DropDownList ID="ddlFilterLocation" runat="server" CssClass="formItem" /></td>
            </tr>
            <tr>
                <td><asp:Button ID="btnFilterApply" runat="server" CssClass="smallText" Text="Apply Filter" OnClick="btnFilterApply_Click" /></td>
                <td>&nbsp;</td>
            </tr>
        </table>
    </asp:Panel>
</div>
<asp:Panel ID="pnlDataGrid" Visible="true" runat="server">
    <Arena:DataGrid ID="dgAttendance" runat="server" Width="100%" ShowFooter="true">
        <Columns>
            <asp:TemplateColumn HeaderText="Name" SortExpression="common_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <a href='default.aspx?page=<%# PersonDetailPageID %>&guid=<%# DataBinder.Eval(Container.DataItem, "guid") %>'><%# DataBinder.Eval(Container.DataItem, "common_name") %></a>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn HeaderText="Name" DataField="common_name" SortExpression="common_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
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
            <asp:TemplateColumn HeaderText="Re-Print" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:LinkButton ID="lbAttendanceRePrint" runat="server" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "occurrence_attendance_id") %>' OnCommand="btnRePrint_Click">Re-Print Labels</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateColumn>
        </Columns>
    </Arena:DataGrid>
</asp:Panel>
