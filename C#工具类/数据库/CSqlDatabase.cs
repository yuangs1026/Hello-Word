using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace HbzhwyApp
{
    class CSqlDatabase
    {
        private string ConnectionString; //数据库连接字符串
        private SqlConnection DatabaseCon; //数据库连接
        public SqlCommand tmpCommand;

        public CSqlDatabase(string ConStr)
        {
            ConnectionString = ConStr;
            DatabaseCon = new SqlConnection(ConnectionString);
            tmpCommand = DatabaseCon.CreateCommand();
        }
        public bool DatabaseConnect()
        {
            try
            {
                if (DatabaseCon.State == ConnectionState.Closed || DatabaseCon.State == ConnectionState.Broken)
                {
                    DatabaseCon.Close();
                    DatabaseCon.Open();
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message,"数据库连接错误");
                return false;
            }
            return true;
        }

        public void DatabaseClose()
        {
            DatabaseCon.Close();
        }

        //1
        public bool ReadTableData(DataTable theDataTable, string strSql)
        {
            try
            {
                if (tmpCommand.Transaction == null)
                {
                    DatabaseConnect();
                } 
                //SqlCommand tmpCommand = DatabaseCon.CreateCommand();
                tmpCommand.CommandText = strSql;

                SqlDataAdapter tmp_Adapter = new SqlDataAdapter(tmpCommand);
                tmp_Adapter.Fill(theDataTable);
                tmp_Adapter.Dispose();
                if (tmpCommand.Transaction == null)
                {
                    DatabaseClose();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误提示");
                return false;
            }
            
        }

        /// <summary>
        /// 保存气象要素定义到数据库中
        /// </summary>
        /// <param name="theDataTable"></param>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public bool SaveTableData(DataTable theDataTable, string strSql)
        {
            try
            {
                if (tmpCommand.Transaction == null)
                {
                    DatabaseConnect();
                } 
                //SqlCommand tmpCommand = DatabaseCon.CreateCommand();
                tmpCommand.CommandText = strSql;

                SqlDataAdapter tmp_Adapter = new SqlDataAdapter(tmpCommand);
                SqlCommandBuilder cb = new SqlCommandBuilder(tmp_Adapter);

                tmp_Adapter.Update(theDataTable);
                if (tmpCommand.Transaction == null)
                {
                    DatabaseClose();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误提示");
                return false;
            }
 
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
                if (tmpCommand.Transaction == null)
                {
                    DatabaseConnect();
                } 
                //SqlCommand tmpCommand = DatabaseCon.CreateCommand();
                tmpCommand.CommandText = strSql;

                tmpCommand.ExecuteNonQuery();
                if (tmpCommand.Transaction == null)
                {
                    DatabaseClose();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误提示");
                return false;
            }
        }

        /// <summary>
        /// 执行任意SQL语句
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>

        public bool ExcuteSqlCommandwithParam(string strSql,SqlParameter[] paras)
        {
            try
            {
                if (tmpCommand.Transaction == null)
                {
                    DatabaseConnect();
                } 
                //SqlCommand tmpCommand = DatabaseCon.CreateCommand();
                tmpCommand.CommandText = strSql;
                tmpCommand.Parameters.Clear();
                tmpCommand.Parameters.AddRange(paras);
                tmpCommand.ExecuteNonQuery();
                if (tmpCommand.Transaction == null)
                {
                    DatabaseClose();
                }
                return true;

                /*
                  string sql = "UPDATE CONTRACTS SET CONTRACT_FILE=@CONTRACT_FILE WHERE ID=@ID";
10             using (SqlConnection conn = new SqlConnection(this.m_DataAccess.ConnectString))
11             {
12                 conn.Open();
13                 using (SqlCommand cmd = new SqlCommand())
14                 {
15                     cmd.Connection = conn;
16                     cmd.CommandText = sql;
17                     cmd.Parameters.Clear();
18 
19                     cmd.Parameters.Add(new SqlParameter("@CONTRACT_FILE", SqlDbType.Image));
20                     cmd.Parameters["@CONTRACT_FILE"].Value = fileBytes;
21 
22                     cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.VarChar));
23                     cmd.Parameters["@ID"].Value = id;
24 
25                     return cmd.ExecuteNonQuery() > 0 ? true : false;
26                 }
27             }
                 */
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误提示");
                return false;
            }

        }

        /// <summary>
        /// 取出查询结果的第一个行第一个字段
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public string ExecuteScalar(string strSql)
        {
            try
            {
                if (tmpCommand.Transaction == null)
                {
                    DatabaseConnect();
                } 
                //SqlCommand tmpCommand = DatabaseCon.CreateCommand();
                tmpCommand.CommandText = strSql;
                string ls_return=tmpCommand.ExecuteScalar().ToString();
                if (tmpCommand.Transaction == null)
                {
                    DatabaseClose();
                }
                return ls_return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误提示");
                return "";
            }

        }

        /// <summary>
        /// 开启事务
        /// </summary>
        public bool begintrans()
        {
            try
            {
                DatabaseConnect();
                //SqlCommand tmpCommand = DatabaseCon.CreateCommand();
                tmpCommand.Transaction = tmpCommand.Connection.BeginTransaction();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("事务开启失败！\r\n"+ex.Message, "错误提示");
                return false;
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public bool commit()
        {
            try
            {
                //SqlCommand tmpCommand = DatabaseCon.CreateCommand();
                tmpCommand.Transaction.Commit();
                DatabaseClose();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("事务提交失败！\r\n" + ex.Message, "错误提示");
                return false;
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public bool rollback()
        {
            try
            {
                //SqlCommand tmpCommand = DatabaseCon.CreateCommand();
                tmpCommand.Transaction.Rollback();
                DatabaseClose();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("事务回滚失败！\r\n" + ex.Message, "错误提示");
                return false;
            }
        }

        public DataTable ExecuteSelectCommand(string sql, params SqlParameter[] values)
        {
            DataSet ds = new DataSet();
            tmpCommand = DatabaseCon.CreateCommand();
            try
            {
                if (tmpCommand.Transaction == null)
                {
                    DatabaseConnect();
                }
                tmpCommand.CommandText = sql;
                tmpCommand.Parameters.AddRange(values);
                SqlDataAdapter da = new SqlDataAdapter(tmpCommand);
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误提示");
            }
            finally
            {
                tmpCommand.Connection.Close();
            }
            return ds.Tables[0];
        }
    }
}
