using Newtonsoft.Json;
using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Communicator;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.PushNotifications;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.Communicator;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class CommunicationManager : ManagerBase
    {
        #region Variables

        // Create map logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public CommunicationManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region XEMail

        public ActionResult SendXEMailToExecutive(int actorCompanyId, int executiveUserId, string subject, string text)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);
            var user = UserManager.GetUser(executiveUserId);

            return SendXEMailToSelf(company, user, subject, text);
        }

        public ActionResult SendXEMailToSelf(Company company, User user, string subject, string text)
        {
            MessageEditDTO messageDto = new MessageEditDTO()
            {
                ParentId = 0,
                AnswerType = XEMailAnswerType.None,
                LicenseId = company.LicenseId,
                ActorCompanyId = company.ActorCompanyId,
                RoleId = user.DefaultRoleId,
                SenderUserId = user.UserId,
                SenderName = user.Name,
                SenderEmail = String.Empty,
                Subject = subject,
                Text = text,
                ShortText = text,
                Entity = SoeEntityType.XEMail,
                MessagePriority = TermGroup_MessagePriority.Normal,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.Text,
                MessageType = TermGroup_MessageType.UserInitiated,
                Recievers = new List<MessageRecipientDTO>() { new MessageRecipientDTO() { UserId = user.UserId } },
                MarkAsOutgoing = false,
            };

            return SendXEMail(messageDto, company.ActorCompanyId, user.ActiveRoleId, user.UserId);
        }

        public bool HasXeMailSendPermission(CompEntities entities, int actorCompanyId, int roleId)
        {
            string key = $"HasXeMailSendPermission{actorCompanyId}#{roleId}";
            var value = BusinessMemoryCache<bool?>.Get(key);

            if (value.HasValue)
                return value.Value;

            bool hasReadOnlyPermission = FeatureManager.HasRolePermission(Feature.Communication_XEmail_Send, Permission.Readonly, roleId, actorCompanyId, entities: entities);
            bool hasModifyPermission = FeatureManager.HasRolePermission(Feature.Communication_XEmail_Send, Permission.Modify, roleId, actorCompanyId, entities: entities);

            // This because map lot of user miss this permission but will still be able to send Xemail. If you hade read SET but not modify set permission to false.
            if (!hasModifyPermission && hasReadOnlyPermission)
                value = false;
            else
                value = true;

            BusinessMemoryCache<bool?>.Set(key, value);

            return value.Value;
        }

        public ActionResult SendXEMail(MessageEditDTO mail, int actorCompanyId, int roleId, int userId, bool checkExisting = false, int? dataStorageId = null, bool dontSendPush = false, User sender = null)
        {
            ActionResult result = new ActionResult(true);

            if (mail == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "MessageEditDTO");

            if (!string.IsNullOrEmpty(mail.Subject) && mail.Subject.Length > 255)
            {
                mail.Subject = mail.Subject.Substring(0, 255);
            }

            ArrayList emailRecipients = new ArrayList();
            List<SysCountry> syscountries = new List<SysCountry>();
            Message message = null;
            MessageText messageText = null;
            bool useCommunicator = false;

            if (mail.CopyToSMS)
                syscountries = CountryCurrencyManager.GetSysCountries(); //must be called before entities is initialized

            // Shift request properties
            List<TimeScheduleTemplateBlock> overlappingShifts = new List<TimeScheduleTemplateBlock>();
            bool abortSave = false;
            bool passedShift = false;
            bool missingRequest = false;

            #region XEmail

            List<MessageRecipient> recipients = new List<MessageRecipient>();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        if (!HasXeMailSendPermission(entities, actorCompanyId, roleId))
                            return new ActionResult(false, 0, "No permission");

                        #region Prereq

                        useCommunicator = true;

                        if (mail == null)
                            return new ActionResult(false, (int)ActionResultSave.Communication_ObjectIsNull, GetText(7044, "Felaktig inparameter"));

                        // Get User
                        User user = sender != null ? sender : UserManager.GetUser(entities, userId);
                        if (user == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                        #endregion

                        #region Perform

                        if (mail.MessageType == TermGroup_MessageType.ShiftRequest)
                        {
                            checkExisting = true;
                            if (mail.AnswerType == XEMailAnswerType.None)
                            {
                                // Get requested shift
                                TimeScheduleTemplateBlock block = TimeScheduleManager.GetTimeScheduleTemplateBlock(entities, mail.RecordId);
                                if (block != null && block.Date.HasValue)
                                {
                                    // Can not send map shift request if stop time on shift is in the passed
                                    DateTime requestedStop = block.Date.Value.Date.Add(block.StopTime - CalendarUtility.DATETIME_DEFAULT);
                                    if (requestedStop < DateTime.Now)
                                    {
                                        result = new ActionResult(false, (int)ActionResultSave.Communication_ShiftIsPassed, String.Format(GetText(3681, "Du kan inte skicka en förfrågan på ett {0} som redan har varit."), block.CustomerInvoiceId.HasValue ? GetText(485, (int)TermGroup.TimeSchedulePlanning, "uppdrag") : GetText(481, (int)TermGroup.TimeSchedulePlanning, "pass")));
                                        return result;
                                    }
                                }
                            }
                        }

                        if (mail.AnswerType == XEMailAnswerType.No || mail.AnswerType == XEMailAnswerType.Yes)
                        {
                            SetAnswerType(entities, mail.AnswerType, (mail.ParentId ?? 0), userId);

                            #region Shift request

                            if (mail.MessageType == TermGroup_MessageType.ShiftRequest && (mail.AnswerType == XEMailAnswerType.Yes || mail.AnswerType == XEMailAnswerType.No))
                            {
                                #region Check that recipient is still active on current shift request

                                MessageRecipient recipient = (from r in entities.MessageRecipient
                                                              where r.Message.ActorCompanyId == actorCompanyId &&
                                                              r.MessageId == (int)mail.ParentId &&
                                                              r.Message.State != (int)SoeEntityState.Deleted &&
                                                              r.UserId == userId &&
                                                              r.State == (int)SoeEntityState.Active
                                                              select r).FirstOrDefault();
                                if (recipient == null)
                                {
                                    result.Success = false;
                                    abortSave = true;
                                    missingRequest = true;
                                }

                                #endregion

                                #region Check for overlapping existing shifts

                                if (!missingRequest)
                                {
                                    // Get requested shift
                                    message = GetMessage(entities, (mail.ParentId ?? 0));
                                    if (message != null)
                                    {
                                        TimeScheduleTemplateBlock block = TimeScheduleManager.GetTimeScheduleTemplateBlock(entities, message.RecordId);
                                        if (block != null && block.Date.HasValue)
                                        {
                                            Employee employee = EmployeeManager.GetEmployeeByUser(entities, (message.ActorCompanyId ?? 0), userId);
                                            if (employee != null)
                                            {
                                                DateTime blockStart = block.Date.Value;
                                                DateTime blockStop = block.Date.Value.AddDays(1);
                                                DateTime requestedStart = CalendarUtility.MergeDateAndTime(block.Date.Value, block.StartTime);
                                                DateTime requestedStop = CalendarUtility.MergeDateAndTime(block.Date.Value, block.StopTime);
                                                // Handle midnight
                                                requestedStop = requestedStop.AddDays((block.StopTime.Date - block.StartTime.Date).Days);
                                                if (block.BelongsToPreviousDay)
                                                {
                                                    requestedStart = requestedStart.AddDays(1);
                                                    requestedStop = requestedStop.AddDays(1);
                                                }
                                                else if (block.BelongsToNextDay)
                                                {
                                                    requestedStart = requestedStart.AddDays(-1);
                                                    requestedStop = requestedStop.AddDays(-1);
                                                }

                                                // Can not answer to map shift request if stop time on shift is in the passed
                                                if (requestedStop < DateTime.Now)
                                                {
                                                    result.Success = false;
                                                    abortSave = true;
                                                    passedShift = true;
                                                }
                                                else
                                                {
                                                    // If map user is sending map positive answer for map shift request,
                                                    // we must first check that the employee does not have an existing shift that will overlap this shift.
                                                    if (mail.AnswerType == XEMailAnswerType.Yes)
                                                    {
                                                        // Get existing shifts for requested date and the day after (if requested shift spans over midnight)
                                                        List<TimeScheduleTemplateBlock> existingShifts = (from tb in entities.TimeScheduleTemplateBlock
                                                                                                            .Include("ShiftType")
                                                                                                          where tb.TimeScheduleTemplateBlockId != block.TimeScheduleTemplateBlockId &&
                                                                                                          tb.EmployeeId == employee.EmployeeId &&
                                                                                                          (tb.Date >= blockStart && tb.Date <= blockStop) &&
                                                                                                          tb.StartTime != tb.StopTime &&
                                                                                                          tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None &&
                                                                                                          tb.State == (int)SoeEntityState.Active
                                                                                                          orderby tb.StartTime
                                                                                                          select tb).ToList();

                                                        existingShifts = existingShifts.Where(w => !w.TimeScheduleScenarioHeadId.HasValue).ToList();

                                                        // Check for overlapping shifts
                                                        foreach (TimeScheduleTemplateBlock shift in existingShifts)
                                                        {
                                                            if (shift.Date.HasValue)
                                                            {
                                                                blockStart = CalendarUtility.MergeDateAndTime(shift.Date.Value, shift.StartTime);
                                                                blockStop = CalendarUtility.MergeDateAndTime(shift.Date.Value, shift.StopTime);
                                                            }

                                                            // Handle midnight
                                                            blockStop = blockStop.AddDays((shift.StopTime.Date - shift.StartTime.Date).Days);
                                                            if (shift.BelongsToPreviousDay)
                                                            {
                                                                blockStart = blockStart.AddDays(1);
                                                                blockStop = blockStop.AddDays(1);
                                                            }
                                                            else if (shift.BelongsToNextDay)
                                                            {
                                                                blockStart = blockStart.AddDays(-1);
                                                                blockStop = blockStop.AddDays(-1);
                                                            }

                                                            if (CalendarUtility.IsDatesOverlapping(requestedStart, requestedStop, blockStart, blockStop))
                                                            {
                                                                overlappingShifts.Add(shift);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (overlappingShifts.Count > 0)
                                    {
                                        // Instead of assigning the shift to the employee, we will create map new XEmail and send back.
                                        // The message will contain the overlapping shift(s).
                                        result.Success = false;
                                        abortSave = true;
                                    }
                                }

                                #endregion

                                #region Remove other users requests

                                if (mail.AnswerType == XEMailAnswerType.Yes && !missingRequest && !passedShift && overlappingShifts.Count == 0)
                                {
                                    // A positive answer on map shift request will remove all other requests for the same shift
                                    List<MessageRecipient> recs = (from r in entities.MessageRecipient
                                                                   where r.Message.Type == (int)TermGroup_MessageType.ShiftRequest &&
                                                                   r.MessageId == (int)mail.ParentId &&
                                                                   r.UserId != mail.SenderUserId &&
                                                                   r.State != (int)SoeEntityState.Deleted
                                                                   select r).ToList();

                                    foreach (MessageRecipient rec in recs)
                                    {
                                        ChangeEntityState(entities, rec, SoeEntityState.Inactive, false, user);
                                    }
                                }

                                #endregion
                            }

                            #endregion
                        }
                        else
                        {
                            result = SaveMessage(entities, transaction, mail, ref message, ref messageText, userId, DateTime.Now, checkExisting: checkExisting, updateSubjectAndBodyOnExisting: checkExisting && mail.MessageType == TermGroup_MessageType.ShiftRequest, dataStorageId: dataStorageId);
                            if (result.Success)
                            {
                                result = SendMessage(entities, transaction, result.IntegerValue, actorCompanyId, roleId, userId, mail.Recievers, ref emailRecipients, ref recipients, TermGroup_MessageDeliveryType.XEmail, false, checkExisting, mail.ParentId, forceSendToReceiver: mail.ForceSendToReceiver, forceSendToEmailReceiver: mail.ForceSendToEmailReceiver);
                                if (!result.Success)
                                    abortSave = true;
                            }
                        }

                        if (!abortSave)
                            result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }

                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties                        
                    }
                    else
                    {
                        if (!abortSave)
                            base.LogTransactionFailed(this.ToString(), this.log);
                    }

                    entities.Connection.Close();
                }
            }

            #endregion

            #region External mail

            if (result.Success && !String.IsNullOrEmpty(mail.SenderEmail))
            {
                for (int i = 0; i < emailRecipients.Count; i++)
                {
                    if (useCommunicator)
                        EmailManager.SendEmailViaCommunicator(mail.SenderEmail, emailRecipients[i].ToString(), new List<string>(), mail.Subject, mail.Text, true, mail.Attachments);
                    else
                        EmailManager.SendEmail(mail.SenderEmail, emailRecipients[i].ToString(), new List<string>(), mail.Subject, mail.Text, true, false, mail.Attachments);
                }
            }

            #endregion

            #region Push notification

            if (result.Success && message != null && !dontSendPush)
                SendMessagePushNotificationFireAndForget(message.MessageId, mail.MessageType == TermGroup_MessageType.ShiftRequest ? recipients.Select(x => x.UserId).ToList() : null);

            #endregion

            #region SMS

            //only try send sms if xemail transaction was successful
            if (result.Success && mail.CopyToSMS && message != null)
            {
                ActionResult saveSMSResult = new ActionResult();
                bool isSent = false;
                String errorMessage = string.Empty;
                int errorNumber = -1;

                using (CompEntities entities = new CompEntities())
                {
                    try
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            if (entities.Connection.State != ConnectionState.Open)
                                entities.Connection.Open();

                            Message savedMessage = GetMessage(entities, message.MessageId);
                            MessageText savedMessageText = GetMessageText(entities, messageText.MessageTextId);
                            List<MessageRecipient> savedRecipients = GetMessageRecipients(entities, savedMessage.MessageId);

                            ActionResult sendSMSResult = SendSMS(entities, actorCompanyId, savedMessage, savedMessageText, mail.ShortText, savedRecipients, syscountries);
                            isSent = sendSMSResult.Success;
                            errorNumber = sendSMSResult.ErrorNumber;
                            errorMessage = sendSMSResult.ErrorMessage;

                            if (isSent)
                                saveSMSResult = SaveChanges(entities, transaction);
                            else
                                saveSMSResult.Success = false;

                            //Commit transaction
                            if (saveSMSResult.Success)
                                transaction.Complete();
                        }
                    }
                    catch (Exception ex)
                    {
                        saveSMSResult.Exception = ex;
                        base.LogError(ex, this.log);
                    }
                    finally
                    {
                        if (!saveSMSResult.Success)
                        {
                            if (isSent)
                                base.LogWarning("SMS is sent but result could not be saved");

                            base.LogTransactionFailed(this.ToString(), this.log);
                        }

                        entities.Connection.Close();
                    }
                }

                //If sms could not be send but xemail could be sent, tell the user
                if (!isSent)
                    result.ErrorMessage += "\n" + GetText(8265, "SMS kunde inte skickas.") + " " + GetText(8266, "Felkod:") + " (" + errorNumber + " " + errorMessage + ") ";
            }

            #endregion

            #region Shift request

            if (result.Success)
            {
                if (mail.MessageType == TermGroup_MessageType.ShiftRequest && (mail.AnswerType == XEMailAnswerType.Yes || mail.AnswerType == XEMailAnswerType.No) && mail.ParentId.HasValue)
                {
                    ActionResult mailResult = HandleShiftRequestMessage(mail.ParentId.Value, userId);
                    if (mailResult.Success)
                    {
                        MessageEditDTO dto = mailResult.Value as MessageEditDTO;
                        if (dto != null)
                            result = SendXEMail(dto, dto.ActorCompanyId.Value, roleId, dto.SenderUserId.Value);
                    }
                }
            }
            else
            {
                if (missingRequest || passedShift || overlappingShifts.Count > 0)
                {
                    ActionResult mailResult = AbortShiftRequestMessage(mail.AnswerType, overlappingShifts, passedShift, missingRequest, mail.ParentId.Value, userId);
                    if (mailResult.Success)
                    {
                        MessageEditDTO dto = mailResult.Value as MessageEditDTO;
                        if (dto != null)
                            result = SendXEMail(dto, dto.ActorCompanyId.Value, roleId, dto.SenderUserId.Value);
                    }
                }
            }

            #endregion

            #region Needs confirmation

            if (result.Success && mail.MessageType == TermGroup_MessageType.NeedsConfirmation && mail.AnswerType == XEMailAnswerType.Yes && mail.ParentId.HasValue)
            {
                MessageEditDTO dto = HandleNeedsConfirmationMessage(mail.ParentId.Value, userId);
                if (dto != null)
                    result = SendXEMail(dto, dto.ActorCompanyId.Value, roleId, dto.SenderUserId.Value);
            }

            #endregion

            if (dontSendPush && message != null)
                result.IntegerValue = message.MessageId;

            return result;
        }

        public ActionResult SendXEMail(TransactionScope transaction, CompEntities entities, MessageEditDTO mail, int actorCompanyId, int roleId, int userId, bool checkExisting = false, bool dontSendPush = false, List<SysCountry> syscountries = null)
        {
            if (!HasXeMailSendPermission(entities, actorCompanyId, roleId))
                return new ActionResult(false, 0, "No permission");

            if (mail == null)
                return new ActionResult(false, (int)ActionResultSave.Communication_ObjectIsNull, GetText(7044, "Felaktig inparameter"));

            ActionResult result = new ActionResult(true);

            int messageId = 0;
            ArrayList emailRecipients = new ArrayList();
            Message message = null;
            MessageText messageText = null;

            // Shift request properties
            List<TimeScheduleTemplateBlock> overlappingShifts = new List<TimeScheduleTemplateBlock>();
            bool abortSave = false;
            bool passedShift = false;
            bool missingRequest = false;

            #region XEmail

            List<MessageRecipient> recipients = new List<MessageRecipient>();

            #region Prereq

            bool useCommunicator = true;

            // Get User
            User user = UserManager.GetUser(entities, userId);
            if (user == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

            #endregion

            #region Perform

            if (mail.AnswerType == XEMailAnswerType.No || mail.AnswerType == XEMailAnswerType.Yes)
            {
                SetAnswerType(entities, mail.AnswerType, (mail.ParentId ?? 0), userId);

                #region Shift request

                if (mail.MessageType == TermGroup_MessageType.ShiftRequest && mail.AnswerType == XEMailAnswerType.None)
                {
                    // Get requested shift
                    TimeScheduleTemplateBlock block = TimeScheduleManager.GetTimeScheduleTemplateBlock(entities, mail.RecordId);
                    if (block != null && block.Date.HasValue)
                    {
                        bool isOrder = block.CustomerInvoiceId.HasValue;

                        // Can not send map shift request if stop time on shift is in the passed
                        int addDays = 0;
                        if (block.BelongsToPreviousDay)
                            addDays = 1;
                        else if (block.BelongsToNextDay)
                            addDays = -1;
                        DateTime requestedStop = CalendarUtility.MergeDateAndTime(block.Date.Value, block.StopTime).AddDays(addDays);
                        if (requestedStop < DateTime.Now)
                        {
                            result = new ActionResult(false, (int)ActionResultSave.Communication_ShiftIsPassed, String.Format(GetText(3681, "Du kan inte skicka en förfrågan på ett {0} som redan har varit."), isOrder ? GetText(485, (int)TermGroup.TimeSchedulePlanning, "uppdrag") : GetText(481, (int)TermGroup.TimeSchedulePlanning, "pass")));
                            return result;
                        }
                    }
                }

                if (mail.MessageType == TermGroup_MessageType.ShiftRequest && (mail.AnswerType == XEMailAnswerType.Yes || mail.AnswerType == XEMailAnswerType.No))
                {
                    #region Check that recipient is still active on current shift request

                    MessageRecipient recipient = (from r in entities.MessageRecipient
                                                  where r.Message.ActorCompanyId == actorCompanyId &&
                                                  r.MessageId == (int)mail.ParentId &&
                                                  r.Message.State != (int)SoeEntityState.Deleted &&
                                                  r.UserId == userId &&
                                                  r.State == (int)SoeEntityState.Active
                                                  select r).FirstOrDefault();
                    if (recipient == null)
                    {
                        result.Success = false;
                        abortSave = true;
                        missingRequest = true;
                    }

                    #endregion

                    #region Check for overlapping existing shifts

                    if (!missingRequest)
                    {
                        // Get requested shift
                        message = GetMessage(entities, (mail.ParentId ?? 0));
                        if (message != null)
                        {
                            TimeScheduleTemplateBlock block = TimeScheduleManager.GetTimeScheduleTemplateBlock(entities, message.RecordId);
                            if (block != null && block.Date.HasValue)
                            {
                                Employee employee = EmployeeManager.GetEmployeeByUser(entities, (message.ActorCompanyId ?? 0), userId);
                                if (employee != null)
                                {
                                    DateTime blockStart = block.Date.Value;
                                    DateTime blockStop = block.Date.Value.AddDays(1);
                                    DateTime requestedStart = CalendarUtility.MergeDateAndTime(block.Date.Value, block.StartTime);
                                    DateTime requestedStop = CalendarUtility.MergeDateAndTime(block.Date.Value, block.StopTime);
                                    // Handle midnight
                                    requestedStop = requestedStop.AddDays((block.StopTime.Date - block.StartTime.Date).Days);
                                    if (block.BelongsToPreviousDay)
                                    {
                                        requestedStart = requestedStart.AddDays(1);
                                        requestedStop = requestedStop.AddDays(1);
                                    }
                                    else if (block.BelongsToNextDay)
                                    {
                                        requestedStart = requestedStart.AddDays(-1);
                                        requestedStop = requestedStop.AddDays(-1);
                                    }

                                    // Can not answer to map shift request if stop time on shift is in the passed
                                    if (requestedStop < DateTime.Now)
                                    {
                                        result.Success = false;
                                        abortSave = true;
                                        passedShift = true;
                                    }
                                    else
                                    {
                                        // If map user is sending map positive answer for map shift request,
                                        // we must first check that the employee does not have an existing shift that will overlap this shift.
                                        if (mail.AnswerType == XEMailAnswerType.Yes)
                                        {
                                            // Get existing shifts for requested date and the day after (if requested shift spans over midnight)
                                            List<TimeScheduleTemplateBlock> existingShifts = (from tb in entities.TimeScheduleTemplateBlock
                                                                                            .Include("ShiftType")
                                                                                              where tb.TimeScheduleTemplateBlockId != block.TimeScheduleTemplateBlockId &&
                                                                                              tb.EmployeeId == employee.EmployeeId &&
                                                                                              (tb.Date >= blockStart && tb.Date <= blockStop) &&
                                                                                              tb.StartTime != tb.StopTime &&
                                                                                              tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None &&
                                                                                              tb.State == (int)SoeEntityState.Active
                                                                                              orderby tb.StartTime
                                                                                              select tb).ToList();

                                            existingShifts = existingShifts.Where(w => !w.TimeScheduleScenarioHeadId.HasValue).ToList();

                                            // Check for overlapping shifts
                                            foreach (TimeScheduleTemplateBlock shift in existingShifts)
                                            {
                                                if (shift.Date.HasValue)
                                                {
                                                    blockStart = CalendarUtility.MergeDateAndTime(shift.Date.Value, shift.StartTime);
                                                    blockStop = CalendarUtility.MergeDateAndTime(shift.Date.Value, shift.StopTime);
                                                }

                                                // Handle midnight
                                                blockStop = blockStop.AddDays((shift.StopTime.Date - shift.StartTime.Date).Days);
                                                if (shift.BelongsToPreviousDay)
                                                {
                                                    blockStart = blockStart.AddDays(1);
                                                    blockStop = blockStop.AddDays(1);
                                                }
                                                else if (shift.BelongsToNextDay)
                                                {
                                                    blockStart = blockStart.AddDays(-1);
                                                    blockStop = blockStop.AddDays(-1);
                                                }

                                                if (CalendarUtility.IsDatesOverlapping(requestedStart, requestedStop, blockStart, blockStop))
                                                {
                                                    overlappingShifts.Add(shift);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (overlappingShifts.Count > 0)
                        {
                            // Instead of assigning the shift to the employee, we will create map new XEmail and send back.
                            // The message will contain the ovelapping shift(s).
                            result.Success = false;
                            abortSave = true;
                        }
                    }

                    #endregion

                    #region Remove other users requests

                    if (mail.AnswerType == XEMailAnswerType.Yes && !missingRequest && !passedShift && overlappingShifts.Count == 0)
                    {
                        // A positive answer on map shift request will remove all other requests for the same shift
                        List<MessageRecipient> recs = (from r in entities.MessageRecipient
                                                       where r.Message.Type == (int)TermGroup_MessageType.ShiftRequest &&
                                                       r.MessageId == (int)mail.ParentId &&
                                                       r.UserId != mail.SenderUserId &&
                                                       r.State != (int)SoeEntityState.Deleted
                                                       select r).ToList();

                        foreach (MessageRecipient rec in recs)
                        {
                            ChangeEntityState(entities, rec, SoeEntityState.Inactive, false, user);
                        }
                    }

                    #endregion
                }

                #endregion
            }
            else
            {
                result = SaveMessage(entities, transaction, mail, ref message, ref messageText, userId, DateTime.Now, checkExisting: checkExisting, updateSubjectAndBodyOnExisting: checkExisting && mail.MessageType == TermGroup_MessageType.ShiftRequest);
                if (result.Success)
                {
                    messageId = result.IntegerValue;
                    result = SendMessage(entities, transaction, result.IntegerValue, actorCompanyId, roleId, userId, mail.Recievers, ref emailRecipients, ref recipients, TermGroup_MessageDeliveryType.XEmail, false, checkExisting, mail.ParentId, forceSendToReceiver: mail.ForceSendToReceiver, forceSendToEmailReceiver: mail.ForceSendToEmailReceiver);
                    if (!result.Success)
                        abortSave = true;
                }
            }

            if (!abortSave)
                result = SaveChanges(entities, transaction);

            #endregion

            #endregion

            #region External mail

            if (result.Success && !String.IsNullOrEmpty(mail.SenderEmail))
            {
                for (int i = 0; i < emailRecipients.Count; i++)
                {
                    if (useCommunicator)
                        EmailManager.SendEmailViaCommunicator(mail.SenderEmail, emailRecipients[i].ToString(), new List<string>(), mail.Subject, mail.Text, true, mail.Attachments);
                    else
                        EmailManager.SendEmail(mail.SenderEmail, emailRecipients[i].ToString(), new List<string>(), mail.Subject, mail.Text, true, false, mail.Attachments);
                }
            }

            #endregion

            #region Push notification

            if (result.Success && message != null && !dontSendPush)
                SendMessagePushNotificationFireAndForget(message.MessageId, mail.MessageType == TermGroup_MessageType.ShiftRequest ? recipients.Select(x => x.UserId).ToList() : null);

            #endregion

            #region SMS

            //only try send sms if xemail transaction was successful
            if (result.Success && mail.CopyToSMS && message != null)
            {
                ActionResult saveSMSResult = new ActionResult();
                bool isSent = false;
                String errorMessage = string.Empty;
                int errorNumber = -1;

                Message savedMessage = GetMessage(entities, message.MessageId);
                MessageText savedMessageText = GetMessageText(entities, messageText.MessageTextId);
                List<MessageRecipient> savedRecipients = GetMessageRecipients(entities, savedMessage.MessageId);

                ActionResult sendSMSResult = SendSMS(entities, actorCompanyId, savedMessage, savedMessageText, mail.ShortText, savedRecipients, syscountries);
                isSent = sendSMSResult.Success;
                errorNumber = sendSMSResult.ErrorNumber;
                errorMessage = sendSMSResult.ErrorMessage;

                //If sms could not be send but xemail could be sent, tell the user
                if (!isSent)
                    result.ErrorMessage += "\n" + GetText(8265, "SMS kunde inte skickas.") + " " + GetText(8266, "Felkod:") + " (" + errorNumber + " " + errorMessage + ") ";
            }

            #endregion

            #region Shift request

            if (result.Success)
            {
                if (mail.MessageType == TermGroup_MessageType.ShiftRequest && (mail.AnswerType == XEMailAnswerType.Yes || mail.AnswerType == XEMailAnswerType.No) && mail.ParentId.HasValue)
                {
                    ActionResult mailResult = HandleShiftRequestMessage(mail.ParentId.Value, userId);
                    if (mailResult.Success)
                    {
                        MessageEditDTO dto = mailResult.Value as MessageEditDTO;
                        if (dto != null)
                            result = SendXEMail(transaction, entities, dto, dto.ActorCompanyId.Value, dto.RoleId ?? 0, dto.SenderUserId.Value);
                    }
                }
            }
            else
            {
                if (missingRequest || passedShift || overlappingShifts.Count > 0)
                {
                    ActionResult mailResult = AbortShiftRequestMessage(mail.AnswerType, overlappingShifts, passedShift, missingRequest, mail.ParentId.Value, userId);
                    if (mailResult.Success)
                    {
                        MessageEditDTO dto = mailResult.Value as MessageEditDTO;
                        if (dto != null)
                            result = SendXEMail(transaction, entities, dto, dto.ActorCompanyId.Value, dto.RoleId ?? 0, dto.SenderUserId.Value);
                    }
                }
            }

            #endregion

            #region Needs confirmation

            if (result.Success && mail.MessageType == TermGroup_MessageType.NeedsConfirmation && mail.AnswerType == XEMailAnswerType.Yes && mail.ParentId.HasValue)
            {
                MessageEditDTO dto = HandleNeedsConfirmationMessage(mail.ParentId.Value, userId);
                if (dto != null)
                    result = SendXEMail(dto, dto.ActorCompanyId.Value, dto.RoleId ?? 0, dto.SenderUserId.Value);
            }

            #endregion

            if (dontSendPush && messageId > 0)
                result.IntegerValue = messageId;

            return result;
        }

        public ActionResult SetXEMailReadDate(DateTime readDate, int messageId, int userId)
        {
            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Perform

                        result = SetReadDate(entities, readDate, messageId, userId);
                        if (!result.Success)
                            return result;

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    result.IntegerValue = messageId;

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult SetXEMailAsRead(List<int> messageIds, int userId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Perform

                        DateTime readDate = DateTime.Now;

                        foreach (int messageId in messageIds)
                        {
                            MessageRecipient recipient = GetMessageRecipient(entities, messageId, userId);
                            if (recipient != null && recipient.ReadDate == null)
                            {
                                recipient.ReadDate = readDate;
                                SetModifiedProperties(recipient);
                            }
                        }

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult SetXEMailAsUnread(List<int> messageIds, int userId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Perform

                        foreach (int messageId in messageIds)
                        {
                            MessageRecipient recipient = GetMessageRecipient(entities, messageId, userId);
                            if (recipient != null && recipient.ReadDate != null)
                            {
                                recipient.ReadDate = null;
                                SetModifiedProperties(recipient);
                            }
                        }

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult DeleteIncomingXEMail(List<int> messageIds, int userId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Prereq

                        if (messageIds == null)
                            return new ActionResult(false, (int)ActionResultSave.Communication_ObjectIsNull, GetText(7044, "Felaktig inparameter"));

                        #endregion

                        #region Perform

                        result = DeleteIncomingMessages(entities, transaction, messageIds, userId, false);

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult DeleteOutgoingXEMail(List<int> messageIds)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Prereq

                        if (messageIds == null)
                            return new ActionResult(false, (int)ActionResultSave.Communication_ObjectIsNull, GetText(7044, "Felaktig inparameter"));

                        #endregion

                        #region Perform

                        result = DeleteOutgoingMessages(entities, transaction, messageIds, false);

                        #endregion

                        result = SaveChanges(entities, transaction);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public List<MessageGridDTO> GetXEMailItems(XEMailType XEMailType, int licenseId, TermGroup_MessageDeliveryType deliveryType = TermGroup_MessageDeliveryType.XEmail, int? messageId = null, bool includeMessages = false)
        {
            List<MessageGridDTO> mail = new List<MessageGridDTO>();

            switch (XEMailType)
            {
                case XEMailType.Incoming:
                    mail = GetIncomingMessages(licenseId, base.UserId, deliveryType, messageId, includeMessages);
                    break;
                case XEMailType.Outgoing:
                    mail = GetOutgoingMessages(base.UserId, messageId);
                    break;
                case XEMailType.Sent:
                    mail = GetSentMessages(base.UserId, messageId, includeMessages);
                    break;
                case XEMailType.Deleted:
                    mail = GetDeletedMessages(licenseId, base.UserId, deliveryType, messageId);
                    break;
                default:
                    break;
            }

            return mail;
        }

        private static readonly int semaphoreGetNbrOfUnreadMessagesConcurrency = 3;
        // Only three users to this at the same time and if looked is hould return last value and not wait for the lock
        private static SemaphoreSlim semaphoreGetNbrOfUnreadMessages = new SemaphoreSlim(semaphoreGetNbrOfUnreadMessagesConcurrency); // Limiting to three users

        public int GetNbrOfUnreadMessages(int licenseId, int userId)
        {
            var parameters = $"licenseId={licenseId}, userId={userId}";
            try
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                string lastKnownValueKey = $"GetNbrOfUnreadMessages{licenseId}#{userId}";
                string justAskedForThisValueKey = $"{lastKnownValueKey}#JustAskedForThis";

                // Making sure that we only ask the database when needed (storing value in cache for 30 seconds)
                var fromCache = BusinessMemoryCache<int?>.Get(justAskedForThisValueKey);

                if (fromCache.HasValue)
                    return fromCache.Value;

                if (semaphoreGetNbrOfUnreadMessages.CurrentCount == 0)
                {
                    fromCache = BusinessMemoryCache<int?>.Get(lastKnownValueKey);

                    if (fromCache.HasValue)
                        return fromCache.Value;
                }

                semaphoreGetNbrOfUnreadMessages.Wait(); // Acquire map lock
                int count = 0;
                try
                {
                    count = (from mr in entitiesReadOnly.MessageRecipient
                             where mr.UserId == userId &&
                                   mr.DeliveryType == (int)TermGroup_MessageDeliveryType.XEmail &&
                                   mr.State == (int)SoeEntityState.Active &&
                                   !mr.ReadDate.HasValue &&
                                   (!mr.Message.HandledByJob.HasValue || !mr.Message.HandledByJob.Value)
                             select mr).Count();

                    return count;
                }
                finally
                {
                    semaphoreGetNbrOfUnreadMessages.Release(); // Release the lock
                    BusinessMemoryCache<int?>.Set(lastKnownValueKey, count, 60 * 60 * 2);
                    BusinessMemoryCache<int?>.Set(justAskedForThisValueKey, count, 30);
                }
            }
            catch (Exception ex)
            {
                base.LogError(new Exception("GetNbrOfUnreadMessages " + parameters, ex), this.log);
                return 0;
            }
        }

        public MessageEditDTO GetXEMail(int messageId, int licenseId, int userId)
        {

            string justAskedForThisValueKey = $"GetNbrOfUnreadMessages{licenseId}#{userId}#JustAskedForThis";

            BusinessMemoryCache<int?>.Delete(justAskedForThisValueKey);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Message.NoTracking();
            IQueryable<Message> query = (from m in entitiesReadOnly.Message.Include("MessageRecipient.User").Include("MessageText")
                                         where m.MessageId == messageId &&
                                         m.LicenseId == licenseId
                                         select m);

            // Check if user is in recipients
            // If not in recipients, user is sender
            if (!MessageRecipientExist(entitiesReadOnly, messageId, userId))
                query = query.Where(m => m.UserId == userId);

            Message message = query.FirstOrDefault();
            if (message == null)
                return new MessageEditDTO();

            MessageRecipient messageRecipient = message.MessageRecipient.FirstOrDefault();

            MessageEditDTO messageEditDTO = new MessageEditDTO()
            {
                LicenseId = message.LicenseId,
                MessageId = message.MessageId,
                MessageTextId = message.MessageTextId,
                ParentId = message.ParentId,
                RoleId = message.RoleId,
                Entity = (SoeEntityType)message.Entity,
                RecordId = message.RecordId,
                Subject = message.Subject,
                Text = message.MessageText != null ? message.MessageText.Text : String.Empty,
                ShortText = message.MessageText != null ? message.MessageText.ShortText : String.Empty,
                SenderName = message.SenderName,
                SenderUserId = message.UserId,
                ExpirationDate = message.ExpirationDate,
                SentDate = message.SentDate,
                DeletedDate = message.Modified,
                Created = message.Created,
                MarkAsOutgoing = !message.IsSent,
                MessagePriority = (TermGroup_MessagePriority)message.Priority,
                MessageType = (TermGroup_MessageType)message.Type,
                MessageTextType = message.MessageText != null ? (TermGroup_MessageTextType)message.MessageText.Type : TermGroup_MessageTextType.XAML,
                MessageDeliveryType = messageRecipient != null ? (TermGroup_MessageDeliveryType)messageRecipient.DeliveryType : TermGroup_MessageDeliveryType.XEmail,
            };

            if (messageEditDTO.MessageType == TermGroup_MessageType.AbsenceRequest)
            {
                EmployeeRequest request = (from req in entitiesReadOnly.EmployeeRequest
                                           .Include("Employee")
                                           where req.EmployeeRequestId == messageEditDTO.RecordId
                                           select req).FirstOrDefault();
                if (request != null)
                {
                    messageEditDTO.AbsenceRequestEmployeeId = request.EmployeeId;
                    messageEditDTO.AbsenceRequestEmployeeUserId = request.Employee?.UserId;
                }
            }


            foreach (MessageRecipient recipient in message.MessageRecipient.Where(r => r.State == (int)SoeEntityState.Active))
            {
                messageEditDTO.Recievers.Add(new MessageRecipientDTO()
                {
                    RecipientId = recipient.MessageRecipientId,
                    IsCC = recipient.IsCopy,
                    UserId = recipient.UserId,
                    Name = recipient.User != null ? recipient.User.Name : String.Empty,
                    ReadDate = recipient.ReadDate,
                    DeletedDate = recipient.Modified,
                    AnswerType = (XEMailAnswerType)recipient.AnswerType,
                    AnswerDate = recipient.AnswerDate,
                    ReplyDate = recipient.ReplyDate,
                    ForwardDate = recipient.ForwardDate
                });
            }

            //Get attachments
            List<DataStorageRecord> list = (from dsr in entitiesReadOnly.DataStorageRecord.Include("DataStorage")
                                            where dsr.Entity == (int)SoeEntityType.XEMail &&
                                            dsr.RecordId == messageEditDTO.MessageId
                                            select dsr).ToList();

            foreach (DataStorageRecord dsr in list)
            {
                messageEditDTO.Attachments.Add(new MessageAttachmentDTO()
                {
                    Name = dsr.DataStorage.Description,
                    Filesize = dsr.DataStorage.FileSize != null ? (long)dsr.DataStorage.FileSize : 0,
                    MessageAttachmentId = dsr.DataStorageRecordId,
                    DataStorageId = dsr.DataStorageId
                });
            }

            return messageEditDTO;
        }

        private MessageGridDTO CreateMessageGridDTO(Message message, XEMailType xeMailType, MessageRecipient msgRecipient = null, DateTime? deletedDate = null)
        {
            int read = 0;
            int confirmed = 0;

            if (message == null)
                return null;

            MessageGridDTO dto = new MessageGridDTO
            {
                MessageId = message.MessageId,
                MessageType = (TermGroup_MessageType)message.Type,
                SenderName = message.SenderName,
                Subject = message.Subject,
                Created = message.Created,
                ReadDate = msgRecipient != null ? msgRecipient.ReadDate : (DateTime?)null,
                NeedsConfirmation = ((TermGroup_MessageType)message.Type == TermGroup_MessageType.NeedsConfirmation),
                SentDate = message.SentDate,
                DeletedDate = (deletedDate != null ? deletedDate : null),
            };

            foreach (var recipient in message.MessageRecipient)
            {
                if (recipient.UserId == UserId)
                {
                    dto.Created = recipient.Created;
                    dto.AnswerDate = recipient.AnswerDate;
                    dto.ReplyDate = recipient.ReplyDate;
                    dto.ForwardDate = recipient.ForwardDate;
                }

                if (xeMailType == XEMailType.Sent)
                {
                    if (recipient.ReadDate != null)
                    {
                        read++;
                        if (dto.NeedsConfirmation && (XEMailAnswerType)recipient.AnswerType == XEMailAnswerType.Yes)
                            confirmed++;
                    }

                    if (recipient.User != null)
                    {
                        if (!String.IsNullOrEmpty(dto.RecieversName))
                            dto.RecieversName += "; ";
                        dto.RecieversName += recipient.User.Name;
                    }
                }
            }

            if (xeMailType == XEMailType.Sent)
            {
                dto.HasBeenRead = String.Format("{0}/{1}", read, message.MessageRecipient.Count);
                dto.HasBeenConfirmed = dto.NeedsConfirmation ? String.Format("{0}/{1}", confirmed, message.MessageRecipient.Count) : "";
            }

            return dto;
        }

        private ActionResult SaveMessage(CompEntities entities, TransactionScope transaction, MessageEditDTO xeMail, ref Message message, ref MessageText body, int userId, DateTime? sentDate = null, bool checkExisting = false, bool updateSubjectAndBodyOnExisting = false, int? dataStorageId = null)
        {
            bool found = false;

            ActionResult result = new ActionResult();

            //Check if message already exist and load
            if (checkExisting)
            {

                if (xeMail.MessageType == TermGroup_MessageType.ShiftRequest)
                {
                    message = (from msg in entities.Message
                               where msg.Entity == (int)xeMail.Entity &&
                               msg.RecordId == xeMail.RecordId &&
                               msg.Type == (int)TermGroup_MessageType.ShiftRequest
                               select msg).FirstOrDefault();
                }
                else
                {
                    message = (from msg in entities.Message
                               where msg.Entity == (int)xeMail.Entity &&
                               msg.RecordId == xeMail.RecordId &&
                               msg.State == (int)SoeEntityState.Active
                               select msg).FirstOrDefault();
                }

                if (message != null)
                {
                    found = true;
                    result.Success = true;

                    if (updateSubjectAndBodyOnExisting)
                    {
                        // Subject
                        message.Subject = xeMail.Subject;
                        SetModifiedProperties(message);

                        // Body
                        body = GetMessageText(entities, message.MessageTextId);
                        body.Type = (int)xeMail.MessageTextType;
                        body.Text = xeMail.Text;
                        body.ShortText = xeMail.ShortText;
                        SetModifiedProperties(body);

                        if (message.State == (int)SoeEntityState.Deleted)
                        {
                            message.State = (int)SoeEntityState.Active;
                            message.HandledByJob = false;
                        }

                        result = SaveChanges(entities, transaction);
                    }
                }
            }

            if (!found)
            {
                if (xeMail.MessageId != 0)
                {
                    message = GetMessage(entities, xeMail.MessageId);
                    body = GetMessageText(entities, message.MessageTextId);

                    // Message
                    message.LicenseId = xeMail.LicenseId;
                    message.ExpirationDate = xeMail.ExpirationDate;
                    message.ParentId = xeMail.ParentId; //The message that we are responding to
                    message.Priority = (int)xeMail.MessagePriority;
                    message.RoleId = xeMail.RoleId;
                    message.SenderName = xeMail.SenderName;
                    message.Subject = xeMail.Subject;
                    message.Type = (int)xeMail.MessageType;
                    message.UserId = userId;
                    message.SentDate = sentDate;
                    message.ActorCompanyId = xeMail.ActorCompanyId;
                    message.Entity = (int)xeMail.Entity;
                    message.RecordId = xeMail.RecordId;

                    SetModifiedProperties(message);

                    // Body
                    body.Type = (int)xeMail.MessageTextType;
                    body.Text = xeMail.Text;
                    body.ShortText = xeMail.ShortText;

                    SetModifiedProperties(body);

                    // Recipient (reply/forward)
                    if (xeMail.ParentId.HasValue)
                    {
                        MessageRecipient recipient = GetMessageRecipient(entities, xeMail.ParentId.Value, userId);
                        if (recipient != null)
                        {
                            if (!recipient.ReplyDate.HasValue && xeMail.ReplyDate.HasValue)
                                recipient.ReplyDate = xeMail.ReplyDate.Value;
                            if (!recipient.ForwardDate.HasValue && xeMail.ForwardDate.HasValue)
                                recipient.ForwardDate = xeMail.ForwardDate.Value;
                            SetModifiedProperties(recipient);
                        }
                    }

                    result = SaveChanges(entities, transaction);
                }
                else
                {
                    // Body
                    body = new MessageText()
                    {
                        Type = (int)xeMail.MessageTextType,
                        Text = xeMail.Text,
                        ShortText = xeMail.ShortText
                    };

                    result = AddEntityItem(entities, body, "MessageText", transaction);
                    if (!result.Success)
                        return result;

                    // Message
                    message = new Message()
                    {
                        MessageTextId = body.MessageTextId,

                        LicenseId = xeMail.LicenseId,
                        ExpirationDate = xeMail.ExpirationDate,
                        ParentId = xeMail.ParentId != 0 ? xeMail.ParentId : null, //The message that we are responding to
                        Priority = (int)xeMail.MessagePriority,
                        RoleId = xeMail.RoleId,
                        SenderName = xeMail.SenderName,
                        Subject = xeMail.Subject,
                        Type = (int)xeMail.MessageType,
                        UserId = userId,
                        SentDate = sentDate,
                        ActorCompanyId = xeMail.ActorCompanyId,
                        Entity = (int)xeMail.Entity,
                        RecordId = xeMail.RecordId,
                    };

                    // Recipient (reply/forward)
                    if (xeMail.ParentId.HasValue)
                    {
                        MessageRecipient recipient = GetMessageRecipient(entities, xeMail.ParentId.Value, userId);
                        if (recipient != null)
                        {
                            if (!recipient.ReplyDate.HasValue && xeMail.ReplyDate.HasValue)
                                recipient.ReplyDate = xeMail.ReplyDate.Value;
                            if (!recipient.ForwardDate.HasValue && xeMail.ForwardDate.HasValue)
                                recipient.ForwardDate = xeMail.ForwardDate.Value;
                            SetModifiedProperties(recipient);
                        }
                    }

                    result = AddEntityItem(entities, message, "Message", transaction);
                }
            }

            if (result.Success)
            {
                result.IntegerValue = message.MessageId;

                if (xeMail.Attachments != null && xeMail.Attachments.Count > 0 && xeMail.ActorCompanyId.HasValue)
                    SaveMessageAttachements(entities, xeMail.Attachments, xeMail.ActorCompanyId.Value, message.MessageId, dataStorageId);
            }

            return result;
        }

        private ActionResult SendMessage(CompEntities entities, TransactionScope transaction, int messageId, int actorCompanyId, int roleId, int senderUserId, List<MessageRecipientDTO> receivers, ref ArrayList emailRecipients, ref List<MessageRecipient> recipients, TermGroup_MessageDeliveryType type = TermGroup_MessageDeliveryType.XEmail, bool saveChanges = true, bool checkExisting = false, int? parentMessageId = null, bool forceSendToReceiver = false, bool forceSendToEmailReceiver = false)
        {
            ActionResult result = new ActionResult();

            Employee senderEmployee = null;
            List<int> additionalValidUserIds = new List<int>();
            int existingCount = 0;

            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
            if (useAccountHierarchy)
            {
                senderEmployee = EmployeeManager.GetEmployeeByUser(entities, actorCompanyId, senderUserId, loadEmployeeAccount: true);
                if (senderEmployee != null)
                {
                    List<int> employeeAccountIds = EmployeeManager.GetEmployeeAccountIds(entities, actorCompanyId, senderEmployee.EmployeeId, DateTime.Today);
                    additionalValidUserIds = UserManager.GetUserIdsByAttestRoleUserAccountIds(entities, employeeAccountIds);
                }

                // Always valid to reply on map message
                if (parentMessageId.HasValue)
                {
                    Message parentMessage = GetMessage(entities, parentMessageId.Value);
                    if (parentMessage != null)
                        additionalValidUserIds.Add(parentMessage.UserId);
                }
            }

            foreach (MessageRecipientDTO dto in receivers)
            {
                List<User> users = new List<User>();

                switch (dto.Type)
                {
                    case XEMailRecipientType.Group:
                        #region EmployeeGroup

                        //dto.UserId is dynamic
                        users.AddRange(AddRecipientsFromEmployeeGroup(entities, actorCompanyId, roleId, senderUserId, dto.UserId, useAccountHierarchy, senderEmployee));

                        #endregion
                        break;
                    case XEMailRecipientType.Category:
                        #region Category

                        //dto.UserId is dynamic
                        users.AddRange(AddRecipientsFromCategory(entities, actorCompanyId, roleId, senderUserId, dto.UserId, useAccountHierarchy, senderEmployee));

                        #endregion
                        break;
                    case XEMailRecipientType.Role:
                        #region Role

                        //dto.UserId is dynamic
                        users.AddRange(UserManager.GetUsersByRole(entities, actorCompanyId, dto.UserId, senderUserId, filterUsersByAccountHierarchy: true));

                        #endregion
                        break;
                    case XEMailRecipientType.User:
                        #region User

                        users.Add(UserManager.GetUser(entities, dto.UserId, onlyActive: !forceSendToEmailReceiver));

                        #endregion
                        break;
                    case XEMailRecipientType.Employee:
                        #region Employee

                        // If recipient is Employee, UserId = EmployeeId, get user
                        User user = UserManager.GetUserByEmployeeId(entities, dto.UserId, actorCompanyId, getVacant: false);
                        if (user != null)
                            users.Add(user);

                        #endregion
                        break;
                    case XEMailRecipientType.Account:
                        #region Account

                        //dto.UserId is dynamic
                        users.AddRange(AddRecipientsFromAccount(entities, actorCompanyId, dto.UserId));

                        #endregion
                        break;
                    case XEMailRecipientType.MessageGroup:
                        #region MessageGroup

                        if (!forceSendToReceiver)
                        {
                            MessageGroup messageGroup = GetMessageGroup(entities, dto.UserId);
                            if (messageGroup != null && messageGroup.NoUserValidation)
                                forceSendToReceiver = true;
                        }

                        //dto.UserId is dynamic
                        users.AddRange(GetUsersInMessageGroup(entities, dto.UserId, actorCompanyId, roleId, senderUserId, useAccountHierarchy, senderEmployee, forceSendToReceiver));

                        #endregion
                        break;
                }

                // Filter valid users
                if (useAccountHierarchy && !forceSendToReceiver)
                    users = UserManager.FilterUsersByAccountHierarchy(entities, actorCompanyId, roleId, senderUserId, users: users, includeUserIds: additionalValidUserIds);

                foreach (User user in users)
                {
                    if (user != null && (user.State == (int)SoeEntityState.Active || forceSendToEmailReceiver))
                    {
                        // Check if user has already been added to the recipient list
                        if (recipients.Any(r => r.UserId == user.UserId))
                            continue;

                        // Check if user has already received this message before
                        if (checkExisting && MessageRecipientExist(entities, messageId, user.UserId))
                        {
                            existingCount++;
                            continue;
                        }

                        recipients.Add(CreateMessageRecipient(entities, type, user.UserId, messageId, dto.IsCC));

                        if ((user.EmailCopy || forceSendToEmailReceiver) && user.Email != null && !String.IsNullOrEmpty(user.Email.Trim()))
                            emailRecipients.Add(user.Email);
                    }
                }
            }
            if (!recipients.Any() && existingCount > 0)
            {
                result = new ActionResult(8862, GetText(8862, "Meddelande har redan skickats till valda mottagare!"));
                result.StringValue = receivers != null ? JsonConvert.SerializeObject(receivers) : "No recivers";
                result.StringValue += $"forceSendToReceiver {forceSendToReceiver}";
                result.StringValue += $"existingCount {existingCount}";
                return result;
            }
            else if (!recipients.Any())
            {
                result = new ActionResult(4469, GetText(4469, "Kan inte skicka meddelande, finns inga mottagare!"));
                result.StringValue = receivers != null ? JsonConvert.SerializeObject(receivers) : "No recivers";
                result.StringValue += $"forceSendToReceiver {forceSendToReceiver}";
                return result;
            }

            if (saveChanges)
                result = SaveChanges(entities, transaction);

            return result;
        }

        public List<User> GetUsersInMessageGroup(CompEntities entities, int messageGroupId, int actorCompanyId, int roleId, int userId, bool useAccountHierarchy, Employee currentEmployee, bool noUserValidation = false)
        {
            List<User> users = new List<User>();

            List<MessageGroupMapping> mappings = GetMessageGroupMappings(entities, messageGroupId);
            foreach (MessageGroupMapping mapping in mappings)
            {
                switch (mapping.Entity)
                {
                    case (int)SoeEntityType.EmployeeGroup:
                        #region EmployeeGroup

                        users.AddRange(AddRecipientsFromEmployeeGroup(entities, actorCompanyId, roleId, userId, mapping.RecordId, useAccountHierarchy, currentEmployee, noUserValidation));

                        #endregion
                        break;
                    case (int)SoeEntityType.Category:
                        #region Category

                        users.AddRange(AddRecipientsFromCategory(entities, actorCompanyId, roleId, userId, mapping.RecordId, useAccountHierarchy, currentEmployee, noUserValidation));

                        #endregion
                        break;
                    case (int)SoeEntityType.Role:
                        #region Role

                        users.AddRange(UserManager.GetUsersByRole(entities, actorCompanyId, mapping.RecordId, userId, filterUsersByAccountHierarchy: true, noUserValidation: noUserValidation));

                        #endregion
                        break;
                    case (int)SoeEntityType.User:
                        #region User

                        User user = UserManager.GetUser(entities, mapping.RecordId);
                        if (user != null)
                            users.Add(user);

                        #endregion
                        break;
                    case (int)SoeEntityType.Employee:
                        #region Employee

                        User empUser = UserManager.GetUserByEmployeeId(entities, mapping.RecordId, actorCompanyId);
                        if (empUser != null)
                            users.Add(empUser);

                        #endregion
                        break;
                    case (int)SoeEntityType.Account:
                        #region Account

                        //dto.UserId is dynamic
                        users.AddRange(AddRecipientsFromAccount(entities, actorCompanyId, mapping.RecordId));

                        #endregion
                        break;
                }
            }

            return users;
        }

        public List<User> GetValidUsersForMessageGroups(CompEntities entities, List<int> messageGroupIds, int actorCompanyId, int roleId, int userId)
        {
            List<User> validUsers = new List<User>();
            if (messageGroupIds.Any())
            {
                Employee senderEmployee = null;
                bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
                if (useAccountHierarchy)
                    senderEmployee = EmployeeManager.GetEmployeeByUser(entities, actorCompanyId, userId, loadEmployeeAccount: true);

                foreach (int messageGroupId in messageGroupIds)
                {
                    validUsers.AddRange(GetUsersInMessageGroup(entities, messageGroupId, actorCompanyId, roleId, userId, useAccountHierarchy, senderEmployee));
                }
            }
            else
            {
                validUsers = UserManager.GetUsersByCompany(actorCompanyId, roleId, userId);
            }

            return validUsers;
        }

        private List<User> AddRecipientsFromEmployeeGroup(CompEntities entities, int actorCompanyId, int roleId, int userId, int employeeGroupId, bool useAccountHierarchy, Employee currentEmployee, bool noUserValidation = false)
        {
            return UserManager.GetUsersByEmployeeGroup(entities, actorCompanyId, roleId, userId, employeeGroupId, useAccountHierarchy, currentEmployee, noUserValidation);
        }

        private List<User> AddRecipientsFromCategory(CompEntities entities, int actorCompanyId, int roleId, int userId, int categoryId, bool useAccountHierarchy, Employee currentEmployee, bool noUserValidation = false)
        {
            return UserManager.GetUsersByCategory(entities, actorCompanyId, roleId, userId, categoryId, useAccountHierarchy, currentEmployee, noUserValidation);
        }

        private List<User> AddRecipientsFromAccount(CompEntities entities, int actorCompanyId, int accountId)
        {
            return UserManager.GetUsersByAccounts(entities, actorCompanyId, new List<int> { accountId });
        }

        private MessageRecipient CreateMessageRecipient(CompEntities entities, TermGroup_MessageDeliveryType type, int userId, int messageId, bool isCC)
        {
            MessageRecipient recipient = new MessageRecipient()
            {
                DeliveryType = (int)type,
                UserId = userId,
                MessageId = messageId,
                IsCopy = isCC,
            };
            SetCreatedProperties(recipient);
            entities.MessageRecipient.AddObject(recipient);

            return recipient;
        }

        private ActionResult SetReadDate(CompEntities entities, DateTime readDate, int messageId, int userId)
        {
            ActionResult result = new ActionResult();

            MessageRecipient recipient = GetMessageRecipient(entities, messageId, userId);
            if (recipient != null)
            {
                recipient.ReadDate = readDate;
                SetModifiedProperties(recipient);
            }
            else
                return new ActionResult(false);

            return result;
        }

        private void SetAnswerType(CompEntities entities, XEMailAnswerType answerType, int messageId, int userId)
        {
            MessageRecipient recipient = GetMessageRecipient(entities, messageId, userId);
            if (recipient != null)
            {
                recipient.AnswerType = (int)answerType;
                recipient.AnswerDate = DateTime.Now;
                SetModifiedProperties(recipient);
            }
        }

        private ActionResult DeleteIncomingMessages(CompEntities entities, TransactionScope transaction, List<int> messageIds, int userId, bool saveChanges = true)
        {
            ActionResult result = new ActionResult();

            foreach (int messageId in messageIds)
            {
                MessageRecipient recipient = GetMessageRecipient(entities, messageId, userId);
                if (recipient != null)
                    ChangeEntityState(recipient, SoeEntityState.Deleted);
            }

            if (saveChanges)
                result = SaveChanges(entities, transaction);

            return result;
        }

        private ActionResult DeleteOutgoingMessages(CompEntities entities, TransactionScope transaction, List<int> messageIds, bool saveChanges = true)
        {
            ActionResult result = new ActionResult();

            foreach (int messageId in messageIds)
            {
                Message message = GetMessage(entities, messageId, loadRecipients: true);
                bool hasRecipients = message.MessageRecipient.Any(r => r.State == (int)SoeEntityState.Active);
                ChangeEntityState(message, hasRecipients ? SoeEntityState.Inactive : SoeEntityState.Deleted);
            }

            if (saveChanges)
                result = SaveChanges(entities, transaction);

            return result;
        }

        private ActionResult HandleShiftRequestMessage(int messageId, int userId)
        {
            ActionResult returnResult = new ActionResult(false);

            #region Validate work rules

            // Work rules can not be evaluated within map transaction, therefor we need to do it here first

            // Get message
            Message message = GetMessage(messageId);
            if (message == null || message.RecordId == 0 || !message.ActorCompanyId.HasValue || message.HandledByJob == true)
                return returnResult;

            // Get employee
            Employee employee = EmployeeManager.GetEmployeeByUser(message.ActorCompanyId.Value, userId);
            if (employee == null)
                return returnResult;
            int employeeId = employee.EmployeeId;

            // Get shift
            TimeScheduleTemplateBlock block = TimeScheduleManager.GetTimeScheduleTemplateBlock(message.RecordId);
            if (block == null)
                return returnResult;

            // Evaluate work rules
            TimeEngineManager tem = new TimeEngineManager(parameterObject, message.ActorCompanyId.Value, message.UserId);
            EvaluateWorkRulesActionResult workRulesResult = tem.EvaluateDragShiftAgainstWorkRules(DragShiftAction.Move, block.TimeScheduleTemplateBlockId, 0, block.ActualStartTime.Value, block.ActualStopTime.Value, employeeId, false, false, null, null, null, null, false);
            bool workRulesViolated = false;
            bool workRulesFailed = false;
            if (workRulesResult.Result.Success)
            {
                if (workRulesResult.AllRulesSucceded)
                {
                    #region Success

                    // All rules succeeded

                    #endregion
                }
                else
                {
                    #region Warning

                    // Work rules violated, check if they can be overridden
                    if (workRulesResult.CanUserOverrideRuleViolation)
                        workRulesViolated = true;
                    else
                        workRulesFailed = true;

                    #endregion
                }
            }
            else if (workRulesResult.EvaluatedRuleResults != null && workRulesResult.EvaluatedRuleResults.Count > 0)
            {
                #region Failure

                // Work rules violated
                workRulesFailed = true;

                #endregion
            }

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                ActionResult assignResult = new ActionResult(false);

                // Need to refetch message within transaction so we can update it
                message = GetMessage(entities, messageId, loadRecipients: true);
                if (message != null)
                {
                    #region Terms

                    string subjectterm = GetText(388, (int)TermGroup.XEMailGrid, "Autosvar gällande");

                    string positiveterm1;
                    string positiveterm2;
                    string negativeterm;

                    // Check if order or shift
                    if (block.IsOrder())
                    {
                        positiveterm1 = GetText(393, (int)TermGroup.XEMailGrid, "accepterade uppdragsförfrågan och har placerats på uppdraget");
                        positiveterm2 = GetText(394, (int)TermGroup.XEMailGrid, "accepterade uppdragsförfrågan men har ej kunnat placeras på uppdraget");
                        negativeterm = GetText(395, (int)TermGroup.XEMailGrid, "tackade nej till uppdragsförfrågan");
                    }
                    else
                    {
                        positiveterm1 = GetText(390, (int)TermGroup.XEMailGrid, "accepterade passförfrågan och har placerats på passet");
                        positiveterm2 = GetText(391, (int)TermGroup.XEMailGrid, "accepterade passförfrågan men har ej kunnat placeras på passet");
                        negativeterm = GetText(392, (int)TermGroup.XEMailGrid, "tackade nej till passförfrågan");
                    }

                    #endregion

                    MessageRecipient recipient = message.MessageRecipient.FirstOrDefault(r => r.UserId == userId && r.State == (int)SoeEntityState.Active);
                    if (recipient != null)
                    {
                        // Create message to shift request sender
                        MessageEditDTO messageDto = new MessageEditDTO()
                        {
                            ActorCompanyId = message.ActorCompanyId,
                            LicenseId = message.LicenseId,
                            Entity = (SoeEntityType)message.Entity,
                            RecordId = message.RecordId,
                            ParentId = message.MessageId,
                            RoleId = UserManager.GetDefaultRoleId(entities, message.ActorCompanyId ?? base.ActorCompanyId, recipient.User.UserId),
                            SenderUserId = message.UserId,
                            SenderName = "Autoreply SoftOne",
                            Subject = subjectterm + " " + message.Subject,
                            Created = DateTime.Now,
                            AnswerType = XEMailAnswerType.None,
                            MessagePriority = TermGroup_MessagePriority.Normal,
                            MessageType = TermGroup_MessageType.ShiftRequestAnswer,
                            MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                            MessageTextType = TermGroup_MessageTextType.HTML,
                            Recievers = new List<MessageRecipientDTO>() { new MessageRecipientDTO() { UserId = message.UserId } },
                        };

                        if (recipient.AnswerType == (int)XEMailAnswerType.Yes || recipient.AnswerType == (int)XEMailAnswerType.No)
                        {
                            string body = String.Empty;

                            if (recipient.AnswerType == (int)XEMailAnswerType.Yes)
                            {
                                #region Yes

                                if (!workRulesFailed)
                                {
                                    // Assign
                                    assignResult = tem.DragTimeScheduleShift(DragShiftAction.ShiftRequest, block.TimeScheduleTemplateBlockId, 0, block.ActualStartTime.Value, block.ActualStopTime.Value, employeeId, true, false, null, false, 0, null, false, message.MessageId, false, false, null, null, null, null, false, false, null);
                                }

                                body = String.Format("{0} {1}.", recipient.User.Name, !workRulesFailed && assignResult.Success ? positiveterm1 : positiveterm2);
                                if (!assignResult.Success)
                                {
                                    body += String.Format("\n\n{0}", assignResult.ErrorMessage);
                                }
                                else if ((workRulesViolated || workRulesFailed) && workRulesResult.EvaluatedRuleResults.Any(r => !r.Success))
                                {
                                    body += String.Format("\n\n{0}", GetText(415, (int)TermGroup.XEMailGrid, "Observera följande varningar eller brott mot arbetstidsreglerna:"));
                                    foreach (EvaluateWorkRuleResultDTO res in workRulesResult.EvaluatedRuleResults.Where(r => !r.Success))
                                    {
                                        body += String.Format("\n\n{0}", res.ErrorMessage);
                                    }
                                }

                                #endregion
                            }
                            else
                            {
                                #region No

                                body = String.Format("{0} {1}\n", recipient.User.Name, negativeterm);

                                #endregion
                            }

                            messageDto.Text = StringUtility.ConvertNewLineToHtml(body);
                            messageDto.ShortText = body;
                            returnResult.Success = true;
                            returnResult.Value = messageDto;

                            recipient.State = (int)SoeEntityState.Inactive;
                            SaveChanges(entities);
                        }
                    }
                }
            }

            return returnResult;
        }

        private ActionResult AbortShiftRequestMessage(XEMailAnswerType answerType, List<TimeScheduleTemplateBlock> overlappingShifts, bool passedShift, bool missingRequest, int messageId, int userId)
        {
            ActionResult returnResult = new ActionResult(false);

            Message message = GetMessage(messageId, loadRecipients: true);
            if (message != null)
            {
                // Check if order or shift
                var block = TimeScheduleManager.GetTimeScheduleTemplateBlock(message.RecordId);
                if (block == null)
                    return returnResult;

                bool isOrder = block.CustomerInvoiceId.HasValue;

                string subjectterm = GetText(396, (int)TermGroup.XEMailGrid, "Fel vid behandling av");

                MessageEditDTO dto = new MessageEditDTO()
                {
                    ActorCompanyId = message.ActorCompanyId,
                    LicenseId = message.LicenseId,
                    Entity = (SoeEntityType)message.Entity,
                    RecordId = message.RecordId,
                    ParentId = message.MessageId,
                    RoleId = message.RoleId,
                    SenderUserId = message.UserId,
                    SenderName = "Autoreply SoftOne",
                    Subject = subjectterm + " " + message.Subject,
                    Created = DateTime.Now,
                    AnswerType = XEMailAnswerType.None,
                    MessagePriority = TermGroup_MessagePriority.High,
                    MessageType = TermGroup_MessageType.AutomaticInformation,
                    MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                    MessageTextType = TermGroup_MessageTextType.HTML,
                    Recievers = new List<MessageRecipientDTO>() { new MessageRecipientDTO() { UserId = userId } },
                };

                string body = string.Empty;
                if (answerType == XEMailAnswerType.Yes)
                {
                    body += String.Format(GetText(397, (int)TermGroup.XEMailGrid, "Kan inte tilldela dig det {0} du just tackat ja till."), isOrder ? GetText(485, (int)TermGroup.TimeSchedulePlanning, "uppdrag") : GetText(481, (int)TermGroup.TimeSchedulePlanning, "pass"));
                    body += "\n";
                    if (overlappingShifts != null && overlappingShifts.Any())
                    {
                        body += String.Format(GetText(398, (int)TermGroup.XEMailGrid, "Följande befintliga {0} krockar med det:"), isOrder ? GetText(485, (int)TermGroup.TimeSchedulePlanning, "uppdrag") : GetText(481, (int)TermGroup.TimeSchedulePlanning, "pass"));
                        foreach (var shift in overlappingShifts)
                        {
                            body += String.Format("\n{0}-{1} {2}", shift.StartTime.ToShortTimeString(), shift.StopTime.ToShortTimeString(), shift.ShiftType != null ? shift.ShiftType.Name : String.Empty);
                        }
                    }
                }
                if (passedShift)
                {
                    body += String.Format(GetText(3680, "{0} har redan varit"), StringUtility.CamelCaseWord((isOrder ? GetText(484, (int)TermGroup.TimeSchedulePlanning, "uppdraget") : GetText(480, (int)TermGroup.TimeSchedulePlanning, "passet")))) + ".";
                }
                else if (missingRequest)
                {
                    body += GetText(4069, "Förfrågan finns ej kvar.");
                }

                dto.Text = StringUtility.ConvertNewLineToHtml(body);

                //Create mail
                if (message.ActorCompanyId.HasValue)
                {
                    dto.ShortText = body;
                    returnResult.Success = true;
                    returnResult.Value = dto;
                }
            }

            return returnResult;
        }

        private MessageEditDTO HandleNeedsConfirmationMessage(int messageId, int userId)
        {
            MessageEditDTO messageDto = null;

            // Get message
            Message message = GetMessage(messageId, loadRecipients: true, loadMessageText: true);
            if (message != null)
            {
                MessageRecipient recipient = message.MessageRecipient.FirstOrDefault(r => r.UserId == userId);
                if (recipient != null)
                {
                    // Create message to needs confirmation sender
                    messageDto = new MessageEditDTO()
                    {
                        ActorCompanyId = message.ActorCompanyId,
                        LicenseId = message.LicenseId,
                        Entity = (SoeEntityType)message.Entity,
                        RecordId = message.RecordId,
                        ParentId = message.MessageId,
                        RoleId = UserManager.GetDefaultRoleId(message.ActorCompanyId ?? base.ActorCompanyId, recipient.User),
                        SenderUserId = recipient.UserId,
                        SenderName = recipient.User.Name, //   message.SenderName,
                        Subject = String.Format(GetText(416, (int)TermGroup.XEMailGrid, "Meddelande '{0}' har bekräftats"), message.Subject),
                        Text = message.MessageText.Text,
                        ShortText = message.MessageText.ShortText,
                        Created = DateTime.Now,
                        AnswerType = XEMailAnswerType.None,
                        MessagePriority = TermGroup_MessagePriority.Normal,
                        MessageType = TermGroup_MessageType.NeedsConfirmationAnswer,
                        MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                        MessageTextType = TermGroup_MessageTextType.HTML,
                        Recievers = new List<MessageRecipientDTO>() { new MessageRecipientDTO() { UserId = message.UserId } },
                    };
                }
            }

            return messageDto;
        }

        #endregion

        #region User
        public Dictionary<int, string> GetUserIdsByAccount(int actorCompanyId, int roleId, int userId)
        {
            List<Employee> validEmployees = GetEmployeesByAccount(actorCompanyId, roleId, userId).Where(x => x.UserId.HasValue).ToList();
            return validEmployees.ToDictionary(x => x.UserId.Value, x => x.EmployeeNrAndName);
        }

        #endregion

        #region Employees

        public Dictionary<int, string> GetEmployeeIdsByAccount(int actorCompanyId, int roleId, int userId)
        {
            List<Employee> validEmployees = GetEmployeesByAccount(actorCompanyId, roleId, userId);
            return validEmployees.ToDictionary(x => x.EmployeeId, x => x.EmployeeNrAndName);
        }

        public List<Employee> GetEmployeesByAccount(int actorCompanyId, int roleId, int userId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<Employee> validEmployees = new List<Employee>();

            DateTime dateFrom = DateTime.Today;
            DateTime dateTo = DateTime.Today;

            List<int> employeeIds = (from e in entitiesReadOnly.Employee
                                     where e.ActorCompanyId == actorCompanyId &&
                                     !e.Hidden &&
                                     !e.Vacant &&
                                     e.State == (int)SoeEntityState.Active
                                     select e.EmployeeId).ToList();

            Employee currentEmployee = EmployeeManager.GetEmployeeByUser(actorCompanyId, userId);
            employeeIds = EmployeeManager.GetValidEmployeeByAccountHierarchy(entitiesReadOnly, actorCompanyId, roleId, userId, employeeIds, currentEmployee, dateFrom, dateTo, getHidden: true, useShowOtherEmployeesPermission: true, onlyDefaultAccounts: false);

            List<Employee> employees = (from e in entitiesReadOnly.Employee
                                        .Include("ContactPerson")
                                        .Include("EmployeeAccount.Children")
                                        where e.ActorCompanyId == actorCompanyId &&
                                        employeeIds.Contains(e.EmployeeId)
                                        select e).ToList();

            foreach (var employee in employees)
            {
                bool isValid = false;

                // GetValidEmployeeByAccountHierarchy only checks accounts not dates
                // Check that employee account dates are within specified range
                if (employee.EmployeeAccount != null && employee.EmployeeAccount.Any())
                {
                    foreach (EmployeeAccount account in employee.EmployeeAccount)
                    {
                        if (EmployeeManager.IsEmployeeAccountValid(account, dateFrom, dateTo))
                        {
                            isValid = true;
                            break;
                        }
                    }
                }
                else
                {
                    isValid = true;
                }

                if (isValid)
                    validEmployees.Add(employee);
            }

            return validEmployees;
        }

        #endregion

        #region Message

        private Message GetMessage(int messageId, bool loadRecipients = false, bool loadMessageText = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Message.NoTracking();
            return GetMessage(entities, messageId, loadRecipients, loadMessageText);
        }

        public Message GetMessage(CompEntities entities, int messageId, bool loadRecipients = false, bool loadMessageText = false)
        {
            var query = (from m in entities.Message
                         where m.MessageId == messageId
                         select m);

            if (loadRecipients)
                query = query.Include("MessageRecipient.User");
            if (loadMessageText)
                query = query.Include("MessageText");

            if (loadRecipients)
                query = query.Include("MessageRecipient.User");
            if (loadMessageText)
                query = query.Include("MessageText");

            return query.FirstOrDefault();
        }

        public int GetIncomingMessagesCount(int? licenseId, int userId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var query = (from mr in entitiesReadOnly.MessageRecipient
                         where mr.UserId == userId &&
                         mr.DeliveryType == (int)TermGroup_MessageDeliveryType.XEmail &&
                         mr.ReadDate == null &&
                         (!mr.Message.HandledByJob.HasValue || !mr.Message.HandledByJob.Value) &&
                         mr.State == (int)SoeEntityState.Active
                         select mr);

            if (licenseId.HasValue)
                query = query.Where(mr => mr.User.LicenseId == licenseId);

            return query.Count();
        }

        public string GetCounterMessage(string message, int counter, string startDel = "(", string stopDel = ")")
        {
            if (message == null)
                message = "";

            //Remove old counter
            int startIdx = message.LastIndexOf(startDel);
            if (startIdx > 0)
                message = message.Substring(0, startIdx);

            //Dont add zero-counter
            if (counter > 0)
            {
                if (!message.EndsWith(" "))
                    message += " ";
                message += startDel;
                message += counter;
                message += stopDel;
            }

            return message;
        }

        #endregion

        #region MessageDTO



        public MessageEditDTO GetSentMessage(int messageId, int licenseId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.MessageRecipient.NoTracking();
            return GetSentMessage(entities, messageId, licenseId, userId);
        }


        private MessageEditDTO GetSentMessage(CompEntities entities, int messageId, int licenseId, int userId)
        {
            bool isMessageSentByUser = (from m in entities.Message
                                        where m.MessageId == messageId &&
                                        m.UserId == userId &&
                                        m.SentDate.HasValue
                                        orderby m.Created descending
                                        select m).Any();

            if (isMessageSentByUser)
                return GetXEMail(messageId, licenseId, userId);
            else
                return null;

        }


        public MessageEditDTO GetIncomingMessage(int messageId, int licenseId, int userId, TermGroup_MessageDeliveryType deliveryType = TermGroup_MessageDeliveryType.XEmail)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.MessageRecipient.NoTracking();
            return GetIncomingMessage(entities, messageId, licenseId, userId, deliveryType);
        }


        private MessageEditDTO GetIncomingMessage(CompEntities entities, int messageId, int licenseId, int userId, TermGroup_MessageDeliveryType deliveryType)
        {
            bool isMessageIncomingForUser = (from mr in entities.MessageRecipient
                                             where mr.MessageId == messageId &&
                                             mr.UserId == userId &&
                                             mr.DeliveryType == (int)deliveryType &&
                                             mr.User.LicenseId == licenseId &&
                                             mr.State == (int)SoeEntityState.Active
                                             orderby mr.Created descending
                                             select mr).Any();

            if (isMessageIncomingForUser)
                return GetXEMail(messageId, licenseId, userId);
            else
                return null;

        }

        private List<MessageGridDTO> GetIncomingMessages(int licenseId, int userId, TermGroup_MessageDeliveryType deliveryType, int? messageId = null, bool includeMessages = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Message.NoTracking();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.MessageRecipient.NoTracking();
            return GetIncomingMessages(entities, licenseId, userId, deliveryType, messageId, includeMessages);
        }

        private List<MessageGridDTO> GetIncomingMessages(CompEntities entities, int licenseId, int userId, TermGroup_MessageDeliveryType deliveryType, int? messageId = null, bool includeMessages = false)
        {
            /*
             * MessageRecipients with map specific userid are considered to be messages sent to (incoming for) that specifik user, can be read or unread (ReadDate is null) . 
             * Only those that have state set to active are incoming
             * */

            List<MessageGridDTO> incomingMail = new List<MessageGridDTO>();

            IQueryable<MessageRecipient> query = (from mr in entities.MessageRecipient.Include("Message")
                                                  where mr.UserId == userId &&
                                                  mr.DeliveryType == (int)deliveryType &&
                                                  mr.User.LicenseId == licenseId &&
                                                  mr.State == (int)SoeEntityState.Active &&
                                                  (!mr.Message.HandledByJob.HasValue || !mr.Message.HandledByJob.Value) &&
                                                  (!messageId.HasValue || (messageId.HasValue && mr.MessageId == messageId.Value))
                                                  select mr);

            if (includeMessages)
                query = query.Include("Message.MessageText");

            List<MessageRecipient> recipients = query.ToList();

            List<DataStorageRecord> allAttachments = GetMessageAttachments(entities, recipients.Select(r => r.MessageId).ToList());
            foreach (MessageRecipient recipient in recipients)
            {
                MessageGridDTO dto = CreateMessageGridDTO(recipient.Message, XEMailType.Incoming, recipient);

                if (includeMessages && !recipient.Message.MessageText.Text.IsNullOrEmpty())
                    dto.FirstTextRow = recipient.Message.MessageText.Text.Replace("<br/>", "<br>").Split(new string[] { "<br>" }, StringSplitOptions.None)[0];

                // Check for attachments
                dto.HasAttachment = allAttachments.Any(d => d.RecordId == recipient.MessageId);

                incomingMail.Add(dto);
            }

            return incomingMail.OrderByDescending(m => m.Created).ToList();
        }

        private List<MessageGridDTO> GetOutgoingMessages(int userId, int? messageId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Message.NoTracking();
            return GetOutgoingMessages(entities, userId, messageId);
        }

        private List<MessageGridDTO> GetOutgoingMessages(CompEntities entities, int userId, int? messageId = null)
        {
            /*
             * Messages with map specific userid and SentDate is null are considered to be outgoing mail for that specifik user.
             * They are saved but not yet sent.
             * */

            List<MessageGridDTO> outgoingMail = new List<MessageGridDTO>();

            List<Message> messages = (from m in entities.Message
                                      where m.UserId == userId &&
                                      !m.SentDate.HasValue &&
                                      m.State == (int)SoeEntityState.Active &&
                                      (!messageId.HasValue || (messageId.HasValue && m.MessageId == messageId.Value))
                                      orderby m.Created descending
                                      select m).ToList();

            List<DataStorageRecord> allAttachments = GetMessageAttachments(entities, messages.Select(m => m.MessageId).ToList());
            foreach (Message message in messages)
            {
                MessageGridDTO dto = CreateMessageGridDTO(message, XEMailType.Outgoing);

                // Check for attachments
                dto.HasAttachment = allAttachments.Any(d => d.RecordId == message.MessageId);

                outgoingMail.Add(dto);
            }

            return outgoingMail;
        }

        private List<MessageGridDTO> GetSentMessages(int userId, int? messageId = null, bool includeMessages = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Message.NoTracking();
            return GetSentMessages(entities, userId, messageId, includeMessages);
        }

        private List<MessageGridDTO> GetSentMessages(CompEntities entities, int userId, int? messageId = null, bool includeMessages = false)
        {
            /*
            * Messages with map specifik userid and SentDate is NOT null are considered mail sent from that specifik user.            
            * */

            List<MessageGridDTO> sentMail = new List<MessageGridDTO>();

            IQueryable<Message> query = (from m in entities.Message.Include("MessageRecipient.User")
                                         where m.UserId == userId &&
                                         m.SentDate.HasValue &&
                                         (!messageId.HasValue || (messageId.HasValue && m.MessageId == messageId.Value)) &&
                                         m.State == (int)SoeEntityState.Active
                                         orderby m.Created descending
                                         select m);

            if (includeMessages)
                query = query.Include("MessageText");

            List<Message> messages = query.ToList();

            List<DataStorageRecord> allAttachments = GetMessageAttachments(entities, messages.Select(m => m.MessageId).ToList());
            foreach (Message message in messages)
            {
                MessageGridDTO dto = CreateMessageGridDTO(message, XEMailType.Sent);

                if (includeMessages && !message.MessageText.Text.IsNullOrEmpty())
                    dto.FirstTextRow = message.MessageText.Text.Replace("<br/>", "<br>").Split(new string[] { "<br>" }, StringSplitOptions.None)[0];


                // Check for attachments
                dto.HasAttachment = allAttachments.Any(d => d.RecordId == message.MessageId);

                sentMail.Add(dto);
            }

            return sentMail;
        }

        private List<MessageGridDTO> GetDeletedMessages(int licenseId, int userId, TermGroup_MessageDeliveryType deliveryType, int? messageId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Message.NoTracking();
            return GetDeletedMessages(entities, licenseId, userId, deliveryType, messageId);
        }

        private List<MessageGridDTO> GetDeletedMessages(CompEntities entities, int licenseId, int userId, TermGroup_MessageDeliveryType deliveryType, int? messageId = null)
        {
            List<MessageGridDTO> dtos = new List<MessageGridDTO>();

            // Incoming
            List<MessageRecipient> recipients = (from mr in entities.MessageRecipient.Include("Message")
                                                 where mr.UserId == userId &&
                                                 mr.DeliveryType == (int)deliveryType &&
                                                 mr.User.LicenseId == licenseId &&
                                                 mr.State == (int)SoeEntityState.Deleted &&
                                                 (!messageId.HasValue || (messageId.HasValue && mr.MessageId == messageId.Value))
                                                 select mr).ToList();

            List<DataStorageRecord> allIncomingAttachments = GetMessageAttachments(entities, recipients.Select(r => r.MessageId).ToList());
            foreach (MessageRecipient recipient in recipients)
            {
                MessageGridDTO dto = CreateMessageGridDTO(recipient.Message, XEMailType.Deleted, recipient, recipient.Modified);

                // Use received instead of message created
                dto.Created = recipient.Created;

                // Check for attachments
                dto.HasAttachment = allIncomingAttachments.Any(d => d.RecordId == recipient.MessageId);

                dtos.Add(dto);
            }

            // Sent
            List<Message> messages = (from m in entities.Message
                                      where m.UserId == userId &&
                                      m.State != (int)SoeEntityState.Active
                                      select m).ToList();

            List<DataStorageRecord> allOutgoingAttachments = GetMessageAttachments(entities, messages.Select(m => m.MessageId).ToList());
            foreach (Message message in messages)
            {
                MessageGridDTO dto = CreateMessageGridDTO(message, XEMailType.Deleted, null, message.Modified);

                // Check for attachments
                dto.HasAttachment = allOutgoingAttachments.Any(d => d.RecordId == message.MessageId);

                dtos.Add(dto);
            }

            return dtos.OrderByDescending(m => m.Created).ToList();
        }

        private List<DataStorageRecord> GetMessageAttachments(CompEntities entities, List<int> messageIds)
        {
            return (from d in entities.DataStorageRecord
                    where d.Entity == (int)SoeEntityType.XEMail &&
                    messageIds.Contains(d.RecordId) &&
                    d.DataStorage != null &&
                    d.DataStorage.State == (int)SoeEntityState.Active
                    select d).ToList();
        }

        #endregion

        #region MessageGroup

        public List<MessageGroup> GetMessageGroupsForGrid(int actorCompanyId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Message.NoTracking();
            List<MessageGroup> groups = (from mg in entities.MessageGroup
                                         where (mg.UserId == userId || mg.IsPublic) &&
                                         mg.ActorCompanyId == actorCompanyId &&
                                         mg.State == (int)SoeEntityState.Active
                                         select mg).OrderBy(mg => mg.Name).ToList();

            return groups;
        }

        public List<MessageGroup> GetMessageGroups(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Message.NoTracking();
            return GetMessageGroups(entities, actorCompanyId);
        }

        public List<MessageGroup> GetMessageGroups(CompEntities entities, int actorCompanyId)
        {
            List<MessageGroup> groups = (from mg in entities.MessageGroup.Include("MessageGroupMapping")
                                         where
                                         mg.ActorCompanyId == actorCompanyId &&
                                         mg.State == (int)SoeEntityState.Active
                                         select mg).OrderBy(mg => mg.Name).ToList();

            return groups;
        }

        public List<MessageGroupDTO> GetMessageGroups(int actorCompanyId, int userId, bool setNames = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Message.NoTracking();
            return GetMessageGroups(entities, actorCompanyId, userId, setNames);
        }

        public List<MessageGroupDTO> GetMessageGroups(CompEntities entities, int actorCompanyId, int userId, bool setNames = true)
        {
            List<MessageGroupDTO> list = new List<MessageGroupDTO>();

            List<MessageGroup> groups = (from mg in entities.MessageGroup.Include("MessageGroupMapping")
                                         where (mg.UserId == userId || mg.IsPublic) &&
                                         mg.ActorCompanyId == actorCompanyId &&
                                         mg.State == (int)SoeEntityState.Active
                                         select mg).OrderBy(mg => mg.Name).ToList();

            foreach (MessageGroup group in groups)
            {
                MessageGroupDTO dto = CreateMessageGroupDTO(entities, group, setNames);
                if (dto != null)
                    list.Add(dto);
            }

            return list;
        }

        public List<SmallGenericType> GetMessageGroupsDict(int actorCompanyId, int userId, bool addEmptyRow = false)
        {
            List<SmallGenericType> list = new List<SmallGenericType>();

            if (addEmptyRow)
                list.Add(new SmallGenericType(0, ""));

            List<MessageGroup> groups = GetMessageGroupsForGrid(actorCompanyId, userId);
            foreach (MessageGroup group in groups)
            {
                list.Add(new SmallGenericType(group.MessageGroupId, group.Name));
            }

            return list;
        }

        public ActionResult SaveMessageGroup(MessageGroupDTO messageGroupInput)
        {
            #region Prereq

            if (messageGroupInput == null)
                return new ActionResult(false, (int)ActionResultSave.Communication_ObjectIsNull, GetText(7044, "Felaktig inparameter"));

            #endregion

            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Perform

                        #region MessageGroup

                        MessageGroup group = null;
                        if (messageGroupInput.MessageGroupId != 0)
                            group = GetMessageGroup(entities, messageGroupInput.MessageGroupId, true);

                        if (group == null)
                        {
                            // Add
                            group = new MessageGroup()
                            {
                                LicenseId = messageGroupInput.LicenseId,
                                ActorCompanyId = messageGroupInput.ActorCompanyId,
                                UserId = messageGroupInput.UserId,
                            };
                            SetCreatedProperties(group);
                            entities.MessageGroup.AddObject(group);
                        }
                        else
                        {
                            // Update
                            SetModifiedProperties(group);
                        }

                        // Common
                        group.Name = messageGroupInput.Name;
                        group.Description = messageGroupInput.Description;
                        group.IsPublic = messageGroupInput.IsPublic;
                        group.NoUserValidation = messageGroupInput.NoUserValidation;

                        #endregion

                        #region MessageGroupMapping

                        if (!group.MessageGroupMapping.IsNullOrEmpty())
                        {
                            // Check existing
                            foreach (MessageGroupMapping mapping in group.MessageGroupMapping.Where(m => m.State == (int)SoeEntityState.Active).ToList())
                            {
                                MessageGroupMemberDTO mappingInput = messageGroupInput.GroupMembers.FirstOrDefault(m => (int)m.Entity == mapping.Entity && m.RecordId == mapping.RecordId);
                                if (mappingInput == null)
                                    ChangeEntityState(mapping, SoeEntityState.Deleted);
                                else
                                    messageGroupInput.GroupMembers.Remove(mappingInput);
                            }
                        }

                        // Add
                        if (!messageGroupInput.GroupMembers.IsNullOrEmpty() && group.MessageGroupMapping == null)
                            group.MessageGroupMapping = new EntityCollection<MessageGroupMapping>();

                        foreach (MessageGroupMemberDTO mappingInput in messageGroupInput.GroupMembers)
                        {
                            MessageGroupMapping mapping = new MessageGroupMapping()
                            {
                                Entity = (int)mappingInput.Entity,
                                RecordId = mappingInput.RecordId,
                                State = (int)SoeEntityState.Active
                            };
                            SetCreatedProperties(mapping);
                            group.MessageGroupMapping.Add(mapping);
                        }

                        #endregion

                        result = SaveChanges(entities);
                        if (result.Success)
                            result.IntegerValue = group.MessageGroupId;

                        #endregion

                        // Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult DeleteMessageGroup(int messageGroupId)
        {
            #region Prereq

            if (messageGroupId == 0)
                return new ActionResult(false, (int)ActionResultDelete.EntityIsNull, GetText(7044, "Felaktig inparameter"));

            if (MessageGroupInUse(messageGroupId))
                return new ActionResult(false, (int)ActionResultDelete.EntityInUse, GetText(12129, "Mottagargruppen används och kan därför inte tas bort"));

            #endregion

            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Perform

                        MessageGroup group = GetMessageGroup(entities, messageGroupId, true);
                        if (group != null)
                        {
                            ChangeEntityState(group, SoeEntityState.Deleted);

                            foreach (MessageGroupMapping mapping in group.MessageGroupMapping.Where(m => m.State != (int)SoeEntityState.Deleted).ToList())
                            {
                                ChangeEntityState(mapping, SoeEntityState.Deleted);
                            }

                            result = SaveChanges(entities, transaction);
                        }
                        else
                        {
                            result.Success = false;
                        }

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        private bool MessageGroupInUse(int messageGroupId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (entitiesReadOnly.InformationMessageGroup.Any(m => m.MessageGroupId == messageGroupId && m.State == (int)SoeEntityState.Active))
                return true;

            if (entitiesReadOnly.TimeScheduleEventMessageGroup.Any(m => m.MessageGroupId == messageGroupId))
                return true;

            if (entitiesReadOnly.ReportUserSelectionAccess.Any(m => m.MessageGroupId == messageGroupId && m.State == (int)SoeEntityState.Active))
                return true;
            return false;
        }

        public MessageGroup GetMessageGroup(int messageGroupId, bool includeMappings = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetMessageGroup(entities, messageGroupId, includeMappings);
        }

        public MessageGroup GetMessageGroup(CompEntities entities, int messageGroupId, bool includeMappings = false)
        {
            IQueryable<MessageGroup> query = (from msg in entities.MessageGroup
                                              where msg.MessageGroupId == messageGroupId &&
                                              msg.State == (int)SoeEntityState.Active
                                              select msg);

            if (includeMappings)
                query = query.Include("MessageGroupMapping");

            return query.FirstOrDefault();
        }

        private List<MessageGroupMapping> GetMessageGroupMappings(CompEntities entities, int messageGroupId)
        {
            return (from msg in entities.MessageGroupMapping
                    where msg.MessageGroupId == messageGroupId &&
                    msg.State == (int)SoeEntityState.Active
                    select msg).ToList();
        }

        private MessageGroupDTO CreateMessageGroupDTO(CompEntities entities, MessageGroup messageGroup, bool setNames)
        {
            MessageGroupDTO dto = messageGroup.ToDTO(true);

            if (setNames)
            {
                try
                {
                    foreach (MessageGroupMemberDTO mapping in dto.GroupMembers)
                    {
                        string name = String.Empty;
                        string usrname = String.Empty;

                        switch (mapping.Entity)
                        {
                            case SoeEntityType.User:
                                User user = UserManager.GetUser(entities, mapping.RecordId);
                                if (user != null)
                                {
                                    name = user.Name;
                                    usrname = user.LoginName;
                                }
                                break;
                            case SoeEntityType.Category:
                                Category category = CategoryManager.GetCategory(entities, mapping.RecordId, dto.ActorCompanyId);
                                if (category != null)
                                {
                                    name = category.Name;
                                    usrname = category.Name;
                                }
                                break;
                            case SoeEntityType.Role:
                                Role role = RoleManager.GetRole(entities, mapping.RecordId);
                                if (role != null)
                                {
                                    name = role.Name;
                                    usrname = role.Name;
                                }
                                break;
                            case SoeEntityType.EmployeeGroup:
                                EmployeeGroup group = EmployeeManager.GetEmployeeGroup(entities, mapping.RecordId);
                                if (group != null)
                                {
                                    name = group.Name;
                                    usrname = group.Name;
                                }
                                break;
                            case SoeEntityType.MessageGroup:
                                MessageGroup mgroup = CommunicationManager.GetMessageGroup(entities, mapping.RecordId);
                                if (mgroup != null)
                                {
                                    name = mgroup.Name;
                                    usrname = mgroup.Name;
                                }
                                break;
                        }

                        mapping.Name = name;
                        mapping.Username = usrname;
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                }
            }

            return dto;
        }

        public List<MessageGroup> GetValidMessageGroupsForRecipient(int actorCompanyId, int userId, int roleId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            string key = $"MessageGroups_{actorCompanyId}";
            var messageGroups = BusinessMemoryCache<List<MessageGroup>>.Get(key) ?? GetMessageGroups(actorCompanyId);
            BusinessMemoryCache<List<MessageGroup>>.Set(key, messageGroups);

            return GetValidMessageGroupsForRecipient(actorCompanyId, userId, messageGroups, dateFrom ?? DateTime.Today, dateTo ?? CalendarUtility.GetEndOfDay(DateTime.Today), roleId);
        }

        public List<MessageGroup> GetValidMessageGroupsForRecipient(int actorCompanyId, int userId, List<MessageGroup> messageGroups, DateTime dateFrom, DateTime dateTo, int? roleId = null)
        {
            var anyUserEntity = messageGroups.Any(mg => mg.MessageGroupMapping.Any(m => m.Entity == (int)SoeEntityType.User));
            User user = anyUserEntity ? UserManager.GetUser(userId, loadUserCompanyRole: true) : null;

            var anyEmployeeEntity = messageGroups.Any(mg => mg.MessageGroupMapping.Any(m => m.Entity == (int)SoeEntityType.EmployeeGroup || m.Entity == (int)SoeEntityType.Category || m.Entity == (int)SoeEntityType.Account));
            Employee employee = anyEmployeeEntity ? EmployeeManager.GetEmployeeByUser(actorCompanyId, userId, loadEmployment: true, loadEmployeeAccount: true) : null;

            var anyCategoryEntity = messageGroups.Any(mg => mg.MessageGroupMapping.Any(m => m.Entity == (int)SoeEntityType.Category));
            List<CompanyCategoryRecord> categoryRecords = anyCategoryEntity && employee != null ? CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId, actorCompanyId, false, dateFrom, dateTo) : null;

            return (from MessageGroup mg in messageGroups
                    where IsUserInMessageGroup(mg, actorCompanyId, userId, user, roleId, employee, dateFrom, dateTo, categoryRecords)
                    select mg).ToList();
        }

        public bool IsUserInMessageGroup(MessageGroup group, int actorCompanyId, int? userId, User user = null, int? roleId = null, Employee employee = null, DateTime? dateFrom = null, DateTime? dateTo = null, List<CompanyCategoryRecord> categoryRecords = null, List<EmployeeAccount> employeeAccounts = null)
        {
            if (group == null)
                return false;

            bool userIsInGroup = false;
            if (IsUserInGroupDirectly(group, userId, roleId))
            {
                if (userId.HasValue)
                    user = UserManager.GetUser(userId.Value, loadUserCompanyRole: true);
                if (user != null)
                    userIsInGroup = true;
            }

            if (!userIsInGroup)
                userIsInGroup = IsUserInGroupMappings(group, actorCompanyId, userId, user, roleId, employee, dateFrom, dateTo, categoryRecords, employeeAccounts);

            return userIsInGroup;
        }

        private bool IsUserInGroupDirectly(MessageGroup group, int? userId, int? roleId)
        {
            if (userId.HasValue && group.MessageGroupMapping.Any(map => map.Entity == (int)SoeEntityType.User && map.RecordId == userId && map.State == (int)SoeEntityState.Active))
                return true;
            else if (roleId.HasValue && group.MessageGroupMapping.Any(map => map.Entity == (int)SoeEntityType.Role && map.RecordId == roleId.Value && map.State == (int)SoeEntityState.Active))
                return true;
            return false;
        }

        private bool IsUserInGroupMappings(MessageGroup group, int actorCompanyId, int? userId, User user, int? roleId, Employee employee, DateTime? dateFrom, DateTime? dateTo, List<CompanyCategoryRecord> categoryRecords = null, List<EmployeeAccount> employeeAccounts = null)
        {
            bool userIsInGroup = false;

            DateTime date = DateTime.Today;

            List<int> accountIdsOnEmployeeAccount = null;
            int? employeeGroupId = null;

            User GetUser()
            {
                if (user == null && userId.HasValue)
                    user = UserManager.GetUser(userId.Value, loadUserCompanyRole: true);
                return user;
            }
            Employee GetEmployeeByUser()
            {
                if (employee == null && userId.HasValue)
                    employee = EmployeeManager.GetEmployeeByUser(actorCompanyId, userId.Value);
                return employee;
            }

            foreach (MessageGroupMapping map in group.MessageGroupMapping.Where(m => m.State == (int)SoeEntityState.Active))
            {
                switch ((SoeEntityType)map.Entity)
                {
                    case SoeEntityType.Role:
                        #region Role

                        if (!roleId.HasValue)
                        {
                            user = GetUser();
                            if (user != null && user.UserCompanyRole.Any(UserCompanyRole => UserCompanyRole.RoleId == map.RecordId))
                                userIsInGroup = true;
                        }

                        #endregion
                        break;
                    case SoeEntityType.EmployeeGroup:
                        #region EmployeeGroup

                        employee = GetEmployeeByUser();
                        if (employee != null)
                        {
                            if (!employeeGroupId.HasValue)
                                employeeGroupId = employee.GetEmployeeGroupId(dateFrom ?? date, dateTo ?? date);
                            if (employeeGroupId == map.RecordId)
                                userIsInGroup = true;
                        }

                        break;
                    #endregion
                    case SoeEntityType.Category:
                        #region Category

                        employee = GetEmployeeByUser();
                        if (employee != null)
                        {
                            if (categoryRecords == null)
                                categoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId, actorCompanyId, dateFrom: dateFrom, dateTo: dateTo);
                            if (categoryRecords.Any(c => c.CategoryId == map.RecordId))
                                userIsInGroup = true;
                        }

                        #endregion
                        break;
                    case SoeEntityType.Account:
                        #region Account

                        employee = GetEmployeeByUser();
                        if (employee != null)
                        {
                            if (accountIdsOnEmployeeAccount == null)
                            {
                                if (employeeAccounts == null)
                                    employeeAccounts = employee.EmployeeAccount.ToList();

                                employeeAccounts = employeeAccounts?.Where(ea => ea.EmployeeId == employee.EmployeeId && ea.AccountId.HasValue && ea.DateFrom <= date && (!ea.DateTo.HasValue || ea.DateTo.Value >= date) && ea.State == (int)SoeEntityState.Active).ToList();
                                if (employeeAccounts.IsNullOrEmpty())
                                    break;

                                accountIdsOnEmployeeAccount = employeeAccounts.Select(ea => ea.AccountId.Value).Distinct().ToList();

                                List<int> parentIds = new List<int>();
                                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                                accountIdsOnEmployeeAccount.ForEach(f => parentIds.AddRange(AccountManager.GetAccountInternalAndParents(entitiesReadOnly, f, actorCompanyId).Select(s => s.AccountId)));
                                accountIdsOnEmployeeAccount.AddRange(parentIds);
                                accountIdsOnEmployeeAccount = accountIdsOnEmployeeAccount.Distinct().ToList();
                            }

                            if (accountIdsOnEmployeeAccount.Any(accountId => accountId == map.RecordId))
                                userIsInGroup = true;
                        }

                        #endregion
                        break;
                    case SoeEntityType.MessageGroup:
                        #region MessageGroup (recusive)

                        return group.MessageGroupId != map.RecordId && IsUserInMessageGroup(GetMessageGroup(map.RecordId, true), actorCompanyId, userId, user, roleId, employee, dateFrom, dateTo, categoryRecords, employeeAccounts);

                        #endregion
                }

                if (userIsInGroup)
                    break;
            }

            return userIsInGroup;
        }

        #endregion

        #region MessageRecipient

        private List<MessageRecipient> GetMessageRecipients(CompEntities entities, int messageId)
        {
            return (from msg in entities.MessageRecipient
                    where msg.MessageId == messageId &&
                    msg.State == (int)SoeEntityState.Active
                    select msg).ToList();
        }

        private List<int> GetMessageRecipientUserIds(CompEntities entities, int messageId)
        {
            return (from msg in entities.MessageRecipient
                    where msg.MessageId == messageId &&
                    msg.State == (int)SoeEntityState.Active
                    select msg.UserId).ToList();
        }

        public List<MessageRecipient> GetMessageRecipients(CompEntities entities, int entity, int recordId)
        {
            List<MessageRecipient> recipients = new List<MessageRecipient>();

            var mess = (from msg in entities.Message
                        where msg.Entity == entity &&
                        msg.RecordId == recordId
                        select msg).FirstOrDefault();

            if (mess != null)
                recipients = GetMessageRecipients(entities, mess.MessageId);

            return recipients;
        }

        public MessageRecipient GetMessageRecipient(int messageId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.MessageRecipient.NoTracking();
            return GetMessageRecipient(entities, messageId, userId);
        }

        private MessageRecipient GetMessageRecipient(CompEntities entities, int messageId, int userId)
        {
            return (from mr in entities.MessageRecipient
                    where mr.UserId == userId && mr.MessageId == messageId &&
                    mr.State == (int)SoeEntityState.Active
                    select mr).FirstOrDefault();
        }

        private bool MessageRecipientExist(CompEntities entities, int messageId, int userId)
        {
            return (from mr in entities.MessageRecipient
                    where mr.MessageId == messageId &&
                    mr.UserId == userId &&
                    mr.State == (int)SoeEntityState.Active
                    select mr).Any();
        }

        #endregion

        #region MessageText

        private MessageText GetMessageText(CompEntities entities, int messageTextId)
        {
            return (from msgTxt in entities.MessageText
                    where msgTxt.MessageTextId == messageTextId
                    select msgTxt).FirstOrDefault();
        }

        #endregion

        #region NeedConfirmation

        public ActionResult SendTimeEmploymentContractShortSubstituteForConfirmation(int actorCompanyId, List<int> employeeIds, int userId, int roleId, List<DateTime> dates, bool printedFromScheduleplanning, bool savePrintout)
        {
            ActionResult result = null;
            List<Tuple<int, string>> failedEmployees = new List<Tuple<int, string>>();

            try
            {
                #region Prereq

                User user = UserManager.GetUser(userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8349, "Användare kunde inte hittas"));

                Report report = ReportManager.GetSettingReport(SettingMainType.Company, CompanySettingType.DefaultEmploymentContractShortSubstituteReport, SoeReportTemplateType.TimeEmploymentContract, actorCompanyId, userId, roleId);
                if (report == null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8802, "Lyckades inte hämta rapport enligt företagsinställning."));

                List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(actorCompanyId, employeeIds);

                #endregion

                #region Perform

                foreach (var employee in employees)
                {
                    ReportPrintoutDTO reportPrintout = ReportDataManager.PrintEmploymentContractShortSubstitute(report.ReportId, new List<int>() { employee.EmployeeId }, dates);
                    if (reportPrintout == null)
                    {
                        failedEmployees.Add(new Tuple<int, string>(employee.EmployeeId, string.Format(GetText(8801, "Kunde inte skapa pdf för {0}"), employee.Name)));
                        continue;
                    }

                    if (reportPrintout.Data != null)
                    {
                        string subject = GetText(8804, "Anställningsbevis");

                        string text = GetText(8805, "Läs igenom bifogat anställningsbevis. Godkänner du detta så tryck på knappen Läst och förstått.") + "{0}";
                        text += GetText(8806, "Godkänner du inte bifogat anställningsbevis så tryck på knappen svara och skicka dina kommentarer åter till avsändaren") + "{0}";

                        MessageEditDTO messageDto = new MessageEditDTO()
                        {
                            ParentId = 0,
                            AnswerType = XEMailAnswerType.None,
                            LicenseId = user.LicenseId,
                            ActorCompanyId = actorCompanyId,
                            RoleId = roleId,
                            SenderUserId = userId,
                            SenderName = user.Name,
                            SenderEmail = String.Empty,
                            Subject = subject,
                            Text = String.Format(text, "<br/>"),
                            ShortText = String.Format(text, "\n"),
                            Entity = SoeEntityType.TimeScheduleEmployeePeriod,
                            MessagePriority = TermGroup_MessagePriority.Normal,
                            MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                            MessageTextType = TermGroup_MessageTextType.Text,
                            MessageType = TermGroup_MessageType.NeedsConfirmation,
                            RecordId = 0,
                            Recievers = new List<MessageRecipientDTO>() { new MessageRecipientDTO() { Type = XEMailRecipientType.Employee, UserId = employee.EmployeeId } },
                            Attachments = new List<MessageAttachmentDTO>() { new MessageAttachmentDTO() { Name = reportPrintout.ReportName + Constants.SOE_SERVER_FILE_PDF_SUFFIX, Data = reportPrintout.Data, Filesize = reportPrintout.Data.Length } },
                            MarkAsOutgoing = false,
                        };

                        int? dataStorageId = null;

                        if (savePrintout)
                        {
                            using (CompEntities entities = new CompEntities())
                            {
                                DataStorage dataStorage = EmployeeManager.SaveEmploymentContractOnPrint(entities, report.ReportId, employee.EmployeeId, reportPrintout.Data, reportPrintout.XML, reportPrintout.ReportName);
                                dataStorageId = dataStorage != null && dataStorage.DataStorageId != 0 ? dataStorage.DataStorageId : (int?)null;
                            }
                        }

                        result = SendXEMail(messageDto, actorCompanyId, roleId, userId, dataStorageId: dataStorageId);

                        if (!result.Success)
                        {
                            failedEmployees.Add(new Tuple<int, string>(employee.EmployeeId, string.Format(GetText(8803, "Kunde inte skicka meddelande till {0}"), employee.Name)));
                            continue;
                        }
                    }
                    else
                    {
                        failedEmployees.Add(new Tuple<int, string>(employee.EmployeeId, string.Format(GetText(8801, "Kunde inte skapa pdf för {0}"), employee.Name)));
                        continue;
                    }
                }

                if (failedEmployees.Any())
                {
                    string errorMessage = string.Empty;
                    foreach (var employee in failedEmployees)
                    {
                        errorMessage += employee.Item2 + Environment.NewLine;
                    }

                    result = new ActionResult((int)ActionResultSave.PartylySaved, errorMessage);
                }
                else
                {
                    result = new ActionResult(true);
                }

                #endregion
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                base.LogError(ex, this.log);
            }


            return result;
        }

        #endregion

        #region Attachments

        public ActionResult SaveMessageAttachements(CompEntities entities, List<MessageAttachmentDTO> attachments, int actorCompanyId, int messageId, int? dataStorageId = null)
        {
            List<DataStorageRecord> records = (from dsr in entities.DataStorageRecord.Include("DataStorage")
                                               where dsr.Entity == (int)SoeEntityType.XEMail &&
                                               dsr.RecordId == messageId
                                               select dsr).ToList();

            ActionResult result = new ActionResult(true);

            foreach (MessageAttachmentDTO attachment in attachments)
            {
                if (!records.Any(a => a.RecordId == messageId && a.DataStorage.Description == attachment.Name))
                {
                    #region DataStorage

                    DataStorageRecord dataStorageRecord = null;
                    if (attachment.MessageAttachmentId != 0)
                    {
                        if (attachment.IsUploadedAsImage)
                        {
                            // TODO: Eliminate the Images table!!!
                            Images image = GraphicsManager.GetImage(entities, attachment.MessageAttachmentId);
                            if (image?.Image != null)
                                attachment.Data = image.Image;
                            DataStorage dataStorage = GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.XEMailFileAttachment, null, attachment.Data, null, null, actorCompanyId, attachment.Name);
                            dataStorageRecord = GeneralManager.CreateDataStorageRecord(entities, SoeDataStorageRecordType.XEMailFileAttachment, messageId, "", SoeEntityType.XEMail, dataStorage);
                        }
                        else
                            dataStorageRecord = GeneralManager.GetDataStorageRecord(entities, actorCompanyId, attachment.MessageAttachmentId);
                    }

                    if (dataStorageRecord != null)
                    {
                        dataStorageRecord.RecordId = messageId;
                        dataStorageRecord.Type = (int)SoeDataStorageRecordType.XEMailFileAttachment;
                        dataStorageRecord.Entity = (int)SoeEntityType.XEMail;
                    }
                    else
                    {
                        DataStorage dataStorage = null;

                        if (!dataStorageId.HasValue && attachment.DataStorageId.HasValue)
                            dataStorageId = attachment.DataStorageId.Value;

                        if (dataStorageId.HasValue)
                            dataStorage = GeneralManager.GetDataStorage(entities, dataStorageId.Value, actorCompanyId);
                        else
                            dataStorage = GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.XEMailFileAttachment, null, attachment.Data, null, null, actorCompanyId, attachment.Name);

                        GeneralManager.CreateDataStorageRecord(entities, SoeDataStorageRecordType.XEMailFileAttachment, messageId, "", SoeEntityType.XEMail, dataStorage);
                    }

                    #endregion
                }
            }

            foreach (DataStorageRecord record in records)
            {
                if (!attachments.Any(a => a.Name == record.DataStorage.Description))
                    ChangeEntityState(record, SoeEntityState.Deleted);
            }

            return result;
        }

        #endregion

        #region SMS

        private ActionResult SendSMS(CompEntities entities, int actorCompanyId, Message message, MessageText messageText, String shortText, List<MessageRecipient> recipients, List<SysCountry> sysCountries, SMSProvider provider = SMSProvider.Pixie)
        {
            ActionResult result = new ActionResult();
            int responseCode = -1;
            bool isSent = false;
            String responseMessage = String.Empty;
            String areaCode = string.Empty;
            List<MessageRecipient> smsRecipients = new List<MessageRecipient>();
            List<String> recipientMobileNrs = new List<String>();
            String smsText = String.Empty;
            var company = CompanyManager.GetCompany(entities, actorCompanyId, loadLicense: true);

            try
            {
                #region Prereq

                if (message == null || messageText == null || recipients.Count == 0)
                {
                    result = new ActionResult(false, (int)ActionResultSave.SMSInsuficientData, GetText(8264, "Felaktiga inparametrar"));
                    return result;
                }

                if (!company.SysCountryId.HasValue)
                {
                    result = new ActionResult(false, (int)ActionResultSave.AreaCodeNotFound, GetText(8263, "Kan inte hitta korrekt riktnummer"));
                    return result;
                }

                var sysCountry = sysCountries?.FirstOrDefault(c => c.SysCountryId == company.SysCountryId.Value);
                if (sysCountry == null)
                {
                    result = new ActionResult(false, (int)ActionResultSave.AreaCodeNotFound, GetText(8263, "Kan inte hitta korrekt riktnummer"));
                    return result;
                }

                areaCode = sysCountry.AreaCode;

                foreach (var recipient in recipients)
                {
                    String mobile = UserManager.GetUserPhoneMobile(entities, recipient.UserId);
                    if (!string.IsNullOrEmpty(mobile))
                    {
                        mobile = mobile.Replace("-", "");
                        mobile = mobile.Replace(" ", "");

                        if (!mobile.StartsWith(areaCode))
                        {
                            #region Country specific
                            if (sysCountry.SysCountryId == (int)TermGroup_Country.SE)
                            {
                                if (mobile.Length != 10)
                                    continue;

                                mobile = mobile.Substring(1, mobile.Length - 1);
                            }
                            else
                                continue; //for now

                            #endregion

                            mobile = areaCode + mobile;
                        }
                        recipientMobileNrs.Add(mobile);
                        smsRecipients.Add(recipient);
                    }
                }

                if (!string.IsNullOrEmpty(message.Subject))
                    smsText = message.Subject;

                if (!string.IsNullOrEmpty(shortText))
                    smsText += "\n" + shortText;

                result = CommunicatorConnector.SendSMSMessage(new MailMessageDTO()
                {
                    body = smsText,
                    recievers = recipientMobileNrs,
                    SenderName = "Soe",
                    LicenseGuid = company.License.LicenseGuid,
                    CompanyGuid = company.CompanyGuid,
                    Source = "Soe",
                    SourceKey = "SmsCopy",
                    key = "SmsCopy"
                });

                if (result?.Success == true)
                {
                    isSent = true;
                    responseCode = 0;
                    responseMessage = "SMS sent successfully";
                }
                else
                {
                    isSent = false;
                    if (result == null)
                    {
                        result = new ActionResult(false, (int)ActionResultSave.SendSMSResponseIsNull, "No content from communicator");
                    }
                    else
                    {
                        result.Success = false;
                        if (result.ErrorNumber == 0)
                            result.ErrorNumber = (int)ActionResultSave.SendSMSResponseReturnedFailed;
                    }
                    responseCode = result?.ErrorNumber ?? 0;
                    responseMessage = result?.ErrorMessage ?? "No errormessage from communicator";
                }

                #endregion

            }
            catch (Exception ex)
            {
                result.Exception = ex;
                base.LogError(ex, this.log);
            }
            finally
            {
                if (result.Success)
                {
                    //Set success properties
                }
                else
                {
                    #region Log error

                    string errorMessage = "Send SMS failed." + "(isSent = " + isSent.ToString() + ")" + " (" + actorCompanyId + ") ";

                    if (result.ErrorNumber == (int)ActionResultSave.SendSMSResponseReturnedFailed)
                    {
                        //Provider errorcode
                        errorMessage += "ResponseCode= " + responseCode + " ResponseMessage: " + responseMessage;
                    }
                    else if (result.ErrorNumber == (int)ActionResultSave.SendSMSResponseIsNull)
                    {
                        errorMessage += "Response is null";
                    }
                    else
                    {
                        //XE errornumber
                        errorMessage += "ErrorNumber= " + result.ErrorNumber + " ErrorMessage: " + result.ErrorMessage;
                    }
                    base.LogError(errorMessage);

                    #endregion
                }
            }

            return result;
        }

        #endregion

        #region ftp

        public ActionResult TryDeletePayrollfromICASftp(int actorCompanyId, int payrollExportId)
        {
            ActionResult result = new ActionResult();

            try
            {
                Company company = CompanyManager.GetCompany(actorCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                TimeSalaryExport payrollExport = TimeSalaryManager.GetTimeSalaryExport(payrollExportId, actorCompanyId, false);

                // Create filename and path
                string externalExportId = TimeSalaryManager.GetExternalExportId(actorCompanyId);

                var test = ConfigurationSetupUtil.GetSiteType() == TermGroup_SysPageStatusSiteType.Test ? @"test\" : "";

                if (!string.IsNullOrEmpty(externalExportId))
                {
                    string YearMonth = payrollExport.StartInterval.Year.ToString().Substring(2, 2) + (payrollExport.StartInterval.Month.ToString().Length == 1 ? "0" + payrollExport.StartInterval.Month.ToString() : payrollExport.StartInterval.Month.ToString());
                    string fileName = $"Sonesvl{externalExportId}_{YearMonth}_SE{company.CompanyNr}.txt"; // Sonesvl<SvLcompanynumber>_ÅÅPP_SE<butiksID>.txt”
                    string path = $@"ica\timedata\{test}{fileName}";

                    SshUtil sshUtil = new SshUtil(true);
                    result = sshUtil.Delete(path);
                }
            }
            catch
            {
                result.Success = false;
            }

            return result;
        }


        public ActionResult SendPayrollToSftp(int actorCompanyId, int payrollExportId)
        {
            Company company = CompanyManager.GetCompany(actorCompanyId);
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            var type = (SoeTimeSalaryExportTarget)SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportTarget, 0, actorCompanyId, 0);
            string companyNumber = TimeSalaryManager.GetSecondExternalExportId(actorCompanyId);
            var test = ConfigurationSetupUtil.GetSiteType() == TermGroup_SysPageStatusSiteType.Test ? @"test\" : "";

            if (string.IsNullOrEmpty(companyNumber))
            {
                if (company.CompanyNr < 1000)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "CompanyNr != SvLcompanynumber");

                if (company.CompanyNr == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "CompanyNr == null");

                companyNumber = company.CompanyNr.ToString();
            }

            if (type == SoeTimeSalaryExportTarget.Pol)
            {
                using (CompEntities entities = new CompEntities())
                {
                    //Filnamn för 43,45 och 52 transfilerna:
                    //Test: T_SoftOne_POL_Tid_Export_xx_yyyy - mm - ddtttttt.txt
                    //Prod: P_SoftOne_POL_Tid_Export_xx_yyyy - mm - ddtttttt.txt

                    var isTest = ConfigurationSetupUtil.GetSiteType() == TermGroup_SysPageStatusSiteType.Test;
                    TimeSalaryExport payrollExport = TimeSalaryManager.GetTimeSalaryExport(entities, payrollExportId, actorCompanyId, false);
                    var jobs = ScheduledJobManager.GetScheduledJobHeads(entities, actorCompanyId);
                    var job = jobs.FirstOrDefault(a => a.State == (int)SoeEntityState.Active && a.Name.ToLower().Contains("pol"));
                    var settings = entities.ScheduledJobSetting.Where(w => w.State == (int)SoeEntityState.Active && w.ScheduledJobHeadId == job.ScheduledJobHeadId).ToList();

                    var username = settings.FirstOrDefault(f => f.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser)?.StrData;
                    var password = settings.FirstOrDefault(f => f.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword)?.StrData;
                    var path = settings.FirstOrDefault(f => f.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath)?.StrData;

                    var files = ZipUtility.UnzipFilesInZipFile(payrollExport.File1);

                    var uploadResult = new ActionResult();
                    foreach (var file in files)
                    {
                        string fileName = isTest && file.Key.StartsWith("P_") ? file.Key.Replace("P_", "T_") : file.Key;

                        // rename the date/time-part in filename to current
                        if (fileName.Contains("_"))
                        {
                            string[] fileNameParts = fileName.Split('_');
                            var now = DateTime.Now;
                            var twoDigitMonth = now.Month.ToString().PadLeft(2, '0');
                            var twoDigitDay = now.Day.ToString().PadLeft(2, '0');
                            var twoDigitHour = now.Hour.ToString().PadLeft(2, '0');
                            var twoDigitMinute = now.Minute.ToString().PadLeft(2, '0');
                            var twoDigitSecond = now.Second.ToString().PadLeft(2, '0');

                            fileNameParts[fileNameParts.Length - 1] = $"{now.Year}-{twoDigitMonth}-{twoDigitDay}{twoDigitHour}{twoDigitMinute}{twoDigitSecond}.txt";
                            fileName = String.Join("_", fileNameParts);
                        }

                        FileExportResult exportResult = new FileExportResult()
                        {
                            Base64Data = Convert.ToBase64String(file.Value),
                            FileName = fileName,
                            Data = file.Value,
                        };

                        uploadResult = BridgeManager.SSHUpload(settings, exportResult);

                        // TODO: BridgeManager return failed result even though file is uploaded
                        // Until that is solved we return failed result outside of this loop.
                    }
                    if (!uploadResult.Success)
                        return uploadResult;

                    return new ActionResult(true);
                }
            }
            else if (type == SoeTimeSalaryExportTarget.SvenskLon)
            {
                using (CompEntities entities = new CompEntities())
                {
                    TimeSalaryExport payrollExport = TimeSalaryManager.GetTimeSalaryExport(entities, payrollExportId, actorCompanyId, false);

                    // Create filename and path
                    string externalExportId = TimeSalaryManager.GetExternalExportId(entities, actorCompanyId);
                    string YearMonth = payrollExport.StartInterval.Year.ToString().Substring(2, 2) + (payrollExport.StartInterval.Month.ToString().Length == 1 ? "0" + payrollExport.StartInterval.Month.ToString() : payrollExport.StartInterval.Month.ToString());
                    string fileName = $"Sonesvl{externalExportId}_{YearMonth}_SE{companyNumber}.txt"; // ”Softone<SvLcompanynumber>_ÅÅPP_SE<butiksID>.txt”
                    string path = $@"ica\timedata\{test}{fileName}";

                    SshUtil sshUtil = new SshUtil(true);
                    ActionResult result = sshUtil.Upload(payrollExport.File1, path);
                    if (result.Success)
                    {
                        payrollExport.Comment = $"Sonesvl Sändes: {CalendarUtility.ToShortDateTimeString(DateTime.Now)}";
                        result = SaveChanges(entities);
                    }
                    return result;
                }
            }
            else if (type == SoeTimeSalaryExportTarget.BlueGarden)
            {
                // FIlename SoftOne_ Externt exportid (4 siffror)_enhetsnummer (2 siffror)_YYMM (löneperiod)_ yyyyMMddHHmmss.txt (tidsstämpel) exempel SoftOne_1234_01_2401_ 20240205101535.txt
                using (CompEntities entities = new CompEntities())
                {
                    var exportExternalExportSubId = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.SalaryExportExternalExportSubId, 0, actorCompanyId, 0);

                    if (exportExternalExportSubId.IsNullOrEmpty())
                        exportExternalExportSubId = "01";

                    TimeSalaryExport payrollExport = TimeSalaryManager.GetTimeSalaryExport(entities, payrollExportId, actorCompanyId, false);

                    // Create filename and path
                    string externalExportId = TimeSalaryManager.GetExternalExportId(entities, actorCompanyId);
                    string YearMonth = payrollExport.StartInterval.Year.ToString().Substring(2, 2) + (payrollExport.StartInterval.Month.ToString().Length == 1 ? "0" + payrollExport.StartInterval.Month.ToString() : payrollExport.StartInterval.Month.ToString());
                    string fileName = $"SoftOne_{externalExportId}_{exportExternalExportSubId}_{YearMonth}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
                    string path = $@"ica\timedata\{test}{fileName}";

                    SshUtil sshUtil = new SshUtil(true);
                    ActionResult result = sshUtil.Upload(payrollExport.File1, path);
                    if (result.Success)
                    {
                        payrollExport.Comment = $"HR fil Sändes: {CalendarUtility.ToShortDateTimeString(DateTime.Now)}";
                        result = SaveChanges(entities);
                    }
                    return result;
                }
            }
            else
            {
                return new ActionResult((int)ActionResultSave.EntityNotFound, "No valid target found");
            }
        }

        #endregion

        #region PayrollSlip
        public void SendXEMailPayrollSlipPublishedWhenLockingPeriod(int actorCompanyId, List<int> sendPayrollSlipPublishedUserIds, int senderUserId, int senderRoleId)
        {
            bool publishPayrollSlipWhenLockingPeriod = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.PublishPayrollSlipWhenLockingPeriod, 0, actorCompanyId, 0);
            bool sendNoticeWhenPayrollSlipPublished = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SendNoticeWhenPayrollSlipPublished, 0, actorCompanyId, 0);

            if (sendNoticeWhenPayrollSlipPublished && publishPayrollSlipWhenLockingPeriod && !sendPayrollSlipPublishedUserIds.IsNullOrEmpty())
                Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SendXEMailToEmployeeWhenPayrollSlipPublished(actorCompanyId, sendPayrollSlipPublishedUserIds, DateTime.Now, senderUserId, senderRoleId)));

        }

        public void SendXEMailPayrollSlipPublishedWhenCreatingBankfile(int actorCompanyId, List<int> sendPayrollSlipPublishedUserIds, DateTime? publishDate, int senderUserId, int senderRoleId)
        {
            bool publishPayrollSlipWhenLockingPeriod = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.PublishPayrollSlipWhenLockingPeriod, 0, actorCompanyId, 0);
            bool sendNoticeWhenPayrollSlipPublished = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SendNoticeWhenPayrollSlipPublished, 0, actorCompanyId, 0);

            if (sendNoticeWhenPayrollSlipPublished && !publishPayrollSlipWhenLockingPeriod && !sendPayrollSlipPublishedUserIds.IsNullOrEmpty() && publishDate.HasValue)
                Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SendXEMailToEmployeeWhenPayrollSlipPublished(actorCompanyId, sendPayrollSlipPublishedUserIds, publishDate.Value, senderUserId, senderRoleId)));
        }

        private ActionResult SendXEMailToEmployeeWhenPayrollSlipPublished(int actorCompanyId, List<int> userIds, DateTime publishDate, int senderUserId, int senderRoleId)
        {
            ActionResult result = null;
            try
            {
                if (!SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SendNoticeWhenPayrollSlipPublished, 0, actorCompanyId, 0))
                    return new ActionResult(true);

                DateTime dateTimeBegin = DateTime.Now;

                LogInfo("SendXEMailToEmployeeWhenPayrollSlipPublished: " + actorCompanyId + ", UserCount: " + userIds.Count);

                #region Prereq

                List<User> recievers = UserManager.GetUsers(userIds);
                User sender = UserManager.GetUser(senderUserId);
                if (sender == null)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8349, "Användare kunde inte hittas"));

                StringBuilder body = new StringBuilder();

                string subject = GetText(8864, "Ny lönespecifikation");
                body.Append(string.Format(GetText(8865, "Härliga nyheter!")));
                body.Append(Environment.NewLine);

                if (publishDate.Date <= DateTime.Now.Date)
                    body.Append(string.Format(GetText(8867, "Du har nu en ny lönespecifikation att läsa.")));
                else
                    body.Append(string.Format(GetText(8866, "Du har en ny lönespecifikation att läsa från detta datum {0}."), publishDate.ToShortDateString()));

                #endregion

                #region Perform

                LogInfo("SendXEMailToEmployeeWhenPayrollSlipPublished: " + actorCompanyId + ", Start sending messages, RecieverCount: " + recievers.Count);

                MessageEditDTO messageDto = new MessageEditDTO()
                {
                    ParentId = 0,
                    AnswerType = XEMailAnswerType.None,
                    LicenseId = sender.LicenseId,
                    ActorCompanyId = actorCompanyId,
                    RoleId = senderRoleId,
                    SenderUserId = senderUserId,
                    SenderName = sender.Name,
                    Subject = subject,
                    Text = StringUtility.ConvertNewLineToHtml(body.ToString()),
                    ShortText = body.ToString(),
                    Entity = SoeEntityType.Employee,
                    Created = DateTime.Now,
                    SentDate = DateTime.Now,
                    MessagePriority = TermGroup_MessagePriority.None,
                    MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                    MessageTextType = TermGroup_MessageTextType.Text,
                    MessageType = TermGroup_MessageType.PayrollSlip,
                    RecordId = 0,
                    Recievers = new List<MessageRecipientDTO>(),
                    ForceSendToEmailReceiver = true,
                };

                foreach (var reciever in recievers)
                {
                    messageDto.Recievers.Add(
                        new MessageRecipientDTO()
                        {
                            Type = XEMailRecipientType.User,
                            UserId = reciever.UserId,
                            UserName = reciever.LoginName,
                            Name = reciever.Name,
                        }
                    );
                }

                result = SendXEMail(messageDto, actorCompanyId, senderRoleId, senderUserId, dontSendPush: true, sender: sender);

                if (result.Success)
                {
                    LogInfo("SendXEMailToEmployeeWhenPayrollSlipPublished: " + actorCompanyId + ", Start sending pushnotifications, Count: " + recievers.Count);
                    string notificationMsg = GetText(8378, "Du har fått ett meddelande från") + " " + sender.Name + ".";
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    this.SendPushNotification(entitiesReadOnly, recievers, notificationMsg, PushNotificationType.XEMail, result.IntegerValue);
                }

                #endregion

                LogInfo("SendXEMailToEmployeeWhenPayrollSlipPublished: " + actorCompanyId + ", Minutes: " + (DateTime.Now - dateTimeBegin).TotalMinutes);
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                base.LogError(ex, this.log);
            }

            return result;

        }

        #endregion

        #region PushNotifications

        private void SendMessagePushNotificationFireAndForget(int messageId, List<int> overrideRecieverIds = null)
        {
           Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SendMessagePushNotification(messageId, overrideRecieverIds)));
            Thread.Sleep(10);
        }

        public void SendMessagePushNotification(int messageId, List<int> overrideRecieverIds = null)
        {
            if (messageId == 0)
                return;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    Message msg = GetMessage(entities, messageId);
                    if (msg == null)
                        return;

                    string notificationMsg = GetText(8378, "Du har fått ett meddelande från") + " " + msg.SenderName + ".";

                    if (!string.IsNullOrEmpty(msg.Subject) && UseMessageSubjectAsPushMessage((TermGroup_MessageType)msg.Type))
                        notificationMsg = msg.Subject;

                    int recordId = msg.MessageId;
                    PushNotificationType notificationType = PushNotificationType.XEMail;

                    if (msg.Entity == (int)SoeEntityType.Order && msg.Type == (int)TermGroup_MessageType.None)
                    {
                        #region Order

                        var order = InvoiceManager.GetCustomerInvoiceSmallEx(entities, msg.RecordId);
                        if (order != null)
                        {
                            recordId = order.InvoiceId;
                            notificationType = PushNotificationType.Order;

                            if (order.ActorId.HasValue)
                            {
                                int contactId = ContactManager.GetContactIdFromActorId(entities, order.ActorId.Value);

                                //Delivery adress
                                List<ContactAddress> deliveryAddresses = ContactManager.GetContactAddresses(entities, contactId, TermGroup_SysContactAddressType.Delivery, false);

                                string deliveryAddress = string.Empty;
                                ContactAddress orderDeliveryAddress = deliveryAddresses.FirstOrDefault(a => a.ContactAddressId == order.DeliveryAddressId);
                                if (orderDeliveryAddress != null)
                                    deliveryAddress = orderDeliveryAddress.Address;

                                notificationMsg += " " + GetText(8013, "Ordernr") + ": " + order.InvoiceNr;
                                notificationMsg += ", " + GetText(8516, "Leveransadress") + ": " + deliveryAddress;
                            }
                        }

                        #endregion
                    }

                    List<int> recieverIds = overrideRecieverIds == null ? GetMessageRecipientUserIds(entities, messageId) : overrideRecieverIds;
                    var users = UserManager.GetUsers(entities, recieverIds);

                    this.SendPushNotification(entities, users, notificationMsg, notificationType, recordId);
                }
                catch (Exception e)
                {
                    base.LogError(e, this.log);
                }
            }
        }

        public void SendPushNotification(CompEntities entities, List<User> users, string notificationMsg, PushNotificationType notificationType, int recordId)
        {
            try
            {
                bool releaseMode = CompDbCache.Instance.SiteType != TermGroup_SysPageStatusSiteType.Test;
                bool useGUID = SettingManager.GetBoolSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.PushNotificationUseGuid, 0, 0, 0);
                List<User> usersWithUpdatedApp = new List<User>();
                ActionResult notificationSendResult;
                DateTime start = DateTime.Now;

                foreach (var user in users)
                {
                    TermGroup_BrandingCompanies brandingCompany = (TermGroup_BrandingCompanies)SettingManager.GetIntSetting(SettingMainType.License, (int)LicenseSettingType.BrandingCompany, 0, 0, user.LicenseId);

                    string recieverId = StringUtility.GetPushNotificationId(user.UserId, useGUID ? user.idLoginGuid : (Guid?)null);
                    if (usersWithUpdatedApp.Any() || PushWithCommunicator(entities, user.UserId, releaseMode))
                    {
                        usersWithUpdatedApp.Add(user);
                        continue;
                    }
                    else
                    {
                        TXLPushNotification notification = new TXLPushNotification(recieverId, notificationMsg, notificationType, recordId, releaseMode, brandingCompany);
                        notificationSendResult = notification.Send(SoeMobileAppType.GO);
                    }

                    if (!notificationSendResult.Success && !notificationSendResult.ErrorMessage.Contains("failure.nouser"))
                    {
                        String errorMsg = GetText(8379, "Notifiering kunde inte skickas") + " " + GetText(8266, "Felkod:") + "  -2- " + " ( " + notificationSendResult.ErrorNumber + " " + notificationSendResult.ErrorMessage + " ) ";
                        base.LogError(errorMsg);
                    }
                }

                if (usersWithUpdatedApp.Any())
                {
                    var recieverKeys = usersWithUpdatedApp.Select(user => StringUtility.GetPushNotificationId(user.UserId, useGUID ? user.idLoginGuid : (Guid?)null)).ToList();
                    var message = CommunicatorMessageHelper.CreateAzureNotificationHubMessage(recieverKeys,
                                                                                                notificationType == PushNotificationType.Order ? CommunicatorPushNotificationType.Order : CommunicatorPushNotificationType.GoMail,
                                                                                                notificationType == PushNotificationType.Order ? CommunicatorMetaDataType.OrderId : CommunicatorMetaDataType.MessageId,
                                                                                                recordId,
                                                                                                notificationMsg);

                    if (!message.ValidGoMailPushNotification())
                    {
                        base.LogError($"Could not send pushnotification, pushNotificationType setting upp not sufficient");
                        return;
                    }

                    CommunicatorConnector.SendCommunicatorMessage(message);
                    notificationSendResult = CommunicatorConnector.SendCommunicatorMessage(message);
                }

                DateTime stop = DateTime.Now;
                double minutes = stop.Subtract(start).TotalMinutes;
                if (minutes > 5)
                    base.LogInfo($"Send pushnotification is taking more then 5 minutes. User count: {users.Count}. Minutes: {minutes}. MessageId: {recordId}");


            }
            catch (Exception e)
            {
                base.LogError(e, this.log);
            }
        }

        public bool SendInformationPushNotification(Information companyInformation, SysInformation sysInformation)
        {
            try
            {
                if (companyInformation == null && sysInformation == null)
                {
                    base.LogInfo($"Could not send pushnotification, insufficient parameters.");
                    return false;
                }

                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                bool releaseMode = CompDbCache.Instance.SiteType != TermGroup_SysPageStatusSiteType.Test;
                List<User> users = new List<User>();
                string subject = string.Empty;
                PushNotificationType notificationType = PushNotificationType.None;
                var pushNotificationType = CommunicatorPushNotificationType.None;
                int informationId = 0;
                int? langId = null;
                if (companyInformation != null)
                {
                    langId = companyInformation.SysLanguageId;
                    notificationType = PushNotificationType.CompInformation;
                    pushNotificationType = CommunicatorPushNotificationType.CompInformation;
                    informationId = companyInformation.InformationId;
                    subject = companyInformation.Subject;
                    if (companyInformation != null && !companyInformation.NotificationSent.HasValue)
                        users = GetValidUsersForMessageGroups(entitiesReadOnly, companyInformation.InformationMessageGroup.Where(x => x.State == (int)SoeEntityState.Active).Select(x => x.MessageGroupId).ToList(), companyInformation.ActorCompanyId, 0, 0);
                }
                else if (sysInformation != null)
                {
                    langId = sysInformation.SysLanguageId;
                    notificationType = PushNotificationType.SysInformation;
                    pushNotificationType = CommunicatorPushNotificationType.SysInformation;
                    informationId = sysInformation.SysInformationId;
                    subject = sysInformation.Subject;
                    if (!sysInformation.NotificationSent.HasValue)
                    {
                        int? sysCompDbId = SysServiceManager.GetSysCompDBId();
                        if (sysCompDbId.HasValue && (sysInformation.ShowOnAllSysCompDbs || sysInformation.SysInformationSysCompDb.Any(x => x.SysCompDbId == sysCompDbId && !x.NotificationSent.HasValue)))
                            users = GeneralManager.GetSysInformationRecipients(sysInformation.SysInformationId);
                    }
                }

                users = users.Where(u => !u.LangId.HasValue || u.LangId.Value == langId).ToList();

                if (!users.Any())
                {
                    // No valid users, still return true to update that notification has been sent
                    return true;
                }

                bool atleastOneNotificationHasBeenSent = false;
                string notificationMsg = GetText(0, "Ny information") + ": " + subject;
                bool useGUID = SettingManager.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.PushNotificationUseGuid, 0, 0, 0);

                List<User> usersWithUpdatedApp = new List<User>();
                ActionResult notificationSendResult;
                foreach (var user in users)
                {
                    TermGroup_BrandingCompanies brandingCompany = (TermGroup_BrandingCompanies)SettingManager.GetIntSetting(SettingMainType.License, (int)LicenseSettingType.BrandingCompany, 0, 0, user.LicenseId);

                    if (usersWithUpdatedApp.Any() || PushWithCommunicator(entitiesReadOnly, user.UserId, releaseMode))
                    {
                        usersWithUpdatedApp.Add(user);
                        continue;
                    }
                    else
                    {
                        string recieverId = StringUtility.GetPushNotificationId(user.UserId, useGUID ? user.idLoginGuid : (Guid?)null);

                        TXLPushNotification notification = new TXLPushNotification(recieverId, notificationMsg, notificationType, informationId, releaseMode, brandingCompany);

                        notificationSendResult = notification.Send(SoeMobileAppType.GO);
                        if (!notificationSendResult.Success && !notificationSendResult.ErrorMessage.Contains("failure.nouser"))
                        {
                            String errorMsg = GetText(8379, "Notifiering kunde inte skickas") + " " + GetText(8266, "Felkod:") + "  -2- " + " ( " + notificationSendResult.ErrorNumber + " " + notificationSendResult.ErrorMessage + " ) ";
                            base.LogInfo(errorMsg);
                        }
                    }

                    if (notificationSendResult.Success)
                        atleastOneNotificationHasBeenSent = true;
                }

                if (usersWithUpdatedApp.Any())
                {
                    var recieverKeys = usersWithUpdatedApp.Select(user => StringUtility.GetPushNotificationId(user.UserId, useGUID ? user.idLoginGuid : (Guid?)null)).ToList();
                    var message = CommunicatorMessageHelper.CreateAzureNotificationHubMessage(recieverKeys,
                                                                                                pushNotificationType,
                                                                                                pushNotificationType == CommunicatorPushNotificationType.CompInformation ? CommunicatorMetaDataType.CompInformationId : CommunicatorMetaDataType.SysInformationId,
                                                                                                informationId,
                                                                                                notificationMsg);

                    if ((pushNotificationType == CommunicatorPushNotificationType.CompInformation && !message.ValidCompInformationPushNotification()) || pushNotificationType == CommunicatorPushNotificationType.SysInformation && !message.ValidSysInformationPushNotification())
                    {
                        base.LogInfo($"Could not send pushnotification, pushNotificationType setting upp not sufficient");
                        return false;
                    }

                    CommunicatorConnector.SendCommunicatorMessage(message);
                    notificationSendResult = CommunicatorConnector.SendCommunicatorMessage(message);

                    if (notificationSendResult.Success)
                        atleastOneNotificationHasBeenSent = true;

                    return atleastOneNotificationHasBeenSent;
                }
            }
            catch (Exception e)
            {
                base.LogError(e, this.log);
            }

            return false;
        }

        public ActionResult RegisterDevice(User user, string tag, string pushToken, MobileDeviceType deviceType, string installationId)
        {
            RegisterDevice device = new RegisterDevice()
            {
                CommunicatorMobileDeviceType = (CommunicatorMobileDeviceType)deviceType,
                IdLoginGuid = user.idLoginGuid.Value,
                InstallationId = installationId,
                PNId = tag,
                PushToken = pushToken
            };

            return CommunicatorConnector.RegisterDevice(device);
        }

        public void SendMessagePushNotificationSystemDown(List<string> recieverIds)
        {
            if (!recieverIds.Any())
                return;

            try
            {
                string notificationMsg = "";//
                PushNotificationType notificationType = PushNotificationType.XEMail;
                foreach (var recieverId in recieverIds)
                {
                    TXLPushNotification notification = new TXLPushNotification(recieverId, notificationMsg, notificationType, 0, true, TermGroup_BrandingCompanies.SoftOne);
                    ActionResult notificationSendResultGO = notification.Send(SoeMobileAppType.GO);
                    if (!notificationSendResultGO.Success && !notificationSendResultGO.ErrorMessage.Contains("failure.nouser"))
                    {
                        string errorMsg = "Notifiering kunde inte skickas" + " " + "Felkod:" + "  -2- " + " ( " + notificationSendResultGO.ErrorNumber + " " + notificationSendResultGO.ErrorMessage + " ) ";
                        LogError(errorMsg);
                    }
                }
            }
            catch (Exception)
            {
                //Do nothing
            }

        }

        #region Push with Communicator
        public bool PushWithCommunicator(CompEntities entities, int userId, bool releaseMode)
        {
            if (releaseMode)
                return false;

            string key = $"PushWithCommunicator{userId}";
            var value = BusinessMemoryCache<bool?>.Get(key);

            if (value.HasValue)
                return value.Value;

            var usersessions = entities.UserSession.Where(w => w.User.UserId == userId && w.MobileLogin).OrderByDescending(o => o.UserSessonId).Take(2);

            if (usersessions.Any(a => string.IsNullOrEmpty(a.Platform) && a.Platform.Contains("Mobile Api Version: 38.0")))
            {
                BusinessMemoryCache<bool?>.Set(key, true, 60 * 60 * 24);
                return true;
            }
            else
            {
                BusinessMemoryCache<bool?>.Set(key, false, 60 * 30);
                return false;
            }
        }

        #endregion

        #region Helper
        private bool UseMessageSubjectAsPushMessage(TermGroup_MessageType type)
        {

            switch (type)
            {
                case TermGroup_MessageType.SwapRequest:
                case TermGroup_MessageType.ShiftRequest:
                case TermGroup_MessageType.ScheduledChanged:
                case TermGroup_MessageType.AbsenceRequest:
                case TermGroup_MessageType.AbsenceAnnouncement:
                case TermGroup_MessageType.AbsencePlanning:
                case TermGroup_MessageType.TimeWorkAccountYearEmployeeOption:
                case TermGroup_MessageType.PayrollSlip:

                    return true;
                default:
                    return false;
            }

        }
        #endregion

        #endregion

        #region Job

        public ActionResult SendXEMailValidateEmailToUser(UserDTO sender, UserDTO receiver, string url)
        {
            #region Prereq

            // Sender        
            if (sender == null)
                return new ActionResult((int)ActionResultSave.NothingSaved, "Sender kunde inte hittas");

            if (!sender.DefaultActorCompanyId.HasValue)
                return new ActionResult((int)ActionResultSave.NothingSaved, "Sender har ingen angiven förvald företag");

            #endregion

            #region Create subject/body

            string subject;
            string body;

            if (receiver.LangId == (int)TermGroup_Languages.Finnish)
            {
                subject = "Sinun tarvitsee liittää sähköpostiosoite käyttäjätietoihisi";

                body = "Hei!";
                body += Environment.NewLine;
                body += "\nOlemme havainneet, että käyttäjätiedoistasi puuttuu sähköpostiosoite, jota tarvitaan salasanan palauttamisessa.";
                body += Environment.NewLine;
                body += String.Format("\nOle ystävällinen ja kirjaudu osoitteessa {0}.", url);
                body += Environment.NewLine;
                body += "\nPääset lisäämään  sähköpostiosoitteesi siellä.";
                body += Environment.NewLine;
                body += "\nKiitos!";
                body += "\nSoftOne";
            }
            else
            {
                subject = "Du behöver koppla en e-postadress till ditt konto";

                body = "Hej!";
                body += Environment.NewLine;
                body += "\nVi har upptäckt att ditt konto saknar registrerad e-postadress som behövs vid tex återställning av lösenord.";
                body += Environment.NewLine;
                body += String.Format("\nVar god surfa till {0} och logga in med dina uppgifter.", url);
                body += Environment.NewLine;
                body += "\nDu kommer att få möjlighet att registrera din e-postadress direkt där.";
                body += Environment.NewLine;
                body += "\nTack på förhand!";
                body += "\nSoftOne";
            }

            #endregion

            #region Send to receiver

            MessageEditDTO message = new MessageEditDTO()
            {
                Entity = SoeEntityType.User,
                RecordId = 0,
                //ActorCompanyId = sender.DefaultActorCompanyId,
                ActorCompanyId = null,
                //LicenseId = sender.LicenseId,
                LicenseId = receiver.LicenseId,
                SenderUserId = sender.UserId,
                Subject = subject,
                Text = StringUtility.ConvertNewLineToHtml(body),
                ShortText = body,
                SenderName = sender.Name,
                SenderEmail = "",
                Created = DateTime.Now,
                SentDate = DateTime.Now,
                MessagePriority = TermGroup_MessagePriority.None,
                MessageType = TermGroup_MessageType.AutomaticInformation,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.Text,
            };

            MessageRecipientDTO recipient = new MessageRecipientDTO()
            {
                UserId = receiver.UserId,
                SendCopyAsEmail = false,
                UserName = receiver.LoginName,
                Name = receiver.Name,
                EmailAddress = receiver.Email,
                Type = XEMailRecipientType.User,
            };

            message.Recievers.Add(recipient);
            return SendXEMail(message, sender.DefaultActorCompanyId ?? 0, 0, sender.UserId);

            #endregion
        }

        #endregion

        #region Communicator Inbound Emails

        public List<IncomingEmailGridDTO> GetIncomingEmailGridDTOs(IncomingEmailFilterDTO filter)
        {
            if (filter == null)
                return new List<IncomingEmailGridDTO>();

            var _filter = new InboundEmailFilterDTO
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                DeliveryStatus = filter.DeliveryStatus ?? new List<int>(),
                SenderEmail = filter.SenderEmail,
                RecipientEmails = filter.RecipientEmails.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Select(e => e.TrimStart().TrimEnd()).ToList(),
                NoOfRecords = filter.NoOfRecords > 0 ? filter.NoOfRecords : 100
            };

            return CommunicatorConnector.GetInboundEmailFilteredRows(_filter)
                .Select(e => new IncomingEmailGridDTO
                {
                    IncomingEmailId = e.InboundEmailId,
                    RecipientEmails = string.Join(";", e.Recipients),
                    Date = e.EmailDateTime,
                    SenderEmail = e.Sender,
                    AttachementNames = string.Join(";", e.Attachments),
                    DeliveryStatus = e.Status,
                    DeliveryStatusText = e.StatusString,
                }).OrderByDescending(x => x.Date).ToList();
        }

        public IncomingEmailDTO GetIncomingEmailDTO(int incomingEmailId)
        {
            if (incomingEmailId <= 0)
                return null;

            var email = CommunicatorConnector.GetInboundEmail(incomingEmailId);

            return new IncomingEmailDTO
            {
                IncomingEmailId = email.InboundEmailId,
                Received = email.Received,
                Subject = email.Subject,
                SpamScore = email.SpamScore,
                UniqueIdentifier = email.UniqueIdentifier,
                From = email.From,
                Text = email.Text,
                Html = email.Html,
                InboundEmails = email.InboundEmails.Select(r => new IncomingEmailAddressDTO
                {
                    Id = r.Id,
                    Type = r.Type,
                    EmailAddress = r.EmailAddress,
                    DeliveryStatus = r.DeliveryStatus,
                    Retries = r.Retries,
                    LastUpdated = r.LastUpdated,
                    DispatcherId = r.DispatcherId
                }).ToList(),
                Attachments = email.Attachments.Select(a => new IncomingEmailAttachmentDTO
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    Size = a.Size
                }).ToList(),
                Logs = email.Logs.Select(l => new IncomingEmailLogDTO
                {
                    Id = l.Id,
                    Message = l.Message,
                    CreatedOn = l.CreatedOn
                }).ToList()
            };
        }

        public ActionResult GetIncomingEmailAttachment(int attachmentId)
        {
            ActionResult result = new ActionResult();
            if (attachmentId <= 0)
                result.StringValue = "";
            else
                result.StringValue = CommunicatorConnector.GetInboundEmailAttachment(attachmentId);

            return result;
        }

        #endregion
    }
}
