import { ICategoryDTO, ICompanyCategoryRecordDTO } from "@shared/models/generated-interfaces/SOECompModelDTOs";

export class CategoryItem {

    categoryId!: number;
    selected!: boolean;
    name!: string;
    default?: boolean;
    dateFrom?: Date;
    dateTo?: Date;
    isExecutive?: boolean;
    disabled!: boolean;

    static fromCategory(model: ICategoryDTO, selectedCategoryIds: number[]): CategoryItem {
        return {
            categoryId: model.categoryId,
            selected: selectedCategoryIds.indexOf(model.categoryId) >= 0,
            name: model.name,
            disabled: false
        };
    }

    static fromCompCategory(model: ICategoryDTO, selectedCompCategories?: ICompanyCategoryRecordDTO[]): CategoryItem {
        const match = selectedCompCategories?.find(x => x.categoryId === model.categoryId);
        return match ? {
            categoryId: model.categoryId,
            selected: true,
            name: model.name,
            default: match.default,
            dateFrom: match.dateFrom,
            dateTo: match.dateTo,
            isExecutive: match.isExecutive,
            disabled: false
        } : {
            categoryId: model.categoryId,
            selected: false,
            name: model.name,
            disabled: true
        };
    }
}