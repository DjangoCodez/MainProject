import { TimeScheduleTemplateBlockSlim, TimeScheduleTemplateHeadDTO } from "../../../Common/Models/TimeScheduleTemplateDTOs";
import { IEmployeeSchedulePlacementGridViewDTO } from "../../../Scripts/TypeLite.Net4";

export class TemplatesValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                placement: '=',
                stopDate: '=',
                blocks: '=',
                shiftTypeMandatory: '=',
                hasOverlappingBreakWindows: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue) => {
                    if (newValue) {
                        let templateHead: TimeScheduleTemplateHeadDTO = ngModelController.$modelValue;

                        // Start date mandatory for personal templates
                        let validStartDateMandatory: boolean = true;
                        if (!templateHead.startDate)
                            validStartDateMandatory = false;
                        ngModelController.$setValidity("startDateMandatory", validStartDateMandatory);

                        // Stop date must be after start date
                        let validStopDate: boolean = true;
                        if (templateHead.startDate && templateHead.stopDate && templateHead.startDate.isAfterOnDay(templateHead.stopDate))
                            validStopDate = false;
                        ngModelController.$setValidity("stopDateAfterStartDate", validStopDate);

                        // StartOnMonday requires start date to be on a monday
                        let validStartOnMonday: boolean = true;
                        if (templateHead.startOnFirstDayOfWeek && templateHead.startDate && templateHead.startDate.getDay() !== 1)
                            validStartOnMonday = false;
                        ngModelController.$setValidity("mustStartOnMonday", validStartOnMonday);
                    }
                }, true);

                scope.$watchGroup(['placement', 'stopDate'], (newValues, oldValues, scope) => {
                    let placement: IEmployeeSchedulePlacementGridViewDTO = scope['placement'];
                    let stopDate: Date = scope['stopDate'];

                    // Template can not end before last placement ends
                    let validStopDateForPlacement: boolean = true;
                    if (stopDate && placement && placement.employeeScheduleStopDate.isAfterOnDay(stopDate))
                        validStopDateForPlacement = false;
                    ngModelController.$setValidity("validStopDateForPlacement", validStopDateForPlacement);
                });

                scope.$watch(() => scope['blocks'], (newValues, oldValues) => {
                    let blocks: TimeScheduleTemplateBlockSlim[] = scope['blocks'];

                    // Shift type mandatory (setting)
                    let shiftTypeMandatory: boolean = scope["shiftTypeMandatory"];
                    let validShiftType: boolean = true;
                    if (shiftTypeMandatory && _.filter(blocks, b => !b.shiftTypeId && !b.startTime.isSameMinuteAs(b.stopTime)).length > 0)
                        validShiftType = false;
                    ngModelController.$setValidity("shiftTypeMandatory", validShiftType);

                    // Blocks with negative lengths are not allowed
                    let validDuration: boolean = true;
                    if (_.filter(blocks, b => b.duration < 0).length > 0)
                        validDuration = false;
                    ngModelController.$setValidity("duration", validDuration);

                    let validMultipleBreaksPerType: boolean = true;

                    // Clear
                    blocks.forEach(block => {
                        block['overlappingBreak1'] = false;
                        block['overlappingBreak2'] = false;
                        block['overlappingBreak3'] = false;
                        block['overlappingBreak4'] = false;
                    });

                    let blocksByDay = _.groupBy(blocks, b => b.dayNumber);
                    _.forEach(Object.keys(blocksByDay), dayNumber => {
                        let dayBlocks = _.orderBy(blocksByDay[dayNumber], b => b.startTime);

                        // Overlapping times not allowed on same day
                        let prevBlock: TimeScheduleTemplateBlockSlim = null;
                        _.forEach(dayBlocks, block => {
                            block['overlapping'] = false;
                            if (prevBlock) {
                                if (block.startTime.isBeforeOnMinute(prevBlock.stopTime)) {
                                    prevBlock['overlapping'] = true;
                                    block['overlapping'] = true;
                                }
                            }
                            prevBlock = block;
                        });

                        // Only one break per column (1-4) in same day allowed
                        for (let i = 1; i <= 4; i++) {
                            if (_.filter(dayBlocks, b => b[`break${i}TimeCodeId`]).length > 1) {
                                _.filter(blocks, b => b.dayNumber === parseInt(dayNumber, 10) && b[`break${i}TimeCodeId`]).forEach(block => block[`overlappingBreak${i}`] = true);
                                validMultipleBreaksPerType = false;
                            }
                        }
                    });

                    ngModelController.$setValidity("overlapping", _.filter(blocks, b => b['overlapping']).length === 0);
                    ngModelController.$setValidity("overlappingBreaks", validMultipleBreaksPerType);

                    // Overlapping break windows are not allowed
                    let hasOverlappingBreakWindows: boolean = scope["hasOverlappingBreakWindows"];
                    ngModelController.$setValidity("overlappingBreakWindows", !hasOverlappingBreakWindows);
                }, true);
            }
        }
    }
}


