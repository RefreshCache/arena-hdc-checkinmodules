<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceOverview.ascx.cs" CodeBehind="AttendanceOverview.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.AttendanceOverview" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<asp:HiddenField ID="hfFilterTypeGroupID" runat="server" Value="-1" />
<asp:HiddenField ID="hfFilterService" runat="server" Value="-1" />
<asp:HiddenField ID="hfCloseOccurrenceIDs" runat="server" Value="" />

<asp:Panel ID="pnlCloseOccurrence" Visible="false" runat="server">
    <div class="smallText">
        The following occurrences have been selected for closing. Please select the locations
        you would like to open up instead for each type of occurrence.
    </div>
    <div class="smallText"><asp:Label ID="lbCloseError" runat="server" ForeColor="Red"></asp:Label></div>
    <asp:Table ID="tblClose" runat="server" Width="100%">
        <asp:TableHeaderRow>
            <asp:TableHeaderCell HorizontalAlign="Left" style="display: none;">ID</asp:TableHeaderCell>
            <asp:TableHeaderCell HorizontalAlign="Left">Occurrence</asp:TableHeaderCell>
            <asp:TableHeaderCell HorizontalAlign="Left">Occurence Type</asp:TableHeaderCell>
            <asp:TableHeaderCell HorizontalAlign="Left">Current Location</asp:TableHeaderCell>
            <asp:TableHeaderCell HorizontalAlign="Left">New Location</asp:TableHeaderCell>
        </asp:TableHeaderRow>
        <asp:TableRow>
            <asp:TableCell style="display: none;">1</asp:TableCell>
            <asp:TableCell>2</asp:TableCell>
            <asp:TableCell>3</asp:TableCell>
            <asp:TableCell>4</asp:TableCell>
            <asp:TableCell>5</asp:TableCell>
        </asp:TableRow>
        <asp:TableFooterRow>
            <asp:TableCell ColumnSpan="3">&nbsp;</asp:TableCell>
            <asp:TableCell CssClass="smallText">
                <asp:LinkButton ID="lbCloseCancel" runat="server" OnClick="btnCloseCancel_Click">Cancel</asp:LinkButton>
            </asp:TableCell>
            <asp:TableCell CssClass="smallText">
                <asp:LinkButton ID="lbCloseFinish" runat="server" OnClick="btnCloseFinish_Click">Close Occurrence(s)</asp:LinkButton>
            </asp:TableCell>
        </asp:TableFooterRow>
    </asp:Table>
</asp:Panel>

<div class="listFilter">
    <asp:Panel ID="pnlDataFilter" Visible="true" runat="server" DefaultButton="btnFilterApply">
        <table cellpadding="0" cellspacing="3" border="0" style="margin: 5px;">
            <tr>
                <td valign="top" rowspan="4" align="left" style="padding-left: 10px; padding-top: 10px;"><img src="images/filter.gif" alt="Filter" border="0" /></td>
                <td class="formLabel" align="right">Attendance Group</td>
                <td><asp:DropDownList ID="ddlFilterTypeGroup" runat="server" CssClass="formItem" OnSelectedIndexChanged="ddlFilterTypeGroup_Changed" AutoPostBack="true"/></td>
            </tr>
            <tr>
                <td class="formLabel" align="right">Service Time</td>
                <td><asp:DropDownList ID="ddlFilterService" runat="server" CssClass="formItem" /></td>
            </tr>
            <tr>
                <td><asp:Button ID="btnFilterApply" runat="server" CssClass="smallText" Text="Apply Filter" OnClick="btnFilterApply_Click" /></td>
                <td>&nbsp;</td>
            </tr>
        </table>
    </asp:Panel>
</div>
<br />
<asp:Panel ID="pnlTotalAttendance" Visible="true" runat="server">
    <asp:Label ID="lbTotalAttendance" runat="server" CssClass="smallText"></asp:Label>
    <asp:HyperLink ID="hlDetails" runat="server" style="font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 10px;"></asp:HyperLink>
</asp:Panel>
<br />
<asp:Panel ID="pnlLocationGrid" Visible="true" runat="server">
    <Arena:DataGrid ID="dgLocation" runat="server" Width="100%" ShowFooter="true">
        <Columns>
            <asp:TemplateColumn HeaderText="Location" SortExpression="building_location_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <a href="default.aspx?page=<%# Server.UrlEncode(AttendanceDetailPageID.ToString()) %>&groupID=<%# Server.UrlEncode(hfFilterTypeGroupID.Value) %>&service=<%# Server.UrlEncode(hfFilterService.Value) %>&locationID=<%# Server.UrlEncode(DataBinder.Eval(Container.DataItem, "location_id").ToString()) %>"><%# DataBinder.Eval(Container.DataItem, "building_location_name") %></a>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn HeaderText="Location" DataField="building_location_name" SortExpression="building_location_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Attendance" DataField="attendance_count" SortExpression="attendance_count" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Active Occurrences" DataField="active_occurrence_count" SortExpression="active_occurrence_count" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:TemplateColumn HeaderText="Close" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:LinkButton ID="lbClose" CommandName="Close" OnCommand="dgLocation_Close" Visible="true" runat="server">Close</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateColumn>
        </Columns>
    </Arena:DataGrid>
</asp:Panel>
<br />
<asp:Panel ID="pnlOccurrenceGrid" Visible="true" runat="server">
    <Arena:DataGrid ID="dgOccurrence" runat="server" Width="100%" ShowFooter="true">
        <Columns>
            <asp:TemplateColumn HeaderText="Occurrence" SortExpression="occurrence_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <a href="default.aspx?page=<%# Server.UrlEncode(AttendanceDetailPageID.ToString()) %>&groupID=<%# Server.UrlEncode(hfFilterTypeGroupID.Value) %>&service=<%# Server.UrlEncode(hfFilterService.Value) %>&occurrenceID=<%# Server.UrlEncode(DataBinder.Eval(Container.DataItem, "occurrence_id").ToString()) %>"><%# DataBinder.Eval(Container.DataItem, "occurrence_name") %></a>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn HeaderText="Occurrence" DataField="occurrence_name" SortExpression="occurrence_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:TemplateColumn HeaderText="Attendance Type" SortExpression="occurrence_group_type_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <a href="default.aspx?page=<%# Server.UrlEncode(AttendanceDetailPageID.ToString()) %>&groupID=<%# Server.UrlEncode(hfFilterTypeGroupID.Value) %>&service=<%# Server.UrlEncode(hfFilterService.Value) %>&typeID=<%# Server.UrlEncode(DataBinder.Eval(Container.DataItem, "occurrence_type_id").ToString()) %>"><%# DataBinder.Eval(Container.DataItem, "occurrence_group_type_name") %></a>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn HeaderText="Attendance Type" DataField="occurrence_group_type_name" SortExpression="occurrence_group_type_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:TemplateColumn HeaderText="Checkin Start" SortExpression="check_in_start" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:Label><%# DateTime.Parse((String)DataBinder.Eval(Container.DataItem, "check_in_start")).ToString("T") %></asp:Label>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="Checkin End" SortExpression="check_in_end" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:Label><%# DateTime.Parse((String)DataBinder.Eval(Container.DataItem, "check_in_end")).ToString("T") %></asp:Label>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn HeaderText="Location" DataField="building_location_name" SortExpression="building_location_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Attendance" DataField="attendance_count" SortExpression="attendance_count" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top"></asp:BoundColumn>
            <asp:TemplateColumn HeaderText="Close" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:LinkButton ID="lbClose" CommandName="Close" OnCommand="dgOccurrence_Close" Visible="true" runat="server">Close</asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateColumn>
        </Columns>
    </Arena:DataGrid>
</asp:Panel>
