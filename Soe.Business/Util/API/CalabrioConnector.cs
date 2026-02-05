using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.Util.API.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.API
{
    public class CalabrioConnector
    {
        private readonly string _url;

        public CalabrioConnector(string url)
        {
            _url = url;
        }

        public List<TimeScheduleEmployeeIO> GetTimeScheduleEmployeeIOs(string businessUnitId, DateTime startDate, DateTime endDate, List<EmployeeUserDTO> employees, List<TimeDeviationCauseDTO> timeDeviationCauses, List<ShiftTypeDTO> shiftTypes, string scenarioId = null)
        {
            List<TimeScheduleEmployeeIO> ios = new List<TimeScheduleEmployeeIO>();
            var activities = GetActivityResponse(businessUnitId)?.Result;
            var absenses = GetAbsenceResponse(businessUnitId)?.Result;

            if (activities == null || absenses == null)
                return new List<TimeScheduleEmployeeIO>();

            var teamResult = GetTeamsResponse(businessUnitId, startDate, endDate);

            if (teamResult?.Result == null)
                return new List<TimeScheduleEmployeeIO>();

            foreach (var team in teamResult.Result)
            {
                var employeeSchedules = GetScheduleByTeamResponse(businessUnitId, team.Id, startDate, endDate, scenarioId)?.Result;

                if (employeeSchedules.IsNullOrEmpty())
                    continue;

                var employeesByTeam = GetEmployeeByTeamResponse(team.Id, endDate);

                if (employeesByTeam?.Result == null)
                    continue;

                foreach (var scheduleGroup in employeeSchedules.Where(w => w.Shift.Any()).GroupBy(g => g.PersonId))
                {
                    TimeScheduleEmployeeIO employeeIO = new TimeScheduleEmployeeIO();

                    var TOEmployee = employeesByTeam.Result.FirstOrDefault(f => f.Id == scheduleGroup.First().PersonId);

                    var employee = FindEmployee(TOEmployee.EmploymentNumber, employees);

                    if (employee != null)
                    {
                        foreach (var schedule in scheduleGroup)
                        {
                            employeeIO.EmployeeNr = employee.EmployeeNr;
                            employeeIO.EmployeeExternalCode = employee.ExternalCode;

                            foreach (var shift in schedule.Shift)
                            {
                                TimeDeviationCauseDTO cause = FindTimeDeviationCause(shift.AbsenceId, timeDeviationCauses);
                                ShiftTypeDTO shiftType = FindShiftType(shift.ActivityId, shiftTypes);

                                employeeIO.ScheduleInfos.Add(new ScheduleInfo()
                                {
                                    ShiftTypeId = shiftType.ShiftTypeId,
                                    Date = shift.Period.StartTime.Date,
                                    StartTime = shift.Period.StartTime,
                                    StopTime = shift.Period.EndTime,
                                    PlannedAbsence = cause != null,
                                    TimeDeviationCause = cause != null ? cause.ExtCode : string.Empty,
                                });
                            }
                        }
                    }

                    ios.Add(employeeIO);
                }
            }

            return ios;

        }

        private EmployeeUserDTO FindEmployee(string personId, List<EmployeeUserDTO> employees)
        {
            if (employees == null)
                return null;

            if (string.IsNullOrEmpty(personId) || personId == "0")
                return null;

            return employees.FirstOrDefault(f => f.EmployeeNr.Equals(personId, StringComparison.OrdinalIgnoreCase) || f.ExternalCode.Equals(personId, StringComparison.OrdinalIgnoreCase));
        }

        private TimeDeviationCauseDTO FindTimeDeviationCause(string code, List<TimeDeviationCauseDTO> causes)
        {
            if (causes == null)
                return null;

            if (string.IsNullOrEmpty(code) || code == "0")
                return null;

            return causes.FirstOrDefault(f => f.ExtCode.Equals(code, StringComparison.OrdinalIgnoreCase) || f.TimeCodeName.Equals(code, StringComparison.OrdinalIgnoreCase));
        }

        private ShiftTypeDTO FindShiftType(string code, List<ShiftTypeDTO> shiftTypes)
        {
            if (shiftTypes == null)
                return null;

            if (string.IsNullOrEmpty(code) || code == "0")
                return null;

            var type = shiftTypes.FirstOrDefault(f => f.ExternalCode.Equals(code, StringComparison.OrdinalIgnoreCase) || f.Name.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (type == null)
            {
                int id = 0;

                int.TryParse(code, out id);

                if (id != 0)
                    type = shiftTypes.FirstOrDefault(f => f.ExternalId.HasValue && f.ExternalId.Value == id);
            }

            return null;
        }


        private EmployeeByTeamResponse GetEmployeeByTeamResponse(string teamId, DateTime date)
        {
            EmployeeByTeamRequest employeeRequest = new EmployeeByTeamRequest() { TeamId = teamId, Date = date };
            var client = new GoRestClient(_url);
            var request = CreateRequest("query/Person/PeopleByTeamId", Method.Post, employeeRequest);
            var response = client.Execute(request);
            var result = JsonConvert.DeserializeObject<EmployeeByTeamResponse>(response.Content);
            return result;
        }

        private ActivityResponse GetActivityResponse(string businessUnitId)
        {
            ActivityRequest activityRequest = new ActivityRequest() { BusinessUnitId = businessUnitId };
            var client = new GoRestClient(_url);
            var request = CreateRequest("/query/Activity/AllActivities", Method.Post, activityRequest);
            var response = client.Execute(request);
            var result = JsonConvert.DeserializeObject<ActivityResponse>(response.Content);
            return result;
        }

        private AbsenceResponse GetAbsenceResponse(string businessUnitId)
        {
            AbsenseRequest activityRequest = new AbsenseRequest() { BusinessUnitId = businessUnitId };
            var client = new GoRestClient(_url);
            var request = CreateRequest("/query/Absence/AllAbsences", Method.Post, activityRequest);
            var response = client.Execute(request);
            var result = JsonConvert.DeserializeObject<AbsenceResponse>(response.Content);
            return result;
        }

        private TeamResponse GetTeamsResponse(string businessUnitId, DateTime startDate, DateTime endDate)
        {
            TeamRequest teamRequest = new TeamRequest()
            {
                BusinessUnitId = businessUnitId,
                Period = new TeamPeriod()
                {
                    StartDate = startDate,
                    EndDate = endDate
                }
            };

            var client = new GoRestClient(_url);
            var request = CreateRequest("query/Team/AllTeamsWithAgents", Method.Post, teamRequest);
            var response = client.Execute(request);
            var result = JsonConvert.DeserializeObject<TeamResponse>(response.Content);

            return result;
        }

        private ScheduleByTeamResponse GetScheduleByTeamResponse(string businessUnitId, string teamId, DateTime startDate, DateTime endDate, string scenarioId = null)
        {
            ScheduleByTeamRequest scheduleByTeamRequest = new ScheduleByTeamRequest()
            {
                ScenarioId = scenarioId,
                TeamId = teamId,
                BusinessUnitId = businessUnitId,
                Period = new Period()
                {
                    StartDate = startDate,
                    EndDate = endDate
                }
            };
            var client = new GoRestClient(_url);
            var request = CreateRequest("/query/Schedule/ScheduleByTeamId", Method.Post, scheduleByTeamRequest);
            var response = client.Execute(request);
            var result = JsonConvert.DeserializeObject<ScheduleByTeamResponse>(response.Content);

            return result;
        }

        private RestRequest CreateRequest(string resource, RestSharp.Method method, object obj = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            try
            {
                if (obj != null)
                {
                    request.AddJsonBody(obj);
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
            }

            request.AddHeader("Authorization", "Bearer " + "ZjA5NTIzYjEzOGJmNDBlODgxZDIyYzMyYmY3Y2Q0NjJmODEyMzZmYmI0YTQ0ODJmODNhMTA4NmJlZWZmZWQzMA==");

            return request;
        }

        public static void LogError(Exception ex, string message = "")
        {
            string error = message + " " + ex.ToString();
            error = error.ToString().Length > 800 ? error.ToString().Substring(0, 800) : error.ToString();
            SysLogConnector.SaveErrorMessage(error);
        }
    }
}
