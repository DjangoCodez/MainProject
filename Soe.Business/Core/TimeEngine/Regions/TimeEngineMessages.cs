using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Mail
        private ActionResult SendXEMail(UserDTO receiver, User sender, int roleId, string subject, string body, string shorttext, SoeEntityType entity, TermGroup_MessageType messageType, int recordId, bool sendCopyAsEmail = false, bool forceSendToReceiver = false)
        {
            var user = entities.User.FirstOrDefault(f => f.UserId == receiver.UserId);
            return SendXEMail(user, sender, roleId, subject, body, shorttext, entity, messageType, recordId, sendCopyAsEmail, forceSendToReceiver);
        }
        private ActionResult SendXEMail(User receiver, User sender, int roleId, string subject, string body, string shorttext, SoeEntityType entity, TermGroup_MessageType messageType, int recordId, bool sendCopyAsEmail = false, bool forceSendToReceiver = false)
        {
            MessageEditDTO message = new MessageEditDTO()
            {
                Entity = entity,
                RecordId = recordId,
                ActorCompanyId = actorCompanyId,
                LicenseId = sender.LicenseId,
                SenderUserId = sender.UserId,
                Subject = subject,
                Text = body,
                ShortText = shorttext,
                SenderName = sender.Name,
                SenderEmail = sender.Email,
                Created = DateTime.Now,
                SentDate = DateTime.Now,
                MessagePriority = TermGroup_MessagePriority.None,
                MessageType = messageType,
                MessageDeliveryType = TermGroup_MessageDeliveryType.XEmail,
                MessageTextType = TermGroup_MessageTextType.Text,
                ForceSendToReceiver = forceSendToReceiver,
            };

            if (GetCompanyBoolSettingFromCache(CompanySettingType.UseDefaultEmailAddress))
            {
                string defaultCompanyEmailAddress = GetCompanyStringSettingFromCache(CompanySettingType.DefaultEmailAddress);
                if (defaultCompanyEmailAddress.Trim() != String.Empty)
                    message.SenderEmail = defaultCompanyEmailAddress;
            }

            MessageRecipientDTO recipient = new MessageRecipientDTO()
            {
                UserId = receiver.UserId,
                SendCopyAsEmail = sendCopyAsEmail,
                UserName = receiver.LoginName,
                Name = receiver.Name,
                EmailAddress = receiver.Email,
                Type = XEMailRecipientType.User,
            };
            message.Recievers.Add(recipient);

            return CommunicationManager.SendXEMail(message, actorCompanyId, roleId, sender.UserId);
        }
        private void SendXEMailOnAbsencePlanning(List<TimeSchedulePlanningDayDTO> shifts, int employeeId, int timeDeviationCauseId)
        {
            try
            {
                #region Prereq

                if (shifts.IsNullOrEmpty() || shifts.Any(x => x.TimeScheduleScenarioHeadId.HasValue))
                    return;

                TimeSchedulePlanningDayDTO first = shifts.OrderBy(x => x.StartTime.Date).FirstOrDefault();
                TimeSchedulePlanningDayDTO last = shifts.OrderBy(x => x.StopTime.Date).LastOrDefault();
                if (first == null || last == null)
                    return;

                TimeDeviationCause deviationCause = GetTimeDeviationCauseFromCache(timeDeviationCauseId);
                if (deviationCause == null)
                    return;

                User sender = GetUserFromCache();
                if (sender == null)
                    return;

                User receiver = UserManager.GetUserByEmployeeId(entities, employeeId, actorCompanyId);
                if (receiver == null || sender.UserId == receiver.UserId)
                    return;

                #endregion

                #region Create subject

                string subject = $"{GetText(8572, "Du har beviljats frånvaro")}  - {deviationCause.Name}: {first.StartTime.Date.ToShortDateString()} - {last.StopTime.Date.ToShortDateString()}";

                #endregion

                #region Create body

                int nbrOfApproved = shifts.Count(s => s.ApprovalTypeId == (int)TermGroup_YesNo.Yes);
                string body = nbrOfApproved + " " + GetText(8336, "pass har godkänts") + "." + "{0}";

                #endregion

                SendXEMail(receiver, sender, base.RoleId, subject, String.Format(body, "<br/>"), String.Format(body, "\n"), SoeEntityType.AbsencePlanning, TermGroup_MessageType.AbsencePlanning, 0);

            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }
        private void SendXEMailOnAbsenceRequestPlanning(bool isDefinitive, bool isForcedDefinitive, int shiftsLeftToProcess, EmployeeRequest absenceRequest, List<TimeSchedulePlanningDayDTO> shifts)
        {
            try
            {
                #region Prereq

                int nbrOfApproved = shifts.Count(s => s.ApprovalTypeId == (int)TermGroup_YesNo.Yes);
                var shiftsNotApproved = shifts.Where(s => s.ApprovalTypeId == (int)TermGroup_YesNo.No).ToList();
                int nbrOfNotApproved = shiftsNotApproved.Count;
                bool approved = shifts.Count == nbrOfApproved;
                bool notApporved = shifts.Count == nbrOfNotApproved;

                User sender = GetUserFromCache();
                User receiver = UserManager.GetUserByEmployeeId(absenceRequest.EmployeeId, actorCompanyId);

                #endregion

                #region Subject

                string subject = "";
                subject += GetText(8270, "Ledighetsansökan");

                if (approved)
                    subject += " " + GetText(8660, "Godkänd");
                else if (notApporved)
                    subject += " " + GetText(8661, "Nekad");

                subject += " - " + absenceRequest.TimeDeviationCauseName + ": ";
                DateTime dateFrom = absenceRequest.Start;
                DateTime dateTo = absenceRequest.Stop;
                if (absenceRequest.ExtendedAbsenceSetting != null && !absenceRequest.ExtendedAbsenceSetting.AbsenceWholeFirstDay && !absenceRequest.ExtendedAbsenceSetting.AbsenceWholeLastDay)
                {
                    if (absenceRequest.ExtendedAbsenceSetting.AbsenceFirstDayStart.HasValue)
                        dateFrom = CalendarUtility.MergeDateAndTime(dateFrom, absenceRequest.ExtendedAbsenceSetting.AbsenceFirstDayStart.Value);
                    if (absenceRequest.ExtendedAbsenceSetting.AbsenceLastDayStart.HasValue)
                        dateTo = CalendarUtility.MergeDateAndDefaultTime(dateTo, absenceRequest.ExtendedAbsenceSetting.AbsenceLastDayStart.Value);
                }

                subject += dateFrom.ToShortDateShortTimeString() + " - " + dateTo.ToShortDateShortTimeString();

                #endregion

                #region Create body

                StringBuilder body = new StringBuilder();
                if (isForcedDefinitive)
                {
                    body.Append(GetText(9926, "Din ansökan har stängts") + "." + "{0}");
                }
                else
                {
                    if (nbrOfApproved > 0)
                        body.Append(nbrOfApproved + " " + GetText(8336, "pass har godkänts") + "." + "{0}");
                    if (nbrOfNotApproved > 0)
                        body.Append(nbrOfNotApproved + " " + GetText(8337, "pass har inte godkänts") + "." + "{0}");
                    if (shiftsLeftToProcess > 0)
                        body.Append(shiftsLeftToProcess + " " + GetText(8338, "pass kvar att behandla") + "." + "{0}");

                    if (shiftsNotApproved.Any())
                    {
                        body.Append(GetText(8339, "Pass som inte blivit godkända:") + "{0}");
                        foreach (var shiftNotApproved in shiftsNotApproved)
                        {
                            body.Append(shiftNotApproved.StartTime.ToShortDateShortTimeString() + " - " + shiftNotApproved.StopTime.ToShortDateShortTimeString() + "{0}");
                        }
                    }

                    if (isDefinitive)
                        body.Append("{0}" + GetText(8271, "Ansökan är färdigbehandlad") + "." + "{0}");
                }

                if (GetCompanyBoolSettingFromCache(CompanySettingType.AbsenceRequestPlanningIncludeNoteInMessages))
                {
                    body.Append("{0}" + GetText(9222, "Notering") + ":" + "{0}");
                    body.Append(absenceRequest.Comment);
                }

                #endregion

                #region Send to receivers

                SendXEMail(receiver, sender, base.RoleId, subject, String.Format(body.ToString(), "<br/>"), String.Format(body.ToString(), "\n"), SoeEntityType.EmployeeRequest, TermGroup_MessageType.AbsenceRequest, absenceRequest.EmployeeRequestId);

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }
        private void SendXEMailOnAbsenceAnnouncement(List<TimeSchedulePlanningDayDTO> shifts, int employeeId, int timeDeviationCauseId, ref bool sentWithErrors)
        {
            try
            {
                #region Prereq

                if (shifts.IsNullOrEmpty())
                    return;

                TimeSchedulePlanningDayDTO first = shifts.OrderBy(x => x.StartTime.Date).FirstOrDefault();
                TimeSchedulePlanningDayDTO last = shifts.OrderBy(x => x.StopTime.Date).LastOrDefault();
                if (first == null || last == null)
                    return;

                sentWithErrors = false;

                TimeDeviationCause deviationCause = GetTimeDeviationCauseFromCache(timeDeviationCauseId);
                if (deviationCause == null)
                    return;

                Employee employee = GetEmployeeWithContactPersonFromCache(employeeId);
                if (employee == null)
                    return;

                User sender = GetUserFromCache();
                if (sender == null)
                    return;

                #endregion

                #region Create subject

                string subject = GetText(8459, "Frånvaroanmälan för") + " " + employee.Name;

                #endregion

                #region Create body

                DateTime dateStart = first.StartTime.Date;
                DateTime dateStop = last.StopTime.Date;

                string date = "";
                if (dateStart == dateStop)
                    date = dateStart.ToShortDateString();
                else
                    date = dateStart.ToShortDateString() + " - " + dateStop.ToShortDateString();

                StringBuilder body = new StringBuilder();
                body.Append(string.Format(GetText(8460, "{0} har anmält {1} {2}."), employee.Name, deviationCause.Name, date) + " ");
                body.Append(GetText(8461, "Följande pass har lagts som lediga pass") + " {0}");

                foreach (var shift in shifts)
                {
                    ShiftType shiftType = TimeScheduleManager.GetShiftType(shift.ShiftTypeId);
                    body.Append(shift.StartTime.ToShortDateShortTimeString() + " - " + shift.StopTime.ToShortDateShortTimeString() + " " + (shiftType?.Name ?? "") + " {0}");
                }

                #endregion

                #region Send to receivers

                List<UserDTO> receivers = UserManager.GetEmployeeNearestExecutives(entities, employee, dateStart, dateStop, actorCompanyId);
                foreach (UserDTO receiver in receivers)
                {
                    if (!SendXEMail(receiver, sender, base.RoleId, subject, String.Format(body.ToString(), "<br/>"), String.Format(body.ToString(), "\n"), SoeEntityType.AbsenceAnnouncement, TermGroup_MessageType.AbsenceAnnouncement, 0).Success)
                        sentWithErrors = true;
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                sentWithErrors = true;
            }
        }
        private void SendXEMailOnEmployeeRequest(int employeeId, int timeDeviationCauseId, EmployeeRequest employeeRequest, ref bool sentWithErrors)
        {
            try
            {
                #region Prereq

                sentWithErrors = false;

                TimeDeviationCause deviationCause = GetTimeDeviationCauseFromCache(timeDeviationCauseId);
                if (deviationCause == null)
                    return;

                Employee employee = GetEmployeeWithContactPersonFromCache(employeeId);
                if (employee == null)
                    return;

                User sender = GetUserFromCache();
                if (sender == null)
                    return;

                #endregion

                #region Create subject

                string subject = GetText(8549, "Frånvaroansökan för") + " " + employee.Name;

                #endregion

                #region Create body

                DateTime dateStart = employeeRequest.Start;
                DateTime dateStop = employeeRequest.Stop;

                string date = "";
                if (dateStart == dateStop)
                    date = dateStart.ToShortDateString();
                else
                    date = dateStart.ToShortDateString() + " - " + dateStop.ToShortDateString();

                StringBuilder body = new StringBuilder();
                body.Append(string.Format(GetText(8550, "{0} har ansökt {1} för {2}."), employee.Name, deviationCause.Name, date) + " ");
                if (GetCompanyBoolSettingFromCache(CompanySettingType.AbsenceRequestPlanningIncludeNoteInMessages))
                {
                    body.Append("{0}" + GetText(9222, "Notering") + ":" + "{0}");
                    body.Append(employeeRequest.Comment);
                }

                #endregion

                #region Send to receivers

                // Get all nearest executives for the whole period of the request
                List<UserDTO> receiversRequestPeriod = UserManager.GetEmployeeNearestExecutives(entities, employee, dateStart, dateStop, actorCompanyId);
                // Get all nearest executives for today (in case the request is for a future period and manager role is currently delegated)
                List<UserDTO> receiversToday = UserManager.GetEmployeeNearestExecutives(entities, employee, DateTime.Today.Date, DateTime.Today.Date.AddDays(1), actorCompanyId);
                // Merge both lists and remove duplicates
                List<UserDTO> receivers = receiversRequestPeriod;
                foreach (UserDTO receiver in receiversToday)
                {
                    if (receivers.All(r => r.UserId != receiver.UserId))
                        receivers.Add(receiver);
                }

                foreach (UserDTO receiver in receivers)
                {
                    if (!SendXEMail(receiver, sender, base.RoleId, subject, String.Format(body.ToString(), "<br/>"), String.Format(body.ToString(), "\n"), SoeEntityType.EmployeeRequest, TermGroup_MessageType.AbsenceRequest, employeeRequest.EmployeeRequestId, forceSendToReceiver: true).Success)
                        sentWithErrors = true;
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
                sentWithErrors = true;
            }
        }
        private void SendXEMailOnReActivatedEmployeeRequest(int employeeId, EmployeeRequest employeeRequest)
        {
            try
            {
                if (!employeeRequest.TimeDeviationCauseId.HasValue)
                    return;

                #region Prereq

                TimeDeviationCause deviationCause = GetTimeDeviationCauseFromCache(employeeRequest.TimeDeviationCauseId.Value);
                if (deviationCause == null)
                    return;

                Employee employee = GetEmployeeWithContactPersonFromCache(employeeId);
                if (employee == null)
                    return;

                User sender = GetUserFromCache();
                if (sender == null)
                    return;

                #endregion

                #region Create subject

                string subject = string.Format(GetText(8945, "Frånvaroansökan för {0} har återaktiverats"), employee.Name);

                #endregion

                #region Create body

                DateTime dateStart = employeeRequest.Start;
                DateTime dateStop = employeeRequest.Stop;

                string date = "";
                if (dateStart == dateStop)
                    date = dateStart.ToShortDateString();
                else
                    date = dateStart.ToShortDateString() + " - " + dateStop.ToShortDateString();

                StringBuilder body = new StringBuilder();
                body.Append(string.Format(GetText(8946, "Ansökan {0}, {1} för {2} har återaktiverats."), deviationCause.Name, date, employee.Name) + " ");
                if (GetCompanyBoolSettingFromCache(CompanySettingType.AbsenceRequestPlanningIncludeNoteInMessages))
                {
                    body.Append("{0}" + GetText(9222, "Notering") + ":" + "{0}");
                    body.Append(employeeRequest.Comment);
                }

                #endregion

                #region Send to receivers

                List<UserDTO> receivers = UserManager.GetEmployeeNearestExecutives(entities, employee, dateStart, dateStop, actorCompanyId);
                foreach (UserDTO receiver in receivers)
                {
                    SendXEMail(receiver, sender, base.RoleId, subject, String.Format(body.ToString(), "<br/>"), String.Format(body.ToString(), "\n"), SoeEntityType.EmployeeRequest, TermGroup_MessageType.AbsenceRequest, employeeRequest.EmployeeRequestId);
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }
        private void SendXEMailOnDaysChanged(int employeeId, List<DateTime> dates, TermGroup_TimeScheduleTemplateBlockType type)
        {
            if (dates.IsNullOrEmpty())
                return;

            #region Prereq

            // Do not send XEmail to hidden employee
            if (employeeId == GetHiddenEmployeeIdFromCache())
                return;

            // Check company setting if XEmail should be sent at all
            if (!GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningSendXEMailOnChange))
                return;

            // Check Employee
            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return;

            // Check user setting if XEmail should be sent at all
            if (!employee.UserReference.IsLoaded)
                employee.UserReference.Load();
            if (employee.User == null || SettingManager.GetBoolSetting(entities, SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDontSendXEMailOnChange, employee.UserId.Value, actorCompanyId, 0))
                return;

            // Sender
            User sender = GetUserFromCache();
            if (sender == null)
                return;

            // Receiver
            User reciever = employee.User;

            bool isOrder = (type == TermGroup_TimeScheduleTemplateBlockType.Order);

            #endregion

            #region Create subject

            string subject = String.Format(isOrder ? GetSysTerm(371, (int)TermGroup.XEMailGrid, "Förändrade uppdrag för {0}") : GetSysTerm(372, (int)TermGroup.XEMailGrid, "Förändrat schema för {0}"), GetSysTerm(417, (int)TermGroup.XEMailGrid, "flera dagar").ToLower());

            #endregion

            #region Create body

            StringBuilder body = new StringBuilder();
            body.Append(String.Format(GetSysTerm(418, (int)TermGroup.XEMailGrid, "Följande dagar har förändrade {0}:"), isOrder ? GetSysTerm(485, (int)TermGroup.TimeSchedulePlanning, "uppdrag") : GetSysTerm(481, (int)TermGroup.TimeSchedulePlanning, "pass")));
            body.Append(Environment.NewLine);

            foreach (DateTime date in dates)
            {
                body.Append(date.ToShortDateString());
                body.Append(Environment.NewLine);
            }

            #endregion

            #region Send to receivers

            SendXEMail(reciever, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.EmployeeSchedule, TermGroup_MessageType.ScheduledChanged, 0, employee.User.EmailCopy);

            #endregion
        }
        private void SendXEMailOnDayChanged(int excludeEmployeeId = 0)
        {
            if (this.currentSendXEMailEmployeeDates.IsNullOrEmpty())
                return;

            foreach (var employeeDates in this.currentSendXEMailEmployeeDates.GroupBy(x => x.Item1).ToList())
            {
                if (employeeDates.Key == excludeEmployeeId)
                    continue;

                foreach (var employeeDate in employeeDates)
                {
                    SendXEMailOnDayChanged(employeeDate.Item1, employeeDate.Item2, employeeDate.Item3, employeeDate.Item4, employeeDate.Item5);
                }
            }

            this.currentSendXEMailEmployeeDates.Clear();
        }
        private void SendXEMailOnDayChanged(int employeeId, DateTime date, TermGroup_TimeScheduleTemplateBlockType type, bool wantedShiftsAreAssigned, bool unWantedShiftsAreAssigned)
        {
            #region Prereq

            // Do not send XEmail to hidden employee
            if (employeeId == GetHiddenEmployeeIdFromCache())
                return;

            // Check company setting if XEmail should be sent at all
            if (!GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningSendXEMailOnChange))
                return;

            // Check user setting if XEmail should be sent at all
            Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId, loadContactPerson: true, loadUser: true, getHidden: true);
            if (employee == null || employee.User == null || SettingManager.GetBoolSetting(entities, SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDontSendXEMailOnChange, employee.UserId.Value, actorCompanyId, 0))
                return;

            // Sender
            User sender = GetUserFromCache();
            if (sender == null)
                return;

            // Receiver
            User reciever = employee.User;

            bool isOrder = (type == TermGroup_TimeScheduleTemplateBlockType.Order);

            #endregion

            #region Create subject

            string subject = String.Format(isOrder ? GetSysTerm(371, (int)TermGroup.XEMailGrid, "Förändrade uppdrag för {0}") : GetSysTerm(372, (int)TermGroup.XEMailGrid, "Förändrat schema för {0}"), date.ToShortDateString());

            #endregion

            #region Create body

            List<TermGroup_TimeScheduleTemplateBlockType> blockTypes = new List<TermGroup_TimeScheduleTemplateBlockType> { TermGroup_TimeScheduleTemplateBlockType.Booking };
            if (isOrder)
                blockTypes.Add(TermGroup_TimeScheduleTemplateBlockType.Order);
            else
                blockTypes.Add(TermGroup_TimeScheduleTemplateBlockType.Schedule);

            List<TimeSchedulePlanningDayDTO> dayShifts = TimeScheduleManager.GetTimeScheduleShifts(entities, actorCompanyId, userId, 0, employeeId, date, date, blockTypes, true, false, includePreliminary: false).Where(x => !x.TimeScheduleScenarioHeadId.HasValue).ToList();


            StringBuilder body = new StringBuilder();
            string tempText = "";
            if (wantedShiftsAreAssigned)
            {
                tempText = GetText(10262, "Du har blivit tilldelad ett eller flera önskade {0}, {1}, din dag ser nu ut så här:");
            }
            else if (unWantedShiftsAreAssigned)
            {
                if (dayShifts.Any())
                    tempText = GetText(10264, "Ett eller flera av dina erbjudna pass {0}, {1}, har nu tilldelats en annan medarbetare, din dag ser nu ut så här:");
                else
                    tempText = GetText(10265, "Ett eller flera av dina erbjudna pass {0}, {1}, har nu tilldelats en annan medarbetare.");
            }
            else
            {
                tempText = GetSysTerm(373, (int)TermGroup.XEMailGrid, "Ett eller flera {0} för {1} har förändrats, din dag ser nu ut så här:");
            }

            body.Append(String.Format(tempText, isOrder ? GetSysTerm(485, (int)TermGroup.TimeSchedulePlanning, "uppdrag") : GetSysTerm(481, (int)TermGroup.TimeSchedulePlanning, "pass"), date.ToShortDateString()));
            body.Append(Environment.NewLine);

            if (dayShifts.Any())
            {
                TimeSchedulePlanningDayDTO firstShift = dayShifts.First();
                string breakText = GetSysTerm(188, (int)TermGroup.TimeSchedulePlanning, "Rast");

                #region Get all breaks

                string break1 = firstShift.Break1TimeCodeId != 0 ? String.Format("\n{0}-{1}  {2}", firstShift.Break1StartTime.ToShortTimeString(), firstShift.Break1StartTime.AddMinutes(firstShift.Break1Minutes).ToShortTimeString(), breakText) : String.Empty;
                string break2 = firstShift.Break2TimeCodeId != 0 ? String.Format("\n{0}-{1}  {2}", firstShift.Break2StartTime.ToShortTimeString(), firstShift.Break2StartTime.AddMinutes(firstShift.Break2Minutes).ToShortTimeString(), breakText) : String.Empty;
                string break3 = firstShift.Break3TimeCodeId != 0 ? String.Format("\n{0}-{1}  {2}", firstShift.Break3StartTime.ToShortTimeString(), firstShift.Break3StartTime.AddMinutes(firstShift.Break3Minutes).ToShortTimeString(), breakText) : String.Empty;
                string break4 = firstShift.Break4TimeCodeId != 0 ? String.Format("\n{0}-{1}  {2}", firstShift.Break4StartTime.ToShortTimeString(), firstShift.Break4StartTime.AddMinutes(firstShift.Break4Minutes).ToShortTimeString(), breakText) : String.Empty;

                #endregion

                #region Schedule

                body.Append(String.Format("{0}:", GetSysTerm(152, (int)TermGroup.TimeSchedulePlanning, "Dagens schema")));
                foreach (var dayShift in dayShifts.OrderBy(s => s.StartTime))
                {
                    #region Breaks within day

                    if (!String.IsNullOrEmpty(break1) && CalendarUtility.MergeDateAndTime(dayShift.StartTime, dayShift.Break1StartTime).AddMinutes(dayShift.Break1Minutes) <= dayShift.StartTime)
                    {
                        body.Append(break1);
                        break1 = String.Empty;
                    }
                    if (!String.IsNullOrEmpty(break2) && CalendarUtility.MergeDateAndTime(dayShift.StartTime, dayShift.Break2StartTime).AddMinutes(dayShift.Break2Minutes) <= dayShift.StartTime)
                    {
                        body.Append(break2);
                        break2 = String.Empty;
                    }
                    if (!String.IsNullOrEmpty(break3) && CalendarUtility.MergeDateAndTime(dayShift.StartTime, dayShift.Break3StartTime).AddMinutes(dayShift.Break3Minutes) <= dayShift.StartTime)
                    {
                        body.Append(break3);
                        break3 = String.Empty;
                    }
                    if (!String.IsNullOrEmpty(break4) && CalendarUtility.MergeDateAndTime(dayShift.StartTime, dayShift.Break4StartTime).AddMinutes(dayShift.Break4Minutes) <= dayShift.StartTime)
                    {
                        body.Append(break4);
                        break4 = String.Empty;
                    }

                    #endregion

                    // Time
                    body.Append(String.Format("\n{0}-{1}  ", dayShift.StartTime.ToShortTimeString(), dayShift.StopTime.ToShortTimeString()));

                    // Order number
                    if (dayShift.Order != null)
                        body.Append(String.Format("{0} ", dayShift.Order.OrderNr));

                    // Shift type
                    if (!String.IsNullOrEmpty(dayShift.ShiftTypeName))
                        body.Append(dayShift.ShiftTypeName);
                }

                #endregion

                #region The rest of the breaks

                if (!String.IsNullOrEmpty(break1))
                    body.Append(break1);
                if (!String.IsNullOrEmpty(break2))
                    body.Append(break2);
                if (!String.IsNullOrEmpty(break3))
                    body.Append(break3);
                if (!String.IsNullOrEmpty(break4))
                    body.Append(break4);

                #endregion

                #region Summary

                int minutes = dayShifts.Sum(s => (int)(s.StopTime - s.StartTime).TotalMinutes);
                int breakMinutes = 0;
                if (firstShift.Break1TimeCodeId != 0)
                    breakMinutes += firstShift.Break1Minutes;
                if (firstShift.Break2TimeCodeId != 0)
                    breakMinutes += firstShift.Break2Minutes;
                if (firstShift.Break3TimeCodeId != 0)
                    breakMinutes += firstShift.Break3Minutes;
                if (firstShift.Break4TimeCodeId != 0)
                    breakMinutes += firstShift.Break4Minutes;

                body.Append(String.Format("\n{0}: {1} ({2})", GetSysTerm(124, (int)TermGroup.TimeSchedulePlanning, "Summa"), CalendarUtility.FormatTimeSpan(new TimeSpan(0, minutes - breakMinutes, 0), false, false), breakMinutes));

                #endregion
            }

            #endregion

            #region Send to receivers

            SendXEMail(reciever, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.EmployeeSchedule, TermGroup_MessageType.ScheduledChanged, 0, employee.User?.EmailCopy ?? false);

            #endregion
        }
        private void SendXEMailOnInitiateScheduleSwapByEmployee(TimeScheduleSwapRequest swapRequest)
        {
            if (swapRequest == null || swapRequest.TimeScheduleSwapRequestId == 0)
                return;

            //This method is designed to only handle an initiation from an employee
            if (!swapRequest.InitiatorEmployeeId.HasValue)
                return;

            User sender = GetUserFromCache();
            if (sender == null)
                return;

            Employee initiatorEmployee = EmployeeManager.GetEmployee(entities, swapRequest.InitiatorEmployeeId.Value, actorCompanyId, loadContactPerson: true, loadUser: true, getHidden: false);
            if (initiatorEmployee == null)
                return;

            int? swapWithEmployeeId = swapRequest.TimeScheduleSwapRequestRow.FirstOrDefault(x => x.EmployeeId != initiatorEmployee.EmployeeId)?.EmployeeId;
            if (!swapWithEmployeeId.HasValue)
                return;

            Employee swapWithEmployee = EmployeeManager.GetEmployee(entities, swapWithEmployeeId.Value, actorCompanyId, loadContactPerson: true, loadUser: true, getHidden: false);
            User reciever = swapWithEmployee.User;
            if (reciever == null)
                return;

            string subject = GetText(9987, "Förfrågan om passbyte");

            #region Create body

            StringBuilder body = new StringBuilder();

            body.Append(String.Format(GetText(9988, "{0} önskar att byta bort följande pass:"), initiatorEmployee.Name));
            body.Append(Environment.NewLine);

            foreach (var row in swapRequest.TimeScheduleSwapRequestRow.Where(x => x.EmployeeId == initiatorEmployee.EmployeeId).OrderBy(x => x.Date))
            {
                body.Append(row.Date.ToShortDateString());
                body.Append(", ");
                body.Append(row.ShiftsInfo);
                body.Append(Environment.NewLine);
            }

            body.Append(Environment.NewLine);
            body.Append(Environment.NewLine);
            body.Append(String.Format(GetText(9989, "Dina pass som {0} vill byta mot:"), initiatorEmployee.Name));
            body.Append(Environment.NewLine);

            foreach (var row in swapRequest.TimeScheduleSwapRequestRow.Where(x => x.EmployeeId != initiatorEmployee.EmployeeId).OrderBy(x => x.Date))
            {
                body.Append(row.Date.ToShortDateString());
                body.Append(", ");
                body.Append(row.ShiftsInfo);
                body.Append(Environment.NewLine);
            }

            body.Append(Environment.NewLine);

            if (!swapRequest.Comment.IsNullOrEmpty())
            {
                body.Append(Environment.NewLine);
                body.Append(GetText(1436, "Kommentar") + ": ");
                body.Append(Environment.NewLine);
                body.Append(swapRequest.Comment);
                body.Append(Environment.NewLine);
            }

            #endregion

            #region Send to receivers

            SendXEMail(reciever, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.EmployeeSchedule, TermGroup_MessageType.SwapRequest, swapRequest.TimeScheduleSwapRequestId, reciever.EmailCopy, true);

            #endregion
        }

        private void SendXEMailOnApproveScheduleSwapRequestByEmployee(TimeScheduleSwapRequest swapRequest, bool approved, bool isInitiator)
        {
            try
            {
                #region Prereq

                List<Employee> swapRequestEmployees = new List<Employee>();
                List<UserDTO> receivers = new List<UserDTO>();

                if (swapRequest == null || swapRequest.TimeScheduleSwapRequestId == 0)
                    return;

                if (swapRequest.TimeScheduleSwapRequestRow.IsNullOrEmpty())
                    return;

                User sender = GetUserFromCache();
                if (sender == null)
                    return;

                foreach (int swapRequestEmployeeId in swapRequest.TimeScheduleSwapRequestRow.Select(e => e.EmployeeId))
                {
                    if (!swapRequestEmployees.Any(e => e.EmployeeId == swapRequestEmployeeId))
                    {
                        Employee swapRequestEmployee = EmployeeManager.GetEmployee(entities, swapRequestEmployeeId, actorCompanyId, loadContactPerson: true, loadUser: true, getHidden: false);
                        swapRequestEmployees.Add(swapRequestEmployee);
                    }
                }

                #endregion

                #region Create subject
                string subject = "";
                if (!isInitiator && approved)
                    subject = GetText(9990, "Godkännande av passbyte");
                else if (!isInitiator && !approved)
                    subject = GetText(9993, "Passbyte nekat");
                else if (isInitiator && !approved)
                    subject = GetText(9994, "Passbyte borttaget");

                #endregion

                #region Create body

                StringBuilder body = new StringBuilder();
                string initiatorName = "";
                string targetName = "";
                StringBuilder initiatorShifts = new StringBuilder();
                StringBuilder targetShifts = new StringBuilder();

                // For every unique employee in the swap request
                foreach (Employee employee in swapRequestEmployees)
                {
                    List<DateTime> dateSpan = new List<DateTime>();

                    if (employee.EmployeeId == swapRequest.InitiatorEmployeeId)
                        initiatorName = employee.Name;
                    else
                        targetName = employee.Name;

                    // For every shift swap request row of each employee
                    foreach (TimeScheduleSwapRequestRow row in swapRequest.TimeScheduleSwapRequestRow.Where(e => e.EmployeeId == employee.EmployeeId))
                    {
                        if (employee.EmployeeId == swapRequest.InitiatorEmployeeId)
                        {
                            initiatorShifts.Append(row.Date.ToShortDateString());
                            initiatorShifts.Append(", ");
                            initiatorShifts.Append(row.ShiftsInfo);
                            initiatorShifts.Append(Environment.NewLine);
                        }
                        else
                        {
                            targetShifts.Append(row.Date.ToShortDateString());
                            targetShifts.Append(", ");
                            targetShifts.Append(row.ShiftsInfo);
                            targetShifts.Append(Environment.NewLine);
                        }
                        dateSpan.Add(row.Date);
                    }
                    dateSpan = dateSpan.OrderBy(e => e.Date).ToList();

                    if (!isInitiator && approved)
                    {

                        // Get nearest executive and att to list
                        List<UserDTO> employeeReceivers = UserManager.GetEmployeeNearestExecutives(entities, employee, dateSpan.First(), dateSpan.Last(), actorCompanyId);
                        foreach (UserDTO employeeReceiver in employeeReceivers)
                        {
                            if (!receivers.Any(e => e.UserId == employeeReceiver.UserId))
                                receivers.Add(employeeReceiver);
                        }

                    }
                    else if (!isInitiator && !approved && employee.EmployeeId == swapRequest.InitiatorEmployeeId)
                    {
                        UserDTO initiatorUser = UserManager.GetUserByEmployeeId(entities, employee.EmployeeId, actorCompanyId)?.ToDTO();
                        receivers.Add(initiatorUser);
                    }
                    else if (isInitiator && !approved && employee.EmployeeId != swapRequest.InitiatorEmployeeId)
                    {
                        UserDTO swapwith = UserManager.GetUserByEmployeeId(entities, employee.EmployeeId, actorCompanyId)?.ToDTO();
                        receivers.Add(swapwith);
                    }
                }
                if (!isInitiator && approved)
                {
                    body.Append(string.Format(GetText(9991, "{0} har accepterat passbytesförfrågan från {1}."), targetName, initiatorName));
                }
                else if (!isInitiator && !approved)
                {
                    body.Append(string.Format(GetText(9995, "{0} har nekat passbytesförfrågan."), targetName));
                }
                else if (isInitiator && !approved)
                {
                    body.Append(string.Format(GetText(9996, "{0} har tagit bort passbytesförfrågan."), initiatorName));
                }

                body.Append(Environment.NewLine);
                body.Append(Environment.NewLine);
                body.Append(string.Format(GetText(9992, "{0} vill byta bort följande pass:"), initiatorName));
                body.Append(Environment.NewLine);
                body.Append(initiatorShifts);
                body.Append(Environment.NewLine);
                body.Append(string.Format(GetText(9992, "{0} vill byta bort följande pass:"), targetName));
                body.Append(Environment.NewLine);
                body.Append(targetShifts);

                #endregion

                #region Send to receivers

                // Send email to each executive
                foreach (UserDTO receiver in receivers)
                {
                    SendXEMail(receiver, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.EmployeeSchedule, TermGroup_MessageType.SwapRequest, swapRequest.TimeScheduleSwapRequestId, receiver.EmailCopy, true);
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }
        private void SendXEMailOnApproveScheduleSwapRequestByAdmin(TimeScheduleSwapRequest swapRequest, bool approved, string comment)
        {
            try
            {
                #region Prereq

                List<Employee> swapRequestEmployees = new List<Employee>();
                List<User> receivers = new List<User>();

                if (swapRequest == null || swapRequest.TimeScheduleSwapRequestId == 0)
                    return;

                if (swapRequest.TimeScheduleSwapRequestRow.IsNullOrEmpty())
                    return;

                User sender = GetUserFromCache();
                if (sender == null)
                    return;

                foreach (int swapRequestEmployeeId in swapRequest.TimeScheduleSwapRequestRow.Select(e => e.EmployeeId))
                {
                    if (!swapRequestEmployees.Any(e => e.EmployeeId == swapRequestEmployeeId))
                    {
                        Employee swapRequestEmployee = EmployeeManager.GetEmployee(entities, swapRequestEmployeeId, actorCompanyId, loadContactPerson: true, loadUser: true, getHidden: false);
                        swapRequestEmployees.Add(swapRequestEmployee);
                    }
                }

                #endregion

                #region Create subject
                string subject = "";
                if (approved)
                    subject = GetText(9990, "Godkännande av passbyte");
                else
                    subject = GetText(9993, "Passbyte nekat");

                #endregion

                #region Create body

                StringBuilder body = new StringBuilder();
                string initiatorName = "";
                string targetName = "";
                StringBuilder initiatorShifts = new StringBuilder();
                StringBuilder targetShifts = new StringBuilder();

                // For every unique employee in the swap request
                foreach (Employee employee in swapRequestEmployees)
                {
                    if (employee.EmployeeId == swapRequest.InitiatorEmployeeId)
                        initiatorName = employee.Name;
                    else
                        targetName = employee.Name;

                    // For every shift swap request row of each employee
                    foreach (TimeScheduleSwapRequestRow row in swapRequest.TimeScheduleSwapRequestRow.Where(e => e.EmployeeId == employee.EmployeeId))
                    {
                        if (employee.EmployeeId == swapRequest.InitiatorEmployeeId)
                        {
                            initiatorShifts.Append(row.Date.ToShortDateString());
                            initiatorShifts.Append(", ");
                            initiatorShifts.Append(row.ShiftsInfo);
                            initiatorShifts.Append(Environment.NewLine);
                        }
                        else
                        {
                            targetShifts.Append(row.Date.ToShortDateString());
                            targetShifts.Append(", ");
                            targetShifts.Append(row.ShiftsInfo);
                            targetShifts.Append(Environment.NewLine);
                        }
                    }

                    User user = UserManager.GetUserByEmployeeId(entities, employee.EmployeeId, actorCompanyId);
                    receivers.Add(user);

                }
                if (approved)
                {
                    body.Append(string.Format(GetText(9997, "Passbytet är godkänt"), targetName, initiatorName));
                }
                else if (!approved)
                {
                    body.Append(string.Format(GetText(9993, "Passbyte nekat"), targetName));
                }
                if (!string.IsNullOrEmpty(comment))
                {
                    body.Append(Environment.NewLine);
                    body.Append(Environment.NewLine);
                    body.Append(string.Format(comment));
                }
                body.Append(Environment.NewLine);
                body.Append(Environment.NewLine);
                body.Append(string.Format(GetText(9992, "{0} vill byta bort följande pass:"), initiatorName));
                body.Append(Environment.NewLine);
                body.Append(initiatorShifts);
                body.Append(Environment.NewLine);
                body.Append(string.Format(GetText(9992, "{0} vill byta bort följande pass:"), targetName));
                body.Append(Environment.NewLine);
                body.Append(targetShifts);

                #endregion

                #region Send to receivers

                // Send email to each executive
                foreach (User receiver in receivers)
                {
                    SendXEMail(receiver, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.EmployeeSchedule, TermGroup_MessageType.SwapRequest, swapRequest.TimeScheduleSwapRequestId, receiver.EmailCopy, forceSendToReceiver: true);
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }
        private void SendXEMailOnActivateScenario(int employeeId)
        {
            if (this.currentSendXEMailEmployeeDates.IsNullOrEmpty())
                return;

            //Send
            List<DateTime> dates = this.currentSendXEMailEmployeeDates.Where(x => x.Item1 == employeeId).OrderBy(x => x.Item2).Select(x => x.Item2).ToList();
            this.SendXEMailOnDaysChanged(employeeId, dates, TermGroup_TimeScheduleTemplateBlockType.Schedule);

            this.currentSendXEMailEmployeeDates = this.currentSendXEMailEmployeeDates.Where(x => x.Item1 != employeeId).ToList();
        }
        private void SendXEMailOnDeniedShift(TimeSchedulePlanningDayDTO shift, CompEntities entities)
        {
            EmployeeRequest newReq = null;

            // Check company setting if XEmail should be sent at all
            if (!GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningSendXEMailOnChange))
                return;

            List<TimeScheduleTemplateBlockQueue> shiftQueue = TimeScheduleManager.GetShiftQueue(entities, shift.TimeScheduleTemplateBlockId, TermGroup_TimeScheduleTemplateBlockQueueType.Wanted);
            foreach (var queue in shiftQueue)
            {
                #region Prereq

                // Do not send XEmail to hidden employee
                if (queue.EmployeeId == GetHiddenEmployeeIdFromCache())
                    continue;

                // Do not send to employee that received the shift
                if (queue.EmployeeId == shift.EmployeeId)
                    continue;

                // Check user setting if XEmail should be sent at all
                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(queue.EmployeeId);
                if (employee == null)
                    continue;

                if (!employee.UserReference.IsLoaded)
                    employee.UserReference.Load();
                if (employee.User == null || SettingManager.GetBoolSetting(entities, SettingMainType.User, (int)UserSettingType.TimeSchedulePlanningDontSendXEMailOnChange, employee.UserId.Value, actorCompanyId, 0))
                    continue;

                User sender = GetUserFromCache();
                if (sender == null)
                    return;

                User receiver = employee.User;

                bool employeeRequestCreated = false;
                bool overlappingEmployeeRequest = false;
                if (GetCompanyBoolSettingFromCache(CompanySettingType.CreateEmployeeRequestWhenDeniedWantedShift))
                {
                    TimeDeviationCause deviationCauseEmployee = GetTimeDeviationCauseFromPrio(employee, employee.GetEmployeeGroup(shift.ActualDate, GetEmployeeGroupsFromCache()), null);

                    //Get other requests that day to avoid overlap
                    EmployeeRequest employeeRequest = GetEmployeeRequestByExactTime(shift.EmployeeId, shift.StartTime, shift.StopTime, TermGroup_EmployeeRequestType.InterestRequest, false);
                    if (employeeRequest == null)
                    {
                        newReq = new EmployeeRequest()
                        {
                            ActorCompanyId = actorCompanyId,
                            EmployeeId = employee.EmployeeId,
                            TimeDeviationCauseId = deviationCauseEmployee?.TimeDeviationCauseId,
                            Start = shift.StartTime,
                            Stop = shift.StopTime,
                            Comment = String.Format(GetText(408, (int)TermGroup.XEMailGrid, "Skapat genom ej tilldelat önskat pass på {0}"), shift.ShiftTypeName),
                            Type = (int)TermGroup_EmployeeRequestType.InterestRequest,
                            Status = (int)TermGroup_EmployeeRequestStatus.Definate,
                            ResultStatus = (int)TermGroup_EmployeeRequestResultStatus.None,
                        };
                        employeeRequestCreated = true;
                    }
                    else
                    {
                        overlappingEmployeeRequest = true;
                    }
                }

                bool isOrder = shift?.Order != null;

                #endregion

                #region Create subject

                string subject = String.Format(isOrder ? GetText(399, (int)TermGroup.XEMailGrid, "Annan person tilldelades önskat uppdrag {0}") : GetText(404, (int)TermGroup.XEMailGrid, "Annan anställd tilldelades önskat pass {0}"), shift.StartTime.ToShortDateString());

                #endregion

                #region Create body

                StringBuilder body = new StringBuilder();
                body.Append(String.Format(isOrder ? GetText(400, (int)TermGroup.XEMailGrid, "Annan person tilldelades önskat uppdrag {0}") : GetText(405, (int)TermGroup.XEMailGrid, "Annan anställd tilldelades önskat pass {0}"), shift.StartTime.ToShortDateString()));
                body.Append(Environment.NewLine);

                // Time
                body.Append(String.Format("\n{0}-{1}  ", shift.StartTime.ToShortTimeString(), shift.StopTime.ToShortTimeString()));

                // Order number
                if (isOrder)
                    body.Append(String.Format("{0} - {1} ", shift.Order.OrderNr, shift.Order.CustomerName));

                // Shift type
                if (shift.ShiftTypeId != 0 && String.IsNullOrEmpty(shift.ShiftTypeName))
                {
                    ShiftType shiftType = TimeScheduleManager.GetShiftType(entities, shift.ShiftTypeId);
                    if (shiftType != null)
                        shift.ShiftTypeName = shiftType.Name;
                }
                if (!String.IsNullOrEmpty(shift.ShiftTypeName))
                    body.Append(shift.ShiftTypeName);

                List<TermGroup_TimeScheduleTemplateBlockType> blockTypes = new List<TermGroup_TimeScheduleTemplateBlockType> { TermGroup_TimeScheduleTemplateBlockType.Booking };
                if (isOrder)
                    blockTypes.Add(TermGroup_TimeScheduleTemplateBlockType.Order);
                else
                    blockTypes.Add(TermGroup_TimeScheduleTemplateBlockType.Schedule);

                body.Append(Environment.NewLine);
                body.Append(Environment.NewLine);

                if (employeeRequestCreated)
                {
                    body.Append(GetText(406, (int)TermGroup.XEMailGrid, "Passets tider har nu istället lagts till under din tillgänglighet"));
                    body.Append(Environment.NewLine);
                    body.Append(Environment.NewLine);
                }

                if (overlappingEmployeeRequest)
                {
                    body.Append(GetText(407, (int)TermGroup.XEMailGrid, "Du är fortfarande markerad som tillgänglig under passets tider"));
                    body.Append(Environment.NewLine);
                    body.Append(Environment.NewLine);
                }

                List<TimeSchedulePlanningDayDTO> dayShifts = TimeScheduleManager.GetTimeScheduleShifts(entities, actorCompanyId, userId, 0, queue.EmployeeId, shift.StartTime, shift.StartTime, blockTypes, true, false, includePreliminary: false);
                if (!dayShifts.Any())
                {
                    body.Append(GetText(401, (int)TermGroup.XEMailGrid, "Du har just nu inga pass denna dag"));
                    body.Append(Environment.NewLine);
                    body.Append(Environment.NewLine);
                }
                else
                {
                    body.Append(String.Format(GetText(402, (int)TermGroup.XEMailGrid, "Du har följande pass {0}: "), shift.StartTime.ToShortDateString()));

                    TimeSchedulePlanningDayDTO firstShift = dayShifts.First();
                    string breakText = GetText(188, (int)TermGroup.TimeSchedulePlanning);

                    #region Get all breaks

                    string break1 = firstShift.Break1TimeCodeId != 0 ? String.Format("\n{0}-{1}  {2}", firstShift.Break1StartTime.ToShortTimeString(), firstShift.Break1StartTime.AddMinutes(firstShift.Break1Minutes).ToShortTimeString(), breakText) : String.Empty;
                    string break2 = firstShift.Break2TimeCodeId != 0 ? String.Format("\n{0}-{1}  {2}", firstShift.Break2StartTime.ToShortTimeString(), firstShift.Break2StartTime.AddMinutes(firstShift.Break2Minutes).ToShortTimeString(), breakText) : String.Empty;
                    string break3 = firstShift.Break3TimeCodeId != 0 ? String.Format("\n{0}-{1}  {2}", firstShift.Break3StartTime.ToShortTimeString(), firstShift.Break3StartTime.AddMinutes(firstShift.Break3Minutes).ToShortTimeString(), breakText) : String.Empty;
                    string break4 = firstShift.Break4TimeCodeId != 0 ? String.Format("\n{0}-{1}  {2}", firstShift.Break4StartTime.ToShortTimeString(), firstShift.Break4StartTime.AddMinutes(firstShift.Break4Minutes).ToShortTimeString(), breakText) : String.Empty;

                    #endregion

                    body.Append(Environment.NewLine);
                    foreach (var dayShift in dayShifts.OrderBy(s => s.StartTime))
                    {
                        #region Breaks within day

                        if (!String.IsNullOrEmpty(break1) && CalendarUtility.MergeDateAndTime(dayShift.StartTime, dayShift.Break1StartTime).AddMinutes(dayShift.Break1Minutes) <= dayShift.StartTime)
                        {
                            body.Append(break1);
                            break1 = String.Empty;
                        }
                        if (!String.IsNullOrEmpty(break2) && CalendarUtility.MergeDateAndTime(dayShift.StartTime, dayShift.Break2StartTime).AddMinutes(dayShift.Break2Minutes) <= dayShift.StartTime)
                        {
                            body.Append(break2);
                            break2 = String.Empty;
                        }
                        if (!String.IsNullOrEmpty(break3) && CalendarUtility.MergeDateAndTime(dayShift.StartTime, dayShift.Break3StartTime).AddMinutes(dayShift.Break3Minutes) <= dayShift.StartTime)
                        {
                            body.Append(break3);
                            break3 = String.Empty;
                        }
                        if (!String.IsNullOrEmpty(break4) && CalendarUtility.MergeDateAndTime(dayShift.StartTime, dayShift.Break4StartTime).AddMinutes(dayShift.Break4Minutes) <= dayShift.StartTime)
                        {
                            body.Append(break4);
                            break4 = String.Empty;
                        }

                        #endregion

                        // Time
                        body.Append(String.Format("\n{0}-{1}  ", dayShift.StartTime.ToShortTimeString(), dayShift.StopTime.ToShortTimeString()));

                        // Order number
                        if (dayShift.Order != null)
                            body.Append(String.Format("{0} ", dayShift.Order.OrderNr));

                        // Shift type
                        if (!String.IsNullOrEmpty(dayShift.ShiftTypeName))
                            body.Append(dayShift.ShiftTypeName);
                    }

                    #region The rest of the breaks

                    if (!String.IsNullOrEmpty(break1))
                        body.Append(break1);
                    if (!String.IsNullOrEmpty(break2))
                        body.Append(break2);
                    if (!String.IsNullOrEmpty(break3))
                        body.Append(break3);
                    if (!String.IsNullOrEmpty(break4))
                        body.Append(break4);

                    #endregion

                    #region Summary

                    int minutes = dayShifts.Sum(s => (int)(s.StopTime - s.StartTime).TotalMinutes);
                    int breakMinutes = 0;
                    if (firstShift.Break1TimeCodeId != 0)
                        breakMinutes += firstShift.Break1Minutes;
                    if (firstShift.Break2TimeCodeId != 0)
                        breakMinutes += firstShift.Break2Minutes;
                    if (firstShift.Break3TimeCodeId != 0)
                        breakMinutes += firstShift.Break3Minutes;
                    if (firstShift.Break4TimeCodeId != 0)
                        breakMinutes += firstShift.Break4Minutes;

                    body.Append(String.Format("\n{0}: {1} ({2})", GetSysTerm(124, (int)TermGroup.TimeSchedulePlanning, "Summa"), CalendarUtility.FormatTimeSpan(new TimeSpan(0, minutes - breakMinutes, 0), false, false), breakMinutes));

                    #endregion

                }

                //Check if there are other requests that day
                var reqQueue = TimeScheduleManager.GetShiftQueuesForEmployee(entities, queue.EmployeeId, CalendarUtility.GetBeginningOfDay(shift.StartTime));
                if (!reqQueue.IsNullOrEmpty())
                {
                    StringBuilder queueInfoBody = new StringBuilder();
                    bool includeInfoAboutQueue = false;

                    queueInfoBody.Append(Environment.NewLine);
                    queueInfoBody.Append(GetText(403, (int)TermGroup.XEMailGrid, "Du står fortfarande i kö till följande pass den dagen"));
                    queueInfoBody.Append(Environment.NewLine);

                    foreach (var item in reqQueue)
                    {
                        //exclude the shift that is denied
                        if (item.TimeScheduleTemplateBlockId == queue.TimeScheduleTemplateBlockId)
                            continue;

                        includeInfoAboutQueue = true;
                        TimeSchedulePlanningDayDTO queuedShift = TimeScheduleManager.GetTimeScheduleShift(entities, item.TimeScheduleTemplateBlockId, actorCompanyId, false);
                        if (queuedShift == null)
                            continue;

                        // Time
                        queueInfoBody.Append(String.Format("\n{0}-{1}  ", queuedShift.StartTime.ToShortTimeString(), queuedShift.StopTime.ToShortTimeString()));

                        // Order number
                        if (queuedShift.Order != null)
                            queueInfoBody.Append(String.Format("{0} {1} ", queuedShift.Order.OrderNr, queuedShift.Order.CustomerName));

                        // Shift type
                        if (!String.IsNullOrEmpty(queuedShift.ShiftTypeName))
                            queueInfoBody.Append(queuedShift.ShiftTypeName);

                    }

                    if (includeInfoAboutQueue)
                        body.Append(queueInfoBody.ToString());
                }

                #endregion

                #region Send to recievers

                SendXEMail(receiver, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.EmployeeSchedule, TermGroup_MessageType.AutomaticInformation, shift.TimeScheduleTemplateBlockId, employee.User.EmailCopy);

                //Save employeeRequest
                if (newReq != null)
                    SaveEmployeeRequest(newReq, employee.EmployeeId, TermGroup_EmployeeRequestType.InterestRequest);

                #endregion
            }
        }
        private void SendXEMailOnDeviationsChanged(Employee employee, string datesDescription)
        {
            if (employee == null || string.IsNullOrEmpty(datesDescription))
                return;

            #region Prereq

            // Do not send XEmail to hidden employee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return;

            // Sender
            User sender = GetUserFromCache();
            if (sender == null)
                return;

            // Receiver
            if (!employee.UserReference.IsLoaded)
                employee.UserReference.Load();
            User reciever = employee.User;
            if (reciever == null)
                return;

            #endregion

            #region Create subject

            string subject = GetText(11987, "Förändring av dag");

            #endregion

            #region Create body

            StringBuilder body = new StringBuilder();
            body.Append(string.Format(GetText(11988, "Förändring av dag har skett den {0}"), datesDescription));
            body.Append(Environment.NewLine);

            #endregion

            #region Send to receivers

            SendXEMail(reciever, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.Employee, TermGroup_MessageType.AutomaticInformation, 0, employee.User.EmailCopy);

            #endregion
        }
        private void SendXEMailOnAttestStateChanged(Employee employee, string datesDescription)
        {
            if (employee == null || string.IsNullOrEmpty(datesDescription))
                return;

            #region Prereq

            // Do not send XEmail to hidden employee
            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return;

            // Sender
            User sender = GetUserFromCache();
            if (sender == null)
                return;

            // Receiver
            if (!employee.UserReference.IsLoaded)
                employee.UserReference.Load();
            User reciever = employee.User;
            if (reciever == null)
                return;

            #endregion

            #region Create subject

            string subject = GetText(11989, "Förändring av atteststatus");

            #endregion

            #region Create body

            StringBuilder body = new StringBuilder();
            body.Append(string.Format(GetText(11990, "Förändring av atteststatus har skett den {0}"), datesDescription));
            body.Append(Environment.NewLine);

            #endregion

            #region Send to receivers

            SendXEMail(reciever, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.Employee, TermGroup_MessageType.AutomaticInformation, 0, employee.User.EmailCopy);

            #endregion
        }
        private void SendXEMailOnDayUnlocked(Employee employee, User reciever, string datesDescription)
        {
            if (employee == null || reciever == null || string.IsNullOrEmpty(datesDescription))
                return;

            #region Prereq

            User sender = GetUserFromCache();
            if (sender == null || sender.UserId == reciever.UserId)
                return;

            if (!employee.UserReference.IsLoaded)
                employee.UserReference.Load();

            #endregion

            #region Create subject

            string subject = GetText(11989, "Förändring av atteststatus");

            #endregion

            #region Create body

            StringBuilder body = new StringBuilder();
            body.Append(string.Format(GetText(11990, "Förändring av atteststatus har skett den {0} på anställd {1}"), datesDescription, employee.EmployeeNrAndName));
            body.Append(". ");
            body.Append(string.Format(GetText(91893, "Användare {0} har låst upp dag"), sender.Name));
            body.Append(Environment.NewLine);

            #endregion

            #region Send to receivers

            SendXEMail(reciever, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.Employee, TermGroup_MessageType.AutomaticInformation, 0, employee.User.EmailCopy, forceSendToReceiver: true);

            #endregion
        }
        private void SendXEMailToEmployeeToRemindToAttest(Employee employee, int reminderPeriodType, AttestState reminderAttestState, DateTime dateTo, TimePeriod timePeriod)
        {
            if (employee == null)
                return;

            #region Prereq

            // Sender
            User sender = GetUserFromCache();
            if (sender == null || sender.UserId == employee.UserId)
                return;

            if (!employee.UserReference.IsLoaded)
                employee.UserReference.Load();

            #endregion

            #region Create subject and body

            var (subject, body) = AttestManager.GetAttestReminderMailToEmployee(employee, reminderPeriodType, reminderAttestState, dateTo, timePeriod);

            #endregion

            #region Send to receivers

            SendXEMail(employee.User, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.Employee, TermGroup_MessageType.AttestReminder, 0, employee.User?.EmailCopy ?? false);

            #endregion
        }
        private void SendXEMailToExecutiveToRemindToAttest(User user, List<string> employeeNrAndNames)
        {
            if (user == null)
                return;

            #region Prereq

            // Sender
            User sender = GetUserFromCache();
            if (sender == null || sender.UserId == user.UserId)
                return;

            #endregion

            #region Create subject and body

            var (subject, body) = AttestManager.GetAttestReminderMailToExecutive(user, employeeNrAndNames);

            #endregion

            #region Send to receivers

            SendXEMail(user, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.Employee, TermGroup_MessageType.AttestReminder, 0, user.EmailCopy);

            #endregion
        }

        private bool SendXEMailTimeWorkAccountChoice(Employee employee, DateTime lastDate, int recordId, User sender = null)
        {
            #region Prereq

            // Check Employee
            if (employee == null)
                return false;

            // Check user setting if XEmail should be sent at all
            if (!employee.UserReference.IsLoaded)
                employee.UserReference.Load();
            if (employee.User == null)
                return false;

            // Receiver
            User reciever = employee.User;

            // Sender
            if (sender == null)
                sender = GetUserFromCache();

            #endregion

            #region Create subject

            string subject = string.Format(GetText(91955, "Arbetstidskonto"));

            #endregion

            #region Create body

            StringBuilder body = new StringBuilder();
            body.Append(GetText(12090, "Dags att göra ditt val för arbetstidskontot. Du ska ha gjort valet senast ") + " " + lastDate.ToShortDateString());
            body.Append(Environment.NewLine);

            #endregion

            #region Send to receivers

            if (SendXEMail(reciever, sender, base.RoleId, subject, StringUtility.ConvertNewLineToHtml(body.ToString()), body.ToString(), SoeEntityType.TimeWorkAccountYearEmployee, TermGroup_MessageType.TimeWorkAccountYearEmployeeOption, recordId, employee.User.EmailCopy).Success)
                return true;
            else
                return false;

            #endregion
        }

        #endregion

        #region Messages

        private string GetTimeStampsErrorMessage(Employee employee, TimeBlockDate timeBlockdate)
        {
            return $"Failed to save timestamps. EmployeeId:{employee?.EmployeeId}. EmployeeNr:{employee?.EmployeeNr}. TimeBlockdateId:{timeBlockdate?.TimeBlockDateId}.Date:{timeBlockdate?.Date.ToShortDateString()}";
        }
        private ActionResult GetAttestFailedDuplicateTimeBlocksResult(Employee employee, List<TimeBlock> timeBlocksForDay)
        {
            return new ActionResult((int)ActionResultSave.SaveAttestDuplicateTimeBlocks,
                $"{GetText(10130, "Dagen innehåller dubbla närvarotider. Se över rapporterad tid")}. {employee.EmployeeNrAndName} {timeBlocksForDay?.FirstOrDefault()?.TimeBlockDate?.Date.ToShortDateString() ?? ""}");
        }
        private string GetPayrollProductIsMissingMessage(int? level1, int? level2, int? level3, int? level4)
        {
            string levelsMissingMsg = string.Empty;
            string errorMsg = string.Empty;

            string level1Term = level1.HasValue ? GetText(level1.Value, (int)TermGroup.SysPayrollType) : string.Empty;
            string level2Term = level2.HasValue ? GetText(level2.Value, (int)TermGroup.SysPayrollType) : string.Empty;
            string level3Term = level3.HasValue ? GetText(level3.Value, (int)TermGroup.SysPayrollType) : string.Empty;
            string level4Term = level4.HasValue ? GetText(level4.Value, (int)TermGroup.SysPayrollType) : string.Empty;

            if (!string.IsNullOrEmpty(levelsMissingMsg))
                levelsMissingMsg += ", ";

            levelsMissingMsg += level1Term;
            levelsMissingMsg += !string.IsNullOrEmpty(level2Term) ? "-" + level2Term : string.Empty;
            levelsMissingMsg += !string.IsNullOrEmpty(level3Term) ? "-" + level3Term : string.Empty;
            levelsMissingMsg += !string.IsNullOrEmpty(level4Term) ? "-" + level4Term : string.Empty;

            if (!string.IsNullOrEmpty(levelsMissingMsg))
            {
                errorMsg = GetText(8717, "Löneart med följande typer saknas:") + "\n";
                errorMsg += levelsMissingMsg;
            }

            return errorMsg;
        }

        #endregion
    }
}
