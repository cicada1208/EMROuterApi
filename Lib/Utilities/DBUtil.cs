using AdoNetCore.AseClient;
using Dapper;
using Lib.Attributes;
using Lib.Models;
using Lib.Utilities;
using Microsoft.Extensions.Configuration;
//using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Lib.Utilities
{
    public class DBUtil
    {
        /// <summary>
        /// DB類型
        /// </summary>
        public DBType DBType { get; } = DBType.SQLSERVER;

        /// <summary>
        /// DB名稱
        /// </summary>
        public string DBName { get; } = string.Empty;

        /// <summary>
        /// DB Connection String
        /// </summary>
        public string ConnectionString { get; } = string.Empty;

        public DBUtil(string dbName, DBType dbType)
        {
            DBType = dbType;
            DBName = dbName;
            ConnectionString = GetConnString(dbName);
            SqlMapper.AddTypeMap(typeof(string), DbType.AnsiString);
        }

        /// <summary>
        /// Get Connnection String By DBName
        /// </summary>
        public static string GetConnString(string dbName)
        {
            IConfigurationRoot configuration = ConfigUtil.GetAppSettings();
            string connString = configuration.GetConnectionString(dbName) ?? string.Empty;
            return connString;
        }

        /// <summary>
        /// 建立Connection：依Command
        /// </summary>
        public IDbConnection CreateConn(IDbCommand cmd, bool openState = false)
        {
            IDbConnection conn;

            if (cmd.GetType() == typeof(AseCommand))
                conn = new AseConnection(ConnectionString);
            //else if (cmd.GetType() == typeof(OracleCommand))
            //    conn = new OracleConnection(ConnectionString);
            else
                conn = new SqlConnection(ConnectionString);

            if (openState && conn.State == ConnectionState.Closed) conn.Open();

            return conn;
        }

        /// <summary>
        /// 建立Connection：依DBType
        /// </summary>
        public IDbConnection CreateConn(DBType dbType, bool openState = false)
        {
            IDbConnection conn = dbType switch
            {
                DBType.SYBASE => new AseConnection(ConnectionString),
                //DBType.ORACLE => new OracleConnection(ConnectionString),
                _ => new SqlConnection(ConnectionString),
            };

            if (openState && conn.State == ConnectionState.Closed) conn.Open();

            return conn;
        }

        /// <summary>
        /// 建立Connection：依DBName、DBType
        /// </summary>
        public IDbConnection CreateConn(string dbName, DBType dbType, bool openState = false)
        {
            string connString = GetConnString(dbName);

            IDbConnection conn = dbType switch
            {
                DBType.SYBASE => new AseConnection(connString),
                //DBType.ORACLE => new OracleConnection(connString),
                _ => new SqlConnection(connString),
            };

            if (openState && conn.State == ConnectionState.Closed) conn.Open();

            return conn;
        }

        public DataTable? ExecQuery(IDbCommand cmd, DataTable? dtData = null, int cmdTimeout = 30,
            TrimType trimType = TrimType.TrimEnd, bool nullToEmpty = true)
        {
            IDbConnection? conn = null;
            IDataReader? reader = null;
            DataTable? dtSchema = null;

            try
            {
                conn = CreateConn(cmd, true);
                cmd.Connection = conn;
                cmd.CommandTimeout = cmdTimeout;
                reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
                dtSchema = reader.GetSchemaTable();
                if (dtData != null)
                {
                    dtData.Load(reader);
                    if (trimType != TrimType.None || nullToEmpty)
                        dtData = dtData.StrProcess(trimType, nullToEmpty);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(cmd), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
                cmd?.Dispose();
                reader?.Close(); reader?.Dispose();
            }

            return dtSchema;
        }

        public DataTable? ExecQuery(string sql, DataTable? dtData = null, int cmdTimeout = 30,
            TrimType trimType = TrimType.TrimEnd, bool nullToEmpty = true)
        {
            IDbConnection? conn = null;
            IDbCommand? cmd = null;
            IDataReader? reader = null;
            DataTable? dtSchema = null;

            try
            {
                conn = CreateConn(DBName, DBType, true);
                cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = cmdTimeout;
                reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
                dtSchema = reader.GetSchemaTable();
                if (dtData != null)
                {
                    dtData.Load(reader);
                    if (trimType != TrimType.None || nullToEmpty)
                        dtData = dtData.StrProcess(trimType, nullToEmpty);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(sql), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
                cmd?.Dispose();
                reader?.Close(); reader?.Dispose();
            }

            return dtSchema;
        }

        public string ExecuteScalar(IDbCommand cmd, int cmdTimeout = 30)
        {
            IDbConnection? conn = null;
            string result = string.Empty;

            try
            {
                conn = CreateConn(cmd, true);
                cmd.Connection = conn;
                cmd.CommandTimeout = cmdTimeout;

                // cmd.ExecuteScalar() is null: ?? => string.Empty
                // cmd.ExecuteScalar() is DBNull.Value: .ToString() => string.Empty
                result = (cmd.ExecuteScalar() ?? string.Empty).ToString()!;
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(cmd), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
                cmd?.Dispose();
            }

            return result;
        }

        public bool ExecuteNonQuery(IDbCommand cmd, int cmdTimeout = 30)
        {
            IDbConnection? conn = null;
            IDbTransaction? tran = null;
            bool succ = true;

            try
            {
                conn = CreateConn(cmd, true);
                tran = conn.BeginTransaction();
                cmd.Connection = conn;
                cmd.CommandTimeout = cmdTimeout;
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
                tran.Commit();
                succ = true;
            }
            catch (Exception ex)
            {
                if (tran != null) tran.Rollback();
                succ = false;
                throw new Exception(LogSql(cmd), ex);
            }
            finally
            {
                tran?.Dispose();
                conn?.Close(); conn?.Dispose();
                cmd?.Dispose();
            }

            return succ;
        }

        public bool ExecuteNonQuery(IEnumerable<IDbCommand> cmds, int cmdTimeout = 30)
        {
            IDbConnection? conn = null;
            IDbTransaction? tran = null;
            bool succ = true;

            try
            {
                conn = CreateConn(cmds.First(), true);
                tran = conn.BeginTransaction();
                foreach (var cmd in cmds)
                {
                    cmd.Connection = conn;
                    cmd.CommandTimeout = cmdTimeout;
                    cmd.Transaction = tran;
                    cmd.ExecuteNonQuery();
                }
                tran.Commit();
                succ = true;
            }
            catch (Exception ex)
            {
                if (tran != null) tran.Rollback();
                succ = false;
                throw new Exception(LogSql(cmds), ex);
            }
            finally
            {
                tran?.Dispose();
                conn?.Close(); conn?.Dispose();
                if (cmds != null)
                {
                    foreach (var cmd in cmds)
                        cmd?.Dispose();
                }
            }

            return succ;
        }

        public bool ExecuteNonQueryWithAffected(IDbCommand cmd, ref string msg, int cmdTimeout = 30)
        {
            IDbConnection? conn = null;
            IDbTransaction? tran = null;
            bool succ = true;
            int rowsAffected = 0;

            try
            {
                conn = CreateConn(cmd, true);
                tran = conn.BeginTransaction();
                cmd.Connection = conn;
                cmd.CommandTimeout = cmdTimeout;
                cmd.Transaction = tran;
                rowsAffected = cmd.ExecuteNonQuery();
                tran.Commit();

                succ = rowsAffected == 0 ? false : true;
                msg = rowsAffected.ToString() + " Row Affected！";
            }
            catch (Exception ex)
            {
                if (tran != null) tran.Rollback();
                succ = false;
                throw new Exception(LogSql(cmd), ex);
            }
            finally
            {
                tran?.Dispose();
                conn?.Close(); conn?.Dispose();
                cmd?.Dispose();
            }

            return succ;
        }

        /// <summary>
        /// 取得DB類型
        /// </summary>
        public static DBType GetDBType<TModel>()
        {
            Attribute? attr = Attribute.GetCustomAttribute(typeof(TModel), typeof(TableAttribute), false);
            DBType dbType = (attr as dynamic)?.DBType ?? DBType.SQLSERVER;
            return dbType;
        }

        /// <summary>
        /// 取得DB類型(Nullable)
        /// </summary>
        public static DBType? GetDBTypeNullable<TModel>()
        {
            Attribute? attr = Attribute.GetCustomAttribute(typeof(TModel), typeof(TableAttribute), false);
            DBType? dbType = (attr as dynamic)?.DBType;
            return dbType;
        }

        /// <summary>
        /// 取得DB名稱
        /// </summary>
        public static string GetDBName<TModel>()
        {
            Attribute? attr = Attribute.GetCustomAttribute(typeof(TModel), typeof(TableAttribute), false);
            var dbName = (attr as dynamic)?.DBName ?? string.Empty;
            return dbName;
        }

        /// <summary>
        /// 取得表格名稱
        /// </summary>
        public static string GetTableName<TModel>()
        {
            Attribute? attr = Attribute.GetCustomAttribute(typeof(TModel), typeof(TableAttribute), false);
            var tableName = (attr as dynamic)?.TableName ?? string.Empty;
            return tableName;
        }

        /// <summary>
        /// 取得表格名稱
        /// </summary>
        public static string GetTableName(Type type)
        {
            Attribute? attr = Attribute.GetCustomAttribute(type, typeof(TableAttribute), false);
            var tableName = (attr as dynamic)?.TableName ?? string.Empty;
            return tableName;
        }

        /// <summary>
        /// Dapper Query：自動建立條件相等查詢，若無 <paramref name="param"/> 則 select all
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName、TableName，具有定義 TableAttribute</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        public IEnumerable<TModel> Query<TModel>(object? param = null, int? cmdTimeout = null,
            TrimType trimType = TrimType.TrimEnd, bool nullToEmpty = true) =>
            QueryAsync<TModel>(param, cmdTimeout, trimType, nullToEmpty).Result;

        /// <summary>
        /// Dapper Query：自動建立條件相等查詢，若無 <paramref name="param"/> 則 select all
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName、TableName，具有定義 TableAttribute</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        public async Task<IEnumerable<TModel>> QueryAsync<TModel>(object? param = null, int? cmdTimeout = null,
            TrimType trimType = TrimType.TrimEnd, bool nullToEmpty = true)
        {
            IDbConnection? conn = null;
            SqlCmd? sqlCmd = null;
            IEnumerable<TModel>? result = null;

            try
            {
                //string tableName = GetTableName<TModel>();
                //SqlBuildUtil sqlBuild = new SqlBuildUtil(tableName);
                SqlBuildUtil sqlBuild = new SqlBuildUtil();
                sqlCmd = sqlBuild.Select(typeof(TModel), param);
                DBType dbType = GetDBType<TModel>();
                string dbName = GetDBName<TModel>();
                conn = CreateConn(dbName, dbType);
                result = await conn.QueryAsync<TModel>(sqlCmd.Sql, sqlCmd.Param, commandTimeout: cmdTimeout);
                if (trimType != TrimType.None || nullToEmpty)
                    result = result.ToList().StrProcess(trimType, nullToEmpty);
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(sqlCmd), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Dapper Query：自訂 sql 查詢
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName，若 TModel 無定義則取自建構子定義</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        public IEnumerable<TModel> Query<TModel>(string sql, object? param = null, int? cmdTimeout = null,
            TrimType trimType = TrimType.TrimEnd, bool nullToEmpty = true) =>
            QueryAsync<TModel>(sql, param, cmdTimeout, trimType, nullToEmpty).Result;

        /// <summary>
        /// Dapper Query：自訂 sql 查詢
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName，若 TModel 無定義則取自建構子定義</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        public async Task<IEnumerable<TModel>> QueryAsync<TModel>(string sql, object? param = null, int? cmdTimeout = null,
            TrimType trimType = TrimType.TrimEnd, bool nullToEmpty = true)
        {
            IDbConnection? conn = null;
            IEnumerable<TModel>? result = null;

            try
            {
                DBType? dbType = GetDBTypeNullable<TModel>();
                if (dbType == null) dbType = DBType;
                string dbName = GetDBName<TModel>();
                if (dbName == string.Empty) dbName = DBName;
                conn = CreateConn(dbName, (DBType)dbType);
                result = await conn.QueryAsync<TModel>(sql, param, commandTimeout: cmdTimeout);
                if (trimType != TrimType.None || nullToEmpty)
                    result = result.ToList().StrProcess(trimType, nullToEmpty);
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(sql, param), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Dapper Query：自訂 sql 查詢  data 及 table schema
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="param">未必定義 TableAttribute</param>
        /// <param name="dtData">資料</param>
        /// <returns>table schema</returns>
        public DataTable? Query(string sql, object? param = null, DataTable? dtData = null, int? cmdTimeout = null,
            TrimType trimType = TrimType.TrimEnd, bool nullToEmpty = true) =>
            QueryAsync(sql, param, dtData, cmdTimeout, trimType, nullToEmpty).Result;

        /// <summary>
        /// Dapper Query：自訂 sql 查詢 data 及 table schema
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="param">未必定義 TableAttribute</param>
        /// <param name="dtData">資料</param>
        /// <returns>table schema</returns>
        public async Task<DataTable?> QueryAsync(string sql, object? param = null, DataTable? dtData = null, int? cmdTimeout = null,
            TrimType trimType = TrimType.TrimEnd, bool nullToEmpty = true)
        {
            IDbConnection? conn = null;
            CommandDefinition cmd;
            IDataReader? reader = null;
            DataTable? dtSchema = null;

            try
            {
                conn = CreateConn(DBName, DBType);
                //reader = conn.ExecuteReader(sql, param, commandTimeout: cmdTimeout);
                cmd = new CommandDefinition(sql, param, commandTimeout: cmdTimeout);
                reader = await conn.ExecuteReaderAsync(cmd, CommandBehavior.KeyInfo); // 不只primary key，所有key皆會回傳
                dtSchema = reader.GetSchemaTable();
                if (dtData != null)
                {
                    dtData.Load(reader);
                    if (trimType != TrimType.None || nullToEmpty)
                        dtData = dtData.StrProcess(trimType, nullToEmpty);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(sql, param), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
                reader?.Close(); reader?.Dispose();
            }

            return dtSchema;
        }

        /// <summary>
        /// Dapper Execute Insert：自動建立新增
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName、TableName，具有定義 TableAttribute</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        public int Insert<TModel>(object param, int? cmdTimeout = null) =>
            InsertAsync<TModel>(param, cmdTimeout).Result;

        /// <summary>
        /// Dapper Execute Insert：自動建立新增
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName、TableName，具有定義 TableAttribute</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        public async Task<int> InsertAsync<TModel>(object param, int? cmdTimeout = null)
        {
            IDbConnection? conn = null;
            SqlCmd? sqlCmd = null;
            int rowsAffected = 0;

            try
            {
                //string tableName = GetTableName<TModel>();
                //SqlBuildUtil sqlBuild = new SqlBuildUtil(tableName);
                SqlBuildUtil sqlBuild = new SqlBuildUtil();
                sqlCmd = sqlBuild.Insert(typeof(TModel), param);
                DBType dbType = GetDBType<TModel>();
                string dbName = GetDBName<TModel>();
                conn = CreateConn(dbName, dbType);
                rowsAffected = await conn.ExecuteAsync(sqlCmd.Sql, sqlCmd.Param, commandTimeout: cmdTimeout);
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(sqlCmd), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Dapper Execute Update：自動建立整筆更新
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName、TableName，具有定義 TableAttribute</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        public int Update<TModel>(object param, int? cmdTimeout = null) =>
            UpdateAsync<TModel>(param, cmdTimeout).Result;

        /// <summary>
        /// Dapper Execute Update：自動建立整筆更新
        /// </summary>
        /// <typeparam name="TModel">依此查詢DBType、DBName、TableName，具有定義 TableAttribute</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        public async Task<int> UpdateAsync<TModel>(object param, int? cmdTimeout = null)
        {
            IDbConnection? conn = null;
            SqlCmd? sqlCmd = null;
            int rowsAffected = 0;

            try
            {
                //string tableName = GetTableName<TModel>();
                //SqlBuildUtil sqlBuild = new SqlBuildUtil(tableName);
                SqlBuildUtil sqlBuild = new SqlBuildUtil();
                sqlCmd = sqlBuild.Update(typeof(TModel), param);
                DBType dbType = GetDBType<TModel>();
                string dbName = GetDBName<TModel>();
                conn = CreateConn(dbName, dbType);
                rowsAffected = await conn.ExecuteAsync(sqlCmd.Sql, sqlCmd.Param, commandTimeout: cmdTimeout);
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(sqlCmd), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Dapper Execute Patch：自動建立部份欄位更新
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName、TableName，具有定義 TableAttribute</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        /// <param name="updateCol">update 欄位</param>
        public int Patch<TModel>(object param, HashSet<string> updateCol, int? cmdTimeout = null) =>
            PatchAsync<TModel>(param, updateCol, cmdTimeout).Result;

        /// <summary>
        /// Dapper Execute Patch：自動建立部份欄位更新
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName、TableName，具有定義 TableAttribute</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        /// <param name="updateCol">update 欄位</param>
        public async Task<int> PatchAsync<TModel>(object param, HashSet<string> updateCol, int? cmdTimeout = null)
        {
            IDbConnection? conn = null;
            SqlCmd? sqlCmd = null;
            int rowsAffected = 0;

            try
            {
                //string tableName = GetTableName<TModel>();
                //SqlBuildUtil sqlBuild = new SqlBuildUtil(tableName);
                SqlBuildUtil sqlBuild = new SqlBuildUtil();
                sqlCmd = sqlBuild.Patch(typeof(TModel), param, updateCol);
                DBType dbType = GetDBType<TModel>();
                string dbName = GetDBName<TModel>();
                conn = CreateConn(dbName, dbType);
                rowsAffected = await conn.ExecuteAsync(sqlCmd.Sql, sqlCmd.Param, commandTimeout: cmdTimeout);
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(sqlCmd), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Dapper Execute Delete：自動建立刪除
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName、TableName，具有定義 TableAttribute</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        public int Delete<TModel>(object param, int? cmdTimeout = null) =>
            DeleteAsync<TModel>(param, cmdTimeout).Result;

        /// <summary>
        /// Dapper Execute Delete：自動建立刪除
        /// </summary>
        /// <typeparam name="TModel">依此查詢 DBType、DBName、TableName，具有定義 TableAttribute</typeparam>
        /// <param name="param">未必定義 TableAttribute</param>
        public async Task<int> DeleteAsync<TModel>(object param, int? cmdTimeout = null)
        {
            IDbConnection? conn = null;
            SqlCmd? sqlCmd = null;
            int rowsAffected = 0;

            try
            {
                //string tableName = GetTableName<TModel>();
                //SqlBuildUtil sqlBuild = new SqlBuildUtil(tableName);
                SqlBuildUtil sqlBuild = new SqlBuildUtil();
                sqlCmd = sqlBuild.Delete(typeof(TModel), param);
                DBType dbType = GetDBType<TModel>();
                string dbName = GetDBName<TModel>();
                conn = CreateConn(dbName, dbType);
                rowsAffected = await conn.ExecuteAsync(sqlCmd.Sql, sqlCmd.Param, commandTimeout: cmdTimeout);
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(sqlCmd), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Dapper Execute Insert、Update、Delete：自訂 sql
        /// </summary>
        /// <param name="param">entity, 未必定義 TableAttribute</param>
        public int Execute(string sql, object? param = null, int? cmdTimeout = null) =>
            ExecuteAsync(sql, param, cmdTimeout).Result;

        /// <summary>
        /// Dapper Execute Insert、Update、Delete：自訂 sql
        /// </summary>
        /// <param name="param">entity, 未必定義 TableAttribute</param>
        public async Task<int> ExecuteAsync(string sql, object? param = null, int? cmdTimeout = null)
        {
            IDbConnection? conn = null;
            int rowsAffected = 0;

            try
            {
                conn = CreateConn(DBName, DBType);
                rowsAffected = await conn.ExecuteAsync(sql, param, commandTimeout: cmdTimeout);
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(sql, param), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Dapper Execute Insert、Update、Delete：批次
        /// </summary>
        public int Execute(IEnumerable<SqlCmd> sqlCmds, int? cmdTimeout = null, bool isTransaction = true) =>
            ExecuteAsync(sqlCmds, cmdTimeout, isTransaction).Result;

        /// <summary>
        /// Dapper Execute Insert、Update、Delete：批次
        /// </summary>
        public async Task<int> ExecuteAsync(IEnumerable<SqlCmd> sqlCmds, int? cmdTimeout = null, bool isTransaction = true)
        {
            IDbConnection? conn = null;
            int rowsAffected = 0;

            try
            {
                conn = CreateConn(DBName, DBType, true);
                using (var tran = isTransaction ? conn.BeginTransaction() : null)
                {
                    foreach (var cmd in sqlCmds)
                        rowsAffected += await conn.ExecuteAsync(cmd.Sql, cmd.Param, tran, cmdTimeout);
                    tran?.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(LogSql(sqlCmds), ex);
            }
            finally
            {
                conn?.Close(); conn?.Dispose();
            }

            return rowsAffected;
        }

        public static string LogSql(IDbCommand? cmd)
        {
            string sql = cmd?.CommandText ?? string.Empty;
            string paramName;
            object paramValue;

            try
            {
                if (cmd?.Parameters != null)
                {
                    foreach (IDbDataParameter p in cmd.Parameters)
                    {
                        paramName = p.ParameterName.StartsWith("@") ? p.ParameterName : "@" + p.ParameterName;
                        paramValue = p.Value ?? "null";
                        //if (paramValue.GetType() == typeof(string))
                        //    paramValue = "'" + (paramValue ?? string.Empty).ToString().Replace("'", "''") + "'";
                        if (paramValue.GetType() == typeof(string) && paramValue.ToString() != "null")
                            paramValue = "'" + paramValue.ToString()?.Replace("'", "''") + "'";
                        else if (paramValue.GetType() == typeof(DateTime) && paramValue.ToString() != "null")
                            paramValue = "'" + ((DateTime)paramValue).ToString("yyyy-MM-ddTHH:mm:ss") + "'";
                        sql = Regex.Replace(sql, @"\B" + paramName + @"\b", paramValue.ToString()!);
                    }
                }
            }
            catch (Exception) { }

            sql += Environment.NewLine;
            return sql;
        }

        public static string LogSql(IEnumerable<IDbCommand>? cmds)
        {
            string sqls = string.Empty;

            try
            {
                if (cmds != null)
                {
                    foreach (var cmd in cmds)
                        sqls += LogSql(cmd);
                }
            }
            catch (Exception) { }

            return sqls;
        }

        public static string LogSql(string sql, object? entity = null)
        {
            //string paramName = string.Empty;
            object paramValue;

            try
            {
                if (entity != null)
                {
                    //foreach (var p in entity.GetType().GetProperties())
                    //{
                    //    paramName = p.Name.StartsWith("@") ? p.Name : "@" + p.Name;
                    //    paramValue = p.GetValue(entity) ?? "null";
                    //    if (paramValue.GetType() == typeof(string) && paramValue.ToString() != "null")
                    //        paramValue = "'" + paramValue.ToString().Replace("'", "''") + "'";
                    //    sql = Regex.Replace(sql, @"\B" + paramName + @"\b", paramValue.ToString());
                    //}
                    string pattern = @"\@[\w\.\$]+";
                    var paramNames = Regex.Matches(sql, pattern).Cast<Match>().Select(m => m.Value).Distinct().ToArray();
                    foreach (var paramName in paramNames)
                    {
                        paramValue = entity.GetType().GetProperty(paramName.TrimStart('@'))?.GetValue(entity) ?? "null";
                        if (paramValue.GetType() == typeof(string) && paramValue.ToString() != "null")
                            paramValue = "'" + paramValue.ToString()?.Replace("'", "''") + "'";
                        else if (paramValue.GetType() == typeof(DateTime) && paramValue.ToString() != "null")
                            paramValue = "'" + ((DateTime)paramValue).ToString("yyyy-MM-ddTHH:mm:ss") + "'";
                        sql = Regex.Replace(sql, @"\B" + paramName + @"\b", paramValue.ToString()!);
                    }
                }
            }
            catch (Exception) { }

            sql += Environment.NewLine;
            return sql;
        }

        public static string LogSql(SqlCmd? cmd)
        {
            string sql = string.Empty;

            try
            {
                if (cmd != null)
                    sql = LogSql(cmd.Sql, cmd.Param);
            }
            catch (Exception) { }

            return sql;
        }

        public static string LogSql(IEnumerable<SqlCmd>? cmds)
        {
            string sqls = string.Empty;

            try
            {
                if (cmds != null)
                {
                    foreach (var cmd in cmds)
                        sqls += LogSql(cmd);
                }
            }
            catch (Exception) { }

            return sqls;
        }

        public static readonly Dictionary<Type, string> DBColumnTypeAliases = new Dictionary<Type, string>
        {
            { typeof(byte), "byte" },
            { typeof(byte[]), "byte[]" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(bool), "bool" },
            { typeof(string), "string" }
        };

        public static readonly HashSet<Type> DBColumnNullableTypes = new HashSet<Type>
        {
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(DateTime)
        };

    }

    /// <summary>
    /// DB類型
    /// </summary>
    public enum DBType
    {
        SYBASE = 1,
        SQLSERVER = 2,
        ORACLE = 3,
    }

}