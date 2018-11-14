using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
namespace SQLHelper
{
    class SQL_DAL
    {
        static private string connStr = "Data Source=127.0.0.1;Initial Catalog=AOI_DB;Persist Security Info=True;User ID=sa;Password=Qwerty123456";
        /// <summary>
        /// 执行无参数无返回SQL查询，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="isProc">false:sql语句;true:存储过程</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteNonQuery(string SQLString, bool isProc)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, conn))
                {
                    try
                    {
                        if (isProc)//假如是存储过程
                            cmd.CommandType = CommandType.StoredProcedure;
                        if (conn.State.Equals(ConnectionState.Closed))
                            conn.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException err)
                    {
                        throw new Exception(err.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行有参数无返回查询，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL</param>
        /// <param name="isProc">false:sql语句;true:存储过程</param>
        /// <param name="prams">参数</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteNonQuery(string SQLString, bool isProc, SqlParameter[] prams)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, conn))
                {
                    try
                    {
                        if (isProc)//假如是存储过程
                            cmd.CommandType = CommandType.StoredProcedure;

                        foreach (SqlParameter item in prams)//添加参数
                        {

                            cmd.Parameters.Add(item);
                        }
                        if (conn.State.Equals(ConnectionState.Closed))
                            conn.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException err)
                    {
                        throw new Exception(err.Message);
                    }
                    finally
                    {
                        cmd.Parameters.Clear();
                        cmd.Dispose();
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行无参数有返回查询语句，返回SqlDataReader
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="isProc">是否是存储过程false:sql语句;true:存储过程</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string SQLString, bool isProc)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlDataReader dr = null;
            using (SqlCommand cmd = new SqlCommand(SQLString, conn))
            {
                try
                {
                    if (conn.State.Equals(ConnectionState.Closed))
                        conn.Open();
                    if (isProc)//是否存储过程
                        cmd.CommandType = CommandType.StoredProcedure;
                    dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    return dr;
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    cmd.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行有参数有返回查询语句，返回SqlDataReader
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="isProc">是否是存储过程false:sql语句;true:存储过程</param>
        /// <param name="prams">参数</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string SQLString, bool isProc, SqlParameter[] prams)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlDataReader dr = null;
            using (SqlCommand cmd = new SqlCommand(SQLString, conn))
            {
                try
                {
                    if (conn.State.Equals(ConnectionState.Closed))
                        conn.Open();
                    if (isProc)//是否是存储过程
                        cmd.CommandType = CommandType.StoredProcedure;
                    foreach (SqlParameter item in prams)//添加参数
                    {
                        cmd.Parameters.Add(item);
                    }
                    dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    return dr;
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行无参数有返回一条查询语句，返回object
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="isProc">是否是存储过程false:sql语句;true:存储过程</param>
        /// <returns>object</returns>
        public static object ExecuteScalar(string SQLString, bool isProc)
        {
            SqlConnection conn = new SqlConnection(connStr);
            using (SqlCommand cmd = new SqlCommand(SQLString, conn))
            {
                try
                {
                    if (conn.State.Equals(ConnectionState.Closed))
                        conn.Open();
                    if (isProc)//是否存储过程
                        cmd.CommandType = CommandType.StoredProcedure;
                    return cmd.ExecuteScalar();
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    cmd.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行有参数有返回一条查询语句，返回object
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="isProc">是否是存储过程false:sql语句;true:存储过程</param>
        /// <param name="prams">参数</param>
        /// <returns>object</returns>
        public static object ExecuteScalar(string SQLString, bool isProc, SqlParameter[] prams)
        {
            SqlConnection conn = new SqlConnection(connStr);
            using (SqlCommand cmd = new SqlCommand(SQLString, conn))
            {
                try
                {
                    if (conn.State.Equals(ConnectionState.Closed))
                        conn.Open();
                    if (isProc)//是否是存储过程
                        cmd.CommandType = CommandType.StoredProcedure;
                    foreach (SqlParameter item in prams)//添加参数
                    {
                        cmd.Parameters.Add(item);
                    }
                    return cmd.ExecuteScalar();
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行无参数查询语句，返回DataTable
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="isProc">是否存储过程0:sql语句;1:存储过程</param>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable(string SQLString, bool isProc)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                if (conn.State.Equals(ConnectionState.Closed))
                    conn.Open();
                SqlDataAdapter sa = null;
                try
                {
                    if (isProc)//如果是存储过程
                        sa.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sa = new SqlDataAdapter(SQLString, conn);
                    sa.Fill(dt);

                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    sa.Dispose();
                    conn.Close();
                }
            }
            return dt;
        }

        /// <summary>
        /// 执行有参数查询语句，返回DataTable
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="isProc">是否存储过程0:sql语句;1:存储过程</param>
        /// <param name="prams">参数</param>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable(string SQLString, bool isProc, SqlParameter[] prams)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                if (conn.State.Equals(ConnectionState.Closed))
                    conn.Open();
                SqlDataAdapter sa = null;
                try
                {
                    if (isProc)//如果是存储过程
                        sa.SelectCommand.CommandType = CommandType.StoredProcedure;
                    foreach (SqlParameter item in prams)//添加参数
                    {
                        sa.SelectCommand.Parameters.Add(item);
                    }
                    sa = new SqlDataAdapter(SQLString, conn);
                    sa.Fill(dt);
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    sa.SelectCommand.Parameters.Clear();
                    sa.Dispose();
                    conn.Close();
                }
            }
            return dt;
        }



        /// <summary>
        /// 执行无参数查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="isProc">是否存储过程0:sql语句;1:存储过程</param>
        /// <param name="table">填充的表</param>
        /// <returns>DataSet</returns>
        public static DataSet GetDataSet(string SQLString, bool isProc, string table)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlDataAdapter da = null;
                try
                {
                    if (conn.State.Equals(ConnectionState.Closed))
                        conn.Open();
                    da = new SqlDataAdapter(SQLString, conn);
                    if (isProc)//假如是存储过程
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.Fill(ds, table);
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    da.Dispose();
                    conn.Close();
                }
            }
            return ds;
        }

        /// <summary>
        /// 执行无参数查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <param name="isProc">是否存储过程0:sql语句;1:存储过程</param>
        /// <param name="prams">参数</param>
        /// <param name="table">填充的表</param>
        /// <returns>DataSet</returns>
        public static DataSet GetDataSet(string SQLString, bool isProc, SqlParameter[] prams, string table)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlDataAdapter da = null;
                try
                {
                    if (conn.State.Equals(ConnectionState.Closed))
                        conn.Open();
                    da = new SqlDataAdapter(SQLString, conn);
                    if (isProc)
                        da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    foreach (SqlParameter item in prams)//添加参数
                    {
                        da.SelectCommand.Parameters.Add(item);
                    }
                    da.Fill(ds, table);
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    da.SelectCommand.Parameters.Clear();
                    da.Dispose();
                    conn.Close();
                }
            }
            return ds;
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        /// <param name="isProc">是否是存储过程0:sql语句;1:存储过程</param>
        /// <param name="prams">参数</param>
        public static void ExecuteSqlTran(string[] SQLStringList, bool isProc, SqlParameter[] prams)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                if (conn.State.Equals(ConnectionState.Closed))
                    conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                SqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Length; n++)
                    {
                        cmd.CommandText = SQLStringList[n];
                        if (isProc)
                            cmd.CommandType = CommandType.StoredProcedure;
                        foreach (SqlParameter item in prams)//添加参数
                        {
                            cmd.Parameters.Add(item);
                        }
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                }
                finally
                {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                    conn.Close();
                }
            }
        }
    }
}
