/**********************************************************************
* Description:	This module provides a simple number board display
*				system. It also provides the XML driven source feed.
* Created By:	Daniel Hazelbaker, High Desert Church
* Date Created:	11/6/2009 10:57:32 AM
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
	using System.Collections;
	using System.Configuration;
	using System.Data;
	using System.Data.SqlClient;
	using System.Linq;
	using System.IO;
	using System.Web;
	using System.Web.Security;
	using System.Web.UI;
	using System.Web.UI.HtmlControls;
	using System.Web.UI.WebControls;
	using System.Web.UI.WebControls.WebParts;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;

	using Arena.Portal;
	using Arena.Core;
	using Arena.Organization;
    using Arena.Marketing;

	public partial class DisplayBoard : PortalControl
    {
        #region Module Settings

        [TextSetting("Topic Areas", "Enter a comma separated list of topic areas to be displayed by this module (e.g. \"423,827\")", true)]
        public string TopicAreaList { get { return Setting("TopicAreaList", "", true); } }

        [CampusSetting("Campus", "Select the campus to limit this module to, if you do not enter a campus then all campuses will be used.", false)]
        public int CampusID { get { return Convert.ToInt32(Setting("CampusID", "-1", false)); } }

        #endregion

        private void Page_Load(object sender, System.EventArgs e)
		{
			int promotionID;

			promotionID = GetNextPromotionID();
			if (Request.Params["format"] == "html")
				SendDisplayData(promotionID);
			if (Request.Params["format"] == "xml")
				SendDisplayXML(promotionID);
		}

		private int GetNextPromotionID()
		{
            PromotionRequestCollection prc = new PromotionRequestCollection();
			int i, lastID = -1;


            //
            // Load all promotions for this display.
            //
            prc.LoadCurrentWebRequests(TopicAreaList, "both", CampusID, 1000, false, -1);
            
            //
			// Check for the previous ID number.
			//
			if (Request.Params["lastID"] != null)
				lastID = Convert.ToInt32(Request.Params["lastID"]);

			//
			// If there were no notes found, return -1.
			//
			if (prc.Count == 0)
				return -1;

			//
			// If this was the first time called, return the first note.
			//
			if (lastID == -1)
				return prc[0].PromotionRequestID;

			//
			// We have all the ID numbers for this system, snag the next one.
			//
			for (i = 0; i < prc.Count; i++)
			{
                if (prc[i].PromotionRequestID == lastID)
                {
                    if (++i >= prc.Count)
                        return prc[0].PromotionRequestID;
                    else
                        return prc[i].PromotionRequestID;
                }
			}

			return prc[0].PromotionRequestID;
		}


		private void SendDisplayData(int promotionID)
		{
            PromotionRequest promotion = new PromotionRequest(promotionID);

            
            if (promotion.PromotionRequestID != -1)
			{
				Response.Write(promotion.WebSummary);
			}

			//
			// The End() forces .NET to send the data and close out the connection cleanly.
			//
			Response.End();
		}


		private void SendDisplayXML(int promotionID)
		{
            PromotionRequest promotion = new PromotionRequest(promotionID);
			StringBuilder sb = new StringBuilder();
			StringWriter writer = new StringWriter(sb);
			XmlDocument xdoc = new XmlDocument();
			XmlDeclaration dec;
			XmlNode root, node;


			dec = xdoc.CreateXmlDeclaration("1.0", "utf-8", null);
			xdoc.InsertBefore(dec, xdoc.DocumentElement);

			root = xdoc.CreateElement("Display");

			if (promotion.PromotionRequestID != -1)
			{
				node = xdoc.CreateElement("ID");
				node.AppendChild(xdoc.CreateTextNode(promotion.PromotionRequestID.ToString()));
				root.AppendChild(node);

				node = xdoc.CreateElement("Data");
				node.AppendChild(xdoc.CreateTextNode(promotion.WebSummary));
				root.AppendChild(node);
			}
			else
			{
				node = xdoc.CreateElement("ID");
				node.AppendChild(xdoc.CreateTextNode("-1"));
				root.AppendChild(node);
			}

			xdoc.AppendChild(root);

			//
			// Send the XML stream. The End() forces .NET to send the data and close
			// out the connection cleanly.
			//
			xdoc.Save(writer);
			Response.Write(sb.ToString());
			Response.End();
		}
	}
}
