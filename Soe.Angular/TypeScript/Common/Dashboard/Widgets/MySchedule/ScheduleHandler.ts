import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { TermGroup_TimeScheduleTemplateBlockShiftUserStatus } from "../../../../Util/CommonEnumerations";
import { GraphicsUtility } from "../../../../Util/GraphicsUtility";
import { ShiftDTO } from "../../../Models/TimeSchedulePlanningDTOs";
import { ScheduleController } from "./ScheduleDirective";

export class ScheduleHandler {
    private scheduleWidth: number;

    private shiftHeight: number = 20;
    private shiftMargin: number = 0;

    // Flags
    private firstTimeScheduleSetup: boolean = true;

    constructor(private controller: ScheduleController, private $filter: ng.IFilterService, private $timeout: ng.ITimeoutService, private $q: ng.IQService, private $scope: ng.IScope, private $compile: ng.ICompileService) {
        this.renderScheduleRow = this.renderScheduleRow.bind(this);
    }

    // SETUP

    private performFirstTimeSetup() {
        this.firstTimeScheduleSetup = false;
        var self = this;

        // Single click on shift
        $('.myschedule-schedule').on('click', '.myschedule-schedule-shift', function (event) {
            var shift = self.getShiftFromJQueryElem(this);

            if (shift.selected)
                self.unselectShift(shift, true, true);
            else {
                self.clearSelectedShifts();
                self.selectShift(shift, true, true);
            }
        });

        // Double click on shift
        $('.myschedule-schedule').on('dblclick', '.myschedule-schedule-shift', function (event) {
            var shift = self.getShiftFromElem(this);
            if (shift)
                self.controller.editShift(shift);
        });

        // Resize window
        $(window).resize((e) => {
            // This event is also fired when a shift is dragged or resized.
            // Do not run updateWidthOnAllElements() in those cases.
            let isResizebleElement = $(e.target).hasClass('ui-resizable');
            if (!isResizebleElement) {
                // When left menu is toggled, it has a 500 ms animation,
                // so we need to increase the delay a bit.
                this.updateWidthOnAllElements(600);
            }
        });
    }

    // RENDER

    private getTableAttachmentPoint() {
        return $('.myschedule-schedule tbody');
    }

    public clearScheduleViewBody(): ng.IPromise<any> {
        return this.$timeout(() => {
            var attachmentPoint = this.getTableAttachmentPoint();
            attachmentPoint.empty();
        });
    }

    public renderSchedule() {
        this.renderScheduleBody();
        if (this.firstTimeScheduleSetup)
            this.performFirstTimeSetup();
    }

    private renderScheduleBody() {
        this.clearScheduleViewBody().then(() => {
            var attachmentPoint = this.getTableAttachmentPoint();

            for (let date: Date = this.controller.dateFrom; date.isSameOrBeforeOnDay(this.controller.dateTo); date = date.addDays(1)) {
                let shifts: ShiftDTO[] = this.controller.getShifts(date);
                let presenceShifts = _.filter(shifts, s => !s.isAbsenceRequest && !s.isAbsence);
                let absenceShifts = _.filter(shifts, s => s.isAbsenceRequest || s.isAbsence);

                if (presenceShifts.length > 0 || absenceShifts.length === 0) {
                    let row = this.renderScheduleRow(date, presenceShifts);
                    attachmentPoint.append(row);
                }
                if (absenceShifts.length > 0) {
                    let row = this.renderScheduleRow(date, absenceShifts, presenceShifts.length > 0);
                    attachmentPoint.append(row);
                }

                if (this.controller.isDateExpanded(date)) {
                    if (this.controller.showOpenShifts)
                        this.renderOpenShiftsRows(attachmentPoint, date);
                    if (this.controller.showColleaguesShifts)
                        this.renderColleaguesShiftsRows(attachmentPoint, date);
                }
            }

            this.updateWidthOnAllElements();
        });
    }

    private renderScheduleRow(date: Date, shifts: ShiftDTO[], hideLabel: boolean = false) {
        var isSaturday: boolean = this.controller.isSaturday(date);
        var isSunday: boolean = this.controller.isSunday(date);

        var row: HTMLTableRowElement = document.createElement('tr');
        row.classList.add('myschedule-schedule-row');
        row.setAttribute('row-date', date.toPipedDate());
        $(row).css('height', this.shiftHeight);

        var rowLabelTd: HTMLTableDataCellElement = document.createElement('td');
        rowLabelTd.classList.add('myschedule-schedule-rowlabel');
        if (isSunday)
            rowLabelTd.classList.add('myschedule-schedule-horizontal-separator');

        if (!hideLabel && (this.controller.showOpenShifts || this.controller.showColleaguesShifts)) {
            var labelIcon = document.createElement('i');
            labelIcon.classList.add('fal');
            labelIcon.setAttribute('data-ng-class', "{\'fa-minus\': ctrl.isDateExpandedFormatted(" + date.toPipedDate() + "), \'fa-plus\': !ctrl.isDateExpandedFormatted(" + date.toPipedDate() + ")}");
            labelIcon.setAttribute('data-ng-click', '$event.stopPropagation(); ctrl.toggleDate(' + date.toPipedDate() + ');');
            rowLabelTd.appendChild(labelIcon);
        }

        var labelSpan = document.createElement('span');
        if (date.isToday())
            labelSpan.classList.add('today');
        else if (isSaturday)
            labelSpan.classList.add('saturday');
        else if (isSunday)
            labelSpan.classList.add('sunday');
        if (!hideLabel)
            labelSpan.innerText = "{0} {1}/{2}".format(CalendarUtility.getDayName(date.dayOfWeek()).toUpperCaseFirstLetter(), date.getDate().toString(), (date.getMonth() + 1).toString());

        // Context menu (date)
        if (this.controller.enableAvailibility) {
            // For now, only availibility is present in the context menu,
            // so if availibility is disabled, do not show any menu at all.
            // This must be removed if other options are added to the menu.
            rowLabelTd.setAttribute('context-menu', "ctrl.getDateContextMenuOptions(" + date.toPipedDate() + ")");
            rowLabelTd.setAttribute('context-menu-empty-text', "\' \'");
            rowLabelTd.setAttribute('model', date.toPipedDate());
        }

        rowLabelTd.appendChild(labelSpan);

        this.$compile(rowLabelTd)(this.$scope);
        row.appendChild(rowLabelTd);

        for (let i = 0, j = this.controller.timeSlots.length; i < j; i++) {
            var td = document.createElement('td');
            td.classList.add('myschedule-schedule-day');

            // Saturday/Sunday
            if (isSaturday)
                td.classList.add('saturday');
            else if (isSunday)
                td.classList.add('sunday');

            // Sunday
            if (isSunday)
                td.classList.add('myschedule-schedule-horizontal-separator');

            // Availability
            let availabilityToolTip: string = '';
            if (this.controller.employee && (this.controller.employee.hasAvailability || this.controller.employee.hasUnavailability)) {
                // Get date and time range for current cell
                let dateFrom: Date = date.beginningOfDay().addMinutes(this.controller.timeSlots[i]);
                let dateTo: Date = dateFrom.addHours(1).addSeconds(-1);

                if (this.controller.employee.isFullyAvailableInRange(dateFrom, dateTo)) {
                    td.classList.add('planning-day-available');
                    availabilityToolTip = this.controller.terms["time.schedule.planning.available"];
                } else if (this.controller.employee.isFullyUnavailableInRange(dateFrom, dateTo)) {
                    td.classList.add('planning-day-unavailable');
                    availabilityToolTip = this.controller.terms["time.schedule.planning.unavailable"];
                } else {
                    let partlyAvailable = this.controller.employee.isAvailableInRange(dateFrom, dateTo);
                    let partlyUnavailable = this.controller.employee.isUnavailableInRange(dateFrom, dateTo);
                    if (partlyAvailable && !partlyUnavailable) {
                        td.classList.add('planning-day-partly-available');
                    } else if (partlyUnavailable && !partlyAvailable) {
                        td.classList.add('planning-day-partly-unavailable');
                    } else if (partlyAvailable && partlyUnavailable) {
                        td.classList.add('planning-day-mixed-available');
                    }
                    if (partlyAvailable) {
                        let availableDates = this.controller.employee.getAvailableInRange(dateFrom, dateTo);
                        if (availableDates.length > 0) {
                            _.forEach(availableDates, availableDate => {
                                availabilityToolTip += "{0} {1}-{2}".format(this.controller.terms["time.schedule.planning.available"], availableDate.start.toFormattedTime(), availableDate.stop.toFormattedTime());
                                if (availableDate.comment)
                                    availabilityToolTip += ", {0}".format(availableDate.comment);
                                availabilityToolTip += "\n";
                            });
                        }
                    }
                    if (partlyUnavailable) {
                        let unavailableDates = this.controller.employee.getUnavailableInRange(dateFrom, dateTo);
                        if (unavailableDates.length > 0) {
                            _.forEach(unavailableDates, unavailableDate => {
                                availabilityToolTip += "{0} {1}-{2}".format(this.controller.terms["time.schedule.planning.unavailable"], unavailableDate.start.toFormattedTime(), unavailableDate.stop.toFormattedTime());
                                if (unavailableDate.comment)
                                    availabilityToolTip += ", {0}".format(unavailableDate.comment);
                                availabilityToolTip += "\n";
                            });
                        }
                    }
                }
                if (availabilityToolTip.length > 0)
                    availabilityToolTip = "{0}:\n{1}".format(this.controller.terms["time.schedule.planning.availability"], availabilityToolTip);
            }

            if (availabilityToolTip.length > 0)
                td.title = availabilityToolTip;

            if (i === 0 && shifts.length > 0) {
                let divPlanningShift: HTMLDivElement;

                for (let k = 0, l = shifts.length; k < l; k++) {
                    let shift = shifts[k];
                    let isUnwanted: boolean = (shift.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted);
                    let hasWanted: boolean = shift.nbrOfWantedInQueue > 0;
                    let hasAbsenceRequest: boolean = shift.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested && !shift.isAbsenceRequest;
                    let isAbsence: boolean = !!shift.timeDeviationCauseId && !shift.isAbsenceRequest;

                    divPlanningShift = document.createElement('div');
                    divPlanningShift.classList.add('myschedule-schedule-shift');
                    divPlanningShift.style.backgroundColor = shift.shiftTypeColor;

                    if (shift.isAbsenceRequest || shift.isPreliminary) {
                        var divStriped = document.createElement('div');
                        divStriped.classList.add('shift-striped-background');
                        divPlanningShift.appendChild(divStriped);
                    }

                    divPlanningShift.id = shift.timeScheduleTemplateBlockId.toString();
                    divPlanningShift.title = shift.toolTip;
                    shift.availabilityToolTip = availabilityToolTip;

                    var textColor: string = GraphicsUtility.foregroundColorByBackgroundBrightness(!shift.isOnDuty && !shift.isStandby ? shift.shiftTypeColor : '', shift.isAbsence && !shift.isOnDuty && !shift.isStandby);

                    // Time
                    var timeText: string = shift.label1;
                    if (shift.actualStartTime.isBeforeOnMinute(date.beginningOfDay().addMinutes(this.controller.dayViewStartTime)))
                        timeText = '< ' + timeText;
                    if (shift.actualStopTime.isAfterOnMinute(date.beginningOfDay().addMinutes(this.controller.dayViewEndTime)))
                        timeText += ' >';

                    // Time and shift type
                    var spanTimeStart = document.createElement('span');
                    spanTimeStart.classList.add('shift-time-start');
                    spanTimeStart.innerText = timeText;
                    spanTimeStart.style.color = textColor;
                    if (isUnwanted || hasWanted || hasAbsenceRequest || isAbsence)
                        spanTimeStart.style.left = '8px';
                    divPlanningShift.appendChild(spanTimeStart);

                    // Breaks
                    this.addBreaks(shift, divPlanningShift);

                    $(divPlanningShift).css('top', this.shiftMargin);

                    if (isUnwanted || hasWanted || hasAbsenceRequest || isAbsence) {
                        var divStatus = document.createElement('div');
                        if (isUnwanted || hasWanted) {
                            if (hasWanted) {
                                divStatus.classList.add('shift-wanted');
                                if (this.controller.showQueuePermission) {
                                    var spanQueue = document.createElement('span');
                                    spanQueue.classList.add('shift-queue');
                                    spanQueue.innerText = shift.nbrOfWantedInQueue.toString();
                                    divStatus.appendChild(spanQueue);
                                }
                            }
                            if (isUnwanted) {
                                divStatus.classList.add('shift-unwanted');
                            }
                        } else {
                            if (hasAbsenceRequest) {
                                divStatus.classList.add('shift-absence-requested');
                            } else if (isAbsence) {
                                divStatus.classList.add('shift-absence-approved');
                            }
                        }

                        divPlanningShift.appendChild(divStatus);
                    }

                    // Context menu (shift)
                    divPlanningShift.setAttribute('context-menu', "ctrl.getShiftContextMenuOptions(" + shift.timeScheduleTemplateBlockId.toString() + ")");
                    divPlanningShift.setAttribute('context-menu-empty-text', "\' \'");
                    divPlanningShift.setAttribute('model', shift.timeScheduleTemplateBlockId.toString());
                    this.$compile(divPlanningShift)(this.$scope);

                    td.appendChild(divPlanningShift);
                }
            }

            row.appendChild(td);
        }

        if (this.controller.enableAvailibility) {
            // Availability icon
            var availTd = document.createElement('td');
            availTd.classList.add('myschedule-schedule-day');
            availTd.classList.add('iconcolumn');

            // Saturday/Sunday
            if (isSaturday)
                availTd.classList.add('saturday');
            else if (isSunday)
                availTd.classList.add('sunday');

            // Sunday
            if (isSunday)
                availTd.classList.add('myschedule-schedule-horizontal-separator');

            var availIcon = document.createElement('i');
            availIcon.classList.add('fal');
            availIcon.classList.add('fa-calendar-check');
            availIcon.setAttribute('title', "{0} {1}".format(this.controller.terms["common.dashboard.myschedule.availability.edit"], date.toFormattedDate()));
            availIcon.setAttribute('data-ng-click', '$event.stopPropagation(); ctrl.editAvailability(' + date.toPipedDate() + ');');
            this.$compile(availIcon)(this.$scope);
            availTd.appendChild(availIcon);
            row.appendChild(availTd);
        }

        return row;
    }

    private renderOpenShiftsRows(attachmentPoint, date: Date) {
        var openShifts: ShiftDTO[] = this.controller.getOpenShifts(date);

        // Open shifts must be separated on its own rows.
        // Linked shifts should be on the same row.
        let group = _.groupBy(openShifts, s => s.link);
        let links: string[] = Object.keys(group);

        let emps: any[] = [];
        for (let i = 0, j = links.length; i < j; i++) {
            let shifts = group[links[i]];
            emps.push({ employeeId: links[i], startTime: shifts[0].actualStartTime, stopTime: shifts[0].actualStopTime, shifts: shifts })
        };
        // Special to make shifts order by time
        emps = _.orderBy(emps, ['startTime', 'stopTime']);
        for (let i = 0, j = emps.length; i < j; i++) {
            let employeeRow = this.renderOpenShiftsRow(date, emps[i].shifts, i === 0, i === emps.length - 1);
            attachmentPoint.append(employeeRow);
        };
    }

    private renderOpenShiftsRow(date: Date, shifts: ShiftDTO[], isFirstRow: boolean, isLastRow: boolean) {
        var row: HTMLTableRowElement = document.createElement('tr');
        row.classList.add('myschedule-schedule-row');
        row.setAttribute('row-date', date.toPipedDate());
        $(row).css('height', this.shiftHeight);
        row.style.fontStyle = 'italic';

        var rowLabelTd: HTMLTableDataCellElement = document.createElement('td');
        rowLabelTd.classList.add('myschedule-schedule-rowlabel');

        if (isFirstRow)
            rowLabelTd.classList.add('myschedule-schedule-horizontal-separator-top');
        if (isLastRow)
            rowLabelTd.classList.add('myschedule-schedule-horizontal-separator');

        var labelSpan = document.createElement('span');
        labelSpan.classList.add('subrow');
        labelSpan.innerText = shifts[0].employeeName;
        rowLabelTd.appendChild(labelSpan);

        row.appendChild(rowLabelTd);

        for (let i = 0, j = this.controller.timeSlots.length; i < j; i++) {
            var td = document.createElement('td');
            td.classList.add('myschedule-schedule-day');
            td.classList.add('subrow');

            if (isFirstRow)
                td.classList.add('myschedule-schedule-horizontal-separator-top');
            if (isLastRow)
                td.classList.add('myschedule-schedule-horizontal-separator');

            if (i === 0 && shifts.length > 0) {
                let divPlanningShift: HTMLDivElement;

                for (let k = 0, l = shifts.length; k < l; k++) {
                    let shift = shifts[k];
                    let isUnwanted: boolean = (shift.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted);
                    let hasWanted: boolean = shift.nbrOfWantedInQueue > 0;

                    divPlanningShift = document.createElement('div');
                    divPlanningShift.classList.add('myschedule-schedule-shift');
                    divPlanningShift.style.backgroundColor = shift.shiftTypeColor;
                    GraphicsUtility.fadeBackground(divPlanningShift, 0.65);

                    if (shift.isPreliminary) {
                        var divStriped = document.createElement('div');
                        divStriped.classList.add('shift-striped-background');
                        divPlanningShift.appendChild(divStriped);
                    }

                    divPlanningShift.id = shift.timeScheduleTemplateBlockId.toString();
                    divPlanningShift.title = shift.toolTip;

                    var textColor: string = GraphicsUtility.foregroundColorByBackgroundBrightness(!shift.isOnDuty && !shift.isStandby ? shift.shiftTypeColor : '', shift.isAbsence && !shift.isOnDuty && !shift.isStandby);

                    // Time
                    var timeText: string = shift.label1;
                    if (shift.actualStartTime.isBeforeOnMinute(date.beginningOfDay().addMinutes(this.controller.dayViewStartTime)))
                        timeText = '< ' + timeText;
                    if (shift.actualStopTime.isAfterOnMinute(date.beginningOfDay().addMinutes(this.controller.dayViewEndTime)))
                        timeText += ' >';

                    // Time and shift type
                    var spanTimeStart = document.createElement('span');
                    spanTimeStart.classList.add('shift-time-start');
                    spanTimeStart.innerText = timeText;
                    spanTimeStart.style.color = textColor;
                    if (hasWanted)
                        spanTimeStart.style.left = '8px';
                    divPlanningShift.appendChild(spanTimeStart);

                    // Breaks
                    this.addBreaks(shift, divPlanningShift);

                    $(divPlanningShift).css('top', this.shiftMargin);

                    if (isUnwanted || hasWanted) {
                        var divStatus = document.createElement('div');
                        if (hasWanted) {
                            divStatus.classList.add('shift-wanted');
                            if (this.controller.showQueuePermission) {
                                var spanQueue = document.createElement('span');
                                spanQueue.classList.add('shift-queue');
                                spanQueue.innerText = shift.nbrOfWantedInQueue.toString();
                                divStatus.appendChild(spanQueue);
                            }
                        }
                        if (isUnwanted) {
                            divStatus.classList.add('shift-unwanted');
                        }

                        divPlanningShift.appendChild(divStatus);
                    }

                    // Context menu (shift)
                    divPlanningShift.setAttribute('context-menu', "ctrl.getShiftContextMenuOptions(" + shift.timeScheduleTemplateBlockId.toString() + ")");
                    divPlanningShift.setAttribute('context-menu-empty-text', "\' \'");
                    divPlanningShift.setAttribute('model', shift.timeScheduleTemplateBlockId.toString());
                    this.$compile(divPlanningShift)(this.$scope);

                    td.appendChild(divPlanningShift);
                }
            }

            row.appendChild(td);
        }

        // Availability placeholder (in my shifts)
        if (this.controller.enableAvailibility) {
            var availTd = document.createElement('td');
            availTd.classList.add('myschedule-schedule-day');
            availTd.classList.add('subrow');
            if (isFirstRow)
                availTd.classList.add('myschedule-schedule-horizontal-separator-top');
            if (isLastRow)
                availTd.classList.add('myschedule-schedule-horizontal-separator');
            row.appendChild(availTd);
        }

        return row;
    }

    private renderColleaguesShiftsRows(attachmentPoint, date: Date) {
        var colleaguesShifts: ShiftDTO[] = _.orderBy(this.controller.getColleaguesShifts(date), ['actuatStartTime', 'actualStopTime']);

        let group = _.groupBy(colleaguesShifts, s => s.employeeId);
        let employeeIds: string[] = Object.keys(group);

        let emps: any[] = [];
        for (let i = 0, j = employeeIds.length; i < j; i++) {
            let shifts = group[employeeIds[i]];
            emps.push({ employeeId: employeeIds[i], startTime: shifts[0].actualStartTime, stopTime: shifts[0].actualStopTime, shifts: shifts })
        };
        // Special to make shifts order by time
        emps = _.orderBy(emps, ['startTime', 'stopTime']);
        for (let i = 0, j = emps.length; i < j; i++) {
            let employeeRow = this.renderColleaguesShiftsRow(date, emps[i].shifts, i === 0, i === emps.length - 1);
            attachmentPoint.append(employeeRow);
        };
    }

    private renderColleaguesShiftsRow(date: Date, shifts: ShiftDTO[], isFirstRow: boolean, isLastRow: boolean) {
        var row: HTMLTableRowElement = document.createElement('tr');
        row.classList.add('myschedule-schedule-row');
        row.setAttribute('row-date', date.toPipedDate());
        $(row).css('height', this.shiftHeight);
        row.style.fontStyle = 'italic';

        var rowLabelTd: HTMLTableDataCellElement = document.createElement('td');
        rowLabelTd.classList.add('myschedule-schedule-rowlabel');

        if (isFirstRow)
            rowLabelTd.classList.add('myschedule-schedule-horizontal-separator-top');
        if (isLastRow)
            rowLabelTd.classList.add('myschedule-schedule-horizontal-separator');

        var labelSpan = document.createElement('span');
        labelSpan.classList.add('subrow');
        labelSpan.innerText = shifts[0].employeeName;
        rowLabelTd.appendChild(labelSpan);

        row.appendChild(rowLabelTd);

        for (let i = 0, j = this.controller.timeSlots.length; i < j; i++) {
            var td = document.createElement('td');
            td.classList.add('myschedule-schedule-day');
            td.classList.add('subrow');

            if (isFirstRow)
                td.classList.add('myschedule-schedule-horizontal-separator-top');
            if (isLastRow)
                td.classList.add('myschedule-schedule-horizontal-separator');

            if (i === 0 && shifts.length > 0) {
                let divPlanningShift: HTMLDivElement;

                for (let k = 0, l = shifts.length; k < l; k++) {
                    let shift = shifts[k];
                    let isUnwanted: boolean = (shift.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted);
                    let hasWanted: boolean = shift.nbrOfWantedInQueue > 0;

                    divPlanningShift = document.createElement('div');
                    divPlanningShift.classList.add('myschedule-schedule-shift');
                    divPlanningShift.style.backgroundColor = shift.shiftTypeColor;
                    GraphicsUtility.fadeBackground(divPlanningShift, 0.65);

                    if (shift.isPreliminary) {
                        var divStriped = document.createElement('div');
                        divStriped.classList.add('shift-striped-background');
                        divPlanningShift.appendChild(divStriped);
                    }

                    divPlanningShift.id = shift.timeScheduleTemplateBlockId.toString();
                    divPlanningShift.title = shift.toolTip;

                    var textColor: string = GraphicsUtility.foregroundColorByBackgroundBrightness(!shift.isOnDuty && !shift.isStandby ? shift.shiftTypeColor : '', shift.isAbsence && !shift.isOnDuty && !shift.isStandby);

                    // Time
                    var timeText: string = shift.label1;
                    if (shift.actualStartTime.isBeforeOnMinute(date.beginningOfDay().addMinutes(this.controller.dayViewStartTime)))
                        timeText = '< ' + timeText;
                    if (shift.actualStopTime.isAfterOnMinute(date.beginningOfDay().addMinutes(this.controller.dayViewEndTime)))
                        timeText += ' >';

                    // Time and shift type
                    var spanTimeStart = document.createElement('span');
                    spanTimeStart.classList.add('shift-time-start');
                    spanTimeStart.innerText = timeText;
                    spanTimeStart.style.color = textColor;
                    if (isUnwanted || hasWanted)
                        spanTimeStart.style.left = '8px';
                    divPlanningShift.appendChild(spanTimeStart);

                    // Breaks
                    this.addBreaks(shift, divPlanningShift);

                    $(divPlanningShift).css('top', this.shiftMargin);

                    if (isUnwanted || hasWanted) {
                        var divStatus = document.createElement('div');
                        if (hasWanted) {
                            divStatus.classList.add('shift-wanted');
                            if (this.controller.showQueuePermission) {
                                var spanQueue = document.createElement('span');
                                spanQueue.classList.add('shift-queue');
                                spanQueue.innerText = shift.nbrOfWantedInQueue.toString();
                                divStatus.appendChild(spanQueue);
                            }
                        }
                        if (isUnwanted) {
                            divStatus.classList.add('shift-unwanted');
                        }

                        divPlanningShift.appendChild(divStatus);
                    }

                    // Context menu (shift)
                    divPlanningShift.setAttribute('context-menu', "ctrl.getShiftContextMenuOptions(" + shift.timeScheduleTemplateBlockId.toString() + ")");
                    divPlanningShift.setAttribute('context-menu-empty-text', "\' \'");
                    divPlanningShift.setAttribute('model', shift.timeScheduleTemplateBlockId.toString());
                    this.$compile(divPlanningShift)(this.$scope);

                    td.appendChild(divPlanningShift);
                }
            }

            row.appendChild(td);
        }

        // Availability placeholder (in my shifts)
        if (this.controller.enableAvailibility) {
            var availTd = document.createElement('td');
            availTd.classList.add('myschedule-schedule-day');
            availTd.classList.add('subrow');
            if (isFirstRow)
                availTd.classList.add('myschedule-schedule-horizontal-separator-top');
            if (isLastRow)
                availTd.classList.add('myschedule-schedule-horizontal-separator');
            row.appendChild(availTd);
        }

        return row;
    }

    private getPixelsForTime(time: Date, currentDate: Date): number {
        var pixels: number = 0;

        // Get number of minutes after visible start
        var minutesFromVisibleStart = this.getDifferenceInMinutes(currentDate.beginningOfDay().addMinutes(this.controller.dayViewStartTime), time);
        if (time.isBeforeOnMinute(currentDate.beginningOfDay().addMinutes(this.controller.dayViewStartTime)))
            minutesFromVisibleStart = 0;
        else if (time.isAfterOnMinute(currentDate.beginningOfDay().addMinutes(this.controller.dayViewEndTime)))
            minutesFromVisibleStart = 24 * 60;

        // Get number of complete columns from visible start to the time
        var columns = Math.floor(minutesFromVisibleStart / 60);
        // Get number of minutes after an even tick length (eg: 8:35 gives 5 minutes if interval is 15 minutes)
        var oddMinutes = minutesFromVisibleStart - (columns * 60);

        // Loop each column before time
        for (let i = 0; i < columns && i < this.columnWidths.length; i++) {
            for (let j = 0, k = this.columnWidths.length; j < k; j++) {
                if (this.columnWidths[j].index === i) {
                    pixels += this.columnWidths[j].width;
                }
            }
        }

        // Add odd minutes
        if (oddMinutes > 0) {
            pixels += (this.columnWidths[columns].width / 60) * oddMinutes;
        }

        // Make sure pixels are inside visible range
        if (pixels < 0)
            pixels = 0;
        else if (pixels > this.scheduleWidth)
            pixels = this.scheduleWidth;

        return pixels;
    }

    private updateScheduleRowSizeAndPosition(row: HTMLElement) {
        if (!row)
            return;

        var currentDate: Date = row.getAttribute('row-date').parsePipedDate();

        var shifts = row.getElementsByClassName('myschedule-schedule-shift');
        for (let i = 0; i < shifts.length; i++) {
            let shiftElem: HTMLElement = <HTMLElement>shifts[i];
            let width: number;

            let shift = this.getShiftFromElem(shiftElem);
            if (shift) {
                let left = this.getPixelsForTime(shift.actualStartTime, currentDate);
                this.setElemPosition(shiftElem, left);
                width = this.getPixelsForTime(shift.actualStopTime, currentDate) - left - 1;
                this.setElemWidth(shiftElem, width);

                // Make sure start and stop times does not write over each other
                var stopTimeElem = this.getFirstElementByClassName(shiftElem, 'shift-time-stop');
                if (stopTimeElem)
                    this.setElemWidth(stopTimeElem, width - 25);

                let breaks = shiftElem.getElementsByClassName('shift-break');
                for (let j = 0; j < breaks.length; j++) {
                    let breakElem: HTMLElement = <HTMLElement>breaks[j];
                    let breakData = this.getBreakData(shift, parseInt(breakElem.id, 10));
                    let breakLeft = this.getPixelsForTime(breakData.actualBreakStart, currentDate);
                    this.setElemPosition(breakElem, breakLeft - left - 1); // Break starts relative from its shift, therefore we need to reduce shift start length
                    this.setElemWidth(breakElem, this.getPixelsForTime(breakData.actualBreakStart.addMinutes(breakData.breakMinutes), currentDate) - breakLeft + 1);
                }
            }
        }
    }

    private setElemWidth(elem: HTMLElement, widthInPixels: number) {
        elem.style.width = "{0}px".format(widthInPixels.toString());
    }

    private setElemPosition(elem: HTMLElement, positionInPixels: number) {
        elem.style.left = "{0}px".format(positionInPixels.toString());
    }

    private setWidthOnShiftElement(elem: HTMLElement, width: number): number {
        var shift = this.getShiftFromElem(elem);
        var days = this.getShiftLengthInDays(shift);
        var startDaysBeforeVisibleRange = shift.startTime.isBeforeOnDay(this.controller.dateFrom) ? this.controller.dateFrom.diffDays(shift.startTime) : 0;
        days = days - startDaysBeforeVisibleRange;
        var newWidth: number = (width * days) - 7;  // -7 is to restore the padding
        this.setElemWidth(elem, newWidth);

        return newWidth;
    }

    public columnWidths: any[];
    private updateWidth() {
        var cols = $('.myschedule-schedule tbody tr:first .myschedule-schedule-day');

        this.columnWidths = [];
        cols.each((i, e) => {
            this.columnWidths.push({ index: i, width: $(e).outerWidth() });
        });
    }

    private setScheduleWidth() {
        this.scheduleWidth = $('.myschedule-schedule thead').outerWidth();
    }

    public updateWidthOnAllElements(delay: number = 100) {
        this.$timeout(() => {
            this.updateWidth();
            this.setScheduleWidth();
            $('.myschedule-schedule tbody tr').each((_, row) => this.updateScheduleRowSizeAndPosition(row));
        }, delay);
    }

    public setShiftToolTip(shiftId: number, toolTip: string) {
        var shift = $('#' + shiftId);
        if (shift)
            shift.attr('title', toolTip);
    }

    // HELP-METHODS

    // Html

    private getFirstElementByClassName(elem: Element, className: string): HTMLElement {
        var elems = elem.getElementsByClassName(className);
        if (elems.length > 0)
            return <HTMLElement>elems[0];

        return null;
    }

    // Shifts

    public getShiftFromElem(elem): ShiftDTO {
        return this.controller.getShiftById(parseInt(elem.id, 10));
    }

    public getShiftFromJQueryElem(elem): ShiftDTO {
        return this.controller.getShiftById(parseInt($(elem).attr('id'), 10));
    }

    public getShiftLengthInDays(shift: ShiftDTO) {
        // Get shift actual stop time or end of display if shift goes beyond that
        var actualStop = CalendarUtility.minOfDates(shift.actualStopTime, this.controller.dateTo).beginningOfDay();

        return actualStop.diffDays(shift.actualStartTime.beginningOfDay()) + 1;
    }

    public getDifferenceInMinutes(startDate: Date, endDate: Date) {
        // Get shift actual stop time or end of display if shift goes beyond that
        var actualStop = CalendarUtility.getMinDate(endDate, startDate.beginningOfDay().addMinutes(this.controller.dayViewEndTime));
        // Get shift actual stop time or beginning of display if shift goes beyond that
        actualStop = CalendarUtility.getMaxDate(actualStop, startDate.beginningOfDay().addMinutes(this.controller.dayViewStartTime));

        return actualStop.diffMinutes(startDate);
    }

    public selectShift(shift: ShiftDTO, selectLinked: boolean = true, notify: boolean = false) {
        if (shift) {
            shift.selected = true;
            $('#' + shift.timeScheduleTemplateBlockId).addClass('selected-shift');

            if (selectLinked && shift.link) {
                // Select all linked shifts
                var linkedShifts = _.filter(this.controller.getShiftsOfSameType(shift.timeScheduleTemplateBlockId), s => s.link === shift.link && s.type === shift.type && s.timeScheduleTemplateBlockId !== shift.timeScheduleTemplateBlockId);
                this.selectShifts(linkedShifts, false, false);
            }

            if (notify)
                this.controller.shiftSelected();
        }
    }

    public selectShifts(shifts: ShiftDTO[], selectLinked: boolean = false, notify: boolean = false) {
        _.forEach(shifts, shift => {
            this.selectShift(shift, selectLinked, false);
        });

        if (notify)
            this.controller.shiftSelected();
    }

    public unselectShift(shift: ShiftDTO, unselectLinked: boolean = true, notify: boolean = false) {
        if (shift) {
            shift.selected = false;
            $('#' + shift.timeScheduleTemplateBlockId).removeClass('selected-shift');

            if (unselectLinked) {
                // Unselect all linked shifts
                var linkedShifts = _.filter(this.controller.getShiftsOfSameType(shift.timeScheduleTemplateBlockId), s => s.link === shift.link && s.type === shift.type && s.timeScheduleTemplateBlockId !== shift.timeScheduleTemplateBlockId);
                this.unselectShifts(linkedShifts, false);
            }

            if (notify)
                this.controller.shiftSelected();
        }
    }

    public unselectShifts(shifts: ShiftDTO[], notify: boolean = false) {
        _.forEach(shifts, shift => {
            this.unselectShift(shift, false, false);
        });

        if (notify)
            this.controller.shiftSelected();
    }

    public getSelectedShifts() {
        var selectedShifts: ShiftDTO[] = [];
        if (this.controller.myShifts)
            selectedShifts.push(...this.controller.myShifts.filter(s => s.selected));
        if (this.controller.openShifts)
            selectedShifts.push(...this.controller.openShifts.filter(s => s.selected));
        if (this.controller.colleaguesShifts)
            selectedShifts.push(...this.controller.colleaguesShifts.filter(s => s.selected));

        return selectedShifts;
    }

    public clearSelectedShifts(notify: boolean = false) {
        this.unselectShifts(this.getSelectedShifts());

        if (notify)
            this.controller.shiftSelected();
    }

    // Breaks

    private addBreaks(shift: ShiftDTO, targetElem: HTMLElement) {
        this.addBreak(shift, targetElem, 1);
        this.addBreak(shift, targetElem, 2);
        this.addBreak(shift, targetElem, 3);
        this.addBreak(shift, targetElem, 4);
    }

    private addBreak(shift: ShiftDTO, targetElem: HTMLElement, breakNo: number) {
        if (!shift[`break${breakNo}Id`])
            return;

        var actualBreakStart = this.getBreakStart(shift, breakNo);
        var breakEnd = actualBreakStart.addMinutes(this.getBreakLength(shift, breakNo));

        // Make sure break is within shift (otherwise breaks on shifts hidden by filter will be visible)
        if (CalendarUtility.getIntersectingDuration(shift.actualStartTime, shift.actualStopTime, actualBreakStart, breakEnd) === 0)
            return;

        var span = document.createElement('span');
        span.classList.add('shift-break');
        span.id = shift[`break${breakNo}Id`];

        // Break text
        var textSpan = document.createElement('span');
        textSpan.classList.add('shift-break-text');

        textSpan.innerText = "{0}-{1}".format(actualBreakStart.toFormattedTime(), breakEnd.toFormattedTime());
        span.appendChild(textSpan);

        // Break tooltip
        var toolTip = this.controller.getBreakToolTip(shift, breakNo);
        span.title = "{0}-{1}  {2}".format(actualBreakStart.toFormattedTime(), breakEnd.toFormattedTime(), toolTip);

        targetElem.appendChild(span);
    }

    public getBreakData(shift: ShiftDTO, breakId: number) {
        var breakNo = this.getBreakNo(shift, breakId);

        return { breakMinutes: this.getBreakLength(shift, breakNo), actualBreakStart: this.getBreakStart(shift, breakNo) };
    }

    public getBreakNo(shift: ShiftDTO, breakId: number): number {
        var nbr: number;
        [1, 2, 3, 4].forEach(no => {
            if (shift[`break${no}Id`] == breakId) {
                nbr = no;
            }
        });

        return nbr;
    }

    public getBreakStart(shift: ShiftDTO, breakNo: number): Date {
        var start: Date = new Date(shift.actualStartTime);
        start.setHours((<Date>shift[`break${breakNo}StartTime`]).getHours());
        start.setMinutes((<Date>shift[`break${breakNo}StartTime`]).getMinutes());

        return start;
    }

    public getBreakLength(shift: ShiftDTO, breakNo: number): number {
        return shift[`break${breakNo}Minutes`];
    }
}