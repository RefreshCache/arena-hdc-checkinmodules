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

        [LookupSetting("New Member Status", "The member status given to new members added through this module.", true, "0b4532db-3188-40f5-b188-e7e6e4448c85")]
        public Lookup NewMemberStatusSetting { get { return new Lookup(Convert.ToInt32(Setting("NewMemberStatus", "", true))); } }

        #endregion

        #region Event Handlers

        private void Page_Load(object sender, System.EventArgs e)
        {
            //
            // If searching for families is allowed, then show the search panel.
            //
            if (AllowSearchSetting == true)
                pnlFindFamily.Visible = true;

            if (!IsPostBack)
            {
                //
                // Add in the drop down list control for selecting the
                // country. Also select the default country.
                //
                Utilities.LoadCountries(ddlAddressCountry);
                if (CountryCodeSetting != null && CountryCodeSetting != "")
                    ddlAddressCountry.SelectedValue = CountryCodeSetting;

                //
                // Set the family roles.
                //
                hfGradeRoleIDs.Value = GradeFamilyRoleIDsSetting.Trim();
                hfEmailRoleIDs.Value = EmailFamilyRoleIDsSetting.Trim();
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
            hfFamily.Value = "";
            hfExtraCount.Value = NewFamilySizeSetting.ToString();
            Page.ClientScript.RegisterStartupScript(typeof(Page), "hdc_hideSearch", "<script>hideFindFamilyContent();</script>");

            //
            // It is safe to set values here since we are starting over.
            //
            phFamilyMembers.Controls.Clear();
            Build_Page(true);

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
                    // TODO: If there is a space in the name, make the primary
                    // search one based upon splitting that into first and
                    // last names.
                    //
                    if (tbFindName.Text.Contains(" ") == false)
                    {
                        int i;

                        collection = new PersonCollection();
                        collection.LoadByName(tbFindName.Text, "");

                        collection2 = new PersonCollection();
                        collection2.LoadByName("", tbFindName.Text);

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
                    row.Attributes.Add("class", ((phSearchResults.Controls.Count % 2) != 0 ? "listItem" : "listAltItem"));

                    //
                    // Add in the link button to select this person.
                    //
                    cell = new HtmlTableCell();
                    link = new LinkButton();
                    link.ID = "btnPerson" + p.PersonID;
                    link.Text = p.LastName + ", " + p.FirstName;
                    link.Click += new EventHandler(btnSelectPerson_Click);
                    cell.Controls.Add(link);
                    row.Cells.Add(cell);

                    //
                    // Add in the gender.
                    //
                    row.Cells.Add(TableCellString(null, p.Gender.ToString()));

                    //
                    // Add in the age.
                    //
                    row.Cells.Add(TableCellString(null, (p.Age != -1 ? p.Age.ToString() : "")));

                    //
                    // Add in the grade.
                    //
                    grade = Person.GetGradeName(Person.CalculateGradeLevel(p.GraduationDate, CurrentOrganization.GradePromotionDate));
                    row.Cells.Add(TableCellString(null, grade));

                    //
                    // Add in the Main/Home phone number.
                    //
                    phone = p.Phones.FindByType(new Guid("F2A0FBA2-D5AB-421F-A5AB-0C67DB6FD72E"));
                    if (phone != null && phone.Number != "")
                    {
                        if (phone.Unlisted)
                            row.Cells.Add(TableCellString(null, phone.Number + " (unlisted)"));
                        else
                            row.Cells.Add(TableCellString(null, phone.Number));
                    }
                    else
                        row.Cells.Add(TableCellString(null, ""));

                    //
                    // Add in the e-mail address.
                    //
                    row.Cells.Add(TableCellString(null, p.Emails.FirstActive));

                    phSearchResults.Controls.Add(row);
                }
            }

            //
            // If they have selected a family then build up the members.
            //
            if (hfFamily.Value != "")
            {
                Family f = new Family(Convert.ToInt32(hfFamily.Value));
                FamilyMember fm;
                HtmlTableRow row;
                HtmlTableCell cell;
                DropDownList list;
                string grade;
                int i;

                //
                // Build each family member row.
                //
                phFamilyMembers.Controls.Clear();
                for (i = 0; i < f.FamilyMembers.Count; i++)
                {
                    fm = f.FamilyMembers[i];
                    row = new HtmlTableRow();
                    row.Height = "25";
                    row.VAlign = "top";
                    phFamilyMembers.Controls.Add(row);

                    row.Cells.Add(TableCellLookupDropDownList("ddlMemberTitle_" + i.ToString(), new Guid("3394ca53-5791-42c8-b996-1d77c740cf03"), (SetValues ? fm.Title.LookupID.ToString() : null), false));
                    row.Cells.Add(TableCellTextBox("tbMemberFirstName_" + i.ToString(), (SetValues ? fm.FirstName : null), 65));
                    row.Cells.Add(TableCellTextBox("tbMemberLastName_" + i.ToString(), (SetValues ? fm.LastName : null), 90));
                    row.Cells.Add(TableCellLookupDropDownList("ddlMemberFamilyRole_" + i.ToString(), new Guid("d3ce5e62-4ef2-4ff8-a80d-5492bf995459"), (SetValues ? fm.FamilyRole.LookupID.ToString() : null), true));
                    list = (DropDownList)row.Cells[row.Cells.Count - 1].Controls[0];
                    list.Attributes["onchange"] = "hdc_toggleFamilyAttributes();";
                    row.Cells.Add(TableCellEnumDropDownList("ddlMemberGender_" + i.ToString(), typeof(Arena.Enums.Gender), (SetValues ? fm.Gender.ToString() : null)));
                    row.Cells.Add(TableCellDateTextBox("dtbMemberBirthDate_" + i.ToString(), (SetValues ? fm.BirthDate.ToString("MM/dd/yyyy") : null)));

                    //
                    // Grade
                    //
                    grade = Person.CalculateGradeLevel(fm.GraduationDate, CurrentOrganization.GradePromotionDate).ToString();
                    cell = new HtmlTableCell();
                    list = new DropDownList();
                    cell.Controls.Add(list);
                    PopulateGrades(list);
                    list.CssClass = "smallText";
                    list.ID = "ddlMemberGrade_" + i.ToString();
                    if (SetValues)
                        list.SelectedValue = grade;
                    row.Cells.Add(cell);

                    row.Cells.Add(TableCellLookupDropDownList("ddlMemberMaritalStatus_" + i.ToString(), new Guid("0aad26c7-ad9d-4fe8-96b1-c9bcd033bb5b"), (SetValues ? fm.MaritalStatus.LookupID.ToString() : null), true));
                    row.Cells.Add(TableCellDateTextBox("dtbMemberAnniversaryDate_" + i.ToString(), (SetValues ? fm.AnniversaryDate.ToString("MM/dd/yyyy") : null)));
                    row.Cells.Add(TableCellTextBox("tbMemberEmail_" + i.ToString(), (SetValues ? (fm.Emails.FirstActive != null ? fm.Emails.FirstActive : "") : null), 150));

                    //
                    // Add in an image with a javascript click handler that will toggle
                    // the visibility of the extra fields. If you change the image file
                    // here you must also update the javascript code.
                    //
                    Image img = new Image();
                    img.ImageUrl = BaseUrl() + "Images/information2.gif";
                    img.ID = "imgMemberShowExtra_" + i.ToString();
                    img.Attributes.Add("onclick", "hdc_toggleExtraFields(this);");
                    cell = new HtmlTableCell();
                    cell.Controls.Add(img);
                    row.Cells.Add(cell);

                    //
                    // Add another row for the extra information that will appear
                    // and disappear depending on first-row selections.
                    //
                    cell = TableCellString(null, "extra");
                    cell.ColSpan = row.Cells.Count;
                    row = new HtmlTableRow();
                    row.ID = "trMemberExtraFields_" + i.ToString();
                    row.Cells.Add(cell);
                    row.Style.Add("display", "none");
                    phFamilyMembers.Controls.Add(row);
                }
            }

            BuildExtraRows(SetValues);
            Page.ClientScript.RegisterStartupScript(typeof(Page), "hdc_toggleFamilyAttributes", "<script>hdc_toggleFamilyAttributes();</script>");

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
            HtmlTableRow row;
            HtmlTableCell cell;
            DropDownList list;
            Lookup lu;
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
                //
                // Determine if we are adding a new adult row or child row.
                //
                if (i < 2)
                {
                    lu = new Lookup(new Guid("E410E1A6-8715-4BFB-BF03-1CD18051F815"));
                }
                else
                {
                    lu = new Lookup(new Guid("9EF9E984-923C-4206-A2CF-17ADAF2E6659"));
                }

                //
                // Build the primary data row.
                //
                row = new HtmlTableRow();
                row.Height = "25";
                row.VAlign = "top";
                row.Cells.Add(TableCellLookupDropDownList("ddlMemberTitle_" + i.ToString(), new Guid("3394ca53-5791-42c8-b996-1d77c740cf03"), (SetValues ? "" : null), false));
                row.Cells.Add(TableCellTextBox("tbMemberFirstName_" + i.ToString(), (SetValues ? "" : null), 65));
                row.Cells.Add(TableCellTextBox("tbMemberLastName_" + i.ToString(), (SetValues ? tbFamilyName.Text : null), 90));
                row.Cells.Add(TableCellLookupDropDownList("ddlMemberFamilyRole_" + i.ToString(), new Guid("d3ce5e62-4ef2-4ff8-a80d-5492bf995459"), (SetValues ? lu.LookupID.ToString() : null), true));
                list = (DropDownList)row.Cells[row.Cells.Count - 1].Controls[0];
                list.Attributes["onchange"] = "hdc_toggleFamilyAttributes();";
                row.Cells.Add(TableCellEnumDropDownList("ddlMemberGender_" + i.ToString(), typeof(Arena.Enums.Gender), (SetValues ? "" : null)));
                row.Cells.Add(TableCellDateTextBox("dtbMemberBirthDate_" + i.ToString(), (SetValues ? "" : null)));

                //
                // Grade
                //
                cell = new HtmlTableCell();
                list = new DropDownList();
                cell.Controls.Add(list);
                PopulateGrades(list);
                list.CssClass = "smallText";
                list.ID = "ddlMemberGrade_" + i.ToString();
                row.Cells.Add(cell);

                row.Cells.Add(TableCellLookupDropDownList("ddlMemberMaritalStatus_" + i.ToString(), new Guid("0aad26c7-ad9d-4fe8-96b1-c9bcd033bb5b"), (SetValues ? "" : null), true));
                row.Cells.Add(TableCellDateTextBox("dtbMemberAnniversaryDate_" + i.ToString(), (SetValues ? "" : null)));
                row.Cells.Add(TableCellTextBox("tbMemberEmail_" + i.ToString(), (SetValues ? "" : null), 150));

                phFamilyMembers.Controls.Add(row);

                //
                // Add in an image with a javascript click handler that will toggle
                // the visibility of the extra fields. If you change the image file
                // here you must also update the javascript code.
                //
                Image img = new Image();
                img.ImageUrl = BaseUrl() + "Images/information2.gif";
                img.ID = "imgMemberShowExtra_" + i.ToString();
                img.Attributes.Add("onclick", "hdc_toggleExtraFields(this);");
                cell = new HtmlTableCell();
                cell.Controls.Add(img);
                row.Cells.Add(cell);

                //
                // Add another row for the extra information that will appear
                // and disappear depending on first-row selections.
                //
                cell = TableCellString(null, "extra");
                cell.ColSpan = row.Cells.Count;
                row = new HtmlTableRow();
                row.ID = "trMemberExtraFields_" + i.ToString();
                row.Cells.Add(cell);
                row.Style.Add("display", "none");
                phFamilyMembers.Controls.Add(row);
            }
        }

        /// <summary>
        /// Generate a HtmlTableCell that contains a single control, a Label
        /// that contains the string value.
        /// </summary>
        /// <param name="value">The string to display inside the cell.</param>
        /// <returns>A new TableCell containing the given string.</returns>
        private HtmlTableCell TableCellString(string controlId, string value)
        {
            Label label;
            HtmlTableCell cell = new HtmlTableCell();


            label = new Label();
            label.CssClass = "smallText";
            label.Text = value;
            if (controlId != null)
                label.ID = controlId;
            cell.Controls.Add(label);

            return cell;
        }

        private HtmlTableCell TableCellLookupDropDownList(string controlId, Guid guid, string value, bool required)
        {
            HtmlTableCell cell = new HtmlTableCell();
            LookupCollection lookups = new LookupCollection(guid);
            DropDownList list = new DropDownList();

            if (required == false)
                list.Items.Add(new ListItem("", "-1"));
            lookups.LoadDropDownList(list);
            list.CssClass = "smallText";
            if (controlId != null)
                list.ID = controlId;
            if (value != null)
                list.SelectedValue = value;

            cell.Controls.Add(list);

            return cell;
        }

        private HtmlTableCell TableCellEnumDropDownList(string controlId, Type eType, string value)
        {
            HtmlTableCell cell = new HtmlTableCell();
            DropDownList list = new DropDownList();
            string[] names = Enum.GetNames(eType);
            string eValue;
            int i;

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

            cell.Controls.Add(list);

            return cell;
        }

        private HtmlTableCell TableCellTextBox(string controlId, string value, int width)
        {
            HtmlTableCell cell = new HtmlTableCell();
            TextBox tb = new TextBox();

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

        private HtmlTableCell TableCellDateTextBox(string controlId, string value)
        {
            HtmlTableCell cell = new HtmlTableCell();
            DateTextBox dtb = new DateTextBox();


            dtb.CssClass = "smallText";
            dtb.Width = Unit.Pixel(65);
            dtb.MaxLength = 10;
            if (controlId != null)
                dtb.ID = controlId;
            if (value != null)
                dtb.Text = value;

            cell.Controls.Add(dtb);

            return cell;
        }

        private void PopulateGrades(DropDownList list)
        {
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


        #endregion

        #region Web Form Code

        override protected void OnInit(EventArgs e)
        {
            btnSearch.Click += new EventHandler(btnSearch_Click);
            btnNewFamily.Click += new EventHandler(btnNewFamily_Click);
            btnAddMore.Click += new EventHandler(btnAddMore_Click);
            base.OnInit(e);
        }

        #endregion
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