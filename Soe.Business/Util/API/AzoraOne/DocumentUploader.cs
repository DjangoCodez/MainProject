using SoftOne.Soe.Business.Util.API.AzoraOne.Connectors;
using SoftOne.Soe.Business.Util.API.AzoraOne.Models;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SoftOne.Soe.Business.Util.API.AzoraOne
{
    public class AzoraOnePollingOptions
    {
        public bool UsePolling { get; set; }

        private int? _waitForFirstN { get; set; }
        private int _intervalTimeoutMS { get; set; }
        private int _maxWaitMS { get; set; }

        private DateTime? _pollUntil { get; set; }
        public AzoraOnePollingOptions(int intervalTimeoutMS = 300, int maxWaitMS = 60_000, int? waitForFirstN = null)
        {
            this.UsePolling = true;
            _waitForFirstN = waitForFirstN;
            _intervalTimeoutMS = intervalTimeoutMS;
            _maxWaitMS = maxWaitMS;
        }
        public DateTime PollUntil()
        {
            if (_pollUntil != null)
                return _pollUntil.Value;
            _pollUntil = DateTime.UtcNow.AddMilliseconds(_maxWaitMS);
            return _pollUntil.Value;
        }
        public bool DoContinuePolling(int count, bool shouldSleep)
        {
            if (_waitForFirstN.HasValue && _waitForFirstN >= count)
                return false;

            if (DateTime.UtcNow > PollUntil())
                return false;

            if (shouldSleep)
                Thread.Sleep(_intervalTimeoutMS);
            
            return true;
        }
        public void AddTimeIfNecessary(int ms)
        {
            if (PollUntil() < DateTime.UtcNow)
                _pollUntil = DateTime.UtcNow.AddMilliseconds(ms);
        }
    }
    public class AzoraOneDocumentUploader
    {
        private FileConnector _fileConnector;
        private AzoraOnePollingOptions _pollingOptions;
        private int _expectedCount = 0;
        private int _handledCount = 0;
        private List<(int, string)> _fileIds = new List<(int, string)>();
        private List<(int, ActionResult)> _documentFailed = new List<(int, ActionResult)>();
        public AzoraOneDocumentUploader(FileConnector connector, AzoraOnePollingOptions pollingOptions)
        {
            _fileConnector = connector;
            _pollingOptions = pollingOptions;
        }
        public AzoraOneDocumentUploader(Guid companyGuid) : this(new FileConnector(companyGuid), new AzoraOnePollingOptions()) { }
        public bool UsesPolling() => _pollingOptions.UsePolling;
        public int UploadedCount() => _fileIds.Count;
        public ActionResult UploadDocument(int entityPK, string fileId, string fileName, byte[] fileContent, string webhookUrl = null)
        {
            var file = _fileConnector.AddFile(fileId, fileName, fileContent, webhookUrl);
            var result = file.ToActionResult();
            if (file.IsSuccess || file.HasOnlyError(ConnectorError.File_IdAlreadyExists))
            {
                // If file id already exists, we behave as if it was a success, that way the file will be processed.
                result.Success = true;
                _fileIds.Add((entityPK, fileId));
                _expectedCount++;
            }
            else
            {
                _documentFailed.Add((entityPK, result));
            }
            return result;
        }
        public IEnumerable<(int, AOResponseWrapper<AOSupplierInvoice>)> GetUploadedInvoice()
        {
            foreach (var (scanningEntryId, fileId) in GetDocumentsReadyForExtraction())
            {
                var response = _fileConnector.ExtractSupplierInvoice(fileId);
                if (response.IsSuccess)
                {
                    yield return (scanningEntryId, response);
                }
            }   
        }

        public IEnumerable<(int, ActionResult)> GetFailedEntryIds()
        {
            return _documentFailed;
        }

        public IEnumerable<(int, string)> GetDocumentsReadyForExtraction()
        {
            int countsInSession = 0;
            bool shouldSleep = false;
            while (_pollingOptions.DoContinuePolling(countsInSession, shouldSleep) && _handledCount < _expectedCount)
            {
                shouldSleep = true;
                var (scanningEntryId, fileId) = _fileIds.First();
                var response = _fileConnector.GetFile(fileId);
                if (response.IsSuccess)
                {
                    var file = response.GetValue();
                    if (file.IsNotWaiting())
                    {
                        _fileIds.RemoveAt(0);
                        yield return (scanningEntryId, fileId);
                        countsInSession++;
                        _handledCount++;
                        _pollingOptions.AddTimeIfNecessary(10_000);
                        shouldSleep = false;
                    }
                } 
                else
                {
                    // If response is not ok, we move one.
                    _fileIds.RemoveAt(0);
                    _documentFailed.Add((scanningEntryId, response.ToActionResult()));
                }
            }
        }
    }
}
