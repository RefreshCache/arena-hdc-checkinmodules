/**********************************************************************
* Description:	This module provides a way to print backup labels when
*               using RS labels. The labels are pre-printed with a
*               security code and blank spots for names.
* Created By:	Daniel Hazelbaker @ High Desert Church
* Date Created:	10/21/2009 12:23:14 PM
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
    using Arena.Computer;
    using Arena.Reporting;

	public partial class PrintBackupLabels : PortalControl
	{
        [TextSetting("Report Paths", "Enter the Reporting Services paths, one per line to print.", true)]
        public string ReportPaths { get { return Setting("ReportPaths", "", true); } }
        
        #region Event Handlers

		private void Page_Load(object sender, System.EventArgs e)
		{
			if ( ! IsPostBack )
			{
                //
                // Load up the list of printers.
                //
                ComputerSystemPrinterCollection cspc = new ComputerSystemPrinterCollection(ArenaContext.Current.Organization.OrganizationID);

                foreach (ComputerSystemPrinter csp in cspc)
                {
                    ddlPrinter.Items.Add(new ListItem(csp.PrinterName, csp.PrinterName));
                }

                tbStartingNumber.Text = "19000";
                tbPrintCount.Text = "100";
			}
		}
		
		#endregion

		public void btnPrint_Click(object sender, EventArgs e)
		{
            int i, startingNumber, printCount;
            ReportPrintJobCollection jobs = new ReportPrintJobCollection();
            ReportPrintJob job;
            String[] paths;


            paths =  ReportPaths.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            startingNumber = Convert.ToInt32(tbStartingNumber.Text);
            printCount = Convert.ToInt32(tbPrintCount.Text);

            for (i = startingNumber; i < startingNumber + printCount; i++)
            {
                List<ReportParameter> parameters = new List<ReportParameter>();

                parameters.Add(new ReportParameter("SecurityCode", String.Format("BK{0}", i.ToString())));
                foreach (String path in paths)
                {
                    job = new ReportPrintJob(ddlPrinter.SelectedValue, path, 1, false, parameters, true, "BackupLabel");
                    jobs.Add(job);
                }
            }

            ReportPrinter printer = new ReportPrinter();
            printer.PrintReports(jobs);
		}
	}
}