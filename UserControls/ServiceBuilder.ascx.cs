/**********************************************************************
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
        /// <summary>
        /// The page has loaded, do any initial setup of the controls.
        /// </summary>
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
                    ltAttendanceTypes.Text += String.Format("<li id=\"at-{0}\" class=\"sbAttendanceType sbDraggableItem sbListItem ui-state-default\" data-age=\"{1}\">{2}</li>",
                        ot.OccurrenceTypeId.ToString(), ot.MinAge.ToString(), ot.TypeName);
                }

                //
                // Setup the list of available locations.
                //
                ltLocations.Text = "";
                foreach (Location l in new LocationCollection(ArenaContext.Current.Organization.OrganizationID))
                {
                    ltLocations.Text += String.Format("<li id=\"room-{0}\" class=\"sbRoom sbDraggableItem sbListItem ui-state-default\">{1}</li>",
                        l.LocationId.ToString(), l.FullName);
                }
            }
        }


        /// <summary>
        /// The occurrence type group drop down has changed, update the list of
        /// attendance types available.
        /// </summary>
        protected void ddlOccurrenceTypeGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
            // Setup the list of available attendance types.
            //
            ltAttendanceTypes.Text = "";
            foreach (OccurrenceType ot in new OccurrenceTypeCollection(Convert.ToInt32(ddlOccurrenceTypeGroup.SelectedValue)))
            {
                ltAttendanceTypes.Text += String.Format("<li id=\"at-{0}\" class=\"sbAttendanceType sbDraggableItem sbListItem ui-state-default\" data-age=\"{1}\">{2}</li>",
                    ot.OccurrenceTypeId.ToString(), ot.MinAge.ToString(), ot.TypeName);
            }
        }


        /// <summary>
        /// User is ready to create a new service based on their selection. Do
        /// some basic error checking before creating anything.
        /// </summary>
        protected void btnCreate_Click(object sender, EventArgs e)
        {
            Dictionary<Location, List<OccurrenceType>> occurrenceData = new Dictionary<Location, List<OccurrenceType>>();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<String, Object> data = (Dictionary<String,Object>)serializer.DeserializeObject(jsonData.Value);
            List<Int32> occurrenceIDs = new List<int>();
            DateTime startDateTime = DateTime.Parse(tbStartDate.Text + " " + tbStartTime.Text);
            DateTime endDateTime = DateTime.Parse(tbEndDate.Text + " " + tbEndTime.Text);
            DateTime checkinStart = DateTime.Parse(tbCheckInStartDate.Text + " " + tbCheckInStartTime.Text);
            DateTime checkinEnd = DateTime.Parse(tbCheckInEndDate.Text + " " + tbCheckInEndTime.Text);


            //
            // Reset any status information.
            //
            lbErrors.Text = "";
            lbStatus.Text = "";

            //
            // Check if they have entered a service name.
            //
            if (String.IsNullOrEmpty(tbName.Text))
            {
                lbErrors.Text += "Name is a required field and must be supplied.<br />";
            }

            //
            // Check if they are trying to end before they start.
            //
            if (endDateTime <= startDateTime)
            {
                lbErrors.Text += "End-time for service cannot be before start-time.<br />";
            }
            if (checkinEnd <= checkinStart)
            {
                lbErrors.Text += "End-time for check-in cannot be before start-time.<br />";
            }

            //
            // Check if they checked the "Use For All" box but did not select
            // a profile.
            //
            if (cbUseForAll.Checked == true && ppProfile.ProfileID == -1)
            {
                lbErrors.Text += "You must select a tag if you wish to force all occurrences into that Tag.<br />";
            }

            //
            // Check if they have designed a service or not.
            //
            if (data.Keys.Count == 0)
            {
                lbErrors.Text += "No rooms have been assigned.<br />";
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
                    lbErrors.Text += "Location " + location.FullName + " is being used but no attendance types associated.<br />";
                }

                occurrenceData.Add(location, new List<OccurrenceType>());

                //
                // Walk each attendance type in this room and store the information
                // needed to create a new attendance type.
                //
                foreach (Dictionary<String, Object> at in ats)
                {
                    OccurrenceType ot = new OccurrenceType(Convert.ToInt32(at["id"].ToString().Substring(3)));
                    Location loc = null;

                    //
                    // Make sure the location is valid for this occurrence type.
                    //
                    foreach (Location l in ot.Locations)
                    {
                        if (l.LocationId == location.LocationId)
                        {
                            loc = l;
                            break;
                        }
                    }
                    if (loc == null)
                    {
                        lbErrors.Text += "Attendance type " + ot.TypeName + " is not valid for location " + location.FullName + "<br />";
                        continue;
                    }

                    //
                    // Make sure there is a place will will link the occurrence to.
                    //
                    if (ot.SyncWithProfile == -1 && ot.SyncWithGroup == -1 && ppProfile.ProfileID == -1)
                    {
                        lbErrors.Text += "Attendance type " + ot.TypeName + " is not linked to a tag or group and no <i>Link To Tag</i> option has been specified.<br />";
                        continue;
                    }

                    occurrenceData[location].Add(ot);
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
                            cbUseForAll.Checked,
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


        /// <summary>
        /// Generate a new occurrence in the database from the values given
        /// in the parameters.
        /// </summary>
        /// <param name="type">The OccurrenceType to use for this occurrence.</param>
        /// <param name="profileID">The profile to associate the occurrence with if it is not already linked in the occurrence type.</param>
        /// <param name="forceProfile">Force the occurrence to be associated with the profileID given.</param>
        /// <param name="name">The name of the occurrence.</param>
        /// <param name="location">The Location object to use for check-in.</param>
        /// <param name="startTime">The time the occurrence should start.</param>
        /// <param name="endTime">The time the occurrence should end.</param>
        /// <param name="checkinStart">The time that check-in should start.</param>
        /// <param name="checkinEnd">The time that check-in should end.</param>
        /// <param name="membershipRequired">If the membership required flag on the occurrence should be set.</param>
        /// <returns>The ID number of the new occurrence that was generated.</returns>
        private Int32 CreateOccurrence(OccurrenceType type, int profileID, Boolean forceProfile, String name, Location location, DateTime startTime, DateTime endTime, DateTime checkinStart, DateTime checkinEnd, Boolean membershipRequired)
        {
            Occurrence occurrence = null;


            //
            // Allow the occurrence to override if forceProfile is not true.
            //
            if (forceProfile == false)
            {
                if (type.SyncWithProfile != -1)
                    profileID = type.SyncWithProfile;
                if (type.SyncWithGroup != -1)
                    profileID = -1;
            }

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
                throw new ArgumentException("Occurrences must be tied to either a tag or group.");
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
            occurrence.CheckInStart = checkinStart;
            occurrence.CheckInEnd = checkinEnd;
            occurrence.MembershipRequired = membershipRequired;

            //
            // Save the new occurrence.
            //
            occurrence.Save(CurrentUser.Identity.Name);

            return occurrence.OccurrenceID;
        }
    }
}