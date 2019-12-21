using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FenWeiPingYuanService
{
    public class DBHelperSqlServer
    {
        //private SqlConnection conn;
        private IniFile ini = new IniFile(System.Windows.Forms.Application.StartupPath + "\\setting.ini");
        public SqlConnection Open()
        {
            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection();
                conn.ConnectionString = "server=" + ini.GetValue("Database", "ServerName") + ";database=" + ini.GetValue("Database", "DatabaseName") + ";uid=" + ini.GetValue("Database", "LogId") + ";pwd=" + ini.GetValue("Database", "LogPass") + "";
                conn.Open();
            }
            catch (Exception)
            {
                Close(conn);
            }
            return conn;
        }
        public void Close(SqlConnection conn)
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }
        public DataSet GetDataSet(string sqlstr)
        {
            DataSet ds = null;
            SqlConnection conn = null;
            try
            {
                conn = Open();
                SqlDataAdapter da = new SqlDataAdapter(sqlstr, conn);
                ds = new DataSet();
                da.Fill(ds);
                Close(conn);
            }
            finally
            {
                Close(conn);
            }
            return ds;
        }
        public bool ExeSqlCMD(string sqlstr)
        {
            bool success = false;
            SqlConnection conn = null;
            try
            {
                conn = Open();
                SqlCommand cd = new SqlCommand(sqlstr, conn);
                cd.ExecuteNonQuery();
                Close(conn);
                success = true;
            }
            catch (Exception ex)
            {
                Close(conn);
                success = false;
                ErrorManager.AddErrorToLog(ex, sqlstr);
            }
            finally
            {
                Close(conn);
            }
            return success;
        }
        public DataTable GetDataTable(string sqlstr)
        {
            return GetDataSet(sqlstr).Tables[0];
        }
        public string ExecuteScalar(string safeSql)
        {
            object obj = "";
            SqlConnection conn = null;
            try
            {
                conn = Open();
                SqlCommand cd = conn.CreateCommand();
                cd.CommandText = safeSql;
                obj = cd.ExecuteScalar();
                if (obj == null || Convert.IsDBNull(obj))
                {
                    obj = "";
                }
                Close(conn);
            }
            catch (Exception ex)
            {
                ErrorManager.AddErrorToLog(ex, safeSql);
            }
            finally
            {
                Close(conn);
            }
            return obj.ToString();
        }

        #region 批量插入
        /// <summary>
        /// 向数据库表DestTableName中批量插入（复制）数据
        /// </summary>
        /// <param name="dataTable">需要插入的数据，已表形式提供</param>
        /// <param name="DestTableName">需要插入的表名</param>
        /// <param name="DestTableColNames">dataTable各列对应DestTableName中的各列名</param>
        /// <param name="checkRepeatFlag">是否检测主键重复数据</param>
        /// <param name="errorInfo">错误信息</param>
        /// <returns>是否插入成功</returns>
        public bool BatchInsertData(DataTable table, string DestTableName)
        {
            SqlConnection conn = null;
            try
            {
                //获取插入字段及入库字段的列的列名
                string[] DestTableColNames = new string[table.Columns.Count];
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    DestTableColNames[i] = table.Columns[i].ColumnName;
                }
                //打开连接
                string[] primaryKeyCols = getTablePrimaryKeyfromDatabase(DestTableName);

                //检测重复数据  创建临时表
                //string sqlStr = getCreateTable(DestTableName, primaryKeyCols);
                //sqlStr = sqlStr.Replace(DestTableName, "#" + DestTableName);
                conn = Open();
                SqlBulkCopy sbc = new SqlBulkCopy(conn);
                //字段映射。如果dataTable中的字段数和数据库表DestTableName中的字段数不一样多 一定要进行映射
                for (int colIndex = 0; colIndex < DestTableColNames.Length; colIndex++)
                {
                    sbc.ColumnMappings.Add(colIndex, DestTableColNames[colIndex]);
                }
                //每次加载数据的行数。不设置默认为全部，在本例中就可以不设置
                sbc.BulkCopyTimeout = 6000;
                sbc.BatchSize = 20000 / DestTableColNames.Length;
                SqlCommand tmpCommand = conn.CreateCommand();
                string primary = string.Empty;
                if (primaryKeyCols.Length > 0)
                {
                    primary = "alter table #" + DestTableName + " add constraint pk_#" + DestTableName + " primary key (" + string.Join(",", primaryKeyCols) + ")";
                }
                tmpCommand.CommandText = "select top 0 * into #" + DestTableName + " from " + DestTableName + ";" + primary;
                tmpCommand.ExecuteNonQuery();
                //把数据导入临时表
                sbc.DestinationTableName = "#" + DestTableName;
                sbc.WriteToServer(table);

                string whereStr = "";
                for (int colIndex = 0; colIndex < primaryKeyCols.Length; colIndex++)
                {
                    if (colIndex == 0)
                    {
                        whereStr = " where " + primaryKeyCols[colIndex] + " = " + DestTableName + "." + primaryKeyCols[colIndex];
                    }
                    else
                    {
                        whereStr = whereStr + " and " + primaryKeyCols[colIndex] + " = " + DestTableName + "." + primaryKeyCols[colIndex];
                    }
                }
                if (whereStr.Length > 0)
                {//根据主键删除重复数据
                    tmpCommand.CommandText = "delete " + DestTableName + " where (select count(1) from #" + DestTableName + whereStr + ") > 0";
                    tmpCommand.ExecuteNonQuery();
                }
                string sqlStr = "insert into " + DestTableName + " (";
                for (int colIndex = 0; colIndex < DestTableColNames.Length; colIndex++)
                {
                    if (colIndex == 0)
                    {
                        sqlStr = sqlStr + DestTableColNames[colIndex];
                    }
                    else
                    {
                        sqlStr = sqlStr + "," + DestTableColNames[colIndex];
                    }
                }
                sqlStr = sqlStr + ") select ";
                for (int colIndex = 0; colIndex < DestTableColNames.Length; colIndex++)
                {
                    if (colIndex == 0)
                    {
                        sqlStr = sqlStr + DestTableColNames[colIndex];
                    }
                    else
                    {
                        sqlStr = sqlStr + "," + DestTableColNames[colIndex];
                    }
                }
                sqlStr = sqlStr + " from #" + DestTableName;
                tmpCommand.CommandText = sqlStr;
                tmpCommand.ExecuteNonQuery();

                //删除临时表
                sqlStr = "drop table #" + DestTableName;
                tmpCommand.CommandText = sqlStr;
                tmpCommand.ExecuteNonQuery();

                Close(conn);
            }
            catch (Exception ex)
            {
                ErrorManager.AddErrorToLog(ex, DestTableName + "入库失败");
            }
            finally
            {
                Close(conn);
            }
            return true;
        }

        /// <summary>
        /// 得到一个表的主键列
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <returns>主键列名</returns>
        private string[] getTablePrimaryKeyfromDatabase(string TableName)
        {
            string[] colNames = new string[0];
            SqlConnection conn = null;
            try
            {
                conn = Open();
                SqlCommand tmpCommand = conn.CreateCommand();
                tmpCommand.CommandText = "exec sys.sp_pkeys " + TableName;

                SqlDataAdapter tmp_Adapter = new SqlDataAdapter(tmpCommand);
                DataTable dt = new DataTable();
                tmp_Adapter.Fill(dt);
                tmp_Adapter.Dispose();
                Close(conn);
                colNames = new string[dt.Rows.Count];
                for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
                {
                    colNames[rowIndex] = dt.Rows[rowIndex]["column_name"].ToString();
                }
            }
            finally
            {
                Close(conn);
            }
            return colNames;
        }
        #endregion


        #region 停用  重复数据报主键错误，所以停用
        /// <summary>
        /// 批量插入数据。
        /// 重复数据报主键错误，所以停用
        /// </summary>
        /// <param name="table">数据集。</param>
        /// <param name="tableName">待插入数据的表名。</param>
        /// <returns>插入成功，则返回true；否则返回false。</returns>
        private bool BatchInsertData_停用(DataTable table, string tableName)
        {
            string[] arrDataTableColumn = new string[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                arrDataTableColumn[i] = table.Columns[i].ColumnName;
            }
            string[] arrSqlTableColumn = arrDataTableColumn;
            //bool success = BatchInsertData(table, tableName, arrDataTableColumn, arrSqlTableColumn);
            return false;
        }
        /// <summary>
        /// 从数据库中得到某表的创建脚本
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>表的创建脚本</returns>
        private string getCreateTable(string tableName, string[] primaryKeyCols)
        {
            string sql = "SELECT syscolumns.name as 字段名,systypes.name as 字段类型,syscolumns.prec as 字段长度,syscolumns.scale as 小数位数,syscolumns.isnullable 是否为空 FROM syscolumns, systypes  WHERE syscolumns.xusertype = systypes.xusertype  AND syscolumns.id = object_id('meto_envi_info')";
            DataTable table = GetDataTable(sql);
            string sqlTable = "CREATE TABLE " + tableName + " ( ";
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    string columnName = row["字段名"].ToString();
                    string dataType = row["字段类型"].ToString();
                    string columnLength = row["字段长度"].ToString();
                    string scale = row["小数位数"].ToString();
                    string isNullable = row["是否为空"].ToString() == "1" ? "NULL" : "NOT NULL";
                    sqlTable += "[" + columnName + "] [" + dataType + "] ";
                    if (scale.Length > 0)
                    {
                        sqlTable += "(" + columnLength + "," + scale + ") ";
                    }
                    else
                    {
                        sqlTable += "(" + columnLength + ")";
                    }
                    sqlTable += isNullable + ",";
                }
                sqlTable += "primary key (" + string.Join(",", primaryKeyCols) + "))";
            }
            return sqlTable;
        }
        /// <summary>
        /// 批量插入数据。
        /// </summary>
        /// <param name="table">数据集。</param>
        /// <param name="tableName">待插入数据的表名。</param>
        /// <param name="arrDataTableColumn">DataTable要入库的列。</param>
        /// <param name="arrSqlTableColumn">SqlServer表中待入库的列。</param>
        /// <returns>插入成功，则返回true；否则返回false。</returns>
        private bool BatchInsertData(DataTable table, string tableName, string[] arrDataTableColumn, string[] arrSqlTableColumn)
        {
            bool success = false;
            SqlConnection conn = null;
            try
            {
                conn = Open();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(conn);
                sqlBulkCopy.DestinationTableName = tableName;
                sqlBulkCopy.BatchSize = 100000;//互联网帖子结论10万时效率最高
                sqlBulkCopy.NotifyAfter = table.Rows.Count;
                sqlBulkCopy.BulkCopyTimeout = 7200;

                for (int i = 0; i < arrDataTableColumn.Length; i++)
                {
                    sqlBulkCopy.ColumnMappings.Add(arrDataTableColumn[i], arrSqlTableColumn[i]);
                }
                sqlBulkCopy.WriteToServer(table);
                sqlBulkCopy.Close();
                Close(conn);
                success = true;
            }
            finally
            {
                Close(conn);
            }
            return success;
        }
        #endregion
    }
}
