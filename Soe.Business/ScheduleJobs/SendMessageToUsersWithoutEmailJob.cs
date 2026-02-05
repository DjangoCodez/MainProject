using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.ScheduledJobs
{
    public class SendMessageToUsersWithoutEmailJob : ScheduledJobBase, IScheduledJob
    {
        public void Execute(SysScheduledJobDTO scheduledJob, int batchNr)
        {
            #region Init

            base.Init(scheduledJob, batchNr);

            CompanyManager cm = new CompanyManager(parameterObject);
            CommunicationManager comm = new CommunicationManager(parameterObject);
            UserManager um = new UserManager(parameterObject);

            #endregion

            UserDTO sender = null;
            List<int> companyIds = new List<int>();
            List<UserDTO> users = new List<UserDTO>();

            // Get parameters            
            int? paramSenderUserId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "senderuserid").Select(s => s.IntData).FirstOrDefault();
            int? paramLicenseId = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "licenseid").Select(s => s.IntData).FirstOrDefault();
            string paramUserIds = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "userids").Select(s => s.StrData).FirstOrDefault();
            string paramUrl = scheduledJob.SysJobSettings.Where(s => s.Name.ToLower() == "url").Select(s => s.StrData).FirstOrDefault();

            if (String.IsNullOrEmpty(paramUrl))
            {
                CreateLogEntry(ScheduledJobLogLevel.Error, "url måste anges");
                return;
            }

            if (!paramSenderUserId.HasValue)
            {
                CreateLogEntry(ScheduledJobLogLevel.Error, "senderuserid måste anges");
                return;
            }
            else
            {
                sender = um.GetUser(paramSenderUserId.Value).ToDTO();                
                if (sender == null)
                    CreateLogEntry(ScheduledJobLogLevel.Error, "sender kunde inte hittas");
                else if (!sender.DefaultActorCompanyId.HasValue)
                    CreateLogEntry(ScheduledJobLogLevel.Error, "Sender har ingen angiven förvald företag");
            }

            if (!string.IsNullOrEmpty(paramUserIds))
            {
                List<int> userIds = new List<int>();
                char[] separator = new char[1];
                separator[0] = ',';

                string[] separatedIds = paramUserIds.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var separatedId in separatedIds)
                {
                    int id = 0;
                    if (Int32.TryParse(separatedId.Trim(), out id))
                        userIds.Add(id);
                }

                users.AddRange(um.GetUsers(userIds).ToDTOs());

            }
            else if(paramLicenseId.HasValue)
            {
                users.AddRange(um.GetUsersMissingEmail(paramLicenseId.Value));
            }
            else
            {
                users.AddRange(um.GetUsersMissingEmail(null));
            }
                        
            // Check out scheduled job
            ActionResult result = CheckOutScheduledJob();
            if (result.Success)
            {
                try
                {
                    // Execute job
                    CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Startar utskick av meddelanden för {0} användare", users.Count));
                    foreach (var user in users)
                    {
                      

                        CreateLogEntry(ScheduledJobLogLevel.Information, String.Format("Skickar meddelande till '{0}' ({1})", user.Name, user.UserId));
                        result = comm.SendXEMailValidateEmailToUser(sender, user, paramUrl);
                        if (!result.Success)
                            CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid utskick av meddelanden: '{0}'", result.ErrorMessage));
                    }
                }
                catch (Exception ex)
                {
                    result = new ActionResult(ex);
                    CreateLogEntry(ScheduledJobLogLevel.Error, String.Format("Fel vid exekvering av jobb: '{0}'", result.ErrorMessage));
                    base.LogError(ex);
                }

                // Check in scheduled job
                CheckInScheduledJob(result.Success);
            }
        }
    }
}
