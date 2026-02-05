using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Data
{
    partial class CompEntities : SOECompEntities
    {
        private static string _connectionString;
        public CompEntities()
            : base(GetConnectionString())
        { }
        public CompEntities(string connection)
        : base(connection)
        { }
        public CompEntities(EntityConnection connection)
  : base(connection)
        { }

        private static string GetConnectionString()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["CompEntities"];
            if (connectionStringSettings != null)
            {
                var entityBuilder = new EntityConnectionStringBuilder(connectionStringSettings.ConnectionString);
                var sqlBuilder = new SqlConnectionStringBuilder(entityBuilder.ProviderConnectionString);

                Confi

                // Modify the database and instance values
                //sqlBuilder.InitialCatalog = sqlBuilder.InitialCatalog; //TODO: Change to the correct database
                //sqlBuilder.DataSource = sqlBuilder.DataSource; //TODO: Change to the correct instance

                // Set the modified connection string
                entityBuilder.ProviderConnectionString = sqlBuilder.ToString();

                return entityBuilder.ConnectionString;
            }

            throw new Exception("Connection string not found in the configuration file.");
        }
    }
}
