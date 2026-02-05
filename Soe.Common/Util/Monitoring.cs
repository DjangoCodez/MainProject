using SoftOne.Soe.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.Util
{
    public class SoeProgressInfoSmall
    {
        public bool Abort { get; set; }
        public bool Done { get; set; }
        public bool Error { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
    }

    [TSInclude]
    public class SoeProgressInfo
    {
        public const int MaxLines = 15;
        private readonly DateTime _created = DateTime.Now;
        private DateTime _lastAction = DateTime.Now;

        public SoeProgressInfo(Guid pollingKey, SoeProgressInfoType soeProgressInfoType = SoeProgressInfoType.Unknown, int actorCompanyId = 0, int id = 0)
        {
            this.PollingKey = pollingKey;
            this.Abort = false;
            this.Done = false;
            this.Error = false;
            this._baseMessage = "";
            this._message = "";
            this.ErrorMessage = "";
            this.SoeProgressInfoType = soeProgressInfoType;
            this.ActorCompanyId = actorCompanyId;
            this.Id = id;
        }

        public int ActorCompanyId { get; set; }
        public int Id { get; set; }
        public SoeProgressInfoType SoeProgressInfoType { get; set; }
        public Guid PollingKey { get; set; }
        public bool Abort { get; set; }
        public bool Done { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }

        public DateTime Created
        {
            get
            {
                return this._created;
            }
        }
        public TimeSpan Age { get { return DateTime.Now - _created; } }
        public TimeSpan TimeSinceLastAction { get { return DateTime.Now - _lastAction; } }

        private string _baseMessage;
        public string BaseMessage
        {
            get
            {
                return _baseMessage;
            }
            set
            {
                _baseMessage = value;
                if (String.IsNullOrEmpty(_message))
                    _message = _baseMessage;
            }
        }
        private string _message;
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = (!String.IsNullOrWhiteSpace(BaseMessage) ? BaseMessage + " - " : "") + value;
            }
        }

        public void ActionPerformed()
        {
            _lastAction = DateTime.Now;
        }
    }

    public class SoeMonitor
    {
        public const double MaxActionWaitTime = 0.5; // In minutes
        public const double MaxAge = 60.0; // In minutes

        private readonly object dictionaryLock = new object();
        private readonly Dictionary<Guid, SoeProgressInfo> _dictionary = new Dictionary<Guid, SoeProgressInfo>();

        private Dictionary<Guid, IEnumerable<object>> _result = new Dictionary<Guid, IEnumerable<object>>();

        public void Purge()
        {
            lock (dictionaryLock)
            {
                List<SoeProgressInfo> itemsToRemove =
                    _dictionary
                        .Values
                        .Where(
                            item => item.Age.TotalMinutes > MaxAge || item.TimeSinceLastAction.TotalMinutes > MaxActionWaitTime)
                    .ToList();

                foreach (SoeProgressInfo info in itemsToRemove)
                {
                    _dictionary.Remove(info.PollingKey);
                }
            }
        }

        public SoeProgressInfo RegisterNewProgressProcess(Guid progressFeedbackKey, SoeProgressInfoType soeProgressInfoType = SoeProgressInfoType.Unknown, int actorCompanyId = 0, int id = 0)
        {
            Purge();

            SoeProgressInfo infoHolder = new SoeProgressInfo(progressFeedbackKey, soeProgressInfoType, actorCompanyId);
            lock (dictionaryLock)
            {
                _dictionary.Add(progressFeedbackKey, infoHolder);
            }
            return infoHolder;
        }

        public SoeProgressInfo GetInfo(Guid key)
        {
            lock (dictionaryLock)
            {
                SoeProgressInfo info;
                if (_dictionary.TryGetValue(key, out info))
                {
                    info.ActionPerformed();
                    return info;
                }
                return null;
            }
        }

        public SoeProgressInfo GetInfo(SoeProgressInfoType soeProgressInfoType, int actorCompanyId, int id)
        {
            lock (dictionaryLock)
            {
                if (_dictionary != null)
                {
                    foreach (var pair in _dictionary)
                    {
                        if (pair.Value.SoeProgressInfoType == soeProgressInfoType && pair.Value.ActorCompanyId == actorCompanyId && pair.Value.Id == id)
                            return pair.Value;
                    }

                    return null;
                }
                return null;
            }
        }

        public IEnumerable<object> GetResult(Guid key)
        {
            lock (dictionaryLock)
            {
                IEnumerable<object> result = null;
                if (_result.TryGetValue(key, out result))
                {
                    return result;
                }
                return null;
            }
        }

        public void AddResult(Guid key, object result)
        {
            AddResult(key, new List<object> { result });
        }

        public void AddResult(Guid key, IEnumerable<object> result)
        {
            lock (dictionaryLock)
            {
                if (_result.Keys.Contains(key))
                {
                    //Insert
                    _result[key] = result;
                }
                else
                {
                    //Add
                    _result.Add(key, result);
                }
            }
        }
    }
}
