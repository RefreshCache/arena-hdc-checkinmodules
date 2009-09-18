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

	public partial class AttendanceOverview : PortalControl
	{
        const int kOccurrenceNameLinkColumn = 0;
        const int kOccurrenceNameColumn = 1;
        const int kOccurrenceTypeLinkColumn = 2;
        const int kOccurrenceTypeColumn = 3;
        const int kLocationLocationLinkColumn = 0;
        const int kLocationLocationColumn = 1;

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

        #endregion

        #region Event Handlers

        protected void Page_Init(object sender, EventArgs e)
        {
            dgOccurrence.ReBind += new Arena.Portal.UI.DataGridReBindEventHandler(dgOccurrence_ReBind);
            dgLocation.ReBind += new Arena.Portal.UI.DataGridReBindEventHandler(dgLocation_ReBind);
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
                if (oc.Count > 0)
                {
                    ddlFilterTypeGroup.SelectedValue = new OccurrenceType(oc[0].OccurrenceTypeID).GroupId.ToString();
                }
                else
                    ddlFilterTypeGroup.SelectedIndex = 0;
                ddlFilterTypeGroup_Changed(null, null);
                btnFilterApply_Click(null, null);
            }
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

            if (true)
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