/**********************************************************************
* Description:	This module is designed to provide a place for users to
*               to able to get an overview of a service/event that is
*               using occurrences. A "service" is a collection of
*               occurrences that all start at the same time. With this
*               module you can close down a room and select which room
*               you want to move attendance to after that.
* Created By:	Daniel Hazelbaker, High Desert Church
* Date Created:	9/16/2009 10:34:07 AM
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
    using System.Data.SqlClient;
    using System.Configuration;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
    using System.Text;
    using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.WebControls.WebParts;
	using System.Web.UI.HtmlControls;

	using Arena.Portal;
	using Arena.Core;
    using Arena.Organization;
    using Arena.SmallGroup;

	public partial class AttendanceOverview : PortalControl
	{
        const int kOccurrenceNameLinkColumn = 0;
        const int kOccurrenceNameColumn = 1;
        const int kOccurrenceTypeLinkColumn = 2;
        const int kOccurrenceTypeColumn = 3;
        const int kOccurrenceCheckInStartColumn = 4;
        const int kOccurrenceCheckInEndColumn = 5;
        const int kOccurrenceLocationColumn = 6;
        const int kOccurrenceAttendanceColumn = 7;
        const int kOccurrenceCloseColumn = 8;

        const int kLocationLocationLinkColumn = 0;
        const int kLocationLocationColumn = 1;
        const int kLocationAttendanceColumn = 2;
        const int kLocationActiveOccurrencesColumn = 3;
        const int kLocationCloseColumn = 4;

        const int kCloseOccurrenceIDColumn = 0;
        const int kCloseOccurrenceNameColumn = 1;
        const int kCloseAttendanceTypeColumn = 2;
        const int kCloseOldLocationColumn = 3;
        const int kCloseNewLocationColumn = 4;

        public static string GetBaseURL()
        {
            string url = null;
            url = HttpContext.Current.Request.Url.Scheme + "://" +
                            HttpContext.Current.Request.Url.Authority +
                            HttpContext.Current.Request.ApplicationPath;
            if ((url.EndsWith("/")))
            {
                return url.Remove(url.LastIndexOf("/"));
            }
            else
            {
                return url;
            }
        }

        #region Module Settings

        [PageSettingAttribute("Attendance Detail Page", "Page to use for viewing detailed attendance information.", false)]
        public int AttendanceDetailPageID { get { return Convert.ToInt32(Setting("AttendanceDetailPageID", "-1", false)); } }

        [ListFromSqlSetting("Default Attendance Group", "Defines the default attendance group to use when determining the active service.", false, "",
            "SELECT [group_id],[group_name] FROM [core_occurrence_type_group]")]
        public string DefaultAttendanceGroupIDSetting { get { return Setting("DefaultAttendanceGroupID", "", false); } }

        #endregion

        #region Event Handlers

        protected void Page_Init(object sender, EventArgs e)
        {
            dgOccurrence.ReBind += new Arena.Portal.UI.DataGridReBindEventHandler(dgOccurrence_ReBind);
            dgOccurrence.ItemDataBound += new DataGridItemEventHandler(dgOccurrence_DataBound);
            dgLocation.ReBind += new Arena.Portal.UI.DataGridReBindEventHandler(dgLocation_ReBind);
            dgLocation.ItemDataBound += new DataGridItemEventHandler(dgLocation_DataBound);
        }
        
        private void Page_Load(object sender, System.EventArgs e)
		{
			if ( ! IsPostBack )
			{
                //
                // Setup the data grid.
                //
                dgOccurrence.ItemType = "Occurrence";
                dgOccurrence.ItemBgColor = CurrentPortalPage.Setting("ItemBgColor", string.Empty, false);
                dgOccurrence.ItemAltBgColor = CurrentPortalPage.Setting("ItemAltBgColor", string.Empty, false);
                dgOccurrence.ItemMouseOverColor = CurrentPortalPage.Setting("ItemMouseOverColor", string.Empty, false);
                dgOccurrence.AddEnabled = false;
                dgOccurrence.MoveEnabled = false;
                dgOccurrence.DeleteEnabled = false;
                dgOccurrence.EditEnabled = false;
                dgOccurrence.MergeEnabled = false;
                dgOccurrence.MailEnabled = false;
                dgOccurrence.ExportEnabled = true;
                dgOccurrence.AllowSorting = true;
                dgOccurrence.Columns[kOccurrenceNameColumn].Visible = (AttendanceDetailPageID == -1);
                dgOccurrence.Columns[kOccurrenceNameLinkColumn].Visible = (AttendanceDetailPageID != -1);
                dgOccurrence.Columns[kOccurrenceTypeColumn].Visible = (AttendanceDetailPageID == -1);
                dgOccurrence.Columns[kOccurrenceTypeLinkColumn].Visible = (AttendanceDetailPageID != -1);

                //
                // Setup the data grid.
                //
                dgLocation.ItemType = "Location";
                dgLocation.ItemBgColor = CurrentPortalPage.Setting("ItemBgColor", string.Empty, false);
                dgLocation.ItemAltBgColor = CurrentPortalPage.Setting("ItemAltBgColor", string.Empty, false);
                dgLocation.ItemMouseOverColor = CurrentPortalPage.Setting("ItemMouseOverColor", string.Empty, false);
                dgLocation.AddEnabled = false;
                dgLocation.MoveEnabled = false;
                dgLocation.DeleteEnabled = false;
                dgLocation.EditEnabled = false;
                dgLocation.MergeEnabled = false;
                dgLocation.MailEnabled = false;
                dgLocation.ExportEnabled = true;
                dgLocation.AllowSorting = true;
                dgLocation.Columns[kLocationLocationColumn].Visible = (AttendanceDetailPageID == -1);
                dgLocation.Columns[kLocationLocationLinkColumn].Visible = (AttendanceDetailPageID != -1);

                //
                // Fill in the filter options.
                //
                OccurrenceTypeGroupCollection otgc = new OccurrenceTypeGroupCollection(CurrentOrganization.OrganizationID);
                foreach (OccurrenceTypeGroup otg in otgc)
                {
                    ddlFilterTypeGroup.Items.Add(new ListItem(otg.GroupName, otg.GroupId.ToString()));
                }
                OccurrenceCollection oc = new OccurrenceCollection(DateTime.Now, DateTime.Now);
                if (DefaultAttendanceGroupIDSetting != "")
                {
                    ddlFilterTypeGroup.SelectedValue = DefaultAttendanceGroupIDSetting.ToString();
                }
                else if (oc.Count > 0)
                {
                    //
                    // Find the first occurrence with an actual location.
                    //
                    foreach (Occurrence o in oc)
                    {
                        if (o.LocationID != -1)
                        {
                            ddlFilterTypeGroup.SelectedValue = new OccurrenceType(o.OccurrenceTypeID).GroupId.ToString();
                            break;
                        }
                    }
                }

                if (ddlFilterTypeGroup.SelectedValue == null || ddlFilterTypeGroup.SelectedValue == "")
                    ddlFilterTypeGroup.SelectedIndex = 0;

                ddlFilterTypeGroup_Changed(null, null);
                btnFilterApply_Click(null, null);

                //
                // Setup options on the finalize close button.
                //
                lbCloseFinish.Style.Add("text-decoration", "none");
                lbCloseFinish.Attributes.Add("onmouseover", "this.style.textDecoration='underline';");
                lbCloseFinish.Attributes.Add("onmouseout", "this.style.textDecoration='none';");
            }

            dgClose_Bind();
        }

        void dgOccurrence_ReBind(object sender, EventArgs e)
        {
            int totalAttendance = 0;


            DataTable dt = GetOccurrenceData();
            dgOccurrence.DataSource = dt;
            dgOccurrence.DataBind();

            //
            // Update the total count.
            //
            foreach (DataRow dr in dt.Rows)
            {
                totalAttendance += Convert.ToInt32(dr["attendance_count"]);
            }

            lbTotalAttendance.Text = String.Format("Total attendance this service is {0} people.", totalAttendance);
        }

        void dgLocation_ReBind(object sender, EventArgs e)
        {
            dgLocation.DataSource = GetLocationData();
            dgLocation.DataBind();
        }

        public void dgOccurrence_DataBound(object sender, DataGridItemEventArgs e)
        {
            DataRowView drv = (DataRowView)e.Item.DataItem;
            LinkButton lb;

            //
            // Skip row header and footer.
            //
            if (e.Item.ItemIndex == -1)
                return;

            lb = (LinkButton)e.Item.Cells[kOccurrenceCloseColumn].Controls[1];
            if (drv["occurrence_closed"].ToString() == "True" ||
                (DateTime.Parse(drv["occurrence_end_time"].ToString()) < DateTime.Now &&
                DateTime.Parse(drv["check_in_end"].ToString()) < DateTime.Now))
                lb.Visible = false;
            lb.CommandArgument = drv["occurrence_id"].ToString();
        }

        public void dgLocation_DataBound(object sender, DataGridItemEventArgs e)
        {
            DataRowView drv = (DataRowView)e.Item.DataItem;
            LinkButton lb;

            //            new Occurrence(e.Item.DataItem
            //
            // Skip row header and footer.
            //
            if (e.Item.ItemIndex == -1)
                return;

            lb = (LinkButton)e.Item.Cells[kLocationCloseColumn].Controls[1];
            if (drv["active_occurrence_count"].ToString() == "0")
                lb.Visible = false;
            lb.CommandArgument = drv["location_id"].ToString();
        }

        public void dgOccurrence_Close(object sender, CommandEventArgs e)
        {
            hfCloseOccurrenceIDs.Value = e.CommandArgument.ToString();
            lbCloseError.Text = "";
            pnlCloseOccurrence.Visible = true;
            pnlDataFilter.Visible = false;
            pnlLocationGrid.Visible = false;
            pnlOccurrenceGrid.Visible = false;
            pnlTotalAttendance.Visible = false;

            dgClose_Bind();
        }

        public void dgClose_Bind()
        {
            DropDownList ddl;
            TableRow tr;
            TableCell cell;
            Label lb;

            //
            // Remove everything except the first row.
            //
			while (tblClose.Rows.Count > 2)
				tblClose.Rows.RemoveAt(1);

            if (hfCloseOccurrenceIDs.Value != "")
            {
                string[] ids = hfCloseOccurrenceIDs.Value.Split(new char[] { ',' });


                //
                // For each occurrence, put up a table row for the user to
                // choose the new location for the occurrence.
                //
                foreach (string occurrenceID in ids)
                {
                    Occurrence o = new Occurrence(Convert.ToInt32(occurrenceID));

                    tr = new TableRow();
                    tblClose.Rows.AddAt(1, tr);

                    //
                    // Add in the occurrence ID.
                    //
                    cell = new TableCell();
                    tr.Cells.Add(cell);
					cell.Style.Add("display", "none");
					lb = new Label();
                    cell.Controls.Add(lb);
                    lb.Text = o.OccurrenceID.ToString();

                    //
                    // Add in the cell for the occurrence.
                    //
                    cell = new TableCell();
                    tr.Cells.Add(cell);
                    lb = new Label();
                    cell.Controls.Add(lb);
                    lb.CssClass = "smallText";
                    lb.Text = o.Name;

                    //
                    // Add in the cell showing the occurrence type.
                    //
                    cell = new TableCell();
                    tr.Cells.Add(cell);
                    lb = new Label();
                    cell.Controls.Add(lb);
                    lb.CssClass = "smallText";
                    lb.Text = o.OccurrenceType.TypeName;

                    //
                    // Add in the cell showing the old location name.
                    //
                    cell = new TableCell();
                    tr.Cells.Add(cell);
                    lb = new Label();
                    cell.Controls.Add(lb);
                    lb.CssClass = "smallText";
                    lb.Text = new Location(o.LocationID).FullName;

                    //
                    // Add in the cell for the new room selection.
                    //
                    cell = new TableCell();
                    tr.Cells.Add(cell);
                    ddl = new DropDownList();
                    cell.Controls.Add(ddl);
                    ddl.Items.Add(new ListItem("", ""));
                    if (true)
                        ddl.Items.Add(new ListItem("Do Not Open", "-1"));
                    foreach (Location l in o.OccurrenceType.Locations)
                    {
                        if (l.LocationId == o.LocationID)
                            continue;

                        ddl.Items.Add(new ListItem(l.FullName, l.LocationId.ToString()));
                    }
                }
            }
        }

        public void dgLocation_Close(object sender, CommandEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            Location l;
            int i;


            //
            // Run the function to get the active occurrences for this location.
            //
            SqlDataReader reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(
                        String.Format("SELECT * FROM cust_hdc_checkin_func_active_occurrencesByLocation({0})", e.CommandArgument));
            while (reader.Read())
            {
                if (sb.Length > 0)
                    sb.Append(",");
                sb.Append(reader[0].ToString());
            }

            hfCloseOccurrenceIDs.Value = sb.ToString();
            lbCloseError.Text = "";
            pnlCloseOccurrence.Visible = true;
            pnlDataFilter.Visible = false;
            pnlLocationGrid.Visible = false;
            pnlOccurrenceGrid.Visible = false;
            pnlTotalAttendance.Visible = false;

            dgClose_Bind();
        }

        public void ddlFilterTypeGroup_Changed(object sender, EventArgs e)
        {
            OccurrenceCollection oc;


            //
            // Setup the basic occurrence list.
            //
            ddlFilterService.Items.Clear();

            //
            // Add in all valid choices.
            //
            if (ddlFilterTypeGroup.SelectedValue != "-1")
            {
                OccurrenceTypeCollection otc = new OccurrenceTypeCollection(Convert.ToInt32(ddlFilterTypeGroup.SelectedValue));
                ArrayList dates = new ArrayList();

                foreach (OccurrenceType ot in otc)
                {
                    oc = new OccurrenceCollection(ot.OccurrenceTypeId);
                    foreach (Occurrence o in oc)
                    {
                        if (dates.Contains(o.StartTime) == false)
                            dates.Add(o.StartTime);
                    }
                }

                dates.Sort();
                dates.Reverse();

                foreach (DateTime dt in dates)
                {
                    ddlFilterService.Items.Add(new ListItem(dt.ToString("ddd M/dd/yy h:mm tt"), dt.ToShortDateTimeString()));
                }

                ddlFilterService.Enabled = true;
            }
            else
            {
                ddlFilterService.Enabled = false;
            }

            if (ddlFilterService.Items.Count > 0)
                ddlFilterService.SelectedIndex = 0;
        }

        public void btnFilterApply_Click(object sender, EventArgs e)
        {
            hfFilterTypeGroupID.Value = ddlFilterTypeGroup.SelectedValue;
            hfFilterService.Value = ddlFilterService.SelectedValue;

            dgOccurrence_ReBind(null, null);
            dgLocation_ReBind(null, null);

            if (AttendanceDetailPageID != -1)
            {
                object[] parms = new object[3] { AttendanceDetailPageID, Server.UrlEncode(hfFilterTypeGroupID.Value), Server.UrlEncode(hfFilterService.Value) };

                hlDetails.NavigateUrl = String.Format(GetBaseURL() + "/default.aspx?page={0}&groupID={1}&service={2}", parms);
                hlDetails.Text = "(View Details)";
                hlDetails.Visible = true;
            }
            else
            {
                hlDetails.NavigateUrl = "";
                hlDetails.Text = "";
                hlDetails.Visible = false;
            }
        }

        public void btnCloseFinish_Click(object sender, EventArgs e)
        {
            DropDownList ddl;
            TableRow tr;
            int i;


            //
            // Walk through each occurrence row and make sure they have
            // selected a new target location.
            //
            for (i = 1; i < (tblClose.Rows.Count - 1); i++)
            {
                tr = tblClose.Rows[i];
                ddl = (DropDownList)tr.Cells[kCloseNewLocationColumn].Controls[0];
                if (ddl.SelectedValue == "")
                {
                    lbCloseError.Text = "You must select a new target location for each occurrence.";

                    return;
                }
                else
                {
                    lbCloseError.Text = "";
                }
            }

            //
            // Walk through each occurrence row and open a new occurrence
            // if they have selected a new location. After the occurrence
            // is opened, close the old occurrence.
            //
            for (i = 1; i < (tblClose.Rows.Count - 1); i++)
            {
                tr = tblClose.Rows[i];
                ddl = (DropDownList)tr.Cells[kCloseNewLocationColumn].Controls[0];
                try
                {
                    Occurrence oldOccurrence;
                    int locationID = Convert.ToInt32(ddl.SelectedValue);

                    oldOccurrence = new Occurrence(Convert.ToInt32(((Label)tr.Cells[kCloseOccurrenceIDColumn].Controls[0]).Text));

                    //
                    // Setup the new location.
                    //
                    if (locationID != -1)
                    {
                        ProfileOccurrence po = new ProfileOccurrence(oldOccurrence.OccurrenceID);
                        GroupOccurrence go = new GroupOccurrence(oldOccurrence.OccurrenceID);
                        Occurrence newOccurrence = new Occurrence();

                        newOccurrence.AreaID = oldOccurrence.AreaID;
                        newOccurrence.CheckInEnd = oldOccurrence.CheckInEnd;
                        newOccurrence.CheckInStart = oldOccurrence.CheckInStart;
                        newOccurrence.Description = oldOccurrence.Description;
                        newOccurrence.EndTime = oldOccurrence.EndTime;
                        newOccurrence.LocationID = locationID;
                        newOccurrence.Location = new Location(locationID).FullName;
                        newOccurrence.MembershipRequired = oldOccurrence.MembershipRequired;
                        newOccurrence.Name = oldOccurrence.Name;
                        newOccurrence.OccurrenceTypeID = oldOccurrence.OccurrenceTypeID;
                        newOccurrence.StartTime = oldOccurrence.StartTime;
                        newOccurrence.Title = oldOccurrence.Title;
                        newOccurrence.Save(CurrentUser.Identity.Name);

                        //
                        // Create the link to the profile, if there is one.
                        //
                        if (po.ProfileID != -1)
                        {
                            ProfileOccurrence npo = new ProfileOccurrence(newOccurrence.OccurrenceID);
                            npo.ProfileID = po.ProfileID;
                            npo.Save(CurrentUser.Identity.Name);
                        }

                        //
                        // Create the link to the group, if there is one.
                        //
                        if (go.GroupID != -1)
                        {
                            GroupOccurrence ngo = new GroupOccurrence(newOccurrence.OccurrenceID);
                            ngo.GroupID = go.GroupID;
                            ngo.Save(CurrentUser.Identity.Name);
                        }
                    }

                    //
                    // Close the old occurrence.
                    //
                    oldOccurrence.OccurrenceClosed = true;
                    oldOccurrence.Save(CurrentUser.Identity.Name);
                }
                catch
                {
                }
            }

			btnCloseCancel_Click(sender, e);
        }

		public void btnCloseCancel_Click(object sender, EventArgs e)
		{
			//
			// Clear old content.
			//
			while (tblClose.Rows.Count > 2)
				tblClose.Rows.RemoveAt(1);

			//
			// Switch back to the normal view.
			//
			pnlCloseOccurrence.Visible = false;
			pnlDataFilter.Visible = true;
			pnlLocationGrid.Visible = true;
			pnlOccurrenceGrid.Visible = true;
			pnlTotalAttendance.Visible = true;

			dgOccurrence_ReBind(null, null);
			dgLocation_ReBind(null, null);
		}

        #endregion

        private DataTable GetOccurrenceData()
        {
            ArrayList paramList = new ArrayList();

            paramList.Add(new SqlParameter("group_id", Convert.ToInt32(hfFilterTypeGroupID.Value)));
            paramList.Add(new SqlParameter("service_time", DateTime.Parse(hfFilterService.Value)));

            //
            // Open the data reader
            //
            SqlDataReader reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(
                        "cust_hdc_checkin_sp_get_service_overviewByOccurrence", paramList);

            return SqlReaderToDataTable(reader);
        }

        private DataTable GetLocationData()
        {
            ArrayList paramList = new ArrayList();

            paramList.Add(new SqlParameter("group_id", Convert.ToInt32(hfFilterTypeGroupID.Value)));
            paramList.Add(new SqlParameter("service_time", DateTime.Parse(hfFilterService.Value)));

            //
            // Open the data reader
            //
            SqlDataReader reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(
                        "cust_hdc_checkin_sp_get_service_overviewByLocation", paramList);

            return SqlReaderToDataTable(reader);
        }

        static private DataTable SqlReaderToDataTable(SqlDataReader rdr)
        {
            DataTable dt = new DataTable("hdc_customTable");
            DataRow row;
            int i;


            for (i = 0; i < rdr.FieldCount; i++)
            {
                dt.Columns.Add(rdr.GetName(i));
            }

            while (rdr.Read())
            {
                row = dt.NewRow();

                for (i = 0; i < rdr.FieldCount; i++)
                {
                    row[i] = rdr[i];
                }

                dt.Rows.Add(row);
            }

            return dt;
        }
    }
}