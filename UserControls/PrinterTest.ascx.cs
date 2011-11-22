using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Printing;

using Arena.CheckIn;
using Arena.Computer;
using Arena.Framework.ReportService;
using Arena.Organization;
using Arena.Portal;
using Arena.Reporting;

namespace ArenaWeb.UserControls.Custom.HDC.CheckIn
{
    public partial class PrinterTest : PortalControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //
                // Populate the list of system printers.
                //
                ddlSystemPrinters.Items.Add(new ListItem(""));
                foreach (String printer in PrinterSettings.InstalledPrinters)
                {
                    ddlSystemPrinters.Items.Add(new ListItem(printer));
                }

                //
                // Populate the list of Arena printers.
                //
                ddlArenaPrinters.Items.Add(new ListItem(""));
                foreach (ComputerSystemPrinter csp in new ComputerSystemPrinterCollection(CurrentArenaContext.Organization.OrganizationID))
                {
                    ddlArenaPrinters.Items.Add(new ListItem(csp.PrinterName));
                }

                //
                // Populate the list of Locations.
                //
                ddlLocations.Items.Add(new ListItem("", "-1"));
                foreach (Location loc in new LocationCollection(CurrentArenaContext.Organization.OrganizationID))
                {
                    ddlLocations.Items.Add(new ListItem(loc.FullName, loc.LocationId.ToString()));
                }

                //
                // Populate the list of Kiosks.
                //
                ddlKiosks.Items.Add(new ListItem(""));
                ComputerSystemCollection csc = new ComputerSystemCollection();
                csc.LoadAllSystems(CurrentArenaContext.Organization.OrganizationID);
                foreach (ComputerSystem comp in csc)
                {
                    if (comp.Kiosk)
                        ddlKiosks.Items.Add(new ListItem(comp.SystemName, comp.SystemId.ToString()));
                }

                ReportingService2005 reportingService = new ReportServiceBuilder().GetReportingService();
                foreach (CatalogItem item in reportingService.ListChildren(CurrentOrganization.Settings["ReportServerRoot"] + "/CheckIn", true))
                {
                    if (item.Type == ItemTypeEnum.Report)
                    {
                        ddlRSDirectLabel.Items.Add(new ListItem(item.Name, item.Path));
                        ddlRSFrameworkLabel.Items.Add(new ListItem(item.Name, item.Path));
                    }
                }
            }
        }


        protected void PrintTestButton(object sender, EventArgs e)
        {
            String printerName = null;


            try
            {
                if (rbSystemPrinters.Checked)
                    printerName = ddlSystemPrinters.SelectedItem.Text;
                else if (rbArenaPrinters.Checked)
                    printerName = ddlArenaPrinters.SelectedValue;
                else if (rbLocations.Checked)
                    printerName = new Location(Convert.ToInt32(ddlLocations.SelectedValue)).Printer.PrinterName;
                else if (rbKiosks.Checked)
                    printerName = new Kiosk(Convert.ToInt32(ddlKiosks.SelectedValue)).System.Printer.PrinterName;
            }
            catch (System.Exception ex)
            {
                lbResults.Text = "Invalid printer:<br />" + ex.Message + "<br />" + ex.StackTrace;
                return;
            }

            if (String.IsNullOrEmpty(printerName))
            {
                lbResults.Text = "No valid printer has been selected.";
                return;
            }

            //
            // Do the print.
            //
            lbResults.Text = "No printing errors encountered.";
            try
            {
                if (rbPrintBasicText.Checked)
                    PrintStandardPage(printerName);
                else if (rbPrintRSDirect.Checked)
                {
                    PrintRSDirect(printerName, ddlRSDirectLabel.SelectedValue);
                }
                else if (rbPrintRSFramework.Checked)
                {
                    PrintRSFramework(printerName, ddlRSFrameworkLabel.SelectedValue);
                }
            }
            catch (System.Exception ex)
            {
                lbResults.Text = "Exception while printing:<br />" + ex.Message + "<br />" + ex.StackTrace;
            }
        }


        #region Standard Test Print Page, basically "hello world".

        private void PrintStandardPage(String printerName)
        {
            PrintDocument pDoc = new PrintDocument();


            pDoc.PrintPage += new PrintPageEventHandler(pDoc_PrintPage);
            pDoc.PrinterSettings.PrinterName = printerName;

            pDoc.Print();
        }


        void pDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font font = new Font("Arial", 12);
            SolidBrush brush = new SolidBrush(Color.Black);
            e.Graphics.DrawString("Arena Test Print", font, brush, 0, 0);
        }

        #endregion


        #region Reporting Services Print.

        private void PrintRSDirect(String printerName, String reportPath)
        {
            ReportPrintJob job;
            ReportPrinter printer = new ReportPrinter();
            List<Arena.Reporting.ReportParameter> parameters = new List<Arena.Reporting.ReportParameter>();


            parameters.Add(new Arena.Reporting.ReportParameter("OccurrenceAttendanceID", "-1"));
            job = new ReportPrintJob(printerName, reportPath, 1, cbRSDirectLandscape.Checked, parameters, false, string.Empty);

            printer.PrintReport(job.PrinterName, job.ReportPath, job.Copies, job.Landscape, job.Parameters, job.PrinterName);
        }

        private void PrintRSFramework(String printerName, String reportPath)
        {
            ReportPrintJobCollection jobs = new ReportPrintJobCollection();
            ReportPrintJob job;
            ReportPrinter printer = new ReportPrinter();
            List<Arena.Reporting.ReportParameter> parameters = new List<Arena.Reporting.ReportParameter>();


            parameters.Add(new Arena.Reporting.ReportParameter("OccurrenceAttendanceID", "-1"));
            job = new ReportPrintJob(printerName, reportPath, 1, cbRSDirectLandscape.Checked, parameters, false, string.Empty);
            jobs.Add(job);

            printer.PrintReports(jobs);
        }

        #endregion
    }
}