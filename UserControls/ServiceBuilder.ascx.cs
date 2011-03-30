﻿/**********************************************************************
* Description:	This module helps with the creation of full services
*               for use with Check-In.
* Created By:	Daniel Hazelbaker @ High Desert Church
* Date Created:	10/21/2009 12:23:14 PM
**********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using Arena.Core;
using Arena.Portal;
using Arena.Organization;
using Arena.SmallGroup;

namespace ArenaWeb.UserControls.Custom.HDC.CheckIn
{
    public partial class ServiceBuilder : PortalControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //
                // Setup list of available attendance type groups.
                //
                foreach (OccurrenceTypeGroup otg in new OccurrenceTypeGroupCollection(ArenaContext.Current.Organization.OrganizationID))
                {
                    ddlOccurrenceTypeGroup.Items.Add(new ListItem(otg.GroupName, otg.GroupId.ToString()));
                }
                ddlOccurrenceTypeGroup.SelectedIndex = 0;

                //
                // Setup the list of available attendance types.
                //
                ltAttendanceTypes.Text = "";
                foreach (OccurrenceType ot in new OccurrenceTypeCollection(Convert.ToInt32(ddlOccurrenceTypeGroup.SelectedValue)))
                {
                    ltAttendanceTypes.Text += String.Format("<li id=\"at-{0}\" class=\"attendanceType draggableItem ui-state-default\" data-age=\"{1}\">{2}</li>",
                        ot.OccurrenceTypeId.ToString(), ot.MinAge.ToString(), ot.TypeName);
                }

                //
                // Setup the list of available locations.
                //
                ltLocations.Text = "";
                foreach (Location l in new LocationCollection(ArenaContext.Current.Organization.OrganizationID))
                {
                    ltLocations.Text += String.Format("<li id=\"room-{0}\" class=\"room draggableItem ui-state-default\">{1}</li>",
                        l.LocationId.ToString(), l.FullName);
                }
            }
        }


        protected void ddlOccurrenceTypeGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
            // Setup the list of available attendance types.
            //
            ltAttendanceTypes.Text = "";
            foreach (OccurrenceType ot in new OccurrenceTypeCollection(Convert.ToInt32(ddlOccurrenceTypeGroup.SelectedValue)))
            {
                ltAttendanceTypes.Text += String.Format("<li id=\"at-{0}\" class=\"attendanceType draggableItem ui-state-default\" data-age=\"{1}\">{2}</li>",
                    ot.OccurrenceTypeId.ToString(), ot.MinAge.ToString(), ot.TypeName);
            }
        }


        protected void btnCreate_Click(object sender, EventArgs e)
        {
            Dictionary<Location, List<OccurrenceType>> occurrenceData = new Dictionary<Location, List<OccurrenceType>>();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<String, Object> data = (Dictionary<String,Object>)serializer.DeserializeObject(jsonData.Value);
            List<Int32> occurrenceIDs = new List<int>();


            lbErrors.Text = "";
            lbStatus.Text = "";

            //
            // Check if they have designed a service or not.
            //
            if (data.Keys.Count == 0)
            {
                lbErrors.Text = "No rooms have been assigned.";
                return;
            }

            //
            // Walk each room and create a list of the attendance types that should
            // be created for the room.
            //
            foreach (String r in data.Keys)
            {
                Location location = new Location(Convert.ToInt32(r.Substring(11)));
                Dictionary<String, Object> room = (Dictionary<String, Object>)data[r];
                Object[] ats = (Object[])room["ats"];

                //
                // Check for empty room.
                //
                if (ats.Length == 0)
                {
                    lbErrors.Text = "Location " + location.FullName + " is being used but no attendance types associated.";
                }

                occurrenceData.Add(location, new List<OccurrenceType>());

                //
                // Walk each attendance type in this room and store the information
                // needed to create a new attendance type.
                //
                foreach (Dictionary<String, Object> at in ats)
                {
                    occurrenceData[location].Add(new OccurrenceType(Convert.ToInt32(at["id"].ToString().Substring(3))));
                }
            }

            //
            // Check if there were errors, if so abort.
            //
            if (lbErrors.Text != "")
                return;

            //
            // Generate each occurrence.
            //
            try
            {
                foreach (Location loc in occurrenceData.Keys)
                {
                    foreach (OccurrenceType ot in occurrenceData[loc])
                    {
                        occurrenceIDs.Add(CreateOccurrence(ot,
                            ppProfile.ProfileID,
                            tbName.Text,
                            loc,
                            DateTime.Parse(tbStartDate.Text + " " + tbStartTime.Text),
                            DateTime.Parse(tbEndDate.Text + " " + tbEndTime.Text),
                            DateTime.Parse(tbCheckInStartDate.Text + " " + tbCheckInStartTime.Text),
                            DateTime.Parse(tbCheckInEndDate.Text + " " + tbCheckInEndTime.Text),
                            cbMembershipRequired.Checked));

                    }
                }
            }
            catch (System.Exception ex)
            {
                //
                // If something goes bad, try to delete all the occurrences we just created.
                //
                foreach (Int32 id in occurrenceIDs)
                {
                    try
                    {
                        Occurrence oc = new Occurrence(id);
                        oc.Delete();
                    }
                    catch { }
                }

                throw ex;
            }

            //
            // Everything successful.
            //
            jsonData.Value = "{}";
            lbStatus.Text = "Generated " + occurrenceIDs.Count.ToString() + " occurrences.";
        }


        private Int32 CreateOccurrence(OccurrenceType type, int profileID, String name, Location location, DateTime startTime, DateTime endTime, DateTime checkinStart, DateTime checkinEnd, Boolean membershipRequired)
        {
            Occurrence occurrence = null;


            if (profileID == -1 && type.SyncWithProfile != -1)
                profileID = type.SyncWithProfile;

            //
            // Create the appropriate type of occurrence object.
            //
            if (profileID != -1)
            {
                ProfileOccurrence pOccurrence = new ProfileOccurrence();

                pOccurrence.ProfileID = profileID;

                occurrence = pOccurrence;
            }
            else if (type.SyncWithGroup != -1)
            {
                GroupOccurrence gOccurrence = new GroupOccurrence();

                gOccurrence.GroupID = type.SyncWithGroup;

                occurrence = gOccurrence;
            }
            else
            {
                occurrence = new Occurrence();
            }

            //
            // Set all the common information.
            //
            occurrence.OccurrenceID = -1;
            occurrence.OccurrenceType = type;
            occurrence.Name = name;
            occurrence.Location = location.FullName;
            occurrence.LocationID = location.LocationId;
            occurrence.Description = "";
            occurrence.StartTime = startTime;
            occurrence.EndTime = endTime;
            occurrence.CheckInStart = (checkinStart != null ? checkinStart : DateTime.Parse("1/1/1900"));
            occurrence.CheckInEnd = (checkinEnd != null ? checkinEnd : DateTime.Parse("1/1/1900"));
            occurrence.MembershipRequired = membershipRequired;

            //
            // Save the new occurrence.
            //
            occurrence.Save(CurrentUser.Identity.Name);

            return occurrence.OccurrenceID;
        }
    }
}