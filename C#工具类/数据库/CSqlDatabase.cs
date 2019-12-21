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
        private string ConnectionString; //���ݿ������ַ���
        private SqlConnection DatabaseCon; //���ݿ�����
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
                MessageBox.Show(e.Message,"���ݿ����Ӵ���");
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
                MessageBox.Show(ex.Message, "������ʾ");
                return false;
            }
            
        }

        /// <summary>
        /// ��������Ҫ�ض��嵽���ݿ���
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
                MessageBox.Show(ex.Message, "������ʾ");
                return false;
            }
 
        }

        /// <summary>
        /// ִ������SQL���
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
                MessageBox.Show(ex.Message, "������ʾ");
                return false;
            }
        }

        /// <summary>
        /// ִ������SQL���
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
                MessageBox.Show(ex.Message, "������ʾ");
                return false;
            }

        }

        /// <summary>
        /// ȡ����ѯ����ĵ�һ���е�һ���ֶ�
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
                MessageBox.Show(ex.Message, "������ʾ");
                return "";
            }

        }

        /// <summary>
        /// ��������
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
                MessageBox.Show("������ʧ�ܣ�\r\n"+ex.Message, "������ʾ");
                return false;
            }
        }

        /// <summary>
        /// �ύ����
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
                MessageBox.Show("�����ύʧ�ܣ�\r\n" + ex.Message, "������ʾ");
                return false;
            }
        }

        /// <summary>
        /// �ع�����
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
                MessageBox.Show("����ع�ʧ�ܣ�\r\n" + ex.Message, "������ʾ");
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
                MessageBox.Show(ex.Message, "������ʾ");
            }
            finally
            {
                tmpCommand.Connection.Close();
            }
            return ds.Tables[0];
        }
    }
}
