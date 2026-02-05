import { ITimeBreakTemplateGridDTO, ISmallGenericType, IActionResult } from "../../Scripts/TypeLite.Net4";
import { DayTypeDTO } from "./DayTypeDTO";
import { ShiftTypeDTO } from "./ShiftTypeDTO";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class TimeBreakTemplateGridDTO implements ITimeBreakTemplateGridDTO {
    actorCompanyId: number;
    dayOfWeeks: ISmallGenericType[];
    dayTypes: DayTypeDTO[];
    majorMinTimeAfterStart: number;
    majorMinTimeBeforeEnd: number;
    majorNbrOfBreaks: number;
    majorTimeCodeBreakGroupId: number;
    majorTimeCodeBreakGroupName: string;
    minorMinTimeAfterStart: number;
    minorMinTimeBeforeEnd: number;
    minorNbrOfBreaks: number;
    minorTimeCodeBreakGroupId: number;
    minorTimeCodeBreakGroupName: string;
    minTimeBetweenBreaks: number;
    rowNr: number;
    shiftLength: number;
    shiftStartFromTimeMinutes: number;
    shiftTypes: ShiftTypeDTO[];
    startDate: Date;
    state: SoeEntityState;
    stopDate: Date;
    timeBreakTemplateId: number;
    useMaxWorkTimeBetweenBreaks: boolean;
    validationResult: IActionResult;

    // Extensions
    get isValid(): boolean {
        return !this.validationResult || this.validationResult.success;
    }
    get shiftTypeNames(): string {
        if (this.shiftTypes && this.shiftTypes.length > 0)
            return _.map(_.sortBy(_.filter(this.shiftTypes, s => s), 'name'), s => s.name).join(', ');
        else
            return '';
    }
    get dayTypeNames(): string {
        if (this.dayTypes && this.dayTypes.length > 0)
            return _.map(_.sortBy(_.filter(this.dayTypes, s => s), 'name'), s => s.name).join(', ');
        else
            return '';
    }
    get dayOfWeekNames(): string {
        if (this.dayOfWeeks && this.dayOfWeeks.length > 0) {
            var mondayToSaturdays = _.sortBy(_.filter(this.dayOfWeeks, s => s && s.id > 0), 'id');
            var sundays = _.filter(this.dayOfWeeks, s => s && s.id === 0);
            var allDays = mondayToSaturdays.concat(sundays);
            return _.map(allDays, s => s.name).join(', ');
        }
        else
            return '';
    }
}
