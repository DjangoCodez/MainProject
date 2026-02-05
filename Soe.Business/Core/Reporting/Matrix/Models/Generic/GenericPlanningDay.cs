using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Generic
{
    public enum PlanningDayCellType
    {
        Time = 0,
        EmployeeInfo = 1,
        EmployeeGroup = 2,
        ContactInformation = 3,
        Absence = 4,
    }

    public class GenericPlanningDay
    {
        GenericPlanningDayInput _genericPlanningDayInput { get; set; }

        Dictionary<int, List<TimeSchedulePlanningDayDTO>> _timeSchedulePlanningDayDict { get; set; }

        List<EmployeeListDTO> _employeeListDTOs { get { return _genericPlanningDayInput.EmployeeListDTOs; } }
        List<ShiftType> _shiftTypes { get { return _genericPlanningDayInput.ShiftTypes; } }
        List<DateTime> _intervalStarts { get { return _genericPlanningDayInput.IntervalStarts; } }
        List<ContactEcomView> _contactEcoms { get { return _genericPlanningDayInput.ContactEcoms; } }
        private int _interval { get; set; }
        private DateTime _startDate { get; set; }
        private DateTime _stopDate { get; set; }

        public GenericPlanningDay(GenericPlanningDayInput genericPlanningDayInput)
        {
            _genericPlanningDayInput = genericPlanningDayInput;
            _interval = _intervalStarts.Count == 1 ? (60 * 24) : Convert.ToInt32((_intervalStarts[1] - _intervalStarts[0]).TotalMinutes);
            _startDate = _intervalStarts.First().Date;
            _stopDate = _intervalStarts.Last().Date;
            _timeSchedulePlanningDayDict = _genericPlanningDayInput.TimeSchedulePlanningDays.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
        }

        public MatrixResult CreateDay()
        {
            int rowNumber = 1;
            List<PlanningDayColumn> columns = AddHeaderColumns();

            foreach (var planningDayGroup in GetPlanningDayGroups())
            {
                rowNumber++;
                planningDayGroup.AddPlanningDayGroupRows(columns, rowNumber);

                foreach (var groupOnEmployee in planningDayGroup.MemberDays.GroupBy(g => g.EmployeeInfo + g.LinkStr))
                {
                    rowNumber++;
                    AddEmployeeInfo(columns, groupOnEmployee.FirstOrDefault().EmployeeInfo, rowNumber);
                    AddEmployeeGroupInfo(columns, groupOnEmployee.First().EmployeeId, rowNumber);
                    AddContactInformation(columns, groupOnEmployee.First().EmployeeId, rowNumber);

                    foreach (var block in groupOnEmployee)
                    {
                        if (!block.ValidIntervalStartTimes.IsNullOrEmpty())
                        {
                            var breaks = block.GetBreaks();

                            foreach (var startInterval in block.ValidIntervalStartTimes)
                            {
                                var column = columns.FirstOrDefault(f => f.Start == startInterval);

                                if (column != null)
                                {
                                    PlanningDayColumnRow row = new PlanningDayColumnRow() { RowNumber = rowNumber };

                                    if (block.Type != TermGroup_TimeScheduleTemplateBlockType.OnDuty && !breaks.IsNullOrEmpty() && breaks.Any(a => CalendarUtility.GetOverlappingMinutes(startInterval, startInterval.AddMinutes(_interval), a.StartTime, a.StopTime) > 1))
                                    {
                                        row.Value = " ";
                                        row.BackgroundColor = GraphicsUtil.Opacitate(block.ShiftTypeColor, new decimal(0.5));
                                        row.FontColor = GraphicsUtil.ForegroundColorByBackgroundBrightness(row.BackgroundColor);
                                    }
                                    else
                                    {
                                        ShiftType shiftType = _shiftTypes.FirstOrDefault(f => f.ShiftTypeId == block.ShiftTypeId);
                                        if (shiftType != null && !shiftType.AccountId.HasValue && !string.IsNullOrEmpty(shiftType?.NeedsCode))
                                            row.Value = shiftType.NeedsCode;
                                        else
                                            row.Value = block.ShiftTypeName;

                                        if (!string.IsNullOrEmpty(block.TimeDeviationCauseName))
                                            row.Value = StringUtility.FillWithZerosBeginning(4, GetTerm(PlanningDayCellType.Absence), true);

                                        row.BackgroundColor = string.IsNullOrEmpty(block.TimeDeviationCauseName) ? block.ShiftTypeColor : "#8B0000";

                                        if (block.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty)
                                        {
                                            row.BackgroundColor = GraphicsUtil.RemoveAlphaValue(GraphicsUtil.Opacitate(block.ShiftTypeColor, new decimal(0.8)));
                                            row.Value = "*" + row.Value;
                                        }

                                        row.FontColor = GraphicsUtil.ForegroundColorByBackgroundBrightness(row.BackgroundColor);
                                    }
                                    column.PlanningDayColumnRows.Add(row);
                                }
                            }
                        }
                    }
                }
            }

            return ConvertToMatrixResult(columns);
        }

        public MatrixResult CreateDays()
        {
            int rowNumber = 1;
            List<PlanningDayColumn> columns = AddHeaderColumns();

            foreach (var planningDayGroup in GetPlanningDayGroups())
            {
                rowNumber++;
                planningDayGroup.AddPlanningDayGroupRows(columns, rowNumber);

                foreach (var groupOnEmployee in planningDayGroup.MemberDays.GroupBy(g => g.EmployeeInfo))
                {
                    rowNumber++;
                    AddEmployeeInfo(columns, groupOnEmployee.Key, rowNumber);
                    AddEmployeeGroupInfo(columns, groupOnEmployee.First().EmployeeId, rowNumber);
                    AddContactInformation(columns, groupOnEmployee.First().EmployeeId, rowNumber);

                    if (columns.Any(a => a.PlanningDayCellType == PlanningDayCellType.EmployeeGroup))
                    {
                        var employeeGroupColumn = columns.FirstOrDefault(f => f.PlanningDayCellType == PlanningDayCellType.EmployeeGroup);
                        var employee = _employeeListDTOs.FirstOrDefault(f => f.EmployeeId == groupOnEmployee.First().EmployeeId);

                        PlanningDayColumnRow employeeGroupRow = new PlanningDayColumnRow()
                        {
                            RowNumber = rowNumber,
                            Value = employee?.GetEmployment(_startDate, _stopDate)?.EmployeeGroupName ?? string.Empty
                        };

                        employeeGroupColumn.PlanningDayColumnRows.Add(employeeGroupRow);
                    }

                    foreach (var date in _intervalStarts)
                    {
                        foreach (var groupOndate in groupOnEmployee.GroupBy(g => g.ActualDate))
                        {
                            var block = groupOndate.First();
                            var onDate = groupOndate.ToList();

                            if (CalendarUtility.GetOverlappingMinutes(date, date.AddMinutes(_interval), block.StartTime, block.StopTime) > 0)
                            {
                                var column = columns.FirstOrDefault(f => f.Start == date);

                                if (column != null)
                                {
                                    PlanningDayColumnRow row = new PlanningDayColumnRow()
                                    {
                                        RowNumber = rowNumber,
                                        Value = string.IsNullOrEmpty(block.TimeDeviationCauseName) ? $"{CalendarUtility.FormatTime(onDate.GetScheduleIn())}-{CalendarUtility.FormatTime(onDate.GetScheduleOut())} {CalendarUtility.FormatMinutes(onDate.GetWorkMinutes(null))}" : GetTerm(PlanningDayCellType.Absence),
                                    };

                                    row.BackgroundColor = string.IsNullOrEmpty(block.TimeDeviationCauseName) ? block.ShiftTypeColor : "#8B0000";
                                    row.FontColor = GraphicsUtil.ForegroundColorByBackgroundBrightness(row.BackgroundColor);
                                    column.PlanningDayColumnRows.Add(row);
                                }
                            }
                        }
                    }
                }
            }

            return ConvertToMatrixResult(columns);
        }

        private List<PlanningDayColumn> AddHeaderColumns()
        {
            DateTime startDate = _intervalStarts.First();
            DateTime stopDate = _intervalStarts.Last();
            List<PlanningDayColumn> columns = new List<PlanningDayColumn>();
            int columnNumber = 1;
            int rowNumber = 1;

            columns.Add(new PlanningDayColumn()
            {
                ColumnNumber = columnNumber,
                PlanningDayCellType = PlanningDayCellType.EmployeeInfo,
                Key = Guid.NewGuid(),
                PlanningDayColumnRows = new List<PlanningDayColumnRow>()
                    { new PlanningDayColumnRow()
                        {
                            RowNumber = rowNumber,
                            MatrixDataType = MatrixDataType.String,
                            Value = GetTerm(PlanningDayCellType.EmployeeInfo),
                            BoldFont = true,
                        }
                    }
            });
            columnNumber++;

            if (_genericPlanningDayInput.HeaderColumnTypes.Any(a => a == PlanningDayCellType.EmployeeGroup))
            {
                columns.Add(new PlanningDayColumn()
                {
                    ColumnNumber = columnNumber,
                    PlanningDayCellType = PlanningDayCellType.EmployeeGroup,
                    Key = Guid.NewGuid(),
                    PlanningDayColumnRows = new List<PlanningDayColumnRow>()
                    { new PlanningDayColumnRow()
                        {
                            RowNumber = 1,
                            MatrixDataType = MatrixDataType.String,
                            Value = GetTerm(PlanningDayCellType.EmployeeGroup),
                            BoldFont = true
                        }
                    }
                });
                columnNumber++;
            }

            if (_genericPlanningDayInput.HeaderColumnTypes.Any(a => a == PlanningDayCellType.ContactInformation))
            {
                columns.Add(new PlanningDayColumn()
                {
                    ColumnNumber = columnNumber,
                    PlanningDayCellType = PlanningDayCellType.ContactInformation,
                    Key = Guid.NewGuid(),
                    PlanningDayColumnRows = new List<PlanningDayColumnRow>()
                    { new PlanningDayColumnRow()
                        {
                            RowNumber = 1,
                            MatrixDataType = MatrixDataType.String,
                            Value = GetTerm(PlanningDayCellType.ContactInformation),
                            BoldFont = true
                        }
                    }
                });
                columnNumber++;
            }

            columnNumber++;
            foreach (var item in _intervalStarts)
            {
                columns.Add(new PlanningDayColumn()
                {
                    ColumnNumber = columnNumber,
                    PlanningDayCellType = PlanningDayCellType.Time,
                    Start = item,
                    Stop = item.AddMinutes(_interval),
                    Key = Guid.NewGuid(),
                    PlanningDayColumnRows = new List<PlanningDayColumnRow>()
                    {
                        new PlanningDayColumnRow()
                        {
                            RowNumber = rowNumber,
                            Value = _interval < 1400 ? CalendarUtility.FormatTime(item) : CalendarUtility.GetShortDayName(item.Date).ToUpper()+CalendarUtility.ToMonthAndDay(item.Date)
                        }
                    }
                });

                columnNumber++;
            }

            return columns;
        }

        private void AddEmployeeInfo(List<PlanningDayColumn> columns, string employeeInfo, int rowNumber)
        {
            if (!columns.Any(a => a.PlanningDayCellType == PlanningDayCellType.EmployeeInfo))
                return;

            var employeeInfoColumn = columns.FirstOrDefault(f => f.PlanningDayCellType == PlanningDayCellType.EmployeeInfo);
            PlanningDayColumnRow startRow = new PlanningDayColumnRow()
            {
                RowNumber = rowNumber,
                Value = employeeInfo
            };

            employeeInfoColumn.PlanningDayColumnRows.Add(startRow);
        }

        private void AddEmployeeGroupInfo(List<PlanningDayColumn> columns, int employeeId, int rowNumber)
        {
            if (!columns.Any(a => a.PlanningDayCellType == PlanningDayCellType.EmployeeGroup))
                return;

            var employeeGroupColumn = columns.FirstOrDefault(f => f.PlanningDayCellType == PlanningDayCellType.EmployeeGroup);
            var employee = _employeeListDTOs.FirstOrDefault(f => f.EmployeeId == employeeId);

            PlanningDayColumnRow employeeGroupRow = new PlanningDayColumnRow()
            {
                RowNumber = rowNumber,
                Value = employee?.GetEmployment(_startDate, _stopDate)?.EmployeeGroupName ?? string.Empty
            };

            employeeGroupColumn.PlanningDayColumnRows.Add(employeeGroupRow);
        }

        private void AddContactInformation(List<PlanningDayColumn> columns, int employeeId, int rowNumber)
        {
            if (!columns.Any(a => a.PlanningDayCellType == PlanningDayCellType.ContactInformation))
                return;

            var contactInformationColumn = columns.FirstOrDefault(f => f.PlanningDayCellType == PlanningDayCellType.ContactInformation);
            var employee = _employeeListDTOs.FirstOrDefault(f => f.EmployeeId == employeeId && !f.Description.IsNullOrEmpty());

            PlanningDayColumnRow employeeGroupRow = new PlanningDayColumnRow()
            {
                RowNumber = rowNumber,
                Value = employee?.Description ?? string.Empty
            };

            contactInformationColumn.PlanningDayColumnRows.Add(employeeGroupRow);
        }

        private MatrixResult ConvertToMatrixResult(List<PlanningDayColumn> columns)
        {
            MatrixResult result = new MatrixResult();
            result.MatrixDefinition = new MatrixDefinition();

            foreach (var column in columns)
            {
                foreach (var row in column.PlanningDayColumnRows.Where(w => w.RowNumber == 1))
                {
                    result.MatrixDefinition.MatrixDefinitionColumns.Add(new MatrixDefinitionColumn()
                    {
                        Field = row.Value,
                        Key = column.Key,
                        MatrixDataType = row.MatrixDataType,
                        Title = row.Value
                    });
                }
            }

            #region Create matrix

            List<MatrixField> fields = new List<MatrixField>();

            foreach (var column in columns)
            {
                foreach (var row in column.PlanningDayColumnRows.Where(w => w.RowNumber != 1))
                {
                    var field = new MatrixField(row.RowNumber - 1, column.Key, row.Value, row.MatrixDataType);
                    if (!string.IsNullOrEmpty(row.BackgroundColor))
                        field.MatrixFieldOptions.Add(new MatrixFieldOption()
                        {
                            MatrixFieldSetting = MatrixFieldSetting.BackgroundColor,
                            StringValue = row.BackgroundColor,
                        });
                    if (!string.IsNullOrEmpty(row.FontColor))
                        field.MatrixFieldOptions.Add(new MatrixFieldOption()
                        {
                            MatrixFieldSetting = MatrixFieldSetting.FontColor,
                            StringValue = row.FontColor,
                        });
                    if (row.BoldFont)
                        field.MatrixFieldOptions.Add(new MatrixFieldOption()
                        {
                            MatrixFieldSetting = MatrixFieldSetting.BoldFont
                        });
                    fields.Add(field);
                }
            }
            result.MatrixFields.AddRange(fields);
            #endregion

            return result;
        }

        private List<PlanningDayGroup> GetPlanningDayGroups()
        {
            List<PlanningDayGroup> planningDayGroups = new List<PlanningDayGroup>();

            _employeeListDTOs.Where(w => w.IsGroupHeader).ToList().ForEach(f => f.GroupName = f.Name);

            if (!_employeeListDTOs.Any(a => a.IsGroupHeader))
            {
                _employeeListDTOs.Insert(0, new EmployeeListDTO()
                {
                    GroupName = " ",
                    IsGroupHeader = true,
                });
            }

            string groupName = string.Empty;
            foreach (var employee in _employeeListDTOs.Where(e => e.IsGroupHeader && !e.GroupName.IsNullOrEmpty()))
            {
                groupName = employee.GroupName.Trim();
                string empGroupName = groupName + "__";
                var group = new PlanningDayGroup(groupName);

                var employeesInGroup = _employeeListDTOs.Where(e => !e.IsGroupHeader && e.GroupName == empGroupName).ToList();

                foreach (var emp in employeesInGroup)
                {
                    if (_timeSchedulePlanningDayDict.TryGetValue(emp.EmployeeId, out var timeSchedulePlanningDaysOnEmployee))
                    {
                        foreach (var dayG in timeSchedulePlanningDaysOnEmployee.GroupBy(g => $"{(emp.Hidden && g.Link.HasValue ? g.Link.Value.ToString() + "#" : "")}{g.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule}#{g.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty}"))
                        {
                            foreach (var day in dayG)
                            {
                                day.LinkStr = dayG.Key;
                                group.MemberDays.Add(day);
                                var breaks = _interval < 400 ? day.GetBreaks() : null;
                                foreach (var intervalstart in _intervalStarts)
                                {
                                    var intervalStop = intervalstart.AddMinutes(_interval);

                                    if (day.StartTime > intervalStop || day.StopTime < intervalstart)
                                        continue;

                                    if (CalendarUtility.GetOverlappingMinutes(intervalstart, intervalStop, day.StartTime, day.StopTime) > 0)
                                    {
                                        if (day.ValidIntervalStartTimes == null)
                                            day.ValidIntervalStartTimes = new List<DateTime>();

                                        if (breaks.IsNullOrEmpty() || !breaks.Any(a => CalendarUtility.GetOverlappingMinutes(intervalstart, intervalStop, a.StartTime, a.StopTime) > 1))
                                            group.ValidIntervalStartTimes.Add(intervalstart);

                                        day.ValidIntervalStartTimes.Add(intervalstart);
                                    }
                                }
                            }
                        }
                    }

                    if (emp.IsGroupHeader && emp.GroupName != groupName)
                        break;
                }

                planningDayGroups.Add(group);
            }

            if (!planningDayGroups.Any())
            {
                var group = new PlanningDayGroup(groupName);
            }

            return planningDayGroups;
        }

        private string GetTerm(PlanningDayCellType type)
        {
            if (!_genericPlanningDayInput.TermsDict.IsNullOrEmpty())
            {
                _genericPlanningDayInput.TermsDict.TryGetValue(type, out string value);

                if (value == null)
                    value = string.Empty;

                return value;
            }

            return string.Empty;
        }

        private class PlanningDayColumn
        {
            public PlanningDayColumn()
            {
                PlanningDayColumnRows = new List<PlanningDayColumnRow>();
            }
            public PlanningDayCellType PlanningDayCellType { get; set; }
            public int ColumnNumber { get; set; }
            public DateTime Start { get; set; }
            public DateTime Stop { get; set; }
            public List<PlanningDayColumnRow> PlanningDayColumnRows { get; set; }
            public Guid Key { get; set; }
        }

        private class PlanningDayGroup
        {
            public PlanningDayGroup(string name)
            {
                MemberDays = new List<TimeSchedulePlanningDayDTO>();
                ValidIntervalStartTimes = new List<DateTime>();
                Name = name;
            }
            public string Name { get; set; }
            public List<TimeSchedulePlanningDayDTO> MemberDays { get; set; }
            public List<DateTime> ValidIntervalStartTimes { get; internal set; }

            public void AddPlanningDayGroupRows(List<PlanningDayColumn> planningDayColumns, int rowNumber)
            {
                foreach (var firstColumn in planningDayColumns.Where(w => w.PlanningDayCellType != PlanningDayCellType.Time))
                {
                    firstColumn.PlanningDayColumnRows.Add(new PlanningDayColumnRow()
                    {
                        RowNumber = rowNumber,
                        Value = Name,
                        BackgroundColor = "#e6e6e6",
                        BoldFont = true
                    });
                }

                List<PlanningDayColumnRow> planningDayColumnRows = new List<PlanningDayColumnRow>();

                foreach (var column in planningDayColumns.Where(w => w.PlanningDayCellType == PlanningDayCellType.Time))
                {
                    DateTime? startInterval = ValidIntervalStartTimes?.FirstOrDefault(f => f == column.Start);

                    if (startInterval.HasValue)
                    {
                        column.PlanningDayColumnRows.Add(new PlanningDayColumnRow()
                        {
                            RowNumber = rowNumber,
                            Value = ValidIntervalStartTimes.Count(c => c == startInterval).ToString(),
                            BackgroundColor = "#e6e6e6",
                            BoldFont = true
                        });
                    }
                    else
                    {
                        column.PlanningDayColumnRows.Add(new PlanningDayColumnRow()
                        {
                            RowNumber = rowNumber,
                            Value = " ",
                            BackgroundColor = "#e6e6e6",
                            BoldFont = true
                        });
                    }
                }
            }
        }

        private class PlanningDayColumnRow
        {
            public PlanningDayColumnRow()
            {
                MatrixDataType = MatrixDataType.String;
            }
            public int RowNumber { get; set; }
            public string Value { get; set; }
            public MatrixDataType MatrixDataType { get; set; }
            public string FontColor { get; set; }
            public string BackgroundColor { get; set; }
            public bool BoldFont { get; set; }
        }
    }

    public class GenericPlanningDayInput
    {
        public List<TimeSchedulePlanningDayDTO> TimeSchedulePlanningDays { get; set; }
        public List<EmployeeListDTO> EmployeeListDTOs { get; set; }
        public List<ShiftType> ShiftTypes { get; set; }
        public List<DateTime> IntervalStarts { get; set; }
        public List<PlanningDayCellType> HeaderColumnTypes { get; set; }
        public Dictionary<PlanningDayCellType, string> TermsDict { get; set; }
        public List<ContactEcomView> ContactEcoms { get; set; }

        public GenericPlanningDayInput(List<TimeSchedulePlanningDayDTO> timeSchedulePlanningDays, List<EmployeeListDTO> employeeListDTOs, List<ShiftType> shiftTypes, List<DateTime> intervalStartTimes, Dictionary<PlanningDayCellType, string> terms)
        {
            TimeSchedulePlanningDays = timeSchedulePlanningDays;
            EmployeeListDTOs = employeeListDTOs;
            ShiftTypes = shiftTypes;
            IntervalStarts = intervalStartTimes;
            TermsDict = terms;
            ContactEcoms = new List<ContactEcomView>();
            HeaderColumnTypes = new List<PlanningDayCellType>();
        }
    }
}
