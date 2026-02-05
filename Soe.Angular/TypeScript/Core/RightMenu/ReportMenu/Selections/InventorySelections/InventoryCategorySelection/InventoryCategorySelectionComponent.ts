import { IdListSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ISmallGenericType } from "../../../../../../Scripts/TypeLite.Net4";
import { IReportService } from "../../../../../../Core/Services/reportservice";

export class InventoryCategorySelection {
    public static component(): ng.IComponentOptions {
        return {
            controller: InventoryCategorySelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/InventorySelections/InventoryCategorySelection/InventoryCategorySelectionView.html",
            bindings: {
                onSelected: "&",
                selectedCategories: "=",
            }
        };
    }
    public static readonly componentKey = "inventoryCategorySelection";
    private selectedCategories: ISmallGenericType[] = null;
    private availableCategories: ISmallGenericType[];

    private readonly onSelected: (_: { selection: IdListSelectionDTO }) => void = angular.noop;

    //@ngInject
    constructor(private readonly reportService: IReportService) {
    }

    public $onInit() {
        this.loadCategories();
    }

    private categoriesChanged(selection: IdListSelectionDTO) {
        this.onSelected({ selection: selection });
    }
    
    private loadCategories() {
        return this.reportService.getInventoryCategories().then(data => {
            this.availableCategories = data;
            this.selectedCategories = [];
        });
    }
}