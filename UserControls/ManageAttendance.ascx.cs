/**********************************************************************
* Description:	This module is designed to allow a user with appropriate access
*               to more easily manage attendance and the check-in process while
*               a program is happening.
* Created By:	Daniel Hazelbaker, High Desert Church
* Date Created:	7/22/2009 1:13:30 PM
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

    using Arena.Custom.Cccev.CheckIn;

	public partial class ManageAttendance : PortalControl
	{
        [PageSettingAttribute("Person Detail Page", "Page to use for viewing a person's detail information.", false)]
        public int PersonDetailPageID { get { return Convert.ToInt32(Setting("PersonDetailPageID", "-1", false)); } }

		#region Event Handlers

        protected void Page_Init(object sender, EventArgs e)
        {
            dgAttendance.ReBind += new Arena.Portal.UI.DataGridReBindEventHandler(dgAttendance_ReBind);
        }

		private void Page_Load(object sender, System.EventArgs e)
		{
			if ( ! IsPostBack )
			{
                //
                // Setup the data grid.
                //
                dgAttendance.ItemType = "Attendance Record";
                dgAttendance.ItemBgColor = CurrentPortalPage.Setting("ItemBgColor", string.Empty, false);
                dgAttendance.ItemAltBgColor = CurrentPortalPage.Setting("ItemAltBgColor", string.Empty, false);
                dgAttendance.ItemMouseOverColor = CurrentPortalPage.Setting("ItemMouseOverColor", string.Empty, false);
                dgAttendance.AddEnabled = false;
                dgAttendance.MoveEnabled = false;
                dgAttendance.DeleteEnabled = false;
                dgAttendance.EditEnabled = false;
                dgAttendance.MergeEnabled = false;
                dgAttendance.MailEnabled = false;
                dgAttendance.ExportEnabled = true;
                dgAttendance.AllowSorting = true;

                //
                // Turn off either the regular name, or the hyperlink name.
                //
                if (PersonDetailPageID == -1)
                {
                    dgAttendance.Columns[0].Visible = false;
                }
                else
                {
                    dgAttendance.Columns[1].Visible = false;
                }

                //
                // Fill in the filter options.
                //
                OccurrenceTypeGroupCollection otgc = new OccurrenceTypeGroupCollection(CurrentOrganization.OrganizationID);
                ddlFilterAttendanceTypeGroup.Items.Add(new ListItem("Show All", "-1"));
                foreach (OccurrenceTypeGroup otg in otgc)
                {
                    ddlFilterAttendanceTypeGroup.Items.Add(new ListItem(otg.GroupName, otg.GroupId.ToString()));
                }
                ddlFilterAttendanceTypeGroup.SelectedIndex = 0;
                ddlFilterAttendanceTypeGroup_Changed(null, null);

                //
                // Do initial sorting.
                //
            }
		}

        void dgAttendance_ReBind(object sender, EventArgs e)
        {
            dgAttendance.DataSource = GetOccurrenceData();
            dgAttendance.DataBind();
        }

        public void btnFilterApply_Click(object sender, EventArgs e)
        {
            hfFilterAttendanceTypeGroupID.Value = ddlFilterAttendanceTypeGroup.SelectedValue;
            hfFilterAttendanceTypeID.Value = ddlFilterAttendanceType.SelectedValue;
            hfFilterOccurrenceID.Value = ddlFilterOccurrence.SelectedValue;
            hfFilterLocationID.Value = ddlFilterLocation.SelectedValue;

            dgAttendance_ReBind(null, null);
        }

        public void ddlFilterAttendanceTypeGroup_Changed(object sender, EventArgs e)
        {
            OccurrenceTypeCollection otc;

            ddlFilterAttendanceType.Items.Clear();
            ddlFilterAttendanceType.Items.Add(new ListItem("Show All", "-1"));
            ddlFilterAttendanceType.SelectedIndex = 0;
            if (ddlFilterAttendanceTypeGroup.SelectedValue != "-1")
            {
                otc = new OccurrenceTypeCollection(Convert.ToInt32(ddlFilterAttendanceTypeGroup.SelectedValue));

                foreach (OccurrenceType ot in otc)
                {
                    ddlFilterAttendanceType.Items.Add(new ListItem(ot.TypeName, ot.OccurrenceTypeId.ToString()));
                }

                ddlFilterAttendanceType.Enabled = true;
            }
            else
            {
                ddlFilterAttendanceType.Enabled = false;
            }

            ddlFilterAttendanceType_Changed(null, null);
        }

        public void ddlFilterAttendanceType_Changed(object sender, EventArgs e)
        {
            OccurrenceCollection oc;
            LocationCollection lc = new LocationCollection(CurrentOrganization.OrganizationID);


            //
            // Setup the basic occurrence list.
            //
            ddlFilterOccurrence.Items.Clear();
            ddlFilterOccurrence.Items.Add(new ListItem("Show Active", "-1"));
            ddlFilterOccurrence.Items.Add(new ListItem("Show All", "-2"));
            ddlFilterOccurrence.SelectedIndex = 0;

            //
            // Setup the basic room list.
            //
            ddlFilterLocation.Items.Clear();
            ddlFilterLocation.Items.Add(new ListItem("Show All", "-1"));
            ddlFilterLocation.SelectedIndex = 0;

            //
            // Add in all valid choices.
            //
            if (ddlFilterAttendanceType.SelectedValue != "-1")
            {
                oc = new OccurrenceCollection(Convert.ToInt32(ddlFilterAttendanceType.SelectedValue));

                foreach (Occurrence o in oc)
                {
                    ddlFilterOccurrence.Items.Add(new ListItem(String.Format("{0} ({1})", o.Name, o.StartTime.ToShortDateTimeString()), o.OccurrenceID.ToString()));
                }

                lc = lc.FilterByOccurrenceType(Convert.ToInt32(ddlFilterAttendanceType.SelectedValue));
                ddlFilterOccurrence.Enabled = true;
            }
            else
            {
                ddlFilterOccurrence.Enabled = false;
            }

            //
            // Add all valid locations.
            //
            foreach (Location l in lc)
            {
                ddlFilterLocation.Items.Add(new ListItem(String.Format("{0} - {1}", l.BuildingName, l.LocationName), l.LocationId.ToString()));
            }

            dgAttendance_ReBind(null, null);
        }

        #endregion

        private DataTable GetOccurrenceData()
        {
            // open the data reader
            StringBuilder command = new StringBuilder("SELECT coa.occurrence_attendance_id,coa.occurrence_id,coa.person_id,coa.security_code,coa.attended,coa.check_in_time,coa.check_out_time" +
                            ",cp.last_name + ', ' + cp.first_name AS 'common_name',cp.first_name,cp.last_name,cp.nick_name,cp.gender,cp.birth_date,cp.graduation_date,cp.guid" +
                            ",co.occurrence_name AS 'common_occurrence_name'" +
                            ",cotg.group_name + ' - ' + cot.type_name AS 'common_attendance_name'" +
                            ",ol.building_name + ' - ' + ol.location_name AS 'common_location_name',ol.building_name,ol.location_name" +
                            " FROM core_occurrence_attendance AS coa" +
                            " LEFT JOIN core_person AS cp ON cp.person_id = coa.person_id" +
                            " LEFT JOIN core_occurrence AS co ON coa.occurrence_id = co.occurrence_id" +
                            " LEFT JOIN core_occurrence_type AS cot ON cot.occurrence_type_id = co.occurrence_type" +
                            " LEFT JOIN core_occurrence_type_group AS cotg ON cotg.group_id = cot.group_id" +
                            " LEFT JOIN orgn_location AS ol ON ol.location_id = co.location_id" +
                            " WHERE co.location_id IS NOT NULL");

            //
            // Do the basic occurrence filtering.
            //
            if (hfFilterOccurrenceID.Value == "-1")
            {
                command.Append(" AND coa.occurrence_id IN (SELECT occurrence_id FROM core_occurrence WHERE " +
                                "(occurrence_start_time IS NOT NULL AND occurrence_end_time IS NOT NULL AND occurrence_start_time <= GetDate() AND occurrence_end_time >= GetDate())" +
                                "OR (occurrence_start_time IS NOT NULL AND check_in_end IS NOT NULL AND occurrence_start_time <= GetDate() AND check_in_end >= GetDate())" +
                                "OR (check_in_start IS NOT NULL AND occurrence_end_time IS NOT NULL AND check_in_start <= GetDate() AND occurrence_end_time >= GetDate())" +
                                "OR (check_in_start IS NOT NULL AND check_in_end IS NOT NULL AND check_in_start <= GetDate() AND check_in_end >= GetDate())" +
                                ")");
            }
            else if (Convert.ToInt32(hfFilterOccurrenceID.Value) >= 0)
            {
                command.AppendFormat(" AND coa.occurrence_id = {0}", hfFilterOccurrenceID.Value);
            }

            //
            // Add a filter on attendance type.
            //
            if (hfFilterAttendanceTypeGroupID.Value != "-1")
            {
                if (hfFilterAttendanceTypeID.Value != "-1")
                {
                    command.AppendFormat(" AND cot.occurrence_type_id = {0}", hfFilterAttendanceTypeID.Value);
                }
                else
                {
                    command.AppendFormat(" AND cotg.group_id = {0}", hfFilterAttendanceTypeGroupID.Value);
                }
            }

            //
            // Add a filter on location.
            //
            if (hfFilterLocationID.Value != "-1")
            {
                command.AppendFormat(" AND ol.location_id = {0}", hfFilterLocationID.Value);
            }

            //
            // By default, sort by common_name(last, first).
            //
            command.Append(" ORDER BY common_name");

            SqlDataReader reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(command.ToString());

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


        public String GraduationDateToGradeString(Object graduationYear)
        {
            DateTime date = DateTime.Parse(graduationYear.ToString());
            int grade = Person.CalculateGradeLevel(date, CurrentOrganization.GradePromotionDate);

            if (grade != -1)
                return Person.GetGradeName(grade);

            return "";
        }


        public String BirthDateToAgeString(Object birth_date)
        {
            try
            {
                DateTime today = DateTime.Now, birthDate = DateTime.Parse(birth_date.ToString());
                
                int Years = 0;                                                          
                int Months = 0;
                int Days = 0;

                if (birthDate.Year == 1900 || birthDate.Year == 1901)
                    return String.Empty;

                Days = today.Day - birthDate.Day;
                if (Days < 0)
                {                   
                    Months -= 1;
                    DateTime lm = today.AddMonths(-1);
                    Days += DateTime.DaysInMonth(lm.Year, lm.Month);
                }
                Months += today.Month - birthDate.Month;
                if (Months < 0)
                {
                    Years -= 1;
                    Months += 12;
                }
                Years += today.Year - birthDate.Year;

                if (Years == 0 && Months == 0)
                    return String.Format("{0} days", Days);
                else if (Years == 0)
                    return String.Format("{0} months", Months);
                else
                    return String.Format("{0}", Years);
            }
            catch
            {
                return string.Empty;
            }
        }

        public void btnRePrint_Click(object sender, CommandEventArgs e)
        {
            OccurrenceAttendance oa = new OccurrenceAttendance(Int32.Parse(e.CommandArgument.ToString()));
            FamilyMember member = new FamilyMember(oa.PersonID);
            Occurrence occurrence = oa.Occurrence;
            List<Occurrence> list = new List<Occurrence>(1);
            Boolean status;

            list.Add(occurrence);
            status = Controller.Print(member.Family(), member, list, oa, Controller.GetCurrentKiosk(Request.ServerVariables["REMOTE_ADDR"]));
            ((LinkButton)sender).Text = String.Format("Re-Print Labels ({0})", (status ? "OK" : "Error"));
        }
    }
}