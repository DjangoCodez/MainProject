import { DragDropHelper } from "./DragDropHelper";
import { DateDay, EditController } from "./EditController";
import { PlanningEditModes, PlanningOrderListSortBy } from "../../../Util/Enumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { EmployeeListDTO } from "../../../Common/Models/EmployeeListDTO";
import { ShiftDTO, SlotDTO } from "../../../Common/Models/TimeSchedulePlanningDTOs";
import { GraphicsUtility } from "../../../Util/GraphicsUtility";
import { SoeEmployeePostStatus, TermGroup_StaffingNeedsDayViewGroupBy, TermGroup_StaffingNeedsScheduleViewGroupBy, TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat, TermGroup_TimeSchedulePlanningBreakVisibility, TermGroup_TimeSchedulePlanningShiftStyle, TermGroup_TimeSchedulePlanningShiftTypePosition, TermGroup_TimeSchedulePlanningTimePosition, TermGroup_TimeScheduleTemplateBlockShiftUserStatus, XEMailAnswerType } from "../../../Util/CommonEnumerations";
import { ContactAddressItemDTO } from "../../../Common/Models/ContactAddressDTOs";
import { StaffingNeedsHeadDTO, StaffingNeedsRowDTO, StaffingNeedsRowPeriodDTO, StaffingNeedsTaskDTO, StaffingStatisticsIntervalValue } from "../../../Common/Models/StaffingNeedsDTOs";
import { DateRangeDTO } from "../../../Common/Models/DateRangeDTO";
import { ChartHandler } from "./ChartHandler";

export class ScheduleHandler {
    private dragDropHelper: DragDropHelper;
    private chartHandler: ChartHandler;
    private columnWidth: number;
    private scheduleWidth: number;
    public pixelsPerTimeUnit: number;
    private renderInterval: any;
    private scrollY;
    private currentTime: Date;

    // Flags
    private firstTimeScheduleSetup: boolean = true;
    public isRendering: boolean = false;

    // Filters
    private amountFilter: any;

    constructor(private controller: EditController, private $filter: ng.IFilterService, private $timeout: ng.ITimeoutService, private $interval: ng.IIntervalService, private $q: ng.IQService, private $scope: ng.IScope, private $compile: ng.ICompileService) {
        this.dragDropHelper = new DragDropHelper(controller, this, $filter, $q);
        this.chartHandler = new ChartHandler(controller, this, this.$timeout, this.$interval, this.$q, this.$scope, this.$compile);

        this.amountFilter = $filter("amount");

        this.renderEmployeeRow = this.renderEmployeeRow.bind(this);
        this.renderStaffingNeedsRow = this.renderStaffingNeedsRow.bind(this);
    }

    // SETUP

    private performFirstTimeSetup() {
        this.firstTimeScheduleSetup = false;
        let self = this;

        // Events

        // Single click on empty slot
        $('.planning-scheduleview').on('click', '.empty-slot', function (event) {
            if (self.controller.isTasksAndDeliveriesView) {
                self.clearSelectedTasks();
            } else {
                if (!self.controller.hasCurrentViewModifyPermission)
                    return;

                let elem = $(this);
                let slot = self.getSlotFromElement(elem);
                self.controller.setSlotReadOnly(slot);

                // User can select multiple slots (using the ctrl key) as long as it's the same employee
                for (let s of self.getSelectedSlots()) {
                    if (event.ctrlKey && s.employeeId !== slot.employeeId || !event.ctrlKey) {
                        self.clearSelectedSlots();
                        break;
                    }
                }

                self.clearSelectedShifts(true);
                self.selectSlot(elem);
            }
        });

        // Double click on empty slot
        $('.planning-scheduleview').on('dblclick', '.empty-slot', function (event) {
            if (!self.controller.hasCurrentViewModifyPermission || self.controller.isTasksAndDeliveriesView)
                return;

            // New shift on double click
            let elem = $(this);
            let slot = self.getSlotFromElement(elem);
            self.controller.setSlotReadOnly(slot);
            if (self.controller.isSchedulePlanningMode && !self.controller.isStandbyView && !slot.isReadOnly)
                self.controller.editShift(null, slot.startTime, slot.employeeId, false, false);
        });

        // Single click on shift, task or leisure code
        $('.planning-scheduleview').on('click', '.planning-shift', function (event) {
            if (self.controller.isTasksAndDeliveriesView) {
                let elem = $(this);
                let task = self.getTaskFromJQueryElem(elem);

                self.getSelectedTasks().forEach(t => {
                    self.unselectTask(t);
                });

                if (task.selected)
                    self.unselectTask(task);
                else
                    self.selectTask(task);
            } else {
                if (!self.controller.hasCurrentViewModifyPermission)
                    return;

                let shift = self.getShiftFromJQueryElem(this);

                if (event.shiftKey) {
                    // User selected a shift while holding the shift key down
                    // Select all shifts between first and last in range
                    let selectedShifts = _.sortBy(self.getSelectedShifts(), s => s.actualStartTime);
                    if (selectedShifts.length > 0 && selectedShifts[0].employeeId === shift.employeeId) {
                        let start = _.first(selectedShifts).actualStartTime;
                        if (shift.actualStartTime.isBeforeOnMinute(start))
                            start = shift.actualStartTime;
                        let stop = _.last(selectedShifts).actualStopTime;
                        if (shift.actualStopTime.isAfterOnDay(stop))
                            stop = shift.actualStopTime;

                        let firstSelected = _.first(selectedShifts);
                        self.selectShifts(self.controller.shifts.filter(s => s.employeeId === shift.employeeId && s.isVisible && s.type === firstSelected.type && s.isAbsence === firstSelected.isAbsence && s.isLeisureCode === firstSelected.isLeisureCode && s.actualStartTime.isSameOrAfterOnMinute(start) && s.actualStopTime.isSameOrBeforeOnMinute(stop)), true, true);
                    } else {
                        self.clearSelectedShifts();
                    }

                    // Can't select shifts from different templates
                    if (self.controller.isTemplateView && _.uniqBy(self.getSelectedShifts(), s => s.timeScheduleTemplateHeadId).length > 1)
                        self.clearSelectedShifts();
                } else {
                    self.getSelectedShifts().forEach(s => {
                        if (self.controller.isTemplateView && s.timeScheduleTemplateHeadId != shift.timeScheduleTemplateHeadId) {
                            // Can't select shifts from different templates
                            self.unselectShift(s);
                        } else {
                            if (event.ctrlKey) {
                                // User can select multiple shifts (using the ctrl key) as long as it's the same employee and same type of shift
                                if (s.employeeId !== shift.employeeId || s.type !== shift.type || (s.isAbsence && !shift.isAbsence) || (!s.isAbsence && shift.isAbsence) || s.isLeisureCode !== shift.isLeisureCode)
                                    self.unselectShift(s);
                            } else {
                                // Ctrl key was not pressed, unselect all other shifts
                                self.unselectShift(s);
                            }
                        }
                    });

                    if (shift.selected)
                        self.unselectShift(shift, true, true);
                    else {
                        self.selectShift(shift, true, true);
                    }
                }
            }
        });

        // Double click on shift/task
        $('.planning-scheduleview').on('dblclick', '.planning-shift', function (event) {
            if (self.controller.isTasksAndDeliveriesView) {
                let elem = $(this);
                let task = self.getTaskFromJQueryElem(elem);
                if (task) {
                    self.selectTask(task);
                    self.controller.editTask(task);
                }
            } else {
                if (!self.controller.hasCurrentViewModifyPermission)
                    return;

                // Edit shift on double click
                let elem = $(this);
                let shift = self.getShiftFromJQueryElem(elem);
                if (shift) {
                    self.selectShift(shift);
                    if (shift.isLeisureCode)
                        self.controller.editLeisureCode(shift);
                    else if ((shift.isSchedule || shift.isStandby || shift.isOnDuty) && !shift.isAnnualLeave)
                        self.controller.editShift(shift, null, null, shift.isStandby, shift.isOnDuty);
                    else if (shift.isOrder)
                        self.controller.editAssignment(shift);
                    else if (shift.isBooking)
                        self.controller.editBooking(shift);
                }
            }
        });

        // Double click on employee
        $('.planning-scheduleview').on('dblclick', '.planning-employee', function (event) {
            if (!self.controller.hasCurrentViewModifyPermission || self.controller.isStandbyView)
                return;

            // Edit employee on double click
            let elem = $(this);
            let employee = self.getEmployeeFromElem(elem);
            if (employee)
                self.controller.editEmployee(employee);
        });

        // Single click on employee post
        $('.planning-scheduleview').on('click', '.planning-employee-post', function (event) {
            // Mark employee post on single click
            let elem = $(this);
            let employeePost = self.getEmployeePostFromElem(elem);
            if (employeePost) {
                if (employeePost.selected)
                    self.unselectEmployeePost(employeePost, true);
                else {
                    self.selectEmployeePost(employeePost, true);
                }
            }
        });

        // Double click on employee post
        $('.planning-scheduleview').on('dblclick', '.planning-employee-post', function (event) {
            if (!self.controller.hasCurrentViewModifyPermission)
                return;

            // Edit employee post on double click
            let elem = $(this);
            let employeePost = self.getEmployeePostFromElem(elem);
            if (employeePost)
                self.controller.editEmployeePost(employeePost);
        });

        // Resize window
        $(window).resize((e) => {
            // This event is also fired when a shift is dragged or resized.
            // Do not run updateWidthOnAllElements() in those cases.
            let isResizebleElement = $(e.target).hasClass('ui-resizable');
            if (!isResizebleElement) {
                // When left menu is toggled, it has a 500 ms animation,
                // so we need to increase the delay a bit.
                this.controller.setDateColumnWidth(600).then(() => {
                    this.updateWidthOnAllElements(0, this.controller.isStaffingNeedsView);
                });
            }
        });

        this.dragDropHelper.performFirstTimeSetup();
    }

    // RENDER

    // Common

    public clearScheduleViewBody(): ng.IPromise<any> {
        return this.clearAttachmentPoint('.planning-scheduleview tbody');
    }

    public clearScheduleViewSummary(): ng.IPromise<any> {
        return this.clearAttachmentPoint('.planning-summary-row');
    }

    public clearPlanningSummary(): ng.IPromise<any> {
        return this.clearAttachmentPoint('.staffing-needs-summary-row');
    }

    public clearPlanningSummaryEmployees(): ng.IPromise<any> {
        return this.clearAttachmentPoint('.staffing-needs-summary-employees-row');
    }

    private clearAttachmentPoint(name: string): ng.IPromise<any> {
        return this.$timeout(() => {
            let attachmentPoint = $(name);
            attachmentPoint.empty();
        });
    }

    public rememberVerticalScroll() {
        this.scrollY = window.scrollY;
    }

    private restoreVerticalScroll() {
        window.scrollTo(0, this.scrollY);
    }

    // Calendar
    public renderCalendar() {
        this.stopRenderLoop();
        this.isRendering = true;

        this.$timeout(() => {//This timeout is here to give IE time to stop the rendering. It seems that even if we are cancelling the rendering, IE might still be working on an old one since its so damn slow.
            this.renderCalendarBody().then(() => {
                this.controller.renderingDone();
                this.restoreVerticalScroll();
                this.isRendering = false;

                if (this.firstTimeScheduleSetup) {
                    this.performFirstTimeSetup();
                }
            });
        });
    }

    private renderCalendarBody(): ng.IPromise<any> {
        return this.clearScheduleViewBody();
    }

    // Schedule
    public renderSchedule(stopProgressWhenDone: boolean) {
        this.currentTime = new Date();

        this.stopRenderLoop();//in case we are still rendering something, we need to stop so that we dont start two different renderings. Like one for 2 weeks and one for 3 weeks..
        this.isRendering = true;

        this.chartHandler.renderPlanningAgChartRow();
        this.renderPlanningFollowUpTableRow();
        this.renderEmptyScheduleSummary();
        this.renderScheduleBody().then(() => {
            this.renderScheduleSummary();

            this.dragDropHelper.reset();

            // If changing number of weeks, the width calculation is done before rendered,
            // therefore we need to update the width again on all shifts after rendered
            if (!this.controller.isCommonDayView && this.controller.nbrOfColumnsChanged)
                this.updateWidthOnAllElements();

            if (this.controller.showPlanningAgChart) {
                this.controller.createPlanningAgChartData();
                this.controller.renderPlanningAgChart(false);
            }

            if (this.controller.showPlanningFollowUpTable)
                this.controller.createPlanningFollowUpTableData(true);

            this.controller.renderingDone(stopProgressWhenDone);
            this.restoreVerticalScroll();
            this.isRendering = false;

            if (this.firstTimeScheduleSetup) {
                this.performFirstTimeSetup();
            }
        });
    }

    public setOrderListHeight(useTimeout: boolean = true) {
        if (this.controller.isOrderPlanningMode) {
            this.$timeout(() => {
                let schedule = $('.planning-scheduleview table');
                let orderList = (this.controller.orderListSortBy === PlanningOrderListSortBy.PlannedStartDate || this.controller.orderListSortBy === PlanningOrderListSortBy.PlannedStopDate) ? $(".unscheduledorderdate-list") : $(".unscheduledorder-list");
                let header = $(".unscheduledorder-list-filter-header");
                if (schedule && orderList && header) {
                    let newHeight = schedule.height() - header.height() - 11;
                    if (newHeight < 300)
                        newHeight = 300;
                    orderList.css('height', newHeight);
                }
                this.enableDragAndDropOfOrders();
            }, useTimeout ? 1000 : 0);
        }
    }

    public clearPlanningAgChartElem() {
        this.chartHandler.clearPlanningAgChartElem();
    }

    public renderPlanningAgChart() {
        this.chartHandler.renderPlanningAgChart();
    }

    public setPlanningAgChartData(data: any[]) {
        this.chartHandler.setPlanningChartData(data);
    }

    private renderPlanningFollowUpTableRow() {
        let attachmentPoint = $('.planning-followup-table-row');
        attachmentPoint.empty();

        let name = $('<td>');
        name.addClass('staffing-needs-row-identifier');
        name.addClass('link');
        name.attr('colspan', 2);
        name.text(this.controller.terms["core.table"]);
        name.attr('ng-attr-title', '{{(ctrl.showPlanningFollowUpTable ? ctrl.terms["core.hide"] : ctrl.terms["core.show"]) + " " + ctrl.terms["core.table"].toLowerCase()}}');
        name.attr('data-ng-click', 'ctrl.showPlanningFollowUpTable = !ctrl.showPlanningFollowUpTable; ctrl.renderPlanningFollowUpTable(ctrl.showPlanningFollowUpTable);');

        let icon = $('<i>');
        icon.addClass('far');
        icon.attr('data-ng-class', "{\'fa-chevron-down\': ctrl.showPlanningFollowUpTable, \'fa-chevron-right\': !ctrl.showPlanningFollowUpTable}");
        name.append(icon);

        let excelDiv = $('<div>');
        excelDiv.addClass('margin-large-top margin-large-bottom');
        excelDiv.attr('data-ng-if', 'ctrl.showPlanningFollowUpTable');
        let excel = $('<i>');
        excel.addClass('fal fa-file-excel');
        excel.attr('title', this.controller.terms["core.exportexcel"]);
        excel.attr('style', 'font-size: 20px;');
        excel.attr('data-ng-if', 'ctrl.showPlanningFollowUpTableRows');
        excel.attr('data-ng-click', '$event.stopPropagation();ctrl.exportPlanningFollowUpTableToExcel();');
        excelDiv.append(excel);
        name.append(excelDiv);

        let reloadIcon = $('<i>');
        reloadIcon.addClass('fal fa-sync');
        reloadIcon.attr('data-ng-class', "{'fa-spin':ctrl.loadingSelectableInformation}");
        reloadIcon.attr('data-ng-if', 'ctrl.showPlanningFollowUpTable');
        reloadIcon.attr('style', 'font-size: 20px;');
        reloadIcon.attr('ng-attr-title', '{{ctrl.terms["core.reload_data"]}}');
        reloadIcon.attr('data-ng-click', '$event.stopPropagation(); ctrl.loadStaffingNeed();');
        excelDiv.append(reloadIcon);

        attachmentPoint.append(this.$compile(name)(this.$scope));

        let td = $('<td>');
        td.addClass('planning-followup-table');
        td.attr('colspan', this.controller.dates.length);
        attachmentPoint.append(td);
    }

    public renderPlanningFollowUpTable() {
        let attachmentPoint = $('.planning-followup-table');
        attachmentPoint.empty();

        if (this.controller.showPlanningFollowUpTable) {
            let table = $('<table>');
            table.attr('id', 'planning-followup-table');
            attachmentPoint.append(table);

            // Build table head
            let thead = $('<thead>');
            table.append(thead);

            let headTr1 = $('<tr>');
            thead.append(headTr1);

            let headDateTd = $('<td>');
            headDateTd.attr('rowspan', 2);
            headDateTd.addClass('link');
            headDateTd.text(this.controller.terms["common.date"]);
            headDateTd.attr('ng-attr-title', '{{(ctrl.showPlanningFollowUpTableRows ? ctrl.terms["core.hide"] : ctrl.terms["core.show"]) + " " + ctrl.terms["common.rows"].toLowerCase()}}');
            headDateTd.attr('data-ng-click', 'ctrl.showPlanningFollowUpTableRows = !ctrl.showPlanningFollowUpTableRows; ctrl.renderPlanningFollowUpTable(false);');

            let icon = $('<i>');
            icon.addClass('far');
            icon.attr('data-ng-class', "{\'fa-chevron-down\': ctrl.showPlanningFollowUpTableRows, \'fa-chevron-right\': !ctrl.showPlanningFollowUpTableRows}");
            headDateTd.append(icon);

            headTr1.append(this.$compile(headDateTd)(this.$scope));
            if (this.showFollowUpTableMainColumn(0)) {
                let headSalesTd = $('<td>');
                headSalesTd.attr('colspan', this.getNbrOfSubColumns(0));
                headSalesTd.text(this.controller.gaugeSalesLabel);
                headTr1.append(headSalesTd);
            }
            if (this.showFollowUpTableMainColumn(1)) {
                let headHoursTd = $('<td>');
                headHoursTd.attr('colspan', this.getNbrOfSubColumns(1));
                headHoursTd.text(this.controller.gaugeHoursLabel);
                headTr1.append(headHoursTd);
            }
            if (this.showFollowUpTableMainColumn(2)) {
                let headCostTd = $('<td>');
                headCostTd.attr('colspan', this.getNbrOfSubColumns(2));
                headCostTd.text(this.controller.gaugeCostLabel);
                headTr1.append(headCostTd);
            }
            if (this.showFollowUpTableMainColumn(3)) {
                let headSalaryPercentTd = $('<td>');
                headSalaryPercentTd.attr('colspan', this.getNbrOfSubColumns(3));
                headSalaryPercentTd.text(this.controller.gaugeSalaryPercentLabel);
                headTr1.append(headSalaryPercentTd);
            }
            if (this.showFollowUpTableMainColumn(4)) {
                let headLpatTd = $('<td>');
                headLpatTd.attr('colspan', this.getNbrOfSubColumns(4));
                headLpatTd.text(this.controller.gaugeLPATLabel);
                headTr1.append(headLpatTd);
            }
            if (this.showFollowUpTableMainColumn(5)) {
                let headFpatTd = $('<td>');
                headFpatTd.attr('colspan', this.getNbrOfSubColumns(5));
                headFpatTd.text(this.controller.gaugeFPATLabel);
                headTr1.append(headFpatTd);
            }

            let headTr2 = $('<tr>');
            thead.append(headTr2);

            for (let i = 0; i < 6; i++) {
                if (!this.showFollowUpTableMainColumn(i))
                    continue;

                if (this.showFollowUpTableSubColumn(i, 0)) {
                    let headBudgetTd = $('<td>');
                    headBudgetTd.text(this.controller.terms["time.schedule.planning.followuptable.budget"]);
                    headTr2.append(headBudgetTd);
                }

                if (this.showFollowUpTableSubColumn(i, 1)) {
                    let headForecastTd = $('<td>');
                    headForecastTd.text(this.controller.terms["time.schedule.planning.followuptable.forecast"]);
                    headTr2.append(headForecastTd);
                }

                // Do not show schedule in sales
                if (i > 0) {
                    if (this.showFollowUpTableSubColumn(i, 2)) {
                        let headTemplateTd = $('<td>');
                        headTemplateTd.text(this.controller.terms["time.schedule.planning.followuptable.templateschedule"]);
                        headTr2.append(headTemplateTd);
                    }
                    if (this.showFollowUpTableSubColumn(i, 3)) {
                        let headScheduleTd = $('<td>');
                        headScheduleTd.text(this.controller.terms["time.schedule.planning.followuptable.schedule"]);
                        headTr2.append(headScheduleTd);
                    }
                }

                if (this.showFollowUpTableSubColumn(i, 4)) {
                    let headTimeTd = $('<td>');
                    headTimeTd.text(this.controller.terms["time.schedule.planning.followuptable.time"]);
                    headTr2.append(headTimeTd);
                }

                // Schedule and time only in personell cost
                if (i === 2) {
                    if (this.showFollowUpTableSubColumn(i, 5)) {
                        let headScheduleAndTimeTd = $('<td>');
                        headScheduleAndTimeTd.text(this.controller.terms["time.schedule.planning.followuptable.scheduleandtime"]);
                        headTr2.append(headScheduleAndTimeTd);
                    }
                }
            }

            let body = $('<tbody>');
            table.append(body);

            let dates: Date[] = this.controller.planningFollowUpTableData.map(d => d.date);
            const today = CalendarUtility.getDateToday();
            let isSummaryRow = false;
            for (let date of dates) {
                isSummaryRow = date.isSameDayAs(_.last(dates));

                if (!this.controller.showPlanningFollowUpTableRows && !isSummaryRow)
                    continue;

                let tr = $('<tr>');
                if (isSummaryRow)
                    tr.addClass('summary-row');
                body.append(tr);

                let dateTd = $('<td>');
                if (isSummaryRow) {
                    dateTd.text(this.controller.terms["common.sum"]);
                    dateTd.attr('style', 'font-size: 12px; padding-top: 3px;');

                    if (this.controller.adjustKPIsPermission &&
                        (this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesBudget ||
                            this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursBudget ||
                            this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostBudget ||
                            this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesForecast ||
                            this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursForecast ||
                            this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostForecast)) {

                        let editIcon = $('<i>');
                        editIcon.addClass('fal');
                        editIcon.addClass('fa-pencil');
                        editIcon.addClass('iconEdit');
                        editIcon.addClass('link');
                        editIcon.attr('style', 'padding: 3px; float: right;');
                        editIcon.attr('title', this.controller.terms["time.schedule.planning.followuptable.adjust"]);
                        editIcon.attr('data-ng-click', 'ctrl.openAdjustFollowUpData();');
                        dateTd.append(editIcon);
                    }
                    tr.append(this.$compile(dateTd)(this.$scope));
                } else {
                    dateTd.text(date.toFormattedDate());
                    tr.append(dateTd);
                }

                let dateData = this.controller.planningFollowUpTableData.find(d => d.date.isSameDayAs(date));

                for (let i = 0; i < 6; i++) {
                    if (!this.showFollowUpTableMainColumn(i))
                        continue;

                    let groupData;
                    if (dateData) {
                        if (i === 0)
                            groupData = dateData.sales;
                        else if (i === 1)
                            groupData = dateData.hours;
                        else if (i === 2)
                            groupData = dateData.cost;
                        else if (i === 3)
                            groupData = dateData.salaryPercent;
                        else if (i === 4)
                            groupData = dateData.lpat;
                        else if (i === 5)
                            groupData = dateData.fpat;
                    }

                    for (let j = 0; j < 6; j++) {
                        // Do not show schedule in sales
                        if (i === 0 && (j === 2 || j === 3))
                            continue;

                        // Schedule and time only in personell cost
                        if (i !== 2 && j === 5)
                            continue;

                        if (!this.showFollowUpTableSubColumn(i, j))
                            continue;

                        let td = $('<td>');

                        let fieldData: number = 0;
                        if (groupData) {
                            if (j === 0)
                                fieldData = groupData.budget;
                            else if (j === 1)
                                fieldData = groupData.forecast;
                            else if (j === 2)
                                fieldData = groupData.templateSchedule;
                            else if (j === 3)
                                fieldData = groupData.schedule;
                            else if (j === 4)
                                fieldData = groupData.time;
                            else if (j === 5)
                                fieldData = groupData.scheduleAndTime;
                        }

                        let text: string;
                        if (i === 1) {
                            // Hours
                            text = CalendarUtility.minutesToTimeSpan(fieldData);
                            td.addClass('format-time');
                        } else if (i === 3) {
                            // Salary percent
                            text = fieldData.round(2).toLocaleString(undefined, { minimumFractionDigits: 2 }) + '%';
                            td.addClass('format-percent');
                        } else {
                            text = fieldData.round(0).toLocaleString(undefined, { minimumFractionDigits: 0 });
                            td.addClass('format-amount');
                        }

                        td.text(text);

                        if (isSummaryRow) {
                            // Show original value in tooltip
                            // If value is adjusted show it in a different color
                            if (this.controller.staffingNeedOriginalSummaryRow) {
                                let origFieldData: StaffingStatisticsIntervalValue;
                                if (j === 0)
                                    origFieldData = this.controller.staffingNeedOriginalSummaryRow.budget;
                                else if (j === 1)
                                    origFieldData = this.controller.staffingNeedOriginalSummaryRow.forecast;
                                else if (j === 2)
                                    origFieldData = this.controller.staffingNeedOriginalSummaryRow.templateSchedule;
                                else if (j === 3)
                                    origFieldData = this.controller.staffingNeedOriginalSummaryRow.schedule;
                                else if (j === 4)
                                    origFieldData = this.controller.staffingNeedOriginalSummaryRow.time;
                                else if (j === 5)
                                    origFieldData = this.controller.staffingNeedOriginalSummaryRow.scheduleAndTime;

                                let origValue: number = 0;
                                if (origFieldData) {
                                    if (i === 0)
                                        origValue = origFieldData.sales.round(0);
                                    else if (i === 1)
                                        origValue = origFieldData.hours;
                                    else if (i === 2)
                                        origValue = origFieldData.personelCost.round(0);
                                    else if (i === 3)
                                        origValue = origFieldData.salaryPercent.round(2);
                                    else if (i === 4)
                                        origValue = origFieldData.lpat.round(0);
                                    else if (i === 5)
                                        origValue = origFieldData.fpat.round(0);

                                    let origValueFormatted: string;
                                    if (i === 1)
                                        origValueFormatted = CalendarUtility.minutesToTimeSpan(origValue);
                                    else if (i === 3)
                                        origValueFormatted = origValue.toLocaleString(undefined, { minimumFractionDigits: 2 }) + '%';
                                    else
                                        origValueFormatted = origValue.toLocaleString(undefined, { minimumFractionDigits: 0 });

                                    td.attr('title', `${this.controller.terms["common.initialvalue"]}: ${origValueFormatted}`);
                                    if (fieldData !== origValue)
                                        td.addClass('warningColor');
                                }
                            }
                        } else {
                            if (i === 2 && j === 5) {
                                // Personell cost, Calculated
                                if (date.isBeforeOnDay(today))
                                    td.addClass('scheduleview-header');
                                else
                                    td.addClass('info');
                            }
                        }

                        tr.append(td);
                    }
                }
            }
        }
    }

    private showFollowUpTableMainColumn(mainColumnIndex: number): boolean {
        if (mainColumnIndex === 0) {
            if (!this.controller.selectableInformationSettings.followUpShowCalculationTypeSales)
                return false;
            else if (!this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesBudget && !this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesForecast && !this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesTime)
                return false
        } else if (mainColumnIndex === 1) {
            if (!this.controller.selectableInformationSettings.followUpShowCalculationTypeHours)
                return false;
            else if (!this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursBudget && !this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursForecast && !this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursTemplateSchedule && !this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursSchedule && !this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursTime)
                return false
        } else if (mainColumnIndex === 2) {
            if (!this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCost)
                return false;
            else if (!this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostBudget && !this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostForecast && !this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostTemplateSchedule && !this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostSchedule && !this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostScheduleAndTime && !this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostTime)
                return false
        } else if (mainColumnIndex === 3) {
            if (!this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercent)
                return false;
            else if (!this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentBudget && !this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentForecast && !this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentTemplateSchedule && !this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentSchedule && !this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentTime)
                return false
        } else if (mainColumnIndex === 4) {
            if (!this.controller.selectableInformationSettings.followUpShowCalculationTypeLPAT)
                return false;
            else if (!this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATBudget && !this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATForecast && !this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATTemplateSchedule && !this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATSchedule && !this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATTime)
                return false
        } else if (mainColumnIndex === 5) {
            if (!this.controller.selectableInformationSettings.followUpShowCalculationTypeFPAT)
                return false;
            else if (!this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATBudget && !this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATForecast && !this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATTemplateSchedule && !this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATSchedule && !this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATTime)
                return false
        }

        return true;
    }

    private showFollowUpTableSubColumn(mainColumnIndex: number, subColumnIndex: number): boolean {
        if (mainColumnIndex === 0) {
            if (subColumnIndex === 0 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesBudget || !this.controller.showBudgetPermission))
                return false;
            else if (subColumnIndex === 1 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesForecast || !this.controller.showForecastPermission))
                return false;
            else if (subColumnIndex === 4 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesTime)
                return false;
        } else if (mainColumnIndex === 1) {
            if (subColumnIndex === 0 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursBudget || !this.controller.showBudgetPermission))
                return false;
            else if (subColumnIndex === 1 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursForecast || !this.controller.showForecastPermission))
                return false;
            else if (subColumnIndex === 2 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursTemplateSchedule)
                return false;
            else if (subColumnIndex === 3 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursSchedule)
                return false;
            else if (subColumnIndex === 4 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursTime)
                return false;
        } else if (mainColumnIndex === 2) {
            if (subColumnIndex === 0 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostBudget || !this.controller.showBudgetPermission))
                return false;
            else if (subColumnIndex === 1 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostForecast || !this.controller.showForecastPermission))
                return false;
            else if (subColumnIndex === 2 && !this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostTemplateSchedule)
                return false;
            else if (subColumnIndex === 3 && !this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostSchedule)
                return false;
            else if (subColumnIndex === 4 && !this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostTime)
                return false;
            else if (subColumnIndex === 5 && !this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostScheduleAndTime)
                return false;
        } else if (mainColumnIndex === 3) {
            if (subColumnIndex === 0 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentBudget || !this.controller.showBudgetPermission))
                return false;
            else if (subColumnIndex === 1 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentForecast || !this.controller.showForecastPermission))
                return false;
            else if (subColumnIndex === 2 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentTemplateSchedule)
                return false;
            else if (subColumnIndex === 3 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentSchedule)
                return false;
            else if (subColumnIndex === 4 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentTime)
                return false;
        } else if (mainColumnIndex === 4) {
            if (subColumnIndex === 0 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATBudget || !this.controller.showBudgetPermission))
                return false;
            else if (subColumnIndex === 1 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATForecast || !this.controller.showForecastPermission))
                return false;
            else if (subColumnIndex === 2 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATTemplateSchedule)
                return false;
            else if (subColumnIndex === 3 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATSchedule)
                return false;
            else if (subColumnIndex === 4 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATTime)
                return false;
        } else if (mainColumnIndex === 5) {
            if (subColumnIndex === 0 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATBudget || !this.controller.showBudgetPermission))
                return false;
            else if (subColumnIndex === 1 && (!this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATForecast || !this.controller.showForecastPermission))
                return false;
            else if (subColumnIndex === 2 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATTemplateSchedule)
                return false;
            else if (subColumnIndex === 3 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATSchedule)
                return false;
            else if (subColumnIndex === 4 && !this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATTime)
                return false;
        }

        return true;
    }

    private getNbrOfSubColumns(mainColumnIndex: number): number {
        let nbrOfColumns: number = 0;

        if (mainColumnIndex === 0) {
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesBudget && this.controller.showBudgetPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesForecast && this.controller.showForecastPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeSalesTime)
                nbrOfColumns++;
        } else if (mainColumnIndex === 1) {
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursBudget && this.controller.showBudgetPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursForecast && this.controller.showForecastPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursTemplateSchedule)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursSchedule)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeHoursTime)
                nbrOfColumns++;
        } else if (mainColumnIndex === 2) {
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostBudget && this.controller.showBudgetPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostForecast && this.controller.showForecastPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostTemplateSchedule)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostSchedule)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostTime)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypePersonelCostScheduleAndTime)
                nbrOfColumns++;
        } else if (mainColumnIndex === 3) {
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentBudget && this.controller.showBudgetPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentForecast && this.controller.showForecastPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentTemplateSchedule)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentSchedule)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentTime)
                nbrOfColumns++;
        } else if (mainColumnIndex === 4) {
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATBudget && this.controller.showBudgetPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATForecast && this.controller.showForecastPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATTemplateSchedule)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATSchedule)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeLPATTime)
                nbrOfColumns++;
        } else if (mainColumnIndex === 5) {
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATBudget && this.controller.showBudgetPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATForecast && this.controller.showForecastPermission)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATTemplateSchedule)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATSchedule)
                nbrOfColumns++;
            if (this.controller.selectableInformationSettings.followUpShowCalculationTypeFPATTime)
                nbrOfColumns++;
        }

        return nbrOfColumns;
    }

    public renderEmptyScheduleSummary() {
        let attachmentPoint = document.getElementsByClassName('planning-summary-row')[0];
        if (attachmentPoint) {
            this.clearScheduleViewSummary().then(() => {
                // Total sum (top left corner)
                let totalSumTd = document.createElement('td');
                totalSumTd.classList.add('planning-day-summary');
                totalSumTd.classList.add('planning-day-summary-total');
                totalSumTd.colSpan = 2;
                attachmentPoint.appendChild(totalSumTd);

                // Sum for each hour (interval)/day
                this.controller.dates.forEach(dateDay => {
                    let daySumTd = document.createElement('td');
                    daySumTd.classList.add('planning-day-summary');
                    attachmentPoint.appendChild(daySumTd);
                });
            });
        }
    }

    public renderScheduleSummary() {
        const showGrossTime = this.controller.selectableInformationSettings.showGrossTime;
        const showTotalCost = this.controller.selectableInformationSettings.showTotalCost;
        const showTotalCostIncEmpTaxAndSuppCharge = this.controller.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge;
        const showStaffingNeed = this.controller.selectableInformationSettings.followUpOnNeed;

        const attachmentPoint = document.getElementsByClassName('planning-summary-row')[0];

        this.clearScheduleViewSummary().then(() => {
            let tooltip: string = '';

            // Total sum (top left corner)
            let totalStaffingNeed = '';
            if (showStaffingNeed) {
                let staffingNeedSum = _.sumBy(this.controller.staffingNeedSum, 'need');
                if (this.controller.isCommonDayView)
                    staffingNeedSum *= this.controller.dayViewMinorTickLength;
                totalStaffingNeed = CalendarUtility.minutesToTimeSpan(staffingNeedSum);
                tooltip += `${this.controller.terms["time.schedule.staffingneeds.planning.need"]}: ${totalStaffingNeed}\n`;
            }

            let netTimeMinutes = _.sumBy(this.controller.plannedMinutesSum, 'value');
            let totalNetTime = CalendarUtility.minutesToTimeSpan(netTimeMinutes);
            tooltip += `${this.controller.terms["time.schedule.planning.nettime"]}: ${CalendarUtility.minutesToTimeSpan(netTimeMinutes)}`;

            if (this.controller.selectableInformationSettings.showScheduleTypeFactorTime) {
                let factorMinutes = _.sumBy(this.controller.factorMinutesSum, 'value');
                totalNetTime += `/${CalendarUtility.minutesToTimeSpan(netTimeMinutes - factorMinutes)}`;
                tooltip += `\n${this.controller.terms["time.schedule.planning.scheduletypefactortime"]}: ${CalendarUtility.minutesToTimeSpan(factorMinutes)}`;
            }

            if (this.controller.isCommonScheduleView) {
                let workTimeMinutes = _.sum(this.controller.visibleEmployees.map(e => e.workTimeMinutes));
                totalNetTime += `/${CalendarUtility.minutesToTimeSpan(workTimeMinutes)}`;
                tooltip += `\n${this.controller.terms["time.schedule.planning.worktimeweek"]}: ${CalendarUtility.minutesToTimeSpan(workTimeMinutes)}`;
            }

            let totalGrossTime = '';
            let totalCostSum = '';
            if (!this.controller.loadingGrossNetAndCost && (showGrossTime || showTotalCost || showTotalCostIncEmpTaxAndSuppCharge)) {
                tooltip += "\n";
                if (showGrossTime) {
                    totalGrossTime = CalendarUtility.minutesToTimeSpan(_.sumBy(this.controller.grossMinutesSum, 'value'));
                    tooltip += `\n${this.controller.terms["time.schedule.planning.grosstime"]}: ${totalGrossTime}`;
                }
                if (showTotalCost || showTotalCostIncEmpTaxAndSuppCharge) {
                    totalCostSum = showTotalCostIncEmpTaxAndSuppCharge ? this.amountFilter(_.sumBy(this.controller.totalCostIncEmpTaxAndSupplementChargeSum, 'value'), 0) : this.amountFilter(_.sumBy(this.controller.totalCostSum, 'value'), 0);
                    tooltip += `\n${this.controller.terms["time.schedule.planning.cost"]}: ${totalCostSum}`;
                }
            }

            let totalSumTd = document.createElement('td');
            totalSumTd.classList.add('planning-day-summary');
            totalSumTd.classList.add('planning-day-summary-total');
            totalSumTd.colSpan = 2;
            totalSumTd.title = tooltip;

            if (showStaffingNeed) {
                let needSpan = document.createElement('span');
                needSpan.innerText = `${this.controller.terms["time.schedule.staffingneeds.planning.need"]}: ${totalStaffingNeed}`;
                totalSumTd.appendChild(needSpan);
                totalSumTd.appendChild(document.createElement('br'));
            }

            let netSpan = document.createElement('span');
            netSpan.innerText = `${this.controller.terms["time.schedule.planning.nettime"]}: ${totalNetTime}`;
            totalSumTd.appendChild(netSpan);

            if (this.controller.loadingGrossNetAndCost) {
                let spinDiv = document.createElement('div');
                spinDiv.classList.add('textColor');
                let spinSpan = document.createElement('span');
                spinSpan.classList.add('far');
                spinSpan.classList.add('fa-spinner');
                spinSpan.classList.add('fa-pulse');
                spinSpan.classList.add('fa-fw');
                spinDiv.appendChild(spinSpan);
                totalSumTd.appendChild(spinDiv);
            } else {
                if (showGrossTime) {
                    let spanTotalGrossTime = document.createElement('span');
                    spanTotalGrossTime.innerText = `${this.controller.terms["time.schedule.planning.grosstime"]}: ${totalGrossTime}`;
                    totalSumTd.appendChild(document.createElement('br'));
                    totalSumTd.appendChild(spanTotalGrossTime);
                }

                if (showTotalCost || showTotalCostIncEmpTaxAndSuppCharge) {
                    let spanTotalCost = document.createElement('span');
                    spanTotalCost.innerText = `${this.controller.terms["time.schedule.planning.cost"]}: ${totalCostSum}`;
                    totalSumTd.appendChild(document.createElement('br'));
                    totalSumTd.appendChild(spanTotalCost);
                }
            }

            attachmentPoint.appendChild(totalSumTd);

            // Sum for each hour (interval)/day
            let i = 0;
            this.controller.dates.forEach(dateDay => {
                let date: Date = dateDay.date;
                let daySumTd = document.createElement('td');
                daySumTd.classList.add('planning-day-summary');
                let daySumDiv = document.createElement('div');
                daySumDiv.style.overflowX = 'hidden';
                daySumDiv.style.overflowY = 'hidden';

                let daySumTooltip: string = '';

                let needSum: number;
                if (showStaffingNeed) {
                    let need = this.controller.staffingNeedSum.find(d => this.controller.isCommonDayView ? d.date.isSameMinuteAs(date) : d.date.isSameDayAs(date));
                    needSum = need?.need || 0;
                }

                if (this.controller.isCommonDayView) {
                    // Whole hour
                    if (this.controller.hourParts > 1 && i > 0 && date.minutes() === 0)
                        daySumTd.classList.add('whole-hour');

                    // calculateTimes in controller will store nbrOfShifts for each interval
                    let timeShifts = this.controller.timeShifts.filter(s => s.time === date.diffMinutes(date.beginningOfDay()));
                    let nbrOfShifts: number = timeShifts && timeShifts.length > 0 ? timeShifts[0].nbrOfShifts : 0;

                    if (showStaffingNeed) {
                        // In day view, need is specified by number of employees per interval
                        let needSpan = document.createElement('span');
                        needSpan.innerText = needSum.toString();
                        daySumDiv.appendChild(needSpan);
                        daySumDiv.appendChild(document.createElement('br'));
                        daySumTooltip += `${this.controller.terms["time.schedule.staffingneeds.planning.need"]}: ${needSum}\n`;
                    }

                    let nbrOfShiftsSpan = document.createElement('span');
                    nbrOfShiftsSpan.innerText = nbrOfShifts.toString();
                    daySumDiv.appendChild(nbrOfShiftsSpan);
                    daySumTooltip += nbrOfShifts.toString();
                } else if (this.controller.isCommonScheduleView) {
                    this.addDayTypeColor(daySumTd, dateDay, true, false);

                    if (date.getDay() === 0 && i < this.controller.dates.length - 1)
                        daySumTd.classList.add('planning-vertical-separator');

                    if (showStaffingNeed)
                        daySumTooltip += `${this.controller.terms["time.schedule.staffingneeds.planning.need"]}: ${CalendarUtility.minutesToTimeSpan(needSum)}\n`;

                    let planned = this.controller.plannedMinutesSum.find(d => d.date.isSameDayAs(date));
                    let plannedMinutes: number = planned ? planned.value : 0;
                    let netTime = CalendarUtility.minutesToTimeSpan(plannedMinutes);
                    daySumTooltip += `${this.controller.terms["time.schedule.planning.nettime"]}: ${netTime}`;

                    if (this.controller.selectableInformationSettings.showScheduleTypeFactorTime) {
                        let factor = this.controller.factorMinutesSum.find(d => d.date.isSameDayAs(date));
                        let factorMinutes: number = factor ? factor.value : 0;
                        if (factorMinutes !== 0) {
                            netTime += `/${CalendarUtility.minutesToTimeSpan(plannedMinutes - factorMinutes)}`;
                            daySumTooltip += `\n${this.controller.terms["time.schedule.planning.scheduletypefactortime"]}: ${CalendarUtility.minutesToTimeSpan(factorMinutes)}`;
                        }
                    }

                    if (showStaffingNeed) {
                        // In week view, need is specified in minutes per day
                        let needSumSpan = document.createElement('span');
                        needSumSpan.innerText = CalendarUtility.minutesToTimeSpan(needSum);
                        daySumDiv.appendChild(needSumSpan);
                        daySumDiv.appendChild(document.createElement('br'));
                    }

                    let netTimeSpan = document.createElement('span');
                    netTimeSpan.innerText = netTime;
                    daySumDiv.appendChild(netTimeSpan);

                    let grossTime = '';
                    let totalCost = '';
                    if (this.controller.loadingGrossNetAndCost) {
                        let spinDiv = document.createElement('div');
                        spinDiv.classList.add('textColor');
                        let spinSpan = document.createElement('span');
                        spinSpan.classList.add('far');
                        spinSpan.classList.add('fa-spinner');
                        spinSpan.classList.add('fa-pulse');
                        spinSpan.classList.add('fa-fw');
                        spinDiv.appendChild(spinSpan);
                        daySumDiv.appendChild(spinDiv);
                    } else {
                        if (showGrossTime) {
                            let gross = this.controller.grossMinutesSum.find(d => d.date.isSameDayAs(date));
                            grossTime = CalendarUtility.minutesToTimeSpan(gross ? gross.value : 0);
                            let spanDayGrossTime = document.createElement('span');
                            spanDayGrossTime.innerText = grossTime;
                            daySumDiv.appendChild(document.createElement('br'));
                            daySumDiv.appendChild(spanDayGrossTime);
                            daySumTooltip += `\n${this.controller.terms["time.schedule.planning.grosstime"]}: ${grossTime}`;
                        }
                        if (showTotalCost || showTotalCostIncEmpTaxAndSuppCharge) {
                            let cost = showTotalCostIncEmpTaxAndSuppCharge ? this.controller.totalCostIncEmpTaxAndSupplementChargeSum.find(d => d.date.isSameDayAs(date)) : this.controller.totalCostSum.find(d => d.date.isSameDayAs(date))
                            totalCost = this.amountFilter(cost ? cost.value : 0, 0);
                            let spanDayCost = document.createElement('span');
                            spanDayCost.innerText = totalCost;
                            daySumDiv.appendChild(document.createElement('br'));
                            daySumDiv.appendChild(spanDayCost);
                            daySumTooltip += `\n${this.controller.terms["time.schedule.planning.cost"]}: ${totalCost}`;
                        }
                    }
                }

                daySumTd.title = daySumTooltip;

                daySumTd.appendChild(daySumDiv);
                attachmentPoint.appendChild(daySumTd);
                i++;
            });
        });
    }

    private renderScheduleBody(): ng.IPromise<any> {
        if (this.controller.isCommonDayView) {
            if (this.controller.editMode === PlanningEditModes.Breaks) {
                $('.planning-scheduleview').addClass('shift-break-mode');
            } else {
                $('.planning-scheduleview').removeClass('shift-break-mode');
            }
        } else {
            $('.planning-scheduleview').removeClass('shift-break-mode');
        }

        return this.clearScheduleViewBody().then(() => {
            let attachmentPoint = $('.planning-scheduleview tbody');
            this.controller.renderedShifts = [];
            this.controller.renderedLeisureCodes = [];
            this.clearSelectedShifts();

            // Get employees
            let employees: EmployeeListDTO[] = [];
            let employeeIds = this.controller.getFilteredEmployeeIds();

            // Grouped on account
            if ((this.controller.isCommonDayView && this.controller.dayViewGroupBy > 10) ||
                (this.controller.isCommonScheduleView && this.controller.scheduleViewGroupBy > 10)) {
                // Get group headers
                let groupHeaders = this.controller.employedEmployees.filter(e => e.isGroupHeader);
                // Add employees under all groups where they have accounts
                groupHeaders.forEach(groupHeader => {
                    employees.push(groupHeader);

                    this.controller.employedEmployees.filter(e => (e['shiftAccountIds'] || []).includes(groupHeader['accountId'])).forEach(emp => {
                        let obj = new EmployeeListDTO();
                        angular.extend(obj, emp);
                        obj.fixDates();
                        obj.setTypes();
                        obj['accountId'] = groupHeader['accountId'];
                        employees.push(obj);
                    });
                });

                // Find all employees that exists more than once
                let empGroups = _.groupBy(employees, e => e.identifier);
                let multiEmpGroups = _.filter(empGroups, g => Object.keys(g).length > 1);
                let keys = Object.keys(multiEmpGroups);
                keys.forEach(key => {
                    multiEmpGroups[key].forEach(e => e['isDuplicate'] = true);
                });
            } else {
                if (this.controller.isEmployeePostView) {
                    this.controller.employedEmployees.forEach(employedEmployee => {
                        if (employedEmployee.isGroupHeader || (employedEmployee.employeePostId && employeeIds.includes(employedEmployee.employeePostId)))
                            employees.push(employedEmployee);
                    });

                } else {
                    this.controller.employedEmployees.forEach(employedEmployee => {
                        if (employedEmployee.isGroupHeader || (!employedEmployee.employeePostId && employeeIds.includes(employedEmployee.employeeId)))
                            employees.push(employedEmployee);
                    });
                }
            }

            // Hide all employees
            this.controller.allEmployees.forEach(e => e.isVisible = false);

            employees.forEach(employee => {
                let r = this.renderEmployeeRow(employee);
                if (r) {
                    employee.isVisible = true;
                    attachmentPoint.append(r);
                }
            });

            this.$compile(attachmentPoint)(this.$scope);
            this.controller.setDateColumnWidth(0).then(() => {
                this.updateWidthOnAllElements(0);
            });
        });
    }

    public setColgroupWidths() {
        const attachmentPoint = $('.planning-scheduleview tbody');
        const colgroups = $(attachmentPoint.parent()).find('colgroup');
        if (colgroups?.length) {
            const cols = colgroups[0].children;
            if (cols?.length) {
                if (this.controller.selectableInformationSettings.showPlanningPeriodSummary && (this.controller.isScheduleView || this.controller.isTemplateScheduleView)) {
                    if (this.controller.calculatePlanningPeriodScheduledTimeUseAveragingPeriod) {
                        cols[0]['width'] = 160;
                        cols[1]['width'] = 40;
                    } else {
                        cols[0]['width'] = 190;
                        cols[1]['width'] = 10;
                    }
                } else {
                    cols[0]['width'] = 125;
                    cols[1]['width'] = 75;
                }
            }
        }
    }

    // Tasks and deliveries
    public renderTasksAndDeliveries() {
        this.currentTime = new Date();

        this.stopRenderLoop();

        this.renderTasksAndDeliveriesBody().then(() => {
            this.renderTasksAndDeliveriesSummary();
            this.controller.renderingDone();
            this.restoreVerticalScroll();

            if (this.firstTimeScheduleSetup)
                this.performFirstTimeSetup();
        });
    }

    private renderTasksAndDeliveriesSummary() {
        // No summaries for now
    }

    private renderTasksAndDeliveriesBody(): ng.IPromise<any> {
        let deferral = this.$q.defer();

        return this.clearScheduleViewBody().then(() => {
            let attachmentPoint = $('.planning-scheduleview tbody');
            this.clearSelectedTasks();

            // Get tasks
            // One task can have several occurrences in current date range
            // Get distinct parent ids and just pick the first. All rows will be fetched below
            let tasks = [];
            let taskHeadIds = _.uniq(this.controller.tasksOfTypeTask.map(t => t.parentId));
            taskHeadIds.forEach(parentId => {
                tasks.push(this.controller.tasksOfTypeTask.find(t => t.parentId === parentId));
            });

            // Get incoming deliveries
            // One delivery head can have multiple rows
            // Get distinct head ids and just pick first head. All rows will be fetched below
            let deliveries = [];
            let deliveryHeadIds = _.uniq(this.controller.tasksOfTypeDelivery.map(t => t.parentId));
            deliveryHeadIds.forEach(parentId => {
                deliveries.push(this.controller.tasksOfTypeDelivery.find(t => t.parentId === parentId));
            });

            // TODO: Grouping on department (account dim) does not work if delivery rows have different accounting.
            // All rows will belong to the same head, and it is the first row that decides the grouping.

            let tasksAndDeliveries = _.compact(_.concat(tasks, deliveries));

            //This is to make IE not hang itself for half a year. Numbers can be tweaked
            let size = 10;
            let delay = 10;
            let maxLoops = Math.ceil(tasksAndDeliveries.length / size + 10);    // stopRenderLoop does not always work. This is just to prevent infinite loop
            this.columnWidth = 0;

            let hasCompensatedForScrollbar = false;
            this.renderInterval = this.$interval(() => {
                let tasksAndDeliveriesRows = tasksAndDeliveries.splice(0, size);
                if (tasksAndDeliveriesRows.length === 0) {
                    this.stopRenderLoop();
                    deferral.resolve();
                } else {
                    for (let tasksAndDeliveriesRow of tasksAndDeliveriesRows) {
                        let r = this.renderTaskRow(tasksAndDeliveriesRow);
                        attachmentPoint.append(r);

                        if (!this.columnWidth) {
                            this.setScheduleWidth();
                            this.updateWidth();
                        }

                        if (!hasCompensatedForScrollbar) {
                            if ($(document).height() > $(window).height()) {
                                this.columnWidth = 0;
                                hasCompensatedForScrollbar = true;
                            }
                        }

                        this.updateRowSizeAndPosition(r);//otherwise, just render it.
                    }
                }
            }, delay, maxLoops, false);

            return deferral.promise.finally(() => {
                this.$compile(attachmentPoint)(this.$scope);
                this.updateWidthOnAllElements(200);
            });
        });
    }

    private renderTaskRow(task: StaffingNeedsTaskDTO) {
        let dateTasks = [];
        let hasTasks: boolean = false;
        let dateDay: DateDay;

        if (this.controller.isTasksAndDeliveriesDayView) {
            dateDay = this.controller.dates[0];
            if (dateDay) {
                dateTasks = this.controller.dates.map(date => ({ date: date.date, tasks: [] }));
                let tasks = this.controller.getTasks(task.taskId, dateDay.date);
                dateTasks[0].tasks = tasks;
                hasTasks = (tasks && tasks.length > 0);
            }
        } else {
            dateTasks = this.controller.dates.map(date => ({ date: date.date, tasks: this.controller.getTasks(task.taskId, date.date) }));
            dateTasks.forEach(ds => {
                if (ds.tasks && ds.tasks.length > 0) {
                    hasTasks = true;
                }
            });
        }

        task.isVisible = (hasTasks || (this.isGroupedByDepartment && task['isDepartment']) || (this.isGroupedByShiftType && task['isShiftType']));

        if (!task.isVisible)
            return null;

        let row = document.createElement('tr');
        $(row).data('taskId', task.taskId);
        row.setAttribute('id', 'taskId' + task.taskId);

        if (task['isDepartment'])
            row.classList.add('planning-tr-department');
        else if (task['isShiftType'])
            row.classList.add('planning-tr-shifttype');

        if (this.controller.isCompressedStyle)
            row.classList.add('planning-tr-compressed');

        // Task
        let taskTd = document.createElement('td');
        taskTd.classList.add('staffing-needs-task');
        taskTd.setAttribute('colspan', '2');
        // Name
        let nameSpan = document.createElement('span');
        if (task['isDepartment'] || task['isShiftType']) {
            nameSpan.innerText = task.name;
            taskTd.append(nameSpan);
            if (task['isDepartment'])
                taskTd.classList.add('staffing-needs-task-department');
            else if (task['isShiftType'])
                taskTd.classList.add('staffing-needs-task-shifttype');
        } else {
            if (task.headDescription)
                taskTd.setAttribute('title', task.headDescription);
            nameSpan.innerText = task.headName;
            // Icon
            let icon = document.createElement('i');
            icon.classList.add('fal');
            if (task.isTask) {
                icon.classList.add('fa-check-square');
                icon.setAttribute('title', this.controller.terms["time.schedule.timescheduletask.task"]);
            } else if (task.isDelivery) {
                icon.classList.add('fa-truck');
                icon.setAttribute('title', this.controller.terms["time.schedule.incomingdelivery.incomingdelivery"]);
            }
            taskTd.append(icon);
            // Recurrence icon
            if (task.isReccurring) {
                let rIcon = document.createElement('i');
                rIcon.classList.add('fal');
                rIcon.classList.add('fa-repeat');
                rIcon.classList.add('recurring');
                rIcon.setAttribute('title', this.controller.terms["common.dailyrecurrencepattern.patterntitle"]);
                taskTd.append(rIcon);
            }
            taskTd.append(nameSpan);
        }

        row.append(taskTd);

        let maxRows = 0;
        let extendsOverDays = [];

        //precalc all indexes
        for (let i = 0, j = dateTasks.length; i < j; i++) {
            let ds = dateTasks[i];
            if (!ds.tasks)
                continue;

            ds.tasks = ds.tasks.map(t => {
                let obj = new StaffingNeedsTaskDTO(t.type);
                angular.extend(obj, t);
                return obj;
            });

            maxRows = Math.max(ds.tasks.length + extendsOverDays.length, maxRows);
            maxRows = Math.max(1, maxRows);

            let index = 0;
            for (let tsk of ds.tasks) {
                while (_.some(extendsOverDays, (t) => t.index === index)) //if there is an item that extends from a previous day on this index, increase until we find a free index.
                    index++;

                tsk.index = index;
                index++;
            }

            let endOfToday = (<Date>ds.date).endOfDay();
            extendsOverDays = extendsOverDays.filter(t => t.actualStopTime > endOfToday);//remove stuff that ended today.

            for (let l = 0, j = ds.tasks.length; l < j; l++) {
                if (ds.tasks[l].actualStopTime > endOfToday)
                    extendsOverDays.push(ds.tasks[l]);//push new stuff that extends from today.
            }
        }

        // Render using the precalced indexes from above
        for (let dti = 0, j = dateTasks.length; dti < j; dti++) {
            let ds = dateTasks[dti];

            let date: Date = ds.date;
            if (!this.controller.isTasksAndDeliveriesDayView)
                dateDay = this.controller.getDateDay(date);

            let td = document.createElement('td');
            td.classList.add('planning-day');
            td.classList.add('prevent-select');

            // Whole hour
            if (this.controller.isCommonDayView && this.controller.hourParts > 1 && dti > 0 && date.minutes() === 0)
                td.classList.add('whole-hour');

            // Today
            if (dateDay.isToday) {
                td.classList.add('planning-day-today');

                // Current time
                if (this.controller.isCommonDayView && dti === 0) {
                    let currentTimeDiv = document.createElement('div');
                    currentTimeDiv.classList.add('current-time');
                    currentTimeDiv.title = this.currentTime.toFormattedTime();
                    td.appendChild(currentTimeDiv);
                }
            }

            if (task['isDepartment'])
                td.classList.add('planning-day-department');
            else if (task['isShiftType'])
                td.classList.add('planning-day-shifttype');
            else {
                this.addDayTypeColor(td, dateDay, false, false);

                if (!_.last(dateTasks).date.isSameDayAs(date))
                    td.classList.add('planning-vertical-separator');
            }

            let divTaskDay = document.createElement('div');
            td.append(divTaskDay);
            divTaskDay.classList.add('shift-day');
            if (!task['isDepartment'] && !task['isShiftType']) {
                let divDropZone = document.createElement('div');
                divDropZone.classList.add('shift-drop-zone');
                if (this.controller.isCompressedStyle)
                    divDropZone.classList.add('shift-drop-zone-compressed');

                if (!ds.tasks || ds.tasks.length === 0)
                    divDropZone.classList.add('empty-slot');

                // Context menu
                divDropZone.setAttribute('context-menu', 'ctrl.getTaskSlotContextMenuOptions(' + task.type + ', ' + date.toPipedDateTime() + ')');
                divDropZone.setAttribute('context-menu-empty-text', "\' \'");
                divTaskDay.append(divDropZone);

                // TODO: Tasks not sorted by time in week view

                let tasks: StaffingNeedsTaskDTO[] = ds.tasks;
                if (tasks && tasks.length) {
                    let prevTask: StaffingNeedsTaskDTO;
                    let prevTop: number = 2;
                    if (this.controller.isCompressedStyle)
                        maxRows = 0;

                    _.orderBy(tasks, t => t.actualStartTime).forEach(t => {
                        let divPlanningShift = document.createElement('div');
                        divPlanningShift.classList.add('planning-shift');
                        if (this.controller.isCompressedStyle)
                            divPlanningShift.classList.add('planning-shift-compressed');
                        divPlanningShift.style.backgroundColor = t.color;
                        divPlanningShift.style.borderColor = '#CCCCCC';
                        divPlanningShift.setAttribute('title', t.toolTip);
                        divPlanningShift.setAttribute('id', t.taskId);

                        let textColor: string = GraphicsUtility.foregroundColorByBackgroundBrightness(t.color);

                        // Time
                        let spanTime = document.createElement('span');
                        spanTime.classList.add('shift-time');
                        spanTime.innerText = t.label1;
                        spanTime.style.color = textColor;

                        if (t.actualStopTime.diffMinutes(t.actualStartTime) > t.length) {
                            // Window icon
                            let wIcon = document.createElement('i');
                            wIcon.classList.add('fal');
                            wIcon.classList.add('fa-arrows-h');

                            let spanOuterTime = document.createElement('span');
                            spanOuterTime.append(wIcon);
                            spanTime.style.left = '18px';
                            spanOuterTime.append(spanTime);
                            divPlanningShift.append(spanOuterTime);
                        } else {
                            divPlanningShift.append(spanTime);
                        }

                        if (!this.controller.isCompressedStyle) {
                            // Title
                            let spanTitle = document.createElement('span');
                            spanTitle.classList.add('shift-title');
                            spanTitle.innerText = t.label2;
                            spanTitle.style.color = textColor;
                            divPlanningShift.append(spanTitle);
                        }

                        if (this.controller.isTasksAndDeliveriesDayView)
                            this.addActualTaskLength(t, divPlanningShift);

                        let top: number = 0;
                        if (this.controller.isCompressedStyle) {
                            // In compressed stype, place tasks on same row unless they overlap
                            if (prevTask?.actualStopTime.isSameOrBeforeOnMinute(t.actualStartTime)) {
                                top = prevTop;
                            } else {
                                top = this.controller.getShiftTopPosition(maxRows);
                                prevTask = t;
                                prevTop = top;
                                maxRows++;
                            }
                        } else {
                            // In uncompressed style, always put tasks on separate rows
                            top = this.controller.getShiftTopPosition(t.index);
                        }
                        $(divPlanningShift).css('top', top);

                        // Context menu
                        divPlanningShift.setAttribute('context-menu', 'ctrl.getTaskContextMenuOptions("' + t.taskId + '", ' + date.toPipedDateTime() + ')');
                        divPlanningShift.setAttribute('context-menu-empty-text', "\' \'");
                        divDropZone.append(divPlanningShift);

                        prevTask = t;
                    });
                }
            }

            // Set row height based on number of overlapping tasks
            if (!task['isDepartment'] && !task['isShiftType'])
                $(row).css('height', this.controller.getEmployeeRowHeight(maxRows));

            row.append(td);
        }

        return row;
    }

    private addActualTaskLength(task: StaffingNeedsTaskDTO, targetElem: HTMLDivElement) {
        // If actual task length is smaller than "task window", draw actual length in the middle of the window

        let visibleTaskLength = task.actualStopTime.diffMinutes(task.actualStartTime);
        if (visibleTaskLength === task.length)
            return;

        targetElem.classList.add('task-window-length');
        GraphicsUtility.fadeBackground(targetElem, 0.2);

        let span = document.createElement('span');
        let text = `${CalendarUtility.minutesToTimeSpan(task.length)} ${this.controller.terms["core.time.short.hour"].toLowerCase()}`;
        span.innerText = text;
        span.setAttribute('title', text);
        span.classList.add('task-actual-length');
        span.style.backgroundColor = task.color;
        span.style.color = GraphicsUtility.foregroundColorByBackgroundBrightness(task.color);

        targetElem.append(span);
    }

    // Staffing needs planning
    public renderStaffingNeeds() {
        this.currentTime = new Date();

        this.stopRenderLoop();

        this.renderStaffingNeedsBody().then(() => {
            this.renderPlanningSummary();
            this.renderPlanningSummaryEmployees();
            this.renderPlanningFilteredSummary();
            this.renderPlanningShiftTypeSummary();

            // Chart
            if (this.controller.isStaffingNeedsDayView && this.controller.staffingNeedsDayViewShowDiagram) {
                this.chartHandler.renderStaffingNeedsAgChartRow();

                if (this.controller.showStaffingNeedsAgChart)
                    this.controller.createStaffingNeedsAgChartData();

                if (this.controller.showStaffingNeedsAgChart)
                    this.controller.renderStaffingNeedsAgChart(false);
            }

            this.controller.renderingDone();
            this.restoreVerticalScroll();

            if (this.firstTimeScheduleSetup)
                this.performFirstTimeSetup();
        });
    }

    private renderStaffingNeedsBody(): ng.IPromise<any> {

        return this.clearScheduleViewBody().then(() => {
            let attachmentPoint = $('.planning-scheduleview tbody');

            if (this.controller.hasStaffingNeedsRows) {
                let staffingNeeds;
                if (this.controller.isStaffingNeedsScheduleView) {
                    staffingNeeds = this.renderStaffingNeedsRowsForHeads(_.sortBy(this.controller.heads, h => h.date));
                    this.renderStaffingNeedsDays(_.sortBy(this.controller.heads, h => h.date));
                } else {
                    staffingNeeds = _.compact(_.sortBy(this.controller.visibleStaffingNeedsRows, r => r.rowNr).map(this.renderStaffingNeedsRow));
                }

                staffingNeeds.forEach(r => {
                    attachmentPoint.append(r);
                });

                this.$compile(attachmentPoint)(this.$scope);
                this.updateWidthOnAllElements(500, true);
            }
        });
    }

    private renderStaffingNeedsRowsForHeads(heads: StaffingNeedsHeadDTO[]) {
        let attachmentPoint = $('.planning-scheduleview tbody');
        let trs = [];

        heads.forEach(head => {
            let rowIndex: number = 0;
            head.rows.forEach(row => {
                row.visiblePeriods.forEach(period => {
                    rowIndex++;
                    if ($('#rowId' + rowIndex).length === 0) {
                        let tr = document.createElement('tr');
                        tr.setAttribute('id', 'rowId' + rowIndex);
                        $(tr).css('height', this.controller.getStaffingNeedsRowHeight(1));

                        let nameTd = document.createElement('td');
                        nameTd.classList.add('staffing-needs-row-identifier');
                        tr.append(nameTd);
                        nameTd.setAttribute('colspan', '2');

                        for (let i = 1; i <= this.controller.nbrOfVisibleDays; i++) {
                            let td = document.createElement('td');
                            td.setAttribute('id', 'colId' + rowIndex + '_' + i);
                            td.classList.add('planning-day');
                            td.classList.add('prevent-select');
                            tr.append(td);
                        }

                        attachmentPoint.append(tr);
                        trs.push(tr);
                    }
                });
            });
        });

        return trs;
    }

    private renderStaffingNeedsDays(heads: StaffingNeedsHeadDTO[]) {
        heads.forEach(head => {
            let rowIndex: number = 0;
            let colIndex: number = head.date.diffDays(this.controller.dateFrom) + 1;

            head.rows.forEach(row => {
                row.visiblePeriods.forEach(period => {
                    rowIndex++;
                    let td = $('#colId' + rowIndex + '_' + colIndex);
                    if (td.length) {
                        let divPeriod = document.createElement('div');
                        divPeriod.setAttribute('id', 'periodId' + period.staffingNeedsRowPeriodId);
                        divPeriod.classList.add('staffing-needs-period');
                        divPeriod.style.backgroundColor = period.shiftTypeColor;
                        divPeriod.style.color = GraphicsUtility.foregroundColorByBackgroundBrightness(period.shiftTypeColor);
                        let tooltip: string = `${period.shiftTypeName || ''} ${CalendarUtility.toFormattedTime(period.actualStartTime)}-${CalendarUtility.toFormattedTime(period.actualStopTime)}`;
                        divPeriod.setAttribute('title', tooltip);

                        this.addDayTypeColor((<unknown>td) as HTMLTableCellElement, this.controller.getDateDay(period.actualStartTime), false, false);

                        if (period.isRemovedNeed) {
                            GraphicsUtility.fadeBackground(<any>divPeriod, 0.5);
                            let icon = document.createElement('i');
                            icon.classList.add('fal');
                            icon.classList.add('fa-calendar-times');
                            icon.classList.add('iconDelete');
                            icon.setAttribute('title', this.controller.terms["time.schedule.staffingneeds.planning.needremovedforspecificdate"]);
                            icon.style.marginTop = '1px';
                            icon.style.marginRight = '2px';
                            divPeriod.append(icon);
                        }

                        let spanText = document.createElement('span');
                        spanText.innerText = this.controller.isStaffingNeedsDayView && period.length <= 15 ? period.shiftTypeNeedsCode : period.shiftTypeName;
                        divPeriod.append(spanText);

                        td.append(divPeriod);
                    }
                });
            });
        });
    }

    private renderStaffingNeedsRow(row: StaffingNeedsRowDTO) {
        let datePeriods = [];
        let dateDay = this.controller.dates[0];
        if (dateDay) {
            datePeriods = this.controller.dates.map(date => ({ date: date.date, periods: [] }));
            let periods: StaffingNeedsRowPeriodDTO[] = _.sortBy(row.visiblePeriods.filter(p => dateDay.date.isSameDayAs(p.actualStartTime) || dateDay.date.isSameDayAs(p.actualStopTime)), p => p.actualStartTime);
            datePeriods[0].periods = periods;
        }

        let tr = document.createElement('tr');
        $(tr).data('rowId', row.staffingNeedsRowId);
        tr.setAttribute('id', 'rowId' + row.staffingNeedsRowId);
        tr.classList.add('staffing-needs-row');

        let nameTd = document.createElement('td');
        nameTd.classList.add('staffing-needs-row-identifier');
        tr.append(nameTd);
        if (this.controller.isStaffingNeedsScheduleView) {
            nameTd.setAttribute('colspan', '2');
        } else {
            let rowSum = document.createElement('td');
            rowSum.classList.add('staffing-needs-row-sum');
            rowSum.innerText = CalendarUtility.minutesToTimeSpan(row.totalLength);
            tr.append(rowSum);
        }

        let maxRows = 0;
        let extendsOverDays = [];

        //precalc all indexes
        for (let ds of datePeriods) {
            if (!ds.periods)
                continue;

            ds.periods = ds.periods.map(p => {
                let obj = new StaffingNeedsRowPeriodDTO();
                angular.extend(obj, p);
                return obj;
            });

            maxRows = Math.max(ds.periods.length + extendsOverDays.length, maxRows);
            maxRows = Math.max(1, maxRows);

            let index = 0;
            for (let period of ds.periods) {
                while (_.some(extendsOverDays, p => p.index === index))//if there is an item that extends from a previous day on this index, increase until we find a free index.
                    index++;

                period.index = index;
                index++;
            }

            let endOfToday = (<Date>ds.date).endOfDay();
            extendsOverDays = extendsOverDays.filter(s => s.actualStopTime > endOfToday);//remove stuff that ended today.

            ds.periods.forEach(p => {
                if (p.actualStopTime > endOfToday)
                    extendsOverDays.push(p);//push new stuff that extends from today.
            });
        }

        //render using the precalced indexes from above.
        let idx = 0;
        datePeriods.forEach(ds => {
            let td = document.createElement('td');
            td.classList.add('planning-day');
            td.classList.add('prevent-select');

            // Whole hour
            if (this.controller.isCommonDayView && this.controller.hourParts > 1 && idx > 0 && ds.date.minutes() === 0)
                td.classList.add('whole-hour');

            // Today
            if (ds.date.isToday()) {
                td.classList.add('planning-day-today');

                // Current time
                if (this.controller.isCommonDayView && idx === 0) {
                    let currentTimeDiv = document.createElement('div');
                    currentTimeDiv.classList.add('current-time');
                    currentTimeDiv.title = this.currentTime.toFormattedTime();
                    td.appendChild(currentTimeDiv);
                }
            }

            let divPeriodDay = document.createElement('div');
            td.append(divPeriodDay);
            divPeriodDay.classList.add('staffing-needs-day');
            let divDropZone = document.createElement('div');
            divDropZone.classList.add('staffing-needs-drop-zone');

            if (!ds.periods || ds.periods.length === 0)
                divDropZone.classList.add('empty-slot');

            divPeriodDay.append(divDropZone);

            let periods: StaffingNeedsRowPeriodDTO[] = ds.periods;
            if (periods && periods.length) {
                periods.forEach(period => {
                    let divPeriod = document.createElement('div');
                    divPeriod.classList.add('staffing-needs-period');
                    divPeriod.style.backgroundColor = period.shiftTypeColor;
                    divPeriod.style.color = GraphicsUtility.foregroundColorByBackgroundBrightness(period.shiftTypeColor);
                    let tooltip: string = `${period.shiftTypeName || ''} ${CalendarUtility.toFormattedTime(period.actualStartTime)}-${CalendarUtility.toFormattedTime(period.actualStopTime)}`;
                    divPeriod.setAttribute('title', tooltip);
                    divPeriod.setAttribute('id', 'periodId' + period.staffingNeedsRowPeriodId);
                    $(divPeriod).css('top', this.controller.getStaffingNeedsPeriodTopPosition(period.index));

                    if (period.isRemovedNeed) {
                        GraphicsUtility.fadeBackground(<any>divPeriod, 0.5);

                        let icon = document.createElement('i');
                        icon.classList.add('fal fa-calendar-times iconDelete');
                        icon.setAttribute('title', this.controller.terms["time.schedule.staffingneeds.planning.needremovedforspecificdate"]);
                        icon.style.marginTop = '1px';
                        icon.style.marginRight = '2px';
                        divPeriod.append(icon);
                    }

                    let spanText = document.createElement('span');
                    spanText.innerText = this.controller.isStaffingNeedsDayView && period.length <= 15 ? period.shiftTypeNeedsCode : period.shiftTypeName;
                    divPeriod.append(spanText);


                    divDropZone.append(divPeriod);
                });
            }

            tr.append(td);

            idx++;
        });

        return tr;
    }

    private renderPlanningSummary() {
        this.clearPlanningSummary().then(() => {
            if (!this.controller.hasStaffingNeedsRows)
                return;

            let attachmentPoint = $('.staffing-needs-summary-row');

            let name = document.createElement('td');
            name.classList.add('staffing-needs-row-identifier');
            name.innerText = this.controller.terms["common.total"];

            if ((this.controller.isStaffingNeedsDayView && this.controller.staffingNeedsDayViewShowDetailedSummary) || (this.controller.isStaffingNeedsScheduleView && this.controller.staffingNeedsScheduleViewShowDetailedSummary)) {
                let icon = document.createElement('i');
                icon.classList.add('far');
                icon.setAttribute('data-ng-class', "{\'fa-chevron-down\': ctrl.showShiftTypeSum, \'fa-chevron-right\': !ctrl.showShiftTypeSum}");
                icon.setAttribute('ng-attr-title', '{{ctrl.showShiftTypeSum ? ctrl.terms["time.schedule.staffingneeds.planning.hideshifttypesum"] : ctrl.terms["time.schedule.staffingneeds.planning.showshifttypesum"]}}');
                name.append(icon);
                name.classList.add('link');
                name.setAttribute('data-ng-click', 'ctrl.showShiftTypeSum = !ctrl.showShiftTypeSum');
                let content = this.$compile(name)(this.$scope);
                attachmentPoint.append(content);
            } else {
                attachmentPoint.append(name);
            }

            let rowSum = document.createElement('td');
            rowSum.classList.add('staffing-needs-row-sum');
            rowSum.innerText = CalendarUtility.minutesToTimeSpan(this.controller.staffingNeedsTotalSum);
            attachmentPoint.append(rowSum);

            // Sum for each hour/day
            this.controller.dates.forEach(dateDay => {
                let date = dateDay.date;
                let dateEnd = date.addMinutes(this.controller.dayViewMinorTickLength);
                let dateSum: number = 0;

                let td = document.createElement('td');
                this.addDayTypeColor(td, dateDay, true, true);

                this.controller.heads.forEach(head => {
                    head.rows.forEach(row => {
                        row.periods.forEach(p => {
                            if (this.controller.isStaffingNeedsScheduleView) {
                                // Add length of period
                                if (p.actualStartTime.isSameDayAs(date))
                                    dateSum += p.length;
                            } else {
                                // Add length of period that spans over this time period (eg: 15 min)
                                dateSum += CalendarUtility.getIntersectingDuration(date, dateEnd, p.actualStartTime, p.actualStopTime);
                            }
                        });
                    });
                });

                let span = document.createElement('span');
                let totalSumId: string = 'totalSumId';
                if (this.controller.isStaffingNeedsScheduleView)
                    totalSumId += date.diffDays(this.controller.dateFrom);
                else
                    totalSumId += date.diffMinutes(date.beginningOfDay());
                span.setAttribute('id', totalSumId);
                span.append(CalendarUtility.minutesToTimeSpan(dateSum));
                if (this.controller.isStaffingNeedsDayView && this.controller.dayViewMinorTickLength < 30)
                    span.classList.add('tinyFont');
                td.append(span);
                //if (this.controller.isShiftsView && this.controller.originalHead)
                //    this.setOverUnderstaffedClass(span, date);

                attachmentPoint.append(td);
            });
        });
    }

    private renderPlanningSummaryEmployees() {
        this.clearPlanningSummaryEmployees().then(() => {
            let attachmentPoint = $('.staffing-needs-summary-employees-row');

            if (!this.controller.hasStaffingNeedsRows)
                return;

            let name = document.createElement('td');
            name.classList.add('staffing-needs-row-identifier');
            name.innerText = this.controller.terms["common.quantity"];
            attachmentPoint.append(name);

            let rowSum = document.createElement('td');
            rowSum.classList.add('staffing-needs-row-sum');
            attachmentPoint.append(rowSum);

            // Number of employees for each hour/day
            this.controller.dates.forEach(dateDay => {
                let date = dateDay.date;
                let dateEnd = date.addMinutes(this.controller.dayViewMinorTickLength);
                let dateCount: number = 0;

                let td = document.createElement('td');
                this.addDayTypeColor(td, dateDay, true, true);

                this.controller.heads.forEach(head => {
                    head.rows.forEach(row => {
                        row.visiblePeriods.forEach(p => {
                            if (this.controller.isStaffingNeedsScheduleView) {
                                // Add length of period
                                if (p.actualStartTime.isSameDayAs(date))
                                    dateCount++;
                            } else {
                                if (CalendarUtility.getIntersectingDuration(date, dateEnd, p.actualStartTime, p.actualStopTime) > 0)
                                    dateCount++;
                            }
                        });
                    });
                });

                let span = document.createElement('span');
                let totalSumId: string = 'totalSumEmployeesId';
                if (this.controller.isStaffingNeedsScheduleView)
                    totalSumId += date.diffDays(this.controller.dateFrom);
                else
                    totalSumId += date.diffMinutes(date.beginningOfDay());
                span.setAttribute('id', totalSumId);
                span.append(dateCount.toString());
                td.append(span);

                attachmentPoint.append(td);
            });
        });
    }

    private renderPlanningFilteredSummary() {
        let attachmentPoint = $('.staffing-needs-filtered-summary-row');
        attachmentPoint.empty();

        if (!this.controller.isFiltered || !this.controller.hasStaffingNeedsRows)
            return;

        let name = document.createElement('td');
        name.classList.add('staffing-needs-row-identifier');
        name.innerText = this.controller.terms["common.filtered"];
        attachmentPoint.append(name);

        let rowSum = document.createElement('td');
        rowSum.classList.add('staffing-needs-row-sum');
        rowSum.innerText = CalendarUtility.minutesToTimeSpan(this.controller.staffingNeedsFilteredSum);
        attachmentPoint.append(rowSum);

        // Sum for each hour/day
        this.controller.dates.forEach(dateDay => {
            let date = dateDay.date;
            let dateEnd = date.addMinutes(this.controller.dayViewMinorTickLength);
            let dateSum: number = 0;

            let td = document.createElement('td');
            this.addDayTypeColor(td, dateDay, true, true);

            this.controller.heads.forEach(head => {
                head.rows.forEach(row => {
                    row.visiblePeriods.forEach(p => {
                        if (this.controller.isStaffingNeedsScheduleView) {
                            // Add length of period
                            if (p.actualStartTime.isSameDayAs(date))
                                dateSum += p.length;
                        } else {
                            // Add length of period that spans over this time period (eg: 15 min)
                            dateSum += CalendarUtility.getIntersectingDuration(date, dateEnd, p.actualStartTime, p.actualStopTime);
                        }
                    });
                });
            });

            let span = document.createElement('span');
            span.append(dateSum ? CalendarUtility.minutesToTimeSpan(dateSum) : '');
            if (this.controller.isStaffingNeedsDayView && this.controller.dayViewMinorTickLength < 30)
                span.classList.add('tinyFont');
            td.append(span);

            attachmentPoint.append(td);
        });
    }

    private renderPlanningShiftTypeSummary() {
        if (!this.controller.staffingNeedsShiftTypeSum || this.controller.staffingNeedsShiftTypeSum.length === 0)
            return;

        let attachmentPoint = $('.planning-scheduleview thead');

        // Remove existing summary rows
        $('.staffing-needs-shifttype-summary-row').remove();

        _.orderBy(this.controller.staffingNeedsShiftTypeSum, ['unspecified', 'shiftTypeName'], ['desc', 'asc']).forEach(shiftType => {
            let row = document.createElement('tr');
            row.classList.add('staffing-needs-shifttype-summary-row');
            row.setAttribute('data-ng-show', 'ctrl.showShiftTypeSum');
            let content = this.$compile(row)(this.$scope);
            attachmentPoint.append(content);

            let name = document.createElement('td');
            name.classList.add('staffing-needs-row-identifier');
            name.innerText = shiftType.shiftTypeName;
            name.style.backgroundColor = shiftType.shiftTypeColor;
            name.style.color = GraphicsUtility.foregroundColorByBackgroundBrightness(shiftType.shiftTypeColor);
            row.append(name);

            let rowSum = document.createElement('td');
            rowSum.classList.add('staffing-needs-row-sum');
            rowSum.innerText = CalendarUtility.minutesToTimeSpan(shiftType.sum);
            row.append(rowSum);

            // Calculate number of rows for current shift type
            // Used for setting background opacity below
            shiftType['rowCount'] = 0;
            shiftType['rowSum'] = 0;
            this.controller.visibleStaffingNeedsRows.forEach(needRow => {
                if (_.some(needRow.periods, p => (shiftType.shiftTypeId ? p.shiftTypeId === shiftType.shiftTypeId : !p.shiftTypeId))) {
                    // In day view we just count the number of rows
                    shiftType['rowCount']++;
                    // Is schedule view, we need to sum the length of the periods
                    if (this.controller.isStaffingNeedsScheduleView) {
                        needRow.periods.filter(p => (shiftType.shiftTypeId ? p.shiftTypeId === shiftType.shiftTypeId : !p.shiftTypeId)).forEach(p => {
                            shiftType['rowSum'] += p.length;
                        });
                    }
                }
            });

            // Sum for each hour/day
            this.controller.dates.forEach(dateDay => {
                let date = dateDay.date;
                let dateEnd = date.addMinutes(this.controller.dayViewMinorTickLength);
                let dateSum: number = 0;

                let td = document.createElement('td');
                this.addDayTypeColor(td, dateDay, true, true);

                this.controller.heads.forEach(head => {
                    head.rows.forEach(headRow => {
                        headRow.visiblePeriods.filter(p => (shiftType.shiftTypeId ? p.shiftTypeId === shiftType.shiftTypeId : !p.shiftTypeId)).forEach(p => {
                            if (this.controller.isStaffingNeedsScheduleView) {
                                // Add length of period
                                if (p.actualStartTime.isSameDayAs(date))
                                    dateSum += p.length;
                            } else {
                                // Add length of period that spans over this time period (eg: 15 min)
                                dateSum += CalendarUtility.getIntersectingDuration(date, dateEnd, p.actualStartTime, p.actualStopTime);
                            }
                        });
                    });
                });

                let span = document.createElement('span');
                span.append(dateSum ? CalendarUtility.minutesToTimeSpan(dateSum) : '');
                if (this.controller.isStaffingNeedsDayView && this.controller.dayViewMinorTickLength < 30)
                    span.classList.add('tinyFont');
                td.append(span);

                // Set background opacity based on number of occurrances
                if (dateSum) {
                    let opacity: number = 1;
                    if (this.controller.isStaffingNeedsDayView)
                        opacity = dateSum / (shiftType['rowCount'] * this.controller.dayViewMinorTickLength);
                    else
                        opacity = (dateSum * (this.controller.nbrOfVisibleDays / 2)) / shiftType['rowSum'];
                    td.style.background = GraphicsUtility.addAlphaValue(shiftType.shiftTypeColor, opacity);
                }

                row.append(td);
            });
        });

        // Separator
        let sepRow = document.createElement('tr');
        sepRow.classList.add('staffing-needs-shifttype-summary-row');
        let separator = document.createElement('td');
        separator.classList.add('planning-horizontal-separator');
        sepRow.append(separator);
        sepRow.setAttribute('data-ng-show', 'ctrl.showShiftTypeSum');
        let sepContent = this.$compile(sepRow)(this.$scope);
        attachmentPoint.append(sepContent);
    }

    public renderStaffingNeedsAgChart() {
        this.chartHandler.renderStaffingNeedsAgChart();
    }

    public setStaffingNeedsAgChartData(data: any) {
        this.chartHandler.setStaffingNeedsChartData(data);
    }

    // Common
    private stopRenderLoop() {
        this.$interval.cancel(this.renderInterval);
    }

    public renderEmployeeRow(emp: EmployeeListDTO) {
        // If grouping on account, emp is a clone of the "real" employee to be able to add multiple instances of the employee.
        // But the real employee must be used in here, so we need to fetch it again.
        let e: EmployeeListDTO;
        if (emp['accountId']) {
            e = this.controller.getEmployeeById(emp.employeeId) || emp;
            e['accountId'] = emp['accountId'];
            e['isDuplicate'] = emp['isDuplicate'];
        } else {
            e = emp;
        }

        const isHiddenEmployee = (e.employeeId === this.controller.hiddenEmployeeId);
        let dateShifts: { date: Date, shifts: ShiftDTO[] }[] = [];
        let hasShifts: boolean = false;
        let dateDay: DateDay;

        if (this.controller.isCommonDayView) {
            dateDay = this.controller.dates[0];
            if (dateDay) {
                dateShifts = this.controller.dates.map(d => ({ date: d.date, shifts: [] }));
                let shifts: ShiftDTO[] = [];
                if (!e.isGroupHeader)
                    shifts = this.controller.getShifts(e.identifier, dateDay.date, e['accountId']);
                dateShifts[0].shifts = shifts;
                hasShifts = (shifts && shifts.length > 0);
            }
        } else {
            dateShifts = this.controller.dates.map(d => ({ date: d.date, shifts: !e.isGroupHeader ? this.controller.getShifts(e.identifier, d.date, e['accountId']) : [] }));
            for (let ds of dateShifts) {
                if (ds.shifts && ds.shifts.length > 0) {
                    hasShifts = true;
                    break;
                }
            }
        }

        // Grouping
        if (hasShifts || (this.controller.isGrouped && e.isGroupHeader)) {
            // Employee is always visible if it has shifts
            e.isVisible = true;
        } else if (this.controller.showAllEmployees) {
            // If show all employees is selected, employee is visible if no filter on employee at all,
            // or employee is included in filter, regardless of shifts
            if (!this.controller.isFilteredOnEmployee) {
                e.isVisible = true;
            } else {
                e.isVisible = (this.controller.isEmployeePostView ? this.controller.getFilteredEmployeePostIds().includes(e.employeePostId) : this.controller.getFilteredEmployeeIds().includes(e.employeeId));
            }
        } else {
            e.isVisible = false;
        }

        if (!e.isVisible)
            return null;

        let row = document.createElement('tr');
        let idStr: string;
        if (e['isAccount'])
            idStr = 'accountId';
        else if (e['isCategory'])
            idStr = 'categoryId';
        else if (e['isShiftType'])
            idStr = 'shiftTypeId';
        else
            idStr = this.controller.isEmployeePostView ? 'empPostId' : 'empId';
        // TODO: Get rid of JQuery
        $(row).data(idStr, e.identifier);
        row.id = idStr + e.identifier;
        row.setAttribute('identifier', row.id);

        if (e['accountId'])
            row.setAttribute('account-id', e['accountId']);

        if (e['isAccount'])
            row.classList.add('planning-tr-account');
        else if (e['isCategory'])
            row.classList.add('planning-tr-category');
        else if (e['isShiftType'])
            row.classList.add('planning-tr-shifttype');

        if (this.controller.isCompressedStyle)
            row.classList.add('planning-tr-compressed');

        let employeeTd: HTMLTableCellElement = document.createElement('td');
        let annualTd: HTMLTableCellElement;
        employeeTd.classList.add('planning-employee');
        employeeTd.colSpan = (this.controller.selectableInformationSettings.showPlanningPeriodSummary && (this.controller.isScheduleView || this.controller.isTemplateScheduleView) ? 1 : 2);
        if (this.controller.isEmployeePostView)
            employeeTd.classList.add('planning-employee-post');

        if (e.isModified) {
            let icon = document.createElement('i');
            icon.classList.add('fal');
            icon.classList.add('fa-asterisk');
            icon.classList.add('errorColor');
            icon.style.fontSize = "10px";
            icon.style.marginRight = "3px";
            employeeTd.appendChild(icon);
        }

        if (e['isDuplicate']) {
            let icon = document.createElement('i');
            icon.classList.add('fal');
            icon.classList.add('fa-user-friends');
            icon.classList.add('warningColor');
            icon.style.fontSize = "10px";
            icon.style.marginRight = "3px";
            employeeTd.appendChild(icon);
        }

        if (!e.active && !e.isGroupHeader) {
            let icon = document.createElement('i');
            icon.classList.add('fal');
            icon.classList.add('fa-user-times');
            icon.classList.add('errorColor');
            icon.style.marginRight = "3px";
            employeeTd.appendChild(icon);
            employeeTd.classList.add('inactive');
        } else if (e.vacant) {
            let icon = document.createElement('i');
            icon.classList.add('fal');
            icon.classList.add('fa-newspaper');
            icon.style.marginRight = "3px";
            employeeTd.appendChild(icon);
        } else if (this.controller.isCurrentEmploymentTemporaryPrimary(e)) {
            let icon = document.createElement('i');
            icon.classList.add('fas');
            icon.classList.add('fa-clone');
            icon.style.marginRight = "3px";
            icon.setAttribute('title', e.hibernatingText);
            employeeTd.appendChild(icon);
        }

        let nameSpan = document.createElement('span');
        nameSpan.classList.add('name');
        if (e.isGroupHeader) {
            nameSpan.innerText = e.name;
            employeeTd.appendChild(nameSpan);
            if (e['isAccount'])
                employeeTd.classList.add('planning-employee-account');
            else if (e['isCategory'])
                employeeTd.classList.add('planning-employee-category');
            else if (e['isShiftType'])
                employeeTd.classList.add('planning-employee-shifttype');
        } else {
            if (e.hidden || e.vacant) {
                nameSpan.classList.add('italic');
                if (e.hidden)
                    nameSpan.classList.add('bold');
            }
            nameSpan.innerText = (e.hidden || e.employeePostId ? e.name : e.numberAndName);
            if (this.controller.isCompressedStyle && !this.controller.isEmployeePostView)
                this.setOverUndertimeClass(nameSpan, e);
            employeeTd.appendChild(nameSpan);

            if (this.controller.isEmployeePostView) {
                let outerNameDiv = document.createElement('div');
                let outerNameSpan = document.createElement('span');
                outerNameDiv.appendChild(outerNameSpan);

                // Locked icon
                if (e.employeePostStatus === SoeEmployeePostStatus.Locked) {
                    let icon = document.createElement('i');
                    icon.classList.add('fal');
                    icon.classList.add('fa-lock-alt');
                    icon.style.marginRight = "3px";
                    outerNameSpan.appendChild(icon);

                    employeeTd.classList.add('locked');
                }

                // If employee post has been assigned, set real employee name
                let employeeNameSpan = document.createElement('span');
                employeeNameSpan.classList.add('employee-name');
                if (e.employeeId) {
                    let employee = this.controller.getEmployeeById(e.employeeId);
                    employeeNameSpan.innerText = (employee ? employee.numberAndName : '');
                }
                outerNameSpan.appendChild(employeeNameSpan);
                employeeTd.appendChild(outerNameDiv);
            }

            if (this.controller.selectableInformationSettings.showPlanningPeriodSummary && (this.controller.isScheduleView || this.controller.isTemplateScheduleView)) {
                annualTd = document.createElement('td');
                annualTd.classList.add('planning-employee');
                annualTd.classList.add('planning-employee-annual-time');
                if (e.hidden) {
                    employeeTd.style.borderRightWidth = "0px";
                    annualTd.style.borderLeftWidth = "0px";
                    annualTd.style.cursor = "default";
                } else {
                    annualTd.setAttribute('data-ng-click', '$event.stopPropagation();ctrl.openAnnualSummary(' + e.employeeId + ');');
                    this.setAnnualScheduleTimeClass(annualTd, e);

                    if (this.controller.calculatePlanningPeriodScheduledTimeUseAveragingPeriod &&
                        (this.controller.currentPlanningPeriodChildInRangeExact || this.controller.hasPlanningPeriodHeadButNoChild)) {
                        if (this.controller.planningPeriodHead) {
                            let parentBalanceSpan = document.createElement('span');
                            parentBalanceSpan.classList.add('planning-employee-balance-parent');
                            annualTd.appendChild(parentBalanceSpan);
                        }
                        if (this.controller.planningPeriodChild && this.controller.currentPlanningPeriodChildInRangeExact) {
                            let childBalanceSpan = document.createElement('span');
                            childBalanceSpan.classList.add('planning-employee-balance-child');
                            annualTd.appendChild(childBalanceSpan);
                        }
                    }
                }
            }

            if ((this.controller.isCommonScheduleView) && !this.controller.isCompressedStyle) {
                // Employee group
                let groupSpan;
                if (this.controller.selectableInformationSettings.showEmployeeGroup && !e.hidden) {
                    let groupName = this.controller.getCurrentEmployeeGroupName(e);
                    if (groupName) {
                        groupSpan = document.createElement('span');
                        groupSpan.classList.add('employee-group');
                        groupSpan.style.width = this.controller.selectableInformationSettings.showScheduleTypeFactorTime ? "90px" : "120px";
                        groupSpan.innerText = groupName;
                        groupSpan.title = groupName;
                        employeeTd.appendChild(groupSpan);
                    }
                }

                // Planned time
                let timeSpan = document.createElement('span');
                timeSpan.classList.add('time');
                if (groupSpan)
                    timeSpan.classList.add('use-top-margin');
                employeeTd.appendChild(timeSpan);

                // Cycle time
                let cycleTimeSpan;
                if (this.controller.isScheduleView && this.controller.selectableInformationSettings.showCyclePlannedTime) {
                    let outerCycleTimeSpan = document.createElement('span');
                    outerCycleTimeSpan.classList.add('outer-cycle-time');

                    // Cycle icon
                    let icon = document.createElement('i');
                    icon.classList.add('fal');
                    icon.classList.add('fa-recycle');
                    outerCycleTimeSpan.appendChild(icon);

                    // Cycle planned time
                    cycleTimeSpan = document.createElement('span');
                    cycleTimeSpan.classList.add('cycle-time');
                    outerCycleTimeSpan.appendChild(cycleTimeSpan);

                    employeeTd.appendChild(outerCycleTimeSpan);
                }

                // Annual leave balance
                let annualLeaveBalanceIcon;
                let annualLeaveBalanceSpan;
                if ((this.controller.isScheduleView || this.controller.isDayView) && this.controller.selectableInformationSettings.showAnnualLeaveBalance && this.controller.hasAnnualLeaveGroup(e)) {
                    let outerAnnualLeaveBalanceSpan = document.createElement('span');
                    outerAnnualLeaveBalanceSpan.classList.add('outer-annual-leave-balance');

                    // Balance icon
                    annualLeaveBalanceIcon = document.createElement('i');
                    annualLeaveBalanceIcon.classList.add('fal');
                    outerAnnualLeaveBalanceSpan.appendChild(annualLeaveBalanceIcon);

                    // Balance time
                    annualLeaveBalanceSpan = document.createElement('span');
                    annualLeaveBalanceSpan.classList.add('annual-leave-balance');
                    outerAnnualLeaveBalanceSpan.appendChild(annualLeaveBalanceSpan);

                    employeeTd.appendChild(outerAnnualLeaveBalanceSpan);
                }

                this.setEmployeeTimes(employeeTd, timeSpan, cycleTimeSpan, annualLeaveBalanceSpan, annualLeaveBalanceIcon, annualTd, e);
            }
            employeeTd.title = e.toolTip;
        }

        // Context menu (employee)
        if (!e.isGroupHeader && this.controller.editMode !== PlanningEditModes.Breaks && this.controller.editMode !== PlanningEditModes.TemplateBreaks) {
            employeeTd.setAttribute('context-menu', "ctrl.getEmployeeContextMenuOptions(" + e.identifier + ")");
            employeeTd.setAttribute('context-menu-empty-text', "\' \'");
            employeeTd.setAttribute('model', e.identifier.toString());
        }

        row.appendChild(employeeTd);

        if (annualTd) {
            row.appendChild(annualTd);
        }

        let nbrOfRows = 1;
        let extendsOverDays = [];

        // Calculate indexes for shift position
        for (let i = 0, j = dateShifts.length; i < j; i++) {
            let ds = dateShifts[i];
            if (!ds.shifts)
                continue;

            let index = 0;
            let prevLink: string;
            let prevShift: ShiftDTO;
            for (let shift of ds.shifts) {
                if (index > 0 && !shift.isAbsenceRequest && !shift.isOnDuty) {
                    if (this.controller.isCommonDayView) {
                        // Shifts in day view for hidden employee must be separated on its own rows.
                        // Linked shifts should be on the same row.
                        if (!isHiddenEmployee || (isHiddenEmployee && shift.link && prevLink && shift.link === prevLink))
                            index--;
                    } else if (this.controller.isCommonScheduleView && (this.controller.shiftStyle === TermGroup_TimeSchedulePlanningShiftStyle.ActualTimeCompressed || this.controller.shiftStyle === TermGroup_TimeSchedulePlanningShiftStyle.ActualTime)) {
                        // When actual time is used, shifts should be on the same row, unless they are on duty shifts
                        index--;
                    }
                }

                // If there is an item that extends from a previous day on this index, increase until we find a free index
                if (this.controller.isCommonScheduleView && (this.controller.shiftStyle === TermGroup_TimeSchedulePlanningShiftStyle.ActualTimeCompressed || this.controller.shiftStyle === TermGroup_TimeSchedulePlanningShiftStyle.ActualTime)) {
                    while (_.some(extendsOverDays.filter(ext => ext.actualStopTime > shift.actualStartTime), (s) => s.index === index)) {
                        index++;
                    }
                } else {
                    while (_.some(extendsOverDays, (s) => s.index === index)) {
                        index++;
                    }
                }

                // Place on duty shifts on same row if they don't overlap
                if (this.controller.isCommonDayView &&
                    prevShift?.isOnDuty &&
                    shift.isOnDuty &&
                    shift.actualStartTime.isSameOrAfterOnMinute(prevShift.actualStopTime)) {
                    index--;
                }

                shift.index = index;
                prevLink = shift.link;
                index++;

                if (nbrOfRows < index)
                    nbrOfRows = index;

                prevShift = shift;
            }

            // Remove shifts that ends today
            let endOfToday = ds.date.endOfDay();
            extendsOverDays = extendsOverDays.filter(s => s.actualStopTime > endOfToday);

            for (let l = 0, j = ds.shifts.length; l < j; l++) {
                // Add shifts that extends from today
                if (ds.shifts[l].actualStopTime > endOfToday)
                    extendsOverDays.push(ds.shifts[l]);
            }
        }

        let rowHeight = this.controller.getEmployeeRowHeight(nbrOfRows);
        if (this.controller.isCompressedStyle)
            rowHeight += nbrOfRows;
        $(row).css('height', rowHeight);

        let templateVisibleRanges: { id: number, range: DateRangeDTO }[] = [];

        // Render using the precalced indexes from above
        for (let dsi = 0, j = dateShifts.length; dsi < j; dsi++) {
            let ds = dateShifts[dsi];

            // Get current start date and time
            let date: Date = ds.date;
            if (!this.controller.isCommonDayView)
                dateDay = this.controller.getDateDay(date);

            let noEmployment: boolean = !e.hasEmployment(date, date);
            let noTemplate: boolean = ((this.controller.isTemplateView || this.controller.isEmployeePostView) && !e.hasTemplateSchedule(date));
            let repeatingTemplate: boolean = false;
            let isFirstDayOfTemplate: boolean = ((this.controller.isTemplateView || this.controller.isEmployeePostView) && e.isFirstDayOfTemplate(date));
            let isLastDayOfTemplate: boolean = ((this.controller.isTemplateView || this.controller.isEmployeePostView) && e.isLastDayOfTemplate(date));
            let isTemplateGroup: boolean = false;
            let templateName: string;
            let templateGroupName: string;
            if (this.controller.isTemplateView) {
                let template = e.getTemplateSchedule(date);
                if (template) {
                    let templateVisibleRange: DateRangeDTO;
                    let range = templateVisibleRanges.find(t => t.id === template.timeScheduleTemplateHeadId);
                    if (range) {
                        templateVisibleRange = range.range;
                    } else {
                        templateVisibleRange = this.controller.getTemplateVisibleRange(template);
                        templateVisibleRanges.push({ id: template.timeScheduleTemplateHeadId, range: templateVisibleRange });
                    }

                    if (date.isBeforeOnDay(templateVisibleRange.start) || date.isAfterOnDay(templateVisibleRange.stop))
                        repeatingTemplate = true;

                    if (isFirstDayOfTemplate || isLastDayOfTemplate) {
                        templateName = template.name;
                        if (template.timeScheduleTemplateGroupId) {
                            isTemplateGroup = true;
                            templateGroupName = template.timeScheduleTemplateGroupName;
                        }
                    }
                }
            }

            let hasEmployeeSchedule: boolean = this.controller.isTemplateView && e.hasEmployeeSchedule(date);
            let outsideScenario: boolean = this.controller.isScenarioView && !this.controller.isInsideScenario(date);

            let td = document.createElement('td');
            td.classList.add('planning-day');
            td.classList.add('prevent-select');

            // Whole hour
            if (this.controller.isCommonDayView && this.controller.hourParts > 1 && dsi > 0 && date.minutes() === 0)
                td.classList.add('whole-hour');

            // Today
            if (dateDay.isToday) {
                td.classList.add('planning-day-today');

                // Current time
                if (this.controller.isCommonDayView && dsi === 0) {
                    let currentTimeDiv = document.createElement('div');
                    currentTimeDiv.classList.add('current-time');
                    currentTimeDiv.title = this.currentTime.toFormattedTime();
                    $(currentTimeDiv).css('height', rowHeight);
                    td.appendChild(currentTimeDiv);
                }
            }

            let availabilityToolTip: string = '';
            let slotHasAvailabilityComment: boolean = false;

            if (!e.isGroupHeader) {
                // No employment
                if (noEmployment) {
                    td.classList.add('planning-day-no-employment');
                }

                // No template schedule
                if (noTemplate) {
                    td.classList.add('planning-day-no-template');
                    td.title = this.controller.terms["time.schedule.planning.notemplateschedule"];
                }

                // Repeating template schedule
                if (repeatingTemplate) {
                    td.classList.add('planning-day-repeating-template');
                }

                // First day of template
                if (isFirstDayOfTemplate) {
                    td.classList.add(isTemplateGroup ? 'planning-day-first-day-of-template-group' : 'planning-day-first-day-of-template');
                }

                // Last day of template
                if (isLastDayOfTemplate) {
                    td.classList.add(isTemplateGroup ? 'planning-day-last-day-of-template-group' : 'planning-day-last-day-of-template');
                }

                // Employee schedule
                if (hasEmployeeSchedule) {
                    td.classList.add('planning-day-has-employee-schedule');
                    td.title = this.controller.terms["time.schedule.planning.hasemployeeschedule"];
                }

                // Outside scenario
                if (outsideScenario) {
                    td.classList.add('planning-day-outside-scenario');
                }

                // Availability
                if ((this.controller.isDayView || this.controller.isScheduleView) && !noEmployment && this.controller.selectableInformationSettings.showAvailability && (e.hasAvailability || e.hasUnavailability)) {
                    // Get date and time range for current cell
                    let dateFrom: Date;
                    let dateTo: Date;
                    if (this.controller.isCommonScheduleView) {
                        dateFrom = date.beginningOfDay();
                        dateTo = date.endOfDay();
                    } else if (this.controller.isCommonDayView) {
                        dateFrom = date;
                        dateTo = date.addMinutes(this.controller.dayViewMinorTickLength).addSeconds(-1);
                    }

                    if (e.hasAvailabilityCommentInRange(dateFrom, dateTo) && (!this.controller.isCommonDayView || e.isLastSlotOnAvailability(dateTo))) {
                        slotHasAvailabilityComment = true;
                        let iconComment = this.createShiftRightIcon("comment-dots", this.controller.terms["common.dashboard.myschedule.availability.hascomment"], 1, '#000000');
                        td.appendChild(iconComment);
                    }

                    if (e.isFullyAvailableInRange(dateFrom, dateTo)) {
                        td.classList.add('planning-day-available');
                        availabilityToolTip = this.controller.terms["time.schedule.planning.available"];
                    } else if (e.isFullyUnavailableInRange(dateFrom, dateTo)) {
                        td.classList.add('planning-day-unavailable');
                        availabilityToolTip = this.controller.terms["time.schedule.planning.unavailable"];
                    } else {
                        let partlyAvailable = e.isAvailableInRange(dateFrom, dateTo);
                        let partlyUnavailable = e.isUnavailableInRange(dateFrom, dateTo);
                        if (partlyAvailable && !partlyUnavailable) {
                            td.classList.add('planning-day-partly-available');
                        } else if (partlyUnavailable && !partlyAvailable) {
                            td.classList.add('planning-day-partly-unavailable');
                        } else if (partlyAvailable && partlyUnavailable) {
                            td.classList.add('planning-day-mixed-available');
                        }
                        if (partlyAvailable) {
                            let availableDates = e.getAvailableInRange(dateFrom, dateTo);
                            if (availableDates.length > 0) {
                                availableDates.forEach(availableDate => {
                                    availabilityToolTip += `${this.controller.terms["time.schedule.planning.available"]} ${availableDate.start.toFormattedTime()}-${availableDate.stop.toFormattedTime()}`;
                                    if (availableDate.comment)
                                        availabilityToolTip += `, ${availableDate.comment}`;
                                    availabilityToolTip += "\n";
                                });
                            }
                        }
                        if (partlyUnavailable) {
                            let unavailableDates = e.getUnavailableInRange(dateFrom, dateTo);
                            if (unavailableDates.length > 0) {
                                unavailableDates.forEach(unavailableDate => {
                                    availabilityToolTip += `${this.controller.terms["time.schedule.planning.unavailable"]} ${unavailableDate.start.toFormattedTime()}-${unavailableDate.stop.toFormattedTime()}`;
                                    if (unavailableDate.comment)
                                        availabilityToolTip += `, ${unavailableDate.comment}`;
                                    availabilityToolTip += "\n";
                                });
                            }
                        }
                    }
                    if (availabilityToolTip.length > 0)
                        availabilityToolTip = `${this.controller.terms["time.schedule.planning.availability"]}:\n${availabilityToolTip}`;
                }
            }

            if (e['isAccount'])
                td.classList.add('planning-day-account');
            else if (e['isCategory'])
                td.classList.add('planning-day-category');
            else if (e['isShiftType'])
                td.classList.add('planning-day-shifttype');
            else {
                // Saturday/Sunday, also checks holidays
                if (dateDay.isSunday)
                    td.classList.add(noTemplate || repeatingTemplate || noEmployment || hasEmployeeSchedule ? 'planning-day-sunday-passed' : 'planning-day-sunday');
                else if (dateDay.isSaturday)
                    td.classList.add(noTemplate || repeatingTemplate || noEmployment || hasEmployeeSchedule ? 'planning-day-saturday-passed' : 'planning-day-saturday');

                // Sunday
                if (date.getDay() === 0 && !_.last(dateShifts).date.isSameDayAs(date))
                    td.classList.add('planning-vertical-separator');
            }

            let employeePostLocked = (this.controller.isEmployeePostView && e.employeePostStatus === SoeEmployeePostStatus.Locked);

            let divShiftDay = document.createElement('div');
            divShiftDay.classList.add('shift-day');
            if (this.controller.isGrouped && e.isGroupHeader) {
                // calculateTimes in controller will store nbrOfShifts for each interval
                let timeShifts = this.controller.timeShifts.filter(s => s.time === date.diffMinutes(date.beginningOfDay()));
                let groupedShifts: { groupName: string, nbrOfShifts: number }[] = timeShifts && timeShifts.length > 0 ? timeShifts[0].groupedShifts : [];
                let nbrOfShifts: number = groupedShifts.find(g => g.groupName === e.name.trim())?.nbrOfShifts || 0;
                if (nbrOfShifts > 0)
                    divShiftDay.innerHTML = divShiftDay.title = nbrOfShifts.toString();
            } else {
                if (availabilityToolTip.length > 0)
                    divShiftDay.title = availabilityToolTip;
            }

            // Context menu (slot)
            if (this.controller.editMode !== PlanningEditModes.Breaks && this.controller.editMode !== PlanningEditModes.TemplateBreaks && this.controller.hasCurrentViewModifyPermission) {
                divShiftDay.setAttribute('context-menu', "ctrl.getSlotContextMenuOptions(" + e.identifier + ", " + date.toPipedDateTime() + ")");
                divShiftDay.setAttribute('context-menu-empty-text', "\' \'");
                divShiftDay.setAttribute('model', e.identifier.toString());
            }

            td.appendChild(divShiftDay);

            if (!e.isGroupHeader) {
                let divDropZone = document.createElement('div');
                divDropZone.setAttribute('rowId', e.identifier.toString());
                divDropZone.setAttribute('date', date.toPipedDateTime());
                $(divDropZone).css('height', rowHeight);

                if (!employeePostLocked && !outsideScenario && (!noEmployment || this.controller.isTemplateView)) {
                    divDropZone.classList.add('shift-drop-zone');

                    if (this.controller.isCompressedStyle)
                        divDropZone.classList.add('shift-drop-zone-compressed');
                }

                if (!ds.shifts || ds.shifts.length === 0)
                    divDropZone.classList.add('empty-slot');

                if (((isFirstDayOfTemplate || noEmployment || repeatingTemplate || outsideScenario) && (!this.controller.isCommonDayView || dsi === 0)) ||
                    (isLastDayOfTemplate && (!this.controller.isCommonDayView || dsi === j - 1))) {
                    let icon = document.createElement('i');
                    icon.classList.add('fal');
                    if (isFirstDayOfTemplate) {
                        icon.classList.add('fa-arrow-from-left');
                        icon.title = `${this.controller.terms["time.schedule.planning.firstdayoftemplate"]} '${templateName}'`;
                    } else if (isLastDayOfTemplate) {
                        icon.classList.add('fa-arrow-from-right');
                        icon.title = `${this.controller.terms["time.schedule.planning.lastdayoftemplate"]} '${templateName}'`;
                    } else if (noEmployment) {
                        icon.classList.add('fa-sign-out');
                        icon.title = this.controller.terms["time.schedule.planning.noemployment"];
                    } else if (outsideScenario) {
                        icon.classList.add('fa-calendar-times');
                        icon.title = this.controller.terms["time.schedule.planning.scenario.outside"];
                    } else if (repeatingTemplate) {
                        icon.classList.add('fa-repeat');
                        icon.title = this.controller.terms["time.schedule.planning.repeatingday"];
                    }

                    icon.classList.add('textColor');
                    divDropZone.appendChild(icon);

                    if (isTemplateGroup) {
                        let icon2 = document.createElement('i');
                        icon2.classList.add('fal');
                        icon2.classList.add('fa-layer-group');
                        icon2.title = `${this.controller.terms["time.schedule.templategroup.templategroup"]}: ${templateGroupName}`;
                        divDropZone.appendChild(icon2);
                    }
                }

                divShiftDay.appendChild(divDropZone);

                let shifts: ShiftDTO[] = ds.shifts;
                if (shifts?.length && (!noEmployment || this.controller.isTemplateView)) {
                    let divPlanningShift: HTMLDivElement;
                    let shiftCounter: number = 0;

                    for (let i = 0, j = shifts.length; i < j; i++) {
                        let shft = shifts[i];
                        if (!shft.isAbsenceRequest && !shft.isOnDuty)
                            shiftCounter++;
                        // Get number of shifts (per linked groupfor hidden employee)
                        let nbrOfShifts: number = isHiddenEmployee ? shifts.filter(s => s.link === shft.link).length : shifts.filter(s => !s.isAbsenceRequest && !s.isOnDuty).length;

                        let isUnwanted: boolean = (shft.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted);
                        let hasWanted: boolean = shft.nbrOfWantedInQueue > 0;
                        let hasAbsenceRequest: boolean = shft.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested && !shft.isAbsenceRequest;
                        let isAbsence: boolean = !!shft.timeDeviationCauseId && !shft.isAbsenceRequest;
                        let hasSwapRequest: boolean = shft.hasSwapRequest;
                        let hasAttestState: boolean = !!(this.controller.isOrderPlanningMode && shft.order && shft.order.attestStateColor);

                        divPlanningShift = document.createElement('div');
                        divPlanningShift.classList.add('planning-shift');
                        if (this.controller.isCompressedStyle)
                            divPlanningShift.classList.add('planning-shift-compressed');

                        if (!this.controller.isCommonDayView ||
                            shft.isAbsenceRequest ||
                            shft.isPreliminary ||
                            shft.isStandby ||
                            shft.isOnDuty ||
                            shft.isLeisureCode) {
                            divPlanningShift.style.backgroundColor = shft.shiftTypeColor;
                        }

                        if (shft.isReadOnly ||
                            (shft.isAbsence && !shft.isStandby) ||
                            shft.isAbsenceRequest ||
                            (this.controller.isOrderPlanningMode && shft.isSchedule) ||
                            (this.controller.isStandbyView && !shft.isStandby) ||
                            shft.isLeisureCode) {
                            divPlanningShift.classList.add('no-dragdrop');
                        }

                        if (shft.isReadOnly && !shft.isLended && this.controller.hasCurrentViewModifyPermission) {
                            //divPlanningShift.classList.add('ui-state-disabled');
                            //divPlanningShift.classList.add('blurred');
                            GraphicsUtility.fadeBackground(divPlanningShift, 0.2);
                        }

                        if (isUnwanted || hasWanted || hasAbsenceRequest || isAbsence || hasSwapRequest || hasAttestState) {
                            let divStatus = document.createElement('div');
                            divStatus.style.position = "absolute";
                            divStatus.style.overflow = "visible";
                            divStatus.style.zIndex = "100";

                            if (isUnwanted || hasWanted) {
                                if (hasWanted) {
                                    divStatus.classList.add('shift-wanted');
                                    if (this.controller.showQueuePermission) {
                                        let spanQueue = document.createElement('span');
                                        spanQueue.classList.add('shift-queue');
                                        spanQueue.style.position = "relative";
                                        spanQueue.style.textAlign = "center";
                                        spanQueue.style.overflow = "visible";
                                        spanQueue.style.backgroundColor = "inherit";
                                        spanQueue.style.padding = "0px 0px 0px 0px";
                                        spanQueue.style.display = "initial";
                                        spanQueue.innerText = shft.nbrOfWantedInQueue.toString();
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
                                } else if (hasSwapRequest) {
                                    divStatus.classList.add('shift-swap-request');
                                    divStatus.title = this.controller.terms["common.swapshift.approveinmobileinfo"];
                                } else if (hasAttestState) {
                                    if (shft.order.attestStateColor === "GreenToRedGradientBrush") {
                                        divStatus.classList.add('shift-planning-gradient');
                                    } else {
                                        divStatus.classList.add('shift-planning-empty');
                                        divStatus.style.backgroundColor = shft.order.attestStateColor;
                                    }
                                }
                            }

                            if (this.controller.isCommonDayView) {
                                // Absence requests are drawn on a second row
                                // Therefore we need to position the status div on first row
                                let shiftTop: number = this.controller.shiftMarginCompressed;
                                if ($(divPlanningShift).css('top'))
                                    shiftTop = parseInt($(divPlanningShift).css('top'), 10);
                                $(divStatus).css('top', shiftTop - this.controller.shiftMarginCompressed);
                            }

                            divPlanningShift.appendChild(divStatus);
                        }

                        if (!this.controller.isCommonDayView && (shft.isAbsenceRequest || shft.isPreliminary || shft.isStandby || shft.isOnDuty)) {
                            let divStriped = document.createElement('div');
                            if (shft.isStandby)
                                divStriped.classList.add('shift-standby-background');
                            else if (shft.isOnDuty)
                                divStriped.classList.add('shift-onduty-background');
                            else
                                divStriped.classList.add('shift-striped-background');
                            divPlanningShift.appendChild(divStriped);
                        }

                        if (shft.isLended)
                            divPlanningShift.classList.add('is-lended');

                        if (shft.isLeisureCode)
                            divPlanningShift.classList.add('is-leisurecode');

                        if (!shft.isVisible && !shft.isLended)
                            GraphicsUtility.fadeBackground(divPlanningShift, 0.2);

                        if (this.controller.isCommonDayView && this.controller.editMode === PlanningEditModes.Breaks)
                            GraphicsUtility.fadeBackground(divPlanningShift, 0.3);

                        if (outsideScenario || noEmployment)
                            GraphicsUtility.fadeBackground(divPlanningShift, 0.5);

                        if (availabilityToolTip.length > 0)
                            this.controller.setShiftAvailabilityToolTip(shft.timeScheduleTemplateBlockId, availabilityToolTip);

                        if (shft.isLeisureCode) {
                            divPlanningShift.id = `lc_${shft.timeScheduleEmployeePeriodDetailId}`;
                            divPlanningShift.setAttribute('data-ng-mouseenter', 'ctrl.leisureCodeMouseEnter(' + shft.timeScheduleEmployeePeriodDetailId + ');');
                            divPlanningShift.setAttribute('data-ng-mouseleave', 'ctrl.leisureCodeMouseLeave(' + shft.timeScheduleEmployeePeriodDetailId + ');');
                        } else {
                            divPlanningShift.id = (shft.isAbsenceRequest ? 'ar_' : '') + shft.timeScheduleTemplateBlockId;
                            divPlanningShift.setAttribute('data-ng-mouseenter', 'ctrl.shiftMouseEnter(' + shft.timeScheduleTemplateBlockId + ');');
                            divPlanningShift.setAttribute('data-ng-mouseleave', 'ctrl.shiftMouseLeave(' + shft.timeScheduleTemplateBlockId + ');');
                        }

                        let textColor: string = GraphicsUtility.foregroundColorByBackgroundBrightness(!shft.isOnDuty && !shft.isStandby ? shft.shiftTypeColor : '', shft.isAbsence && !shft.isOnDuty && !shft.isStandby);
                        let weekNumberAdded = false;
                        let rightTimeAdded = false;

                        let prevDayIcon;
                        if (!shft.isLeisureCode) {
                            let startsPreviousDay: boolean = shft.actualStartDate.isBeforeOnDay(date);
                            let startsNextDay: boolean = shft.actualStartDate.isAfterOnDay(date);
                            if (startsPreviousDay || startsNextDay) {
                                prevDayIcon = document.createElement('span');
                                prevDayIcon.classList.add('shift-prev-day-icon');
                                prevDayIcon.classList.add('fal');
                                prevDayIcon.classList.add(startsPreviousDay ? 'fa-arrow-alt-to-right' : 'fa-arrow-alt-from-left');
                                prevDayIcon.style.color = textColor;
                            }
                        }

                        if (this.controller.isCommonDayView) {
                            if (shft.isLeisureCode) {
                                let spanTitle = document.createElement('span');
                                spanTitle.classList.add('shift-title');
                                spanTitle.classList.add('dayview');
                                spanTitle.innerText = shft.label1;
                                divPlanningShift.appendChild(spanTitle);
                            } else {
                                let showTime = (this.controller.selectableInformationSettings.timePosition !== TermGroup_TimeSchedulePlanningTimePosition.Hidden && this.controller.selectableInformationSettings.hideTimeOnShiftShorterThanMinutes <= shft.getShiftLength());

                                let rangeCounter = 0;
                                let splittedShifts: DateRangeDTO[] = this.splitShiftOnBreaks(shft);
                                splittedShifts.forEach(range => {
                                    rangeCounter++;
                                    let firstShift = (shiftCounter === 1 && rangeCounter === 1);
                                    let lastShift = (shiftCounter === nbrOfShifts && rangeCounter === splittedShifts.length);

                                    const divSplittedShiftsContainer = document.createElement('div');
                                    divSplittedShiftsContainer.classList.add('shift-split-container');
                                    divSplittedShiftsContainer.style.backgroundColor = shft.shiftTypeColor;

                                    if (shft.isStandby)
                                        divSplittedShiftsContainer.classList.add('shift-standby-background');
                                    else if (shft.isOnDuty)
                                        divSplittedShiftsContainer.classList.add('shift-onduty-background');
                                    else if (shft.isAbsenceRequest || shft.isPreliminary)
                                        divSplittedShiftsContainer.classList.add('shift-striped-background');

                                    let padding = 0;
                                    let leftText = '';
                                    let centerText = '';
                                    let rightText = '';

                                    if (prevDayIcon) {
                                        padding = 14;
                                        divSplittedShiftsContainer.appendChild(prevDayIcon);
                                    }

                                    // Absence
                                    if (shft.timeDeviationCauseName)
                                        leftText += shft.timeDeviationCauseName + " ";

                                    // Full time text (only used when dragging)
                                    let spanDragTooltip = document.createElement('span');
                                    spanDragTooltip.classList.add('shift-time-for-tooltip');
                                    spanDragTooltip.innerText = `${range.start.toFormattedTime()}-${range.stop.toFormattedTime()}`;
                                    divSplittedShiftsContainer.appendChild(spanDragTooltip);


                                    // Time (start or full)
                                    if (showTime &&
                                        (this.controller.selectableInformationSettings.timePosition === TermGroup_TimeSchedulePlanningTimePosition.Left ||
                                            this.controller.selectableInformationSettings.timePosition === TermGroup_TimeSchedulePlanningTimePosition.ShiftEdges ||
                                            (this.controller.selectableInformationSettings.timePosition === TermGroup_TimeSchedulePlanningTimePosition.DayEdges && firstShift))) {
                                        if (shft.isWholeDay) {
                                            leftText += this.controller.terms["time.schedule.planning.wholedaylabel"]
                                        } else if (this.controller.selectableInformationSettings.timePosition === TermGroup_TimeSchedulePlanningTimePosition.Left) {
                                            leftText += `${range.start.toFormattedTime()}-${range.stop.toFormattedTime()}`;
                                        } else {
                                            if (range.duration > 60 || range.start.minutes() !== 0)
                                                leftText += range.start.toFormattedTime();
                                            else
                                                leftText += range.start.hour().toString();
                                        }
                                    }

                                    // ShiftType
                                    let shiftTypeText = '';
                                    if (this.controller.selectableInformationSettings.useShiftTypeCode && shft.shiftTypeCode)
                                        shiftTypeText = shft.shiftTypeCode;
                                    else if (shft.shiftTypeName)
                                        shiftTypeText = shft.shiftTypeName;

                                    // ScheduleTypes
                                    let scheduleTypeCodes = shft.getTimeScheduleTypeCodes(this.controller.useMultipleScheduleTypes);
                                    if (scheduleTypeCodes)
                                        shiftTypeText += ` - ${scheduleTypeCodes}`;

                                    // Description
                                    if (shft.description)
                                        shiftTypeText += `, ${shft.description.replace(/[\n\r]/g, " ")}`;

                                    if (this.controller.selectableInformationSettings.shiftTypePosition === TermGroup_TimeSchedulePlanningShiftTypePosition.Left) {
                                        leftText += " " + shiftTypeText;
                                    } else {
                                        centerText = shiftTypeText;
                                    }

                                    if (this.controller.selectableInformationSettings.breakVisibility === TermGroup_TimeSchedulePlanningBreakVisibility.TotalMinutes) {
                                        leftText += shft.getTotalBreakText(this.controller.terms["time.schedule.planning.breaklabel"]);
                                    }

                                    if (showTime)
                                        leftText = this.addBelongsToAnotherDayArrow(shft, date, leftText);

                                    if (leftText) {
                                        let spanLeftText = document.createElement('span');
                                        spanLeftText.classList.add('shift-left-text');
                                        spanLeftText.innerText = leftText;
                                        spanLeftText.style.color = textColor;

                                        // Make room for left colored margin
                                        if (isUnwanted || hasWanted || hasAbsenceRequest || isAbsence || hasSwapRequest || hasAttestState) {
                                            padding += 8;
                                        }

                                        if (padding > 0) {
                                            spanLeftText.style.left = `${padding}px`;
                                        }

                                        divSplittedShiftsContainer.appendChild(spanLeftText);
                                    }

                                    if (centerText) {
                                        let spanCenterText = document.createElement('span');
                                        spanCenterText.classList.add('shift-center-text');
                                        spanCenterText.innerText = centerText;
                                        spanCenterText.style.color = textColor;
                                        divSplittedShiftsContainer.appendChild(spanCenterText);
                                    }

                                    // Time (stop)
                                    if (!shft.isWholeDay &&
                                        (this.controller.selectableInformationSettings.timePosition === TermGroup_TimeSchedulePlanningTimePosition.ShiftEdges ||
                                            (this.controller.selectableInformationSettings.timePosition === TermGroup_TimeSchedulePlanningTimePosition.DayEdges && lastShift))) {
                                        if (showTime) {
                                            if (range.duration > 60 || range.stop.minutes() !== 0)
                                                rightText = range.stop.toFormattedTime();
                                            else
                                                rightText = range.stop.hour().toString();
                                            this.addBelongsToAnotherDayArrow(shft, date, rightText);
                                            rightTimeAdded = true;
                                        }

                                        if (this.controller.selectableInformationSettings.showWeekNumber && shft.nbrOfWeeks && lastShift) {
                                            // Add week number on last shift
                                            rightText += " " + this.getWeekNumberText(shft);
                                            weekNumberAdded = true;
                                        }
                                    }

                                    if (rightText) {
                                        let spanRightText = document.createElement('span');
                                        spanRightText.classList.add('shift-right-text');
                                        spanRightText.innerText = rightText;
                                        spanRightText.style.color = textColor;
                                        divSplittedShiftsContainer.appendChild(spanRightText);
                                    }

                                    if (firstShift)
                                        divSplittedShiftsContainer.classList.add('planning-shift-resize-start');

                                    if (lastShift) {
                                        divSplittedShiftsContainer.classList.add('planning-shift-resize-stop');
                                        shiftCounter = 0;
                                    }

                                    divPlanningShift.appendChild(divSplittedShiftsContainer);
                                });
                            }
                        } else {
                            if (shft.isLeisureCode) {
                                let spanTitle = document.createElement('span');
                                spanTitle.classList.add('shift-title');
                                spanTitle.innerText = shft.label1;
                                divPlanningShift.appendChild(spanTitle);
                            } else {
                                let padding = 0;
                                if (prevDayIcon) {
                                    padding = 14;
                                    divPlanningShift.appendChild(prevDayIcon);
                                }

                                let timeText: string = shft.label1;
                                this.addBelongsToAnotherDayArrow(shft, date, timeText);
                                let spanTime = document.createElement('span');
                                spanTime.classList.add('shift-time');
                                spanTime.innerText = timeText;
                                spanTime.style.color = textColor;
                                if (isUnwanted || hasWanted || hasAbsenceRequest || isAbsence || hasSwapRequest || hasAttestState)
                                    padding += 8;
                                if (padding > 0) {
                                    spanTime.style.left = `${padding}px`;
                                    spanTime.style.paddingRight = `${padding}px`;
                                }
                                divPlanningShift.appendChild(spanTime);

                                if (!this.controller.isCompressedStyle) {
                                    let spanTitle = document.createElement('span');
                                    spanTitle.classList.add('shift-title');
                                    spanTitle.innerText = shft.label2;
                                    spanTitle.style.color = textColor;
                                    if (padding > 0) {
                                        spanTitle.style.left = `${padding}px`;
                                        spanTitle.style.paddingRight = `${padding}px`;
                                    }
                                    divPlanningShift.appendChild(spanTitle);
                                }
                            }
                        }

                        // Only show week numbers on last shift in day view (if multiple shifts in same day)
                        if (!weekNumberAdded && shft.nbrOfWeeks && this.controller.selectableInformationSettings.showWeekNumber && (shiftCounter === nbrOfShifts || !this.controller.isCommonDayView) && !shft.isAnnualLeave) {
                            divPlanningShift.appendChild(this.createWeekNumberSpan(shft, textColor));
                            weekNumberAdded = true;
                        }

                        let additionalRightMargin = this.controller.isCompressedStyle && weekNumberAdded ? 20 : 0;
                        if (rightTimeAdded)
                            additionalRightMargin += 25;

                        // Right icons
                        let iconNumber = slotHasAvailabilityComment ? 1 : 0;
                        if (shft.isLended || shft.isOtherAccount) {
                            iconNumber++;
                            // TODO: Add tooltip with information about where it's lended
                            let icon = shft.isLended ? 'map-marker-exclamation' : 'map-marker-question';
                            let tooltip = shft.isLended ? this.controller.terms["time.schedule.planning.islended"] : this.controller.terms["time.schedule.planning.isotheraccount"];
                            let iconLended = this.createShiftRightIcon(icon, tooltip, iconNumber, textColor, additionalRightMargin, true);
                            iconLended.classList.add('lended');
                            divPlanningShift.appendChild(iconLended);
                        }

                        if (shft.isStandby || shft.isOnDuty) {
                            iconNumber++;
                            let icon = this.createShiftRightIcon(shft.isStandby ? 'alarm-snooze' : 'alarm-exclamation', shft.isStandby ? this.controller.terms["time.schedule.planning.blocktype.standby"] : this.controller.terms["time.schedule.planning.blocktype.onduty"], iconNumber, textColor, additionalRightMargin);
                            divPlanningShift.appendChild(icon);
                        }

                        if (shft.hasShiftRequest) {
                            iconNumber++;
                            let icon = this.createShiftRightIcon(shft.shiftRequestAnswerType === XEMailAnswerType.None ? 'envelope' : 'envelope-open', this.controller.terms["time.schedule.planning.hasshiftrequest"], iconNumber, textColor, additionalRightMargin);
                            icon.setAttribute('data-ng-click', "$event.stopPropagation();ctrl.showShiftRequestStatus(" + shft.timeScheduleTemplateBlockId.toString() + ");");
                            divPlanningShift.appendChild(icon);
                        }

                        if (this.controller.showSubstitute && shft.substituteShift) {
                            iconNumber++;
                            let icon = this.createShiftRightIcon('user-plus', this.controller.terms["time.schedule.planning.editshift.substitute"], iconNumber, textColor, additionalRightMargin);
                            divPlanningShift.appendChild(icon);
                        }

                        if (this.controller.showExtraShift && shft.extraShift) {
                            iconNumber++;
                            let icon = this.createShiftRightIcon('calendar-plus', this.controller.terms["time.schedule.planning.editshift.extrashift"], iconNumber, textColor, additionalRightMargin);
                            divPlanningShift.appendChild(icon);
                        }

                        if (this.controller.isCommonDayView && this.controller.selectableInformationSettings.breakVisibility === TermGroup_TimeSchedulePlanningBreakVisibility.Details && !shft.isOnDuty) {
                            this.addBreaks(shft, divPlanningShift);
                        }

                        $(divPlanningShift).css('top', this.controller.getShiftTopPosition(shft.index));

                        // Context menu (shift)
                        if (this.controller.editMode !== PlanningEditModes.Breaks && this.controller.editMode !== PlanningEditModes.TemplateBreaks && this.controller.hasCurrentViewModifyPermission) {
                            divPlanningShift.setAttribute('context-menu', "ctrl.getShiftContextMenuOptions('" + divPlanningShift.id + "')");
                            divPlanningShift.setAttribute('context-menu-empty-text', "\' \'");
                            divPlanningShift.setAttribute('model', "'" + divPlanningShift.id + "'");
                        }

                        divDropZone.appendChild(divPlanningShift);
                    }
                }
            }

            if (this.controller.hasCurrentViewModifyPermission)
                this.dragDropHelper.enableInteractability(td);

            row.appendChild(td);
        }

        return row;
    }

    private splitShiftOnBreaks(shift: ShiftDTO): DateRangeDTO[] {
        let splittedShifts: DateRangeDTO[] = [];
        let shiftStart = shift.actualStartTime;
        let shiftStop = shift.actualStopTime;
        if (this.controller.selectableInformationSettings.breakVisibility === TermGroup_TimeSchedulePlanningBreakVisibility.Holes) {
            for (let breakNbr = 1; breakNbr <= 4; breakNbr++) {
                if (shift[`break${breakNbr}Id`]) {
                    const breakStart = this.getBreakStart(shift, breakNbr);
                    const breakStop = breakStart.addMinutes(this.getBreakLength(shift, breakNbr));

                    // Break starts same time as shift
                    if (breakStart.isSameMinuteAs(shiftStart)) {
                        if (breakStop.isBeforeOnMinute(shiftStop)) {
                            // Break ends inside shift
                            shiftStart = breakStop;
                        } else if (breakStop.isSameOrAfterOnMinute(shiftStop)) {
                            // Break ends same time as shift or in next shift
                            shiftStop = breakStart;
                        }
                    }

                    // Break starts inside shift
                    if (breakStart.isAfterOnMinute(shiftStart) && breakStart.isBeforeOnMinute(shiftStop)) {
                        if (breakStop.isSameOrBeforeOnMinute(shiftStop)) {
                            // Break ends inside shift
                            splittedShifts.push(new DateRangeDTO(shiftStart, breakStart));
                            shiftStart = breakStop;
                        } else if (breakStop.isSameOrAfterOnMinute(shiftStop)) {
                            // Break ends same time as shift or in next shift
                            shiftStop = breakStart;
                        }
                    }

                    // Break starts in previous shift
                    if (breakStart.isBeforeOnMinute(shiftStart)) {
                        if (breakStop.isAfterOnMinute(shiftStart) && breakStop.isSameOrBeforeOnMinute(shiftStop)) {
                            // Break ends inside shift
                            shiftStart = breakStop;
                        } else if (breakStop.isAfterOnMinute(shiftStop)) {
                            // Break ends in next shift, skip shift compleately
                            shiftStart = shiftStop;
                        }
                    }
                }
            }
        }
        if (shiftStart.isBeforeOnMinute(shiftStop))
            splittedShifts.push(new DateRangeDTO(shiftStart, shiftStop));

        return splittedShifts;
    }

    private addBelongsToAnotherDayArrow(shift: ShiftDTO, date: Date, timeText: string): string {
        if (shift.actualStopTime.isBeforeOnDay(date) && shift.actualStopTime.isBeforeOnDay(this.controller.dateFrom))
            timeText += ' <';
        else if (shift.actualStartTime.isBeforeOnDay(date) && shift.actualStartTime.isBeforeOnDay(this.controller.dateFrom))
            timeText = '< ' + timeText;
        else if (shift.actualStartTime.isAfterOnDay(date) && shift.actualStartTime.isAfterOnDay(this.controller.dateTo))
            timeText = '> ' + timeText;
        else if (shift.actualStopTime.isAfterOnDay(date) && shift.actualStopTime.isAfterOnDay(this.controller.dateTo))
            timeText += ' >';

        return timeText;
    }

    private createWeekNumberSpan(shift: ShiftDTO, textColor: string) {
        let span = document.createElement('span');
        span.classList.add('shift-week-number');
        span.innerText = this.getWeekNumberText(shift);
        span.style.color = textColor;

        return span;
    }

    private getWeekNumberText(shift: ShiftDTO): string {
        return `${CalendarUtility.getWeekNr(shift.dayNumber)}/${shift.nbrOfWeeks}${this.controller.terms["common.weekshort"]}`;
    }

    private createShiftRightIcon(iconName: string, tooltip: string, iconNumber: number, textColor: string, additionalRightMargin: number = 0, solid: boolean = false): HTMLElement {
        let rightMargin = this.controller.isCompressedStyle ? 5 : 3;
        rightMargin += (iconNumber - 1) * 20 + additionalRightMargin;

        let icon = document.createElement('i');
        icon.classList.add(solid ? 'fas' : 'fal');
        icon.classList.add(this.controller.isCompressedStyle ? 'shift-compressed-right-icon' : 'shift-right-icon');
        icon.classList.add('fa-' + iconName);
        icon.style.right = `${rightMargin}px`;
        icon.style.color = textColor;
        icon.title = tooltip;

        return icon;
    }

    private getPixelsForTime(time: Date, moveIfStartsAfterVisibleRange: boolean): number {
        let pixels: number = 0;

        let columns: number;
        let oddMinutes: number = 0;
        if (moveIfStartsAfterVisibleRange && time.isAfterOnMinute(this.controller.dateTo)) {
            // Shift starts after visible range.
            // Move it to one hour before visible range ends to be able to see it.
            columns = this.columnWidths.length - this.controller.hourParts;
        } else {
            // Get number of minutes after visible start
            let minutesFromVisibleStart = this.getDifferenceInMinutes(this.controller.dates[0].date, time);
            // Get number of complete columns from visible start to the time
            columns = Math.floor(minutesFromVisibleStart / this.controller.dayViewMinorTickLength);
            // Get number of minutes after an even tick length (eg: 8:35 gives 5 minutes if interval is 15 minutes)
            oddMinutes = minutesFromVisibleStart - (columns * this.controller.dayViewMinorTickLength);
        }

        // Loop each column before time
        for (let i = 0; i < columns && i < this.columnWidths.length; i++) {
            for (let j = 0, k = this.columnWidths.length; j < k; j++) {
                if (this.columnWidths[j].index === i) {
                    pixels += this.columnWidths[j].width + 1;
                }
            }
        }

        // Add odd minutes
        if (oddMinutes > 0 && this.columnWidths.length > columns) {
            pixels += (this.columnWidths[columns].width / this.controller.dayViewMinorTickLength) * oddMinutes;
        }

        // Make sure pixels are inside visible range
        if (pixels < 0)
            pixels = 0;
        else if (pixels > this.scheduleWidth)
            pixels = this.scheduleWidth;

        return pixels;
    }

    public updateRowSizeAndPosition(row, useJQuery: boolean = false) {
        if (!row)
            return;

        if (useJQuery)
            row = $(row);

        if (this.controller.isTasksAndDeliveriesView) {
            for (let taskElem of row.getElementsByClassName('planning-shift')) {
                let task = this.getTaskFromElem(taskElem);
                if (task) {
                    if (this.controller.isTasksAndDeliveriesDayView) {
                        let left = this.getPixelsForTime(task.actualStartTime, true);
                        this.setElemPosition(taskElem, left);
                        this.setElemWidth(taskElem, this.getPixelsForTime(task.actualStopTime, false) - left);

                        let actualLengthElems = taskElem.getElementsByClassName('task-actual-length');
                        if (actualLengthElems && actualLengthElems.length > 0) {
                            let actualLengthElem = actualLengthElems[0];
                            // Place actual length in the middle of the "task window"
                            let windowStart = CalendarUtility.getMaxDate(task.actualStartTime, this.controller.dateFrom)
                            let windowStop = CalendarUtility.getMinDate(task.actualStopTime, this.controller.dateTo)
                            let middleOfWindow = windowStart.addMinutes(windowStop.diffMinutes(windowStart) / 2);

                            let actualStart = middleOfWindow.addMinutes(-(task.length / 2));

                            let windowLeft = this.getPixelsForTime(actualStart, true);
                            this.setElemPosition(actualLengthElem, windowLeft - left - 1); // Window starts relative from its task, therefore we need to reduce task start length
                            this.setElemWidth(actualLengthElem, this.getPixelsForTime(actualStart.addMinutes(task.length), false) - windowLeft + 1);
                        }
                    } else {
                        this.setWidthOnTaskElement(taskElem, this.columnWidth);
                    }
                }
            }
        } else if (this.controller.isStaffingNeedsView) {
            row.find('.staffing-needs-period').each((_, el) => {
                let elem = $(el);
                let period = this.getStaffingNeedsPeriodFromElem(elem);

                if (period) {
                    if (this.controller.isStaffingNeedsDayView) {
                        let left = this.getPixelsForTime(period.actualStartTime, true);
                        elem.css('left', left);
                        elem.css('width', this.getPixelsForTime(period.actualStopTime, false) - left);
                    } else {
                        this.setWidthOnStaffingNeedsPeriodElement(el, period, this.columnWidth);
                    }
                }
            });
        } else {
            for (let shiftElem of row.getElementsByClassName('planning-shift')) {
                let shift = this.getShiftFromElem(shiftElem);
                let width: number;

                if (shift) {
                    if (this.controller.isCommonDayView) {
                        // Whole shift
                        let containerLeft = this.getPixelsForTime(shift.actualStartTime, true);
                        this.setElemPosition(shiftElem, containerLeft);
                        width = this.getPixelsForTime(shift.actualStopTime, false) - containerLeft;
                        // If shift belongs to current date, but is compleately on day before, both left and width will be zero.
                        // Make it take up first hour of the day.
                        if (containerLeft === 0 && width === 0)
                            width = this.columnWidth * this.controller.hourParts;
                        this.setElemWidth(shiftElem, width);

                        let containers = shiftElem.getElementsByClassName('shift-split-container');
                        let rangeConter = 0;
                        let splittedShifts: DateRangeDTO[] = this.splitShiftOnBreaks(shift);
                        splittedShifts.forEach(range => {
                            let containerElem;
                            if (containers.length > rangeConter) {
                                containerElem = containers[rangeConter];
                                if (containerElem) {
                                    let left = this.getPixelsForTime(range.start, true) - containerLeft - 1;
                                    this.setElemPosition(containerElem, left);
                                    width = this.getPixelsForTime(range.stop, false) - containerLeft - left + 1;
                                    // If shift belongs to current date, but is compleately on day before, both left and width will be zero.
                                    // Make it take up first hour of the day.
                                    if (left === 0 && width === 0)
                                        width = this.columnWidth * this.controller.hourParts;
                                    this.setElemWidth(containerElem, width);

                                    let breaks = shiftElem.getElementsByClassName('shift-break');
                                    for (let breakElem of breaks) {
                                        let breakData = this.getBreakData(shift, parseInt(breakElem.id, 10));
                                        let breakLeft = this.getPixelsForTime(breakData.actualBreakStart, false);
                                        this.setElemPosition(breakElem, breakLeft - containerLeft - 1); // Break starts relative from its shift, therefore we need to reduce shift start length
                                        this.setElemWidth(breakElem, this.getPixelsForTime(breakData.actualBreakStart.addMinutes(breakData.breakMinutes), false) - breakLeft);
                                    }
                                }
                            }
                            rangeConter++;
                        });
                    } else if (this.controller.shiftStyle === TermGroup_TimeSchedulePlanningShiftStyle.ActualTimeCompressed || this.controller.shiftStyle === TermGroup_TimeSchedulePlanningShiftStyle.ActualTime) {
                        let pixelsPerMinute = this.pixelsPerTimeUnit / 24 / 60;

                        // Set start position
                        let position: number = 0;
                        if (shift.startTime.isAfterOnDay(this.controller.dateTo)) {
                            // Starts after visible range, show shift with only half the width, right justified in cell
                            position = this.pixelsPerTimeUnit / 2;
                            width = this.setWidthOnShiftElement(shift, shiftElem, this.columnWidth);
                        } else {
                            // If shift starts before visible view use 0
                            let differenceFromFirstDate = shift.actualStartTime.isAfterOnMinute(this.controller.dateFrom) ? this.getDifferenceInMinutes(shift.actualStartTime.beginningOfDay(), shift.actualStartTime) : 0;
                            position = differenceFromFirstDate * pixelsPerMinute;

                            // Set width
                            let shiftLength = this.getShiftLength(shift);
                            width = pixelsPerMinute * shiftLength;
                            this.setElemWidth(shiftElem, width);
                        }
                        this.setElemPosition(shiftElem, position);
                    } else {
                        width = this.setWidthOnShiftElement(shift, shiftElem, this.columnWidth);
                        if (shift.startTime.isAfterOnDay(this.controller.dateTo)) {
                            // Starts after visible range, show shift with only half the width, right justified in cell
                            this.setElemPosition(shiftElem, this.pixelsPerTimeUnit / 2);
                        }
                    }
                }

                // Make sure time and shift type does not write over week number
                if (shiftElem.getElementsByClassName('shift-week-number').length) {
                    // In day view time and shifttype are different spans
                    // In schedule view shifttype is written in time span
                    if (this.controller.isCommonDayView) {
                        // TODO: Fix for day view, depending on user setting
                    } else {
                        let timeElem = this.getFirstElementByClassName(shiftElem, 'shift-time');
                        if (timeElem)
                            this.setElemWidth(timeElem, width - 25);
                    }
                }
            }
        }

        if (this.controller.isCommonDayView) {
            let currentTimeDiv = useJQuery ? row.find('.current-time') : row.getElementsByClassName('current-time');
            if (currentTimeDiv.length > 0) {
                let left = this.getPixelsForTime(this.currentTime, false);
                this.setElemPosition(currentTimeDiv[0], left);
            }
        }
    }

    private setElemWidth(elem: HTMLElement, widthInPixels: number) {
        const newWidth = widthInPixels + "px";
        if (elem.style.width !== newWidth) {
            elem.style.width = newWidth;
        }
    }

    private setElemPosition(elem: HTMLElement, positionInPixels: number) {
        const newLeft = positionInPixels + "px";
        if (elem.style.left !== newLeft) {
            elem.style.left = newLeft;
        }
    }

    private setWidthOnShiftElement(shift: ShiftDTO, elem: HTMLElement, width: number): number {
        let days = 1;

        if (shift) {
            days = this.getShiftLengthInDays(shift);
            if (shift.startTime) {
                // Starts before visible range, show shift with only half the width, left justified in cell
                if (shift.startTime.isBeforeOnDay(this.controller.dateFrom) && (shift.stopTime.isSameDayAs(this.controller.dateFrom) || shift.stopTime.isBeforeOnDay(this.controller.dateFrom)))
                    days = 0.5;

                // Starts after visible range, show shift with only half the width, right justified in cell
                if (shift.startTime.isAfterOnDay(this.controller.dateTo))
                    days = 0.5;
            }

            if (days > this.controller.nbrOfVisibleDays)
                days = this.controller.nbrOfVisibleDays;
        }

        let newWidth: number = (width * days) + days - 5;
        this.setElemWidth(elem, newWidth);

        return newWidth;
    }

    private setWidthOnTaskElement(elem: HTMLElement, width) {
        let task = this.getTaskFromElem(elem);
        if (task) {
            let days = this.getTaskLengthInDays(task);
            this.setElemWidth(elem, width * days - 7); // -7 is to restore the padding.
        }
    }

    private setWidthOnStaffingNeedsPeriodElement(elem, period: StaffingNeedsRowPeriodDTO, width) {
        let days = this.getStaffingNeedsPeriodLengthInDays(period);
        this.setElemWidth(elem, width * days - 7);//-7 is to restore the padding.
    }

    public columnWidths: any[];
    private updateWidth() {
        let cols = $('.planning-scheduleview tbody tr:first .planning-day');

        this.columnWidths = [];
        let totalWidth = 0;
        let count = 0;
        cols.each((i, e) => {
            let colWidth = $(e).outerWidth() - 1;
            this.columnWidths.push({ index: i, width: colWidth });
            totalWidth += colWidth;
            count++;
        });

        let width = totalWidth / count;

        if (width) {
            this.columnWidth = width;
            if (this.controller.isCommonDayView)
                this.pixelsPerTimeUnit = this.columnWidth / 60 * this.controller.hourParts;//minutes
            else
                this.pixelsPerTimeUnit = this.columnWidth / 1;//hours, currently not used.
        }
    }

    private setScheduleWidth() {
        let col = $('.planning-scheduleview thead tr:first .planning-daterange');
        this.scheduleWidth = col.outerWidth();
    }

    public updateWidthOnAllElements(delay: number = 100, useJQuery: boolean = false) {
        this.$timeout(() => {
            this.updateWidth();
            this.setScheduleWidth();
            $('.planning-scheduleview table tbody tr').each((_, row) => this.updateRowSizeAndPosition(row, useJQuery));
        }, delay);
    }

    public updateEmployeesInfo(employees: EmployeeListDTO[]) {
        this.unrenderedEmployees = [];
        employees.forEach(employee => {
            this.updateEmployeeInfo(employee);
        });

        // Update info on employees that were not rendered in previous loop
        if (this.unrenderedEmployees.length > 0) {
            this.$timeout(() => {
                this.updateEmployeesInfo(this.unrenderedEmployees);
            }, 1000);
        }
    }

    private unrenderedEmployees: EmployeeListDTO[] = [];
    public updateEmployeeInfo(employee: EmployeeListDTO) {
        if (!employee || employee.isGroupHeader)
            return;

        let empRows = this.getEmployeeRows(employee.identifier);
        if (empRows.length > 0) {
            empRows.forEach(empTr => {
                let empTd = this.getFirstElementByClassName(empTr, 'planning-employee');
                if (empTd) {
                    if (this.controller.isCommonScheduleView && !this.controller.isCompressedStyle) {
                        if (this.controller.selectableInformationSettings.showEmployeeGroup && !employee.hidden) {
                            let groupName = this.controller.getCurrentEmployeeGroupName(employee);
                            if (groupName) {
                                let groupSpan = this.getFirstElementByClassName(empTd, 'employee-group');
                                if (groupSpan) {
                                    groupSpan.innerText = groupName;
                                    groupSpan.title = groupName;
                                }
                            }
                        }

                        let timeSpan = this.getFirstElementByClassName(empTd, 'time');
                        let cycleTimeSpan = this.getFirstElementByClassName(empTd, 'cycle-time');
                        let annualLeaveBalanceIcon = this.getFirstElementByClassName(empTd, 'annual-leave-balance-icon');
                        let annualLeaveBalanceSpan = this.getFirstElementByClassName(empTd, 'annual-leave-balance');
                        let annualTd = (this.controller.selectableInformationSettings.showPlanningPeriodSummary && this.controller.isScheduleView) ? this.getFirstElementByClassName(empTd, 'planning-employee-annual-time') : null;
                        this.setEmployeeTimes(empTd, timeSpan, cycleTimeSpan, annualLeaveBalanceSpan, annualLeaveBalanceIcon, annualTd, employee);
                    } else {
                        empTd.title = employee.toolTip;
                    }

                    let nameSpan = this.getFirstElementByClassName(empTd, 'name');
                    if (nameSpan) {
                        nameSpan.innerText = (employee.hidden || employee.employeePostId ? employee.name : employee.numberAndName);
                        if (this.controller.isCompressedStyle)
                            this.setOverUndertimeClass(nameSpan, employee);
                    }
                }
            });
        } else {
            // Employee not rendered yet, add employee to collection that will be retried
            this.unrenderedEmployees.push(employee);
        }
    }

    public updateEmployeeRow(targetRow, employee) {
        if (!targetRow || !employee)
            return;

        // Remove all current shifts from renderedShifts collection, otherwise they will not be rendered again
        let empShifts = this.controller.shifts.filter(s => (this.controller.isEmployeePostView ? s.employeePostId : s.employeeId) === employee.identifier);
        if (employee['accountId'])
            empShifts = empShifts.filter(s => s.accountId == employee['accountId']);

        let renderedShiftsForEmployee = this.controller.renderedShifts.find(r => r.employeeId === employee.identifier);
        if (renderedShiftsForEmployee)
            _.pullAll(renderedShiftsForEmployee.shiftIds, empShifts.map(s => s.timeScheduleTemplateBlockId));

        let renderedLeisureCodesForEmployee = this.controller.renderedLeisureCodes.find(r => r.employeeId === employee.identifier);
        if (renderedLeisureCodesForEmployee)
            _.pullAll(renderedLeisureCodesForEmployee.detailIds, empShifts.map(s => s.timeScheduleEmployeePeriodDetailId));

        // Replace the row
        let updatedSourceRow = this.renderEmployeeRow(employee);
        if (!(targetRow instanceof jQuery))
            targetRow = $(targetRow)
        targetRow.replaceWith(updatedSourceRow);

        // Update new row
        this.updateRowSizeAndPosition(updatedSourceRow);
        this.$compile(updatedSourceRow)(this.$scope);
    }

    private getIdString(identifier: number): string {
        return `${this.controller.isEmployeePostView ? 'empPostId' : 'empId'}${identifier}`;
    }

    public hasMultipleRows(identifier: number): boolean {
        return this.getEmployeeRows(identifier).length > 1;
    }

    public getEmployeeRows(identifier: number) {
        return document.querySelectorAll(`[identifier="${this.getIdString(identifier)}"]`);
    }

    public setShiftToolTip(shiftId: number, toolTip: string) {
        let shift = $('#' + shiftId);
        if (shift)
            shift.attr('title', toolTip);
    }

    public setLeisureCodeToolTip(detailId: number, toolTip: string) {
        let shift = $('#lc_' + detailId);
        if (shift)
            shift.attr('title', toolTip);
    }

    // HELP-METHODS

    // Html

    private getFirstElementByClassName(elem: Element, className: string): HTMLElement {
        const elems = elem.getElementsByClassName(className);
        if (elems.length > 0)
            return <HTMLElement>elems[0];

        return null;
    }

    // Employees

    private getEmployeeFromElem(elem): EmployeeListDTO {
        const employeeId: number = parseInt($(elem).parent().attr('id').substring(5), 10);

        return this.getEmployeeById(employeeId);
    }

    private getEmployeeById(employeeId: number) {
        return this.controller.getEmployeeById(employeeId);
    }

    private setEmployeeTimes(employeeTd: HTMLElement, timeSpan: HTMLElement, cycleTimeSpan: HTMLElement, annualLeaveSpan: HTMLElement, annualLeaveIcon: HTMLElement, annualTd: HTMLElement, employee: EmployeeListDTO) {
        if (!employee.hidden && !employee.workTimeMinutes && this.controller.getCurrentWorkTimeWeek(employee) > 0)
            this.controller.calculateEmployeeWorkTimes(employee);

        if (timeSpan) {
            let timeText = CalendarUtility.minutesToTimeSpan(employee.plannedMinutes ?? 0);

            if (!employee.hidden) {
                if (this.controller.selectableInformationSettings.showScheduleTypeFactorTime) {
                    let factorMinutes = (employee.plannedMinutes ?? 0) - (employee.timeScheduleTypeFactorMinutes ?? 0);
                    timeText += `/${CalendarUtility.minutesToTimeSpan(factorMinutes)}`;
                }

                timeText += `/${CalendarUtility.minutesToTimeSpan(employee.workTimeMinutes ?? 0)}`;
                if (this.controller.nbrOfVisibleDays > 7)
                    timeText += ` (${CalendarUtility.minutesToTimeSpan(employee.oneWeekWorkTimeMinutes)})`;
                this.setOverUndertimeClass(timeSpan, employee);
            }
            timeSpan.innerText = timeText;
        }

        if (!employee.hidden) {
            if (cycleTimeSpan) {
                let cycleTimeText = CalendarUtility.minutesToTimeSpan(employee.cyclePlannedMinutes ?? 0);
                let cycleTimeAverageText = CalendarUtility.minutesToTimeSpan(employee.cyclePlannedAverageMinutes ?? 0);
                cycleTimeSpan.innerText = `${cycleTimeText}/${cycleTimeAverageText}`;
            }

            if (annualLeaveSpan) {
                annualLeaveSpan.innerText = `${employee.getAnnualLeaveBalanceValue(this.controller.selectableInformationSettings.showAnnualLeaveBalanceFormat)}`;
                this.setAnnualLeaveBalanceClass(annualLeaveSpan, annualLeaveIcon, employee);
            }

            if (annualTd) {
                this.setAnnualScheduleTimeClass(annualTd, employee);

                if (this.controller.calculatePlanningPeriodScheduledTimeUseAveragingPeriod &&
                    (this.controller.currentPlanningPeriodChildInRangeExact || this.controller.hasPlanningPeriodHeadButNoChild)) {
                    let parentBalanceSpan = this.getFirstElementByClassName(annualTd, 'planning-employee-balance-parent');
                    if (parentBalanceSpan)
                        parentBalanceSpan.innerText = CalendarUtility.minutesToTimeSpan(employee.parentPeriodBalanceTimeMinutes, false, false, false, false, true);
                    let childBalanceSpan = this.getFirstElementByClassName(annualTd, 'planning-employee-balance-child');
                    if (childBalanceSpan)
                        childBalanceSpan.innerText = CalendarUtility.minutesToTimeSpan(employee.childPeriodBalanceTimeMinutes, false, false, false, false, true);
                }
            }
        }

        if (employeeTd) {
            this.controller.setEmployeeToolTip(employee);
            employeeTd.title = employee.toolTip;
        }
    }

    private setOverUndertimeClass(span: HTMLElement, employee: EmployeeListDTO) {
        if (employee.hidden)
            return;

        if (span) {
            span.classList.remove('overtime');
            span.classList.remove('undertime');

            // If filtered on shift type, do not show over/undertime color
            if ((this.controller.useAccountHierarchy || !this.controller.isFilteredOnAccountDim()) && !this.controller.isFilteredOnShiftType) {
                if (employee.plannedMinutes > (employee.workTimeMinutes + employee.maxScheduleTime))
                    span.classList.add('overtime');
                else if (employee.plannedMinutes < (employee.workTimeMinutes + employee.minScheduleTime))
                    span.classList.add('undertime');
            }
        }
    }

    private setAnnualLeaveBalanceClass(span: HTMLElement, icon: HTMLElement, employee: EmployeeListDTO) {
        if (employee.hidden)
            return;

        if (span) {
            span.classList.remove('balance-positive');
            span.classList.remove('balance-negative');

            if (employee.annualLeaveBalanceMinutes > 0) {
                span.classList.add('balance-positive');
                if (icon)
                    icon.classList.add('fa-balance-scale-left');
            } else if (employee.annualLeaveBalanceMinutes < 0) {
                span.classList.add('balance-negative');
                if (icon)
                    icon.classList.add('fa-balance-scale-right');
            } else {
                if (icon)
                    icon.classList.add('fa-balance-scale');
            }
        }
    }

    public setContactInfo(employeeId: number, contactItems: ContactAddressItemDTO[]) {

        // TODO: Replace with new 'non jquery' methods

        let rows = this.getEmployeeRows(employeeId);
        rows.forEach(row => {
            let employeeTd = row.getElementsByClassName('planning-employee')[0];

            // Check if list already exists
            let lists = row.getElementsByClassName('contact-info');

            // If no items, remove list
            if (!contactItems || contactItems.length === 0) {
                if (lists.length)
                    employeeTd.removeChild(lists[0]);
                return;
            }

            let list: HTMLElement;
            if (lists.length) {
                list = <HTMLElement>lists[0];
                list.innerHTML = '';
            } else {
                list = document.createElement('ul');
                list.classList.add('contact-info');
                employeeTd.appendChild(list);
            }

            // Create list items
            let deleteLi = document.createElement('li');
            let deleteIconSpan = document.createElement('span');
            deleteIconSpan.classList.add('fal', 'fa-times', 'iconDelete', 'pull-right', 'smallFont', 'link');
            deleteIconSpan.setAttribute('data-ng-click', 'ctrl.scheduleHandler.setContactInfo(' + employeeId + ', null)');
            this.$compile(deleteIconSpan)(this.$scope);
            deleteLi.appendChild(deleteIconSpan);
            list.appendChild(deleteLi);

            contactItems.forEach(item => {
                let li = document.createElement('li');
                li.title = item.name;
                if (item.isSecret)
                    li.title = `${li.title} (${this.controller.terms["common.contactaddresses.issecret"]})`;
                let div = document.createElement('div');
                let iconSpan = document.createElement('span');
                let iconClasses: string[] = ContactAddressItemDTO.getIcon(item.contactAddressItemType).split(' ');
                iconClasses.forEach(cls => {
                    iconSpan.classList.add(cls);
                });
                if (item.isSecret)
                    iconSpan.classList.add('errorColor');
                iconSpan.style.marginRight = '5px';
                let textSpan = document.createElement('span');
                textSpan.innerHTML = item.displayAddress
                if (item.isSecret) {
                    textSpan.classList.add('errorColor');
                    textSpan.classList.add('italic');
                }
                div.appendChild(iconSpan);
                div.appendChild(textSpan);
                li.appendChild(div);
                list.appendChild(li);
            });
        });
    }

    public showingContactInfo(employeeId: number): boolean {
        let showing: boolean = false;

        let rows = this.getEmployeeRows(employeeId);
        rows.forEach(row => {
            if (row.getElementsByClassName('contact-info').length > 0)
                showing = true;
        });

        return showing;
    }

    private setAnnualScheduleTimeClass(td: HTMLElement, employee: EmployeeListDTO) {
        if (td && employee) {
            if (this.controller.calculatePlanningPeriodScheduledTimeUseAveragingPeriod) {
                if (this.controller.currentPlanningPeriod &&
                    (this.controller.currentPlanningPeriodChildInRangeExact || this.controller.hasPlanningPeriodHeadButNoChild)) {
                    let parentBalance = employee.parentPeriodBalanceTimeMinutes || 0;
                    let childBalance = employee.childPeriodBalanceTimeMinutes || 0;

                    if (parentBalance > 0)
                        td.style.backgroundColor = this.controller.planningPeriodColorOver;
                    else if (parentBalance < 0)
                        td.style.backgroundColor = this.controller.planningPeriodColorUnder;
                    else
                        td.style.backgroundColor = this.controller.planningPeriodColorEqual;
                    td.style.color = GraphicsUtility.foregroundColorByBackgroundBrightness(td.style.backgroundColor);

                    let tooltip = '';
                    let headName = this.controller.planningPeriodHead ? this.controller.planningPeriodHead.name : '';
                    let childName = this.controller.planningPeriodHead && this.controller.planningPeriodChild ? this.controller.planningPeriodHead.childName : '';
                    if (headName)
                        tooltip += `${headName}: ${CalendarUtility.minutesToTimeSpan(parentBalance)}`;
                    if (childName)
                        tooltip += `\n${childName}: ${CalendarUtility.minutesToTimeSpan(childBalance)}`;

                    tooltip += `\n\n${this.controller.terms["time.schedule.planning.annualsummarytooltip"]}`
                    td.title = tooltip;
                } else {
                    td.style.backgroundColor = 'f7f4f0';    // @soe-header-background-color
                    td.title = this.controller.terms["time.schedule.planning.employeeperiodtimesummary.opensummaryerror.message"];
                }
            } else {
                let scheduledTime = employee.annualScheduledTimeMinutes || 0;
                let workTime = employee.annualWorkTimeMinutes || 0;

                if (!scheduledTime && !workTime)
                    td.style.backgroundColor = 'f7f4f0';    // @soe-header-background-color
                else if (scheduledTime > workTime)
                    td.style.backgroundColor = this.controller.planningPeriodColorOver;
                else if (scheduledTime < workTime)
                    td.style.backgroundColor = this.controller.planningPeriodColorUnder;
                else
                    td.style.backgroundColor = this.controller.planningPeriodColorEqual;
                td.title = `${CalendarUtility.minutesToTimeSpan(scheduledTime)}/${CalendarUtility.minutesToTimeSpan(workTime)}\n\n${this.controller.terms["time.schedule.planning.annualsummarytooltip"]}`;
            }
        }
    }

    // Employee posts

    private getEmployeePostFromElem(elem): EmployeeListDTO {
        let employeePostId: number = parseInt($(elem).parent().attr('id').substring(9), 10);

        return this.getEmployeePostById(employeePostId);
    }

    private getEmployeePostById(employeePostId: number) {
        return this.controller.getEmployeePostById(employeePostId);
    }

    public selectEmployeePost(employeePost: EmployeeListDTO, notify: boolean = false) {
        if (employeePost) {
            // Unselect any previously selected
            this.controller.allEmployees.filter(e => e.selected).forEach(empPost => {
                this.unselectEmployeePost(empPost);
            });

            employeePost.selected = true;
            let cell = this.getEmployeePostCell(employeePost);
            if (cell)
                cell.classList.add('selected-employeepost');

            if (notify)
                this.controller.employeePostSelected(employeePost.identifier);
        }
    }

    public unselectEmployeePost(employeePost: EmployeeListDTO, notify: boolean = false) {
        if (employeePost) {
            employeePost.selected = false;
            let cell = this.getEmployeePostCell(employeePost);
            if (cell)
                cell.classList.remove('selected-employeepost');

            if (notify)
                this.controller.employeePostSelected(0);
        }
    }

    private getEmployeePostCell(employeePost: EmployeeListDTO) {
        let tr = document.getElementById('empPostId' + employeePost.identifier);
        if (tr && tr.children.length > 0) {
            let td = tr.getElementsByClassName('planning-employee-post');
            if (td && td.length > 0)
                return td[0];
        }

        return null;
    }

    // Slots

    private getSlotFromElement(elem): SlotDTO {
        elem = $(elem);
        let slot = new SlotDTO();
        slot.startTime = (<string>elem.attr('date')).parsePipedDateTime();
        slot.employeeId = parseInt(elem.attr('rowId'), 10);

        return slot;
    }

    private selectSlot(elem) {
        $(elem).addClass('selected-slot');
    }

    private unselectSlot(elem) {
        $(elem).removeClass('selected-slot');
    }

    public getSelectedSlots(): SlotDTO[] {
        let selectedSlots: SlotDTO[] = [];

        let slots = $('.planning-scheduleview tbody').find('.selected-slot');
        _.forEach(slots, slot => {
            selectedSlots.push(this.getSlotFromElement(slot));
        });

        return _.orderBy(selectedSlots, s => s.startTime);
    }

    private clearSelectedSlots() {
        let slots = $('.planning-scheduleview tbody').find('.selected-slot');
        _.forEach(slots, slot => {
            this.unselectSlot(slot);
        });
    }

    public getSlotInfo() {
        // Get selected slot (employee and date)
        let employeeId: number;
        let date: Date;
        let selectedShifts = this.getSelectedShifts();
        if (selectedShifts.length > 0) {
            employeeId = selectedShifts[0].employeeId;
            date = selectedShifts[0].actualStartTime.date();
        } else {
            let slots = this.getSelectedSlots();
            if (slots.length > 0) {
                employeeId = slots[0].employeeId;
                date = slots[0].startTime.date();
            }
        }

        return { employeeId, date };
    }

    private addDayTypeColor(td: HTMLTableCellElement, dateDay: DateDay, addToday: boolean, addSeparator: boolean) {
        if (td?.classList) {
            if (addToday && dateDay.isToday)
                td.classList.add('planning-day-today');

            // Saturday/Sunday, also checks holidays
            if (dateDay.isSunday)
                td.classList.add('planning-day-sunday');
            else if (dateDay.isSaturday)
                td.classList.add('planning-day-saturday');

            if (addSeparator && dateDay.date.getDay() === 0 && !_.last(this.controller.dates).date.isSameDayAs(dateDay.date))
                td.classList.add('planning-vertical-separator');
        }
    }

    // Shifts

    public getShiftById(shiftId: number, isAbsenceRequest = false, onlyVisible = false): ShiftDTO {
        const key = this.controller.getShiftKey(shiftId, isAbsenceRequest);
        if (onlyVisible && !this.controller.selectableInformationSettings.showHiddenShifts) {
            return this.controller.visibleShiftsMap.get(key);
        } else {
            return this.controller.allShiftsMap.get(key);
        }
    }

    public getShiftFromElem(elem): ShiftDTO {
        return this.getShiftFromIdString(elem.id);
    }

    public getShiftFromJQueryElem(elem): ShiftDTO {
        return this.getShiftFromIdString($(elem).attr('id'));
    }

    private getShiftFromIdString(idStr: string): ShiftDTO {
        if (!idStr)
            return null;

        if (idStr.startsWith('lc_')) {
            // Leisure code
            return this.getLeisureCodeById(idStr);
        } else if (idStr.startsWith('ar_')) {
            // Absence request
            let parts = idStr.split('_');
            return this.getShiftById(parseInt(parts[1], 10), true, true);
        } else {
            // Shift
            return this.getShiftById(parseInt(idStr, 10), false, true);
        }
    }

    public getIntersectingOnDutyShifts(shiftId: number): ShiftDTO[] {
        const shift = this.getShiftById(shiftId);
        return shift ? this.controller.shifts.filter(s => s.isOnDuty && s.timeScheduleTemplateBlockId !== shift.timeScheduleTemplateBlockId && s.employeeId === shift.employeeId && CalendarUtility.isRangesOverlapping(s.actualStartTime.addSeconds(1), s.actualStopTime.addSeconds(-1), shift.actualStartTime, shift.actualStopTime) && (s.accountId === shift.accountId || !s.accountId || !shift.accountId)) : [];
    }

    private getShiftLength(shift: ShiftDTO) {
        // Get shift actual start time or beginning of display if shift goes beyond that
        let actualStart = CalendarUtility.maxOfDates(shift.actualStartTime, this.controller.dateFrom);
        // Get shift actual stop time or end of display if shift goes beyond that
        let actualStop = CalendarUtility.minOfDates(shift.actualStopTime, this.controller.dateTo);

        return actualStop.diffMinutes(actualStart);
    }

    public getShiftLengthInDays(shift: ShiftDTO) {
        // Get shift actual start time or beginning of display if shift goes beyond that
        let actualStart = CalendarUtility.maxOfDates(shift.actualStartTime.beginningOfDay(), this.controller.dateFrom);
        // Get shift actual stop time or end of display if shift goes beyond that
        let actualStop = CalendarUtility.minOfDates(shift.actualStopTime, this.controller.dateTo).beginningOfDay();

        let days = actualStop.diffDays(actualStart) + 1;
        if (days === 0)
            days = 1;

        return days;
    }

    public getDifferenceInMinutes(startDate: Date, endDate: Date) {
        // Get shift actual stop time or end of display if shift goes beyond that
        let actualStop = CalendarUtility.getMinDate(endDate, this.controller.dateTo.addSeconds(1));
        // Get shift actual stop time or beginning of display if shift goes beyond that
        actualStop = CalendarUtility.getMaxDate(actualStop, this.controller.dateFrom);

        return actualStop.diffMinutes(startDate);
    }

    public selectShift(shift: ShiftDTO, selectLinked: boolean = true, notify: boolean = false) {
        if (shift && !shift.isReadOnly) {
            shift.selected = true;
            if (shift.isLeisureCode)
                $('#lc_' + shift.timeScheduleEmployeePeriodDetailId).addClass('selected-shift');
            else if (shift.isAbsenceRequest)
                $('#ar_' + shift.timeScheduleTemplateBlockId).addClass('selected-shift');
            else
                $('#' + shift.timeScheduleTemplateBlockId).addClass('selected-shift');
            this.clearSelectedSlots();

            if (selectLinked && shift.link) {
                // Select all linked shifts
                let linkedShifts = this.controller.shifts.filter(s => s.link === shift.link && s.type === shift.type && s.timeScheduleTemplateBlockId !== shift.timeScheduleTemplateBlockId);
                this.selectShifts(linkedShifts, false, false);
            }

            if (notify)
                this.controller.shiftSelected();
        }
    }

    public selectShifts(shifts: ShiftDTO[], selectLinked: boolean = false, notify: boolean = false) {
        shifts.forEach(shift => {
            this.selectShift(shift, selectLinked, false);
        });

        if (notify)
            this.controller.shiftSelected();
    }

    public unselectShift(shift: ShiftDTO, unselectLinked: boolean = true, notify: boolean = false) {
        if (shift) {
            shift.selected = false;
            if (shift.isLeisureCode)
                $('#lc_' + shift.timeScheduleEmployeePeriodDetailId).removeClass('selected-shift');
            else if (shift.isAbsenceRequest)
                $('#ar_' + shift.timeScheduleTemplateBlockId).removeClass('selected-shift');
            else
                $('#' + shift.timeScheduleTemplateBlockId).removeClass('selected-shift');

            if (unselectLinked) {
                // Unselect all linked shifts
                let linkedShifts = this.controller.shifts.filter(s => s.link === shift.link && s.type === shift.type && s.timeScheduleTemplateBlockId !== shift.timeScheduleTemplateBlockId);
                this.unselectShifts(linkedShifts, false);
            }

            if (notify)
                this.controller.shiftSelected();
        }
    }

    public unselectShifts(shifts: ShiftDTO[], notify: boolean = false) {
        shifts.forEach(shift => {
            this.unselectShift(shift, false, false);
        });

        if (notify)
            this.controller.shiftSelected();
    }

    public getSelectedShifts() {
        return this.controller.shifts ? this.controller.shifts.filter(s => s.selected) : [];
    }

    public clearSelectedShifts(notify: boolean = false) {
        this.unselectShifts(this.getSelectedShifts());

        if (notify)
            this.controller.shiftSelected();
    }

    public highlightShift(shift: ShiftDTO) {
        if (shift) {
            shift.highlighted = true;
            if (shift.isLeisureCode)
                $('#lc_' + shift.timeScheduleEmployeePeriodDetailId).addClass('highlighted-shift');
            else if (shift.isAbsenceRequest)
                $('#ar_' + shift.timeScheduleTemplateBlockId).addClass('highlighted-shift');
            else
                $('#' + shift.timeScheduleTemplateBlockId).addClass('highlighted-shift');
        }
    }

    public highlightShifts(shifts: ShiftDTO[]) {
        shifts.forEach(shift => {
            this.highlightShift(shift);
        });
    }

    public unhighlightShift(shift: ShiftDTO) {
        if (shift) {
            shift.highlighted = false;
            if (shift.isLeisureCode)
                $('#lc_' + shift.timeScheduleEmployeePeriodDetailId).removeClass('highlighted-shift');
            else if (shift.isAbsenceRequest)
                $('#ar_' + shift.timeScheduleTemplateBlockId).removeClass('highlighted-shift');
            else
                $('#' + shift.timeScheduleTemplateBlockId).removeClass('highlighted-shift');
        }
    }

    public unhighlightShifts(shifts: ShiftDTO[]) {
        shifts.forEach(shift => {
            this.unhighlightShift(shift);
        });
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

        const actualBreakStart = this.getBreakStart(shift, breakNo);
        const breakEnd = actualBreakStart.addMinutes(this.getBreakLength(shift, breakNo));

        // Make sure break is within shift (otherwise breaks on shifts hidden by filter will be visible)
        if (CalendarUtility.getIntersectingDuration(shift.actualStartTime, shift.actualStopTime, actualBreakStart, breakEnd) === 0)
            return;

        const span = document.createElement('span');
        span.classList.add('shift-break');
        span.id = shift[`break${breakNo}Id`];

        const text = this.dragDropHelper.formatStartStopDate(actualBreakStart, breakEnd);
        if (this.controller.selectableInformationSettings.timePosition === TermGroup_TimeSchedulePlanningTimePosition.Left ||
            this.controller.selectableInformationSettings.timePosition === TermGroup_TimeSchedulePlanningTimePosition.ShiftEdges) {
            const textSpan = document.createElement('span');
            textSpan.classList.add('shift-break-text');
            textSpan.innerText = text;
            span.appendChild(textSpan);
        }
        span.title = text;

        targetElem.appendChild(span);
    }

    public getBreakData(shift: ShiftDTO, breakId: number) {
        const breakNo = this.getBreakNo(shift, breakId);

        return { breakMinutes: this.getBreakLength(shift, breakNo), actualBreakStart: this.getBreakStart(shift, breakNo) };
    }

    public getBreakNo(shift: ShiftDTO, breakId: number): number {
        let nbr: number;
        [1, 2, 3, 4].forEach(no => {
            if (shift[`break${no}Id`] == breakId) {
                nbr = no;
            }
        });

        return nbr;
    }

    public getBreakStart(shift: ShiftDTO, breakNo: number): Date {
        let start: Date = new Date(shift.actualStartTime);
        start.setHours((<Date>shift[`break${breakNo}StartTime`]).getHours());
        start.setMinutes((<Date>shift[`break${breakNo}StartTime`]).getMinutes());

        return start;
        //return shift.actualStartTime.mergeTime(CalendarUtility.convertToDate(shift[`break${breakNo}StartTime`]));
    }

    public getBreakLength(shift: ShiftDTO, breakNo: number): number {
        return shift[`break${breakNo}Minutes`];
    }

    // Leisure codes

    public getLeisureCodeById(detailId: string): ShiftDTO {
        let parts = detailId.split('_');
        return this.controller.shifts.find(s => s.timeScheduleEmployeePeriodDetailId === parseInt(parts[1], 10));
    }

    // Tasks

    public getTaskById(taskId: string): StaffingNeedsTaskDTO {
        let parts = taskId.split('_');
        return this.controller.allTasks.find(t => t.type == parseInt(parts[0], 10) && t.parentId === parseInt(parts[1], 10) && t.id === parseInt(parts[2], 10) && t.dateId === parseInt(parts[3], 10));
    }

    public getTaskFromElem(elem: HTMLElement): StaffingNeedsTaskDTO {
        return this.getTaskById(elem.getAttribute('id'));
    }

    public getTaskFromJQueryElem(elem): StaffingNeedsTaskDTO {
        return this.getTaskById($(elem).attr('id'));
    }

    public getTaskLengthInDays(task: StaffingNeedsTaskDTO) {
        // Get task actual stop time or end of display if task goes beyond that
        let actualStop = CalendarUtility.getMinDate(task.actualStopTime, this.controller.dateTo).beginningOfDay();
        return actualStop.diffDays(task.actualStartTime.beginningOfDay()) + 1;
    }

    public selectTask(task: StaffingNeedsTaskDTO) {
        if (task) {
            task.selected = true;
            $('#' + task.taskId).addClass('selected-shift');
        }
    }

    public selectTasks(tasks: StaffingNeedsTaskDTO[]) {
        tasks.forEach(task => {
            this.selectTask(task);
        });
    }

    public unselectTask(task: StaffingNeedsTaskDTO) {
        if (task) {
            task.selected = false;
            $('#' + task.taskId).removeClass('selected-shift');
        }
    }

    public unselectTasks(tasks: StaffingNeedsTaskDTO[]) {
        tasks.forEach(task => {
            this.unselectTask(task);
        });
    }

    public getSelectedTasks() {
        return this.controller.allTasks ? this.controller.allTasks.filter(s => s.selected) : [];
    }

    public clearSelectedTasks() {
        this.unselectTasks(this.getSelectedTasks());
    }

    private get isGroupedByDepartment(): boolean {
        if (this.controller.isTasksAndDeliveriesDayView) {
            return (this.controller.tadDayViewGroupBy === TermGroup_StaffingNeedsDayViewGroupBy.AccountDim2 || this.controller.tadDayViewGroupBy === TermGroup_StaffingNeedsDayViewGroupBy.AccountDim3 || this.controller.tadDayViewGroupBy === TermGroup_StaffingNeedsDayViewGroupBy.AccountDim4 || this.controller.tadDayViewGroupBy === TermGroup_StaffingNeedsDayViewGroupBy.AccountDim5 || this.controller.tadDayViewGroupBy === TermGroup_StaffingNeedsDayViewGroupBy.AccountDim6);
        } else if (this.controller.isTasksAndDeliveriesScheduleView) {
            return (this.controller.tadScheduleViewGroupBy === TermGroup_StaffingNeedsScheduleViewGroupBy.AccountDim2 || this.controller.tadScheduleViewGroupBy === TermGroup_StaffingNeedsScheduleViewGroupBy.AccountDim3 || this.controller.tadScheduleViewGroupBy === TermGroup_StaffingNeedsScheduleViewGroupBy.AccountDim4 || this.controller.tadScheduleViewGroupBy === TermGroup_StaffingNeedsScheduleViewGroupBy.AccountDim5 || this.controller.tadScheduleViewGroupBy === TermGroup_StaffingNeedsScheduleViewGroupBy.AccountDim6);
        }

        return false;
    }

    private get isGroupedByShiftType(): boolean {
        if (this.controller.isTasksAndDeliveriesDayView) {
            return (this.controller.tadDayViewGroupBy === TermGroup_StaffingNeedsDayViewGroupBy.ShiftType);
        } else if (this.controller.isTasksAndDeliveriesScheduleView) {
            return (this.controller.tadScheduleViewGroupBy === TermGroup_StaffingNeedsScheduleViewGroupBy.ShiftType);
        }

        return false;
    }

    // Staffing needs

    public getStaffingNeedsPeriodById(periodId: string): StaffingNeedsRowPeriodDTO {
        let id: number = parseInt(periodId.substring(8), 10);

        let period;
        for (let head of this.controller.heads) {
            for (let row of head.rows) {
                period = row.periods.find(p => p.staffingNeedsRowPeriodId === id);
                if (period)
                    break;
            }
            if (period)
                break;
        }

        return period;
    }

    public getStaffingNeedsPeriodFromElem(elem): StaffingNeedsRowPeriodDTO {
        return this.getStaffingNeedsPeriodById(elem.attr('id'));
    }

    public getStaffingNeedsPeriodLengthInDays(period: StaffingNeedsRowPeriodDTO) {
        // Get period actual stop time or end of display if period goes beyond that
        let actualStop = CalendarUtility.getMinDate(period.actualStopTime, this.controller.dateTo).beginningOfDay();
        return actualStop.diffDays(period.actualStartTime.beginningOfDay()) + 1;
    }

    // Drag n drop
    public enableDragAndDropOfEmployees() {
        this.dragDropHelper.enableDragDropOfEmployeeList();
    }

    public enableDragAndDropOfTasks() {
        this.dragDropHelper.enableDragDropOfUnscheduledTaskList();
    }

    public enableDragAndDropOfOrders() {
        this.dragDropHelper.enableDragDropOfOrderList();
    }
}