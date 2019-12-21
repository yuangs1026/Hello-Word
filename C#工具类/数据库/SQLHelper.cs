using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace SQLHelper
{
    public class Database
    {
        private string dbtype;
        public SqlCommand cmd_mssql;
        public OleDbCommand cmd_access;
        public DDTek.Oracle.OracleCommand cmd_oracle;

        public Database(string Databasetype,string Connection)
        {
            dbtype = Databasetype;
            switch (dbtype)
            {
                case "Mssql":
                    //server={0};database={1};uid={2};pwd={3}
                    cmd_mssql = new SqlCommand();
                    cmd_mssql.Connection = new SqlConnection(Connection);
                    break;
                case "Access":
                    //"Provider=Microsoft.Jet.OleDb.4.0;Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "\\db.mdb;Jet OLEDB:Database Password=zhwykj&1712"
                    cmd_access = new OleDbCommand();
                    cmd_access.Connection = new OleDbConnection(Connection);
                    break;
                case "Oracle":
                    //"Host=127.0.0.1;Port=1521;User ID=scott;Password=tiger;Service Name=ORCL"
                    cmd_oracle = new DDTek.Oracle.OracleCommand();
                    cmd_oracle.Connection = new DDTek.Oracle.OracleConnection(Connection);
                    break;
                default:
                    throw new Exception("无法识别的数据库类型!");
            }
        }

        public void open()
        {
            switch (dbtype)
            {
                case "Mssql":
                    if (cmd_mssql.Connection.State == ConnectionState.Closed)
                    {
                        cmd_mssql.Connection.Open();
                    }
                    else if (cmd_mssql.Connection.State == ConnectionState.Broken)
                    {
                        cmd_mssql.Connection.Close();
                        cmd_mssql.Connection.Open();
                    }
                    break;
                case "Access":
                    if (cmd_access.Connection.State == ConnectionState.Closed)
                    {
                        cmd_access.Connection.Open();
                    }
                    else if (cmd_access.Connection.State == ConnectionState.Broken)
                    {
                        cmd_access.Connection.Close();
                        cmd_access.Connection.Open();
                    }
                    break;
                case "Oracle":
                    if (cmd_oracle.Connection.State == ConnectionState.Closed)
                    {
                        cmd_oracle.Connection.Open();
                    }
                    else if (cmd_oracle.Connection.State == ConnectionState.Broken)
                    {
                        cmd_oracle.Connection.Close();
                        cmd_oracle.Connection.Open();
                    }
                    break;
                default:
                    throw new Exception("无法识别的数据库类型!");
            }
        }

        public void close()
        {
            switch (dbtype)
            {
                case "Mssql":
                    if (cmd_mssql.Connection.State != ConnectionState.Closed)
                        cmd_mssql.Connection.Close();
                    break;
                case "Access":
                    if (cmd_access.Connection.State != ConnectionState.Closed)
                        cmd_access.Connection.Close();
                    break;
                case "Oracle":
                    if (cmd_oracle.Connection.State != ConnectionState.Closed)
                        cmd_oracle.Connection.Close();
                    break;
                default:
                    throw new Exception("无法识别的数据库类型!");
            }
        }

        public void begintrans()
        {
            switch (dbtype)
            {
                case "Mssql":
                    cmd_mssql.Transaction = cmd_mssql.Connection.BeginTransaction();
                    break;
                case "Access":
                    cmd_access.Transaction = cmd_access.Connection.BeginTransaction();
                    break;
                case "Oracle":
                    cmd_oracle.Transaction = cmd_oracle.Connection.BeginTransaction();
                    break;
                default:
                    throw new Exception("无法识别的数据库类型!");
            }
        }

        public void commit()
        {
            switch (dbtype)
            {
                case "Mssql":
                    cmd_mssql.Transaction.Commit();
                    break;
                case "Access":
                    cmd_access.Transaction.Commit();
                    break;
                case "Oracle":
                    cmd_oracle.Transaction.Commit();
                    break;
                default:
                    throw new Exception("无法识别的数据库类型!");
            }
        }

        public void rollback()
        {
            switch (dbtype)
            {
                case "Mssql":
                    cmd_mssql.Transaction.Rollback();
                    break;
                case "Access":
                    cmd_access.Transaction.Rollback();
                    break;
                case "Oracle":
                    cmd_oracle.Transaction.Rollback();
                    break;
                default:
                    throw new Exception("无法识别的数据库类型!");
            }
        }

        public string ExecuteScalar(string safeSql)
        {
            try
            {
                switch (dbtype)
                {
                    case "Mssql":
                        cmd_mssql.CommandText = safeSql;
                        return cmd_mssql.ExecuteScalar().ToString();
                    case "Access":
                        cmd_access.CommandText = safeSql;
                        return cmd_access.ExecuteScalar().ToString();
                    case "Oracle":
                        cmd_oracle.CommandText = safeSql;
                        return cmd_oracle.ExecuteScalar().ToString();
                    default:
                        throw new Exception("无法识别的数据库类型!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int ExecuteNonQuery(string safeSql)
        {
            try
            {
                open();
                switch (dbtype)
                {
                    case "Mssql":
                        cmd_mssql.CommandText = safeSql;
                        return cmd_mssql.ExecuteNonQuery();
                    case "Access":
                        cmd_access.CommandText = safeSql;
                        return cmd_access.ExecuteNonQuery();
                    case "Oracle":
                        cmd_oracle.CommandText = safeSql;
                        return cmd_oracle.ExecuteNonQuery();
                    default:
                        throw new Exception("无法识别的数据库类型!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable ReadDataTable(string safeSql)
        {
            try
            {
                DataTable dtResult = new DataTable();
                switch (dbtype)
                {
                    case "Mssql":
                        cmd_mssql.CommandText = safeSql;
                        SqlDataAdapter da_mssql = new SqlDataAdapter(cmd_mssql);
                        da_mssql.Fill(dtResult);
                        da_mssql.Dispose();
                        break;
                    case "Access":
                        cmd_access.CommandText = safeSql;
                        OleDbDataAdapter da_access = new OleDbDataAdapter(cmd_access);
                        da_access.Fill(dtResult);
                        da_access.Dispose();
                        break;
                    case "Oracle":
                        cmd_oracle.CommandText = safeSql;
                        DDTek.Oracle.OracleDataAdapter da_oracle = new DDTek.Oracle.OracleDataAdapter(cmd_oracle);
                        da_oracle.Fill(dtResult);
                        da_oracle.Dispose();
                        break;
                    default:
                        throw new Exception("无法识别的数据库类型!");
                }
                return dtResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int SaveDataTable(string safeSql, DataTable dt)
        {
            try
            {
                open();
                switch (dbtype)
                {
                    case "Mssql":
                        cmd_mssql.CommandText = safeSql;
                        SqlDataAdapter da_mssql = new SqlDataAdapter(cmd_mssql);
                        SqlCommandBuilder cb_mssql = new SqlCommandBuilder(da_mssql);
                        return da_mssql.Update(dt);
                    //case "Access":
                    //    cmd_access.CommandText = safeSql;
                    //    OleDbDataAdapter da_access = new OleDbDataAdapter(cmd_access);
                    //    OleDbCommandBuilder cb_access = new OleDbCommandBuilder(da_access);
                    //    return da_access.Update(dt);
                    case "Oracle":
                        cmd_oracle.CommandText = safeSql;
                        DDTek.Oracle.OracleDataAdapter da_oracle = new DDTek.Oracle.OracleDataAdapter(cmd_oracle);
                        DDTek.Oracle.OracleCommandBuilder cb_oracle = new DDTek.Oracle.OracleCommandBuilder(da_oracle);
                        return da_oracle.Update(dt);
                    default:
                        throw new Exception("无法识别的数据库类型!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 判断accesss某表是否存在某个字段
        /// </summary>
        /// <param name="sTblName"></param>
        /// <param name="sFldName"></param>
        /// <returns></returns>
        public bool checkField(String sTblName, String sFldName)
        {
            bool isExist = false;
            try
            {
                //OleDbConnection aConnection = new OleDbConnection(DB.getConnectStr());
                //aConnection.Open();

                object[] oa = { null, null, sTblName, sFldName };

                DataTable schemaTable = cmd_access.Connection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Columns, oa);
                if (schemaTable.Rows.Count == 0)
                {
                    isExist = false;
                }
                else
                {
                    isExist = true;
                }

            }
            catch (Exception)
            {
                isExist = false;
            }

            return isExist;
        }

        /// <summary>
        /// 为一个access表新增加一个字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public bool addColumnToTable(String tableName, String fieldName, String dataType)
        {
            //创建数据库连接

            bool f = true;
            String sqlAlter = "alter table " + tableName + " add column " + fieldName + " " + dataType + ";";
            try
            {
                cmd_access.CommandText = sqlAlter;
                cmd_access.ExecuteNonQuery();
            }
            catch (Exception)
            {
                f = false;
                throw new Exception(); 
            }
            return f;

        }

        /// <summary>
        /// 执行任意SQL语句
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public bool ExcuteSqlCommand(string strSql)
        {
            try
            {
                if (cmd_mssql.Transaction == null)
                {
                    open();
                }
                cmd_mssql.CommandText = strSql;

                cmd_mssql.ExecuteNonQuery();
                if (cmd_mssql.Transaction == null)
                {
                    close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 带参数执行任意SQL语句、可防止SQL注入
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public bool ExcuteSqlCommandwithParam(string strSql, SqlParameter[] paras)
        {
            try
            {
                if (cmd_mssql.Transaction == null)
                {
                    open();
                }
                cmd_mssql.CommandText = strSql;
                cmd_mssql.Parameters.Clear();
                cmd_mssql.Parameters.AddRange(paras);
                cmd_mssql.ExecuteNonQuery();
                if (cmd_mssql.Transaction == null)
                {
                    close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        /// <summary>
        /// 带参数执行查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public DataTable ExecuteSelectCommand(string sql, params SqlParameter[] values)
        {
            DataSet ds = new DataSet();
            try
            {
                if (cmd_mssql.Transaction == null)
                {
                    open();
                }
                cmd_mssql.CommandText = sql;
                cmd_mssql.Parameters.AddRange(values);
                SqlDataAdapter da = new SqlDataAdapter(cmd_mssql);
                da.Fill(ds);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                cmd_mssql.Connection.Close();
            }
            return ds.Tables[0];
        }

        /// <summary>
        /// 向数据库表DestTableName中批量插入（复制）数据
        /// </summary>
        /// <param name="dataTable">需要插入的数据，以表形式提供</param>
        /// <param name="DestTableName">需要插入的表名</param>
        /// <param name="DestTableColNames">dataTable各列对应DestTableName中的各列名</param>
        /// <param name="errorInfo">错误信息</param>
        /// <returns>是否插入成功</returns>
        public bool SqlBulkCopy(DataTable dataTable, string DestTableName, string[] DestTableColNames, ref string errorInfo)
        {
            try
            {
                if (dataTable.Columns.Count != DestTableColNames.Length)
                {
                    errorInfo = "数据表列名个数和目标表列名个数不一样多";
                    close();
                    return false;
                }
                //打开连接

                open();

                SqlBulkCopy sbc = new SqlBulkCopy(cmd_mssql.Connection);
                //字段映射。如果dataTable中的字段数和数据库表DestTableName中的字段数不一样多 一定要进行映射
                for (int colIndex = 0; colIndex < DestTableColNames.Length; colIndex++)
                {
                    sbc.ColumnMappings.Add(colIndex, DestTableColNames[colIndex]);
                }

                //每次加载数据的行数。不设置默认为全部，在本例中就可以不设置
                sbc.BulkCopyTimeout = 6000;
                sbc.BatchSize = 20000 / DestTableColNames.Length;

                //直接将数据导入目标数据表
                sbc.DestinationTableName = DestTableName;
                sbc.WriteToServer(dataTable);
                close();
            }
            catch (Exception ex)
            {
                errorInfo = ex.Message;
                return false;
            }
            return true;
        }

    }
}