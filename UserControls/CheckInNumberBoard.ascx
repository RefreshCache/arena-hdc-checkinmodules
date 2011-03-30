<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckInNumberBoard.ascx.cs" CodeBehind="CheckInNumberBoard.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.CheckInNumberBoard" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<asp:Label ID="lbStatus" runat="server"></asp:Label>
<asp:Panel ID="pnlNoteGrid" Visible="true" runat="server">
    <Arena:DataGrid id="dgNote" runat="server" width="100%" onrebind="dgNote_ReBind" onitemdatabound="dgNote_ItemDataBound" onupdatecommand="dgNote_Update" ondeletecommand="dgNote_Delete" showfooter="false" exportenabled="false" addenabled="false">
        <Columns>
            <asp:TemplateColumn HeaderText="Number" SortExpression="number" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top">
                <ItemTemplate>
                    <asp:Label ID="lbNumber" runat="server"><%# DataBinder.Eval(Container.DataItem, "number_string") %></asp:Label>
                    <asp:Label ID="lbPromotionID" runat="server" style="display: none;"><%# DataBinder.Eval(Container.DataItem, "promotion_id").ToString() %></asp:Label>
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:TemplateColumn HeaderText="Show Name">
                <ItemTemplate>
                    <asp:Image ID="imgShowName" runat="server" />
                    <asp:CheckBox ID="cbEditShowName" runat="server" />
                </ItemTemplate>
            </asp:TemplateColumn>
            <asp:BoundColumn HeaderText="Person" DataField="full_name" SortExpression="sort_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top" ReadOnly="true"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Location" DataField="location_name" SortExpression="location_name" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top" ReadOnly="true"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Date Posted" DataField="created_string" SortExpression="created_date" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top" ReadOnly="true"></asp:BoundColumn>
            <asp:BoundColumn HeaderText="Posted By" DataField="created_by" SortExpression="created_by" Visible="true" HeaderStyle-Wrap="false" HeaderStyle-VerticalAlign="Top" ReadOnly="true"></asp:BoundColumn>
        </Columns>
    </Arena:DataGrid>
</asp:Panel>

<asp:Panel ID="pnlAdd" Visible="true" runat="server" style="margin-top: 15px;">
    <asp:Label class="smallText">Post number: </asp:Label><asp:TextBox ID="tbAddNumber" runat="server" CssClass="smallText"></asp:TextBox><Arena:ArenaButton ID="btnAddPost" runat="server" OnClick="btnAddPost_Click" Text="Post" /><br />
</asp:Panel>
