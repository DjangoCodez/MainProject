using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SoftOne.Common.KeyVault;
using SoftOne.Common.KeyVault.Models;

namespace SoftOne.Soe.Business.Util
{
    public class DatabaseRestorer
    {
        private readonly string sqlServer;
        private readonly string localDatabasePath;
        private readonly string storageAccountName;
        private readonly string containerName;

        public DatabaseRestorer()
            : this(".\\dev", @"c:\Softone\Database\", "soedemodbs", "backups")
        {
        }

        public DatabaseRestorer(string localDatabasePath)
            : this(".\\dev", localDatabasePath, "soedemodbs", "backups")
        {
        }

        public DatabaseRestorer(string sqlServer, string localDatabasePath, string storageAccountName, string containerName)
        {
            this.sqlServer = string.IsNullOrWhiteSpace(sqlServer) ? ".\\dev" : sqlServer;
            this.localDatabasePath = string.IsNullOrWhiteSpace(localDatabasePath) ? @"c:\Softone\Database\" : localDatabasePath;
            this.storageAccountName = string.IsNullOrWhiteSpace(storageAccountName) ? "soedemodbs" : storageAccountName;
            this.containerName = string.IsNullOrWhiteSpace(containerName) ? "backups" : containerName;
        }

        public bool RestoreDatabases()
        {
            // Backwards-compatible overload. Passing null will forward to the primary method.
            return RestoreDatabases((KeyVaultSettings)null);
        }

        public bool RestoreDatabases(KeyVaultSettings keyVaultSettings)
        {
            var userAndPassword = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, $"Database-UserAndPassword-SysCompServerId8", keyVaultSettings.StoreLocation);
            if (!string.IsNullOrEmpty(userAndPassword))
                return false;

            var arr = userAndPassword.Split(new string[] { "##" }, StringSplitOptions.None);
            var user = arr[0];
            var password = arr[1];

            var storageKey = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, $"DemoBackupStorageKey", keyVaultSettings.StoreLocation);

            string[] backupFiles = new string[] { "soedemo.bak", "soesysv2.bak" };

            if (!Directory.Exists(localDatabasePath))
                Directory.CreateDirectory(localDatabasePath);

            try
            {
                // Step 1: Download backup files from Azure Blob Storage
                if (!DownloadBackupFiles(backupFiles, storageKey))
                {
                    return false;
                }

                // Step 2: Restore databases from the downloaded backup files
                foreach (var backupFile in backupFiles)
                {
                    if (!RestoreDatabase(backupFile, user, password))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

        private bool DownloadBackupFiles(string[] backupFiles, string storageAccountKey)
        {
            try
            {
                string blobServiceEndpoint = $"https://{storageAccountName}.blob.core.windows.net";
                StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
                BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(blobServiceEndpoint), storageCredentials);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                foreach (var backupFile in backupFiles)
                {
                    //list files on container
                    foreach (BlobItem blobItem in containerClient.GetBlobs())
                    {
                        Console.WriteLine($"File name: {blobItem.Name}");
                    }

                    BlobClient blobClient = containerClient.GetBlobClient(backupFile);
                    string localFilePath = Path.Combine(localDatabasePath, backupFile);

                    Console.WriteLine($"Downloading {backupFile} from Azure Blob Storage to {localFilePath}...");

                    if (File.Exists(localFilePath))
                        File.Delete(localFilePath);

                    blobClient.DownloadTo(localFilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download files from Azure Blob Storage: {ex.Message}");
                return false;
            }
        }

        private bool RestoreDatabase(string backupFile, string sqlUsername, string sqlPassword)
        {
            try
            {
                if (!Directory.Exists(localDatabasePath))
                {
                    Directory.CreateDirectory(localDatabasePath);
                }

                string connectionString = $"Server={sqlServer};Database=master;User Id={sqlUsername};Password={sqlPassword};Timeout=6000;";
                string databaseName = Path.GetFileNameWithoutExtension(backupFile);
                string backupFilePath = Path.Combine(localDatabasePath, backupFile);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Keep quotedDatabaseName for statements where we build the RESTORE command client-side
                   string quotedDatabaseName = QuoteSqlIdentifier(databaseName);

                    // Use a parameter for database name and build/execute the ALTER DATABASE on the server using QUOTENAME to avoid injection
                    string setSingleUserQuery = @"
                                                IF EXISTS (SELECT name FROM sys.databases WHERE name = @dbName)
                                                BEGIN
                                                    DECLARE @sql NVARCHAR(MAX) = N'ALTER DATABASE ' + QUOTENAME(@dbName) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE';
                                                    EXEC sp_executesql @sql;        
                                                END";

                    using (SqlCommand command = new SqlCommand(setSingleUserQuery, connection))
                    {
                        var dbParam = new SqlParameter("@dbName", SqlDbType.NVarChar, 128) { Value = databaseName };
                        command.Parameters.Add(dbParam);
                        command.ExecuteNonQuery();
                    }

                    // Restore the database from the backup file
                    string restoreFilelistQuery = "RESTORE FILELISTONLY FROM DISK = @backupFilePath";

                    using (SqlCommand command = new SqlCommand(restoreFilelistQuery, connection))
                    {
                        command.CommandTimeout = 60000;
                        var backupParam = new SqlParameter("@backupFilePath", SqlDbType.NVarChar, 4000) { Value = backupFilePath };
                        command.Parameters.Add(backupParam);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            string logicalNameData = string.Empty;
                            string logicalNameLog = string.Empty;

                            while (reader.Read())
                            {
                                if (reader["Type"].ToString() == "D")
                                {
                                    logicalNameData = reader["LogicalName"].ToString();
                                }
                                else if (reader["Type"].ToString() == "L")
                                {
                                    logicalNameLog = reader["LogicalName"].ToString();
                                }
                            }

                            reader.Close();

                            if (!string.IsNullOrEmpty(logicalNameData) && !string.IsNullOrEmpty(logicalNameLog))
                            {
                                string dataFilePath = Path.Combine(localDatabasePath, databaseName + ".mdf");
                                string logFilePath = Path.Combine(localDatabasePath, databaseName + ".ldf");

                                // Build restore command using quoted identifiers for DB/logical names and parameters for file paths
                                string restoreQuery = $@"
                                    RESTORE DATABASE {quotedDatabaseName}
                                    FROM DISK = @backupFilePath
                                    WITH FILE = 1, 
                                    NOUNLOAD, 
                                    REPLACE, 
                                    STATS = 10,
                                    MOVE {QuoteSqlIdentifier(logicalNameData)} TO @dataFilePath,
                                    MOVE {QuoteSqlIdentifier(logicalNameLog)} TO @logFilePath";

                                using (SqlCommand restoreCommand = new SqlCommand(restoreQuery, connection))
                                {
                                    restoreCommand.CommandTimeout = 60000;
                                    var backupParam2 = new SqlParameter("@backupFilePath", SqlDbType.NVarChar, 4000) { Value = backupFilePath };
                                    var dataParam = new SqlParameter("@dataFilePath", SqlDbType.NVarChar, 4000) { Value = dataFilePath };
                                    var logParam = new SqlParameter("@logFilePath", SqlDbType.NVarChar, 4000) { Value = logFilePath };

                                    restoreCommand.Parameters.Add(backupParam2);
                                    restoreCommand.Parameters.Add(dataParam);
                                    restoreCommand.Parameters.Add(logParam);
                                    restoreCommand.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Failed to retrieve logical names from the backup file.");
                                return false;
                            }
                        }
                    }

                    // Set the database back to multi-user mode using server-side QUOTENAME and sp_executesql
                    string setMultiUserQuery = @"DECLARE @sql NVARCHAR(MAX) = N'ALTER DATABASE ' + QUOTENAME(@dbName) + N' SET MULTI_USER'; EXEC sp_executesql @sql;";
                    using (SqlCommand command = new SqlCommand(setMultiUserQuery, connection))
                    {
                        var dbParam = new SqlParameter("@dbName", SqlDbType.NVarChar, 128) { Value = databaseName };
                        command.Parameters.Add(dbParam);
                        command.ExecuteNonQuery();
                    }

                    // set dba owner to DBA using server-side QUOTENAME
                    string setOwnerQuery = @"DECLARE @sql NVARCHAR(MAX) = N'ALTER AUTHORIZATION ON DATABASE::' + QUOTENAME(@dbName) + N' TO [dba]'; EXEC sp_executesql @sql;";
                    using (SqlCommand command = new SqlCommand(setOwnerQuery, connection))
                    {
                        var dbParam = new SqlParameter("@dbName", SqlDbType.NVarChar, 128) { Value = databaseName };
                        command.Parameters.Add(dbParam);
                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to restore database from backup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Quote a SQL identifier (database, logical file name, etc.) by wrapping in brackets and escaping any closing bracket.
        /// This helps prevent SQL injection when identifiers must be inserted into T-SQL statements.
        /// </summary>
        private static string QuoteSqlIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return identifier;

            return "[" + identifier.Replace("]", "]]" ) + "]";
        }
    }
}
