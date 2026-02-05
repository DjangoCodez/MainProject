using SoftOne.Soe.Shared.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Shared.DTO
{
    public class RunningExecution
    {
        public static string RunningExecutionKey = "RunningExecutionKey";
        public RunningExecutionInformation StartRunningExecutionInformation(string title, Guid guid)
        {
            RunningExecutionInformation info = new RunningExecutionInformation()
            {
                Key = guid,
                Started = DateTime.Now,
                Title = title,
            };

            SoeCache.Instance.Connector.AddObject(info, GetKey(guid), 1);

            return info;
        }

        public void UpdateRunningExecutionInformation(Guid guid, string message)
        {

        }
        private string GetKey(Guid guid)
        {
            return RunningExecutionKey + guid.ToString();
        }
    }


    public class RunningExecutionInformation
    {
        public RunningExecutionInformation()
        {
            Messages = new List<string>();
        }
        public Guid Key { get; set; }
        public DateTime Started { get; set; }
        public string Title { get; set; }
        public List<string> Messages { get; set; }
        public int NumberOfItems { get; set; }
        public int CurrentHandled { get; set; }
    }
}
