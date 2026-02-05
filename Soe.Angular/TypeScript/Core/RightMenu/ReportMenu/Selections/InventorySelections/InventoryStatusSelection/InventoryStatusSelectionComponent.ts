import { IdListSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ISmallGenericType } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup } from "../../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../../Services/CoreService";

export class InventoryStatusSelection {
    public static component(): ng.IComponentOptions {
        return {
            controller: InventoryStatusSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/InventorySelections/InventoryStatusSelection/InventoryStatusSelectionView.html",

            bindings: {
                onSelected: "&",
                selectedStatuses: "=",
            }
        };
    }
    public static componentKey = "inventoryStatusSelection";
    private selectedStatuses: ISmallGenericType[] = null;
    private availableStatuses: ISmallGenericType[];

    private onSelected: (_: { selection: IdListSelectionDTO }) => void = angular.noop;

    //@ngInject
    constructor(private coreService: ICoreService) { }

    public $onInit() {
        this.loadStatuses();
    }

    public statusesChanged(selection: IdListSelectionDTO) {
        this.onSelected({ selection: selection });
    }

    private loadStatuses() {
        return this.coreService.getTermGroupContent(TermGroup.InventoryStatus, false, false, false).then(data => {
            this.availableStatuses = data;
            this.selectedStatuses = [];
        });
    }
}