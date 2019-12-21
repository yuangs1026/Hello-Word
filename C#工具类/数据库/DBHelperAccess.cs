using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace FenWeiPingYuanService
{
    public class DBHelperAccess
    {
        public static OleDbConnection GetConnection()
        {
            string AccessConnectionString = "Provider = Microsoft.Jet.OLEDB.4.0;Data Source = " + Application.StartupPath + @"\DatabaseSetting.mdb";
            return new OleDbConnection(AccessConnectionString);
        }

        public static DataTable ExecuteSelectCommand(string safeSql)
        {
            DataSet ds = new DataSet();
            OleDbCommand cmd = new OleDbCommand(safeSql, GetConnection());

            try
            {
                cmd.Connection.Open();
                OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                ErrorManager.AddErrorToLog(ex,null);
            }
            finally
            {
                cmd.Connection.Close();
            }
            return ds.Tables[0];
        }
    }
}
