using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using SoftOne.Soe.Data;
using System.Linq;
using System.Collections.Generic;

namespace Soe.Business.Test
{
    internal static class TestUtil
    {
        #region Connection

        internal static string GetSoeCompConnection()
        {
            return TestUtil.GetSoeConnection("SoeDemo");
        }

        internal static string GetSoeSysConnection()
        {
            return TestUtil.GetSoeConnection("SOESysV2");
        }

        private static string GetSoeConnection(string targetName)
        {
            //// Specify the provider name, server and database.
            //string providerName = "System.Data.SqlClient";

            //// Build the SqlConnection connection string.
            //string providerString = GetSqlConnectionString(targetName);

            //// Initialize the EntityConnectionStringBuilder.
            //var entityBuilder = new EntityConnectionStringBuilder();

            ////Set the provider name.
            //entityBuilder.Provider = providerName;

            //// Set the provider-specific connection string.
            //entityBuilder.ProviderConnectionString = providerString;

            //// Set the Metadata location.
            //entityBuilder.Metadata = @"res://*";
            //return entityBuilder.ToString();
            return "";
        }

        //not working
        //private static CompEntities GetMockContext()
        //{
        //    return new CompEntities(@"metadata=res://*;provider=System.Data.SqlClient;");
        //}

        internal static string GetSqlConnectionString(string target)
        {
            //// Initialize the connection string builder for the underlying provider.
            //var sqlBuilder = new SqlConnectionStringBuilder();
            //string serverName = @"utv\tfs";

            //// Set the properties for the data source.
            //sqlBuilder.DataSource = serverName;
            //sqlBuilder.InitialCatalog = target;
            //sqlBuilder.IntegratedSecurity = true;
            //sqlBuilder.PersistSecurityInfo = true;
            //sqlBuilder.UserID = "dba";
            //sqlBuilder.Password = "sql";
            //sqlBuilder.MultipleActiveResultSets = true;
            //return sqlBuilder.ToString();
            return "";

        }

        #endregion
    }

    internal class NDbUnitUtil
    {
        #region NDbUnit

        //private NDbUnit.Core.INDbUnitTest SetupCompDatabase(string datasetName)
        //{
        ////Connect to database
        //string connectionString = TestUtil.GetSqlConnectionString("SOECompV3");
        //NDbUnit.Core.INDbUnitTest database = new NDbUnit.Core.SqlClient.SqlDbUnitTest(connectionString);

        ////Create xml from existing data
        //BackupData(database, datasetName);

        ////Init ndbunit
        //database.ReadXmlSchema(@"Datasets\" + datasetName + ".xsd");
        //database.ReadXml(@"Datasets\" + datasetName + ".xsd");

        //return database;
        //}

        //private void BackupData(NDbUnit.Core.INDbUnitTest database, string datasetName)
        //{
        //    database.ReadXmlSchema(@"Datasets\" + datasetName + ".xsd");
        //    System.Data.DataSet ds = database.GetDataSetFromDb();
        //    ds.WriteXml("datasetName.xml");
        //}

        //private void TruncateData(NDbUnit.Core.INDbUnitTest database)
        //{
        //    database.PerformDbOperation(NDbUnit.Core.DbOperationFlag.CleanInsertIdentity);
        //}
        //private void EndTest(NDbUnit.Core.INDbUnitTest database)
        //{
        //    database.PerformDbOperation(NDbUnit.Core.DbOperationFlag.DeleteAll);
        //}

        #endregion
    }

}
