import { ShiftDTO, OrderListDTO } from "../../../Common/Models/TimeSchedulePlanningDTOs";
import { EditController } from "./EditController";
import { ScheduleHandler } from "./ScheduleHandler";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { PlanningEditModes } from "../../../Util/Enumerations";
import { StaffingNeedsTaskDTO } from "../../../Common/Models/StaffingNeedsDTOs";
import { EmployeeListDTO } from "../../../Common/Models/EmployeeListDTO";
import { TermGroup_ShiftHistoryType, SoeEmployeePostStatus, TermGroup_TimeScheduleTemplateBlockType } from "../../../Util/CommonEnumerations";
import { Guid } from "../../../Util/StringUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";

interface DraggableEventUIParams extends JQueryUI.DraggableEventUIParams {
    originalPosition: { top: number; left: number; };
}

export class DragDropHelper {
    private resizeableOptionsBreak;
    private draggableOptionsBreak;
    private offsetX: number;
    private clientX: number;
    private wasCancelled: boolean = false;

    private currentDragShift: ShiftDTO;
    private currentResizeShift: ShiftDTO;
    private currentResizeBreak: ShiftDTO;
    private resizeTarget: string;
    private resizeOriginalX: number;
    private dragRoundingMinutes: number = 5;
    private lastDropEvent: any;
    private droppedWasHandled: boolean;

    private moveShiftContainment: number[] = null;
    private viewChanged = true;
    //private ctrlDown: boolean = false;
    //private shiftDown: boolean = false;

    constructor(private controller: EditController, private scheduleHandler: ScheduleHandler, private $filter: ng.IFilterService, private $q: ng.IQService) {
        this.performFirstTimeSetup = this.performFirstTimeSetup.bind(this);

        // Day view, resize break
        this.resizeableOptionsBreak = {
            handles: 'w, e',
            autoHide: true,
            containment: 'document',
            start: this.onBreakResizeStart.bind(this),
            resize: this.onBreakResize.bind(this),
            stop: this.onBreakResizeStop.bind(this),
            cancel: ".ui-state-disabled, .no-dragdrop"
        };

        // Day view, drag break
        this.draggableOptionsBreak = {
            axis: 'x',
            containment: '.planning-tr-compressed',
            start: this.onBreakDragStart.bind(this),
            drag: this.onBreakDrag.bind(this),
            stop: this.onBreakDragStop.bind(this),
            cancel: ".ui-state-disabled, .no-dragdrop"
        };
    }

    // SETUP

    public performFirstTimeSetup() {
        $(document).keyup(e => {
            if (e.keyCode === 27) { // Escape key
                if ($('.shift-drop-zone').sortable("instance")) {
                    this.wasCancelled = true;
                    $('.shift-drop-zone').sortable("cancel");
                }
            }
        });
    }

    public enableInteractability(td: HTMLElement) {
        const planningShifts = td.querySelectorAll('.planning-shift');
        for (let i = 0; i < planningShifts.length; i++) {
            const planningShift = planningShifts[i];

            if (!planningShift.classList.contains('no-dragdrop'))
                planningShift.addEventListener('mouseenter', this.enabledShiftInteractableIfNeeded.bind(this));
        }
    }

    public enabledShiftInteractableIfNeeded(e: MouseEvent) {
        const $element = $(<HTMLElement>e.target);

        if ($element.data('interactable'))
            return;

        $element.data('interactable', true);

        //Calculate the containtment for draggle shifts, which is tbody - first column.
        if (!this.moveShiftContainment) {
            const tableBody = document.querySelector(".planning-scheduleview tbody");
            if (tableBody) {
                const boundingRect = tableBody.getBoundingClientRect();
                const firstColumnBounderies = tableBody.querySelector("tr > td").getBoundingClientRect();
                const firstColumnWidth = firstColumnBounderies.right - firstColumnBounderies.left;
                const rowBounderies = $element[0].getBoundingClientRect();
                const rowHeight = rowBounderies.bottom - rowBounderies.top;

                this.moveShiftContainment = [];
                this.moveShiftContainment.push(boundingRect.left + firstColumnWidth);
                this.moveShiftContainment.push(boundingRect.top + window.scrollY);
                this.moveShiftContainment.push(boundingRect.right);
                this.moveShiftContainment.push(boundingRect.bottom - rowHeight + window.scrollY);
            }
        }

        let draggableOptions: JQueryUI.DraggableOptions = {
            containment: this.moveShiftContainment,
            scrollSensitivity: 50,
            revert: this.shouldEnableRevertAnimation.bind(this),
            revertDuration: 200,
            delay: 100,
            opacity: 0.90
        };

        if (this.controller.isCommonDayView && this.controller.editMode !== PlanningEditModes.Breaks) {
            draggableOptions = angular.extend(draggableOptions, {
                helper: (ev: Event) => this.getHelperDay(ev, ev.currentTarget),
                start: this.onShiftDragStartDay.bind(this),
                stop: this.onShiftDragEndDay.bind(this),
                drag: this.onShiftDragDay.bind(this)
            });

            const shiftContainers = $element.children('.shift-split-container');
            shiftContainers.each(index => {
                let shiftContainer = $(shiftContainers[index]);
                const resizeHandles = [];
                if (shiftContainer.hasClass('planning-shift-resize-start'))
                    resizeHandles.push('w');
                if (shiftContainer.hasClass('planning-shift-resize-stop'))
                    resizeHandles.push('e');
                if (resizeHandles.length > 0) {
                    const resizableOptions: JQueryUI.ResizableOptions = {
                        handles: resizeHandles.join(','),
                        autoHide: true,
                        start: this.onShiftResizeStart.bind(this),
                        resize: this.onShiftResize.bind(this),
                        stop: this.onShiftResizeStop.bind(this),
                        cancel: ".ui-state-disabled, .no-dragdrop"
                    };

                    shiftContainer.resizable(resizableOptions);
                }
            });
        } else if ((this.controller.isScheduleView || this.controller.isTemplateScheduleView || this.controller.isEmployeePostScheduleView || this.controller.isScenarioScheduleView || this.controller.isScenarioCompleteView || this.controller.isStandbyScheduleView) && this.controller.editMode !== PlanningEditModes.Breaks) {
            draggableOptions = angular.extend(draggableOptions, {
                helper: (ev: Event) => this.getHelperSchedule(ev, ev.currentTarget),
                start: this.onShiftDragStartSchedule.bind(this),
                stop: this.onShiftDragStopSchedule.bind(this),
                drag: this.onDraggableDrag.bind(this)
            });
            if (this.controller.isStandbyScheduleView)
                draggableOptions.axis = "y";
        } else if ((this.controller.isDayView || this.controller.isTemplateDayView || this.controller.isScenarioDayView || this.controller.isStandbyDayView) && (this.controller.editMode === PlanningEditModes.Breaks)) {
            const breakContainer = $element.children('.shift-break');
            breakContainer.resizable(this.resizeableOptionsBreak);
            breakContainer.draggable(this.draggableOptionsBreak);
        }

        $element.draggable(draggableOptions);
        //need to retrigger the event on element after resizable has been added.
        $element.trigger('mouseenter', e);
    }

    private root: HTMLElement;

    public enableDragDropOfEmployeeList() {
        $(".employee-list li").draggable({
            revert: this.shouldEnableRevertAnimation.bind(this),
            revertDuration: 200,
            helper: 'clone',
            appendTo: ".planning-scheduleview table",
            stop: (e: any, ui) => this.processDroppableDrop(e, this.hoveringDropTarget, angular.extend(ui, { draggable: $(e.target) })),
            drag: this.onDraggableDrag.bind(this)

        });
    }

    public enableDragDropOfUnscheduledTaskList() {
        $(".unscheduledtask-list li").draggable({
            revert: this.shouldEnableRevertAnimation.bind(this),
            revertDuration: 200,
            helper: 'clone',
            appendTo: ".planning-scheduleview table",
            stop: (e: any, ui) => this.processDroppableDrop(e, this.hoveringDropTarget, angular.extend(ui, { draggable: $(e.target) })),
            drag: this.onDraggableDrag.bind(this)
        });
    }

    public enableDragDropOfOrderList() {
        $(".unscheduledorder-list li").draggable({
            revert: this.shouldEnableRevertAnimation.bind(this),
            revertDuration: 200,
            helper: 'clone',
            appendTo: ".planning-scheduleview table",
            stop: (e: any, ui) => this.processDroppableDrop(e, this.hoveringDropTarget, angular.extend(ui, { draggable: $(e.target) })),
            drag: this.onDraggableDrag.bind(this)
        });
    }

    // DRAG N DROP

    // Day view

    private onShiftDragStartDay(e: Event, ui: DraggableEventUIParams) {
        const shiftElement = $(e.target);
        shiftElement.addClass('dragging-shift');
        shiftElement.attr("drag-object-type", "shift");
        this.droppedWasHandled = false;

        const left: number = ui.originalPosition.left;
        this.getOrCreateShiftTimeElement(ui.helper, left);
    }

    private onShiftDragDay(e: MouseEvent, ui: DraggableEventUIParams) {
        this.onDraggableDrag(e, ui);

        // Called repeatedly while dragging a shift in day view
        // Update time on shift while dragging, but only if dragged on same employee
        const id = $(e.target).attr('id');
        const elem = ui.helper.find(`#${id}`);
        const shift = this.scheduleHandler.getShiftFromJQueryElem(elem);
        if (shift) {
            if (shift.employeeId === this.dragOverEmployeeId) {
                this.updateTimeOnShiftElem(elem, this.getMinutesMoved(e));
            } else {
                this.restoreTimeOnShiftElem(elem);
            }
        }
    }

    private onShiftDragEndDay(e: MouseEvent, ui: DraggableEventUIParams) {
        this.processDroppableDrop(e, this.hoveringDropTarget, angular.extend(ui, { draggable: $(e.target) }));

        const shiftElement = $(e.target);
        this.removeShiftTimeTooltip(this.getOrCreateShiftTimeElement(ui.helper));

        shiftElement.removeClass('dragging-shift');
        shiftElement.attr("drag-object-type", null);

        if (this.droppedWasHandled === false) {
            let { row: sourceRow, employee: sourceEmployee } = this.getInfoFromElem(shiftElement);
            this.scheduleHandler.updateEmployeeRow(sourceRow, sourceEmployee);
        }
    }

    private onShiftResizeStart(e, ui) {
        // Called when starting to resize a shift
        this.currentResizeShift = this.scheduleHandler.getShiftFromJQueryElem($(ui.originalElement).parent());
        this.resizeTarget = $(e.originalEvent.target).hasClass('ui-resizable-w') ? 'start' : 'stop';
        this.resizeOriginalX = e.clientX;

        this.getOrCreateShiftTimeElement(ui.originalElement);
    }

    private onShiftResize(e, ui) {
        // Called repeatedly while resizing a shift
        const sizeDiff = ui.size.width - ui.originalSize.width;
        const pixelsPerMinute = this.scheduleHandler.pixelsPerTimeUnit;

        ui.originalElement.css('width', ui.size.width);
        ui.originalElement.css('left', ui.position.left);

        const minutesMoved = Math.abs(Math.ceil(sizeDiff / pixelsPerMinute));
        const direction = this.resizeOriginalX > e.clientX ? -1 : 1;

        if (this.resizeTarget === 'start') {
            this.currentResizeShift.actualStartTimeDuringMove = this.adjustToClosestMinutes(this.currentResizeShift.actualStartTime.addMinutes(direction * minutesMoved), this.dragRoundingMinutes);
        } else {
            this.currentResizeShift.actualStopTimeDuringMove = this.adjustToClosestMinutes(this.currentResizeShift.actualStopTime.addMinutes(direction * minutesMoved), this.dragRoundingMinutes);
        }

        this.setShiftTime(this.getOrCreateShiftTimeElement(ui.originalElement));
    }

    private onShiftResizeStop(e, ui) {
        this.removeShiftTimeTooltip(this.getOrCreateShiftTimeElement(ui.originalElement));

        // Called when finished resizing a shift

        let { row: sourceRow, date: sourceDate, employee: sourceEmployee } = this.getInfoFromElem(ui.originalElement);

        // Clock rounding
        let shift = this.currentResizeShift;
        if (shift.actualStartTimeDuringMove)
            shift.actualStartTimeDuringMove = shift.actualStartTimeDuringMove.roundMinutes(this.controller.clockRounding);
        if (shift.actualStopTimeDuringMove)
            shift.actualStopTimeDuringMove = shift.actualStopTimeDuringMove.roundMinutes(this.controller.clockRounding);

        // Check that shift was actually resized
        const diffMinutes = shift.getShiftLengthDuringMove() - shift.getShiftLength();
        if (diffMinutes !== 0) {
            // If shift is shortened, show shifts in edit shift dialog, otherwise save it directly
            if (diffMinutes < 0) {
                this.setNewTimes([shift], 0);
                let existingShifts = this.controller.shifts.filter(s => (this.controller.isEmployeePostView ? s.employeePostId === shift.employeePostId : s.employeeId === shift.employeeId) && s.actualDateOnLoad.isSameDayAs(shift.actualDateOnLoad));
                // If hidden employee, only take linked shifts
                if (!this.controller.isEmployeePostView && sourceEmployee.employeeId === this.controller.hiddenEmployeeId)
                    existingShifts = existingShifts.filter(s => s.link === shift.link);
                this.controller.openEditShift(shift, existingShifts, sourceDate, sourceEmployee.identifier, shift.isStandby, shift.isOnDuty, true);
            } else {
                if (this.controller.isTemplateDayView || this.controller.isEmployeePostDayView)
                    this.saveTemplateShiftsForDayView([shift], 0);
                else {
                    this.saveShiftsForDayView([shift], 0).then(success => {
                        if (!success) {
                            // Restore
                            this.scheduleHandler.updateEmployeeRow(sourceRow, sourceEmployee);
                        }
                    });
                    this.currentResizeShift = null;
                }
            }
        } else {
            // Restore
            this.scheduleHandler.updateEmployeeRow(sourceRow, sourceEmployee);
        }
    }

    private getOrCreateShiftTimeElement(parentElem: JQuery<HTMLElement>, offsetLeft: number = 0): JQuery {
        const elem: JQuery = parentElem.find('.shift-time-tooltip');

        if (elem.length < 1) {
            const origin = parentElem.find('.shift-time-for-tooltip').first();
            const tooltip = origin.clone();
            tooltip.addClass('shift-time-tooltip');

            //Remove element styles so the specified class can overwrite these without "!important".
            tooltip.css("width", "");
            tooltip.css("color", "");

            origin.parent().append(tooltip);
        }

        return elem;
    }

    private shouldEnableRevertAnimation(): boolean {
        return !(this.hoveringDropTarget && this.hoveringDropTarget.classList.contains('drop-target'));
    }

    private removeShiftTimeTooltip(elem: JQuery) {
        elem.remove();
    }

    private setShiftTime(elem: JQuery, text?: string) {
        if (elem) {
            if (!text) {
                if (this.resizeTarget === 'start') {
                    text = this.formatStartStopDate(this.currentResizeShift.actualStartTimeDuringMove, this.currentResizeShift.actualStopTime);
                } else {
                    text = this.formatStartStopDate(this.currentResizeShift.actualStartTime, this.currentResizeShift.actualStopTimeDuringMove);
                }
            }

            elem.text(text);
        }
    }

    private setNewTimes(shifts: ShiftDTO[], minutesMoved: number) {
        // Set new times on resized or dragged shifts
        shifts.forEach(shift => {
            if (minutesMoved === 0) {
                // Drag in beginning or end of shift
                if (shift.actualStartTimeDuringMove)
                    shift.actualStartTime = shift.actualStartTimeDuringMove;
                if (shift.actualStopTimeDuringMove)
                    shift.actualStopTime = shift.actualStopTimeDuringMove;
            } else {
                // Drag whole shift
                shift.actualStartTime = shift.actualStartTime.addMinutes(minutesMoved).roundMinutes(this.controller.clockRounding);
                shift.actualStopTime = shift.actualStopTime.addMinutes(minutesMoved).roundMinutes(this.controller.clockRounding);
                if (shift.break1TimeCodeId)
                    shift.break1StartTime = shift.break1StartTime.addMinutes(minutesMoved);
                if (shift.break2TimeCodeId)
                    shift.break2StartTime = shift.break2StartTime.addMinutes(minutesMoved);
                if (shift.break3TimeCodeId)
                    shift.break3StartTime = shift.break3StartTime.addMinutes(minutesMoved);
                if (shift.break4TimeCodeId)
                    shift.break4StartTime = shift.break4StartTime.addMinutes(minutesMoved);
            }
        });
    }

    private saveShiftsForDayView(shifts: ShiftDTO[], minutesMoved: number): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        this.controller.initSaveShiftsForDayView(TermGroup_ShiftHistoryType.TaskSaveTimeScheduleShift, shifts).then(passed => {
            if (passed) {
                this.setNewTimes(shifts, minutesMoved);
                this.controller.saveShifts(Guid.newGuid().toString(), shifts, minutesMoved !== 0, true, minutesMoved !== 0, minutesMoved, [], true);
                deferral.resolve(true);
            } else {
                deferral.resolve(false);
            }
        });

        return deferral.promise;
    }

    private saveTemplateShiftsForDayView(shifts: ShiftDTO[], minutesMoved: number) {
        this.setNewTimes(shifts, minutesMoved);
        this.controller.saveTemplateShiftsForDayView(shifts);
    }

    private getMinutesMoved(e): number {
        const pixelsMoved = e.clientX - this.clientX;
        const minutesMoved = pixelsMoved / this.scheduleHandler.pixelsPerTimeUnit;

        return minutesMoved.round(0);
    }

    // Breaks

    private onBreakDragStart(e, ui) {
        // Called when starting to drag a break
        let elem = $(e.currentTarget);
        let shift = this.scheduleHandler.getShiftFromJQueryElem(elem.parents('.planning-shift'));
        let breakData = this.scheduleHandler.getBreakData(shift, elem.prop('id'));

        this.currentResizeBreak = new ShiftDTO();
        this.currentResizeBreak.timeScheduleTemplateBlockId = elem.prop('id');
        this.currentResizeBreak.actualStartTime = breakData.actualBreakStart;
        this.currentResizeBreak.actualStopTime = breakData.actualBreakStart.addMinutes(breakData.breakMinutes);

        this.showBreakTimeTooltip(this.getBreakTimeElement(ui.helper));
    }

    private onBreakDrag(e, ui) {
        // Called repeatedly while dragging a break
        let sizeDiff = ui.originalPosition.left - ui.position.left;
        let pixelsPerMinute = this.scheduleHandler.pixelsPerTimeUnit;

        ui.helper.css('left', ui.position.left);

        let minutesMoved = Math.ceil(sizeDiff / pixelsPerMinute);

        this.currentResizeBreak.actualStartTimeDuringMove = this.adjustToClosestMinutes(this.currentResizeBreak.actualStartTime.addMinutes(-minutesMoved), this.dragRoundingMinutes);
        this.currentResizeBreak.actualStopTimeDuringMove = this.adjustToClosestMinutes(this.currentResizeBreak.actualStopTime.addMinutes(-minutesMoved), this.dragRoundingMinutes);

        this.setBreakTime(this.getBreakTimeElement(ui.helper));
    }

    private onBreakDragStop(e, ui) {
        // Called when finished dragging a break
        this.handleBreakDragOrResize(ui.helper, (shift: ShiftDTO, dayShifts: ShiftDTO[], breakNo: number, dragStart: boolean, dragStop: boolean) => {
            this.controller.initValidateBreakChange(shift, dayShifts, breakNo, dragStart, dragStop);
        });
    }

    private onBreakResizeStart(e, ui) {
        // Called when starting to resize a break
        let shift = this.scheduleHandler.getShiftFromJQueryElem(ui.originalElement.parents('.planning-shift'));
        let breakData = this.scheduleHandler.getBreakData(shift, ui.originalElement.prop('id'));

        this.currentResizeBreak = new ShiftDTO();
        this.currentResizeBreak.timeScheduleTemplateBlockId = ui.originalElement.prop('id');
        this.currentResizeBreak.actualStartTime = breakData.actualBreakStart;
        this.currentResizeBreak.actualStopTime = breakData.actualBreakStart.addMinutes(breakData.breakMinutes);

        this.resizeTarget = $(e.originalEvent.target).hasClass('ui-resizable-w') ? 'start' : 'stop';
        this.resizeOriginalX = e.clientX;

        this.showBreakTimeTooltip(this.getBreakTimeElement(ui.originalElement));
    }

    private onBreakResize(e, ui) {
        // Called repeatedly while resizing a break
        let sizeDiff = ui.size.width - ui.originalSize.width;
        let pixelsPerMinute = this.scheduleHandler.pixelsPerTimeUnit;

        ui.originalElement.css('width', ui.size.width + 10);//for some reason breaks get smaller when you drag them the first time, it seems to be that the width is calculated from the leftmost point of the drag-cursor, so 10 seems to compensate.
        ui.originalElement.css('left', ui.position.left);

        let minutesMoved = Math.ceil(sizeDiff / pixelsPerMinute);

        if (this.resizeTarget === 'start')
            this.currentResizeBreak.actualStartTimeDuringMove = this.adjustToClosestMinutes(this.currentResizeBreak.actualStartTime.addMinutes(-minutesMoved), this.dragRoundingMinutes);
        else
            this.currentResizeBreak.actualStopTimeDuringMove = this.adjustToClosestMinutes(this.currentResizeBreak.actualStopTime.addMinutes(minutesMoved), this.dragRoundingMinutes);

        this.setBreakTime(this.getBreakTimeElement(ui.originalElement));
    }

    private onBreakResizeStop(e, ui) {
        // Called when finished resizing a break
        this.handleBreakDragOrResize(ui.originalElement, (shift: ShiftDTO, dayShifts: ShiftDTO[], breakNo: number, dragStart: boolean, dragStop: boolean) => {
            this.controller.initValidateBreakChange(shift, dayShifts, breakNo, dragStart, dragStop);
        });
    }

    private handleBreakDragOrResize(elem, updateFunc) {
        this.hideBreakTimeTooltip(this.getBreakTimeElement(elem));

        let shift = this.scheduleHandler.getShiftFromJQueryElem(elem.parents('.planning-shift'));
        let breakNo = this.scheduleHandler.getBreakNo(shift, this.currentResizeBreak.timeScheduleTemplateBlockId);
        let dragStart: boolean = false;
        let dragStop: boolean = false;

        let dayShifts = this.controller.getShifts(shift.employeeId, shift.date, 0, true);
        if (this.currentResizeBreak.actualStartTimeDuringMove) {
            // Make sure break is not dragged before first shift start
            this.currentResizeBreak.actualStartTimeDuringMove = CalendarUtility.getMaxDate(this.currentResizeBreak.actualStartTimeDuringMove, _.first(dayShifts).actualStartTime);
            dayShifts.forEach(s => {
                s['break' + breakNo + 'StartTime'] = this.currentResizeBreak.actualStartTimeDuringMove;
            });
            dragStart = true;
        }

        if (this.currentResizeBreak.actualStopTimeDuringMove) {
            // Make sure break is not dragged after last shift end
            this.currentResizeBreak.actualStopTimeDuringMove = CalendarUtility.getMinDate(this.currentResizeBreak.actualStopTimeDuringMove, _.last(dayShifts).actualStopTime);
            dragStop = true;
        }

        let startTime = this.currentResizeBreak.actualStartTimeDuringMove || this.currentResizeBreak.actualStartTime;
        let stopTime = this.currentResizeBreak.actualStopTimeDuringMove || this.currentResizeBreak.actualStopTime;

        if (this.currentResizeBreak.actualStartTimeDuringMove || this.currentResizeBreak.actualStopTimeDuringMove) {
            dayShifts.forEach(s => {
                s['break' + breakNo + 'Minutes'] = stopTime.diffMinutes(startTime);
            });
        }

        //this.scheduleHandler.updateRowSizeAndPosition(elem.parents('tr'));//since we are not pixel perfect and since we round to closest time, we rerender to make it as perfect as possible.

        updateFunc(shift, dayShifts, breakNo, dragStart, dragStop);

        this.currentResizeBreak.actualStartTimeDuringMove = null;
        this.currentResizeBreak.actualStopTimeDuringMove = null;
        this.currentResizeBreak = null;
    }

    private getBreakTimeElement(parentElem): JQuery {
        let elem: JQuery = parentElem.find('.shift-break-text');
        if (elem && !elem.tooltip('instance')) {
            elem.tooltip();
            elem.tooltip('option', 'classes.ui-tooltip', 'highlight');
            elem.tooltip('option', 'position', { my: 'left bottom', at: 'left top-5' });
            elem.tooltip().off("mouseover");    // Manually show/hide tooltip in drag
            elem.tooltip().off("mouseleave");
        }

        return elem;
    }

    private showBreakTimeTooltip(elem: JQuery) {
        if (elem)
            elem.tooltip('open');
    }

    private hideBreakTimeTooltip(elem: JQuery) {
        if (elem)
            elem.tooltip('close');
    }

    private setBreakTime(elem: JQuery, text?: string) {
        if (elem) {
            if (!text)
                text = this.formatStartStopDate(this.currentResizeBreak.actualStartTimeDuringMove || this.currentResizeBreak.actualStartTime, this.currentResizeBreak.actualStopTimeDuringMove || this.currentResizeBreak.actualStopTime);

            elem.text(text);
            elem.tooltip('option', 'content', text);
        }
    }

    // Schedule view

    private revertDraggedShifts: boolean = false;
    private onShiftDragStartSchedule(e: Event, ui: DraggableEventUIParams) {
        // Called when starting to drag a shift in schedule view
        const shiftElement = $(e.target);

        shiftElement.addClass('dragging-shift');
        shiftElement.attr("drag-object-type", "shift");
        this.currentDragShift = this.scheduleHandler.getShiftFromJQueryElem(shiftElement);
        this.revertDraggedShifts = true;
        this.droppedWasHandled = false;
    }

    private onShiftDragStopSchedule(e: MouseEvent, ui: DraggableEventUIParams) {
        this.processDroppableDrop(e, this.hoveringDropTarget, angular.extend(ui, { draggable: $(e.target) }));

        const shiftElement = $(e.target);
        shiftElement.removeClass('dragging-shift');
        shiftElement.attr("drag-object-type", null);

        // Called when finished dragging a shift in schedule view
        if (this.revertDraggedShifts || this.droppedWasHandled === false) {
            // If shifts are dropped back in same cell
            let { row: sourceRow, employee: sourceEmployee } = this.getInfoFromElem(shiftElement);
            this.scheduleHandler.updateEmployeeRow(sourceRow, sourceEmployee);
        }
    }

    private isWithinBounds(element: HTMLElement, e: MouseEvent): boolean {
        const bounds = element.getBoundingClientRect();
        const x = e.clientX, y = e.clientY;

        return bounds.left <= x && x <= bounds.right &&
            bounds.top <= y && y <= bounds.bottom;
    }

    private querySelectElementMouseInBounds(root: HTMLElement, selector: string, e: MouseEvent): HTMLElement {
        const matches = root.querySelectorAll(selector);

        for (let i = 0; i < matches.length; i++) {
            const curr = <HTMLElement>matches[i];
            if (this.isWithinBounds(curr, e)) return curr;
        }

        return null;
    }

    private childElementsMouseInBounds(root: HTMLElement, e: MouseEvent): HTMLElement {
        const matches = root.children;

        for (let i = 0; i < matches.length; i++) {
            const curr = <HTMLElement>matches[i];
            if (this.isWithinBounds(curr, e)) return curr;
        }

        return null;
    }

    private getRootTable() {
        if (this.root == null) {
            this.root = document.getElementById('rowsTarget');
        }

        return this.root;
    }

    private hoveringRow: HTMLElement;
    private hoveringDropTarget: HTMLElement;

    private onDraggableDrag(e: MouseEvent, ui: JQueryUI.DraggableEventUIParams) {
        const table = this.getRootTable();
        const overUi = angular.extend(ui, { draggable: $(e.target) });

        if (this.hoveringRow)
            $(this.hoveringRow).removeClass('drop-target');
        if (this.hoveringDropTarget)
            $(this.hoveringDropTarget).removeClass('drop-target');

        this.hoveringRow = null;
        this.hoveringDropTarget = null;

        this.hoveringRow = this.childElementsMouseInBounds(table, e);
        if (!this.hoveringRow) {
            return;
        }

        this.hoveringDropTarget =
            this.querySelectElementMouseInBounds(this.hoveringRow, '.planning-employee-post:not(.helper)', e) ||
            this.querySelectElementMouseInBounds(this.hoveringRow, '.planning-shift:not(.helper)', e) ||
            this.querySelectElementMouseInBounds(this.hoveringRow, '.shift-drop-zone:not(.helper)', e);

        if (this.hoveringDropTarget) {
            this.processDroppableOver(this.hoveringRow, this.hoveringDropTarget, overUi);
        }
    }

    private onDraggableStop(e: MouseEvent, ui: any) {
        if (this.hoveringDropTarget) {
            this.processDroppableDrop(e, this.hoveringDropTarget, angular.extend(ui, { draggable: $(e.target) }))
        }
    }

    private dragOverEmployeeId: number = 0;

    // Employee/task/order list
    private processDroppableOver(targetRow: HTMLElement, target: HTMLElement, ui: any) {
        let validDrop = false;

        const $target = $(target);
        const $targetRow = $(targetRow);
        const objectType: string = ui.draggable.attr('drag-object-type');

        if (objectType === 'employee') {
            validDrop = ((this.controller.isDayView || this.controller.isScheduleView) && $target.hasClass('planning-shift')) || (this.controller.isEmployeePostView && $target.hasClass('planning-employee-post'));
        } else if (objectType === 'task') {
            validDrop = ($target.hasClass('planning-shift') || $target.hasClass('shift-drop-zone'));
            if (validDrop && this.controller.isEmployeePostView) {
                let dateIndex = $targetRow.find('td').index($target.parents('td'));
                if (dateIndex >= 0) {
                    let targetDate: Date = this.controller.dates[dateIndex - 1].date;
                    let taskDate: Date = (<string>ui.draggable.attr('task-date')).parsePipedDate();
                    validDrop = targetDate.isSameDayAs(taskDate);
                } else {
                    validDrop = false;
                }
            }
        } else if (objectType === 'order') {
            validDrop = ($target.hasClass('planning-shift') || $target.hasClass('shift-drop-zone'));
        } else if (objectType === 'shift') {
            this.dragOverEmployeeId = Number(targetRow['id'].replace('empId', ''));

            validDrop = ($target.hasClass('planning-shift') || $target.hasClass('shift-drop-zone'));
        }

        if (validDrop) {
            $target.addClass('drop-target');
            $targetRow.addClass('drop-target');
        }
        else {
            $target.removeClass('drop-target');
            $targetRow.removeClass('drop-target');
        }
    }

    private processDroppableDrop(e: MouseEvent, target: HTMLElement, ui: any) {
        let $target = $(target);
        if (!$target.hasClass('drop-target'))
            return;

        // Get target information
        $target.removeClass('drop-target');
        let info = this.getInfoFromElem($target);
        this.lastDropEvent = info;
        this.droppedWasHandled = true;

        if (!info.employee || !info.employee.active)
            return;

        let objectType: string = ui.draggable.attr('drag-object-type');
        if (objectType === 'employee') {
            // Get id of employee post or shift that the employee was dropped on.
            // On employee posts, id is set on the row, as empPostIdNNN.
            // On shifts, id is set directly on the td.
            let targetId: number = this.controller.isEmployeePostView ? parseInt($target.parents('tr').attr('id').substring(9), 10) : parseInt($target.attr('id'), 10);
            if ($target.hasClass('planning-employee-post'))
                info['employeePostId'] = targetId;
            else if ($target.hasClass('planning-shift'))
                info['shiftId'] = targetId;

            // Get source information
            let sourceEmployeeId = parseInt(ui.draggable.attr('id'), 10);
            info['fromEmployeeId'] = sourceEmployeeId;

            //Due to how the schedule is designed, we get two events on dayview if you drop on a shift. One for the actual shift, and one time for the actual date dropped on. If dropped on a shift, we dont care.
            if (this.controller.isDayView && this.lastDropEvent && this.lastDropEvent['shiftId'] && !info['shiftId'] && sourceEmployeeId === this.lastDropEvent['fromEmployeeId'] && info.employee === this.lastDropEvent.employee) {
                this.lastDropEvent = null;
                return;
            }

            // If employee is dropped in an empty slot, take no action
            if (!targetId)
                return;

            if ($target.hasClass('planning-employee-post'))
                this.controller.employeeDroppedOnEmployeePost(sourceEmployeeId, targetId);
            else
                this.controller.employeeDroppedOnShift(sourceEmployeeId, targetId);
        } else if (objectType === 'task') {
            // Get source information
            let taskId: number = parseInt(ui.draggable.attr('id'), 10);
            let taskType: number = parseInt(ui.draggable.attr('task-type'), 10);
            let taskDate: Date = (<string>ui.draggable.attr('task-date')).parsePipedDate();
            let taskStartTime: Date = (<string>ui.draggable.attr('task-start-time')).parsePipedTime(taskDate);
            let taskStopTime: Date = (<string>ui.draggable.attr('task-stop-time')).parsePipedTime(taskDate);
            let task: StaffingNeedsTaskDTO = this.controller.unscheduledTasks.find(t => t.id === taskId && t.type === taskType && t.startTime.isSameMinuteAs(taskStartTime) && t.stopTime.isSameMinuteAs(taskStopTime));
            if (task) {
                if (this.controller.isEmployeePostView) {
                    if (info.employee.employeePostStatus !== SoeEmployeePostStatus.Locked)
                        this.controller.openAssignTaskToEmployeePost([task], info.employee, info.date);
                } else if (this.controller.isTemplateView) {
                    this.controller.openAssignTaskToEmployeePost([task], info.employee, info.date);
                } else {
                    this.controller.openAssignTaskToEmployee([task], info.employee, info.date);
                }
            }
        } else if (objectType === 'order') {
            // Get source information
            let orderId: number = parseInt(ui.draggable.attr('id'), 10);
            let order: OrderListDTO = this.controller.orderList.find(o => o.orderId === orderId);
            if (order) {
                let shift: ShiftDTO = new ShiftDTO(TermGroup_TimeScheduleTemplateBlockType.Order);
                shift.order = order;
                shift.employeeId = info.employee.employeeId;
                shift.shiftTypeId = order.shiftTypeId;
                shift.shiftTypeName = order.shiftTypeName;
                shift.actualStartTime = info.date;  // TODO: In day view, keep time from slot
                shift.link = Guid.newGuid();

                this.controller.openEditAssignment(shift, info.date, info.employee.employeeId);
            }
        } else if (objectType === 'shift' && this.controller.isCommonDayView) {
            let { row: targetRow, date: targetDate, employee: targetEmployee, shift: targetShift } = this.getInfoFromElem($target);
            let { row: sourceRow, date: sourceDate, employee: sourceEmployee } = this.getInfoFromElem(ui.draggable);

            if (!sourceEmployee || !targetEmployee)
                return;

            if (targetEmployee.hidden && this.controller.isHiddenEmployeeReadOnly) {
                // Restore
                this.scheduleHandler.updateEmployeeRow(sourceRow, sourceEmployee);
                return;
            }

            // Get all selected shifts
            // If only dragging one shift, it will not be selected
            // In that case use current
            let shift = this.scheduleHandler.getShiftFromJQueryElem(ui.draggable);
            let selected: ShiftDTO[] = shift.selected ? this.scheduleHandler.getSelectedShifts() : [shift];

            let minutesMoved = this.getMinutesMoved(e);
            minutesMoved = this.adjustToClosest(minutesMoved, this.dragRoundingMinutes);

            if (sourceEmployee.employeeId === targetEmployee.employeeId) {
                if (this.controller.isTemplateDayView || this.controller.isEmployeePostDayView)
                    this.saveTemplateShiftsForDayView(selected, minutesMoved);
                else {
                    this.saveShiftsForDayView(selected, minutesMoved).then(success => {
                        if (!success) {
                            // Restore
                            this.scheduleHandler.updateEmployeeRow(sourceRow, sourceEmployee);
                        }
                    });
                }
            } else {
                this.controller.openDragShiftByIds(selected.map(s => s.timeScheduleTemplateBlockId), targetEmployee, targetDate, targetShift, 0);
            }
        } else if (objectType === 'shift' && (this.controller.isScheduleView || this.controller.isTemplateScheduleView || this.controller.isEmployeePostScheduleView || this.controller.isScenarioScheduleView || this.controller.isScenarioCompleteView || this.controller.isStandbyScheduleView)) {
            // Set attribute on target object so info can be extracted from it within getInfoFromElem
            if ($target)
                $target.attr("drag-object-type", "shift");
            let { row: targetRow, date: targetDate, employee: targetEmployee, shift: targetShift } = this.getInfoFromElem($target);
            let { row: sourceRow, date: sourceDate, employee: sourceEmployee } = this.getInfoFromElem(ui.draggable);

            // Standby shifts can only be dragged to same date
            if (this.controller.isStandbyScheduleView)
                targetDate = sourceDate;

            if (!sourceEmployee || !targetEmployee)
                return;

            // Dropped on same employee and date
            if (sourceEmployee.employeeId == targetEmployee.employeeId && sourceDate.isSameDayAs(targetDate))
                return;

            let shift = this.scheduleHandler.getShiftFromJQueryElem(ui.draggable);

            // Get all selected shifts
            // If only dragging one shift, it might not be selected
            // In that case use current
            if (!shift.selected) {
                this.scheduleHandler.clearSelectedShifts();
                this.scheduleHandler.selectShift(shift);
            }
            let selected: ShiftDTO[] = this.scheduleHandler.getSelectedShifts();

            this.revertDraggedShifts = false;

            // Check that target schedule is not read only
            let isReadonly: boolean = false;
            if (targetEmployee.hidden && this.controller.isHiddenEmployeeReadOnly)
                isReadonly = true;
            else if (this.controller.isEmployeePostView && targetEmployee.employeePostStatus === SoeEmployeePostStatus.Locked)
                isReadonly = true;
            else if (this.controller.isTemplateView) {
                let sourceTemplate = sourceEmployee.getTemplateSchedule(sourceDate);
                let targetTemplate = targetEmployee.getTemplateSchedule(targetDate);

                let lastTargetDate: Date = targetDate;
                if (selected.length > 1) {
                    let diffDays = targetDate.diffDays(this.currentDragShift.actualDateOnLoad);
                    lastTargetDate = _.orderBy(selected, s => s.actualDateOnLoad, 'desc')[0].actualDateOnLoad.addDays(diffDays);
                }

                // On same employee, we can only drag within same template
                if (!targetTemplate || (sourceEmployee.employeeId == targetEmployee.employeeId && sourceTemplate.timeScheduleTemplateHeadId !== targetTemplate.timeScheduleTemplateHeadId)) {
                    isReadonly = true;
                } else {
                    if (targetTemplate.timeScheduleTemplateGroupId)
                        isReadonly = true;
                    else {
                        let range = this.controller.getTemplateVisibleRange(targetTemplate);
                        if (lastTargetDate.isAfterOnDay(range.stop))
                            isReadonly = true;
                    }
                }
            }

            if (isReadonly) {
                this.scheduleHandler.updateEmployeeRow(sourceRow, sourceEmployee);
                if (sourceEmployee.identifier !== targetEmployee.identifier)
                    this.scheduleHandler.updateEmployeeRow(targetRow, targetEmployee);
            } else {
                let diffDays = targetDate.diffDays(this.currentDragShift.actualDateOnLoad);
                if (this.currentDragShift.belongsToPreviousDay) {
                    diffDays--;
                    targetDate = targetDate.addDays(-1);
                } else if (this.currentDragShift.belongsToNextDay) {
                    diffDays++;
                    targetDate = targetDate.addDays(1);
                }
                this.controller.openDragShiftByIds(selected.map(s => s.timeScheduleTemplateBlockId), targetEmployee, targetDate, targetShift, diffDays);
            }
        } else {
            this.droppedWasHandled = false;
        }
    }

    private rightListOver(e, ui) {
        let validDrop = false;

        let objectType: string = ui.draggable.attr('drag-object-type');
        if (objectType === 'employee') {
            validDrop = ((this.controller.isDayView || this.controller.isScheduleView) && $(e.target).hasClass('planning-shift')) || (this.controller.isEmployeePostView && $(e.target).hasClass('planning-employee-post'));
        } else if (objectType === 'task') {
            validDrop = ($(e.target).hasClass('planning-shift') || $(e.target).hasClass('shift-drop-zone'));
            if (validDrop && this.controller.isEmployeePostView) {
                let dateIndex = $(e.target).parents('tr').find('td').index($(e.target).parents('td'));
                if (dateIndex >= 0) {
                    let targetDate: Date = this.controller.dates[dateIndex - 1].date;
                    let taskDate: Date = (<string>ui.draggable.attr('task-date')).parsePipedDate();
                    validDrop = targetDate.isSameDayAs(taskDate);
                } else {
                    validDrop = false;
                }
            }
        } else if (objectType === 'order') {
            validDrop = ($(e.target).hasClass('planning-shift') || $(e.target).hasClass('shift-drop-zone'));
        } else if (objectType === 'shift') {
            this.dragOverEmployeeId = Number($(e.target).parents('tr')[0]['id'].replace('empId', ''));

            validDrop = ($(e.target).hasClass('planning-shift') || $(e.target).hasClass('shift-drop-zone'));
        }

        const $target = $(e.target);
        const $parentRow = $target.parents('tr');

        if (validDrop) {
            $target.addClass('drop-target');
            $parentRow.addClass('drop-target');
        }
        else {
            $target.removeClass('drop-target');
            $parentRow.removeClass('drop-target');
        }
    }

    private rightListOut(e, ui) {
        const $target = $(e.target);
        const $parentRow = $target.parents('tr');

        $target.removeClass('drop-target');
        $parentRow.removeClass('drop-target');
    }

    private onRightListDrop(e: Event, ui: JQueryUI.DroppableEventUIParam) {
        let $target = $(e.target);
        if (!$target.hasClass('drop-target'))
            return;

        // Get target information
        $target.removeClass('drop-target');
        let info = this.getInfoFromElem($target);
        this.lastDropEvent = info;
        this.droppedWasHandled = true;

        let objectType: string = ui.draggable.attr('drag-object-type');
        if (objectType === 'employee') {
            // Get id of employee post or shift that the employee was dropped on.
            // On employee posts, id is set on the row, as empPostIdNNN.
            // On shifts, id is set directly on the td.
            let targetId: number = this.controller.isEmployeePostView ? parseInt($target.parents('tr').attr('id').substring(9), 10) : parseInt($target.attr('id'), 10);
            if ($target.hasClass('planning-employee-post'))
                info['employeePostId'] = targetId;
            else if ($target.hasClass('planning-shift'))
                info['shiftId'] = targetId;

            // Get source information
            let sourceEmployeeId = parseInt(ui.draggable.attr('id'), 10);
            info['fromEmployeeId'] = sourceEmployeeId;

            //Due to how the schedule is designed, we get two events on dayview if you drop on a shift. One for the actual shift, and one time for the actual date dropped on. If dropped on a shift, we dont care.
            if (this.controller.isDayView && this.lastDropEvent && this.lastDropEvent['shiftId'] && !info['shiftId'] && sourceEmployeeId === this.lastDropEvent['fromEmployeeId'] && info.employee === this.lastDropEvent.employee) {
                this.lastDropEvent = null;
                return;
            }

            // If employee is dropped in an empty slot, take no action
            if (!targetId)
                return;

            if ($target.hasClass('planning-employee-post'))
                this.controller.employeeDroppedOnEmployeePost(sourceEmployeeId, targetId);
            else
                this.controller.employeeDroppedOnShift(sourceEmployeeId, targetId);
        } else if (objectType === 'task') {
            // Get source information
            let taskId: number = parseInt(ui.draggable.attr('id'), 10);
            let taskType: number = parseInt(ui.draggable.attr('task-type'), 10);
            let taskDate: Date = (<string>ui.draggable.attr('task-date')).parsePipedDate();
            let taskStartTime: Date = (<string>ui.draggable.attr('task-start-time')).parsePipedTime(taskDate);
            let task: StaffingNeedsTaskDTO = this.controller.unscheduledTasks.find(t => t.id === taskId && t.type === taskType && t.startTime.isSameMinuteAs(taskStartTime));
            if (task) {
                if (this.controller.isEmployeePostView) {
                    if (info.employee.employeePostStatus !== SoeEmployeePostStatus.Locked)
                        this.controller.openAssignTaskToEmployeePost([task], info.employee, info.date);
                } else if (this.controller.isTemplateView) {
                    this.controller.openAssignTaskToEmployeePost([task], info.employee, info.date);
                } else {
                    this.controller.openAssignTaskToEmployee([task], info.employee, info.date);
                }
            }
        } else if (objectType === 'order') {
            // Get source information
            let orderId: number = parseInt(ui.draggable.attr('id'), 10);
            let order: OrderListDTO = this.controller.orderList.find(o => o.orderId === orderId);
            if (order) {
                let shift: ShiftDTO = new ShiftDTO(TermGroup_TimeScheduleTemplateBlockType.Order);
                shift.order = order;
                shift.employeeId = info.employee.employeeId;
                shift.shiftTypeId = order.shiftTypeId;
                shift.shiftTypeName = order.shiftTypeName;
                shift.actualStartTime = info.date;  // TODO: In day view, keep time from slot
                shift.link = Guid.newGuid();

                this.controller.openEditAssignment(shift, info.date, info.employee.employeeId);
            }
        } else if (objectType === 'shift' && this.controller.isCommonDayView) {
            ui.draggable.css('left', ui.position.left);
            ui.draggable.css('top', ui.position.top);
            let { row: targetRow, date: targetDate, employee: targetEmployee, shift: targetShift } = this.getInfoFromElem($target);
            let { row: sourceRow, date: sourceDate, employee: sourceEmployee } = this.getInfoFromElem(ui.draggable);

            // Get all selected shifts
            // If only dragging one shift, it will not be selected
            // In that case use current
            let shift = this.scheduleHandler.getShiftFromJQueryElem(ui.draggable);
            let selected: ShiftDTO[] = shift.selected ? this.scheduleHandler.getSelectedShifts() : [shift];

            let minutesMoved = this.getMinutesMoved(e);
            minutesMoved = this.adjustToClosest(minutesMoved, this.dragRoundingMinutes);

            if (sourceEmployee.employeeId === targetEmployee.employeeId) {
                if (this.controller.isTemplateDayView || this.controller.isEmployeePostDayView)
                    this.saveTemplateShiftsForDayView(selected, minutesMoved);
                else {
                    this.saveShiftsForDayView(selected, minutesMoved).then(success => {
                        if (!success) {
                            // Restore
                            this.scheduleHandler.updateEmployeeRow(sourceRow, sourceEmployee);
                        }
                    });
                }
            } else {
                this.controller.openDragShiftByIds(selected.map(s => s.timeScheduleTemplateBlockId), targetEmployee, targetDate, targetShift, 0);
            }
        } else if (objectType === 'shift' && (this.controller.isScheduleView || this.controller.isTemplateScheduleView || this.controller.isEmployeePostScheduleView || this.controller.isScenarioScheduleView || this.controller.isScenarioCompleteView)) {
            ui.draggable.css('left', ui.position.left);
            ui.draggable.css('top', ui.position.top);
            let { row: targetRow, date: targetDate, employee: targetEmployee, shift: targetShift } = this.getInfoFromElem($target);
            let { row: sourceRow, date: sourceDate, employee: sourceEmployee } = this.getInfoFromElem(ui.draggable);

            let shift = this.scheduleHandler.getShiftFromJQueryElem(ui.draggable);
            let duration = this.scheduleHandler.getShiftLengthInDays(shift);

            if (duration > 1) { //If its an item that spans several days, the user might click on a part of the elem that is in say day 3. The drag and drop does not know that so we need to recalculate what actual date of the shift was dropped.
                let width = $target.width();
                let pixelsPerDay = width / duration;
                let days = Math.ceil(this.offsetX / pixelsPerDay) - 1;
                if (days > 0)
                    targetDate = targetDate.addDays(-1 * days);
            }

            // Get all selected shifts
            // If only dragging one shift, it might not be selected
            // In that case use current
            if (!shift.selected) {
                this.scheduleHandler.clearSelectedShifts();
                this.scheduleHandler.selectShift(shift);
            }
            let selected: ShiftDTO[] = this.scheduleHandler.getSelectedShifts();

            this.revertDraggedShifts = false;
            // Check that target schedule is not read only
            let isReadonly: boolean = false;
            if (this.controller.isTemplateView) {
                let template = targetEmployee.getTemplateSchedule(targetDate);
                if (!template || targetDate.isAfterOnDay(this.controller.dateFrom.addDays(template.noOfDays - 1)))
                    isReadonly = true;
            } else if (this.controller.isEmployeePostView) {
                if (targetEmployee.employeePostStatus === SoeEmployeePostStatus.Locked)
                    isReadonly = true;
                else {
                    let template = targetEmployee.getTemplateSchedule(targetDate);
                    if (!template || targetDate.isAfterOnDay(this.controller.dateFrom.addDays(template.noOfDays - 1)))
                        isReadonly = true;
                }
            }

            if (isReadonly) {
                this.scheduleHandler.updateEmployeeRow(sourceRow, sourceEmployee);
                if (sourceEmployee.identifier !== targetEmployee.identifier)
                    this.scheduleHandler.updateEmployeeRow(targetRow, targetEmployee);
            } else {
                let diffDays = targetDate.diffDays(this.currentDragShift.actualDateOnLoad);
                if (this.currentDragShift.belongsToPreviousDay) {
                    diffDays--;
                    targetDate = targetDate.addDays(-1);
                } else if (this.currentDragShift.belongsToNextDay) {
                    diffDays++;
                    targetDate = targetDate.addDays(1);
                }
                this.controller.openDragShiftByIds(selected.map(s => s.timeScheduleTemplateBlockId), targetEmployee, targetDate, targetShift, diffDays);
            }
        } else {
            this.droppedWasHandled = false;
        }
    }

    // HELP-METHODS

    public reset() {
        this.moveShiftContainment = null;
    }

    private refreshSortable = _.debounce(() => {
        let rowNr = 0;
        _.forEach($('.planning-scheduleview tbody > tr'), row => {
            rowNr++;
            if (HtmlUtility.isElementInViewport(row) || rowNr === 1) {
                $(row).find('.shift-drop-zone').each((i, e) => {
                    if ($(e).sortable("instance")) {
                        $(e).sortable("refresh");
                    }
                });
            }
        });
    }, 200, { leading: false, trailing: true });

    private getHelperDay(e, elem) {
        let $elem = $(elem);
        let shift = this.scheduleHandler.getShiftFromJQueryElem($elem);

        // Linked shifts can be dragged separately if not selected first,
        // therefore we always reselect the shift including linked shifts.
        if (!shift.selected)
            this.scheduleHandler.clearSelectedShifts();
        this.scheduleHandler.selectShift(shift);

        this.offsetX = e.offsetX;
        this.clientX = e.clientX;

        let tr = $elem.parents('tr'); //Find the parent row

        let clone = tr.clone(); //clone it so that we can create a drag-item from it
        //let baseChildren = tr.children();

        let table = angular.element('<table>');
        clone.appendTo(table); //need to add a tr to a table to make it display correctly

        this.removeHiddenTds(clone, tr, $elem);

        clone.children('td').removeClass('planning-day planning-day-saturday planning-day-sunday planning-day-today planning-day-available planning-day-unavailable planning-day-partly-available planning-day-partly-unavailable planning-day-mixed-available planning-day-no-employment planning-day-has-employee-schedule planning-day-outside-scenario planning-day-first-day-of-template-group planning-day-first-day-of-template planning-day-last-day-of-template-group planning-day-last-day-of-template').css('border', 'none'); //remove border and background-classes.

        tr.find('.selected-shift').not($elem).hide(); //hide selected children in base row. 
        clone.find('.planning-shift:not(.selected-shift)').hide().find('.shift-time').removeClass('shift-time'); //hide unselected children in cloned row.
        clone.find('.selected-shift').removeClass('selected-shift');

        clone.find('.planning-shift').addClass('helper');
        clone.find('.shift-drop-zone').addClass('helper');

        this.makeSureClickedElementIsUnderCursorDay(table, clone, $elem);
        table.addClass("dragging-shift-helper");
        return table;
    }

    private getHelperSchedule(e, elem) {
        const $elem = $(elem);
        let shift = this.scheduleHandler.getShiftFromJQueryElem($elem);

        // Linked shifts can be dragged separately if not selected first,
        // therefore we always reselect the shift including linked shifts.
        if (!shift.selected)
            this.scheduleHandler.clearSelectedShifts();
        this.scheduleHandler.selectShift(shift);

        this.offsetX = e.offsetX;
        this.clientX = e.clientX;

        // Find the parent row and clone it so that we can create a drag-item from it
        let tr = $elem.parents('tr');
        let clone = tr.clone();
        let baseChildren = tr.children();

        // Set the width of all children so that our clone matches the parent
        clone.children().outerWidth(function (i, val) {
            return baseChildren.eq(i).outerWidth();
        });

        // Need to add a tr to a table to make it display correctly
        let table = angular.element('<table>');
        clone.appendTo(table);

        this.removeHiddenTds(clone, tr, $elem);

        clone.children('td').removeClass('planning-day planning-day-saturday planning-day-sunday planning-day-today planning-day-available planning-day-unavailable planning-day-partly-available planning-day-partly-unavailable planning-day-mixed-available planning-day-no-employment planning-day-has-employee-schedule planning-day-outside-scenario planning-day-first-day-of-template-group planning-day-first-day-of-template planning-day-last-day-of-template-group planning-day-last-day-of-template').css('border', 'none'); //remove border and background-classes.
        // Hide selected children in base row
        tr.find('.selected-shift').not($elem).hide();
        tr.find('.planning-day-today').removeClass('planning-day-today');
        // Hide unselected children in cloned row
        clone.find('.planning-shift:not(.selected-shift)').hide();
        // Hide some icons
        clone.find("i.fa-arrow-from-right").hide();
        clone.find("i.fa-arrow-from-left").hide();
        clone.find("i.fa-repeat").hide();
        clone.find("i.fa-sign-out").hide();
        clone.find("i.fa-calendar-times").hide();
        clone.find("i.fa-comment-dots").hide();

        clone.find('.planning-shift').addClass('helper');
        clone.find('.shift-drop-zone').addClass('helper');

        this.makeSureClickedElementIsUnderCursorSchedule(table, clone, $elem);
        table.addClass("dragging-shift-helper");

        return table;
    }

    public getInfoFromElem(elem) {
        let row = elem.parents('tr');
        let dateIndex = row.children('td').index(elem.parents('td'));

        // If planning periods is visible, employee element contains one extra td
        if ($(row.children('td')).hasClass('planning-employee-annual-time'))
            dateIndex--;

        let date: Date = dateIndex >= 0 ? this.controller.dates[dateIndex - 1].date : null;
        let employeeId: number;
        let employee: EmployeeListDTO;
        let shift: ShiftDTO;

        let objectType: string = elem.attr('drag-object-type');
        if (objectType && objectType === 'shift') {
            shift = this.scheduleHandler.getShiftFromJQueryElem(elem);
            if (shift && !shift.actualDateOnLoad.isSameDayAs(date))
                date = shift.actualDateOnLoad;
        }

        if (this.controller.isEmployeePostView) {
            employeeId = row.data('empPostId');
            if (employeeId)
                employee = this.controller.getEmployeePostById(employeeId);
        } else {
            employeeId = row.data('empId');
            if (employeeId)
                employee = this.controller.getEmployeeById(employeeId);
        }

        let accountId = $(row).attr('account-id');
        if (employee && accountId) {
            employee['accountId'] = accountId;
        }

        return { row, date, employee, shift };
    }

    private adjustToClosestMinutes(date: Date, minutes: number) {
        let diff = date.minutes() % minutes;

        if (!diff)
            return date;

        if (diff > minutes / 2) {
            return date.addMinutes(minutes - diff);
        } else {
            return date.addMinutes(-diff);
        }
    }

    private adjustToClosest(number, interval): number {
        let result: number = number;

        let diff = number % interval;
        if (diff) {
            if (diff > interval / 2) {
                result = number + interval - diff;
            } else {
                result = number - diff;
            }
        }

        return result.round(0);
    }

    private updatedTimeOnShiftElem: boolean = false;
    private updateTimeOnShiftElem(elem, minutesMoved: number) {
        let shift = this.scheduleHandler.getShiftFromJQueryElem(elem);
        let start = this.adjustToClosestMinutes(shift.actualStartTime.addMinutes(minutesMoved), this.dragRoundingMinutes);
        let stop = this.adjustToClosestMinutes(shift.actualStopTime.addMinutes(minutesMoved), this.dragRoundingMinutes);
        let time = this.formatStartStopDate(start, stop);

        this.setShiftTime(this.getOrCreateShiftTimeElement($(elem)), time);
        this.updatedTimeOnShiftElem = true;

        // Also update break times
        [1, 2, 3, 4].forEach(breakNr => {
            this.updateTimeOnBreakElem($(elem), shift, breakNr, minutesMoved);
        });
    }

    private updateTimeOnBreakElem(shiftElem: JQuery, shift: ShiftDTO, breakNr: number, minutesMoved: number) {
        if (shift[`break${breakNr}TimeCodeId`]) {
            let breakElem: JQuery = shiftElem.parent().find('#' + shift[`break${breakNr}Id`] + '.shift-break');
            if (breakElem) {
                let breakTimeElem = this.getBreakTimeElement(breakElem);
                if (breakTimeElem) {
                    let breakStart = this.adjustToClosestMinutes(shift[`break${breakNr}StartTime`].addMinutes(minutesMoved), this.dragRoundingMinutes);
                    let breakStop = this.adjustToClosestMinutes(breakStart.addMinutes(shift[`break${breakNr}Minutes`]), this.dragRoundingMinutes);
                    let breakTime = this.formatStartStopDate(breakStart, breakStop);
                    this.setBreakTime(breakTimeElem, breakTime);
                }
            }
        }
    }

    private restoreTimeOnShiftElem(elem) {
        if (this.updatedTimeOnShiftElem) {
            let shift = this.scheduleHandler.getShiftFromJQueryElem(elem);
            let time = this.formatStartStopDate(shift.actualStartTime, shift.actualStopTime);

            this.setShiftTime(this.getOrCreateShiftTimeElement($(elem)), time);

            // Also update break times
            [1, 2, 3, 4].forEach(breakNr => {
                this.updateTimeOnBreakElem($(elem), shift, breakNr, 0);
            });

            this.updatedTimeOnShiftElem = false;
        }
    }

    public formatStartStopDate(start: Date, stop: Date) {
        let filter = this.$filter('date');
        return filter(start, 'shortTime') + '-' + filter(stop, 'shortTime');
    }

    private removeHiddenTds(clone, tr, elem) {
        // This method removes stuff that isn't visible but would still be there on the dragged element.
        // This caused scrollbars to appear to soon horizontally since the hidden elems still had a width.

        let td = elem.parents('td');
        let tds = tr.children('td');
        let index = tds.index(td); // Index of the clicked td

        let toRemove = [];
        let clonedTds = clone.children('td');
        for (let j = 0; j < index; j++) {
            // Remove everything before
            if (!$(clonedTds[j]).find('.selected-shift').length)
                // Remove every td until we find one with a selected shift in it, otherwise we drag stuff we don't want
                toRemove.push(clonedTds[j]);
            else
                break;
        }

        for (var k = tds.length - 1; k > 0; k--) {
            // Remove everything after
            if (!$(clonedTds[k]).find('.selected-shift').length)
                // Remove every td until we find one with a selected shift in it, otherwise we drag stuff we don't want
                toRemove.push(clonedTds[k]);
            else
                break;
        }

        $(toRemove).remove();
    }

    private makeSureClickedElementIsUnderCursorDay(table, clone, elem) {
        let shift = $(clone.find('#' + elem.attr('id')));
        if (shift) {
            let leftStyle: string = shift.css('left');
            let leftMargin: number = parseFloat(leftStyle.left(leftStyle.length - 2));
            table.css('margin-left', -leftMargin);

            let topStyle: string = shift.css('top');
            let topMargin: number = parseFloat(topStyle.left(topStyle.length - 2));
            table.css('margin-top', -topMargin);
        }
    }

    private makeSureClickedElementIsUnderCursorSchedule(table, clone, elem) {
        // This makes the clicked element appear under the cursor in case of multiselect
        let currentTd = clone.find('#' + elem.attr('id')).parents('td');
        let prev = currentTd.prev('td');
        let width = 0;
        while (prev && prev.length) {
            width += prev.width();
            prev = prev.prev('td');
        }
        // If we add the total width of the tablerow up until the actual clicked td and apply it as negative left margin, the clicked element appears under the cursor
        table.css('margin-left', -1 * width);

        // Recalc position of shifts in the moved table since we might move index 2 and 3, and it will look bad if they are below the cursor, so we move them up to index 1 and 2
        let diff: number = 0;
        currentTd.find('.selected-shift').each((index, e) => {
            let currentTop = this.getTopPositionFromElem(e);
            let newTop = this.controller.getShiftTopPosition(index);
            diff = currentTop - newTop;
        });

        if (diff != 0) {
            clone.find('.selected-shift').each((index, e) => {
                let currentTop = this.getTopPositionFromElem(e);
                let newTop = currentTop - diff;
                $(e).css('top', newTop);
            });
        }
    }

    private getTopPositionFromElem(e): number {
        return parseInt($(e).css('top').match(/\d+/g).join(''), 10);
    }
}
