using System;

namespace SoftOne.Soe.Common.DTO
{
    public class SoftOneStatusDTO
    {

        public SoftOneStatusDTO()
        {

        }

        public SoftOneStatusDTO(Exception exception)
        {
            this.Alive = false;
            this.Count = -1;
            this.MilliSeconds = -1;
            this.ErrorMessage = exception.ToString();
            this.DBConnected = false;
            this.UTC = DateTime.UtcNow;
            this.SoftOneStatusGuid = Guid.NewGuid();
        }

        public Guid SoftOneStatusGuid { get; set; }
        public ServiceType ServiceType { get; set; }
        public bool Alive { get; set; }
        public bool DBConnected { get; set; }
        public DateTime UTC { get; set; }
        public string MachineName { get; set; }
        public int Count { get; set; }
        public int MilliSeconds { get; set; }
        public string Url { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime RequestStart { get; set; }
        public int StatusServiceTypeId { get; set; }
        public DateTime RequestEnd { get; set; }
        public int Prio { get; set; }
        //Methods

        public void SetRequestDates(DateTime start)
        {
            this.RequestStart = start;
            this.RequestEnd = DateTime.UtcNow;
            if (this.UTC < DateTime.UtcNow.AddDays(10))
                this.UTC = start;
        }
    }

    public enum ServiceType
    {
        Unknown = 0,
        Wcf = 1,
        Webforms = 2,
        WebApi = 3,
        WebApiInternal = 4,
        WebserviceExternal = 5,
        TimeSpotOld = 6,
        TimeSpotNew = 7,
        Communicator = 8,
        SoftOneId = 9,
        Logger = 10,
        EdiCore = 11,
        CrGen = 12,
        Ftp = 13,
        SysService = 14,
        TimeStampService = 15,
        WebApiExternal = 16,
        WebTimeStamp = 17,
        Storage = 18,
        SoftOneIdFront = 19,
        SoftOneIdApi = 20,
        SSH = 21,
        Server = 22,
        ScheduleJobs = 23,
        ReportFromWeb = 24,
        SoftOneStatus = 25,
        Selenium = 26
    }
}
