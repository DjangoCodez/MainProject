import { IEmployeeSkillDTO, ISkillDTO, IEmployeePostSkillDTO, ISearchEmployeeSkillDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class SkillMatcherDTO {
    shiftTypeId: number;
    shiftTypeName: string;
    skillId: number;
    skillName: string;
    skillLevel: number;
    skillRating: number;
    missing: boolean;
    employeeSkillLevel: number;
    employeeSkillRating: number;
    skillLevelUnreached: boolean;
    dateTo: Date;
    dateToPassed: boolean;
    note: string;
    ok: boolean;
    visible: boolean;
    selected: boolean;
}

export class EmployeeSkillDTO implements IEmployeeSkillDTO {
    dateTo: Date;
    employeeId: number;
    employeeSkillId: number;
    skillId: number;
    skillLevel: number;
    skillLevelStars: number;
    skillLevelUnreached: boolean;
    skillName: string;
    skillTypeName: string;
}

export class EmployeePostSkillDTO implements IEmployeePostSkillDTO {
    dateTo: Date;
    employeePostId: number;
    employeePostSkillId: number;
    skillDTO: ISkillDTO;
    skillId: number;
    skillLevel: number;
    skillLevelStars: number;
    skillLevelUnreached: boolean;
    skillName: string;
    skillTypeName: string;
}

export class SearchEmployeeSkillDTO implements ISearchEmployeeSkillDTO {
    accountName: string;
    employeeId: number;
    employeeName: string;
    endDate: Date;
    positions: string;
    skillId: number;
    skillLevel: number;
    skillLevelDifference: number;
    skillLevelDifferenceStars: number;
    skillLevelPosition: number;
    skillLevelPositionStars: number;
    skillLevelStars: number;
    skillName: string;

    // Extansions
    employeeSkillRating: number;
    positionSkillRating: number;
    diffSkillRating: number;

    public get skillRatingText(): string {
        return this.employeeSkillRating === 0 && this.positionSkillRating === 0 ? '' : "{0}/{1}".format(this.employeeSkillRating.toString(), this.positionSkillRating.toString());
    }

    public fixDates() {
        this.endDate = CalendarUtility.convertToDate(this.endDate);
    }
}