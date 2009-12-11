/**********************************************************************
* Description:	This module provides an interface to post numbers to the
*				display board system. 
* Created By:	Daniel Hazelbaker
* Date Created:	11/18/2009 1:33:49 PM
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
	using System.Xml;

	using Arena.Portal;
	using Arena.Core;

	public partial class CheckInNumberBoard : PortalControl
	{
		#region Module Settings

		[NumericSetting("DisplayGroupID", "Display group ID to use when posting new numbers to the system.", true)]
		public int DisplayGroupID { get { return Convert.ToInt32(Setting("DisplayGroupID", "0", true)); } }

		#endregion

		#region Event Handlers

		private void Page_Load(object sender, System.EventArgs e)
		{
			if ( ! IsPostBack )
			{
				//
				// Setup the data grid.
				//
				dgNote.ItemType = "Note";
				dgNote.ItemBgColor = CurrentPortalPage.Setting("ItemBgColor", string.Empty, false);
				dgNote.ItemAltBgColor = CurrentPortalPage.Setting("ItemAltBgColor", string.Empty, false);
				dgNote.ItemMouseOverColor = CurrentPortalPage.Setting("ItemMouseOverColor", string.Empty, false);
				dgNote.MoveEnabled = false;
				dgNote.DeleteEnabled = true;
				dgNote.EditEnabled = true;
				dgNote.AllowSorting = true;

				dgNote_ReBind(null, null);
			}
		}
		
		public void dgNote_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.EditItem)
			{
				DataRowView drv = (DataRowView)e.Item.DataItem;
				Hashtable hash = new Hashtable();
				CheckBox cb;
				Image img;

				hash["note_id"] = drv["note_id"];
				hash["first_name"] = drv["first_name"];
				hash["number_string"] = drv["number_string"];
				hash["system_id"] = drv["system_id"];
				hash["display_group_id"] = drv["display_group_id"];
				hash["user_info"] = drv["user_info"];
				ViewState["editData"] = hash;
				img = (Image)e.Item.Controls[1].Controls[1];
				cb = (CheckBox)e.Item.Controls[1].Controls[3];

				img.Style["display"] = "none";
				cb.Checked = Convert.ToBoolean(drv["show_name"]);
				cb.Style["display"] = "";
				if (drv["first_name"] == null || drv["first_name"].ToString().Length == 0)
					cb.Enabled = false;
			}
			else if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				DataRowView drv = (DataRowView)e.Item.DataItem;
				CheckBox cb;
				Image img;
				Label lb;

				lb = (Label)e.Item.Controls[0].Controls[1];
				img = (Image)e.Item.Controls[1].Controls[1];
				if (Convert.ToBoolean(drv["show_name"]) == true)
				{
					img.ImageUrl = GetBaseURL() + "/images/check.gif";
					img.Style["display"] = "";
				}
				else
				{
					img.Style["display"] = "none";
				}

				cb = (CheckBox)e.Item.Controls[1].Controls[3];
				cb.Checked = Convert.ToBoolean(drv["show_name"]);
				cb.Style["display"] = "none";
			}
		}

		public void dgNote_Update(object sender, DataGridCommandEventArgs e)
		{
			StringBuilder html;
			SqlParameter output;
			Hashtable hash = (Hashtable)ViewState["editData"];
			ArrayList paramList = new ArrayList();
			CheckBox cb;


			cb = (CheckBox)e.Item.Controls[1].Controls[3];

			html = new StringBuilder(String.Format("<p id=\"SecurityCode\">{0}</p>", hash["number_string"]));
			if (cb.Checked)
			    html.AppendFormat("<p id=\"Name\">{0}</p>", hash["first_name"]);

			paramList.Add(new SqlParameter("@NoteId", hash["note_id"]));
			paramList.Add(new SqlParameter("@NoteHtml", html.ToString()));
			paramList.Add(new SqlParameter("@NoteImage", DBNull.Value));
			paramList.Add(new SqlParameter("@NoteInfo", DBNull.Value));
			paramList.Add(new SqlParameter("@SystemId", hash["system_id"]));
			paramList.Add(new SqlParameter("@DisplayGroupId", hash["display_group_id"]));
			paramList.Add(new SqlParameter("@UserInfo", hash["user_info"]));
			paramList.Add(new SqlParameter("@UserId", CurrentUser.Identity.Name));
			output = new SqlParameter("@ID", null);
			output.Direction = ParameterDirection.Output;
			output.DbType = DbType.Int32;
			paramList.Add(output);
			new Arena.DataLayer.Organization.OrganizationData().ExecuteNonQuery("cust_hdc_checkin_sp_save_number_board_note", paramList);

			dgNote.EditItemIndex = -1;
			dgNote_ReBind(null, null);
		}

		public void dgNote_Delete(object sender, DataGridCommandEventArgs e)
		{
			DataBoundLiteralControl lb = (DataBoundLiteralControl)e.Item.Cells[0].Controls[3].Controls[0];
			ArrayList paramList = new ArrayList();

			paramList.Add(new SqlParameter("@NoteId", Convert.ToInt32(lb.Text)));
			new Arena.DataLayer.Organization.OrganizationData().ExecuteNonQuery("cust_hdc_checkin_sp_del_number_board_note", paramList);

			dgNote_ReBind(null, null);
		}

		public void dgNote_ReBind(object sender, EventArgs e)
        {
			DataTable dt = new DataTable();
			ArrayList data = new ArrayList();


			dt.Columns.Add("note_id");
			dt.Columns.Add("number_string");
			dt.Columns.Add("created_date");
			dt.Columns.Add("created_string");
			dt.Columns.Add("created_by");
			dt.Columns.Add("first_name");
			dt.Columns.Add("full_name");
			dt.Columns.Add("sort_name");
			dt.Columns.Add("location_name");
			dt.Columns.Add("show_name");
			dt.Columns.Add("system_id");
			dt.Columns.Add("display_group_id");
			dt.Columns.Add("user_info");

			//
			// Open the data reader to pull a list of all notes in the system.
			//
			SqlDataReader reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(
						"SELECT [nbn].note_id,nbn.user_info,nbn.date_created,nbn.created_by,nbn.note_html,nbn.system_id,nbn.display_group_id" +
						"	,cp.person_id,cp.first_name,cp.last_name" +
						"	,ol.location_id,ol.location_name,ol.building_name" +
						"	,coa.security_code" +
						"	FROM cust_hdc_checkin_number_board_note AS nbn" +
						"	LEFT OUTER JOIN core_occurrence_attendance AS coa ON coa.occurrence_attendance_id = nbn.user_info" +
						"	LEFT OUTER JOIN core_person AS cp ON cp.person_id = coa.person_id" +
						"	LEFT OUTER JOIN core_occurrence AS co ON co.occurrence_id = coa.occurrence_id" +
						"	LEFT OUTER JOIN orgn_location AS ol ON ol.location_id = co.location_id" +
						"	ORDER BY note_id");

			while (reader.Read())
			{
				DataRow row = dt.NewRow();

				row["note_id"] = reader["note_id"];
				row["created_date"] = reader["date_created"];
				row["created_string"] = ((DateTime)reader["date_created"]).ToShortDateTimeString();
				row["created_by"] = reader["created_by"];
				row["first_name"] = reader["first_name"];
				row["full_name"] = reader["first_name"].ToString() + " " + reader["last_name"].ToString();
				row["sort_name"] = reader["last_name"].ToString() + ", " + reader["first_name"].ToString();
				row["location_name"] = reader["building_name"].ToString() + " - " + reader["location_name"];
				row["show_name"] = false;
				row["system_id"] = reader["system_id"];
				row["display_group_id"] = reader["display_group_id"];
				row["user_info"] = reader["user_info"];

				//
				// Pull out the number and status of the first name.
				//
				try
				{
					XmlDocument xdoc = new XmlDocument();
					xdoc.LoadXml("<body>" + reader["note_html"].ToString() + "</body>");
					row["number_string"] = xdoc.ChildNodes[0].ChildNodes[0].ChildNodes[0].Value.ToString();
					if (xdoc.ChildNodes[0].ChildNodes.Count == 2)
						row["show_name"] = true;
				}
				catch
				{
				}

				dt.Rows.Add(row);
			}
			reader.Close();

			dgNote.DataSource = dt;
            dgNote.DataBind();
        }

		public void btnAddPost_Click(object sender, EventArgs e)
		{
			StringBuilder html;
			SqlParameter output;
			ArrayList paramList = new ArrayList();


			if (tbAddNumber.Text.Length > 0)
			{
				html = new StringBuilder(String.Format("<p id=\"SecurityCode\">{0}</p>", tbAddNumber.Text));

				paramList.Add(new SqlParameter("@NoteId", -1));
				paramList.Add(new SqlParameter("@NoteHtml", html.ToString()));
				paramList.Add(new SqlParameter("@NoteImage", DBNull.Value));
				paramList.Add(new SqlParameter("@NoteInfo", DBNull.Value));
				paramList.Add(new SqlParameter("@SystemId", DBNull.Value));
				paramList.Add(new SqlParameter("@DisplayGroupId", DisplayGroupID));
				paramList.Add(new SqlParameter("@UserInfo", DBNull.Value));
				paramList.Add(new SqlParameter("@UserId", CurrentUser.Identity.Name));
				output = new SqlParameter("@ID", null);
				output.Direction = ParameterDirection.Output;
				output.DbType = DbType.Int32;
				paramList.Add(output);
				new Arena.DataLayer.Organization.OrganizationData().ExecuteNonQuery("cust_hdc_checkin_sp_save_number_board_note", paramList);
			}

			dgNote_ReBind(null, null);
		}

		#endregion

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
	}
}