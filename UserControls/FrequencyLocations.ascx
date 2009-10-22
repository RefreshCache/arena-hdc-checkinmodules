<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FrequencyLocations.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.FrequencyLocations" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<asp:Panel ID="pnlOverview" Visible="true" runat="server">
    <Arena:DataGrid ID="dgTemplateLocations" runat="server" Width="100%" ShowFooter="true" OnEditCommand="dgTemplateLocations_EditCommand">
        <Columns>
            <asp:BoundColumn HeaderText="ID" DataField="template_id" Visible="false"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Location IDs" DataField="location_ids" Visible="false"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Name" DataField="schedule_name" Visible="true" HeaderStyle-Wrap="true" HeaderStyle-VerticalAlign="Bottom"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Frequency" DataField="occurrence_freq_type_name" Visible="true" HeaderStyle-Wrap="true" HeaderStyle-VerticalAlign="Bottom"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Frequency<br />Details" DataField="freq_qualifier_name" Visible="true" HeaderStyle-Wrap="true" HeaderStyle-VerticalAlign="Bottom"></asp:BoundColumn>
            <asp:TemplateColumn HeaderText="Start Time" Visible="true" HeaderStyle-Wrap="true" HeaderStyle-VerticalAlign="Bottom">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# DateTime.Parse(DataBinder.Eval(Container.DataItem, "start_time").ToString()).ToString("h:mm tt") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="Check-In<br />Start Time" Visible="true" HeaderStyle-Wrap="true" HeaderStyle-VerticalAlign="Bottom">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# DateTime.Parse(DataBinder.Eval(Container.DataItem, "check_in_start").ToString()).ToString("h:mm tt") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn HeaderText="Locations" DataField="location_names" Visible="true" HeaderStyle-Wrap="true" HeaderStyle-VerticalAlign="Bottom"></asp:BoundColumn>
        </Columns>
    </Arena:DataGrid>
</asp:Panel>

<asp:Panel ID="pnlEdit" Visible="false" runat="server">
    <asp:Label ID="lbEditTemplate" runat="server" CssClass="smallText"></asp:Label>
    <br />
    <asp:CheckBoxList ID="cblLocations" runat="server" CssClass="smallText" CellPadding="5" CellSpacing="5" RepeatColumns="3" RepeatDirection="Horizontal" RepeatLayout="Table" TextAlign="Right">
    </asp:CheckBoxList>

    <asp:Button ID="btnEditCancel" style="float: left;" Text="Cancel" OnClick="btnEditCancel_Click" runat="server" />
    <asp:Button ID="btnEditSave" style="float: right;" Text="Save" OnClick="btnEditSave_Click" runat="server" />
</asp:Panel>

<asp:Label ID="lbLog" runat="server" />
