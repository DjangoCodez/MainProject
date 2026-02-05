using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util
{
    public enum TimeBreakTemplateRule
    {
        //External rules (in database table)
        BreakWindowStart = 1,
        BreakWindowStop = 2,
        MaxShiftLength = 3,
        TimeBetweenBreaks = 4,
        StaffingNeedsCalculation = 5,

        //Internal rules
        LockedTimeSlot = 101,
        Minimize = 102,
        Optimize = 102,
    }

    public class TimeBreakTemplateEvaluation
    {
        #region Variables

        private readonly bool USEREPOSITORY = true;

        private readonly TimeBreakTemplateEvaluationRepository repository = null;

        private int nrOfEvaluations;
        public int NrOfEvaluations
        {
            get
            {
                return this.nrOfEvaluations;
            }
        }
        public int NrOfEvaluationResults
        {
            get
            {
                return repository.GetNrOfResults();
            }
        }

        #endregion

        #region Ctor

        public TimeBreakTemplateEvaluation()
        {
            this.repository = new TimeBreakTemplateEvaluationRepository();
            this.nrOfEvaluations = 0;
        }

        #endregion

        #region Public methods

        public TimeBreakTemplateEvaluationOutput Evaluate(TimeBreakTemplateEvaluationInput input, List<TimeBreakTemplateDTO> templates)
        {
            if (input.Length <= TimeBreakTemplateRules.RULE_MAXSHIFTLENGTH_MINUTES)
                return new TimeBreakTemplateEvaluationOutput(true);

            TimeBreakTemplateEvaluationResult result = USEREPOSITORY ? repository.GetResult(input) : null;
            if (result != null)
                return result.Use();

            this.nrOfEvaluations++;

            if (!ValidateInput(input))
                return new TimeBreakTemplateEvaluationOutput(true); //invalid or zero input, return true to not break calling execution  

            //For net time, break length must be calculated for all templates before validation
            if (input.IsNetTime)
                SetBreakLength(input, templates);

            input.Template = GetTemplate(input, templates, input.IsNetTime);
            if (input.Template == null)
                return new TimeBreakTemplateEvaluationOutput(false);

            if (input.IsNetTime)
                input.AdjustToGrossStopTime();

            //For gross time, break length can be calculated for only the valid one
            if (!input.IsNetTime)
                SetBreakLength(input);

            TimeBreakTemplateEngine engine = new TimeBreakTemplateEngine(input);
            List<TimeBreakTemplateBreakSlot> breakSlots = engine.Render();
            TimeBreakTemplateEvaluationOutput output = new TimeBreakTemplateEvaluationOutput(input, breakSlots);
            repository.AddResult(output);
            return output;
        }

        public TimeBreakTemplateDTO GetTimeBreakTemplate(TimeBreakTemplateEvaluationInput input, List<TimeBreakTemplateDTO> templates, bool isNetTime)
        {
            return GetTemplate(input, templates, isNetTime);
        }

        public TimeBreakTemplateBreakSlot ReEvaluateStartBoundary(TimeBreakTemplateEvaluationOutput output, List<TimeBreakTemplateBreakSlot> breakSlots, TimeBreakTemplateBreakSlot breakSlot)
        {
            if (!output.Success || breakSlots == null || breakSlots.Count == 0 || breakSlot == null || !breakSlots.Any(i => i.Guid == breakSlot.Guid))
                return breakSlot;

            TimeBreakTemplateEngine engine = new TimeBreakTemplateEngine(output.Input);
            return engine.ReEvaluateStartBoundary(breakSlots, breakSlot);
        }

        #endregion

        #region Help-methods

        private bool ValidateInput(TimeBreakTemplateEvaluationInput input)
        {
            if (input == null || input.Length <= 0)
                return false;
            return true;
        }

        private TimeBreakTemplateDTO GetTemplate(TimeBreakTemplateEvaluationInput input, List<TimeBreakTemplateDTO> templates, bool isNetTime)
        {
            if (input.DebugParameters != null)
                return GetTemplate(input.DebugParameters);

            TimeBreakTemplateDTO validTemplate = null;

            if (templates != null && templates.Count > 0)
            {
                List<TimeBreakTemplateDTO> validTemplates = new List<TimeBreakTemplateDTO>();
                validTemplates.AddRange(templates.Where(i => i.ShiftStartFromTime.HasValue));

                //General checks
                validTemplates = validTemplates.Where(i => i.ShiftLength > TimeBreakTemplateRules.RULE_MAXSHIFTLENGTH_MINUTES).ToList();
                validTemplates = validTemplates.Where(i => !i.UseMaxWorkTimeBetweenBreaks || i.ShiftLength >= TimeBreakTemplateRules.RULE_MAXSHIFTLENGTH_MINUTES).ToList();
                validTemplates = validTemplates.Where(i => !i.UseMaxWorkTimeBetweenBreaks || i.MinTimeBetweenBreaks < TimeBreakTemplateRules.RULE_MAXSHIFTLENGTH_MINUTES).ToList();

                //ShiftLength
                if (validTemplates.Count > 0)
                {
                    if (isNetTime)
                        validTemplates = validTemplates.Where(i => i.ShiftLengthNet <= input.Length).ToList();
                    else
                        validTemplates = validTemplates.Where(i => i.ShiftLength <= input.Length).ToList();
                }

                //Date
                if (validTemplates.Count > 0 && input.HasDate)
                    validTemplates = validTemplates.Where(i => CalendarUtility.IsDateInRange(input.Date.Value, i.StartDate, i.StopDate)).ToList();

                if (input.EvaluationType != SoeTimeBreakTemplateEvaluation.RegisterEvaluation)
                {
                    //StartTime
                    if (validTemplates.Count > 0)
                        validTemplates = validTemplates.Where(i => input.StartTime >= i.ShiftStartFromTime.Value).ToList();
                    //Weekdays
                    if (validTemplates.Count > 0 && input.HasDayOfWeek)
                        validTemplates = validTemplates.Where(i => i.DayOfWeeks == null || i.DayOfWeeks.Count == 0 || i.DayOfWeeks.Contains(input.DayOfWeek.Value)).ToList();
                    else
                        validTemplates = validTemplates.Where(i => i.DayOfWeeks == null || i.DayOfWeeks.Count == 0).ToList();
                    //DayType
                    if (validTemplates.Count > 0 && input.HasDayType)
                        validTemplates = validTemplates.Where(i => i.DayTypeIds == null || i.DayTypeIds.Count == 0 || i.DayTypeIds.Contains(input.DayTypeId.Value)).ToList();
                    else
                        validTemplates = validTemplates.Where(i => i.DayTypeIds == null || i.DayTypeIds.Count == 0).ToList();
                    //ShiftType
                    if (validTemplates.Count > 0 && input.HasShiftTypeIds)
                    {
                        if (input.ShiftTypeIds.Count == 1)
                            validTemplates = validTemplates.Where(i => i.ShiftTypeIds == null || i.ShiftTypeIds.Count == 0 || i.ShiftTypeIds.Contains(input.ShiftTypeIds.First())).ToList();
                        else
                            validTemplates = validTemplates.Where(i => i.ShiftTypeIds == null || i.ShiftTypeIds.Count == 0 || i.ShiftTypeIds.ContainsAll(input.ShiftTypeIds)).ToList();
                    }
                    else
                        validTemplates = validTemplates.Where(i => i.ShiftTypeIds == null || i.ShiftTypeIds.Count == 0).ToList();
                }

                if (validTemplates.Count > 0)
                    validTemplate = validTemplates.OrderByDescending(i => i.ShiftLength).ThenByDescending(i => i.ShiftStartFromTime).ThenByDescending(i => i.NrOfShiftTypeIds).FirstOrDefault();
            }

            return validTemplate;
        }

        private TimeBreakTemplateDTO GetTemplate(TimeBreakTemplateDebugParameters debugParameters)
        {
            TimeBreakTemplateDTO template = new TimeBreakTemplateDTO
            {
                UseMaxWorkTimeBetweenBreaks = debugParameters.UseMaxWorkTimeBetweenBreaks,
                MinTimeBetweenBreaks = debugParameters.MinTimeBetweenBreaks,
                TimeBreakTemplateRows = new List<TimeBreakTemplateRowDTO>(),
            };

            for (int breakNr = 1; breakNr <= debugParameters.NrOfBreaksMajor; breakNr++)
            {
                template.TimeBreakTemplateRows.Add(new TimeBreakTemplateRowDTO()
                {
                    Type = SoeTimeBreakTemplateType.Major,
                    MinTimeAfterStart = debugParameters.MinTimeAfterStartMajor,
                    MinTimeBeforeEnd = debugParameters.MinTimeBeforeEndMajor,
                    Length = debugParameters.LengthMajor,
                });
            }

            for (int breakNr = 1; breakNr <= debugParameters.NrOfBreaksMinor; breakNr++)
            {
                template.TimeBreakTemplateRows.Add(new TimeBreakTemplateRowDTO()
                {
                    Type = SoeTimeBreakTemplateType.Minor,
                    MinTimeAfterStart = debugParameters.MinTimeAfterStartMinor,
                    MinTimeBeforeEnd = debugParameters.MinTimeBeforeEndMinor,
                    Length = debugParameters.LengthMinor,
                });
            }

            return template;
        }

        private void SetBreakLength(TimeBreakTemplateEvaluationInput input)
        {
            if (input == null)
                return;

            SetBreakLength(input, new List<TimeBreakTemplateDTO>() { input.Template });
        }

        private void SetBreakLength(TimeBreakTemplateEvaluationInput input, List<TimeBreakTemplateDTO> templates)
        {
            if (input == null || templates == null)
                return;
            CompEntities entities = null; //Because of test

            foreach (TimeBreakTemplateDTO template in templates)
            {
                foreach (TimeBreakTemplateRowDTO templateRow in template.TimeBreakTemplateRows.Where(i => i.State == (int)SoeEntityState.Active && i.TimeCodeBreakGroupId.HasValue))
                {
                    TimeCodeBreakGroupDTO breakGroup = input.TimeCodeBreakGroups.FirstOrDefault(i => i.TimeCodeBreakGroupId == templateRow.TimeCodeBreakGroupId);
                    if (breakGroup == null)
                    {
                        entities = entities ?? new CompEntities();
                        breakGroup = entities.TimeCodeBreakGroup.Include("TimeCodeBreak").FirstOrDefault(i => i.TimeCodeBreakGroupId == templateRow.TimeCodeBreakGroupId).ToDTO();
                    }
                    if (breakGroup == null)
                        continue;

                    //Assumes all breaks in group has same length
                    if (!breakGroup.TimeCodeBreaks.IsNullOrEmpty())
                        templateRow.Length = breakGroup.TimeCodeBreaks.First().DefaultMinutes;

                    if (!input.TimeCodeBreakGroups.Any(i => i.TimeCodeBreakGroupId == breakGroup.TimeCodeBreakGroupId))
                        input.TimeCodeBreakGroups.Add(breakGroup);
                }
            }
            if (entities != null)
                entities.Dispose();
        }

        #endregion
    }

    public class TimeBreakTemplateEngine
    {
        #region Variables

        //Mandatory
        public TimeBreakTemplateEvaluationInput Input { get; }

        //Calculated
        private List<TimeBreakTemplateBreakSlot> breakSlots;
        private int NrOfBreaks { get { return this.breakSlots.Count; } }
        public bool HasLockedTimeSlots { get { return this.Input != null && this.Input.LockedTimeSlots != null && this.Input.LockedTimeSlots.Count > 0; } }

        #endregion

        #region Ctor

        public TimeBreakTemplateEngine(TimeBreakTemplateEvaluationInput input)
        {
            //Mandatory
            this.Input = input;

            //Calculated
            this.breakSlots = new List<TimeBreakTemplateBreakSlot>();
        }

        #endregion

        #region Public

        public List<TimeBreakTemplateBreakSlot> Render()
        {
            if (!RenderBreaks())
                return null;
            if (!ReRenderUnhandledBreaks())
                return null;
            if (!OptimizeBreaks())
                return null;
            if (!ValidateBreaks())
                return null;

            return this.GetBreakSlots();
        }

        private List<TimeBreakTemplateBreakSlot> GetBreakSlots()
        {
            return this.breakSlots.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        public TimeBreakTemplateBreakSlot ReEvaluateStartBoundary(List<TimeBreakTemplateBreakSlot> breakSlots, TimeBreakTemplateBreakSlot breakSlot)
        {
            if (breakSlots == null || breakSlots.Count == 0 || breakSlot == null)
                return breakSlot;

            TimeBreakTemplateRowDTO templateRow = this.Input.Template.GetTemplateRow(breakSlot.TemplateRowGuid);
            if (templateRow == null)
                return breakSlot;

            this.breakSlots = breakSlots;

            if (!breakSlot.BoundaryStartTime.HasValue)
                breakSlot.BoundaryStartTime = breakSlot.StartTime;
            if (!breakSlot.BoundaryStopTime.HasValue)
                breakSlot.BoundaryStopTime = breakSlot.StopTime;

            TimeBreakTemplateTimeSlot prevTimeSlot = GetPrevTimeSlot(breakSlot);
            TimeBreakTemplateBreakSlot prevBreakSlot = GetPrevBreakSlot(breakSlot);

            DateTime boundaryStartTime = GetBoundaryStart(templateRow, prevTimeSlot);
            if (prevBreakSlot != null)
            {
                DateTime minStartBoundary = prevBreakSlot.StopTime.AddMinutes(this.Input.Template.MinTimeBetweenBreaks);
                if (boundaryStartTime < minStartBoundary)
                    boundaryStartTime = minStartBoundary;
            }

            if (breakSlot.BoundaryStartTime != boundaryStartTime)
                breakSlot.BoundaryStartTime = boundaryStartTime;

            return breakSlot;
        }

        #endregion

        #region Help-methods

        #region Main steps

        private bool RenderBreaks()
        {
            List<TimeBreakTemplateRowDTO> templateRowsMajor = this.Input.Template.GetTemplateRows(SoeTimeBreakTemplateType.Major);
            List<TimeBreakTemplateRowDTO> templateRowsMinor = this.Input.Template.GetTemplateRows(SoeTimeBreakTemplateType.Minor);

            List<TimeBreakTemplateTimeSlot> timeSlotsForDay = null;
            if (templateRowsMajor.Count == templateRowsMinor.Count)
                timeSlotsForDay = GenerateTimeSlots(this.Input.StartTime, this.Input.StopTime, nrOfBreaks: templateRowsMajor.Count);

            List<TimeBreakTemplateTimeSlot> timeSlotsMajor = null;
            if (timeSlotsForDay != null)
                timeSlotsMajor = timeSlotsForDay;
            else if (templateRowsMajor.Count == 1)
                timeSlotsMajor = GetTimeSlotsFromBreaks();
            else if (templateRowsMajor.Count > 1)
                timeSlotsMajor = GenerateTimeSlots(this.Input.StartTime, this.Input.StopTime, nrOfBreaks: templateRowsMajor.Count);

            if (!TryCreateBreaks(templateRowsMajor, timeSlotsMajor, placeInMiddle: true))
                return false;

            List<TimeBreakTemplateTimeSlot> timeSlotsMinor;
            if (timeSlotsForDay != null)
                timeSlotsMinor = timeSlotsForDay;
            else if (templateRowsMajor.Count == 0 && templateRowsMinor.Count > 1)
                timeSlotsMinor = GenerateTimeSlots(this.Input.StartTime, this.Input.StopTime, nrOfBreaks: templateRowsMinor.Count);
            else
                timeSlotsMinor = GetTimeSlotsFromBreaks();

            if (!TryCreateBreaks(templateRowsMinor, timeSlotsMinor, placeInMiddle: (timeSlotsForDay != null || timeSlotsMinor.Count >= templateRowsMinor.Count)))
                return false;

            return true;
        }

        private bool ReRenderUnhandledBreaks()
        {
            if (this.NrOfBreaks > 0)
            {
                List<TimeBreakTemplateRowDTO> unhandledTemplateRows = GetUnhandledTemplateRowMinors();
                if (unhandledTemplateRows.Count > 0)
                {
                    if (!MinimizeBreaks())
                        return false;

                    List<TimeBreakTemplateTimeSlot> timeSlots = GetTimeSlotsFromBreaks();
                    foreach (TimeBreakTemplateTimeSlot timeSlot in timeSlots)
                    {
                        TryCreateBreaks(unhandledTemplateRows, new List<TimeBreakTemplateTimeSlot> { timeSlot }, placeInMiddle: false);
                    }

                    unhandledTemplateRows = GetUnhandledTemplateRowMinors();
                    if (unhandledTemplateRows.Count > 0)
                        return false;
                }
            }
            return true;
        }

        private bool MinimizeBreaks()
        {
            if (this.NrOfBreaks > 0)
            {
                for (int breakNr = 1; breakNr <= this.NrOfBreaks; breakNr++)
                {
                    MinimizeBreak(this.GetBreakSlots()[breakNr - 1]);
                }
            }
            return true;
        }

        private bool OptimizeBreaks()
        {
            if (this.NrOfBreaks > 0)
            {
                for (int breakNr = this.NrOfBreaks; breakNr > 0; breakNr--)
                {
                    OptimizeBreak(this.GetBreakSlots()[breakNr - 1]);
                }
            }
            return true;
        }

        private bool ValidateBreaks()
        {
            List<TimeBreakTemplateRowDTO> unhandledTemplateRows = GetUnhandledTemplateRowMinors();
            if (unhandledTemplateRows.Count > 0)
                return false;

            if (this.Input.Template.UseMaxWorkTimeBetweenBreaks)
            {
                List<TimeBreakTemplateTimeSlot> timeSlots = GetTimeSlotsFromBreaks();
                if (timeSlots.IsNullOrEmpty() || timeSlots.Any(i => i.Length > TimeBreakTemplateRules.RULE_MAXSHIFTLENGTH_MINUTES))
                    return false;
            }

            int breakNr = 1;
            foreach (TimeBreakTemplateBreakSlot breakSlot in this.GetBreakSlots())
            {
                foreach (TimeBreakTemplateBreakSlot otherBreakSlot in this.GetBreakSlots().Where(i => i.Guid != breakSlot.Guid))
                {
                    if (CalendarUtility.IsTimesOverlappingNew(breakSlot.StartTime, breakSlot.StopTime, otherBreakSlot.StartTime, otherBreakSlot.StopTime))
                        return false;
                }

                if (!EvalRuleLockedTimeSlotViolationMinutes(breakSlot, out int minutes))
                    return false;
                if (!EvalRuleBreakWindowStartViolationMinutes(breakSlot, out minutes))
                    return false;
                if (!EvalRuleBetweenBreaksViolationMinutes(breakSlot, out minutes))
                    return false;
                if (!EvalRuleBreakWindowStopViolationMinutes(breakSlot, out minutes))
                    return false;
                if (!EvalRuleShiftLengthViolationMinutes(breakSlot, out minutes))
                    return false;
                if (breakNr == this.NrOfBreaks && !EvalRuleShiftLengthViolationMinutes(breakSlot, out minutes, prev: false))
                    return false;

                breakNr++;
            }

            SetBreakBoundaries();

            return true;
        }

        #endregion

        #region Break tweaking

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateRows"></param>
        /// <param name="timeSlots"></param>
        /// <param name="placeInMiddle"></param>
        /// <param name="applyShiftLengthRule"></param>
        /// <returns></returns>
        private bool TryCreateBreaks(List<TimeBreakTemplateRowDTO> templateRows, List<TimeBreakTemplateTimeSlot> timeSlots, bool placeInMiddle)
        {
            if (!templateRows.IsNullOrEmpty() && !timeSlots.IsNullOrEmpty())
            {
                int breakNr = 1;
                foreach (TimeBreakTemplateRowDTO templateRow in templateRows)
                {
                    int timeSlotNr = breakNr > timeSlots.Count ? 1 : breakNr;

                    TimeBreakTemplateBreakSlot breakSlot = CreateBreak(templateRow, timeSlots[timeSlotNr - 1], placeInMiddle);
                    if (breakSlot != null)
                    {
                        breakSlot.SetTemplateRowGuid(templateRow.Guid);
                        this.breakSlots.Add(breakSlot);

                        TimeBreakTemplateBreakSlot nextBreakSlot = GetNextBreakSlot(breakSlot);
                        if (nextBreakSlot != null && !TryAdjustBreakAccordingToRules(nextBreakSlot))
                            return false;
                    }
                    else if (templateRow.Type == SoeTimeBreakTemplateType.Major)
                        return false;

                    breakNr++;
                }
            }
            return true;
        }

        private bool TryAdjustBreakAccordingToRules(TimeBreakTemplateBreakSlot breakSlot)
        {
            #region Forward

            //Break window
            if (!EvalRuleBreakWindowStartViolationMinutes(breakSlot, out int ruleBreakWindowStartViolationMinutes) && !breakSlot.TryMoveForward(ruleBreakWindowStartViolationMinutes, TimeBreakTemplateRule.BreakWindowStart))
                return false;

            //Time between breaks
            if (!EvalRuleBetweenBreaksViolationMinutes(breakSlot, out int ruleBetweenBreaksViolationMinutes) && !breakSlot.TryMoveForward(ruleBetweenBreaksViolationMinutes, TimeBreakTemplateRule.TimeBetweenBreaks))
                return false;

            //Move break so it doesn violate locked slots
            if (!EvalRuleLockedTimeSlotViolationMinutes(breakSlot, out int ruleLockedTimeSlotViolationMinutes))
                return false;

            if (ruleLockedTimeSlotViolationMinutes > 0 && !breakSlot.TryMoveForward(ruleLockedTimeSlotViolationMinutes, TimeBreakTemplateRule.LockedTimeSlot))
                return false;
            if (ruleLockedTimeSlotViolationMinutes < 0 && !breakSlot.TryMoveBackward(-ruleLockedTimeSlotViolationMinutes, TimeBreakTemplateRule.LockedTimeSlot))
                return false;

            #endregion

            #region Backward

            //Break window
            if (!EvalRuleBreakWindowStopViolationMinutes(breakSlot, out int ruleBreakWindowStopViolationMinutes) && !breakSlot.TryMoveBackward(ruleBreakWindowStopViolationMinutes, TimeBreakTemplateRule.BreakWindowStop))
                return false;

            #endregion

            return true;
        }

        private void MinimizeBreak(TimeBreakTemplateBreakSlot breakSlot)
        {
            TimeBreakTemplateTimeSlot timeSlot = GetPrevTimeSlot(breakSlot);
            if (timeSlot != null)
            {
                TimeBreakTemplateRowDTO templateRow = this.Input.Template.GetTemplateRow(breakSlot.TemplateRowGuid);
                if (templateRow != null)
                {
                    breakSlot.SetBoundary(GetBoundaryStart(templateRow, timeSlot), GetBoundaryStop(templateRow, breakSlot));

                    DateTime? newStartTime = null;
                    DateTime tryStartTime = breakSlot.BoundaryStartTime.Value;
                    int initialIncrement = 10; // Start with 10-minute increments
                    int minuteIncrement = initialIncrement;

                    while (!newStartTime.HasValue && tryStartTime < breakSlot.StartTime)
                    {
                        var tryBreakSlot = new TimeBreakTemplateBreakSlot(tryStartTime, breakSlot);
                        if (TryAdjustBreakAccordingToRules(tryBreakSlot))
                        {
                            if (minuteIncrement == initialIncrement && breakSlot.BoundaryStartTime.Value != tryStartTime)
                            {
                                tryStartTime = tryBreakSlot.StartTime.AddMinutes(-initialIncrement);
                                minuteIncrement = 1; // Switch to 1-minute increments if newStartTime has a value
                                continue;
                            }
                            newStartTime = tryBreakSlot.StartTime;
                        }
                        else
                        {
                            tryStartTime = tryStartTime.AddMinutes(minuteIncrement);
                        }
                    }

                    if (newStartTime.HasValue)
                        breakSlot.TryMoveBackward(newStartTime.Value, TimeBreakTemplateRule.Minimize);

                    breakSlot.SetStartBoundary(breakSlot.StartTime);
                }
            }
        }

        private void OptimizeBreak(TimeBreakTemplateBreakSlot breakSlot)
        {
            TimeBreakTemplateRowDTO templateRow = this.Input.Template.GetTemplateRow(breakSlot.TemplateRowGuid);
            if (templateRow != null)
            {
                TimeBreakTemplateTimeSlot prevTimeSlot = GetPrevTimeSlot(breakSlot);
                TimeBreakTemplateTimeSlot nextTimeSlot = GetNextTimeSlot(breakSlot);

                SetBreakBoundaries(breakSlot, templateRow, setStartBoundary: false);

                if (breakSlot.BoundaryStopTime.HasValue && breakSlot.StopTime < breakSlot.BoundaryStopTime.Value)
                {
                    GetBreakInMiddle(breakSlot.Length, prevTimeSlot.StartTime, nextTimeSlot.StopTime, out DateTime tryStartTime, out DateTime tryStopTime);

                    if (tryStopTime > breakSlot.BoundaryStopTime)
                    {
                        tryStopTime = breakSlot.BoundaryStopTime.Value;
                        tryStartTime = tryStopTime.AddMinutes(-breakSlot.Length);
                    }

                    DateTime? newStopTime = null;
                    while (!newStopTime.HasValue && tryStopTime > breakSlot.StopTime)
                    {
                        TimeBreakTemplateBreakSlot tryBreakSlot = new TimeBreakTemplateBreakSlot(tryStartTime, tryStopTime, breakSlot);
                        if (TryAdjustBreakAccordingToRules(tryBreakSlot))
                        {
                            newStopTime = tryBreakSlot.StopTime;
                        }
                        else
                        {
                            tryStartTime = tryStartTime.AddMinutes(-1);
                            tryStopTime = tryStopTime.AddMinutes(-1);
                        }
                    }

                    if (newStopTime.HasValue)
                        breakSlot.TryMoveForward(newStopTime.Value, TimeBreakTemplateRule.Optimize);
                }
            }
        }

        private void SetBreakBoundaries()
        {
            foreach (TimeBreakTemplateBreakSlot breakSlot in this.GetBreakSlots())
            {
                TimeBreakTemplateRowDTO templateRow = this.Input.Template.GetTemplateRow(breakSlot.TemplateRowGuid);
                SetBreakBoundaries(breakSlot, templateRow);
            }
        }

        private void SetBreakBoundaries(TimeBreakTemplateBreakSlot breakSlot, TimeBreakTemplateRowDTO templateRow, bool setStartBoundary = true, bool setStopBoundary = true)
        {
            if (breakSlot == null || templateRow == null)
                return;

            if (setStartBoundary)
            {
                TimeBreakTemplateTimeSlot prevTimeSlot = GetPrevTimeSlot(breakSlot);
                TimeBreakTemplateBreakSlot prevBreakSlot = GetPrevBreakSlot(breakSlot);
                breakSlot.SetStartBoundary(CalendarUtility.GetLatestDate(GetBreakWindowStart(templateRow), prevTimeSlot.StartTime, prevBreakSlot?.BoundaryStopTime));

                if (!EvalRuleBetweenBreaksViolationMinutes(breakSlot, out int ruleBetweenBreaksViolationMinutes, prev: true, useBoundary: true))
                    breakSlot.MoveStartBoundary(ruleBetweenBreaksViolationMinutes);
            }
            if (setStopBoundary)
            {
                TimeBreakTemplateTimeSlot nextTimeSlot = GetNextTimeSlot(breakSlot);
                breakSlot.SetStopBoundary(CalendarUtility.GetEarliestDate(GetBreakWindowStop(templateRow), nextTimeSlot.StopTime));

                if (!EvalRuleBetweenBreaksViolationMinutes(breakSlot, out int ruleBetweenBreaksViolationMinutes, prev: false, useBoundary: true))
                    breakSlot.MoveStopBoundary(-ruleBetweenBreaksViolationMinutes);
            }
        }

        private TimeBreakTemplateBreakSlot CreateBreak(TimeBreakTemplateRowDTO templateRow, TimeBreakTemplateTimeSlot timeSlot, bool placeInMiddle)
        {
            DateTime breakStartTime;
            DateTime breakStopTime;
            if (placeInMiddle)
            {
                GetBreakInMiddle(templateRow.Length, timeSlot.StartTime, timeSlot.StopTime, out breakStartTime, out breakStopTime);
            }
            else
            {
                breakStartTime = timeSlot.StartTime;
                breakStopTime = timeSlot.StartTime.AddMinutes(templateRow.Length);
            }

            TimeBreakTemplateBreakSlot breakSlot = new TimeBreakTemplateBreakSlot(breakStartTime, breakStopTime, templateRow);
            breakSlot.SetBoundary(GetBoundaryStart(templateRow, timeSlot), GetBoundaryStop(templateRow, timeSlot));

            if (TryAdjustBreakAccordingToRules(breakSlot))
                return breakSlot;
            return null;
        }

        #endregion

        #region TimeBreakTemplateRowDTO

        private List<TimeBreakTemplateRowDTO> GetUnhandledTemplateRowMinors()
        {
            List<TimeBreakTemplateRowDTO> unhandledTemplateRows = new List<TimeBreakTemplateRowDTO>();
            foreach (TimeBreakTemplateRowDTO templateRow in this.Input.Template.GetTemplateRows(SoeTimeBreakTemplateType.Minor))
            {
                TimeBreakTemplateBreakSlot breakSlot = this.GetBreakSlots().FirstOrDefault(i => i.TemplateRowGuid == templateRow.Guid && i.Type == SoeTimeBreakTemplateType.Minor);
                if (breakSlot == null)
                    unhandledTemplateRows.Add(templateRow);
            }
            return unhandledTemplateRows;
        }

        private DateTime GetBoundaryStart(TimeBreakTemplateRowDTO templateRow, TimeBreakTemplateTimeSlot timeSlot)
        {
            return CalendarUtility.GetLatestDate(GetBreakWindowStart(templateRow), timeSlot.StartTime);
        }

        private DateTime GetBoundaryStop(TimeBreakTemplateRowDTO templateRow, TimeBreakTemplateBreakSlot breakSlot)
        {
            return CalendarUtility.GetEarliestDate(GetBreakWindowStop(templateRow), breakSlot.StopTime);
        }

        private DateTime GetBoundaryStop(TimeBreakTemplateRowDTO templateRow, TimeBreakTemplateTimeSlot timeSlot)
        {
            return CalendarUtility.GetEarliestDate(GetBreakWindowStop(templateRow), timeSlot.StopTime);
        }

        private DateTime GetBreakWindowStart(TimeBreakTemplateRowDTO templateRow)
        {
            return this.Input.StartTime.AddMinutes(templateRow.MinTimeAfterStart);
        }

        private DateTime GetBreakWindowStop(TimeBreakTemplateRowDTO templateRow)
        {
            return this.Input.StopTime.AddMinutes(-templateRow.MinTimeBeforeEnd);
        }

        #endregion

        #region TimeBreakTemplateTimeSlot

        private TimeBreakTemplateTimeSlot GetPrevTimeSlot(TimeBreakTemplateBreakSlot breakSlot)
        {
            TimeBreakTemplateBreakSlot prevBreakSlot = GetPrevBreakSlot(breakSlot);
            return new TimeBreakTemplateTimeSlot(prevBreakSlot != null ? prevBreakSlot.StopTime : this.Input.StartTime, breakSlot.StartTime);
        }

        private TimeBreakTemplateTimeSlot GetNextTimeSlot(TimeBreakTemplateBreakSlot breakSlot)
        {
            TimeBreakTemplateBreakSlot nextBreakSlot = GetNextBreakSlot(breakSlot);
            return new TimeBreakTemplateTimeSlot(breakSlot.StopTime, nextBreakSlot != null ? nextBreakSlot.StartTime : this.Input.StopTime);
        }

        private List<TimeBreakTemplateTimeSlot> GetTimeSlotsFromBreaks()
        {
            List<TimeBreakTemplateTimeSlot> slots = new List<TimeBreakTemplateTimeSlot>();
            DateTime startTime = this.Input.StartTime;
            if (this.NrOfBreaks > 0)
            {
                foreach (TimeBreakTemplateBreakSlot breakSlot in this.GetBreakSlots())
                {
                    slots.Add(new TimeBreakTemplateTimeSlot(startTime, breakSlot.StartTime));
                    startTime = breakSlot.StopTime;
                }
            }
            slots.Add(new TimeBreakTemplateTimeSlot(startTime, this.Input.StopTime));
            return slots.Where(i => i.StartTime < i.StopTime).OrderBy(i => i.StartTime).ToList();
        }

        private List<TimeBreakTemplateTimeSlot> GenerateTimeSlots(DateTime startTime, DateTime stopTime, int nrOfBreaks)
        {
            List<TimeBreakTemplateTimeSlot> slots = new List<TimeBreakTemplateTimeSlot>();
            if (startTime < stopTime && nrOfBreaks > 0)
            {
                int totalMinutes = (int)stopTime.Subtract(startTime).TotalMinutes;
                int slotMinutes = totalMinutes / nrOfBreaks;
                DateTime currentStartTime = startTime;
                while (currentStartTime < stopTime)
                {
                    DateTime currentStopTime = currentStartTime.AddMinutes(slotMinutes);
                    slots.Add(new TimeBreakTemplateTimeSlot(currentStartTime, currentStopTime));
                    currentStartTime = currentStopTime;
                }
            }
            return slots;
        }

        #endregion

        #region TimeBreakTemplateBreakSlot

        private TimeBreakTemplateBreakSlot GetPrevBreakSlot(TimeBreakTemplateBreakSlot breakSlot)
        {
            TimeBreakTemplateBreakSlot otherBreakSlot = GetOverlappingBreak(breakSlot, orderByDescending: true);
            if (otherBreakSlot == null)
                otherBreakSlot = this.GetBreakSlots().Where(i => i.StopTime <= breakSlot.StartTime && i.Guid != breakSlot.Guid).OrderByDescending(i => i.StopTime).FirstOrDefault();
            return otherBreakSlot;
        }

        private TimeBreakTemplateBreakSlot GetNextBreakSlot(TimeBreakTemplateBreakSlot breakSlot)
        {
            TimeBreakTemplateBreakSlot otherBreakSlot = GetOverlappingBreak(breakSlot, orderByDescending: false);
            if (otherBreakSlot == null)
                otherBreakSlot = this.GetBreakSlots().Where(i => i.StartTime >= breakSlot.StopTime && i.Guid != breakSlot.Guid).OrderBy(i => i.StartTime).FirstOrDefault();
            return otherBreakSlot;
        }

        private TimeBreakTemplateBreakSlot GetOverlappingBreak(TimeBreakTemplateBreakSlot breakSlot, bool orderByDescending = false)
        {
            List<TimeBreakTemplateBreakSlot> otherBreakSlots = (orderByDescending ? this.GetBreakSlots().OrderByDescending(i => i.StopTime) : this.GetBreakSlots().OrderBy(i => i.StartTime)).ToList();
            foreach (TimeBreakTemplateBreakSlot otherBreakSlot in otherBreakSlots.Where(i => i.Guid != breakSlot.Guid))
            {
                if (CalendarUtility.IsTimesOverlappingNew(breakSlot.StartTime, breakSlot.StopTime, otherBreakSlot.StartTime, otherBreakSlot.StopTime))
                    return otherBreakSlot;
            }
            return null;
        }

        private bool EvalRuleLockedTimeSlotViolationMinutes(TimeBreakTemplateBreakSlot breakSlot, out int minutes)
        {
            minutes = 0;
            if (this.Input.LockedTimeSlots == null || !this.Input.LockedTimeSlots.Any())
                return true;

            var freeTimeSlots = new List<TimeBreakTemplateTimeSlot>();

            // Combine and sort locked slots
            var lockedSlots = this.Input.LockedTimeSlots
                                .Concat(this.GetBreakSlots().Where(i => i.Guid != breakSlot.Guid))
                                .OrderBy(slot => slot.StartTime)
                                .ToList();

            if (!lockedSlots.Any(lockedSlot => CalendarUtility.IsTimesOverlappingNew(breakSlot.StartTime, breakSlot.StopTime, lockedSlot.StartTime, lockedSlot.StopTime)))
                return true;

            DateTime lastEndTime = this.Input.StartTime;
            foreach (var slot in lockedSlots)
            {
                var gapStart = lastEndTime;
                var gapEnd = slot.StartTime;

                // If there is a gap, add it to FreeTimeSlots
                if (gapEnd > gapStart)
                {
                    freeTimeSlots.Add(new TimeBreakTemplateTimeSlot
                    {
                        StartTime = gapStart,
                        StopTime = gapEnd
                    });
                }

                lastEndTime = slot.StopTime > lastEndTime ? slot.StopTime : lastEndTime;
            }

            // Check for a gap after the last locked slot
            if (this.Input.StopTime > lastEndTime)
            {
                freeTimeSlots.Add(new TimeBreakTemplateTimeSlot { StartTime = lastEndTime, StopTime = this.Input.StopTime });
            }
            //And before the first locked slot
            if (lockedSlots.Any() && this.Input.StartTime < lockedSlots.First().StartTime)
            {
                freeTimeSlots.Add(new TimeBreakTemplateTimeSlot { StartTime = this.Input.StartTime, StopTime = lockedSlots.First().StartTime });
            }

            var length = breakSlot.Length;
            DateTime middleOfBreak = breakSlot.StartTime.AddMinutes(length / 2);
            // Iterate over each free slot
            foreach (var freeSlot in freeTimeSlots.OrderBy(freeSlot => Math.Abs((freeSlot.StartTime - middleOfBreak).TotalMinutes)))
            {
                TimeBreakTemplateBreakSlot tryBreakSlot = new TimeBreakTemplateBreakSlot(breakSlot);

                var minutesFromBreakslot = (int)(freeSlot.StartTime - breakSlot.StopTime).TotalMinutes;

                if (minutesFromBreakslot < 0) // if the free slot is before the breakslot, move breakslot to the left
                {
                    tryBreakSlot.StopTime = freeSlot.StopTime;
                    tryBreakSlot.StartTime = freeSlot.StopTime.AddMinutes(-length);

                    if (!lockedSlots.Any(lockedSlot => CalendarUtility.IsTimesOverlappingNew(tryBreakSlot.StartTime, tryBreakSlot.StopTime, lockedSlot.StartTime, lockedSlot.StopTime)))
                    {
                        minutes = (int)(tryBreakSlot.StartTime - breakSlot.StartTime).TotalMinutes;
                        return true;
                    }
                }
                else // if the free slot is after the breakslot, move breakslot to the right
                {
                    tryBreakSlot.StartTime = freeSlot.StopTime.AddMinutes(-length);
                    tryBreakSlot.StopTime = freeSlot.StopTime;
                    if (!lockedSlots.Any(lockedSlot => CalendarUtility.IsTimesOverlappingNew(tryBreakSlot.StartTime, tryBreakSlot.StopTime, lockedSlot.StartTime, lockedSlot.StopTime)))
                    {
                        minutes = (int)(tryBreakSlot.StopTime - breakSlot.StopTime).TotalMinutes;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool EvalRuleBreakWindowStartViolationMinutes(TimeBreakTemplateBreakSlot breakSlot, out int minutes)
        {
            minutes = 0;
            if (!breakSlot.BoundaryStartTime.HasValue)
                return true;

            minutes = (int)breakSlot.BoundaryStartTime.Value.Subtract(breakSlot.StartTime).TotalMinutes;
            return minutes <= 0;
        }

        private bool EvalRuleBreakWindowStopViolationMinutes(TimeBreakTemplateBreakSlot breakSlot, out int minutes)
        {
            minutes = 0;
            if (!breakSlot.BoundaryStopTime.HasValue)
                return true;

            minutes = (int)breakSlot.StopTime.Subtract(breakSlot.BoundaryStopTime.Value).TotalMinutes;
            return minutes <= 0;
        }

        private bool EvalRuleBetweenBreaksViolationMinutes(TimeBreakTemplateBreakSlot breakSlot, out int minutes, bool prev = true, bool useBoundary = false)
        {
            minutes = 0;
            if (this.Input.Template.MinTimeBetweenBreaks <= 0 && !useBoundary)
                return true;

            if (prev)
            {
                DateTime? startTime = useBoundary ? breakSlot.BoundaryStartTime : breakSlot.StartTime;
                if (startTime.HasValue)
                {
                    TimeBreakTemplateBreakSlot prevBreakSlot = GetPrevBreakSlot(breakSlot);
                    if (prevBreakSlot != null)
                        minutes = this.Input.Template.MinTimeBetweenBreaks - (int)startTime.Value.Subtract(prevBreakSlot.StopTime).TotalMinutes;
                }
            }
            else
            {
                DateTime? stopTime = useBoundary ? breakSlot.BoundaryStopTime : breakSlot.StopTime;
                if (stopTime.HasValue)
                {
                    TimeBreakTemplateBreakSlot nextBreakSlot = GetNextBreakSlot(breakSlot);
                    if (nextBreakSlot != null)
                        minutes = this.Input.Template.MinTimeBetweenBreaks - (int)nextBreakSlot.StartTime.Subtract(stopTime.Value).TotalMinutes;
                }
            }
            return minutes <= 0;
        }

        private bool EvalRuleShiftLengthViolationMinutes(TimeBreakTemplateBreakSlot breakSlot, out int minutes, bool prev = true)
        {
            minutes = 0;
            if (!this.Input.Template.UseMaxWorkTimeBetweenBreaks)
                return true;

            if (prev)
            {
                TimeBreakTemplateTimeSlot prevTimeSlot = GetPrevTimeSlot(breakSlot);
                if (prevTimeSlot != null)
                    minutes = prevTimeSlot.Length - TimeBreakTemplateRules.RULE_MAXSHIFTLENGTH_MINUTES;
            }
            else
            {
                TimeBreakTemplateTimeSlot nextTimeSlot = GetNextTimeSlot(breakSlot);
                if (nextTimeSlot != null)
                    minutes = nextTimeSlot.Length - TimeBreakTemplateRules.RULE_MAXSHIFTLENGTH_MINUTES;
            }
            return minutes <= 0;
        }

        private void GetBreakInMiddle(int length, DateTime startTime, DateTime stopTime, out DateTime breakStartTime, out DateTime breakStopTime)
        {
            CalendarUtility.GetTimeInMiddle(length, CalendarUtility.GetTotalMinutesFromDateTimeAsTime(startTime, startTime.Date), CalendarUtility.GetTotalMinutesFromDateTimeAsTime(stopTime, stopTime.Date), out breakStartTime, out breakStopTime);
            breakStartTime = breakStartTime.RoundDown(TimeSpan.FromMinutes(TimeBreakTemplateRules.RULE_ROUNDBREAK_MINUTES));
            breakStopTime = breakStartTime.AddMinutes(length);
        }

        #endregion

        #endregion
    }

    #region Help-classes

    internal static class TimeBreakTemplateExtensions
    {
        public static string ToString(this IEnumerable<TimeBreakTemplateBreakSlot> l)
        {
            return l.ToString(null, null);
        }

        public static string ToString(this IEnumerable<TimeBreakTemplateBreakSlot> l, DateTime? startTime, DateTime? stopTime)
        {
            StringBuilder sb = new StringBuilder();

            if (startTime.HasValue)
            {
                sb.Append(startTime.Value.ToString("hh:mm"));
                sb.Append("-->");
            }
            foreach (TimeBreakTemplateBreakSlot breakSlot in l)
            {
                sb.Append(breakSlot.StartTime.ToString("hh:mm"));
                sb.Append("-");
                sb.Append(breakSlot.StopTime.ToString("hh:mm"));
                sb.Append("-->");
            }
            if (stopTime.HasValue)
            {
                sb.Append(stopTime.Value.ToString("hh:mm"));
            }

            return sb.ToString();
        }
    }

    internal static class TimeBreakTemplateRules
    {
        //Constants
        public const int RULE_MAXSHIFTLENGTH_MINUTES = 120; //2h rule
        public const int RULE_ROUNDBREAK_MINUTES = 15;
    }

    public class TimeBreakTemplateEvaluationRepository
    {
        #region Variables

        private readonly Dictionary<string, TimeBreakTemplateEvaluationResult> results;

        #endregion

        #region Ctor

        public TimeBreakTemplateEvaluationRepository()
        {
            this.results = new Dictionary<string, TimeBreakTemplateEvaluationResult>();
        }

        #endregion

        #region Public methods

        public TimeBreakTemplateEvaluationResult GetResult(TimeBreakTemplateEvaluationInput input)
        {
            if (this.results != null)
            {
                var validResult = this.results.FirstOrDefault(f => f.Key == input.CacheKey);

                if (validResult.Value != null && validResult.Value.IsMatching(input))
                    return validResult.Value;
                //w.StartTime.HasValue && w.StartTime.Value == input.StartTime && w.OriginalStopTime.HasValue && w.OriginalStopTime.Value == input.StopTime).ToList();
                // return validResult.Where(w => w.IsMatching(input)).FirstOrDefault();
            }
            return null;
        }

        public int GetNrOfResults()
        {
            return this.results.Count;
        }

        public void AddResult(TimeBreakTemplateEvaluationOutput output)
        {
            var result = new TimeBreakTemplateEvaluationResult(output);
            var cacheKey = result.CacheKey;

            if (!this.results.ContainsKey(cacheKey))
                this.results.Add(result.CacheKey, result);
        }

        #endregion
    }

    public class TimeBreakTemplateEvaluationInput
    {
        #region Variables

        //Mandatory
        public SoeTimeBreakTemplateEvaluation EvaluationType { get; set; }
        private DateTime _startTime;
        private DateTime _stopTime;
        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                if (_startTime != value)
                {
                    _startTime = value;
                    UpdateCacheKey();
                }
            }
        }

        public DateTime StopTime
        {
            get => _stopTime;
            set
            {
                if (_stopTime != value)
                {
                    _stopTime = value;
                    UpdateCacheKey();
                }
            }
        }
        private readonly DateTime originalStopTime;
        public int CalculatedBreakLength
        {
            get
            {
                return (int)this.StopTime.Subtract(this.originalStopTime).TotalMinutes;
            }
        }
        public int Length { get { return (int)StopTime.Subtract(StartTime).TotalMinutes; } }
        public bool IsNetTime { get; set; }

        //Optional
        public DateTime? Date { get; set; }
        public bool HasDate { get { return this.Date.HasValue; } }
        public DayOfWeek? DayOfWeek { get; set; }
        public bool HasDayOfWeek { get { return this.DayOfWeek != null; } }
        public List<int> ShiftTypeIds { get; set; }
        public bool HasShiftTypeIds { get { return this.ShiftTypeIds != null && this.ShiftTypeIds.Count > 0; } }
        public int? DayTypeId { get; set; }
        public bool HasDayType { get { return this.DayTypeId.HasValue && this.DayTypeId.Value > 0; } }
        public List<TimeCodeBreakGroupDTO> TimeCodeBreakGroups { get; set; }
        public bool HasTimeCodeBreakGroups
        {
            get
            {
                return this.TimeCodeBreakGroups != null && this.TimeCodeBreakGroups.Count > 0;
            }
        }
        public List<TimeBreakTemplateTimeSlot> LockedTimeSlots { get; set; }
        public bool HasLockedTimeSlots
        {
            get
            {
                return this.LockedTimeSlots != null && this.LockedTimeSlots.Count > 0;
            }
        }
        public TimeBreakTemplateDebugParameters DebugParameters { get; set; }
        public bool HasDebugParameters
        {
            get
            {
                return this.DebugParameters != null;
            }
        }
        public bool CanBeMatched
        {
            get
            {
                return !this.HasLockedTimeSlots && !this.HasDebugParameters;
            }
        }
        private bool isAdjustedToGrossStopTime;
        public bool AdjustedToGrossStopTime
        {
            get
            {
                return this.isAdjustedToGrossStopTime;
            }
        }

        //Extensions
        public TimeBreakTemplateDTO Template { get; set; }

        #endregion

        #region Ctor

        public TimeBreakTemplateEvaluationInput(SoeTimeBreakTemplateEvaluation evaluationType, DateTime startTime, DateTime stopTime, DateTime? date = null, List<int> shiftTypeIds = null, int? dayTypeId = null, DayOfWeek? dayOfWeek = null, List<TimeBreakTemplateTimeSlot> lockedTimeSlots = null, List<TimeCodeBreakGroupDTO> timeCodeBreakGroups = null, TimeBreakTemplateDebugParameters debugParameters = null, bool isNetTime = false)
        {
            //Mandatory
            this.EvaluationType = evaluationType;
            this.StartTime = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, startTime);
            this.StopTime = CalendarUtility.GetDateTime(CalendarUtility.DATETIME_DEFAULT, stopTime).AddDays((stopTime.Date - startTime.Date).Days);
            this.originalStopTime = this.StopTime;

            //Optional
            this.Date = date;
            this.DayOfWeek = dayOfWeek;
            this.DayTypeId = dayTypeId;
            this.ShiftTypeIds = shiftTypeIds ?? new List<int>();
            this.LockedTimeSlots = lockedTimeSlots;
            this.TimeCodeBreakGroups = timeCodeBreakGroups != null ? timeCodeBreakGroups.Where(i => !i.TimeCodeBreaks.IsNullOrEmpty()).ToList() : new List<TimeCodeBreakGroupDTO>();
            this.DebugParameters = debugParameters;
            this.IsNetTime = isNetTime;
        }

        #endregion

        #region Public methods

        public bool IsMatching(TimeBreakTemplateEvaluationInput input)
        {
            if (!this.CanBeMatched || !input.CanBeMatched)
                return false;

            return
                this.EvaluationType == input.EvaluationType &&
                this.Length - (this.IsNetTime ? this.CalculatedBreakLength : 0) == input.Length &&
                this.StartTime == input.StartTime &&
                this.originalStopTime == input.StopTime &&
                this.IsNetTime == input.IsNetTime &&
                NumberUtility.IsEqual(this.ShiftTypeIds, input.ShiftTypeIds) &&
                NumberUtility.IsEqual(this.DayTypeId, input.DayTypeId) &&
                CalendarUtility.IsEqual(this.DayOfWeek, input.DayOfWeek);
        }

        public void AdjustToGrossStopTime()
        {
            if (this.Template != null)
            {
                DateTime grossStopTime = this.StopTime.AddMinutes(this.Template.BreakLength);
                if (grossStopTime != this.StopTime)
                {
                    this.StopTime = grossStopTime;
                    this.isAdjustedToGrossStopTime = true;
                }
            }
        }

        public DateTime? GetOriginalStopTime()
        {
            return this.originalStopTime;
        }

        public override string ToString()
        {
            return $"{StartTime:HH:mm} {StopTime:HH:mm} ({Length})";
        }

        string _cacheKey;
        public string CacheKey => _cacheKey;

        private void UpdateCacheKey()
        {
            _cacheKey = $"{_startTime}#{_stopTime}";
        }

        #endregion
    }

    public class TimeBreakTemplateEvaluationOutput
    {
        #region Variables

        public bool Success { get; set; }
        public TimeBreakTemplateEvaluationInput Input { get; set; }
        public List<TimeBreakTemplateBreakSlot> BreakSlots { get; set; }
        public List<TimeCodeBreakGroupDTO> TimeCodeBreakGroups { get; set; }
        public int MinTimeBetweenBreaks { get; set; }
        public bool AdjustedToGrossStopTime
        {
            get
            {
                return this.Input != null && this.Input.AdjustedToGrossStopTime;
            }
        }
        public DateTime StartTime
        {
            get
            {
                return this.Input.StartTime;
            }
        }
        public DateTime StopTime
        {
            get
            {
                return this.Input.StopTime;
            }
        }

        #endregion

        #region Ctor

        public TimeBreakTemplateEvaluationOutput(bool success)
        {
            this.Success = success;
            this.TimeCodeBreakGroups = new List<TimeCodeBreakGroupDTO>();
            this.BreakSlots = new List<TimeBreakTemplateBreakSlot>();
        }

        public TimeBreakTemplateEvaluationOutput(TimeBreakTemplateEvaluationInput input, List<TimeBreakTemplateBreakSlot> breakSlots)
        {
            this.Success = breakSlots != null;
            this.Input = input;
            this.BreakSlots = breakSlots ?? new List<TimeBreakTemplateBreakSlot>();
            this.TimeCodeBreakGroups = input.TimeCodeBreakGroups ?? new List<TimeCodeBreakGroupDTO>();
            this.MinTimeBetweenBreaks = input.Template?.MinTimeBetweenBreaks ?? 0;
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.Success ? this.BreakSlots.ToString(this.Input.StartTime, this.Input.StopTime) : String.Empty;
        }

        #endregion
    }

    public class TimeBreakTemplateEvaluationResult
    {
        #region Variables

        private readonly TimeBreakTemplateEvaluationInput input;
        private readonly TimeBreakTemplateEvaluationOutput output;
        private readonly DateTime created;
        public DateTime Created
        {
            get
            {
                return this.created;
            }
        }
        private int usedTimes;
        public int UsedTimes
        {
            get
            {
                return this.usedTimes;
            }
        }
        private readonly int templateShiftLength;
        public int TemplateShiftLength
        {
            get
            {
                return this.templateShiftLength;
            }
        }
        private readonly DateTime? validFromDate;
        public DateTime? ValidFromDate
        {
            get
            {
                return this.validFromDate;
            }
        }
        private readonly DateTime? validToDate;
        public DateTime? ValidToDate
        {
            get
            {
                return this.validToDate;
            }
        }

        public DateTime? StartTime
        {
            get
            {
                if (this.input != null)
                    return this.input.StartTime;
                return null;
            }
        }

        public DateTime? OriginalStopTime
        {
            get
            {
                if (this.input != null)
                    return this.input.GetOriginalStopTime();
                return null;
            }
        }

        #endregion

        #region Ctor

        public TimeBreakTemplateEvaluationResult(TimeBreakTemplateEvaluationOutput output)
        {
            this.output = output;
            this.created = DateTime.UtcNow;
            this.usedTimes = 0;
            if (output != null && output.Input != null)
            {
                this.input = output.Input;
                if (output.Input.Template != null)
                {
                    this.templateShiftLength = output.Input.Template.ShiftLength;
                    if (output.Input.Template.StartDate.HasValue)
                        this.validFromDate = output.Input.Template.StartDate.Value;
                    if (output.Input.Template.StopDate.HasValue)
                        this.validToDate = output.Input.Template.StopDate.Value;
                }
            }
        }

        #endregion

        #region Public methods

        public bool IsMatching(TimeBreakTemplateEvaluationInput input)
        {
            if (this.input == null || input == null)
                return false;
            if (input.Date.HasValue && !CalendarUtility.IsDateInRange(input.Date.Value, this.ValidFromDate, this.ValidToDate))
                return false;
            return this.input.IsMatching(input);
        }

        public TimeBreakTemplateEvaluationOutput Use()
        {
            this.usedTimes++;
            return output;
        }

        public override string ToString()
        {
            return this.output.Input.ToString();
        }

        public string CacheKey
        {
            get
            {
                return $"{this.StartTime}#{this.OriginalStopTime}";
            }
        }

        #endregion
    }

    public class TimeBreakTemplateDebugParameters
    {
        #region Variables

        //TimeBreakTemplate
        public int MinTimeBetweenBreaks { get; set; }
        public bool UseMaxWorkTimeBetweenBreaks { get; set; }

        //TimeBreakTemplateRow - Major
        public int NrOfBreaksMajor { get; set; }
        public int LengthMajor { get; set; }
        public int MinTimeAfterStartMajor { get; set; }
        public int MinTimeBeforeEndMajor { get; set; }

        //TimeBreakTemplateRow - Minor
        public int NrOfBreaksMinor { get; set; }
        public int LengthMinor { get; set; }
        public int MinTimeAfterStartMinor { get; set; }
        public int MinTimeBeforeEndMinor { get; set; }

        #endregion

        #region Ctor

        public TimeBreakTemplateDebugParameters(
            int minTimeBetweenBreaks, bool useMaxWorkTimeBetweenBreaks,
            int nrOfBreaksMajor, int lengthMajor, int minTimeAfterStartMajor, int minTimeBeforeEndMajor,
            int nrOfBreaksMinor, int lengthMinor, int minTimeAfterStartMinor, int minTimeBeforeEndMinor)
        {
            this.MinTimeBetweenBreaks = minTimeBetweenBreaks;
            this.UseMaxWorkTimeBetweenBreaks = useMaxWorkTimeBetweenBreaks;

            //Major
            this.NrOfBreaksMajor = nrOfBreaksMajor;
            this.LengthMajor = lengthMajor;
            this.MinTimeAfterStartMajor = minTimeAfterStartMajor;
            this.MinTimeBeforeEndMajor = minTimeBeforeEndMajor;

            //Minor
            this.NrOfBreaksMinor = nrOfBreaksMinor;
            this.LengthMinor = lengthMinor;
            this.MinTimeAfterStartMinor = minTimeAfterStartMinor;
            this.MinTimeBeforeEndMinor = minTimeBeforeEndMinor;
        }

        #endregion
    }

    public class TimeBreakTemplateTimeSlot
    {
        #region Variables

        private Guid guid;
        public Guid Guid
        {
            get
            {
                return this.guid;
            }
        }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public int Length { get { return (int)this.StopTime.Subtract(this.StartTime).TotalMinutes; } }

        #endregion

        #region Ctor

        public TimeBreakTemplateTimeSlot()
        {
            this.guid = Guid.NewGuid();
        }

        public TimeBreakTemplateTimeSlot(DateTime startTime, DateTime stopTime, Guid? guid = null)
        {
            this.guid = guid ?? Guid.NewGuid();
            this.StartTime = startTime;
            this.StopTime = stopTime;
        }

        public TimeBreakTemplateTimeSlot(DateTime startTime, int length, Guid? guid = null)
        {
            this.guid = guid ?? Guid.NewGuid();
            this.StartTime = startTime;
            this.StopTime = startTime.AddMinutes(length);
        }

        #endregion

        #region Protected methods

        protected void SetGuid(Guid guid)
        {
            this.guid = guid;
        }

        #endregion
    }

    public class TimeBreakTemplateBreakSlot : TimeBreakTemplateTimeSlot
    {
        #region Variables

        //Mandatory
        public SoeTimeBreakTemplateType Type { get; set; }
        public DateTime? BoundaryStartTime { get; set; }
        public DateTime? BoundaryStopTime { get; set; }
        public int? TimeCodeBreakGroupId { get; set; }

        //Calculated
        private Guid templateRowGuid;
        public Guid TemplateRowGuid { get { return this.templateRowGuid; } }
        public bool CanMoveBackward { get { return this.BoundaryStartTime.HasValue && this.BoundaryStartTime.Value < this.StartTime; } }
        public bool CanMoveForward { get { return this.BoundaryStopTime.HasValue && this.BoundaryStopTime.Value > this.StopTime; } }
        private bool isCloned;
        public bool IsCloned
        {
            get
            {
                return this.isCloned;
            }
        }

        public DateTime Middle
        {
            get
            {
                return this.StartTime.AddMinutes(Convert.ToInt32((this.StopTime - this.StartTime).TotalMinutes / 2));
            }
        }

        //Extensions
        public int? ShiftTypeId { get; set; }
        private bool breakNrIsSet;
        private int breakNr;
        public int BreakNr
        {
            get
            {
                return this.breakNr;
            }
            set
            {
                if (!this.breakNrIsSet)
                {
                    this.breakNr = value;
                    this.breakNrIsSet = true;
                }
            }
        }

        #endregion

        #region Ctor

        public TimeBreakTemplateBreakSlot() : base() { }

        public TimeBreakTemplateBreakSlot(TimeBreakTemplateBreakSlot breakSlot) : base(breakSlot.StartTime, breakSlot.StopTime, breakSlot.Guid)
        {
            this.Type = breakSlot.Type;
            this.TimeCodeBreakGroupId = breakSlot.TimeCodeBreakGroupId;
        }

        public TimeBreakTemplateBreakSlot(DateTime startTime, TimeBreakTemplateBreakSlot breakSlot) : base(startTime, breakSlot.Length, breakSlot.Guid)
        {
            this.Type = breakSlot.Type;
            this.TimeCodeBreakGroupId = breakSlot.TimeCodeBreakGroupId;
        }

        public TimeBreakTemplateBreakSlot(DateTime startTime, DateTime stopTime, TimeBreakTemplateBreakSlot breakSlot) : base(startTime, stopTime, breakSlot.Guid)
        {
            this.Type = breakSlot.Type;
            this.TimeCodeBreakGroupId = breakSlot.TimeCodeBreakGroupId;
        }

        public TimeBreakTemplateBreakSlot(DateTime startTime, DateTime stopTime, TimeBreakTemplateRowDTO templateRow, Guid? guid = null) : base(startTime, stopTime, guid)
        {
            this.Type = templateRow.Type;
            this.TimeCodeBreakGroupId = templateRow.TimeCodeBreakGroupId;
        }

        #endregion

        #region Public methods

        public TimeBreakTemplateBreakSlot Clone()
        {
            TimeBreakTemplateBreakSlot cloneBreakSlot = new TimeBreakTemplateBreakSlot()
            {
                StartTime = this.StartTime,
                StopTime = this.StopTime,
                Type = this.Type,
                TimeCodeBreakGroupId = this.TimeCodeBreakGroupId,
                BoundaryStartTime = this.BoundaryStartTime,
                BoundaryStopTime = this.BoundaryStopTime,
                ShiftTypeId = this.ShiftTypeId
            };

            cloneBreakSlot.SetGuid(this.Guid);
            cloneBreakSlot.SetTemplateRowGuid(this.TemplateRowGuid);
            this.isCloned = true;
            return cloneBreakSlot;
        }

        public void SetTemplateRowGuid(Guid templateRowGuid)
        {
            this.templateRowGuid = templateRowGuid;
        }

        public bool HasBoundaries()
        {
            return this.BoundaryStartTime.HasValue && this.BoundaryStopTime.HasValue;
        }

        public void MoveStartBoundary(int minutes)
        {
            this.SetStartBoundary(this.BoundaryStartTime.Value.AddMinutes(minutes));
        }

        public void MoveStopBoundary(int minutes)
        {
            this.SetStopBoundary(this.BoundaryStopTime.Value.AddMinutes(minutes));
        }

        public void SetStartBoundary(DateTime boundaryStartTime)
        {
            this.BoundaryStartTime = boundaryStartTime;
        }

        public void SetStopBoundary(DateTime boundaryStopTime)
        {
            this.BoundaryStopTime = boundaryStopTime;
        }

        public void SetBoundary(DateTime boundaryStartTime, DateTime boundaryStopTime)
        {
            this.BoundaryStartTime = boundaryStartTime;
            this.BoundaryStopTime = boundaryStopTime;
        }

        public void ClearBoundary()
        {
            this.BoundaryStartTime = null;
            this.BoundaryStopTime = null;
        }

        public bool ValidateBoundary(DateTime startTime, DateTime stopTime)
        {
            if (this.BoundaryStartTime.HasValue && this.BoundaryStartTime.Value > startTime)
                return false;
            if (this.BoundaryStopTime.HasValue && this.BoundaryStopTime < stopTime)
                return false;
            return true;
        }

        public bool TryMoveBackward(DateTime newStartTime, TimeBreakTemplateRule rule)
        {
            return TryMoveBackward((int)this.StartTime.Subtract(newStartTime).TotalMinutes, rule);
        }

        public bool TryMoveBackward(int minutes, TimeBreakTemplateRule rule)
        {
            if (minutes <= 0)
                return false;

            DateTime startTime = this.StartTime.AddMinutes(-minutes);
            DateTime stopTime = this.StopTime.AddMinutes(-minutes);
            if (!ValidateBoundary(startTime, stopTime))
                return false;

            this.StartTime = startTime;
            this.StopTime = stopTime;

            if (rule == TimeBreakTemplateRule.BreakWindowStop)
                this.BoundaryStopTime = this.StopTime;
            if (rule == TimeBreakTemplateRule.MaxShiftLength)
                this.BoundaryStopTime = this.StopTime;

            return true;
        }

        public bool TryMoveForward(DateTime newStopTime, TimeBreakTemplateRule rule)
        {
            return TryMoveForward((int)newStopTime.Subtract(this.StopTime).TotalMinutes, rule);
        }

        public bool TryMoveForward(int minutes, TimeBreakTemplateRule rule)
        {
            if (minutes <= 0)
                return false;

            DateTime startTime = this.StartTime.AddMinutes(minutes);
            DateTime stopTime = this.StopTime.AddMinutes(minutes);
            if (!ValidateBoundary(startTime, stopTime))
                return false;

            this.StartTime = startTime;
            this.StopTime = stopTime;

            if (rule == TimeBreakTemplateRule.BreakWindowStart)
                this.BoundaryStartTime = this.StartTime;
            if (rule == TimeBreakTemplateRule.TimeBetweenBreaks)
                this.BoundaryStartTime = this.StartTime;
            if (rule == TimeBreakTemplateRule.MaxShiftLength)
                this.BoundaryStopTime = this.StopTime; //lock stop because of last shift
            if (rule == TimeBreakTemplateRule.LockedTimeSlot)
                this.BoundaryStartTime = this.StartTime;

            return true;
        }

        #endregion
    }

    #endregion
}

