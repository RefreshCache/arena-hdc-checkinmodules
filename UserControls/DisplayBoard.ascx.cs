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

	public partial class DisplayBoard : PortalControl
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			int noteID;

			noteID = GetNextNoteID();
			if (Request.Params["format"] == "html")
				SendDisplayData(noteID);
			if (Request.Params["format"] == "xml")
				SendDisplayXML(noteID);
		}

		private int GetNextNoteID()
		{
			ArrayList idNumbers = new ArrayList();
			ArrayList paramList = new ArrayList();
			int i, lastID = -1;


			paramList.Add(new SqlParameter("@SystemId", 1));

			//
			// Open the data reader
			//
			SqlDataReader reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(
						"cust_hdc_checkin_sp_get_notesForSystemID", paramList);

            while (reader.Read())
            {
                idNumbers.Add(reader["note_id"]);
            }
			reader.Close();

			//
			// Check for the previous ID number.
			//
			if (Request.Params["lastID"] != null)
				lastID = Convert.ToInt32(Request.Params["lastID"]);

			//
			// If there were no notes found, return -1.
			//
			if (idNumbers.Count == 0)
				return -1;

			//
			// If this was the first time called, return the first note.
			//
			if (lastID == -1)
				return (int)idNumbers[0];

			//
			// We have all the ID numbers for this system, snag the next one.
			//
			for (i = 0; i < idNumbers.Count; i++)
			{
				if ((int)idNumbers[i] > lastID)
					return (int)idNumbers[i];
			}

			return (int)idNumbers[0];
		}


		private void SendDisplayData(int noteID)
		{
			ArrayList paramList = new ArrayList();


			paramList.Add(new SqlParameter("@NoteId", noteID));

			//
			// Open the data reader
			//
			SqlDataReader reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(
						"cust_hdc_checkin_sp_get_noteByID", paramList);

			if (reader.Read() == true)
			{
				Response.Write(reader["note_html"]);
			}

			//
			// The End() forces .NET to send the data and close out the connection cleanly.
			//
			reader.Close();
			Response.End();
		}


		private void SendDisplayXML(int noteID)
		{
			StringBuilder sb = new StringBuilder();
			StringWriter writer = new StringWriter(sb);
			XmlDocument xdoc = new XmlDocument();
			XmlDeclaration dec;
			XmlNode root, node;
			ArrayList paramList = new ArrayList();


			paramList.Add(new SqlParameter("@NoteId", noteID));

			//
			// Open the data reader
			//
			SqlDataReader reader = new Arena.DataLayer.Organization.OrganizationData().ExecuteReader(
						"cust_hdc_checkin_sp_get_noteByID", paramList);


			dec = xdoc.CreateXmlDeclaration("1.0", "utf-8", null);
			xdoc.InsertBefore(dec, xdoc.DocumentElement);

			root = xdoc.CreateElement("Display");

			if (reader.Read() == true)
			{
				node = xdoc.CreateElement("ID");
				node.AppendChild(xdoc.CreateTextNode(reader["note_id"].ToString()));
				root.AppendChild(node);

				node = xdoc.CreateElement("Data");
				node.AppendChild(xdoc.CreateTextNode(reader["note_html"].ToString()));
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
			reader.Close();
			xdoc.Save(writer);
			Response.Write(sb.ToString());
			Response.End();
		}
	}
}
