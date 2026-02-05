using SoftOne.Common.KeyVault;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace SoftOne.Soe.Data.Util
{
    public static class FrownedUponSQLClient
    {

        public static string GetADOConnectionString(int timeoutSeconds = 0)
        {
            using (CompEntities entities = new CompEntities())
            {
                EntityConnection entityConnection = entities.Connection as EntityConnection;

                string connectionString = entityConnection.StoreConnection.ConnectionString;

                if (timeoutSeconds != 0)
                {
                    if (!connectionString.ToLower().Contains("connection timeout"))
                    {
                        connectionString = connectionString + ";Connection Timeout=" + timeoutSeconds.ToString();
                    }
                    return connectionString;
                }

                return connectionString;
            }
        }

        public static string GetADOConnectionStringForSysEntities()
        {
            using (SOESysEntities entities = new SOESysEntities())
            {
                //EntityConnection entityConnection = entities.Connection as EntityConnection;

                return entities.Connection.ConnectionString;
            }
        }

        public static SQLReaderResult ExcuteQueryNew(SqlConnection sqlConnection, string query, int? timeoutSeconds = null)
        {
            SQLReaderResult result = new SQLReaderResult();
            try
            {
                SqlCommand cmd = new SqlCommand(query, sqlConnection);
                if (timeoutSeconds.HasValue)
                    cmd.CommandTimeout = timeoutSeconds.Value;
                cmd.CommandType = CommandType.Text;
                if (sqlConnection.State == ConnectionState.Closed)
                    sqlConnection.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();
                return new SQLReaderResult(reader);
            }
            catch (Exception ex)
            {
                result.Result = new ActionResult(ex, "ExcuteQueryNew failed " + query);
                return result;
            }
        }

        public static SqlDataReader ExcuteQuery(SqlConnection sqlConnection, string query, int? timeoutSeconds = null)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(query, sqlConnection);
                if (timeoutSeconds.HasValue)
                    cmd.CommandTimeout = timeoutSeconds.Value;
                cmd.CommandType = CommandType.Text;
                if (sqlConnection.State == ConnectionState.Closed)
                    sqlConnection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                // sqlConnection.Close();
                cmd.Dispose();

                return reader;
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
            return null;
        }

        public static int ExecuteSql(ObjectContext entities, string sql, int timeoutSeconds = 60)
        {
            int rowsAffected = 0;
            var entityConnection = (EntityConnection)entities.Connection;
            DbConnection conn = entityConnection.StoreConnection;
            ConnectionState initialState = conn.State;

            try
            {
                if (initialState != ConnectionState.Open)
                    conn.Open();  // open connection if not already open
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = timeoutSeconds;
                    cmd.CommandText = sql;
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (initialState != ConnectionState.Open)
                    conn.Close(); // only close connection if not initially open
            }

            return rowsAffected;
        }

        public static SqlDataReader ExcuteStoredProcedure(SqlConnection sqlConnection, string procedureName, List<Tuple<string, SqlDbType, object>> parameters)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(procedureName, sqlConnection);
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (var parameter in parameters)
                {
                    SqlParameter param = new SqlParameter("@" + parameter.Item1, parameter.Item2);
                    param.Value = parameter.Item3 == null ? DBNull.Value : parameter.Item3;

                    cmd.Parameters.Add(param);
                }

                sqlConnection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                sqlConnection.Close();
                cmd.Dispose();

                return reader;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static CompEntities ChangeDatabase(this CompEntities entities, string newDBName)
        {
            // Use SqlConnectionStringBuilder for safe manipulation
            var builder = new SqlConnectionStringBuilder(
                (entities.Connection as EntityConnection).StoreConnection.ConnectionString)
            {
                InitialCatalog = newDBName, // Change the database name
                PersistSecurityInfo = true,
            };

            // Get password
            builder.Password = GetDbPassword(entities);

            // Create the new EntityConnection
            MetadataWorkspace workspace = new MetadataWorkspace(
                new string[] { "res://*/" },
                new Assembly[] { Assembly.GetExecutingAssembly() });
            var newConnection = new SqlConnection(builder.ConnectionString);
            var entityConnection = new EntityConnection(workspace, newConnection);

            // Create and return the new context
            return new CompEntities(entityConnection);
        }

        public static string Password { get; set; }

        private static string GetDbPassword(this CompEntities entities)
        {
            string[] separator = new string[1];
            separator[0] = ";";
            string connectionString = (entities.Connection as System.Data.Entity.Core.EntityClient.EntityConnection).StoreConnection.ConnectionString;
            string[] parts = connectionString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            string dbPart = parts.FirstOrDefault(x => x.ToLower().StartsWith("password"));
            if (dbPart != null)
            {
                separator[0] = "=";
                parts = dbPart.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Count() != 2)
                    return string.Empty;

                return parts[1].Trim();
            }

            parts = ConfigurationManager.ConnectionStrings["SOECompEntities"]?.ConnectionString?.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            dbPart = parts.FirstOrDefault(x => x.ToLower().StartsWith("password"));
            if (dbPart != null)
            {
                separator[0] = "=";
                parts = dbPart.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Count() != 2)
                    return string.Empty;

                return parts[1].Trim();
            }

            return Password;
        }

        public static string GetDBName(this CompEntities entities)
        {
            string[] separator = new string[1];
            separator[0] = ";";
            string connectionString = (entities.Connection as System.Data.Entity.Core.EntityClient.EntityConnection).StoreConnection.ConnectionString;
            string[] parts = connectionString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            String dbPart = parts.FirstOrDefault(x => x.StartsWith("Initial Catalog"));
            separator[0] = "=";
            parts = dbPart.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Count() != 2)
                return string.Empty;

            string connectionDBName = parts[1].Trim().ToLower();

            return connectionDBName;
        }


        public static DateTime GetLastUserUpdateInTable(CompEntities entities, string table)
        {
            DateTime result = DateTime.Now;
            try
            {
                string query = $"SELECT top 1 last_user_update from sys.dm_db_index_usage_stats where database_id = DB_ID('{entities.GetDBName()}') and object_id = object_id('{entities.GetDBName()}.dbo.{table}') order by last_user_update desc";

                using (SqlConnection sqlConnection = new SqlConnection(GetADOConnectionString()))
                {
                    var reader = ExcuteQuery(sqlConnection, query);

                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            result = (DateTime)reader["last_user_update"];
                        }
                    }
                }
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
            return result;
        }
    }

    public class SQLReaderResult
    {
        public SQLReaderResult(SqlDataReader sqlDataReader)
        {
            Result = new ActionResult();
            SqlDataReader = sqlDataReader;
        }

        public SQLReaderResult()
        {
            Result = new ActionResult();
        }
        public SqlDataReader SqlDataReader { get; set; }
        public ActionResult Result { get; set; }
    }
}