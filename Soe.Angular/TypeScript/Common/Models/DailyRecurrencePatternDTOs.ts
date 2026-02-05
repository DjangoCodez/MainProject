import { CalendarUtility } from "../../Util/CalendarUtility";
import { IDailyRecurrencePatternDTO, IDailyRecurrenceRangeDTO, IDailyRecurrenceDatesOutput } from "../../Scripts/TypeLite.Net4";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { DayOfWeek } from "../../Util/Enumerations";
import { DailyRecurrencePatternType, DailyRecurrencePatternWeekIndex, DailyRecurrenceRangeType } from "../../Util/CommonEnumerations";

export class DailyRecurrenceParamsDTO {
    date: Date;
    range: DailyRecurrenceRangeDTO;
    pattern: DailyRecurrencePatternDTO;

    constructor(entity: any) {
        if (entity) {
            this.date = entity.startDate ?? CalendarUtility.getDateToday();
            this.range = new DailyRecurrenceRangeDTO();
            this.range.type = DailyRecurrenceRangeDTO.calculateType(entity.stopDate, entity.nbrOfOccurrences);
            this.range.numberOfOccurrences = entity.nbrOfOccurrences;
            this.range.startDate = entity.startDate;
            this.range.endDate = entity.stopDate;
            if (entity.recurrencePattern)
                this.pattern = DailyRecurrencePatternDTO.parse(entity.recurrencePattern);
        }
        else {
            this.date = CalendarUtility.getDateToday();
            this.range = new DailyRecurrenceRangeDTO();
            this.pattern = new DailyRecurrencePatternDTO();
        }
    }

    public parseResult(entity: any, result: any) {
        if (result) {
            if (result.range) {
                entity.startDate = result.range.startDate;
                entity.stopDate = result.range.endDate;
                entity.nbrOfOccurrences = result.range.numberOfOccurrences;
            }
            if (result.pattern) {
                entity.recurrencePattern = result.pattern.toString();
            }
        }
    }
}

export class DailyRecurrencePatternDTO implements IDailyRecurrencePatternDTO {
    dayOfMonth: number;
    daysOfWeek: DayOfWeek[];
    firstDayOfWeek: DayOfWeek;
    interval: number;
    month: number;
    sysHolidayTypeIds: number[];
    type: DailyRecurrencePatternType;
    weekIndex: DailyRecurrencePatternWeekIndex;

    constructor() {
        this.type = DailyRecurrencePatternType.Daily;
        this.firstDayOfWeek = DayOfWeek.Monday;
    }

    public toString(): string {
        var days: number[] = [];
        if (this.daysOfWeek) {
            _.forEach(this.daysOfWeek, day => {
                days.push(day);
            });
        }

        return "{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}".format(
            this.type ? this.type.toString() : '',
            this.type !== DailyRecurrencePatternType.None && this.interval ? this.interval.toString() : '',
            this.type !== DailyRecurrencePatternType.None && this.dayOfMonth ? this.dayOfMonth.toString() : '',
            this.type !== DailyRecurrencePatternType.None && this.month ? this.month.toString() : '',
            this.type !== DailyRecurrencePatternType.None && days.length > 0 ? days.join(',') : '',
            this.type !== DailyRecurrencePatternType.None && this.firstDayOfWeek ? this.firstDayOfWeek.toString() : '',
            this.type !== DailyRecurrencePatternType.None && this.weekIndex ? this.weekIndex.toString() : '',
            this.type !== DailyRecurrencePatternType.None && this.sysHolidayTypeIds && this.sysHolidayTypeIds.length > 0 ? this.sysHolidayTypeIds.join(',') : '');
    }

    public static parse(str: string): DailyRecurrencePatternDTO {
        var parts: string[] = str.split('_');
        if (parts.length !== 7 && parts.length !== 8)
            return null;

        var dto: DailyRecurrencePatternDTO = new DailyRecurrencePatternDTO();

        dto.type = parseInt(parts[0], 10) || 0;
        if (dto.type !== DailyRecurrencePatternType.None) {
            dto.interval = parseInt(parts[1], 10) || 0;
            dto.dayOfMonth = parseInt(parts[2], 10) || 0;
            dto.month = parseInt(parts[3], 10) || 0;

            // Days are comma separated
            var strDays: string[] = parts[4].split(',');
            var days: DayOfWeek[] = [];
            _.forEach(strDays, strDay => {
                if (strDay.length > 0)
                    days.push(parseInt(strDay, 10));
            });
            dto.daysOfWeek = days;

            dto.firstDayOfWeek = parseInt(parts[5], 10) || 0;
            dto.weekIndex = parseInt(parts[6], 10) || 0;

            if (parts.length > 7) {
                // Sys holidays are comma separated
                var strSysHolidayTypeIds: string[] = parts[7].split(',');
                var sysDays: number[] = [];
                _.forEach(strSysHolidayTypeIds, strSysHolidayTypeId => {
                    if (strSysHolidayTypeId.length > 0)
                        sysDays.push(parseInt(strSysHolidayTypeId, 10));
                });
                dto.sysHolidayTypeIds = sysDays;
            }
        }

        return dto;
    }
}

export class DailyRecurrenceRangeDTO implements IDailyRecurrenceRangeDTO {
    type: DailyRecurrenceRangeType;
    startDate: Date;
    endDate: Date;
    numberOfOccurrences: number;

    constructor() {
        this.type = DailyRecurrenceRangeType.NoEnd;
        this.startDate = CalendarUtility.getDateToday();
    }

    public toString(): string {
        return "{0}_{1}_{2}_{3}".format(
            this.type ? this.type.toString() : '',
            this.startDate ? this.startDate.toFormattedDate('YYYY-MM-DD') : '',
            this.endDate ? this.endDate.toFormattedDate('YYYY-MM-DD') : '',
            this.numberOfOccurrences ? this.numberOfOccurrences.toString() : '');
    }

    public static parse(str: string): DailyRecurrenceRangeDTO {
        var parts: string[] = str.split('_');
        if (parts.length !== 4)
            return null;

        var dto: DailyRecurrenceRangeDTO = new DailyRecurrenceRangeDTO();

        // Type
        dto.type = parseInt(parts[0], 10) || 0;

        // Start date
        if (parts[1].length > 0) {
            let dateParts = parts[1].split('-');
            dto.startDate = new Date(parseInt(dateParts[0], 10), parseInt(dateParts[1], 10) - 1, parseInt(dateParts[2], 10));
        }

        // End date
        if (parts[2].length > 0) {
            let dateParts = parts[2].split('-');
            dto.endDate = new Date(parseInt(dateParts[0], 10), parseInt(dateParts[1], 10) - 1, parseInt(dateParts[2], 10));
        }

        // Number of occurrences
        dto.numberOfOccurrences = parseInt(parts[3], 10) || 0;

        return dto;
    }

    public static calculateType(endDate: Date, numberOfOccurrences: number): DailyRecurrenceRangeType {
        if (endDate)
            return DailyRecurrenceRangeType.EndDate;
        else if (numberOfOccurrences)
            return DailyRecurrenceRangeType.Numbered;
        else
            return DailyRecurrenceRangeType.NoEnd;
    }

    public static setRecurrenceInfo(entity: any, translationService: ITranslationService) {
        if (entity) {
            translationService.translate("common.dailyrecurrencepattern.startson").then((term) => {
                if (!entity.startDate)
                    entity.startDate = CalendarUtility.getDateToday();
                entity['startDateDescription'] = term.format(entity.startDate.toFormattedDate('YYYY-MM-DD'));
            });

            var type: DailyRecurrenceRangeType = DailyRecurrenceRangeDTO.calculateType(entity.stopDate, entity.nbrOfOccurrences);
            if (type === DailyRecurrenceRangeType.EndDate) {
                if (entity.stopDate) {
                    translationService.translate("common.dailyrecurrencepattern.endson").then((term) => {
                        entity['stopDateDescription'] = term.format(entity.stopDate.toFormattedDate('YYYY-MM-DD'));
                    });
                }
            } else if (type == DailyRecurrenceRangeType.Numbered) {
                if (!entity.nbrOfOccurrences)
                    entity.nbrOfOccurrences = 0;
                translationService.translate("common.dailyrecurrencepattern.endsafter").then((term) => {
                    entity['stopDateDescription'] = term.format(entity.nbrOfOccurrences.toString());
                });
            } else {
                entity['stopDateDescription'] = "";
            }
        }
    }
}

export class DailyRecurrenceDatesOutput implements IDailyRecurrenceDatesOutput {
    recurrenceDates: Date[];
    removedDates: Date[];

    public getValidDates(includeRemovedDates: boolean = false): Date[] {
        return includeRemovedDates ? this.recurrenceDates : _.filter(this.recurrenceDates, d => !_.includes(this.removedDates, d));
    }

    public doRecurOnDate(date: Date, includeRemovedDates: boolean = false): boolean {
        return _.includes(this.getValidDates(includeRemovedDates), date);
    }

    public doRecurOnDateButIsRemoved(date: Date): boolean {
        return _.includes(this.recurrenceDates, date) && _.includes(this.removedDates, date);
    }

    public hasRecurringDates(includeRemovedDates: boolean = false): boolean {
        return this.getValidDates(includeRemovedDates).length > 0;
    }
}

