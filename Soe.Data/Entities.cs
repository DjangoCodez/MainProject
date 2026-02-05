using SoftOne.Common.KeyVault;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Data
{
    public class CompEntities : SOECompEntities
    {
        private static string _connectionString;

        private static SqlConnectionStringBuilder _connectionStringBuilder;

        public static void SetSqlConnectionStringBuilder(SqlConnectionStringBuilder builder)
        {
            _connectionStringBuilder = _connectionStringBuilder == null ? builder : _connectionStringBuilder;
        }

        public CompEntities() : base(GetConnectionString()) { }
        public CompEntities(string connection) : base(connection) { }
        public CompEntities(EntityConnection connection) : base(connection) { }

        public bool IsReadOnly { get; set; }
        public bool RequestScoped { get; set; }
        public int ThreadId { get; set; }
        public bool IsDisposed { get; private set; } = false;

        public bool IsTaskScoped { get; set; } = false;

        private static string GetConnectionString()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["SOECompEntities"];
            if (connectionStringSettings != null)
            {
                var entityBuilder = new EntityConnectionStringBuilder(connectionStringSettings.ConnectionString);
                var sqlBuilder = new SqlConnectionStringBuilder(entityBuilder.ProviderConnectionString);
                entityBuilder.ProviderConnectionString = sqlBuilder.ToString();

                return entityBuilder.ConnectionString;
            }
            else if (_connectionStringBuilder != null)
            {
                string metadata = "res://*/SoftOne.Soe.Data.SOECompModel.csdl|res://*/SoftOne.Soe.Data.SOECompModel.ssdl|res://*/SoftOne.Soe.Data.SOECompModel.msl";
                EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder
                {
                    Metadata = metadata,
                    Provider = "System.Data.SqlClient",
                    ProviderConnectionString = _connectionStringBuilder.ConnectionString
                };

                return entityConnectionStringBuilder.ConnectionString;
            }

            throw new Exception("Connection string not found in the configuration file.");
        }

        public static bool HasValidConnectionString() => !string.IsNullOrEmpty(GetConnectionString());
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                base.Dispose(false);
                return;
            }

            if (IsTaskScoped)
                return; // Task scoped instances are disposed by the Task manager

            if (RequestScoped)
                return; // EndRequest will call DisposeNow

            IsDisposed = true;
            base.Dispose(true);
        }

        public void DisposeNow()
        {
            try
            {
                IsDisposed = true;
                base.Dispose(true);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to dispose CompEntities instance. Exception: " + ex.Message);
            }
        }
    }



}
