using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Business.Tests.Business.Util.StaffingNeedsCalculation.Mock
{
    public class StaffingNeedMockScenarioDTO
    {
        public StaffingNeedMockScenarioDTO(StaffingNeedMockScenario staffingNeedMockScenario)
        {
            StaffingNeedMockScenario = staffingNeedMockScenario;

            switch (StaffingNeedMockScenario)
            {
                case StaffingNeedMockScenario.All:
                    CreateScenarioAll();
                    break;
                case StaffingNeedMockScenario.FourtyHours:
                    break;
                default:
                    break;
            }

            ParameterObject = ParameterObject.Create(company: Company,
                                          user: User,
                                          thread: "GenerateScheduleForEmployeePostsTest");

            FirstCommonNumberOfWeeks = StaffingNeedsCalculationEngine.GetFirstCommonNumberOfWeeks(EmployeePosts);
            DaysForward = FirstCommonNumberOfWeeks * 7;
            SelectedEmployeePosts = EmployeePosts.Where(w => !TimeSchedulePlanningDays.Select(s => s.EmployeePostId).Contains(w.EmployeePostId)).ToList() ;
        }
        public DateTime FromDate
        {
            get { return new DateTime(2020, 3, 23); }
        }

        public int Interval
        {
            get
            {
                return 15;
            }
            set { }
        }
        public int TimeCodeId
        {
            get
            {
                return 1;
            }
            set { }
        }
        public ParameterObject ParameterObject { get; set; }
        public int FirstCommonNumberOfWeeks { get; private set; }
        public int DaysForward { get; private set; }
        public StaffingNeedMockScenario StaffingNeedMockScenario { get; set; }
        public CompanyDTO Company { get; set; }
        public UserDTO User { get; set; }
        public List<EmployeePostDTO> EmployeePosts { get; set; }
        public List<EmployeePostDTO> SelectedEmployeePosts { get; set; }
        public List<IncomingDeliveryHeadDTO> IncomingDeliveryHeads { get; set; }
        public List<OpeningHoursDTO> OpeningHours { get; set; }
        public List<ScheduleCycleRuleDTO> ScheduleCycleRules { get; set; }
        public List<ShiftTypeDTO> ShiftTypes { get; set; }
        public List<StaffingNeedsHeadDTO> StaffingNeedsHeadsFromTasksAndDeliveries { get; set; }
        public List<StaffingNeedsHeadDTO> StaffingNeedsHeadsFromShifts { get; set; }
        public List<TimeBreakTemplateDTO> TimeBreakTemplates { get; set; }
        public List<TimeCodeBreakDTO> TimeCodeBreaks { get; set; }
        public List<TimeCodeBreakGroupDTO> TimeCodeBreakGroups { get; set; }
        public List<TimeSchedulePlanningDayDTO> TimeSchedulePlanningDays { get; set; }
        public List<TimeScheduleTaskDTO> TimeScheduleTasks { get; set; }

        private void CreateScenarioAll()
        {
            User = UserMock.GetUser(StaffingNeedMockScenario);
            Company = CompanyMock.GetCompany(StaffingNeedMockScenario);
            EmployeePosts = EmployeePostMock.GetEmployeePosts(StaffingNeedMockScenario);
            TimeCodeBreaks = TimeCodeBreakMock.GetTimeCodeBreaks(StaffingNeedMockScenario);
            ScheduleCycleRules = ScheduleCycleMock.GetScheduleCycleRules(StaffingNeedMockScenario);
            ShiftTypes = ShiftTypeMock.GetShiftTypes(StaffingNeedMockScenario);
            TimeScheduleTasks = TimeScheduleTaskMock.GetTimeScheduleTasks(StaffingNeedMockScenario);
            OpeningHours = OpeningHoursMock.GetOpeningHours(StaffingNeedMockScenario);
            IncomingDeliveryHeads = IncomingDeliveryHeadMock.GetIncomingDeliveryHeads(StaffingNeedMockScenario);
            TimeSchedulePlanningDays = TimeSchedulePlanningDayMock.GetTimeSchedulePlanningDays(StaffingNeedMockScenario);
            StaffingNeedsHeadsFromTasksAndDeliveries = StaffingNeedsHeadMock.GetStaffingNeedsHeadsFromTasksAndDeliveries(StaffingNeedMockScenario);
            StaffingNeedsHeadsFromShifts = StaffingNeedsHeadMock.GetStaffingNeedsHeadsFromShifts(StaffingNeedMockScenario);
            TimeBreakTemplates = TimeBreakTemplateMock.GetTimeBreakTemplates(StaffingNeedMockScenario);
            TimeCodeBreakGroups = TimeCodeBreakGroupMock.GetTimeCodeBreakGroups(StaffingNeedMockScenario);
        }
    }

    public enum StaffingNeedMockScenario
    {
        All = 0,
        FourtyHours = 1, // 
    }
}
