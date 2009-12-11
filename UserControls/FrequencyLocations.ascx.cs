/**********************************************************************
* Description:	TYPE THE DESCRIPTION OF YOUR MODULE HERE
* Created By:	
* Date Created:	10/21/2009 12:23:14 PM
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

	public partial class FrequencyLocations : PortalControl
	{

		protected int EditTemplateID
		{
			get { return (int)ViewState["EditTemplateID"]; }
			set { ViewState["EditTemplateID"] = value; }
		}

		protected String SelectedTemplateLocations
		{
			get { return (String)ViewState["SelectedTemplateLocations"]; }
			set { ViewState["SelectedTemplateLocations"] = value; }
		}

		#region Event Handlers

		protected void Page_Init(object sender, EventArgs e)
		{
			dgTemplateLocations.ReBind += new Arena.Portal.UI.DataGridReBindEventHandler(dgTemplateLocations_ReBind);
		}

		private void Page_Load(object sender, System.EventArgs e)
		{
			if ( ! IsPostBack )
			{
				//
				// Setup the overview grid.
				//
				dgTemplateLocations_ReBind(null, null);
			}
		}
		
		#endregion

		void dgTemplateLocations_ReBind(object sender, EventArgs e)
		{
			dgTemplateLocations.EditItemIndex = -1;
			dgTemplateLocations.SelectedIndex = -1;
			dgTemplateLocations.AddEnabled = false;
			dgTemplateLocations.AllowSorting = false;
			dgTemplateLocations.DeleteEnabled = false;
			dgTemplateLocations.EditEnabled = true;
			dgTemplateLocations.ExportEnabled = true;
			dgTemplateLocations.ItemAltBgColor = CurrentPortalPage.Setting("ItemAltBgColor", string.Empty, false);
			dgTemplateLocations.ItemBgColor = CurrentPortalPage.Setting("ItemBgColor", string.Empty, false);
			dgTemplateLocations.ItemMouseOverColor = CurrentPortalPage.Setting("ItemMouseOverColor", string.Empty, false);
			dgTemplateLocations.ItemType = "Attendance Frequency";
			dgTemplateLocations.MailEnabled = false;
			dgTemplateLocations.MergeEnabled = false;
			dgTemplateLocations.MoveEnabled = false;

			dgTemplateLocations.DataSource = GetOverviewData();
			dgTemplateLocations.DataBind();
		}

		public void dgTemplateLocations_EditCommand(object sender, DataGridCommandEventArgs e)
		{
			EditTemplateID = Convert.ToInt32(e.Item.Cells[0].Text);
			SelectedTemplateLocations = e.Item.Cells[1].Text;
			lbEditTemplate.Text = String.Format("Select the rooms to be opened for the {0} template frequency starting at {1} (Check-In starts at {2}):",
												e.Item.Cells[2].Text, ((Label)e.Item.Cells[5].Controls[1]).Text,
												((Label)e.Item.Cells[6].Controls[1]).Text);

			ShowEdit();
		}

		private void ShowList()
		{
			pnlOverview.Visible = true;
			pnlEdit.Visible = false;

			dgTemplateLocations_ReBind(null, null);
		}

		private void ShowEdit()
		{
			ArrayList paramList = new ArrayList();
			string[] selectedLocations = SelectedTemplateLocations.Split(',');

			pnlOverview.Visible = false;
			pnlEdit.Visible = true;

			//
			// Open the data reader
			//
			paramList.Add(new SqlParameter("OccurrenceTypeID", Convert.ToInt32(Page.Request.Params["Type"])));
			SqlDataReader reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(
						"orgn_sp_get_locationByOccurrenceTypeID", paramList);
			cblLocations.Items.Clear();

			while (reader.Read())
			{
				String title, value;
				bool enabled;

				title = String.Format("{0} - {1}", reader["building_name"], reader["location_name"]);
				value = reader["location_id"].ToString();
				enabled = (selectedLocations.Contains(value) || selectedLocations.Length == 0);
				cblLocations.Items.Add(new ListItem(title, value));
				cblLocations.Items[cblLocations.Items.Count - 1].Selected = enabled;
			}
		}

		public void btnEditSave_Click(object sender, EventArgs e)
		{
			ArrayList list = new ArrayList(), paramList;

			foreach (ListItem item in cblLocations.Items)
			{
				if (item.Selected)
					list.Add(item.Value);
			}

			//
			// Delete the old records.
			//
			paramList = new ArrayList();
			paramList.Add(new SqlParameter("TemplateID", EditTemplateID));
			new Arena.DataLayer.Organization.OrganizationData().ExecuteNonQuery(
				"cust_hdc_checkin_sp_delete_locationsByTemplateID", paramList);

			foreach (string location_id in list)
			{
				//
				// Create the template location link if the location has been
				// selected.
				//
				paramList = new ArrayList();
				paramList.Add(new SqlParameter("OccurrenceTypeTemplateId", EditTemplateID));
				paramList.Add(new SqlParameter("LocationId", Convert.ToInt32(location_id)));
				new Arena.DataLayer.Organization.OrganizationData().ExecuteNonQuery(
					"cust_hdc_checkin_sp_save_template_location", paramList);
			}

			ShowList();
		}

		public void btnEditCancel_Click(object sender, EventArgs e)
		{
			ShowList();
		}

		private DataTable GetOverviewData()
		{
			Hashtable table;
			StringBuilder sb;
			Dictionary<String, Hashtable> overview = new Dictionary<String, Hashtable>();
			ArrayList paramList = new ArrayList(), ids;

			//
			// Open the data reader
			//
			paramList.Add(new SqlParameter("TypeID", Convert.ToInt32(Page.Request.Params["Type"])));
			SqlDataReader reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(
						"cust_hdc_checkin_sp_templateLocationsByTypeID", paramList);

			while (reader.Read())
			{
				if (overview.ContainsKey(reader["occurrence_type_template_id"].ToString()) == false)
				{
					table = new Hashtable();
					overview.Add(reader["occurrence_type_template_id"].ToString(), table);
					table["template_id"] = reader["occurrence_type_template_id"];
					table["location_names"] = new StringBuilder();
					table["location_ids"] = new ArrayList();
					table["occurrence_freq_type"] = reader["occurrence_freq_type"];
					table["freq_qualifier"] = reader["freq_qualifier"];
					table["start_time"] = reader["start_time"];
					table["check_in_start"] = reader["check_in_start"];
					table["schedule_name"] = reader["schedule_name"];
				}
				else
				{
					table = overview[reader["occurrence_type_template_id"].ToString()];
				}

				sb = (StringBuilder)table["location_names"];
				ids = (ArrayList)table["location_ids"];

				if (sb.Length == 0)
				{
					sb.Append(reader["location_name"]);
				}
				else
				{
					sb.AppendFormat(",{0}", reader["location_name"]);
				}

				ids.Add(reader["location_id"].ToString());
			}

			DataTable dt = new DataTable("hdc_customTable");
			dt.Columns.Add("template_id");
			dt.Columns.Add("location_names");
			dt.Columns.Add("location_ids");
			dt.Columns.Add("occurrence_freq_type");
			dt.Columns.Add("occurrence_freq_type_name");
			dt.Columns.Add("freq_qualifier");
			dt.Columns.Add("freq_qualifier_name");
			dt.Columns.Add("start_time");
			dt.Columns.Add("check_in_start");
			dt.Columns.Add("schedule_name");
			foreach (KeyValuePair<String, Hashtable> kvp in overview)
			{
				DataRow row = dt.NewRow();
				table = (Hashtable)kvp.Value;

				row["template_id"] = table["template_id"];
				row["location_names"] = table["location_names"].ToString();
				row["location_ids"] = String.Join(",", (string[])((ArrayList)table["location_ids"]).ToArray(typeof(string)));
				row["occurrence_freq_type"] = table["occurrence_freq_type"];
				row["occurrence_freq_type_name"] = "";
				row["freq_qualifier"] = table["freq_qualifier"];
				row["freq_qualifier_name"] = "";
				row["start_time"] = table["start_time"];
				row["check_in_start"] = table["check_in_start"];
				row["schedule_name"] = table["schedule_name"];

				if (row["occurrence_freq_type"].ToString() == "1")
				{
					row["occurrence_freq_type_name"] = "Weekly";

					if (row["freq_qualifier"].ToString() == "0")
						row["freq_qualifier_name"] = "Sunday";
					else if (row["freq_qualifier"].ToString() == "1")
						row["freq_qualifier_name"] = "Monday";
					else if (row["freq_qualifier"].ToString() == "2")
						row["freq_qualifier_name"] = "Tuesday";
					else if (row["freq_qualifier"].ToString() == "3")
						row["freq_qualifier_name"] = "Wednesday";
					else if (row["freq_qualifier"].ToString() == "4")
						row["freq_qualifier_name"] = "Thursday";
					else if (row["freq_qualifier"].ToString() == "5")
						row["freq_qualifier_name"] = "Friday";
					else if (row["freq_qualifier"].ToString() == "6")
						row["freq_qualifier_name"] = "Saturday";
				}

				dt.Rows.Add(row);
			}

			return dt;
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