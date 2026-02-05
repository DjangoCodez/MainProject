import { ScheduleHandler } from "./ScheduleHandler";
import { ScheduleController } from "./ScheduleDirective";
import { AttestEmployeeDayTimeBlockDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { SoeTimeBlockClientChange } from "../../../Util/CommonEnumerations";

export class DragDropHelper {

    private resizeableOptionsBlock;
    private resizeableOptionsBlockStart;
    private resizeableOptionsBlockStop;
    private offsetX: number;
    private clientX: number;
    private wasCancelled: boolean = false;

    private currentResizeBlock: AttestEmployeeDayTimeBlockDTO;
    private resizeTarget: string;
    private resizeOriginalX: number;
    private dragRoundingMinutes: number = 5;

    constructor(private controller: ScheduleController, private scheduleHandler: ScheduleHandler, private $filter: ng.IFilterService) {
        this.performFirstTimeSetup = this.performFirstTimeSetup.bind(this);

        this.resizeableOptionsBlock = {
            handles: 'w, e',
            autoHide: true,
            containment: ".timeattest-schedule > table",
            start: this.onBlockResizeStart.bind(this),
            resize: this.onBlockResize.bind(this),
            stop: this.onBlockResizeStop.bind(this),
            cancel: ".ui-state-disabled, .no-dragdrop"
        };

        this.resizeableOptionsBlockStart = {
            handles: 'w',
            autoHide: true,
            containment: ".timeattest-schedule > table",
            start: this.onBlockResizeStart.bind(this),
            resize: this.onBlockResize.bind(this),
            stop: this.onBlockResizeStop.bind(this),
            cancel: ".ui-state-disabled, .no-dragdrop"
        };

        this.resizeableOptionsBlockStop = {
            handles: 'e',
            autoHide: true,
            containment: ".timeattest-schedule > table",
            start: this.onBlockResizeStart.bind(this),
            resize: this.onBlockResize.bind(this),
            stop: this.onBlockResizeStop.bind(this),
            cancel: ".ui-state-disabled, .no-dragdrop"
        };
    }

    // SETUP

    public performFirstTimeSetup() {
        $(document).keyup(e => {
            if (e.keyCode === 27) { // Escape key
                if ($('.timeattest-shift-drop-zone').sortable("instance")) {
                    this.wasCancelled = true;
                    $('.timeattest-shift-drop-zone').sortable("cancel");
                }
            }
        });
    }

    // DRAG N DROP

    private onBlockResizeStart(e, ui) {
        // Called when starting to resize a shift

        // Make sure resized element is on top of the rest (default 100)
        $((ui.originalElement)[0]).css('z-index', 1000);

        this.currentResizeBlock = this.scheduleHandler.getTimeBlockFromElem(ui.originalElement);
        this.resizeTarget = $(e.originalEvent.target).hasClass('ui-resizable-w') ? 'start' : 'stop';
        this.resizeOriginalX = e.clientX;

        this.showBlockTimeTooltip(this.getBlockTimeElement(ui.originalElement));
    }

    private onBlockResize(e, ui) {
        // Called repeatedly while resizing a block
        if (!this.currentResizeBlock)
            return;

        var pixelsPerMinute = this.scheduleHandler.pixelsPerTimeUnit;

        // For some reason width during drag will nog go below 10 px.
        // On smaller screen resolutions that will not go below 10 minutes,
        // which is needed for the removeTimeBlock to trigger below.
        if (pixelsPerMinute < 1 && ui.size.width === 10)
            ui.size.width = 2;
        var sizeDiff = ui.size.width - ui.originalSize.width;

        ui.originalElement.css('width', ui.size.width);
        ui.originalElement.css('left', ui.position.left);

        var minutesMoved = Math.abs(Math.ceil(sizeDiff / pixelsPerMinute));

        var direction = this.resizeOriginalX > e.clientX ? -1 : 1;

        if (this.resizeTarget === 'start') {
            this.currentResizeBlock.startTimeDuringMove = this.adjustToClosestMinutes(this.currentResizeBlock.startTime.addMinutes(direction * minutesMoved), this.dragRoundingMinutes);

            if (this.currentResizeBlock.startTimeDuringMove.isSameOrBeforeOnMinute(this.controller.dateFrom)) {
                this.hideBlockTimeTooltip(this.getBlockTimeElement(ui.originalElement));
                // Workaround for invalid graphics if dragging fast before start of schedule
                // For some reason the shift is painted with wrong stop time, therefore we repaint the whole row, then restoring original time.
                let origStartTime = this.currentResizeBlock.startTime;
                this.currentResizeBlock.startTimeDuringMove = this.currentResizeBlock.startTime = this.controller.dateFrom;
                let row = this.getResizedRow(ui.originalElement);
                this.controller.editTimeBlock(this.currentResizeBlock, row, true, false);
                this.scheduleHandler.updateTimeBlockRow(row);
                this.currentResizeBlock.startTime = origStartTime;
                this.currentResizeBlock = null;
            } else if (this.currentResizeBlock.startTimeDuringMove.isSameOrAfterOnMinute(this.currentResizeBlock.stopTime.addMinutes(-this.dragRoundingMinutes * 2))) {
                this.hideBlockTimeTooltip(this.getBlockTimeElement(ui.originalElement));
                this.controller.removeTimeBlock(this.currentResizeBlock, this.getResizedRow(ui.originalElement));
                this.currentResizeBlock = null;
            }
        } else {
            this.currentResizeBlock.stopTimeDuringMove = this.adjustToClosestMinutes(this.currentResizeBlock.stopTime.addMinutes(direction * minutesMoved), this.dragRoundingMinutes);

            if (this.currentResizeBlock.stopTimeDuringMove.isSameOrAfterOnMinute(this.controller.dateTo)) {
                this.hideBlockTimeTooltip(this.getBlockTimeElement(ui.originalElement));
                this.controller.editTimeBlock(this.currentResizeBlock, this.getResizedRow(ui.originalElement), false, true);
                this.currentResizeBlock = null;
            } else if (this.currentResizeBlock.stopTimeDuringMove.isSameOrBeforeOnMinute(this.currentResizeBlock.startTime.addMinutes(this.dragRoundingMinutes * 2))) {
                this.hideBlockTimeTooltip(this.getBlockTimeElement(ui.originalElement));
                this.controller.removeTimeBlock(this.currentResizeBlock, this.getResizedRow(ui.originalElement));
                this.currentResizeBlock = null;
            }
        }

        this.setBlockTime(this.getBlockTimeElement(ui.originalElement));
    }

    private onBlockResizeStop(e, ui) {
        // Called when finished resizing a block
        if (!this.currentResizeBlock)
            return;

        this.hideBlockTimeTooltip(this.getBlockTimeElement(ui.originalElement));

        var block = this.currentResizeBlock;
        if (this.resizeTarget === 'stop' && this.currentResizeBlock.stopTimeDuringMove.isSameOrBeforeOnMinute(this.currentResizeBlock.startTime.addMinutes(this.dragRoundingMinutes)))
            return;

        if (block.isAbsence) {
            // Cannot drag absence blocks outside schedule
            var scheduleStart: Date = this.controller.getScheduleStart();
            if (block.startTimeDuringMove < scheduleStart)
                block.startTimeDuringMove = scheduleStart;

            var scheduleStop: Date = this.controller.getScheduleStop();
            if (block.stopTimeDuringMove > scheduleStop)
                block.stopTimeDuringMove = scheduleStop;
        }

        this.controller.timeBlockResized(block, this.getTimeBlockClientChange());
    }

    private getBlockTimeElement(parentElem): JQuery {
        var elem: JQuery = parentElem;
        if (elem && !elem.tooltip('instance')) {
            elem.tooltip();
            elem.tooltip('option', 'classes.ui-tooltip', 'highlight');
            elem.tooltip('option', 'position', { my: 'left bottom', at: 'left top-5' });
        }

        return elem;
    }

    private showBlockTimeTooltip(elem: JQuery) {
        if (elem)
            elem.tooltip('open');
    }

    private hideBlockTimeTooltip(elem: JQuery) {
        if (elem)
            elem.tooltip('close');
    }

    private setBlockTime(elem: JQuery) {
        if (elem && this.currentResizeBlock) {
            elem.tooltip().trigger('onmouseover');
            if (this.resizeTarget === 'start') {
                //elem.text(this.currentResizeBlock.startTimeDuringMove.toFormattedTime());
                elem.tooltip('option', 'content', "{0}-{1}".format(this.currentResizeBlock.startTimeDuringMove.toFormattedTime(), this.currentResizeBlock.stopTime.toFormattedTime()));
            } else {
                //elem.text(this.currentResizeBlock.stopTimeDuringMove.toFormattedTime());
                elem.tooltip('option', 'content', "{0}-{1}".format(this.currentResizeBlock.startTime.toFormattedTime(), this.currentResizeBlock.stopTimeDuringMove.toFormattedTime()));
            }
        }
    }

    // HELP-METHODS

    public setDragDropOptionsForRow(rowElem: HTMLElement) {
        var row = $(rowElem);

        var blocks = row.find('.planning-shift-resize-start.planning-shift-resize-stop');
        if (blocks.length) {
            _.forEach(blocks, block => {
                $(block).resizable(this.resizeableOptionsBlock);
            });
        }

        blocks = row.find('.planning-shift-resize-start').not('.planning-shift-resize-stop');
        if (blocks.length) {
            _.forEach(blocks, block => {
                $(block).resizable(this.resizeableOptionsBlockStart);
            });
        }

        blocks = row.find('.planning-shift-resize-stop').not('.planning-shift-resize-start');
        if (blocks.length) {
            _.forEach(blocks, block => {
                $(block).resizable(this.resizeableOptionsBlockStop);
            });
        }

        return row;
    }

    private adjustToClosestMinutes(date: Date, minutes: number) {
        var diff = date.minutes() % minutes;

        if (!diff)
            return date;

        if (diff > minutes / 2) {
            return date.addMinutes(minutes - diff);
        } else {
            return date.addMinutes(-diff);
        }
    }

    private getResizedRow(elem): HTMLTableRowElement {
        return elem.parents('tr')[0];
    }

    private getTimeBlockClientChange(): SoeTimeBlockClientChange {
        var clientChange: SoeTimeBlockClientChange = SoeTimeBlockClientChange.None;
        if (this.resizeTarget === 'start')
            clientChange = SoeTimeBlockClientChange.Left;
        else if (this.resizeTarget === 'stop')
            clientChange = SoeTimeBlockClientChange.Right;

        return clientChange;
    }
}
