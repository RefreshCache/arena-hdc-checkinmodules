<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyRegistration.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.FamilyRegistration" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<asp:HiddenField ID="hfFindName" runat="server" />
<asp:HiddenField ID="hfFindPhone" runat="server" />
<asp:HiddenField ID="hfFamily" runat="server" />
<asp:HiddenField ID="hfExtraCount" runat="server" Value="0" />
<asp:HiddenField ID="hfStage" runat="server" />

<script type="text/javascript" language="javascript">
function toggleFindFamilyContent()
{
    if (document.getElementById('findFamilyExpanded').style.display == 'none')
        showFindFamilyContent();
    else
        hideFindFamilyContent();
}

function showFindFamilyContent()
{
    document.getElementById('findFamilyExpanded').style.display = 'table';
    document.getElementById('findFamilyCollapsed').style.display = 'none';
}

function hideFindFamilyContent()
{
    document.getElementById('findFamilyExpanded').style.display = 'none';
    document.getElementById('findFamilyCollapsed').style.display = 'table';
}
</script>

<asp:Panel id="pnlFindFamily" runat="server" Visible="false" Width="100%">
<table cellpadding="0" cellspacing="0" border="0" width="100%" style="margin-bottom: 30px;">
    <tr>
        <td align="center" style="width: 150px; font-size: 12px; font-weight: bold; background-color: #f0f0ff; padding: 4px; border-top: solid 2px gray; border-left: solid 2px gray; border-right: solid 2px gray;"><span style="cursor: pointer;" onclick="toggleFindFamilyContent();" >Find Family</span></td>
        <td>&nbsp;</td>
        <td width="10%">&nbsp;</td>
    </tr>
    <tr>
        <td colspan="3">
            <table id="findFamilyExpanded" cellpadding="0" cellspacing="0" border="0" width="100%">
                <tr>
                    <td width="158" style="background-color: #f0f0ff; border-left: solid 2px gray;"></td>
                    <td style="border-bottom: solid 2px gray; border-left: solid 2px gray;"></td>
                    <td width="10%"></td>
                </tr>
                <tr>
                    <td colspan="2" style="background-color: #f0f0ff; padding: 4px; border-left: solid 2px gray; border-right: solid 2px gray; border-bottom: solid 2px gray;">
                        <table border="0" cellpadding="2" cellspacing="0" width="100%" style="margin-top: 10px;">
                            <tr>
                                <td colspan="2">Please enter only one search criterea at a time.</td>
                            </tr>
                            <tr>
                                <td align="right" class="smallText" style="width: 150px;">Name:<br />(Partial searches allowed)</td>
                                <td align="left"><asp:TextBox ID="tbFindName" runat="server" Width="200px" MaxLength="100"></asp:TextBox></td>
                            </tr>
                            <tr>
                                <td align="right" class="smallText" style="width: 150px;">Phone:<br />(Partial searches allowed)</td>
                                <td align="left"><asp:TextBox ID="tbFindPhone" runat="server" Width="130px" MaxLength="14"></asp:TextBox></td>
                            </tr>
                            <tr><td class="smallText">&nbsp;</td><td class="smallText">&nbsp;</td></tr>
                            <tr>
                                <td align="left"><asp:LinkButton ID="btnSearch" runat="server" CssClass="smallText" Text="Search"></asp:LinkButton></td>
                                <td align="right"><asp:LinkButton ID="btnNewFamily" runat="server" CssClass="smallText" Text="Add New Family"></asp:LinkButton></td>
                            </tr>
                        </table>
                    </td>
                    <td>&nbsp;</td>
                </tr>
            </table>
            <table id="findFamilyCollapsed" cellpadding="0" border="0" cellspacing="0" width="100%" style="display: none;">
                <tr>
                    <td style="width: 158px; border-bottom: solid 2px gray; border-left: solid 2px gray; border-right: solid 2px gray;"></td>
                    <td colspan="2"></td>
                </tr>
            </table>
        </td>
    </tr>
</table>
</asp:Panel>

<asp:Panel ID="pnlFindResults" runat="server" Visible="false">
    <table cellpadding="0" cellspacing="0" border="0" width="100%" style="margin-bottom: 30px;">
        <tr>
            <td align="center" style="width: 150px; font-size: 12px; font-weight: bold; background-color: #f0f0ff; padding: 4px; border-top: solid 2px gray; border-left: solid 2px gray; border-right: solid 2px gray;">Search Results</td>
            <td style="border-bottom: solid 2px gray;">&nbsp;</td>
            <td style="width: 10%">&nbsp;</td>
        </tr>
        <tr>
            <td colspan="2" style="background-color: #f0f0ff; padding: 4px; border-left: solid 2px gray; border-right: solid 2px gray; border-bottom: solid 2px gray;">
                <table border="0" cellpadding="2" cellspacing="0" width="100%">
                    <tr>
                        <td align="left" class="smallText" style="font-weight: bold;">Name</td>
                        <td align="left" class="smallText" style="font-weight: bold;">Gender</td>
                        <td align="left" class="smallText" style="font-weight: bold;">Age</td>
                        <td align="left" class="smallText" style="font-weight: bold;">Home Phone</td>
                        <td align="left" class="smallText" style="font-weight: bold;">E-Mail</td>
                    </tr>
                    <asp:PlaceHolder ID="phSearchResults" runat="server"></asp:PlaceHolder>
                </table>
            </td>
            <td>&nbsp;</td>
        </tr>
    </table>
</asp:Panel>

<asp:Panel id="pnlFamily" runat="server" Visible="false">
    <table cellpadding="0" cellspacing="0" border="0" width="100%" style="margin-bottom: 30px;">
        <tr>
            <td align="center" style="width: 150px; font-size: 12px; font-weight: bold; background-color: #f0f0ff; padding: 4px; border-top: solid 2px gray; border-left: solid 2px gray; border-right: solid 2px gray;">Family Information</td>
            <td style="border-bottom: solid 2px gray;">&nbsp;</td>
            <td style="width: 10%">&nbsp;</td>
        </tr>
        <tr>
            <td colspan="2" style="background-color: #f0f0ff; padding: 4px; border-left: solid 2px gray; border-right: solid 2px gray; border-bottom: solid 2px gray;">
                <table cellpadding="0" cellspacing="2" align="center" border="0" width="100%" style="padding-top: 10px;">
                    <tr>
                        <td align="left" class="smallText" style="font-weight: bold; width: 50%;">Family Name:&nbsp;<asp:TextBox ID="tbFamilyName" runat="server" CssClass="smallText" MaxLength="100" Width="150px" TabIndex="0"></asp:TextBox></td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 50%;">Main/Home Phone:&nbsp;<Arena:PhoneTextBox ID="tbMainPhone" runat="server" CssClass="smallText" ShowExtension="false" Width="100px" Required="false" /></td>
                    </tr>
                </table>
                <br />
                <table cellpadding="0" cellspacing="4" align="left" border="0">
                    <tr>
                        <td align="left" class="smallText" style="font-weight: bold; width: 205px;">Address</td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 105px;">City</td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 40px">St</td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 95px;">Postal Code</td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 170px;">Country</td>
                    </tr>
                    <tr>
                        <td class="smallText"><asp:TextBox ID="tbAddressLine1" width="200px" runat="server" CssClass="smallText" MaxLength="100"></asp:TextBox></td>
                        <td class="smallText"><asp:TextBox ID="tbAddressCity" width="100px" runat="server" CssClass="smallText" MaxLength="100"></asp:TextBox></td>
                        <td class="smallText"><asp:TextBox ID="tbAddressState" width="35px" runat="server" CssClass="smallText" MaxLength="2"></asp:TextBox></td>
                        <td class="smallText"><asp:TextBox ID="tbAddressPostalCode" width="90px" runat="server" CssClass="smallText" MaxLength="10"></asp:TextBox></td>
                        <td class="smallText"><asp:DropDownList ID="ddlAddressCountry" runat="server" CssClass="smallText" Width="120" Height="18"></asp:DropDownList><asp:PlaceHolder ID="phAddressCountry" runat="server"></asp:PlaceHolder></td>
                    </tr>
                    <tr>
                        <td class="smallText"><asp:TextBox ID="tbAddressLine2" width="200px" runat="server" CssClass="smallText" MaxLength="100"></asp:TextBox></td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>

    <table cellpadding="0" cellspacing="0" border="0" width="100%" style="margin-bottom: 30px;">
        <tr>
            <td align="center" style="width: 150px; font-size: 12px; font-weight: bold; background-color: #f0f0ff; padding: 4px; border-top: solid 2px gray; border-left: solid 2px gray; border-right: solid 2px gray;">Family Members</td>
            <td style="border-bottom: solid 2px gray;">&nbsp;</td>
            <td style="width: 10%">&nbsp;</td>
        </tr>
        <tr>
            <td colspan="2" style="background-color: #f0f0ff; padding: 4px; border-left: solid 2px gray; border-right: solid 2px gray; border-bottom: solid 2px gray;">
                <table id="tblFamily" cellpadding="0" cellspacing="0" align="center" border="0" style="width: 100%; margin-bottom: 15px;">
                    <tr>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Title</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">First Name</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Last Name</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Family<br />Role</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Birth Date</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Gender</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Member<br />Status</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Marital<br />Status</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Anniversary<br />Date</td>
                    </tr>
                    <asp:PlaceHolder id="phFamilyMembers" runat="server"></asp:PlaceHolder>
                </table>

                <asp:LinkButton ID="btnAddMore" runat="server" CssClass="smallText" Text="Add More"></asp:LinkButton>
            </td>
        </tr>
    </table>
</asp:Panel>
