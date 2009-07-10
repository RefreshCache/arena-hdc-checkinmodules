<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyRegistration.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.HDC.CheckIn.FamilyRegistration" %>
<%@ Register TagPrefix="Arena" Namespace="Arena.Portal.UI" Assembly="Arena.Portal.UI" %>

<asp:HiddenField ID="hfGradeRoleIDs" runat="server" />
<asp:HiddenField ID="hfEmailRoleIDs" runat="server" />
<asp:HiddenField ID="hfFamily" runat="server" />
<asp:HiddenField ID="hfAllowFriend" runat="server" />
<asp:HiddenField ID="hfExtraCount" runat="server" Value="0" />

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
    document.getElementById('findFamilyExpanded').style.display = '';
    document.getElementById('findFamilyCollapsed').style.display = 'none';
}

function hideFindFamilyContent()
{
    document.getElementById('findFamilyExpanded').style.display = 'none';
    document.getElementById('findFamilyCollapsed').style.display = '';
}

function showFamilyFriend()
{
    hdc_getElementById('pnlFamilyFriend').style.display = '';
}

function hideFamilyFriend()
{
    hdc_getElementById('pnlFamilyFriend').style.display = 'none';
}

function hdc_getElementById(elementID)
{
    var i, obj, form = document.forms[0], elements;


    for (i = 0; i < form.length; i++)
    {
        obj = form.elements[i];

        if (obj.id.indexOf(elementID) != -1 && obj.id.substring(obj.id.indexOf(elementID)) == elementID)
            return obj;
    }

    for (i = 0; i < document.links.length; i++)
    {
        obj = document.links[i];

        if (obj.id.indexOf(elementID) != -1 && obj.id.substring(obj.id.indexOf(elementID)) == elementID)
            return obj;
    }
    
    elements = document.getElementsByTagName('*');
    for (i = 0; i < elements.length; i++)
    {
        obj = elements[i];

        if (obj.id.indexOf(elementID) != -1 && obj.id.substring(obj.id.indexOf(elementID)) == elementID)
            return obj;
    }

    return null;
}

function hdc_inarray(arrayObj, value)
{
    var i;
    
    for (i = 0; i < arrayObj.length; i++)
    {
        if (arrayObj[i] == value)
            return true;
    }
    
    return false;
}

function hdc_toggleFamilyAttributes()
{
    var i, role, x, value, lead;
    var grade, email, maritalStatus, anniversary, anniversaryCal, dateCal;
    var gradeRoles, emailRoles;

    emailRoles = hdc_getElementById('hfEmailRoleIDs').value.split(',');
    gradeRoles = hdc_getElementById('hfGradeRoleIDs').value.split(',');

    for (i = 0; document.forms[0].length; i++)
    {
        role = document.forms[0].elements[i];
        if (role == null)
            break;

        if (role.id.indexOf('ddlMemberFamilyRole_') != -1)
        {
            lead = role.id.split('ddlMember')[0];
            value = role.options[role.selectedIndex].value;
            x = role.id.split('Role_')[1];

            //
            // Always hide the calendar date pickers.
            //
            document.getElementById(lead + 'dtbMemberBirthDate_' + x + '_calImage').style.display = 'none';
            document.getElementById(lead + 'dtbMemberAnniversaryDate_' + x + '_calImage').style.display = 'none';

            //
            // Handle marital status and anniversary date.
            //
            maritalStatus = document.getElementById(lead + 'ddlMemberMaritalStatus_' + x);
            anniversary = document.getElementById(lead + 'dtbMemberAnniversaryDate_' + x);
            if (role.options[role.selectedIndex].text.toLowerCase() == 'adult')
            {
                maritalStatus.style.display = 'inline';
                anniversary.style.display = 'inline';
            }
            else
            {
                maritalStatus.style.display = 'none';
                anniversary.style.display = 'none';
            }

            //
            // Handle grades.
            //
            grade = document.getElementById(lead + 'ddlMemberGrade_' + x);
            if (hdc_inarray(gradeRoles, value))
                grade.style.display = 'inline';
            else
                grade.style.display = 'none';

            //
            // Handle e-mail addresses.
            //
            email = document.getElementById(lead + 'tbMemberEmail_' + x);
            if (hdc_inarray(emailRoles, value))
                email.style.display = 'inline';
            else
                email.style.display = 'none';
        }
    }

    //
    // Ensure the date picker for family friend is hidden.
    //
    dateCal = document.getElementById(lead + 'dtbFriendBirthDate_calImage');
    if (dateCal)
        dateCal.style.display = 'none';
}

//
// Expand or collapse the extra fields information of a member.
//
function hdc_toggleExtraFields(ctl)
{
    var elements, memberIdx, lead, obj;

    //
    // Find the member number we are concerned with.
    //
    if (ctl.id.search('imgMember') != -1)
    {
        lead = ctl.id.split('imgMember')[0];
        elements = ctl.id.split('_');
        memberIdx = elements[elements.length - 1];
        obj = document.getElementById(lead + 'trMemberExtraFields_' + memberIdx);
    }
    else if (ctl.id.search('imgFriend') != -1)
    {
        lead = ctl.id.split('imgFriend')[0];
        obj = document.getElementById(lead + 'trFriendExtraFields');
    }

    //
    // Find the extra fields object we are concerned with and toggle them.
    //
    if (obj.style.display == 'none')
    {
        obj.style.display = '';
        ctl.src = ctl.src.substring(0, ctl.src.indexOf('information2.gif')) + 'cancel.gif';
    }
    else
    {
        obj.style.display = 'none';
        ctl.src = ctl.src.substring(0, ctl.src.indexOf('cancel.gif')) + 'information2.gif';
    }
}

//
// Detect if the user pressed enter and if so initiate the search.
//
function hdc_checkSearchSubmit(e)
{
    var keyCode;

    keyCode = (e && e.which ? e.which : event.keyCode);

    if (keyCode == 13)
    {
        var search = hdc_getElementById('btnSearch');
        
        if (search)
            __doPostBack(search.id.replace(/_/g, '$'), '');

        return false;
    }
}

//
// Partially fade the message text in the given object.
//
function hdc_fadeObjectOut(element)
{
    var value, o = document.getElementById(element);

    if (o.filters)
    {
        o.style.visibility = 'hidden';
        return;
    }
    if (o.style.opacity == '')
        value = 1;
    else
        value = parseFloat(o.style.opacity);

    value -= 0.02;
    if (value < 0)
    {
        o.style.opacity = '0';
        o.style.visibility = 'hidden';
    }
    else
    {
        o.style.opacity = value;
        setTimeout('hdc_fadeObjectOut(\'' + element + '\')', 20);
    }
}

</script>

<asp:Panel id="pnlFindFamily" runat="server" Visible="false" Width="100%">
<table cellpadding="0" cellspacing="0" border="0" width="100%" style="margin-bottom: 30px; table-layout: fixed;">
    <tr>
        <td align="center" style="width: 150px; font-size: 12px; font-weight: bold; background-color: #f0f0ff; padding-top: 4px; padding-bottom: 4px; border-top: solid 2px gray; border-left: solid 2px gray; border-right: solid 2px gray;"><span style="cursor: pointer;" onclick="toggleFindFamilyContent();" >Find Family</span></td>
        <td>&nbsp;</td>
        <td style="width: 50px;">&nbsp;</td>
    </tr>
    <tr>
        <td colspan="3">
            <table id="findFamilyExpanded" cellpadding="0" cellspacing="0" border="0" width="100%" style="table-layout: fixed;">
                <tr>
                    <td style="width: 148px; background-color: #f0f0ff; padding-left: 2px; border-left: solid 2px gray; font-size: 1px;">&nbsp;</td>
                    <td style="border-bottom: solid 2px gray; border-left: solid 2px gray; font-size: 1px;">&nbsp;</td>
                    <td style="width: 50px; font-size: 1px;">&nbsp;</td>
                </tr>
                <tr>
                    <td colspan="2" style="background-color: #f0f0ff; padding: 4px; border-left: solid 2px gray; border-right: solid 2px gray; border-bottom: solid 2px gray;">
                        <table border="0" cellpadding="2" cellspacing="0" width="100%" style="margin-top: 10px; table-layout: fixed;">
                            <tr>
                                <td style="width: 150px; font-size: 1px;">&nbsp;</td>
                                <td style="font-size: 1px;">&nbsp;</td>
                            </tr>
                            <tr>
                                <td colspan="2">Please enter only one search criteria at a time.</td>
                            </tr>
                            <tr>
                                <td align="right" class="smallText" style="width: 150px;">Name:<br />(Partial searches allowed)</td>
                                <td align="left"><asp:TextBox ID="tbFindName" onkeydown="return hdc_checkSearchSubmit(event);" runat="server" Width="200px" MaxLength="100"></asp:TextBox></td>
                            </tr>
                            <tr>
                                <td align="right" class="smallText" style="width: 150px;">Phone:<br />(Partial searches allowed)</td>
                                <td align="left"><asp:TextBox ID="tbFindPhone" onkeydown="return hdc_checkSearchSubmit(event);" runat="server" Width="130px" MaxLength="14"></asp:TextBox></td>
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
            <table id="findFamilyCollapsed" cellpadding="0" border="0" cellspacing="0" width="100%" style="display: none; table-layout: fixed;">
                <tr>
                    <td style="width: 150px; background-color: #f0f0ff; border-bottom: solid 2px gray; border-left: solid 2px gray; border-right: solid 2px gray; font-size: 1px;">&nbsp;</td>
                    <td colspan="2" style="font-size: 1px;">&nbsp;</td>
                </tr>
            </table>
        </td>
    </tr>
</table>
</asp:Panel>

<asp:Panel ID="pnlFindResults" runat="server" Visible="false">
    <table cellpadding="0" cellspacing="0" border="0" width="100%" style="margin-bottom: 30px; table-layout: fixed;">
        <tr>
            <td align="center" style="width: 150px; font-size: 12px; font-weight: bold; background-color: #f0f0ff; padding: 4px; border-top: solid 2px gray; border-left: solid 2px gray; border-right: solid 2px gray;">Search Results</td>
            <td style="border-bottom: solid 2px gray;">&nbsp;</td>
            <td style="width: 50px;">&nbsp;</td>
        </tr>
        <tr>
            <td colspan="2" style="background-color: #f0f0ff; padding: 4px; border-left: solid 2px gray; border-right: solid 2px gray; border-bottom: solid 2px gray;">
                <table border="0" cellpadding="2" cellspacing="0" width="100%">
                    <tr>
                        <td align="left" class="smallText" style="font-weight: bold;">Name</td>
                        <td align="left" class="smallText" style="font-weight: bold;">Gender</td>
                        <td align="left" class="smallText" style="font-weight: bold;">Age</td>
                        <td align="left" class="smallText" style="font-weight: bold;">Grade</td>
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
    <table cellpadding="0" cellspacing="0" border="0" width="100%" style="margin-bottom: 30px; table-layout: fixed;">
        <tr>
            <td align="center" style="width: 150px; font-size: 12px; font-weight: bold; background-color: #f0f0ff; padding: 4px; border-top: solid 2px gray; border-left: solid 2px gray; border-right: solid 2px gray;">Family Information</td>
            <td style="border-bottom: solid 2px gray;">&nbsp;</td>
            <td style="width: 50px;">&nbsp;</td>
        </tr>
        <tr>
            <td colspan="2" style="background-color: #f0f0ff; padding: 4px; border-left: solid 2px gray; border-right: solid 2px gray; border-bottom: solid 2px gray;">
                <table cellpadding="0" cellspacing="2" align="center" border="0" width="100%" style="padding-top: 10px;">
                    <asp:PlaceHolder ID="phFamilySaveMessage" runat="server"></asp:PlaceHolder>
                    <tr>
                        <td align="left" class="smallText" style="font-weight: bold; width: 50%;">Family Name:&nbsp;<asp:TextBox ID="tbFamilyName" runat="server" CssClass="smallText" MaxLength="100" Width="150px" TabIndex="0"></asp:TextBox></td>
                        <td align="left" class="smallText" width: 50%;"><table border="0" cellpadding="0" cellspacing="0"><tr><td align="center"><span style="font-weight: bold;">Main/Home Phone:&nbsp;<br /></span>(Applies to all)</td><td><Arena:PhoneTextBox ID="tbMainPhone" runat="server" CssClass="smallText" ShowExtension="false" Width="100px" Required="false" /><asp:CheckBox ID="cbMainPhoneUnlisted" runat="server" CssClass="smallText" Text="(unlisted)" Checked="false" /></td></tr></table></td>
                    </tr>
                </table>
                <table cellpadding="0" cellspacing="4" align="center" border="0" width="100%" style="margin-bottom: 10px;">
                    <tr>
                        <td align="left" class="smallText" style="width: 205px;"><span style="font-weight: bold;">Main/Home Address</span><span> (Applies to all)</span></td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 105px;">City</td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 40px">St</td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 95px;">Postal Code</td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 170px;">Country</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td class="smallText"><asp:TextBox ID="tbAddressLine1" width="200px" runat="server" CssClass="smallText" MaxLength="100"></asp:TextBox></td>
                        <td class="smallText"><asp:TextBox ID="tbAddressCity" width="100px" runat="server" CssClass="smallText" MaxLength="100"></asp:TextBox></td>
                        <td class="smallText"><asp:TextBox ID="tbAddressState" width="35px" runat="server" CssClass="smallText" MaxLength="2"></asp:TextBox></td>
                        <td class="smallText"><asp:TextBox ID="tbAddressPostalCode" width="90px" runat="server" CssClass="smallText" MaxLength="10"></asp:TextBox></td>
                        <td class="smallText"><asp:DropDownList ID="ddlAddressCountry" runat="server" CssClass="smallText" Width="120" Height="18"></asp:DropDownList><asp:PlaceHolder ID="phAddressCountry" runat="server"></asp:PlaceHolder></td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td class="smallText"><asp:TextBox ID="tbAddressLine2" width="200px" runat="server" CssClass="smallText" MaxLength="100"></asp:TextBox></td>
                    </tr>
                </table>
                <span class="smallText" style="font-size: 11px; font-weight: bold;">Family Members:</span>
                <table cellpadding="0" cellspacing="0" align="center" border="0" width="100%" style="margin-bottom: 15px;">
                    <tr>
                        <td style="width: 0px"></td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Title</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">First Name</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Last Name</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Family<br />Role</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Gender</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Birth Date</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Grade</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Marital<br />Status</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Anniversary<br />Date</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">E-mail</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold"></td>
                    </tr>
                    <asp:PlaceHolder id="phFamilyMembers" runat="server"></asp:PlaceHolder>
                </table>

                <table border="0" cellpadding="0" cellspacing="0" width="100%">
                    <tr>
                        <td align="left" width="100px"><asp:LinkButton ID="btnAddMore" runat="server" CssClass="smallText" Text="Add More"></asp:LinkButton></td>
                        <td align="left" width="100px"><a href="#" id="linkShowFriend" class="smallText" onclick="showFamilyFriend(); return false;" style="display: none;">Add Friend</a></td>
                        <td align="right"><asp:LinkButton ID="btnSaveFamily" runat="server" CssClass="smallText" Text="Save Family"></asp:LinkButton></td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</asp:Panel>

<asp:Panel id="pnlFamilyFriend" runat="server" Visible="true" style="display: none;">
    <table cellpadding="0" cellspacing="0" border="0" width="100%" style="margin-bottom: 30px; table-layout: fixed;">
        <tr>
            <td align="center" style="width: 150px; font-size: 12px; font-weight: bold; background-color: #f0f0ff; padding: 4px; border-top: solid 2px gray; border-left: solid 2px gray; border-right: solid 2px gray;">Add Family Friend</td>
            <td style="border-bottom: solid 2px gray;">&nbsp;</td>
            <td style="width: 50px;">&nbsp;</td>
        </tr>
        <tr>
            <td colspan="2" style="background-color: #f0f0ff; padding: 4px; border-left: solid 2px gray; border-right: solid 2px gray; border-bottom: solid 2px gray;">
                <table cellpadding="0" cellspacing="0" align="center" border="0" style="width: 100%; margin-bottom: 15px;">
                    <asp:PlaceHolder ID="phFamilyFriendSaveMessage" runat="server"></asp:PlaceHolder>
                    <tr>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Title</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">First Name</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Last Name</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Gender</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Birth Date</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Grade</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: normal">E-mail</td>
                        <td align="left" valign="bottom" class="smallText" style="font-weight: bold">Phone</td>
                    </tr>
                    <asp:PlaceHolder ID="phFamilyFriend" runat="server"></asp:PlaceHolder>
                </table>

                <table cellpadding="0" cellspacing="4" align="center" border="0" width="100%" style="margin-bottom: 15px;">
                    <tr>
                        <td align="left" class="smallText" style="width: 205px;"><span style="font-weight: bold;">Main/Home Address</span></td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 105px;">City</td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 40px">St</td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 95px;">Postal Code</td>
                        <td align="left" class="smallText" style="font-weight: bold; width: 170px;">Country</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td class="smallText"><asp:TextBox ID="tbFriendAddressLine1" width="200px" runat="server" CssClass="smallText" MaxLength="100"></asp:TextBox></td>
                        <td class="smallText"><asp:TextBox ID="tbFriendAddressCity" width="100px" runat="server" CssClass="smallText" MaxLength="100"></asp:TextBox></td>
                        <td class="smallText"><asp:TextBox ID="tbFriendAddressState" width="35px" runat="server" CssClass="smallText" MaxLength="2"></asp:TextBox></td>
                        <td class="smallText"><asp:TextBox ID="tbFriendAddressPostalCode" width="90px" runat="server" CssClass="smallText" MaxLength="10"></asp:TextBox></td>
                        <td class="smallText"><asp:DropDownList ID="ddlFriendAddressCountry" runat="server" CssClass="smallText" Width="120" Height="18"></asp:DropDownList><asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder></td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td class="smallText"><asp:TextBox ID="tbFriendAddressLine2" width="200px" runat="server" CssClass="smallText" MaxLength="100"></asp:TextBox></td>
                    </tr>
                </table>

                <table border="0" cellpadding="0" cellspacing="0" width="100%">
                    <tr>
                        <td align="left"><a href="#" class="smallText" onclick="hideFamilyFriend(); return false;">Cancel</a></td>
                        <td align="right"><asp:LinkButton ID="btnSaveFriend" runat="server" CssClass="smallText" Text="Save Friend"></asp:LinkButton></td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</asp:Panel>

<script type="text/javascript" language="javascript">
var obj, family, allowFriend;

//
// Auto-load: Ensure the "Add Friend" link only shows if we have
// an existing family.
//
obj = hdc_getElementById('linkShowFriend');
family = hdc_getElementById('hfFamily');
allowFriend = hdc_getElementById('hfAllowFriend');
if (obj && family.value != '' && family.value != '-1' && allowFriend.value == '1')
    obj.style.display = '';
</script>
