using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Arena.Core;
using Arena.DataLib;
using Arena.Custom.Cccev.CheckIn.Entity;
using Arena.Custom.Cccev.CheckIn.DataLayer;

namespace Arena.Custom.HDC.CheckIn
{
	public class HDCSecurityCodeData : SqlData
	{
		public string GetNextSecurityCode(Person p)
		{
			string code = "%%%%%%";
			ArrayList list = new ArrayList();
			list.Add(new SqlParameter("@PersonID", p.PersonID));
			SqlParameter output = new SqlParameter("@FullCode", SqlDbType.Char, 8);
			output.Direction = ParameterDirection.Output;
			list.Add(output);

			try
			{
				this.ExecuteNonQuery("cust_hdc_checkin_sp_get_security_code", list);
				code = output.Value.ToString();
			}
			catch (SqlException ex)
			{
				throw ex;
			}
			finally
			{
				list = null;
			}

			if (code == null)
				code = "ISNULL";
			else if (code.Length < 6)
				code = "ZZLEN" + code.Length.ToString();

			return code;
		}
	}

	public class HDCSecurityCode : ISecurityCode
	{
		string ISecurityCode.GetSecurityCode(Person p)
		{
			return new HDCSecurityCodeData().GetNextSecurityCode(p);
		}
	}
}
