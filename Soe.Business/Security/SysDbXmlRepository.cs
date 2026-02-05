using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Logging;
using SoftOne.Soe.Business.Util;
using SoftOne.Status.Shared.DTO.Local;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Security
{
    public class SysDbXmlRepository : IXmlRepository
    {
        private readonly string connectionstring;
        private readonly ILogger<SysDbXmlRepository> logger;
        private IList<XElement> elements = null;

        public SysDbXmlRepository(string connectionstring, ILogger<SysDbXmlRepository> logger)
        {
            this.connectionstring = connectionstring;
            this.logger = logger;
        }

        private void Log(Exception ex, string message)
        {
            try
            {
                Log($"{message} - Exception: {ex}");
            }
            catch
            {
            }
        }
        private void Log(string message)
        {
            try
            {
                var currentDictory = string.Empty;

                try
                {

                    currentDictory = ConfigurationSetupUtil.GetCurrentFolderName();
                }
                catch
                {
                    currentDictory = "N/A";
                }
                //Simple file logger in c:\temp\SysDbXmlRepositoryLog.txt
                if (!File.Exists(@"c:\temp\SysDbXmlRepositoryLog.txt"))
                    File.WriteAllText(@"c:\temp\SysDbXmlRepositoryLog.txt", "");

                var logMessage = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} - {currentDictory} -  {message}{Environment.NewLine}";
                File.AppendAllText(@"c:\temp\SysDbXmlRepositoryLog.txt", logMessage);
            }
            catch
            {

            }
        }

        public virtual IReadOnlyCollection<XElement> GetAllElements()
        {
            Log("Getting all XML-elements");

            if (elements == null)
            {
                elements = new List<XElement>(GetAllElementsCore());
            }
            return new ReadOnlyCollection<XElement>(elements);
        }
        private IReadOnlyList<XElement> GetAllElementsCore()
        {
            const int maxAttempts = 4;
            int delayMs = 200; // exponential backoff base
            var results = new List<XElement>();

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    Log($"Reading DP keys from DB (attempt {attempt}/{maxAttempts})");

                    using (var conn = new SqlConnection(connectionstring))
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT [Xml] FROM [DataProtectionKeys]"; // add hint if you want: ... WITH (READCOMMITTEDLOCK)
                        cmd.CommandTimeout = 15;

                        conn.Open();
                        using (var dr = cmd.ExecuteReader())
                        {
                            int row = 0;
                            while (dr.Read())
                            {
                                row++;
                                if (dr.IsDBNull(0))
                                {
                                    logger.LogWarning("DataProtectionKeys row {Row} had NULL Xml", row);
                                    continue;
                                }

                                var xml = dr.GetString(0);
                                try
                                {
                                    var xe = XElement.Parse(xml);
                                    results.Add(xe);

                                    // tiny bit of metadata for diagnostics
                                    var id = (string)xe.Attribute("id");
                                    Log($"DP key loaded id={id}");
                                }
                                catch (Exception ex)
                                {
                                    logger.LogWarning(ex, "Malformed key XML at row {Row}", row);
                                }
                            }
                        }
                    }

                    Log($"Loaded {results.Count} DP keys");
                    return new ReadOnlyCollection<XElement>(results);
                }
                catch (SqlException ex)
                {
                    Log(ex, $"SQL error reading DataProtection keys (attempt {attempt}/{maxAttempts})");

                    if (attempt == maxAttempts)
                        throw;

                    var jitter = new Random().Next(0, 100);
                    System.Threading.Thread.Sleep(delayMs + jitter);
                    delayMs = Math.Min(delayMs * 2, 4000);
                    results.Clear();
                }
            }
            // Should never get here
            Log("Exceeded maximum attempts to read DataProtection keys");
            return new ReadOnlyCollection<XElement>(results);
        }

        public virtual void StoreElement(XElement element, string friendlyName)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Log($"Persisting new XML-element ({friendlyName})");

            using (var conn = new SqlConnection(connectionstring))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO DataProtectionKeys (FriendlyName, Xml) VALUES (@FriendlyName, @Xml)";
                cmd.Parameters.AddWithValue("FriendlyName", friendlyName);
                cmd.Parameters.AddWithValue("Xml", element.ToString());
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            if (elements != null)
            {
                Log($"Adding XML-element {friendlyName} to cached list of elements");

                elements.Add(element);
            }
        }
    }
}