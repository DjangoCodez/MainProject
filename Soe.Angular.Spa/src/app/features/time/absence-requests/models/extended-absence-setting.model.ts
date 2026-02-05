import { IExtendedAbsenceSettingDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class ExtendedAbsenceSettingDTO implements IExtendedAbsenceSettingDTO {
  extendedAbsenceSettingId: number = 0;
  absenceFirstAndLastDay: boolean = false;
  absenceWholeFirstDay: boolean = false;
  absenceFirstDayStart?: Date | undefined;
  absenceWholeLastDay: boolean = false;
  absenceLastDayStart?: Date | undefined;
  percentalAbsence: boolean = false;
  percentalValue?: number | undefined;
  percentalAbsenceOccursStartOfDay?: boolean | undefined;
  percentalAbsenceOccursEndOfDay?: boolean | undefined;
  adjustAbsencePerWeekDay: boolean = false;
  adjustAbsenceAllDaysStart?: Date | undefined;
  adjustAbsenceAllDaysStop?: Date | undefined;
  adjustAbsenceMonStart?: Date | undefined;
  adjustAbsenceMonStop?: Date | undefined;
  adjustAbsenceTueStart?: Date | undefined;
  adjustAbsenceTueStop?: Date | undefined;
  adjustAbsenceWedStart?: Date | undefined;
  adjustAbsenceWedStop?: Date | undefined;
  adjustAbsenceThuStart?: Date | undefined;
  adjustAbsenceThuStop?: Date | undefined;
  adjustAbsenceFriStart?: Date | undefined;
  adjustAbsenceFriStop?: Date | undefined;
  adjustAbsenceSatStart?: Date | undefined;
  adjustAbsenceSatStop?: Date | undefined;
  adjustAbsenceSunStart?: Date | undefined;
  adjustAbsenceSunStop?: Date | undefined;
}
