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
	using Arena.Portal;
	using Arena.Core;

	public partial class FamilyRegistration : PortalControl
    {
        #region Module Settings

        [BooleanSetting("Allow Searching", "Allows searches to be performed to look families up, also implies limited edit ability.", true, false)]
        public bool AllowSearchSetting { get { return Convert.ToBoolean(Setting("AllowSearch", "false", true)); } }

        [TextSetting("Country Code", "The default country code (e.g. US) to use when adding new families", false)]
        public string CountryCodeSetting { get { return Setting("CountryCode", "", false); } }

        #endregion

        #region Event Handlers

        private bool pageUpdated = false;

        private void Page_Load(object sender, System.EventArgs e)
		{
            if (!IsPostBack)
            {
                //
                // If searching for families is allowed, then show the search panel.
                //
                if (AllowSearchSetting == true)
                    pnlFindFamily.Visible = true;

                //
                // Add in the drop down list control for selecting the
                // country. Also select the default country.
                //
                Utilities.LoadCountries(ddlAddressCountry);
                if (CountryCodeSetting != null && CountryCodeSetting != "")
                    ddlAddressCountry.SelectedValue = CountryCodeSetting;
            }
            else
                UpdatePage();
		}

        /// <summary>
        /// User is searching for a family, hide the family panel and show
        /// the results panel.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        void btnSearch_Click(object sender, EventArgs e)
        {
            hfFindName.Value = tbFindName.Text;
            hfFindPhone.Value = tbFindPhone.Text;
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
            pnlFindResults.Visible = false;
            pnlFamily.Visible = true;
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
        }

        void UpdatePage()
        {
            PersonCollection collection;

            if (pageUpdated == true)
                return;
            pageUpdated = true;
            collection = new PersonCollection();
            collection.LoadByName(tbFindName.Text, "");

            foreach (Person p in collection)
            {
                TableRow row = new TableRow();
                TableCell cell;
                LinkButton link;
                PersonPhone phone;


                //
                // Add in the link button to select this person.
                //
                cell = new TableCell();
                link = new LinkButton();
                link.ID = "btnPerson" + p.PersonID;
                link.Text = p.FullName;
                link.Click += new EventHandler(btnSelectPerson_Click);
                link.CssClass = "smallText";
                cell.Controls.Add(link);
                row.Cells.Add(cell);

                //
                // Add in the gender.
                //
                row.Cells.Add(TableCellString(p.Gender.ToString()));

                //
                // Add in the age.
                //
                row.Cells.Add(TableCellString((p.Age != -1 ? p.Age.ToString() : "")));

                //
                // Add in the Main/Home phone number.
                //
                phone = p.Phones.FindByType(new Guid("F2A0FBA2-D5AB-421F-A5AB-0C67DB6FD72E"));
                if (phone != null && phone.Number != "")
                {
                    if (phone.Unlisted)
                        row.Cells.Add(TableCellString("(unlisted)"));
                    else
                        row.Cells.Add(TableCellString(phone.Number));
                }
                else
                    row.Cells.Add(TableCellString(""));

                //
                // Add in the e-mail address.
                //
                row.Cells.Add(TableCellString(p.Emails.FirstActive));

                tblSearchResults.Rows.Add(row);
            }
        }

        /// <summary>
        /// Generate a TableCell that contains a single control, a Label
        /// that contains the string value.
        /// </summary>
        /// <param name="value">The string to display inside the cell.</param>
        /// <returns>A new TableCell containing the given string.</returns>
        private TableCell TableCellString(string value)
        {
            Label label;
            TableCell cell = new TableCell();


            label = new Label();
            label.CssClass = "smallText";
            label.Text = value;
            cell.Controls.Add(label);

            return cell;
        }

        #endregion

        #region Web Form Code

        override protected void OnInit(EventArgs e)
        {
            btnSearch.Click += new EventHandler(btnSearch_Click);
            btnNewFamily.Click += new EventHandler(btnNewFamily_Click);
            base.OnInit(e);
        }

        #endregion
    }
}