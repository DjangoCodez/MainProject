using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftOne.Soe.Common.Util;

namespace Soe.Business.Test.Util.API
{
    [TestClass()]
    public class AzoraOneJobTests
    {
        private SimpleLogger _logger = new SimpleLogger("TestBatch", "c:\\temp\\communicator\\logs");

        // These are the target companies which we will run the job for.
        // It's companies which currently use ReadSoft but needs to be migrated to AzoraOne.
        private int[] _targetActorCompanyIds =
        {
            347
        };

        [TestMethod()]
        public void RunJobTest()
        {
            // Since we are using bulk operations, we need to add the license key for Z.EntityFramework.Extensions.
            // This is taken care of when running outside the test project. This code is in Startup.cs also, but does not seem to be enough.
            Z.EntityFramework.Extensions.LicenseManager.AddLicense("836;101-SoftOne", "808e16a2-f300-1dd0-5be5-bc37afb71327");
            var actorCompanyIds = _targetActorCompanyIds;

            foreach (var companyId in actorCompanyIds)
            {
                try
                {
                    RunJobWithCompanyId(companyId);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error in company {companyId}: {ex.Message}");
                }
            };
            Assert.IsTrue(true);
        }

        public void RunJobWithCompanyId(int companyId)
        {
            var ediManager = new EdiManager(null);
            _logger.Info($"Starting job for company {companyId} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            var activateResult = ediManager.ActivateAzoraOneInBackground(companyId, 
                doSyncSuppliers: false);
            if (activateResult.Success)
            {
                var syncSuppliersResult = ediManager.SyncAllSuppliersWithAzoraOne(companyId);
                _logger.Info($"Company {companyId} - Sync Suppliers Result: {syncSuppliersResult.Success}, savedSuppliers: {syncSuppliersResult.ObjectsAffected}");
                var trainAzoraOneInterpretorResult = ediManager.TrainAzoraOneInterpretor(companyId);

                if (trainAzoraOneInterpretorResult.Success)
                {
                    _logger.Info($"Company {companyId} - Train Interpretor Result: {trainAzoraOneInterpretorResult.Success}, successfully bookkept: {trainAzoraOneInterpretorResult.ObjectsAffected}");
                }
                else
                {
                    _logger.Error($"Company {companyId} - Train Interpretor failed: {trainAzoraOneInterpretorResult.ErrorMessage}");
                }
            }
            else
            {
                _logger.Error($"Company {companyId} - Activation failed: {activateResult.ErrorMessage}");
            }
        }
        
        private class SimpleLogger
        {
            private readonly string _filePath;
            private int _logSequenceNumber = 0;
            private readonly object _lockObj = new object();

            public SimpleLogger(string batchName, string logDestinationPath)
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{batchName}_{timestamp}.txt";

                // Expected local path for the log file.
                _filePath = Path.Combine(logDestinationPath, fileName);

                // Optional: create the file immediately
                File.WriteAllText(_filePath, $"Log started at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
            }

            public void Info(string message)
            {
                Log("Info", message);
            }

            public void Error(string message)
            {
                Log("Error", message);
            }

            private void Log(string level, string message)
            {
                lock (_lockObj)
                {
                    _logSequenceNumber++;
                    string timestamp = DateTime.Now.ToString("HH:mm:ss");
                    string logLine = $"[{level}] [{_logSequenceNumber} {timestamp}] {message}";

                    File.AppendAllText(_filePath, logLine + Environment.NewLine, Encoding.UTF8);
                }
            }
        }

    }
}
