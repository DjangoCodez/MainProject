import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ShiftDTO } from "../../../Common/Models/TimeSchedulePlanningDTOs";
import { GraphicsUtility } from "../../../Util/GraphicsUtility";
import { ScheduleController } from "./ScheduleDirective";
import { TimeAttestMode } from "../../../Util/CommonEnumerations";
import { TimeAttestTimeStampDTO } from "../../../Common/Models/TimeAttestDTOs";
import { AttestEmployeeDayTimeBlockDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { DragDropHelper } from "./DragDropHelper";
import { ProjectTimeBlockDTO } from "../../../Common/Models/ProjectDTO";

export class ScheduleHandler {
    private dragDropHelper: DragDropHelper;
    private columnWidth: number;
    private scheduleWidth: number;
    public pixelsPerTimeUnit: number;

    private shiftHeight: number = 20;
    private shiftMargin: number = 0;

    // Flags
    private firstTimeScheduleSetup: boolean = true;

    constructor(private controller: ScheduleController, private $filter: ng.IFilterService, private $timeout: ng.ITimeoutService, private $q: ng.IQService, private $scope: ng.IScope, private $compile: ng.ICompileService) {
        this.dragDropHelper = new DragDropHelper(controller, this, $filter);

        this.renderScheduleRow = this.renderScheduleRow.bind(this);
    }

    // SETUP

    private performFirstTimeSetup() {
        this.firstTimeScheduleSetup = false;
        var self = this;

        // Double click on shift
        $('.timeattest-schedule').on('dblclick', '.timeattest-schedule-shift', function (event) {
            var shift = self.getShiftFromElem(this);
            if (shift && !self.controller.timeStampIsModified && !self.controller.timeBlocksIsModified)
                self.controller.editShift(shift);
        });

        // Single click on time stamp
        $('.timeattest-schedule').on('click', '.timeattest-schedule-timestamp', function (event) {
            var timeStamp = self.getTimeStampFromElem(this);
            if (timeStamp.selected)
                self.unselectTimeStamp(timeStamp);
            else
                self.selectTimeStamp(timeStamp, true);
        });

        // Double click on time block
        $('.timeattest-schedule').on('dblclick', '.timeattest-schedule-timeblock', function (event) {          
            var timeBlock = self.getTimeBlockFromElem(this);
            if (timeBlock && self.controller.isAutogenTimeblocks)
                self.controller.editTimeBlock(timeBlock, null);
        });

        // Resize window
        $(window).resize((e) => {
            // This event is also fired when a shift is dragged or resized.
            // Do not run updateWidthOnAllElements() in those cases.
            var isResizebleElement = $(e.target).hasClass('ui-resizable');
            if (!isResizebleElement) {
                this.renderSchedule(false);
            }
        });
    }

    // RENDER

    private getTableAttachmentPoint() {
        return $('.timeattest-schedule #{0} tbody'.format(this.controller.tableId));
    }

    public clearScheduleViewBody(): ng.IPromise<any> {
        return this.$timeout(() => {
            var attachmentPoint = this.getTableAttachmentPoint();
            attachmentPoint.empty();
        });
    }

    public renderSchedule(useDelay: boolean) {
        this.renderScheduleBody(useDelay);
        if (this.firstTimeScheduleSetup)
            this.performFirstTimeSetup();
    }

    private renderScheduleBody(useDelay: boolean) {
        this.clearScheduleViewBody().then(() => {
            var attachmentPoint = this.getTableAttachmentPoint();

            this.clearSelectedTimeStamps();

            let sr = this.renderScheduleRow();
            attachmentPoint.append(sr);

            let tsr;
            if (this.controller.renderTimeStamps) {
                tsr = this.renderTimeStampRow();
                attachmentPoint.append(tsr);
            }

            let ptbr;
            if (this.controller.renderProjectTimeblocks) {
                ptbr = this.renderProjectTimeBlockRow();
                attachmentPoint.append(ptbr);
            }

            let tbr = this.renderTimeBlockRow();
            attachmentPoint.append(tbr);

            this.$timeout(() => {
                this.setScheduleWidth();
                this.updateWidth();

                this.updateScheduleRowSizeAndPosition(sr);
                if (this.controller.renderTimeStamps && tsr)
                    this.updateTimeStampRowSizeAndPosition(tsr);

                if (this.controller.renderProjectTimeblocks && ptbr)
                    this.updateProjectTimeBlockRowSizeAndPosition(ptbr)

                this.updateTimeBlockRowSizeAndPosition(tbr);
            }, useDelay ? 500 : 0);
        });
    }

    private renderScheduleRow() {
        let row: HTMLTableRowElement = document.createElement('tr');
        row.classList.add('timeattest-schedule-schedulerow');
        $(row).css('height', this.shiftHeight);

        let dateShifts = [];
        let dateDay = this.controller.dates[0];
        if (dateDay) {
            dateShifts = this.controller.dates.map(d => ({ date: d.date, shifts: [] }));
            let shifts: ShiftDTO[] = this.controller.getShifts(dateDay.date);
            dateShifts[0].shifts = shifts;
        }

        for (let i = 0, j = dateShifts.length; i < j; i++) {
            let ds = dateShifts[i];
            if (!ds.shifts)
                break;

            ds.shifts = ds.shifts.map(s => {
                let obj = new ShiftDTO();
                angular.extend(obj, s);
                return obj;
            });
        }

        let rowLabelTd: HTMLTableDataCellElement = document.createElement('td');
        rowLabelTd.classList.add('timeattest-schedule-rowlabel');

        let labelSpan = document.createElement('span');
        labelSpan.classList.add('rowlabel');
        labelSpan.innerText = this.controller.terms["time.time.attest.schedule"];
        rowLabelTd.appendChild(labelSpan);
        row.appendChild(rowLabelTd);

        for (let i = 0, j = dateShifts.length; i < j; i++) {
            let ds = dateShifts[i];

            let td = document.createElement('td');
            td.classList.add('timeattest-schedule-day');

            let divShiftDay = document.createElement('div');
            divShiftDay.classList.add('shift-day');
            td.appendChild(divShiftDay);

            let shifts: ShiftDTO[] = ds.shifts;
            if (shifts && shifts.length) {
                let divPlanningShift: HTMLDivElement;

                let lastShiftId: number = _.orderBy(shifts, s => s.actualStopTime, 'desc')[0].timeScheduleTemplateBlockId;

                for (let k = 0, l = shifts.length; k < l; k++) {
                    let shift = shifts[k];

                    divPlanningShift = document.createElement('div');
                    divPlanningShift.classList.add('timeattest-schedule-shift');
                    if (shift.isAbsenceRequest || shift.isAbsence)
                        shift.shiftTypeColor = "#ef545e";   // @shiftAbsenceBackgroundColor
                    divPlanningShift.style.backgroundColor = shift.shiftTypeColor;

                    if (shift.isAbsenceRequest || shift.isPreliminary || shift.isStandby || shift.isOnDuty) {
                        let divStriped = document.createElement('div');
                        if(shift.isStandby)
                            divStriped.classList.add('shift-standby-background');
                        else if (shift.isOnDuty)
                            divStriped.classList.add('shift-onduty-background');
                        else
                            divStriped.classList.add('shift-striped-background');
                        divPlanningShift.appendChild(divStriped);
                    }

                    divPlanningShift.id = shift.timeScheduleTemplateBlockId.toString();
                    divPlanningShift.title = shift.toolTip;

                    let textColor: string = GraphicsUtility.foregroundColorByBackgroundBrightness(!shift.isOnDuty && !shift.isStandby ? shift.shiftTypeColor : '', shift.isAbsence && !shift.isOnDuty && !shift.isStandby);

                    if (k === 0 || shift.timeDeviationCauseName) {
                        // Start time only on first shift
                        let spanTimeStart = document.createElement('span');
                        spanTimeStart.classList.add('shift-time-start');
                        let timeText: string = '';
                        if (shift.timeDeviationCauseName)
                            timeText += shift.timeDeviationCauseName + ' ';
                        if (k === 0)
                            timeText += shift.actualStartTime.toFormattedTime()
                        spanTimeStart.innerText = timeText;
                        spanTimeStart.style.color = textColor;
                        divPlanningShift.appendChild(spanTimeStart);
                    }

                    if (shift.timeScheduleTemplateBlockId === lastShiftId) {
                        // Stop time only on last shift
                        let spanTimeStop = document.createElement('span');
                        spanTimeStop.classList.add('shift-time-stop');
                        spanTimeStop.innerText = shift.actualStopTime.toFormattedTime();
                        spanTimeStop.style.color = textColor;
                        divPlanningShift.appendChild(spanTimeStop);
                    }

                    this.addBreaks(shift, divPlanningShift);

                    $(divPlanningShift).css('top', this.shiftMargin);
                    $(divPlanningShift).css('left', -1000);

                    divShiftDay.appendChild(divPlanningShift);
                }
            }

            row.appendChild(td);
        }

        // Edit schedule icon
        let rowIconTd: HTMLTableDataCellElement = document.createElement('td');
        rowIconTd.classList.add('timeattest-schedule-day-button');
        if (this.controller.editShiftPermission) {
            let rowIcon = document.createElement('i');
            rowIcon.classList.add('fal');
            rowIcon.classList.add('fa-pencil');
            if (this.controller.timeStampIsModified || this.controller.timeBlocksIsModified) {
                rowIcon.classList.add('disabledTextColor');
            } else {
                rowIcon.classList.add('iconEdit');
                rowIcon.setAttribute('title', this.controller.terms["time.time.attest.schedule.edit"]);
                rowIcon.setAttribute('data-ng-click', '$event.stopPropagation(); ctrl.editShift(null);');
                this.$compile(rowIcon)(this.$scope);
            }
            rowIconTd.appendChild(rowIcon);
        }
        row.appendChild(rowIconTd);

        return row;
    }

    private renderTimeStampRow() {
        var row: HTMLTableRowElement = document.createElement('tr');
        row.classList.add('timeattest-schedule-timestamprow');
        $(row).css('height', this.shiftHeight);

        var rowLabelTd: HTMLTableDataCellElement = document.createElement('td');
        rowLabelTd.classList.add('timeattest-schedule-rowlabel');
        rowLabelTd.setAttribute('data-ng-click', '$event.stopPropagation(); ctrl.toggleTimeStamp();');
        rowLabelTd.classList.add('link');

        var labelIcon = document.createElement('i');
        labelIcon.classList.add('far');
        labelIcon.classList.add('fa-fw');
        labelIcon.classList.add('margin-small-right');
        labelIcon.setAttribute('data-ng-class', "{\'fa-chevron-down\': ctrl.showTimeStamp, \'fa-chevron-right\': !ctrl.showTimeStamp}");
        rowLabelTd.appendChild(labelIcon);

        var labelSpan = document.createElement('span');
        labelSpan.classList.add('rowlabel');
        if (this.controller.hasTimeStampErrors)
            labelSpan.classList.add('has-errors');
        else if (this.controller.hasTimeStampWarnings)
            labelSpan.classList.add('has-warnings');
        labelSpan.innerText = this.controller.terms["time.time.attest.timestamps"];
        rowLabelTd.appendChild(labelSpan);
        this.$compile(rowLabelTd)(this.$scope);
        row.appendChild(rowLabelTd);

        var dateTimeStamps = [];
        let dateDay = this.controller.dates[0];
        if (dateDay) {
            dateTimeStamps = this.controller.dates.map(d => ({ date: d.date, timeStamps: [] }));
            let timeStamps: TimeAttestTimeStampDTO[] = this.controller.getTimeStamps(dateDay.date);
            dateTimeStamps[0].timeStamps = timeStamps;
        }

        for (let i = 0, j = dateTimeStamps.length; i < j; i++) {
            let dts = dateTimeStamps[i];

            var td = document.createElement('td');
            td.classList.add('timeattest-schedule-day');

            var divShiftDay = document.createElement('div');
            divShiftDay.classList.add('shift-day');
            td.appendChild(divShiftDay);

            let timeStamps: TimeAttestTimeStampDTO[] = dts.timeStamps;
            if (timeStamps && timeStamps.length) {
                let divPlanningShift: HTMLDivElement;

                for (let k = 0, l = timeStamps.length; k < l; k++) {
                    let timeStamp = timeStamps[k];

                    divPlanningShift = document.createElement('div');
                    divPlanningShift.id = "ts" + timeStamp.timeStampEntryId.toString();
                    divPlanningShift.classList.add('timeattest-schedule-timestamp');

                    divPlanningShift.title = timeStamp.toolTip;

                    var spanTimeStart = document.createElement('span');
                    spanTimeStart.classList.add('shift-time-start');
                    spanTimeStart.innerText = timeStamp.stampIn.toFormattedTime();
                    divPlanningShift.appendChild(spanTimeStart);

                    var spanTimeStop = document.createElement('span');
                    spanTimeStop.classList.add('shift-time-stop');
                    spanTimeStop.innerText = timeStamp.stampOut.toFormattedTime();
                    divPlanningShift.appendChild(spanTimeStop);

                    $(divPlanningShift).css('top', this.shiftMargin);
                    $(divPlanningShift).css('left', -1000);

                    divShiftDay.appendChild(divPlanningShift);
                }
            }

            row.appendChild(td);
        }

        var rowIconTd: HTMLTableDataCellElement = document.createElement('td');
        rowIconTd.classList.add('timeattest-schedule-day-button');
        row.appendChild(rowIconTd);

        return row;
    }

    private renderProjectTimeBlockRow() {
        var row: HTMLTableRowElement = document.createElement('tr');
        row.classList.add('timeattest-schedule-projecttimeblockrow');
        $(row).css('height', this.shiftHeight);

        var rowLabelTd: HTMLTableDataCellElement = document.createElement('td');
        rowLabelTd.classList.add('timeattest-schedule-rowlabel');
        rowLabelTd.setAttribute('data-ng-click', '$event.stopPropagation(); ctrl.toggleProjectTimeBlock();');
        rowLabelTd.classList.add('link');

        var labelIcon = document.createElement('i');
        labelIcon.classList.add('far');
        labelIcon.classList.add('fa-fw');
        labelIcon.classList.add('margin-small-right');
        labelIcon.setAttribute('data-ng-class', "{\'fa-chevron-down\': ctrl.showProjectTimeBlock, \'fa-chevron-right\': !ctrl.showProjectTimeBlock}");
        rowLabelTd.appendChild(labelIcon);

        var labelSpan = document.createElement('span');
        labelSpan.classList.add('rowlabel');

        labelSpan.innerText = this.controller.terms["time.time.attest.registeredtime"];
        rowLabelTd.appendChild(labelSpan);
        this.$compile(rowLabelTd)(this.$scope);
        row.appendChild(rowLabelTd);

        var dateProjectTimeBlocks = [];
        let dateDay = this.controller.dates[0];
        if (dateDay) {
            dateProjectTimeBlocks = this.controller.dates.map(date => ({ date: date.date, timeStamps: [] }));
            let projectTimeBlocks: ProjectTimeBlockDTO[] = this.controller.getProjectTimeBlocks(dateDay.date);
            dateProjectTimeBlocks[0].projectTimeBlocks = projectTimeBlocks;
        }

        for (let i = 0, j = dateProjectTimeBlocks.length; i < j; i++) {
            let dts = dateProjectTimeBlocks[i];

            var td = document.createElement('td');
            td.classList.add('timeattest-schedule-day');

            var divShiftDay = document.createElement('div');
            divShiftDay.classList.add('shift-day');
            td.appendChild(divShiftDay);

            let projectTimeBlocks: ProjectTimeBlockDTO[] = dts.projectTimeBlocks;
            if (projectTimeBlocks && projectTimeBlocks.length) {
                let divPlanningShift: HTMLDivElement;

                for (let k = 0, l = projectTimeBlocks.length; k < l; k++) {
                    let projectTimeBlock = projectTimeBlocks[k];

                    divPlanningShift = document.createElement('div');
                    divPlanningShift.id = "ts" + projectTimeBlock.projectTimeBlockId.toString();
                    divPlanningShift.classList.add('timeattest-schedule-projecttimeblock');

                    divPlanningShift.title = projectTimeBlock.projectName + " " + projectTimeBlock.invoiceNr;

                    var spanTimeStart = document.createElement('span');
                    spanTimeStart.classList.add('shift-time-start');
                    spanTimeStart.innerText = projectTimeBlock.startTime.toFormattedTime();
                    divPlanningShift.appendChild(spanTimeStart);

                    var spanTimeStop = document.createElement('span');
                    spanTimeStop.classList.add('shift-time-stop');
                    spanTimeStop.innerText = projectTimeBlock.stopTime.toFormattedTime();
                    divPlanningShift.appendChild(spanTimeStop);

                    $(divPlanningShift).css('top', this.shiftMargin);
                    $(divPlanningShift).css('left', -1000);

                    divShiftDay.appendChild(divPlanningShift);
                }
            }

            row.appendChild(td);
        }

        var rowIconTd: HTMLTableDataCellElement = document.createElement('td');
        rowIconTd.classList.add('timeattest-schedule-day-button');
        row.appendChild(rowIconTd);

        return row;
    }

    private renderTimeBlockRow() {
        var row: HTMLTableRowElement = document.createElement('tr');
        row.classList.add('timeattest-schedule-timeblockrow');
        $(row).css('height', this.shiftHeight);

        var rowLabelTd: HTMLTableDataCellElement = document.createElement('td');
        rowLabelTd.classList.add('timeattest-schedule-rowlabel');
        rowLabelTd.setAttribute('data-ng-click', '$event.stopPropagation(); ctrl.toggleTimeBlock();');
        rowLabelTd.classList.add('link');

        var labelIcon = document.createElement('i');
        labelIcon.classList.add('far');
        labelIcon.classList.add('fa-fw');
        labelIcon.classList.add('margin-small-right');
        labelIcon.setAttribute('data-ng-class', "{\'fa-chevron-down\': ctrl.showTransaction, \'fa-chevron-right\': !ctrl.showTransaction}");
        rowLabelTd.appendChild(labelIcon);

        var labelSpan = document.createElement('span');
        labelSpan.classList.add('rowlabel');
        labelSpan.innerText = this.controller.terms["time.time.attest.timeblocks"];
        if (this.controller.timeStampIsModified) {
            labelSpan.classList.add('has-warnings');
            rowLabelTd.title = this.controller.terms["time.time.attest.timeblocks.savetocalculate"];
        }
        rowLabelTd.appendChild(labelSpan);
        this.$compile(rowLabelTd)(this.$scope);
        row.appendChild(rowLabelTd);

        this.renderTimeBlockRowContent(row);

        return row;
    }

    private renderTimeBlockRowContent(row) {
        var dateTimeBlocks = this.controller.dates.map(d => ({ date: d.date, timeBlocks: [] }));
        if (dateTimeBlocks.length > 0) {
            let timeBlocks: AttestEmployeeDayTimeBlockDTO[] = this.controller.getTimeBlocks(this.controller.dates[0].date, _.last(this.controller.dates).date);
            if (timeBlocks.length > 0)
                dateTimeBlocks[0].timeBlocks = timeBlocks;
        }

        for (let i = 0, j = dateTimeBlocks.length; i < j; i++) {
            let dtb = dateTimeBlocks[i];

            var td = document.createElement('td');
            td.classList.add('timeattest-schedule-day');

            var divShiftDay = document.createElement('div');
            divShiftDay.classList.add('shift-day');
            td.appendChild(divShiftDay);

            var divDropZone = document.createElement('div');
            divDropZone.classList.add('timeattest-shift-drop-zone');
            divShiftDay.appendChild(divDropZone);

            let timeBlocks: AttestEmployeeDayTimeBlockDTO[] = dtb.timeBlocks;
            if (timeBlocks && timeBlocks.length) {
                let divPlanningShift: HTMLDivElement;

                for (let k = 0, l = timeBlocks.length; k < l; k++) {
                    let timeBlock = timeBlocks[k];

                    divPlanningShift = document.createElement('div');
                    divPlanningShift.id = "tb" + timeBlock.guidId;
                    divPlanningShift.classList.add('timeattest-schedule-timeblock');
                    if (this.controller.timeStampIsModified)
                        divPlanningShift.classList.add('modified-stamps');

                    if (timeBlock.isBreak)
                        divPlanningShift.classList.add('break');
                    else if (timeBlock.isAbsence)
                        divPlanningShift.classList.add('absence');
                    else if (timeBlock.isOvertime)
                        divPlanningShift.classList.add('overtime');
                    else if (timeBlock.isPresence)
                        divPlanningShift.classList.add('presence');
                    else //if (timeBlock.isOutsideScheduleNotOvertime)
                        divPlanningShift.classList.add('outsideschedule');

                    if (timeBlock.isAbsence) {
                        let spanText = document.createElement('span');
                        spanText.innerText = timeBlock.timeDeviationCauseName;
                        divPlanningShift.appendChild(spanText);
                    } else if (timeBlock.isBreak && timeBlock.timeCodes && timeBlock.timeCodes.length > 0) {
                        let spanText = document.createElement('span');
                        spanText.innerText = _.map(timeBlock.timeCodes, c => c.code).join(', ');
                        divPlanningShift.appendChild(spanText);
                    }

                    divPlanningShift.title = timeBlock.toolTip;
                    
                    // Set classes for drag n drop handles
                    if (this.controller.isAutogenTimeblocks && soeConfig.attestMode !== TimeAttestMode.Project) {
                        // Only first block on day or first block after a hole can be dragged left
                        if (!timeBlock.isReadonlyLeft && (k === 0 || timeBlock.startTime >= timeBlocks[k - 1].stopTime.addMinutes(5)))
                            divPlanningShift.classList.add('planning-shift-resize-start');
                        if (!timeBlock.isReadonlyRight)
                            divPlanningShift.classList.add('planning-shift-resize-stop');
                    }

                    $(divPlanningShift).css('top', this.shiftMargin);
                    $(divPlanningShift).css('left', -1000);

                    divDropZone.appendChild(divPlanningShift);
                }
            }

            row.appendChild(td);
        }

        // Add time block icon
        var rowIconTd: HTMLTableDataCellElement = document.createElement('td');
        if (!this.controller.model.isReadonly && this.controller.model.autogenTimeblocks) {
            rowIconTd.classList.add('timeattest-schedule-day-button');
            var rowIcon = document.createElement('i');
            rowIcon.classList.add('fal');
            rowIcon.classList.add('fa-plus');
            rowIcon.setAttribute('title', this.controller.terms["time.time.attest.timeblocks.add"]);
            rowIcon.setAttribute('data-ng-click', '$event.stopPropagation(); ctrl.editTimeBlock(null);');
            this.$compile(rowIcon)(this.$scope);
            rowIconTd.appendChild(rowIcon);
        }
        row.appendChild(rowIconTd);

        this.dragDropHelper.setDragDropOptionsForRow(row);
    }

    public updateTimeBlockRow(row: HTMLTableRowElement) {
        if (!row || !row.cells)
            return;

        while (row.cells.length > 1) {
            row.deleteCell(1);
        }

        this.renderTimeBlockRowContent(row);
        this.updateTimeBlockRowSizeAndPosition(row);
    }

    private getPixelsForTime(time: Date): number {
        let pixels: number = 0;

        // Get number of minutes after visible start
        let minutesFromVisibleStart = this.getDifferenceInMinutes(this.controller.dates[0].date, time);
        // Get number of complete columns from visible start to the time
        let columns = Math.floor(minutesFromVisibleStart / 60);
        // Get number of minutes after an even tick length (eg: 8:35 gives 5 minutes if interval is 15 minutes)
        let oddMinutes = minutesFromVisibleStart - (columns * 60);

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
            let column = this.columnWidths[columns];
            if (column)
                pixels += (column.width / 60) * oddMinutes;
        }

        // Make sure pixels are inside visible range
        if (pixels < 0)
            pixels = 0;
        else if (pixels > this.scheduleWidth)
            pixels = this.scheduleWidth;

        return pixels;
    }

    private updateScheduleRowSizeAndPosition(row) {
        if (!row)
            return;

        let shifts = row.getElementsByClassName('timeattest-schedule-shift');
        for (let shiftElem of shifts) {
            let shift = this.getShiftFromElem(shiftElem);
            if (shift) {
                let left = this.getPixelsForTime(shift.actualStartTime);
                this.setElemPosition(shiftElem, left);
                let width: number = this.getPixelsForTime(shift.actualStopTime) - left - 1;
                this.setElemWidth(shiftElem, width);

                // Make sure start and stop times does not write over each other
                let stopTimeElem = this.getFirstElementByClassName(shiftElem, 'shift-time-stop');
                if (stopTimeElem)
                    this.setElemWidth(stopTimeElem, width - 25);

                let breaks = shiftElem.getElementsByClassName('shift-break');
                for (let breakElem of breaks) {
                    let breakData = this.getBreakData(shift, parseInt(breakElem.id, 10));
                    let breakLeft = this.getPixelsForTime(breakData.actualBreakStart);
                    this.setElemPosition(breakElem, breakLeft - left - 1); // Break starts relative from its shift, therefore we need to reduce shift start length
                    this.setElemWidth(breakElem, this.getPixelsForTime(breakData.actualBreakStart.addMinutes(breakData.breakMinutes)) - breakLeft + 1);
                }
            }
        }
    }

    private updateTimeStampRowSizeAndPosition(row) {
        if (!row)
            return;

        let timeStamps = row.getElementsByClassName('timeattest-schedule-timestamp');
        for (let timeStampElem of timeStamps) {
            let timeStamp = this.getTimeStampFromElem(timeStampElem);
            if (timeStamp) {
                let left = this.getPixelsForTime(timeStamp.stampIn);

                this.setElemPosition(timeStampElem, left);
                let width: number = this.getPixelsForTime(timeStamp.stampOut) - left - 1;
                this.setElemWidth(timeStampElem, width);

                // Make sure start and stop times does not write over each other
                this.setElemWidth(this.getFirstElementByClassName(timeStampElem, 'shift-time-stop'), width - 25);
            }
        }
    }

    private updateTimeBlockRowSizeAndPosition(row) {
        if (!row)
            return;

        let timeBlocks = row.getElementsByClassName('timeattest-schedule-timeblock');
        for (let timeBlockElem of timeBlocks) {
            let timeBlock = this.getTimeBlockFromElem(timeBlockElem);
            if (timeBlock) {
                let left = this.getPixelsForTime(timeBlock.startTime);

                this.setElemPosition(timeBlockElem, left);
                let width: number = this.getPixelsForTime(timeBlock.stopTime) - left - 1;
                this.setElemWidth(timeBlockElem, width);

                // Make sure start and stop times does not write over each other
                //this.setElemWidth(this.getFirstElementByClassName(timeBlockElem, 'shift-time-stop'), width - 25);
            }
        }
    }

    private updateProjectTimeBlockRowSizeAndPosition(row) {
        if (!row)
            return;

        let projectTimeBlocks = row.getElementsByClassName('timeattest-schedule-projecttimeblock');
        for (let projectTimeBlockElem of projectTimeBlocks) {
            let projectTimeBlock = this.getProjectTimeBlockFromElem(projectTimeBlockElem);
            let left = this.getPixelsForTime(projectTimeBlock.actualStartTime);

            this.setElemPosition(projectTimeBlockElem, left);
            let width: number = this.getPixelsForTime(projectTimeBlock.actualStopTime) - left - 1;
            this.setElemWidth(projectTimeBlockElem, width);

            // Make sure start and stop times does not write over each other
            this.setElemWidth(this.getFirstElementByClassName(projectTimeBlockElem, 'shift-time-stop'), width - 25);
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
        var cols = $('.timeattest-schedule #{0} tbody tr:first .timeattest-schedule-day'.format(this.controller.tableId));

        this.columnWidths = [];
        var totalWidth = 0;
        var count = 0;
        cols.each((i, e) => {
            let colWidth = $(e).outerWidth();
            this.columnWidths.push({ index: i, width: colWidth });
            totalWidth += colWidth;
            count++;
        });

        var width = totalWidth / count;

        if (width) {
            this.columnWidth = width;
            this.pixelsPerTimeUnit = this.columnWidth / 60;
        }
    }

    private setScheduleWidth() {
        this.scheduleWidth = $('.timeattest-schedule #{0} thead'.format(this.controller.tableId)).outerWidth();
    }

    public updateWidthOnAllElements(delay: number = 100) {
        this.$timeout(() => {
            this.updateWidth();
            this.setScheduleWidth();
            $('.timeattest-schedule #{0} tbody tr'.format(this.controller.tableId)).each((_, row) => this.updateScheduleRowSizeAndPosition(row));
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

    public getShiftLengthInDays(shift: ShiftDTO) {
        // Get shift actual stop time or end of display if shift goes beyond that
        let actualStop = CalendarUtility.minOfDates(shift.actualStopTime, this.controller.dateTo).beginningOfDay();

        return actualStop.diffDays(shift.actualStartTime.beginningOfDay()) + 1;
    }

    public getDifferenceInMinutes(startDate: Date, endDate: Date) {
        // Get shift actual stop time or end of display if shift goes beyond that
        let actualStop = CalendarUtility.getMinDate(endDate, this.controller.dateTo);
        // Get shift actual stop time or beginning of display if shift goes beyond that
        actualStop = CalendarUtility.getMaxDate(actualStop, this.controller.dateFrom);

        return actualStop.diffMinutes(startDate);
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

        var actualBreakStart = shift[`break${breakNo}StartTime`];
        var breakEnd = actualBreakStart.addMinutes(shift[`break${breakNo}Minutes`]);

        // Make sure break is within shift (otherwise breaks on shifts hidden by filter will be visible)
        if (CalendarUtility.getIntersectingDuration(shift.actualStartTime, shift.actualStopTime, actualBreakStart, breakEnd) === 0)
            return;

        var span = document.createElement('span');
        span.classList.add('shift-break');
        span.id = shift[`break${breakNo}Id`];
        var breakText: string;
        var breakPrefix: string = this.controller.terms["time.schedule.planning.breakprefix"];
        var timeCode: string = shift[`break${breakNo}TimeCode`];
        if (timeCode && timeCode.startsWithCaseInsensitive(breakPrefix))
            breakText = timeCode;
        else
            breakText = "{0} {1}".format(breakPrefix, timeCode);

        span.title = "{0}  {1}".format(this.formatStartStopDate(actualBreakStart, breakEnd), breakText);

        targetElem.appendChild(span);
    }

    public getBreakData(shift: ShiftDTO, breakId: number) {
        var breakNo = this.getBreakNo(shift, breakId);

        return { breakMinutes: shift[`break${breakNo}Minutes`], actualBreakStart: shift[`break${breakNo}StartTime`] };
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

    public formatStartStopDate(start: Date, stop: Date) {
        var filter = this.$filter('date');
        return filter(start, 'shortTime') + '-' + filter(stop, 'shortTime');
    }

    // TimeStamps

    private getTimeStampFromElem(elem): TimeAttestTimeStampDTO {
        var idStr: string = elem.id;
        return this.controller.getTimeStampById(parseInt(idStr.substr(2), 10));
    }

    private getSelectedTimeStamps() {
        return this.controller.timeStamps ? this.controller.timeStamps.filter(s => s.selected) : [];
    }

    private clearSelectedTimeStamps() {
        this.unselectTimeStamps(this.getSelectedTimeStamps());
    }

    public selectTimeStamp(timeStamp: TimeAttestTimeStampDTO, notify: boolean = false) {
        if (timeStamp) {
            this.clearSelectedTimeStamps();
            timeStamp.selected = true;
            $('#ts' + timeStamp.timeStampEntryId).addClass('timeattest-schedule-selected-shift');

            if (notify)
                this.controller.timeStampSelected(timeStamp);
        }
    }

    public unselectTimeStamps(timeStamps: TimeAttestTimeStampDTO[]) {
        _.forEach(timeStamps, timeStamp => {
            this.unselectTimeStamp(timeStamp);
        });
    }

    private unselectTimeStamp(timeStamp: TimeAttestTimeStampDTO) {
        if (timeStamp) {
            timeStamp.selected = false;
            $('#ts' + timeStamp.timeStampEntryId).removeClass('timeattest-schedule-selected-shift');
        }
    }

    //ProjectTimeBlocks
    private getProjectTimeBlockFromElem(elem): ProjectTimeBlockDTO {
        var idStr: string = elem.id;
        return this.controller.getProjectTimeBlockById(parseInt(idStr.substr(2), 10));
    }

    // TimeBlocks

    public getTimeBlockFromElem(elem): AttestEmployeeDayTimeBlockDTO {
        return this.controller.getTimeBlockByGuidId($(elem).attr('id').substr(2));
    }
}