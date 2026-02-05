import { ICategoryAccountDTO, ICategoryDTO, ICompanyCategoryRecordDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, SoeCategoryRecordEntity, SoeCategoryType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";


export class CategoryAccountDTO implements ICategoryAccountDTO {
    accountId: number;
    actorCompanyId: number;
    categoryAccountId: number;
    categoryId: number;
    dateFrom: Date;
    dateTo: Date;
    state: SoeEntityState;
}

export class CategoryDTO implements ICategoryDTO {
    actorCompanyId: number;
    categoryGroupId: number;
    categoryGroupName: string;
    categoryId: number;
    children: CategoryDTO[];
    childrenNamesString: string;
    code: string;
    companyCategoryRecords: CompanyCategoryRecordDTO[];
    isSelected: boolean;
    isVisible: boolean;
    name: string;
    parentId: number;
    state: SoeEntityState;
    type: SoeCategoryType;
}

export class CompanyCategoryRecordDTO implements ICompanyCategoryRecordDTO {
    actorCompanyId: number;
    category: CategoryDTO;
    categoryId: number;
    companyCategoryId: number;
    dateFrom: Date;
    dateTo: Date;
    default: boolean;
    entity: SoeCategoryRecordEntity;
    isExecutive: boolean;
    recordId: number;
    uniqueId: string;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }
}
