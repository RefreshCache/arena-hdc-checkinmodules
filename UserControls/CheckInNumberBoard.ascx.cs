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
    using Arena.Marketing;

	public partial class CheckInNumberBoard : PortalControl
	{
		#region Module Settings

        [NumericSetting("Topic Area", "Enter the topic area ID that will be used for managing the display board.", true)]
        public int TopicAreaID { get { return Convert.ToInt32(Setting("TopicAreaID", "-1", true)); } }

        [CampusSetting("Campus", "Select the campus to limit this module to, if you do not enter a campus then all campuses will be used.", false)]
        public int CampusID { get { return Convert.ToInt32(Setting("CampusID", "-1", false)); } }

		#endregion

		#region Event Handlers

		private void Page_Load(object sender, System.EventArgs e)
		{
			if ( ! IsPostBack )
			{
				//
				// Setup the data grid.
				//
				dgNote.ItemType = "Number";
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

				hash["promotion_id"] = drv["promotion_id"];
				hash["first_name"] = drv["first_name"];
				hash["number_string"] = drv["number_string"];
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
            PromotionRequest promotion;
			StringBuilder html;
			Hashtable hash = (Hashtable)ViewState["editData"];
			CheckBox cb;


			cb = (CheckBox)e.Item.Controls[1].Controls[3];

			html = new StringBuilder(String.Format("<p id=\"SecurityCode\">{0}</p>", hash["number_string"]));
			if (cb.Checked)
			    html.AppendFormat("<p id=\"Name\">{0}</p>", hash["first_name"]);

            promotion = new PromotionRequest(Convert.ToInt32(hash["promotion_id"]));
            promotion.WebSummary = html.ToString();
            promotion.Save(ArenaContext.Current.User.Identity.Name);

			dgNote.EditItemIndex = -1;
			dgNote_ReBind(null, null);
		}

		public void dgNote_Delete(object sender, DataGridCommandEventArgs e)
		{
			DataBoundLiteralControl lb = (DataBoundLiteralControl)e.Item.Cells[0].Controls[3].Controls[0];
            PromotionRequest promotion = new PromotionRequest(Convert.ToInt32(lb.Text));

            promotion.Delete();

			dgNote_ReBind(null, null);
		}

		public void dgNote_ReBind(object sender, EventArgs e)
        {
            PromotionRequestCollection prc = new PromotionRequestCollection();
			DataTable dt = new DataTable();
			ArrayList data = new ArrayList();


			dt.Columns.Add("promotion_id");
			dt.Columns.Add("number_string");
			dt.Columns.Add("created_date");
			dt.Columns.Add("created_string");
			dt.Columns.Add("created_by");
			dt.Columns.Add("first_name");
			dt.Columns.Add("full_name");
			dt.Columns.Add("sort_name");
			dt.Columns.Add("location_name");
			dt.Columns.Add("show_name");

            prc.LoadCurrentWebRequests(TopicAreaID.ToString(), "primary", CampusID, 1000, false, -1);
            foreach (PromotionRequest promotion in prc)
			{
                OccurrenceAttendance oa;
				DataRow row = dt.NewRow();

                try
                {
                    oa = new OccurrenceAttendance(Convert.ToInt32(promotion.Title));
    				row["first_name"] = oa.Person.FirstName;
	    			row["full_name"] = oa.Person.FullName;
		    		row["sort_name"] = oa.Person.LastName + ", " + oa.Person.FirstName;
                    row["location_name"] = oa.Occurrence.Location;
                }
                catch
                {
                    row["first_name"] = "";
                    row["full_name"] = "";
                    row["sort_name"] = "";
                    row["location_name"] = "";
                }
				row["promotion_id"] = promotion.PromotionRequestID;
				row["created_date"] = promotion.DateCreated;
				row["created_string"] = promotion.DateCreated.ToShortDateTimeString();
				row["created_by"] = promotion.CreatedBy;
				row["show_name"] = false;

				//
				// Pull out the number and status of the first name.
				//
				try
				{
					XmlDocument xdoc = new XmlDocument();
					xdoc.LoadXml("<body>" + promotion.WebSummary + "</body>");
					row["number_string"] = xdoc.ChildNodes[0].ChildNodes[0].ChildNodes[0].Value.ToString();
					if (xdoc.ChildNodes[0].ChildNodes.Count == 2)
						row["show_name"] = true;
				}
				catch
				{
				}

				dt.Rows.Add(row);
			}

			dgNote.DataSource = dt;
            dgNote.DataBind();
        }

		public void btnAddPost_Click(object sender, EventArgs e)
		{
            PromotionRequest promotion = new PromotionRequest();
			StringBuilder html;


			if (tbAddNumber.Text.Length > 0)
			{
                html = new StringBuilder(String.Format("<p id=\"SecurityCode\">{0}</p>", tbAddNumber.Text));

                //
                // Create the new promotion.
                //
                if (CampusID != -1)
                    promotion.Campus = new Arena.Organization.Campus(CampusID);
                promotion.ContactName = ArenaContext.Current.Person.FullName;
                promotion.ContactEmail = "";
                promotion.ContactPhone = "";
                promotion.Title = "Generic Number";
                promotion.TopicArea = new Lookup(TopicAreaID);
                promotion.WebSummary = html.ToString();
                promotion.WebPromote = true;
                promotion.WebStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                promotion.WebEndDate = promotion.WebStartDate.AddYears(1);
                promotion.WebApprovedBy = ArenaContext.Current.User.Identity.Name;
                promotion.WebApprovedDate = DateTime.Now;
                promotion.Save(ArenaContext.Current.User.Identity.Name);
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