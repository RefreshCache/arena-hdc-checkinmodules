
/**********************************************************************
* Description:	This module provides the ability to quickly register a
*               family at check-in time via a web interface.
* Created By:	Daniel Hazelbaker, High Desert Church
* Date Created:	5/19/2009 10:27:43 AM
*
* $Workfile: $
* $Revision: $ 
* $Header: $
* 
* $Log: $
**********************************************************************/

namespace ArenaWeb.UserControls.Custom.HDC.CheckIn
{
	using System;
	using System.Data;
	using System.Configuration;
	using System.Collections;
	using System.Collections.Generic;
    using System.ComponentModel;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;
	using System.Web.UI.WebControls.WebParts;
	using System.Web.UI.HtmlControls;
    using System.Reflection;
    using System.Text;
	using Arena.Portal;
	using Arena.Core;
    using Arena.Portal.UI;
    using Arena.Security;
    using Arena.Organization;

	public partial class FamilyRegistration : PortalControl
    {
        #region Module Settings

        [BooleanSetting("Allow Searching", "Allows searches to be performed to look families up, also implies limited edit ability.", true, false)]
        public bool AllowSearchSetting { get { return Convert.ToBoolean(Setting("AllowSearch", "false", true)); } }

        [TextSetting("Country Code", "The default country code (e.g. US) to use when adding new families", false)]
        public string CountryCodeSetting { get { return Setting("CountryCode", "", false); } }

        [NumericSetting("New Family Size", "The number of family members to provide space for when adding a new family. The default is 4.", false)]
        public int NewFamilySizeSetting { get { return Convert.ToInt32(Setting("NewFamlySize", "4", false)); } }

        [NumericSetting("Add More Count", "The number of blank member entries to add each time the Add More button is clicked. The default is 2.", false)]
        public int AddMoreCountSetting { get { return Convert.ToInt32(Setting("AddMoreCount", "2", false)); } }

        [ListFromSqlSetting("Grade Family Roles", "The family roles that you want grades to be entered for.", false, "",
            "SELECT [cl].[lookup_id],[cl].[lookup_value] FROM [core_lookup] AS [cl] LEFT JOIN [core_lookup_type] AS [clt] ON [clt].[lookup_type_id]=[cl].[lookup_type_id] WHERE [clt].[guid]='D3CE5E62-4EF2-4FF8-A80D-5492BF995459' ORDER BY [cl].[lookup_order]",
            ListSelectionMode.Multiple)]
        public string GradeFamilyRoleIDsSetting { get { return Setting("GradeFamilyRoleIDs", "", false); } }

        [ListFromSqlSetting("Email Family Roles", "The family roles that you want email addresses to be entered for.", false, "",
            "SELECT [cl].[lookup_id],[cl].[lookup_value] FROM [core_lookup] AS [cl] LEFT JOIN [core_lookup_type] AS [clt] ON [clt].[lookup_type_id]=[cl].[lookup_type_id] WHERE [clt].[guid]='D3CE5E62-4EF2-4FF8-A80D-5492BF995459' ORDER BY [cl].[lookup_order]",
            ListSelectionMode.Multiple)]
        public string EmailFamilyRoleIDsSetting { get { return Setting("EmailFamilyRoleIDs", "", false); } }

        [ListFromSqlSetting("Person Attributes", "The person attributes to provide access to in the extra information space of a member.", false, "",
            "SELECT [ca].[attribute_id],[cag].[group_name]+' - '+[ca].[attribute_name] FROM [core_attribute] AS [ca] JOIN [core_attribute_group] AS [cag] ON [ca].[attribute_group_id] = [cag].[attribute_group_id] ORDER BY [cag].[group_name],[ca].[attribute_order]",
            ListSelectionMode.Multiple)]
        public string PersonAttributeIDsSetting { get { return Setting("PersonAttributeIDs", "", false); } }

        [LookupSetting("New Member Status", "The member status given to new members added through this module.", true, "0b4532db-3188-40f5-b188-e7e6e4448c85")]
        public int NewMemberStatusSetting { get { return Convert.ToInt32(Setting("NewMemberStatus", "", true)); } }

        [CampusSetting("New Member Campus", "The campus a new member is assigned to when added through this module.", true)]
        public int NewMemberCampusSetting { get { return Convert.ToInt32(Setting("NewMemberCampus", "", true)); } }

        [BooleanSetting("Field Security", "Enable field level security for this module. This setting behaves the same as the PersonDetails module.", true, false)]
        public bool FieldSecuritySetting { get { return Convert.ToBoolean(Setting("FieldSecurity", "false", true)); } }

        [NumericSetting("Friend Relationship ID", "The relationship ID to be used when adding a family friend. The relationship is added from the friend to the head of family.", false)]
        public int FriendRelationshipIDSetting { get { return Convert.ToInt32(Setting("FriendRelationshipID", "-1", false)); } }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Whenever a page is loaded make sure it is in a sane state. Also rebuild
        /// the page and on initial load populate a few lists.
        /// </summary>
        /// <param name="sender">unused</param>
        /// <param name="e">unused</param>
        private void Page_Load(object sender, System.EventArgs e)
        {
            //
            // Enable or disable some basic functionality depending on settings.
            //
            pnlFindFamily.Visible = AllowSearchSetting;
            pnlFamilyFriend.Visible = (FriendRelationshipIDSetting != -1);

            if (!IsPostBack)
            {
                //
                // Add in the drop down list control for selecting the
                // country. Also select the default country.
                //
                Utilities.LoadCountries(ddlAddressCountry);
                Utilities.LoadCountries(ddlFriendAddressCountry);
                if (CountryCodeSetting != null && CountryCodeSetting != "")
                {
                    ddlAddressCountry.SelectedValue = CountryCodeSetting;
                    ddlFriendAddressCountry.SelectedValue = CountryCodeSetting;
                }

                //
                // Set the family roles.
                //
                hfGradeRoleIDs.Value = GradeFamilyRoleIDsSetting.Trim();
                hfEmailRoleIDs.Value = EmailFamilyRoleIDsSetting.Trim();

                //
                // Enable or disable javascript control of family friends.
                //
                hfAllowFriend.Value = (FriendRelationshipIDSetting != -1 ? "1" : "0");
            }

            Build_Page(!IsPostBack);
        }

        /// <summary>
        /// User is searching for a family, hide the family panel and show
        /// the results panel.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        void btnSearch_Click(object sender, EventArgs e)
        {
            pnlFindResults.Visible = true;
            pnlFamily.Visible = false;
            hfFamily.Value = "";
        }

        /// <summary>
        /// A person has been selected, hide the results panel and show
        /// the family panel, then fill in the information about the
        /// family.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        void btnSelectPerson_Click(object sender, EventArgs e)
        {
            PersonAddress pa;
            PersonPhone pp;
            LinkButton button = (LinkButton)sender;
            Person p;

            //
            // Find the person they clicked on and load up the family
            // information based upon that person.
            //
            p = new Person(Convert.ToInt32(button.ID.Substring(9)));
            hfFamily.Value = p.FamilyId.ToString();
            hfExtraCount.Value = "0";
            pnlFindResults.Visible = false;
            pnlFamily.Visible = true;
            Page.ClientScript.RegisterStartupScript(typeof(Page), "hdc_hideSearch", "<script>hideFindFamilyContent();</script>");

            //
            // Rebuild the Family Information box.
            //
            tbFamilyName.Text = p.Family().FamilyName;
            pa = p.Addresses.PrimaryAddress();
            if (pa != null && pa.AddressID != -1)
            {
                tbAddressLine1.Text = pa.Address.StreetLine1;
                tbAddressLine2.Text = pa.Address.StreetLine2;
                tbAddressCity.Text = pa.Address.City;
                tbAddressState.Text = pa.Address.State;
                tbAddressPostalCode.Text = pa.Address.PostalCode;
                if (pa.Address.Country != null && pa.Address.Country != "")
                    ddlAddressCountry.SelectedValue = pa.Address.Country;
                else
                    ddlAddressCountry.SelectedValue = "";
            }
            else
            {
                tbAddressLine1.Text = "";
                tbAddressLine2.Text = "";
                tbAddressCity.Text = "";
                tbAddressState.Text = "";
                tbAddressPostalCode.Text = "";
                if (CountryCodeSetting != null && CountryCodeSetting != "")
                    ddlAddressCountry.SelectedValue = CountryCodeSetting;
                else
                    ddlAddressCountry.SelectedValue = "";
            }
            pp = p.Phones.FindByType(new Guid("f2a0fba2-d5ab-421f-a5ab-0c67db6fd72e"));
            if (pp != null && pp.PersonID != -1)
            {
                tbMainPhone.PhoneNumber = pp.Number;
                cbMainPhoneUnlisted.Checked = pp.Unlisted;
            }
            else
            {
                tbMainPhone.PhoneNumber = "";
                cbMainPhoneUnlisted.Checked = false;
            }

            //
            // Rebuild the normal page.
            //
            phFamilyMembers.Controls.Clear();
            Build_Page(true);
        }

        /// <summary>
        /// Okay, we are just going to add in a new family from scratch. Hide
        /// theh find results and show the family panel, and then blank
        /// everything out to some defaults.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        void btnNewFamily_Click(object sender, EventArgs e)
        {
            pnlFindResults.Visible = false;
            pnlFamily.Visible = true;
            hfFamily.Value = "-1";
            hfExtraCount.Value = NewFamilySizeSetting.ToString();
            Page.ClientScript.RegisterStartupScript(typeof(Page), "hdc_hideSearch", "<script>hideFindFamilyContent();</script>");

            //
            // Clear out the family information boxes.
            //
            tbFamilyName.Text = "";
            tbAddressLine1.Text = "";
            tbAddressLine2.Text = "";
            tbAddressCity.Text = "";
            tbAddressState.Text = "";
            tbAddressPostalCode.Text = "";
            if (CountryCodeSetting != null && CountryCodeSetting != "")
                ddlAddressCountry.SelectedValue = CountryCodeSetting;
            else
                ddlAddressCountry.SelectedValue = "";
            tbMainPhone.PhoneNumber = "";

            //
            // It is safe to set values here since we are starting over.
            //
            phFamilyMembers.Controls.Clear();
            Build_Page(true);
        }

        void btnSaveFamily_Click(object sender, EventArgs e)
        {
            PersonAddress pa;
            FamilyMember fm;
            PersonPhone pp;
            Lookup addressType = new Lookup(new Guid("CDEC7E95-5B91-40F6-BEA3-FDA9B66A7080"));
            Family f;
            Label lb;
            int index;


            //
            // Load up the family or create a new one.
            //
            if (hfFamily.Value != "-1")
                f = new Family(Convert.ToInt32(hfFamily.Value));
            else
            {
                f = new Family();
                f.OrganizationID = CurrentPortal.OrganizationID;
            }

            //
            // Set the family name.
            //
            f.FamilyName = tbFamilyName.Text.Trim();

            //
            // Walk each person and process them.
            //
            for (index = 0; ; index++)
            {
                lb = (Label)phFamilyMembers.FindControl("textMemberID_" + index.ToString());
                if (lb == null)
                    break;

                //
                // Check for a blank record.
                //
                if (string.IsNullOrEmpty(((TextBox)phFamilyMembers.FindControl("tbMemberFirstName_" + index.ToString())).Text) ||
                    string.IsNullOrEmpty(((TextBox)phFamilyMembers.FindControl("tbMemberLastName_" + index.ToString())).Text))
                    continue;

                //
                // Find the person or create a new one.
                //
                if (lb.Text != "-1")
                    fm = new FamilyMember(Convert.ToInt32(hfFamily.Value), Convert.ToInt32(lb.Text));
                else
                {
                    fm = new FamilyMember();
                    f.FamilyMembers.Add(fm);
                }

                //
                // Ensure some of the basics are set correctly.
                //
                if (fm.Campus == null || fm.Campus.CampusId == -1)
                    fm.Campus = new Campus(NewMemberCampusSetting);
                if (fm.MemberStatus == null || fm.MemberStatus.LookupID == -1)
                    fm.MemberStatus = new Lookup(NewMemberStatusSetting);
                if (fm.RecordStatus == Arena.Enums.RecordStatus.Undefined)
                    fm.RecordStatus = Arena.Enums.RecordStatus.Pending;

                //
                // Save the person's name information.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Name, OperationType.Edit))
                {
                    fm.Title = new Lookup(Convert.ToInt32(((DropDownList)phFamilyMembers.FindControl("ddlMemberTitle_" + index.ToString())).SelectedValue));
                    fm.FirstName = ((TextBox)phFamilyMembers.FindControl("tbMemberFirstName_" + index.ToString())).Text;
                    fm.NickName = fm.FirstName;
                    fm.LastName = ((TextBox)phFamilyMembers.FindControl("tbMemberLastName_" + index.ToString())).Text;
                }

                //
                // Save the person's family role.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Family_Information, OperationType.Edit))
                    fm.FamilyRole = new Lookup(Int32.Parse(((DropDownList)phFamilyMembers.FindControl("ddlMemberFamilyRole_" + index.ToString())).SelectedValue));
                else if (fm.PersonID == -1)
                    fm.FamilyRole = new Lookup(new Guid("e410e1a6-8715-4bfb-bf03-1cd18051f815"));

                //
                // Save the person's gender.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Gender, OperationType.Edit))
                    fm.Gender = (Arena.Enums.Gender)Enum.Parse(typeof(Arena.Enums.Gender), ((DropDownList)phFamilyMembers.FindControl("ddlMemberGender_" + index.ToString())).SelectedValue);
                else if (fm.PersonID == -1)
                    fm.Gender = Arena.Enums.Gender.Unknown;

                //
                // Save the person's birth date.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_BirthDate, OperationType.Edit))
                {
                    TextBox tb = (TextBox)phFamilyMembers.FindControl("dtbMemberBirthDate_" + index.ToString());

                    if (string.IsNullOrEmpty(tb.Text) == false)
                        fm.BirthDate = DateTime.Parse(tb.Text);
                    else
                        fm.BirthDate = DateTime.Parse("1/1/1900");
                }

                //
                // Save the person's grade.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Grade, OperationType.Edit) &&
                    GradeFamilyRoleIDsSetting.Length > 0 && GradeFamilyRoleIDsSetting.Split(new char[] { ',' }).Contains(fm.FamilyRole.LookupID.ToString()))
                {
                    fm.GraduationDate = Person.CalculateGraduationYear(Convert.ToInt32(((DropDownList)phFamilyMembers.FindControl("ddlMemberGrade_" + index.ToString())).SelectedValue), CurrentOrganization.GradePromotionDate);
                }

                //
                // Save the person's marital status.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Marital_Status, OperationType.Edit))
                {
                    if (fm.FamilyRole.Value.ToLower() == "adult")
                    {
                        fm.MaritalStatus = new Lookup(Convert.ToInt32(((DropDownList)phFamilyMembers.FindControl("ddlMemberMaritalStatus_" + index.ToString())).SelectedValue));
                    }
                    else
                        fm.MaritalStatus = new Lookup(new Guid("fe219925-f787-4e7e-9ecb-de00caa0e73d"));
                }
                else if (fm.PersonID == -1)
                    fm.MaritalStatus = new Lookup(new Guid("9C000CF2-677B-4725-981E-BD555FDAFB30"));

                //
                // Save the person's anniversary date.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Anniversary_Date, OperationType.Edit) &&
                    fm.FamilyRole.Value.ToLower() == "adult")
                {
                    TextBox tb = (TextBox)phFamilyMembers.FindControl("dtbMemberAnniversaryDate_" + index.ToString());

                    if (string.IsNullOrEmpty(tb.Text) == false)
                        fm.AnniversaryDate = DateTime.Parse(tb.Text);
                    else
                        fm.AnniversaryDate = DateTime.Parse("1/1/1900");
                }

                //
                // Save the person's e-mail address.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Emails, OperationType.Edit) &&
                    EmailFamilyRoleIDsSetting.Length > 0 && EmailFamilyRoleIDsSetting.Split(new char[] { ',' }).Contains(fm.FamilyRole.LookupID.ToString()))
                {
                    //
                    // Okay, tricky here. When we displayed the e-mail address
                    // we did so by tracking from the "firstactive" e-mail address.
                    // So we need to find that same one to update. If we can't find
                    // it (or it doesn't exist) then we create a new one.
                    //
                    if (string.IsNullOrEmpty(fm.Emails.FirstActive))
                    {
                        //
                        // Create a new e-mail address.
                        //
                        PersonEmail pe = new PersonEmail();

                        pe.Active = true;
                        pe.Email = ((TextBox)phFamilyMembers.FindControl("tbMemberEmail_" + index.ToString())).Text.Trim();
                        pe.Notes = "";
                        fm.Emails.Add(pe);
                    }
                    else
                    {
                        int x;

                        //
                        // Try to find the old one.
                        //
                        for (x = 0; x < fm.Emails.Count; x++)
                        {
                            if (fm.Emails[x].Email == fm.Emails.FirstActive)
                            {
                                fm.Emails[x].Email = ((TextBox)phFamilyMembers.FindControl("tbMemberEmail_" + index.ToString())).Text.Trim();
                                break;
                            }
                        }

                        //
                        // See if we need to add a new one.
                        //
                        if (x == fm.Emails.Count)
                        {
                            PersonEmail pe = new PersonEmail();

                            pe.Active = true;
                            pe.Email = ((TextBox)phFamilyMembers.FindControl("tbMemberEmail_" + index.ToString())).Text.Trim();
                            pe.Notes = "";
                            fm.Emails.Add(pe);
                        }
                    }
                }

                //
                // Update the person's address.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Addresses, OperationType.Edit))
                {
                    pa = fm.Addresses.FindByType(addressType.LookupID);
                    if (pa == null || pa.AddressID == -1)
                    {
                        pa = new PersonAddress();
                        pa.AddressType = addressType;
                        fm.Addresses.Add(pa);
                    }
                    if (fm.Addresses.PrimaryAddress() == null)
                        pa.Primary = true;
                    pa.Address.StreetLine1 = tbAddressLine1.Text.Trim();
                    pa.Address.StreetLine2 = tbAddressLine2.Text.Trim();
                    pa.Address.City = tbAddressCity.Text.Trim();
                    pa.Address.State = tbAddressState.Text.Trim();
                    pa.Address.PostalCode = tbAddressPostalCode.Text.Trim();
                    pa.Address.Country = ddlAddressCountry.SelectedValue;
                    pa.Address.Standardize();
                    pa.Address.Geocode(CurrentUser.Identity.Name);
                }

                //
                // Update the person's phone number.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Phones, OperationType.Edit))
                {
                    pp = fm.Phones.FindByType(new Guid("f2a0fba2-d5ab-421f-a5ab-0c67db6fd72e"));
                    if (pp == null || pp.PersonID == -1)
                    {
                        pp = new PersonPhone();
                        pp.PhoneType = new Lookup(new Guid("f2a0fba2-d5ab-421f-a5ab-0c67db6fd72e"));
                        fm.Phones.Add(pp);
                    }
                    pp.Number = tbMainPhone.PhoneNumber.Trim();
                    pp.Unlisted = cbMainPhoneUnlisted.Checked;
                }

                //
                // Save everything.
                //
                f.Save(CurrentUser.Identity.Name);
                fm.Save(CurrentOrganization.OrganizationID, CurrentUser.Identity.Name, true);
                fm.SaveEmails(CurrentPortal.OrganizationID, CurrentUser.Identity.Name);
                fm.SaveAddresses(CurrentPortal.OrganizationID, CurrentUser.Identity.Name);
                fm.SavePhones(CurrentPortal.OrganizationID, CurrentUser.Identity.Name);
                fm.Save(CurrentUser.Identity.Name);

                //
                // Walk through all the attributes and save each one.
                //
                SavePersonAttributes(index, fm, phFamilyMembers);

                //
                // HACK: This is a temporary hack, force the family wizard to reload.
                //
                Session.Remove("fmlyWizard");
                Session.Remove("ignoreDups");
            }

            //
            // Sync the family.
            //
            f.SyncFamily();

            //
            // Show some text and set it to auto-fade after 5 seconds.
            //
            Label message = new Label();
            HtmlTableRow row = new HtmlTableRow();
            HtmlTableCell cell = new HtmlTableCell();
            phFamilySaveMessage.Controls.Add(row);
            row.Cells.Add(cell);
            cell.ColSpan = 2;
            cell.Align = "center";
            cell.Controls.Add(message);
            message.Text = "Changes to the " + f.FamilyName + " family have been saved.<br />&nbsp;";
            message.ID = "lbFamilySaveMessage";
            message.Style.Add("filter", "alpha(opacity=100)");
            Page.ClientScript.RegisterStartupScript(typeof(Page), "hdc_hideFamilySaveMessage", "<script>setTimeout('hdc_fadeObjectOut(\\'" + message.ClientID + "\\')', 8000);</script>");
        }

        void btnSaveFriend_Click(object sender, EventArgs e)
        {
            PersonAddress pa;
            FamilyMember fm;
            Relationship r;
            PersonPhone pp;
            Lookup addressType = new Lookup(new Guid("CDEC7E95-5B91-40F6-BEA3-FDA9B66A7080"));
            Family f;


            //
            // Check for a blank record.
            //
            if (!string.IsNullOrEmpty(((TextBox)phFamilyFriend.FindControl("tbFriendFirstName")).Text) &&
                !string.IsNullOrEmpty(((TextBox)phFamilyFriend.FindControl("tbFriendLastName")).Text))
            {
                //
                // For a family friend, always create a new family
                // to hold the friend.
                //
                f = new Family();
                f.OrganizationID = CurrentPortal.OrganizationID;
                f.FamilyName = ((TextBox)phFamilyFriend.FindControl("tbFriendLastName")).Text.Trim();

                //
                // Create a new person.
                //
                fm = new FamilyMember();
                f.FamilyMembers.Add(fm);

                //
                // Ensure some of the basics are set correctly.
                //
                fm.Campus = new Campus(NewMemberCampusSetting);
                fm.MemberStatus = new Lookup(NewMemberStatusSetting);
                fm.RecordStatus = Arena.Enums.RecordStatus.Pending;

                //
                // Save the person's name information.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Name, OperationType.Edit))
                {
                    fm.Title = new Lookup(Convert.ToInt32(((DropDownList)phFamilyFriend.FindControl("ddlFriendTitle")).SelectedValue));
                    fm.FirstName = ((TextBox)phFamilyFriend.FindControl("tbFriendFirstName")).Text;
                    fm.NickName = fm.FirstName;
                    fm.LastName = ((TextBox)phFamilyFriend.FindControl("tbFriendLastName")).Text;
                }

                //
                // Save the person's family role.
                //
                fm.FamilyRole = new Lookup(new Guid("9ef9e984-923c-4206-a2cf-17adaf2e6659"));

                //
                // Save the person's gender.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Gender, OperationType.Edit))
                    fm.Gender = (Arena.Enums.Gender)Enum.Parse(typeof(Arena.Enums.Gender), ((DropDownList)phFamilyFriend.FindControl("ddlFriendGender")).SelectedValue);
                else if (fm.PersonID == -1)
                    fm.Gender = Arena.Enums.Gender.Unknown;

                //
                // Save the person's birth date.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_BirthDate, OperationType.Edit))
                {
                    TextBox tb = (TextBox)phFamilyFriend.FindControl("dtbFriendBirthDate");

                    if (string.IsNullOrEmpty(tb.Text) == false)
                        fm.BirthDate = DateTime.Parse(tb.Text);
                    else
                        fm.BirthDate = DateTime.Parse("1/1/1900");
                }

                //
                // Save the person's grade.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Grade, OperationType.Edit))
                {
                    fm.GraduationDate = Person.CalculateGraduationYear(Convert.ToInt32(((DropDownList)phFamilyFriend.FindControl("ddlFriendGrade")).SelectedValue), CurrentOrganization.GradePromotionDate);
                }

                //
                // Save the person's marital status.
                //
                fm.MaritalStatus = new Lookup(new Guid("fe219925-f787-4e7e-9ecb-de00caa0e73d"));

                //
                // Save the person's e-mail address.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Emails, OperationType.Edit))
                {
                    if (String.IsNullOrEmpty(((TextBox)phFamilyFriend.FindControl("tbFriendEmail")).Text) == false)
                    {
                        //
                        // Create a new e-mail address.
                        //
                        PersonEmail pe = new PersonEmail();

                        pe.Active = true;
                        pe.Email = ((TextBox)phFamilyFriend.FindControl("tbFriendEmail")).Text.Trim();
                        pe.Notes = "";
                        fm.Emails.Add(pe);
                    }
                }

                //
                // Update the person's address.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Addresses, OperationType.Edit))
                {
                    pa = new PersonAddress();
                    pa.AddressType = addressType;
                    fm.Addresses.Add(pa);
                    pa.Primary = true;
                    pa.Address.StreetLine1 = tbFriendAddressLine1.Text.Trim();
                    pa.Address.StreetLine2 = tbFriendAddressLine2.Text.Trim();
                    pa.Address.City = tbFriendAddressCity.Text.Trim();
                    pa.Address.State = tbFriendAddressState.Text.Trim();
                    pa.Address.PostalCode = tbFriendAddressPostalCode.Text.Trim();
                    pa.Address.Country = ddlFriendAddressCountry.SelectedValue;
                    pa.Address.Standardize();
                    pa.Address.Geocode(CurrentUser.Identity.Name);
                }

                //
                // Update the person's phone number.
                //
                if (PersonFieldOperationAllowed(PersonFields.Profile_Phones, OperationType.Edit))
                {
                    pp = new PersonPhone();
                    pp.PhoneType = new Lookup(new Guid("f2a0fba2-d5ab-421f-a5ab-0c67db6fd72e"));
                    fm.Phones.Add(pp);
                    pp.Number = ((PhoneTextBox)phFamilyFriend.FindControl("ptbFriendPhone")).PhoneNumber.Trim();
                    pp.Unlisted = ((CheckBox)phFamilyFriend.FindControl("cbFriendPhoneUnlisted")).Checked;
                }

                //
                // Add the relationship to the real families head of family.
                //
                r = new Relationship();
                r.Person = fm;
                r.RelatedPerson = new Family(Int32.Parse(hfFamily.Value)).FamilyHead;
                r.RelationshipTypeId = FriendRelationshipIDSetting;
                fm.Relationships.Add(r);

                //
                // Save everything.
                //
                f.Save(CurrentUser.Identity.Name);
                fm.Save(CurrentOrganization.OrganizationID, CurrentUser.Identity.Name, true);
                fm.SaveEmails(CurrentPortal.OrganizationID, CurrentUser.Identity.Name);
                fm.SaveAddresses(CurrentPortal.OrganizationID, CurrentUser.Identity.Name);
                fm.SavePhones(CurrentPortal.OrganizationID, CurrentUser.Identity.Name);
                fm.SaveRelationships(CurrentPortal.OrganizationID, CurrentUser.Identity.Name);
                fm.Save(CurrentUser.Identity.Name);

                //
                // Walk through all the attributes and save each one.
                //
                SavePersonAttributes(9999, fm, phFamilyFriend);

                //
                // Sync the family.
                //
                f.SyncFamily();

                //
                // Clear out the values.
                //
                BuildFamilyFriend(true);
                tbFriendAddressLine1.Text = "";
                tbFriendAddressLine2.Text = "";
                tbFriendAddressCity.Text = "";
                tbFriendAddressState.Text = "";
                tbFriendAddressPostalCode.Text = "";
                if (CountryCodeSetting != null && CountryCodeSetting != "")
                    ddlFriendAddressCountry.SelectedValue = CountryCodeSetting;

                //
                // Show some text and set it to auto-fade after 5 seconds.
                //
                Label message = new Label();
                HtmlTableRow row = new HtmlTableRow();
                HtmlTableCell cell = new HtmlTableCell();
                phFamilyFriendSaveMessage.Controls.Add(row);
                row.Cells.Add(cell);
                cell.ColSpan = 8;
                cell.Align = "center";
                cell.Controls.Add(message);
                message.Text = fm.FirstName + " " + fm.LastName + " has been made a friend of the " + f.FamilyName + " family.<br />&nbsp;";
                message.ID = "lbFamilyFriendSaveMessage";
                message.Style.Add("filter", "alpha(opacity=100)");
                Page.ClientScript.RegisterStartupScript(typeof(Page), "hdc_hideFamilyFriendSaveMessage", "<script>showFamilyFriend(); setTimeout('hdc_fadeObjectOut(\\'" + message.ClientID + "\\')', 8000);</script>");
            }
        }

        /// <summary>
        /// Request to add more people to the family. The number of people to
        /// add is taken from the module setting, AddMoreCountSetting.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        void btnAddMore_Click(object sender, EventArgs e)
        {
            //
            // Set the panel visibility correctly.
            //
            pnlFindResults.Visible = false;
            pnlFamily.Visible = true;

            //
            // Add the extra rows.
            //
            hfExtraCount.Value = (Convert.ToInt32(hfExtraCount.Value) + AddMoreCountSetting).ToString();
            Page.ClientScript.RegisterStartupScript(typeof(Page), "hdc_hideSearch", "<script>hideFindFamilyContent();</script>");

            BuildExtraRows(true);
        }

        void Build_Page(bool SetValues)
        {
            //
            // If there is a search request then handle that.
            //
            if (tbFindName.Text != "" || tbFindPhone.Text != "")
            {
                PersonCollection collection, collection2;
                Person[] people;

                //
                // Determine what kind of search to do.
                //
                if (tbFindName.Text != "")
                {
                    //
                    // Perform a search by name. Search for any match in the
                    // first name and last name fields.
                    //
                    if (tbFindName.Text.Contains(" ") == false)
                    {
                        int i;

                        //
                        // Search for first name.
                        //
                        collection = new PersonCollection();
                        collection.LoadByName(tbFindName.Text, "");

                        //
                        // Search for last name.
                        //
                        collection2 = new PersonCollection();
                        collection2.LoadByName("", tbFindName.Text);

                        //
                        // Merge the two searches.
                        //
                        for (i = 0; i < collection2.Count; i++)
                        {
                            if (collection.FindByID(collection2[i].PersonID) == null)
                                collection.Add(collection2[i]);
                        }

                        people = collection.ToArray();
                    }
                    else
                    {
                        string[] names = tbFindName.Text.Split(new char[1] { ' ' }, 2);

                        //
                        // Search for a specific first and last name.
                        //
                        collection = new PersonCollection();
                        collection.LoadByName(names[0], names[1]);

                        people = collection.ToArray();
                    }
                }
                else if (tbFindPhone.Text != "")
                {
                    System.Data.SqlClient.SqlDataReader reader;
                    string queryString;
                    ArrayList list = new ArrayList();

                    //
                    // Query the database for all person_id's who have a stripped
                    // phone number containing the given phone number.
                    //
                    queryString = "SELECT DISTINCT person_id FROM core_person_phone WHERE phone_number_stripped LIKE '%" + tbFindPhone.Text + "%';";
                    reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(queryString);

                    //
                    // Walk the reader and add all the results.
                    //
                    while (reader.Read())
                    {
                        list.Add(new Person(Convert.ToInt32(reader[0])));
                    }

                    reader.Close();

                    people = (Person[])list.ToArray(typeof(Person));
                }
                else
                {
                    //
                    // Should never get here, but just in case.
                    //
                    people = new Person[0];
                }

                //
                // Sort the people.
                //
                Array.Sort(people, new PersonComparer());

                //
                // Clear the search results and add each person found.
                //
                phSearchResults.Controls.Clear();
                foreach (Person p in people)
                {
                    HtmlTableRow row = new HtmlTableRow();
                    HtmlTableCell cell;
                    LinkButton link;
                    PersonPhone phone;
                    string grade;

                    //
                    // Alternate row background colors.
                    //
                    phSearchResults.Controls.Add(row);
                    row.Attributes.Add("class", ((phSearchResults.Controls.Count % 2) != 0 ? "listItem" : "listAltItem"));

                    //
                    // Add in the link button to select this person.
                    //
                    cell = new HtmlTableCell();
                    row.Cells.Add(cell);
                    link = new LinkButton();
                    cell.Controls.Add(link);
                    link.ID = "btnPerson" + p.PersonID;
                    link.Text = p.LastName + ", " + p.FirstName;
                    link.Click += new EventHandler(btnSelectPerson_Click);

                    //
                    // Add in the gender.
                    //
                    if (PersonFieldOperationAllowed(PersonFields.Profile_Gender, OperationType.View))
                        TableCellString(row, null, p.Gender.ToString());
                    else
                        TableCellString(row, null, "");

                    //
                    // Add in the age.
                    //
                    if (PersonFieldOperationAllowed(PersonFields.Profile_Age, OperationType.View))
                        TableCellString(row, null, (p.Age != -1 ? p.Age.ToString() : ""));
                    else
                        TableCellString(row, null, "");

                    //
                    // Add in the grade.
                    //
                    if (PersonFieldOperationAllowed(PersonFields.Profile_Grade, OperationType.View))
                    {
                        grade = Person.GetGradeName(Person.CalculateGradeLevel(p.GraduationDate, CurrentOrganization.GradePromotionDate));
                        TableCellString(row, null, grade);
                    }
                    else
                        TableCellString(row, null, "");

                    //
                    // Add in the Main/Home phone number.
                    //
                    if (PersonFieldOperationAllowed(PersonFields.Profile_Phones, OperationType.View))
                    {
                        phone = p.Phones.FindByType(new Guid("F2A0FBA2-D5AB-421F-A5AB-0C67DB6FD72E"));
                        if (phone != null && phone.Number != "")
                        {
                            if (phone.Unlisted)
                                TableCellString(row, null, phone.Number + " (unlisted)");
                            else
                                TableCellString(row, null, phone.Number);
                        }
                        else
                            TableCellString(row, null, "");
                    }
                    else
                        TableCellString(row, null, "");

                    //
                    // Add in the e-mail address.
                    //
                    if (PersonFieldOperationAllowed(PersonFields.Profile_Emails, OperationType.View))
                        TableCellString(row, null, p.Emails.FirstActive);
                    else
                        TableCellString(row, null, "");
                }
            }

            //
            // If they have selected a family then build up the members.
            //
            if (hfFamily.Value != "" && hfFamily.Value != "-1")
            {
                Family f = new Family(Convert.ToInt32(hfFamily.Value));
                int i;

                //
                // Build each family member row.
                //
                phFamilyMembers.Controls.Clear();
                for (i = 0; i < f.FamilyMembers.Count; i++)
                {
                    BuildFamilyMemberRow(f.FamilyMembers[i], i, SetValues);
                }
            }

            //
            // Build up the extra rows and the family friend.
            //
            BuildExtraRows(SetValues);
            Page.ClientScript.RegisterStartupScript(typeof(Page), "hdc_toggleFamilyAttributes", "<script>hdc_toggleFamilyAttributes();</script>");
            BuildFamilyFriend(SetValues);

            //
            // Make sure all the panels are in the correct state.
            //
            pnlFindResults.Visible = (((tbFindName.Text != "" || tbFindPhone.Text != "") && hfFamily.Value == "") ? true : false);
            pnlFamily.Visible = (hfFamily.Value == "" ? false : true);
            if (hfFamily.Value != "")
                Page.ClientScript.RegisterStartupScript(typeof(Page), "hdc_hideSearch", "<script>hideFindFamilyContent();</script>");
        }

        /// <summary>
        /// Build the extra family member rows we need.
        /// </summary>
        /// <param name="SetValues">Wether or not to set blank values.</param>
        private void BuildExtraRows(bool SetValues)
        {
            Family f;
            int i, totalRows;


            //
            // Find the number of rows to add.
            //
            if (hfFamily.Value != "")
            {
                f = new Family(Convert.ToInt32(hfFamily.Value));
                totalRows = (f.FamilyMembers.Count + Convert.ToInt32(hfExtraCount.Value));
            }
            else
                totalRows = Convert.ToInt32(hfExtraCount.Value);

            //
            // Add in extra rows. We do this by hand to keep the data that the
            // user has already entered. If this is updated it also needs to be
            // updated in the Build_Page() method.
            //
            for (i = (phFamilyMembers.Controls.Count / 2); i < totalRows; i++)
            {
                BuildFamilyMemberRow(null, i, SetValues);
            }
        }

        private void BuildFamilyMemberRow(FamilyMember fm, int index, bool SetValues)
        {
            HtmlTableRow row;
            HtmlTableCell cell;
            DropDownList list;
            Lookup luFamilyRole;
            string value;
            bool allowed;


            //
            // Determine if we are adding a new adult row or child row.
            //
            if (fm != null)
                luFamilyRole = fm.FamilyRole;
            else if (index < 2)
                luFamilyRole = new Lookup(new Guid("E410E1A6-8715-4BFB-BF03-1CD18051F815"));
            else
                luFamilyRole = new Lookup(new Guid("9EF9E984-923C-4206-A2CF-17ADAF2E6659"));

            //
            // Prepare the row.
            //
            row = new HtmlTableRow();
            row.Height = "25";
            row.VAlign = "top";
            phFamilyMembers.Controls.Add(row);

            //
            // Add in a hidden cell for the person ID.
            //
            TableCellString(row, "textMemberID_" + index.ToString(), (fm != null ? fm.PersonID.ToString() : "-1"));
            ((Label)row.Cells[row.Cells.Count - 1].Controls[0]).Style.Add("display", "none");

            //
            // Build up the name information for the person.
            //
            allowed = PersonFieldOperationAllowed(PersonFields.Profile_Name, OperationType.View);
            value = (SetValues ? (allowed && fm != null ? fm.Title.LookupID.ToString() : "") : null);
            TableCellLookupDropDownList(row, "ddlMemberTitle_" + index.ToString(), new Guid("3394ca53-5791-42c8-b996-1d77c740cf03"), value, false);
            value = (SetValues ? (allowed && fm != null ? fm.FirstName : "") : null);
            TableCellTextBox(row, "tbMemberFirstName_" + index.ToString(), value, 65);
            value = (SetValues ? (allowed ? (fm != null ? fm.LastName : tbFamilyName.Text) : "") : null);
            TableCellTextBox(row, "tbMemberLastName_" + index.ToString(), value, 90);
            if (PersonFieldOperationAllowed(PersonFields.Profile_Name, OperationType.Edit) == false)
            {
                ((DropDownList)row.Cells[row.Cells.Count - 3].Controls[0]).Enabled = false;
                ((TextBox)row.Cells[row.Cells.Count - 2].Controls[0]).Enabled = false;
                ((TextBox)row.Cells[row.Cells.Count - 1].Controls[0]).Enabled = false;
            }

            //
            // Add in the family role information.
            //
            allowed = PersonFieldOperationAllowed(PersonFields.Profile_Family_Information, OperationType.View);
            value = (SetValues ? (allowed ? luFamilyRole.LookupID.ToString() : "") : null);
            TableCellLookupDropDownList(row, "ddlMemberFamilyRole_" + index.ToString(), new Guid("d3ce5e62-4ef2-4ff8-a80d-5492bf995459"), value, true);
            list = (DropDownList)row.Cells[row.Cells.Count - 1].Controls[0];
            list.Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Gender, OperationType.Edit);
            list.Attributes["onchange"] = "hdc_toggleFamilyAttributes();";

            //
            // Add in the gender.
            //
            allowed = PersonFieldOperationAllowed(PersonFields.Profile_Gender, OperationType.View);
            value = (SetValues ? (allowed ? (fm != null ? ((int)fm.Gender).ToString() : Enum.Format(typeof(Arena.Enums.Gender), Arena.Enums.Gender.Unknown, "d")) : "") : null);
            TableCellEnumDropDownList(row, "ddlMemberGender_" + index.ToString(), typeof(Arena.Enums.Gender), value);
            ((DropDownList)row.Cells[row.Cells.Count - 1].Controls[0]).Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Gender, OperationType.Edit);

            //
            // Add in the birth date.
            //
            allowed = PersonFieldOperationAllowed(PersonFields.Profile_BirthDate, OperationType.View);
            value = (SetValues ? (allowed && fm != null ? (fm.BirthDate.Year > 1901 ? fm.BirthDate.ToString("MM/dd/yyyy") : "") : "") : null);
            TableCellDateTextBox(row, "dtbMemberBirthDate_" + index.ToString(), value);
            ((DateTextBox)row.Cells[row.Cells.Count - 1].Controls[0]).Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Gender, OperationType.Edit);

            //
            // Add in the grade, a bit custom.
            //
            allowed = PersonFieldOperationAllowed(PersonFields.Profile_Grade, OperationType.View);
            cell = new HtmlTableCell();
            row.Cells.Add(cell);
            list = new DropDownList();
            cell.Controls.Add(list);
            PopulateGrades(list);
            list.CssClass = "smallText";
            list.ID = "ddlMemberGrade_" + index.ToString();
            if (SetValues)
                list.SelectedValue = (allowed && fm != null ? Person.CalculateGradeLevel(fm.GraduationDate, CurrentOrganization.GradePromotionDate).ToString() : "");
            list.Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Grade, OperationType.Edit);

            //
            // Add in the marital status.
            //
            allowed = PersonFieldOperationAllowed(PersonFields.Profile_Marital_Status, OperationType.View);
            value = (SetValues ? (allowed && fm != null ? fm.MaritalStatus.LookupID.ToString() : "") : null);
            TableCellLookupDropDownList(row, "ddlMemberMaritalStatus_" + index.ToString(), new Guid("0aad26c7-ad9d-4fe8-96b1-c9bcd033bb5b"), value, true);
            ((DropDownList)row.Cells[row.Cells.Count - 1].Controls[0]).Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Marital_Status, OperationType.Edit);

            //
            // Add in the annivarsary date.
            //
            allowed = PersonFieldOperationAllowed(PersonFields.Profile_Anniversary_Date, OperationType.View);
            value = (SetValues ? (allowed && fm != null ? (fm.AnniversaryDate.Year > 1901 ? fm.AnniversaryDate.ToString("MM/dd/yyyy") : "") : "") : null);
            TableCellDateTextBox(row, "dtbMemberAnniversaryDate_" + index.ToString(), value);
            ((DateTextBox)row.Cells[row.Cells.Count - 1].Controls[0]).Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Anniversary_Date, OperationType.Edit);

            //
            // Add in the e-mail address.
            //
            allowed = PersonFieldOperationAllowed(PersonFields.Profile_Emails, OperationType.View);
            value = (SetValues ? (allowed && fm != null ? (fm.Emails.FirstActive != null ? fm.Emails.FirstActive : "") : "") : null);
            TableCellTextBox(row, "tbMemberEmail_" + index.ToString(), value, 150);
            ((TextBox)row.Cells[row.Cells.Count - 1].Controls[0]).Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Emails, OperationType.Edit);

            //
            // Add in an image with a javascript click handler that will toggle
            // the visibility of the extra fields. If you change the image file
            // here you must also update the javascript code.
            //
            cell = new HtmlTableCell();
            row.Cells.Add(cell);
            Image img = new Image();
            cell.Controls.Add(img);
            img.ImageUrl = BaseUrl() + "Images/information2.gif";
            img.ID = "imgMemberShowExtra_" + index.ToString();
            img.Attributes.Add("onclick", "hdc_toggleExtraFields(this);");

            //
            // Add another row for the extra information that will appear
            // and disappear depending on first-row selections.
            //
            int colspan = (row.Cells.Count - 2);
            row = new HtmlTableRow();
            phFamilyMembers.Controls.Add(row);
            TableCellString(row, null, "");
            row.Cells[row.Cells.Count - 1].ColSpan = 2;
            row.ID = "trMemberExtraFields_" + index.ToString();
            cell = TableCellString(row, null, "");
            cell.ColSpan = colspan;

            //
            // Add in all the person attributes.
            //
            if (PersonAttributeIDsSetting != "")
                BuildPersonAttributes(cell, index, fm, SetValues);

            //
            // Default the row not displayed.
            //
            row.Style.Add("display", "none");
        }

        private void BuildFamilyFriend(bool SetValues)
        {
            HtmlTableRow row;
            HtmlTableCell cell;
            DropDownList list;
            string value;


            //
            // Prepare the row.
            //
            row = new HtmlTableRow();
            row.Height = "25";
            row.VAlign = "top";
            phFamilyFriend.Controls.Clear();
            phFamilyFriend.Controls.Add(row);

            //
            // Build up the name information for the person.
            //
            TableCellLookupDropDownList(row, "ddlFriendTitle", new Guid("3394ca53-5791-42c8-b996-1d77c740cf03"), (SetValues ? "" : null), false);
            TableCellTextBox(row, "tbFriendFirstName", (SetValues ? "" : null), 65);
            TableCellTextBox(row, "tbFriendLastName", (SetValues ? "" : null), 90);
            if (PersonFieldOperationAllowed(PersonFields.Profile_Name, OperationType.Edit) == false)
            {
                ((DropDownList)row.Cells[row.Cells.Count - 3].Controls[0]).Enabled = false;
                ((TextBox)row.Cells[row.Cells.Count - 2].Controls[0]).Enabled = false;
                ((TextBox)row.Cells[row.Cells.Count - 1].Controls[0]).Enabled = false;
            }

            //
            // Add in the gender.
            //
            value = (SetValues ? Enum.Format(typeof(Arena.Enums.Gender), Arena.Enums.Gender.Unknown, "d") : null);
            TableCellEnumDropDownList(row, "ddlFriendGender", typeof(Arena.Enums.Gender), value);
            ((DropDownList)row.Cells[row.Cells.Count - 1].Controls[0]).Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Gender, OperationType.Edit);

            //
            // Add in the birth date.
            //
            TableCellDateTextBox(row, "dtbFriendBirthDate", (SetValues ? "" : null));
            ((DateTextBox)row.Cells[row.Cells.Count - 1].Controls[0]).Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Gender, OperationType.Edit);

            //
            // Add in the grade, a bit custom.
            //
            cell = new HtmlTableCell();
            row.Cells.Add(cell);
            list = new DropDownList();
            cell.Controls.Add(list);
            PopulateGrades(list);
            list.CssClass = "smallText";
            list.ID = "ddlFriendGrade";
            if (SetValues)
                list.SelectedValue = "";
            list.Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Grade, OperationType.Edit);

            //
            // Add in the e-mail address.
            //
            TableCellTextBox(row, "tbFriendEmail", (SetValues ? "" : null), 150);
            ((TextBox)row.Cells[row.Cells.Count - 1].Controls[0]).Enabled = PersonFieldOperationAllowed(PersonFields.Profile_Emails, OperationType.Edit);
            
            //
            // Add in the phone number. Also a custom jobby.
            //
            cell = new HtmlTableCell();
            row.Cells.Add(cell);
            PhoneTextBox ptb = new PhoneTextBox();
            cell.Controls.Add(ptb);
            ptb.ID = "ptbFriendPhone";
            ptb.CssClass = "smallText";
            ptb.ShowExtension = false;
            ptb.Width = Unit.Pixel(100);
            ptb.Required = false;
            CheckBox cb = new CheckBox();
            cell.Controls.Add(cb);
            cb.ID = "cbFriendPhoneUnlisted";
            cb.CssClass = "smallText";
            cb.Text = "(unlisted)";
            cb.Checked = false;

            //
            // Add in an image with a javascript click handler that will toggle
            // the visibility of the extra fields. If you change the image file
            // here you must also update the javascript code.
            //
            cell = new HtmlTableCell();
            row.Cells.Add(cell);
            Image img = new Image();
            cell.Controls.Add(img);
            img.ImageUrl = BaseUrl() + "Images/information2.gif";
            img.ID = "imgFriendShowExtra";
            img.Attributes.Add("onclick", "hdc_toggleExtraFields(this);");

            //
            // Add another row for the extra information that will appear
            // and disappear depending on first-row selections.
            //
            int colspan = (row.Cells.Count - 2);
            row = new HtmlTableRow();
            phFamilyFriend.Controls.Add(row);
            TableCellString(row, null, "");
            row.ID = "trFriendExtraFields";
            cell = TableCellString(row, null, "");
            cell.ColSpan = colspan;

            //
            // Add in all the person attributes.
            //
            if (PersonAttributeIDsSetting != "")
                BuildPersonAttributes(cell, 9999, null, SetValues);

            //
            // Default the row not displayed.
            //
            row.Style.Add("display", "none");
        }

        /// <summary>
        /// Generate a HtmlTableCell that contains a single control, a Label
        /// that contains the string value.
        /// </summary>
        /// <param name="value">The string to display inside the cell.</param>
        /// <returns>A new TableCell containing the given string.</returns>
        private HtmlTableCell TableCellString(HtmlTableRow row, string controlId, string value)
        {
            Label label;
            HtmlTableCell cell = new HtmlTableCell();


            row.Cells.Add(cell);
            label = new Label();
            cell.Controls.Add(label);

            label.CssClass = "smallText";
            label.Text = value;
            if (controlId != null)
                label.ID = controlId;

            return cell;
        }

        private HtmlTableCell TableCellLookupDropDownList(HtmlTableRow row, string controlId, Guid guid, string value, bool required)
        {
            return TableCellLookupDropDownList(row, controlId, new LookupCollection(guid), value, required);
        }

        private HtmlTableCell TableCellLookupDropDownList(HtmlTableRow row, string controlId, int typeId, string value, bool required)
        {
            return TableCellLookupDropDownList(row, controlId, new LookupCollection(typeId), value, required);
        }

        private HtmlTableCell TableCellLookupDropDownList(HtmlTableRow row, string controlId, LookupCollection lookups, string value, bool required)
        {
            HtmlTableCell cell = new HtmlTableCell();
            DropDownList list = new DropDownList();

            row.Cells.Add(cell);
            cell.Controls.Add(list);

            list.Items.Clear();
            if (required == false)
                list.Items.Add(new ListItem("", "-1"));
            lookups.LoadDropDownList(list);
            list.CssClass = "smallText";
            if (controlId != null)
                list.ID = controlId;
            if (value != null)
                list.SelectedValue = value;

            return cell;
        }

        private HtmlTableCell TableCellEnumDropDownList(HtmlTableRow row, string controlId, Type eType, string value)
        {
            HtmlTableCell cell = new HtmlTableCell();
            DropDownList list = new DropDownList();
            string[] names = Enum.GetNames(eType);
            string eValue;
            int i;

            row.Cells.Add(cell);
            cell.Controls.Add(list);

            list.Items.Clear();
            for (i = 0; i < names.Length; i++)
            {
                eValue = Enum.Format(eType, Enum.Parse(eType, names[i]), "d");
                if (eValue != "-1")
                    list.Items.Add(new ListItem(names[i], eValue));
            }

            list.CssClass = "smallText";
            if (controlId != null)
                list.ID = controlId;
            if (value != null)
                list.SelectedValue = value;

            return cell;
        }

        private HtmlTableCell TableCellTextBox(HtmlTableRow row, string controlId, string value, int width)
        {
            HtmlTableCell cell = new HtmlTableCell();
            TextBox tb = new TextBox();

            row.Cells.Add(cell);
            cell.Controls.Add(tb);

            tb.CssClass = "smallText";
            if (controlId != null)
                tb.ID = controlId;
            if (value != null)
                tb.Text = value;
            if (width != 0)
                tb.Width = Unit.Pixel(width);

            return cell;
        }

        private HtmlTableCell TableCellDateTextBox(HtmlTableRow row, string controlId, string value)
        {
            HtmlTableCell cell = new HtmlTableCell();
            DateTextBox dtb = new DateTextBox();


            row.Cells.Add(cell);
            cell.Controls.Add(dtb);

            dtb.CssClass = "smallText";
            dtb.Width = Unit.Pixel(65);
            dtb.MaxLength = 10;
            if (controlId != null)
                dtb.ID = controlId;
            if (value != null)
                dtb.Text = value;

            return cell;
        }

        private void PopulateGrades(DropDownList list)
        {
            list.Items.Clear();
            list.Items.Add(new ListItem("", "-1"));
            list.Items.Add(new ListItem("Kinder", "0"));
            list.Items.Add(new ListItem("1st", "1"));
            list.Items.Add(new ListItem("2nd", "2"));
            list.Items.Add(new ListItem("3rd", "3"));
            list.Items.Add(new ListItem("4th", "4"));
            list.Items.Add(new ListItem("5th", "5"));
            list.Items.Add(new ListItem("6th", "6"));
            list.Items.Add(new ListItem("7th", "7"));
            list.Items.Add(new ListItem("8th", "8"));
            list.Items.Add(new ListItem("9th", "9"));
            list.Items.Add(new ListItem("10th", "10"));
            list.Items.Add(new ListItem("11th", "11"));
            list.Items.Add(new ListItem("12th", "12"));
        }

        /// <summary>
        /// Retrieve the base url (the portion of the URL without the last path
        /// component, that is the filename and query string) of the current
        /// web request.
        /// </summary>
        /// <returns>Base url as a string.</returns>
        private string BaseUrl()
        {
            StringBuilder url = new StringBuilder();
            string[] segments;
            int i;


            url.Append(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority));
            segments = HttpContext.Current.Request.Url.Segments;
            for (i = 0; i < segments.Length - 1; i++)
            {
                url.Append(segments[i]);
            }

            return url.ToString();
        }

        /// <summary>
        /// Determines if the current user has access to perform the
        /// indicated operation on the person field in question.
        /// </summary>
        /// <param name="field">The ID number of the PersonField that the user wants access to.</param>
        /// <param name="operation">The type of access the user needs to proceed.</param>
        /// <returns>true/false indicating if the operation is allowed.</returns>
        private bool PersonFieldOperationAllowed(int field, OperationType operation)
        {
            PermissionCollection permissions;

            //
            // If field security is not enabled then always allow.
            //
            if (FieldSecuritySetting == false)
                return true;

            //
            // Load the permissions.
            //
            permissions = new PermissionCollection(ObjectType.PersonField, field);

            return PermissionsOperationAllowed(permissions, operation);
        }

        /// <summary>
        /// Checks the PermissionCollection class to determine if the
        /// indicated operation is allowed for the current user.
        /// </summary>
        /// <param name="permissions">The collection of permissions to check. These should be object permissions.</param>
        /// <param name="operation">The type of access the user needs to proceed.</param>
        /// <returns>true/false indicating if the operation is allowed.</returns>
        private bool PermissionsOperationAllowed(PermissionCollection permissions, OperationType operation)
        {
            if (FieldSecuritySetting)
                return permissions.Allowed(operation, CurrentUser);
            else
                return true;
        }

        private HtmlTable BuildPersonAttributes(Control parent, int index, Person p, bool SetValues)
        {
            HtmlTable table = new HtmlTable();
            string[] attributeIDs = PersonAttributeIDsSetting.Split(new char[] { ',' });
            int i;

            //
            // Loop through and add each person attribute.
            //
            parent.Controls.Add(table);
            for (i = 0; i < attributeIDs.Length; i++)
            {
                BuildPersonAttribute(table, index, p, Convert.ToInt32(attributeIDs[i]), SetValues);
            }

            return table;
        }

        private HtmlTableRow BuildPersonAttribute(HtmlTable parent, int index, Person p, int attributeID, bool SetValue)
        {
            PersonAttributeEditor attribute;
            HtmlTableRow row = new HtmlTableRow();


            if (parent != null)
                parent.Rows.Add(row);

            //
            // Create the attribute to work with.
            //
            if (p != null)
                attribute = new PersonAttributeEditor(p.PersonID, attributeID);
            else
                attribute = new PersonAttributeEditor(attributeID);

            //
            // Create the attribute title.
            //
            TableCellString(row, null, attribute.AttributeName);

            //
            // Create the data entry portion.
            //
            HtmlTableCell cell = new HtmlTableCell();
            row.Cells.Add(cell);
            attribute.WebControl(cell, "MemberAttribute_" + index.ToString(), SetValue);

            return row;
        }

        private void SavePersonAttributes(int index, Person p, Control parent)
        {
            PersonAttributeEditor attribute;
            string[] attributeIDs = PersonAttributeIDsSetting.Split(new char[] { ',' });
            int i;

            //
            // Loop through and save each person attribute.
            //
            for (i = 0; i < attributeIDs.Length; i++)
            {
                attribute = new PersonAttributeEditor(p.PersonID, Convert.ToInt32(attributeIDs[i]));

                attribute.StoreWebControlValue("MemberAttribute_" + index.ToString(), parent);
                attribute.Save(CurrentPortal.OrganizationID, CurrentUser.Identity.Name);
            }
        }

        #endregion

        #region Web Form Code

        override protected void OnInit(EventArgs e)
        {
            btnSearch.Click += new EventHandler(btnSearch_Click);
            btnNewFamily.Click += new EventHandler(btnNewFamily_Click);
            btnAddMore.Click += new EventHandler(btnAddMore_Click);
            btnSaveFamily.Click += new EventHandler(btnSaveFamily_Click);
            btnSaveFriend.Click += new EventHandler(btnSaveFriend_Click);

            base.OnInit(e);
        }

        #endregion
    }

    public class PersonAttributeEditor : PersonAttribute
    {
        public PersonAttributeEditor(int attributeID) : base(attributeID)
        {
        }

        public PersonAttributeEditor(System.Data.SqlClient.SqlDataReader rdr) : base(rdr)
        {
        }

        public PersonAttributeEditor(int personID, int attributeID) : base(personID, attributeID)
        {
        }

        public Control WebControl(Control parent, string baseControlId, bool SetValue)
        {
            switch (this.AttributeType)
            {
                //
                // Deal with string data types.
                //
                case Arena.Enums.DataType.String:
                    {
                        TextBox tb = new TextBox();

                        parent.Controls.Add(tb);
                        tb.CssClass = "smallText";
                        tb.ID = baseControlId + "_" + this.AttributeId.ToString();
                        if (SetValue)
                            tb.Text = (this.StringValue != null ? this.StringValue : "");
                        tb.Width = Unit.Pixel(120);
                        tb.Enabled = (!this.Readonly && OperationAllowed(OperationType.Edit));

                        return tb;
                    }

                //
                // Deal with lookup data types.
                //
                case Arena.Enums.DataType.Lookup:
                    {
                        LookupCollection lookups;
                        DropDownList list = new DropDownList();

                        parent.Controls.Add(list);
                        list.Items.Clear();
                        list.CssClass = "smallText";
                        list.ID = baseControlId + "_" + this.AttributeId.ToString();
                        list.Enabled = !this.Readonly;
                        if (this.Required == false)
                            list.Items.Add(new ListItem("", "-1"));
                        if (this.TypeQualifier != null && this.TypeQualifier != "")
                        {
                            lookups = new LookupCollection(Convert.ToInt32(this.TypeQualifier));
                            lookups.LoadDropDownList(list);
                        }
                        if (SetValue)
                            list.SelectedValue = this.IntValue.ToString();

                        return list;
                    }

                //
                // Deal with datetime data types.
                //
                case Arena.Enums.DataType.DateTime:
                    {
                        DateTextBox dtb = new DateTextBox();

                        parent.Controls.Add(dtb);
                        dtb.CssClass = "smallText";
                        dtb.Width = Unit.Pixel(65);
                        dtb.Enabled = !this.Readonly;
                        dtb.MaxLength = 10;
                        dtb.InvalidValueMessage = string.Format("{0} must be a valid date!", this.GroupName + " - " + this.AttributeName);
                        dtb.ID = baseControlId + "_" + this.AttributeId.ToString();
                        if (SetValue)
                            dtb.Text = (this.DateValue.Year > 1901 ? this.DateValue.ToString("MM/dd/yyyy") : "");

                        return dtb;
                    }

                //
                // Deal with YesNo data types.
                //
                case Arena.Enums.DataType.YesNo:
                    {
                        CheckBox cb = new CheckBox();

                        parent.Controls.Add(cb);
                        cb.ID = baseControlId + "_" + this.AttributeId.ToString();
                        cb.CssClass = "smallText";
                        cb.Checked = (this.IntValue == 1);
                        cb.Enabled = !this.Readonly;

                        return cb;
                    }

                default:
                    {
                        Label label = new Label();

                        parent.Controls.Add(label);
                        label.CssClass = "smallText";
                        label.Text = "Unable to process datatype " + this.AttributeType.ToString();

                        return label;
                    }
            }
        }

        public void StoreWebControlValue(string baseControlId, Control parent)
        {
            //
            // See if we even have access to do this.
            //
            if (OperationAllowed(OperationType.Edit) == false)
                return;

            switch (this.AttributeType)
            {
                //
                // Deal with string data types.
                //
                case Arena.Enums.DataType.String:
                    {
                        TextBox tb = (TextBox)parent.FindControl(baseControlId + "_" + this.AttributeId.ToString());

                        this.StringValue = tb.Text.Trim();

                        break;
                    }

                //
                // Deal with lookup data types.
                //
                case Arena.Enums.DataType.Lookup:
                    {
                        DropDownList list = (DropDownList)parent.FindControl(baseControlId + "_" + this.AttributeId.ToString());

                        this.IntValue = Convert.ToInt32(list.SelectedValue);

                        break;
                    }

                //
                // Deal with datetime data types.
                //
                case Arena.Enums.DataType.DateTime:
                    {
                        TextBox tb = (TextBox)parent.FindControl(baseControlId + "_" + this.AttributeId.ToString());

                        if (string.IsNullOrEmpty(tb.Text) == false)
                            this.DateValue = DateTime.Parse(tb.Text);
                        else
                            this.DateValue = DateTime.Parse("01/01/1900");

                        break;
                    }

                //
                // Deal with YesNo data types.
                //
                case Arena.Enums.DataType.YesNo:
                    {
                        CheckBox cb = (CheckBox)parent.FindControl(baseControlId + "_" + this.AttributeId.ToString());

                        this.IntValue = Convert.ToInt32(cb.Checked);

                        break;
                    }

                //
                // Unknown type, ignore.
                //
                default:
                    break;
            }
        }

        /// <summary>
        /// Determines if the current user has access to perform the
        /// indicated operation on this attribute.
        /// </summary>
        /// <param name="operation">The type of access the user needs to proceed.</param>
        /// <returns>true/false indicating if the operation is allowed.</returns>
        private bool OperationAllowed(OperationType operation)
        {
            PermissionCollection permissions;

            //
            // Load the permissions.
            //
            permissions = new PermissionCollection(ObjectType.Attribute, this.AttributeId);

            return permissions.Allowed(operation, (System.Security.Principal.GenericPrincipal)HttpContext.Current.User);
        }
    }

    public class PersonComparer : IComparer
    {
        int IComparer.Compare(Object x, Object y)
        {
            CaseInsensitiveComparer comparer = new CaseInsensitiveComparer();
            int result;


            result = comparer.Compare(((Person)x).LastName, ((Person)y).LastName);
            if (result == 0)
                result = comparer.Compare(((Person)x).FirstName, ((Person)y).FirstName);

            return result;
        }
    }
}