using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.ScheduledJobs
{
    public class EdiAdmin : ScheduledJobBase, IScheduledJob, IScheduledService //, IMessageClass
    {
        public void Execute(SoftOne.Soe.Common.DTO.SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            #endregion

            #region Perform
            //// Check out scheduled job
            //ActionResult result = CheckOutScheduledJob();

            //// Parameters
            //bool? moveFilesMode = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "movefilesmode").Select(s => s.BoolData).FirstOrDefault();
            //bool verbose = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "verbose").Select(s => s.BoolData).FirstOrDefault() ?? false;


            //try
            //{
            //    var ediAdminManager = EdiAdminFactory.Instance.GetEdiAdminManager<EdiAdminManager>();
            //    ediAdminManager.OnOutputRecived += writer_Output;
            //    ediAdminManager.Setup(true, scheduledJob.SysScheduledJobId, batchNr);

            //    if (moveFilesMode == true)
            //        result = ediAdminManager.ParseMessagesFromCustomerNotFoundFtp(sendServiceCallToXE: true);
            //    else
            //        result = ediAdminManager.ParseMessages();

            //    if (!result.Success)
            //        CreateLogEntry(ScheduledJobLogLevel.Error, String.Concat("One or more errors occured when running ediAdminManager. ErrorMessage: {0}", result.ErrorMessage));
            //}
            //catch (Exception ex)
            //{
            //    CreateLogEntry(ScheduledJobLogLevel.Error, "Error in EdiAdmin when using the new ediAdminManager: " + ex.GetInnerExceptionMessages().JoinToString(", Inner:"));
            //}

            //CheckInScheduledJob(true);
            #endregion
        }

        public void ExecuteService(SysScheduledJobDTO scheduledJob)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            #endregion

            #region Perform
            //// Check out scheduled job
            //ActionResult result = CheckOutScheduledJob();

            //if (scheduledJob.Type != ScheduledJobType.Service)
            //{
            //    CreateLogEntry(ScheduledJobLogLevel.Error, "ScheduledJobType was not of type service, aborting!");
            //    return;
            //}

            //// Folderwatcher is static and behavies differently since it will continue to run, so try to initiate it here if it isn't
            //var folderWatcher = EdiAdminFactory.Instance.GetEdiAdminManager<EdiAdminFolderWatcherManager>();
            //if (!folderWatcher.IsInitialized)
            //{
            //    CreateLogEntry(ScheduledJobLogLevel.Information, "Initializing folderwatcher");
            //    folderWatcher.Setup(true, this.scheduledJob.SysScheduledJobId, 9999);
            //    CreateLogEntry(ScheduledJobLogLevel.Information, "StartWatch");
            //    folderWatcher.StartWatch(this.scheduledJob.SysScheduledJobId);
            //}
            //else
            //{
            //    CreateLogEntry(ScheduledJobLogLevel.Information, "Folderwatcher was already initialized with hashcode " + folderWatcher.GetHashCode() + ". Will now reset watchers");
            //    folderWatcher.ResetWatchers();
            //    folderWatcher.AddWatchers();
            //    folderWatcher.StartWatch(this.scheduledJob.SysScheduledJobId);
            //}

            //CreateLogEntry(ScheduledJobLogLevel.Information, folderWatcher.GetStatusMessage());

            //CheckInScheduledJob(true);
            #endregion
        }
    }
}
