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
    using System.Reflection;

	using Arena.Portal;
	using Arena.Core;
    using Arena.Organization;
    using Arena.Computer;
    using Arena.Marketing;

	public partial class AttendanceDetails : PortalControl
	{
        const int kAttendanceAbilityLevelColumn = 5;

        #region Module Settings

        [PageSettingAttribute("Person Detail Page", "Page to use for viewing a person's detail information.", false)]
        public int PersonDetailPageID { get { return Convert.ToInt32(Setting("PersonDetailPageID", "-1", false)); } }

        [ListFromSqlSetting("Ability Level Attribute", "Sets ability level person attribute.", false, "",
            "SELECT [attribute_id], [attribute_name] FROM [core_attribute] WHERE [attribute_type] = 3 AND [attribute_group_id] = 16 ORDER BY [attribute_name]")]
        public string AbilityLevelAttributeIDSetting { get { return Setting("AbilityLevelAttributeID", "", false); } }

        [ListFromSqlSetting("Default Attendance Group", "Defines the default attendance group to use when determining the active service.", false, "",
            "SELECT [group_id],[group_name] FROM [core_occurrence_type_group]")]
        public string DefaultAttendanceGroupIDSetting { get { return Setting("DefaultAttendanceGroupID", "", false); } }

        [NumericSetting("Topic Area", "Enter the topic area ID that will be used for posting numbers to.", false)]
        public int TopicAreaID { get { return Convert.ToInt32(Setting("TopicAreaID", "-1", false)); } }

        [CampusSetting("Campus", "Select the campus to limit this module to, if you do not enter a campus then all campuses will be used.", false)]
        public int CampusID { get { return Convert.ToInt32(Setting("CampusID", "-1", false)); } }

		#endregion

        #region Event Handlers

        protected void Page_Init(object sender, EventArgs e)
        {
            dgAttendance.ReBind += new Arena.Portal.UI.DataGridReBindEventHandler(dgAttendance_ReBind);
			ScriptManager.RegisterClientScriptInclude(Page, Page.GetType(), "jquery.min", "include/scripts/jquery.1.3.2.min.js");
			ScriptManager.RegisterClientScriptInclude(Page, Page.GetType(), "jquery.hoverIntent", "include/scripts/jquery.hoverIntent.min.js");
			ScriptManager.RegisterClientScriptInclude(Page, Page.GetType(), "hdcHoverPopup", AppRelativeTemplateSourceDirectory.Remove(0, 2) + "Scripts/hdcHoverPopup.js");
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
                dgAttendance.Columns[5].Visible = (AbilityLevelAttributeIDSetting != "");

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
                foreach (OccurrenceTypeGroup otg in otgc)
                {
                    ddlFilterTypeGroup.Items.Add(new ListItem(otg.GroupName, otg.GroupId.ToString()));
                }
                OccurrenceCollection oc = new OccurrenceCollection(DateTime.Now, DateTime.Now);
                if (this.Request.Params["groupID"] != null)
                {
                    ddlFilterTypeGroup.SelectedValue = this.Request.Params["groupID"];
                }
                else if (DefaultAttendanceGroupIDSetting != "")
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
            }
		}

        void dgAttendance_ReBind(object sender, EventArgs e)
        {
            dgAttendance.DataSource = GetOccurrenceData();
            dgAttendance.DataBind();
        }

		public void dgAttendance_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				if (dgAttendance.EditItemIndex == -1)
				{
					ClientScriptManager csm = Page.ClientScript;
					DataRowView drv = (DataRowView)e.Item.DataItem;
					Control holder = (PersonDetailPageID == -1 ? e.Item.Controls[1] : e.Item.Controls[0]);
					Control hover = holder.Controls[1];
					LinkButton lbReprint = (LinkButton)holder.Controls[3];
					LinkButton lbPost = (LinkButton)holder.Controls[5];
					String html = "", securityNumber;
                    Boolean hasCCCEV = false;


                    try
                    {
                        String arenaBin;
                        Assembly asm;

                        //
                        // Find the location of the Arena bin directory and then try to load
                        // the Cccev assembly file.
                        //
                        arenaBin = new Organization().GetType().Assembly.CodeBase.Split(new string[] { "/Arena." }, StringSplitOptions.None)[0];
                        asm = System.Reflection.Assembly.LoadFrom(arenaBin + "/Arena.Custom.Cccev.CheckIn.dll");
                        if (asm != null)
                            hasCCCEV = true;
                    }
                    catch { }

                    //
                    // Add the link to the person.
                    //
					if (PersonDetailPageID != -1)
						((LinkButton)hover).Attributes["href"] = "default.aspx?page=" + PersonDetailPageID.ToString() + "&guid=" + drv["guid"];

                    //
                    // If we have the CCCEV check-in library, provide a re-print button.
                    //
                    if (hasCCCEV)
                    {
                        html += "<span class=\"smallText\"><a href=\"" + csm.GetPostBackClientHyperlink(lbReprint, null) + "\">Reprint</a> labels for " + drv["first_name"] + "<br /></span>";
                        html += "<br />";
                    }

                    //
                    // If we have a topic area, provide a post/remove security number.
                    //
					if (TopicAreaID != -1)
					{
						//
						// Check if the security code is already posted.
						//
						securityNumber = drv["security_code"].ToString().Substring(2);
						if (FindPromotionRequest(Convert.ToInt32(drv["occurrence_attendance_id"])) != -1)
							html = html + "<span class=\"smallText\">Remove security number <a href=\"" + csm.GetPostBackClientHyperlink(lbPost, null) + "\">" + securityNumber + "</a><br /></span>";
						else
							html = html + "<span class=\"smallText\">Post security number <a href=\"" + csm.GetPostBackClientHyperlink(lbPost, null) + "\">" + securityNumber + "</a><br /></span>";
					}

					html = html.Replace("'", "\\'");
					ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "hover" + hover.ClientID, "$(document).ready(function() {popupOverObject('" + hover.ClientID + "', '" + html + "');});", true);
				}
			}
		}

        public void btnFilterApply_Click(object sender, EventArgs e)
        {
            hfFilterTypeGroupID.Value = ddlFilterTypeGroup.SelectedValue;
            hfFilterService.Value = ddlFilterService.SelectedValue;
            hfFilterName.Value = tbFilterName.Text;
            hfFilterTypeID.Value = ddlFilterType.SelectedValue;
            hfFilterOccurrenceID.Value = ddlFilterOccurrence.SelectedValue;
            hfFilterLocationID.Value = ddlFilterLocation.SelectedValue;

            dgAttendance_ReBind(null, null);
        }

        public void ddlFilterTypeGroup_Changed(object sender, EventArgs e)
        {
            OccurrenceTypeCollection otc;
            OccurrenceCollection oc;
            DateTime bestDate = DateTime.MinValue, now = DateTime.Now;
            int matchIndex = 0;


            //
            // Setup the basic occurrence list.
            //
            ddlFilterService.Items.Clear();

            //
            // Add in all valid choices.
            //
            if (ddlFilterTypeGroup.SelectedValue != "-1")
            {
                otc = new OccurrenceTypeCollection(Convert.ToInt32(ddlFilterTypeGroup.SelectedValue));
                ArrayList dates = new ArrayList();

                foreach (OccurrenceType ot in otc)
                {
                    oc = new OccurrenceCollection(ot.OccurrenceTypeId);
                    foreach (Occurrence o in oc)
                    {
                        if ((o.StartTime <= now && o.EndTime >= now) ||
                            (o.CheckInStart <= now && o.CheckInEnd >= now))
                        {
                            bestDate = o.StartTime;
                        }

                        if (dates.Contains(o.StartTime) == false)
                            dates.Add(o.StartTime);
                    }
                }

                dates.Sort();
                dates.Reverse();

                foreach (DateTime dt in dates)
                {
                    ddlFilterService.Items.Add(new ListItem(dt.ToString("ddd M/dd/yy h:mm tt"), dt.ToShortDateTimeString()));
                    if (!IsPostBack && this.Request.Params["service"] != null)
                    {
                        if (dt.Equals(DateTime.Parse(this.Request.Params["service"])) == true)
                            matchIndex = (ddlFilterService.Items.Count - 1);
                    }
                    else if (dt.Equals(bestDate) == true)
                        matchIndex = (ddlFilterService.Items.Count - 1);
                }

                ddlFilterService.Enabled = true;
            }
            else
            {
                ddlFilterService.Enabled = false;
            }

            if (ddlFilterService.Items.Count > 0)
                ddlFilterService.SelectedIndex = matchIndex;

            ddlFilterService_Changed(null, null);
        }

        public void ddlFilterService_Changed(object sender, EventArgs e)
        {
            OccurrenceTypeCollection otc = new OccurrenceTypeCollection(Convert.ToInt32(ddlFilterTypeGroup.SelectedValue));
            OccurrenceCollection oc;
            ArrayList locations = new ArrayList(), occurrences = new ArrayList(), types = new ArrayList();
            DateTime selectedServiceTime = DateTime.Parse(ddlFilterService.SelectedValue);
            int idNumber;
            bool searchAvailable = true;


            foreach (OccurrenceType ot in otc)
            {
                oc = new OccurrenceCollection(ot.OccurrenceTypeId);
                foreach (Occurrence o in oc)
                {
                    if (o.StartTime.Equals(selectedServiceTime) == true)
                    {
                        occurrences.Add(o);
                        if (locations.Contains(o.LocationID) == false)
                            locations.Add(o.LocationID);
                        if (types.Contains(o.OccurrenceTypeID) == false)
                            types.Add(o.OccurrenceTypeID);
                    }
                }
            }

            //
            // Setup the basic attendance type list.
            //
            ddlFilterType.Items.Clear();
            ddlFilterType.Items.Add(new ListItem("Show All", "-1"));
            ddlFilterType.SelectedIndex = 0;
            ddlFilterType.Enabled = true;

            //
            // Setup the basic occurrence list.
            //
            ddlFilterOccurrence.Items.Clear();
            ddlFilterOccurrence.Items.Add(new ListItem("Show All", "-1"));
            ddlFilterOccurrence.SelectedIndex = 0;
            ddlFilterOccurrence.Enabled = true;

            //
            // Setup the basic room list.
            //
            ddlFilterLocation.Items.Clear();
            ddlFilterLocation.Items.Add(new ListItem("Show All", "-1"));
            ddlFilterLocation.SelectedIndex = 0;
            ddlFilterLocation.Enabled = true;

            //
            // Add in all valid types.
            //
            foreach (int typeID in types)
            {
                OccurrenceType ot = new OccurrenceType(typeID);
                ddlFilterType.Items.Add(new ListItem(ot.TypeName, ot.OccurrenceTypeId.ToString()));

                if (!IsPostBack && this.Request.Params["typeID"] != null)
                {
                    if (typeID == Convert.ToInt32(this.Request.Params["typeID"]))
                    {
                        ddlFilterType.SelectedIndex = (ddlFilterType.Items.Count - 1);
                        ddlFilterType_Changed(null, null);
                        searchAvailable = false;
                    }
                }
            }

            //
            // Add in all valid choices.
            //
            foreach (Occurrence o in occurrences)
            {
                OccurrenceType ot = new OccurrenceType(o.OccurrenceTypeID);
                Location l = new Location(o.LocationID);
                ddlFilterOccurrence.Items.Add(new ListItem(String.Format("{0} / {1} / {2}", o.Name, ot.TypeName, l.LocationName), o.OccurrenceID.ToString()));

                if (searchAvailable && !IsPostBack && Request.Params["occurrenceID"] != null)
                {
                    idNumber = Convert.ToInt32(Request.Params["occurrenceID"]);
                    if (idNumber == o.OccurrenceID)
                    {
                        ddlFilterOccurrence.SelectedIndex = (ddlFilterOccurrence.Items.Count - 1);
                        ddlFilterOccurrence_Changed(null, null);
                        searchAvailable = false;
                    }
                }
            }

            //
            // Add all valid locations.
            //
            foreach (int locationID in locations)
            {
                Location l = new Location(locationID);
                ddlFilterLocation.Items.Add(new ListItem(String.Format("{0} - {1}", l.BuildingName, l.LocationName), l.LocationId.ToString()));
                if (searchAvailable && !IsPostBack && Request.Params["locationID"] != null)
                {
                    idNumber = Convert.ToInt32(Request.Params["locationID"]);
                    if (locationID == idNumber)
                    {
                        ddlFilterLocation.SelectedIndex = locations.IndexOf(idNumber);
                        ddlFilterLocation_Changed(null, null);
                        searchAvailable = false;
                    }
                }
            }

            dgAttendance_ReBind(null, null);
        }

        public void ddlFilterType_Changed(object sender, EventArgs e)
        {
            if (ddlFilterType.SelectedIndex == 0)
            {
                ddlFilterOccurrence.Enabled = true;
                ddlFilterLocation.Enabled = true;
            }
            else
            {
                ddlFilterOccurrence.Enabled = false;
                ddlFilterLocation.Enabled = false;
            }

            dgAttendance_ReBind(null, null);
        }

        public void ddlFilterOccurrence_Changed(object sender, EventArgs e)
        {
            if (ddlFilterOccurrence.SelectedIndex == 0)
            {
                ddlFilterType.Enabled = true;
                ddlFilterLocation.Enabled = true;
            }
            else
            {
                ddlFilterType.Enabled = false;
                ddlFilterLocation.Enabled = false;
            }

            dgAttendance_ReBind(null, null);
        }

        public void ddlFilterLocation_Changed(object sender, EventArgs e)
        {
            if (ddlFilterLocation.SelectedIndex == 0)
            {
                ddlFilterOccurrence.Enabled = true;
                ddlFilterType.Enabled = true;
            }
            else
            {
                ddlFilterOccurrence.Enabled = false;
                ddlFilterType.Enabled = false;
            }

            dgAttendance_ReBind(null, null);
        }

        #endregion

        private DataTable GetOccurrenceData()
        {
            //
            // Open the data reader
            //
            StringBuilder command = new StringBuilder("SELECT coa.occurrence_attendance_id,coa.occurrence_id,coa.person_id,coa.security_code,coa.attended,coa.check_in_time,coa.check_out_time" +
                            ",cp.last_name + ', ' + cp.first_name AS 'common_name',cp.first_name,cp.last_name,cp.nick_name,cp.gender,cp.birth_date,cp.graduation_date,cp.guid" +
                            ",co.occurrence_name AS 'common_occurrence_name'" +
                            ",cotg.group_name + ' - ' + cot.type_name AS 'common_attendance_name'" +
                            ",ol.building_name + ' - ' + ol.location_name AS 'common_location_name',ol.building_name,ol.location_name");

            if (AbilityLevelAttributeIDSetting != "")
            {
                command.AppendFormat(",(SELECT cl_al.lookup_value" +
                    "   FROM core_person_attribute AS cpa_al" +
                    "	LEFT JOIN core_lookup AS cl_al ON cl_al.lookup_id = cpa_al.int_value" +
                    "   WHERE cpa_al.person_id = cp.person_id AND cpa_al.attribute_id = '{0}') AS 'ability_level'",
                    AbilityLevelAttributeIDSetting);
            }
            else
            {
                command.Append(",'' AS 'ability_level'");
            }

            command.AppendFormat(" FROM core_occurrence_attendance AS coa" +
                            " LEFT JOIN core_person AS cp ON cp.person_id = coa.person_id" +
                            " LEFT JOIN core_occurrence AS co ON coa.occurrence_id = co.occurrence_id" +
                            " LEFT JOIN core_occurrence_type AS cot ON cot.occurrence_type_id = co.occurrence_type" +
                            " LEFT JOIN core_occurrence_type_group AS cotg ON cotg.group_id = cot.group_id" +
                            " LEFT JOIN orgn_location AS ol ON ol.location_id = co.location_id" +
                            " WHERE co.location_id IS NOT NULL" +
                            " AND co.occurrence_start_time = '{0}'", (hfFilterService.Value != "" ? DateTime.Parse(hfFilterService.Value) : DateTime.Parse("01-01-1900 00:00:00")));

            //
            // Do the basic occurrence filtering.
            //
            if (hfFilterOccurrenceID.Value != "-1")
            {
                command.AppendFormat(" AND coa.occurrence_id = {0}", hfFilterOccurrenceID.Value);
            }

            //
            // Add a filter on name.
            //
            if (hfFilterName.Value != "")
            {
                if (hfFilterName.Value.IndexOf(' ') == -1)
                    command.AppendFormat(" AND (cp.first_name LIKE '%{0}%' OR cp.last_name LIKE '%{0}%')", hfFilterName.Value);
                else
                {
                    String[] names = hfFilterName.Value.Split(new char[1] { ' ' }, 2);
                    command.AppendFormat(" AND (cp.first_name LIKE '%{0}%' OR cp.last_name LIKE '%{1}%')", names[0], names[1]);
                }
            }

            //
            // Add a filter on attendance type.
            //
            if (hfFilterTypeGroupID.Value != "-1")
            {
                if (hfFilterTypeID.Value != "-1")
                {
                    command.AppendFormat(" AND cot.occurrence_type_id = {0}", hfFilterTypeID.Value);
                }
                else
                {
                    command.AppendFormat(" AND cotg.group_id = {0}", hfFilterTypeGroupID.Value);
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

            //
            // Return the grade name if available.
            //
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
            OccurrenceAttendance oa;
            FamilyMember member;
            Occurrence occurrence;
            List<Occurrence> list;
            Boolean status = false;
            Assembly asm;
            String arenaBin;
            Type t_controller;
            Type[] paramTypes;
            ComputerSystem cs;


            //
            // Set the basic information we need to print the labels.
            //
            oa = new OccurrenceAttendance(Int32.Parse(e.CommandArgument.ToString()));
            member = new FamilyMember(oa.PersonID);
            occurrence = oa.Occurrence;
            list = new List<Occurrence>(1);
            list.Add(occurrence);

            try
            {
                //
                // Find the location of the Arena bin directory and then try to load
                // the Cccev assembly file.
                //
                arenaBin = new Organization().GetType().Assembly.CodeBase.Split(new string[] { "/Arena." }, StringSplitOptions.None)[0];
                asm = System.Reflection.Assembly.LoadFrom(arenaBin + "/Arena.Custom.Cccev.CheckIn.dll");

                //
                // Get the controller object from the assembly.
                //
                t_controller = asm.GetType("Arena.Custom.Cccev.CheckIn.CheckInController");

                //
                // Load the 2 methods we need from the Cccev assembly.
                //
                paramTypes = new Type[] { typeof(Family), typeof(FamilyMember), typeof(IEnumerable<Occurrence>),
                                    typeof(OccurrenceAttendance), typeof(ComputerSystem) };
                System.Reflection.MethodInfo mi_Print = t_controller.GetMethod("Print", paramTypes);
                paramTypes = new Type[] { typeof(string) };
                System.Reflection.MethodInfo mi_GetCurrentKiosk = t_controller.GetMethod("GetCurrentKiosk", paramTypes);

                //
                // Call the GetCurrentKiosk method to find the
                // ComputerSystem object we need to print with.
                //
                cs = (ComputerSystem)mi_GetCurrentKiosk.Invoke(null, new String[] { Request.ServerVariables["REMOTE_ADDR"] });
                if (cs != null)
                {
                    //
                    // Try to print the person's name tags again.
                    //
                    object[] parameters = new object[] { member.Family(), member, list, oa, cs };
                    status = (Boolean)mi_Print.Invoke(null, parameters);
                }
            }
            catch
            {
                status = false;
            }

			dgAttendance_ReBind(null, null);
		}

		public void btnNumberBoard_Click(object sender, CommandEventArgs e)
		{
			OccurrenceAttendance oa = new OccurrenceAttendance(Convert.ToInt32(e.CommandArgument));
			Arena.DataLayer.Organization.OrganizationData org = new Arena.DataLayer.Organization.OrganizationData();
			String securityNumber = oa.SecurityCode.Substring(2);
            int promotionRequestID;


			//
			// Check if the security code is already posted.
			//
            promotionRequestID = FindPromotionRequest(oa.OccurrenceAttendanceID);
            if (promotionRequestID != -1)
            {
                PromotionRequest promotion = new PromotionRequest(promotionRequestID);

                promotion.Delete();
            }
            else
			{
                PromotionRequest promotion = new PromotionRequest();
				String html;

				//
				// Generate the HTML for this note.
				//
				html = String.Format("<p id=\"SecurityCode\">{0}</p>", securityNumber);

                //
                // Create the new promotion.
                //
                if (CampusID != -1)
                    promotion.Campus = new Campus(CampusID);
                promotion.ContactName = ArenaContext.Current.Person.FullName;
                promotion.ContactEmail = "";
                promotion.ContactPhone = "";
                promotion.Title = oa.OccurrenceAttendanceID.ToString();
                promotion.TopicArea = new Lookup(TopicAreaID);
                promotion.WebSummary = html;
                promotion.WebPromote = true;
                promotion.WebStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                promotion.WebEndDate = promotion.WebStartDate.AddYears(1);
                promotion.WebApprovedBy = ArenaContext.Current.User.Identity.Name;
                promotion.WebApprovedDate = DateTime.Now;
                promotion.Save(ArenaContext.Current.User.Identity.Name);
			}

			dgAttendance_ReBind(null, null);
		}

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

        int FindPromotionRequest(int occurrenceAttendanceID)
        {
            PromotionRequestCollection prc = new PromotionRequestCollection();


            prc.LoadCurrentWebRequests(TopicAreaID.ToString(), "primary", CampusID, 100, false, -1);
            foreach (PromotionRequest promotion in prc)
            {
                try
                {
                    if (Convert.ToInt32(promotion.Title) == occurrenceAttendanceID)
                        return promotion.PromotionRequestID;
                }
                catch { }
            }

            return -1;
        }
	}
}