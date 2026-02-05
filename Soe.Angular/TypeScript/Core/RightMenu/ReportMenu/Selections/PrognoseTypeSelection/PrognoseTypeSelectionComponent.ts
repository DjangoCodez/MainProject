import { IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Services/CoreService";

export class PrognoseTypeSelection {
    public static component(): ng.IComponentOptions {
        return {
            controller: PrognoseTypeSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/PrognoseTypeSelection/PrognoseTypeSelectionView.html",
            bindings: {
                onSelected: "&",
                selectedPrognoseType: "=",
            }
        };
    }
    public static componentKey = "prognoseTypeSelection";
    private selectedPrognoseType: ISmallGenericType = null;
    private selectablePrognoseTypes: ISmallGenericType[];

    private onSelected: (_: { selection: IdListSelectionDTO }) => void = angular.noop;

    //@ngInject
    constructor(private coreService: ICoreService) {
    }

    public $onInit() {
        this.loadPrognoseTypes();
    }

    private prognoseTypeChanged(selection: IdListSelectionDTO) {
        this.onSelected({ selection: selection });
    }

    private loadPrognoseTypes() {
        return this.coreService.getTermGroupContent(TermGroup.PrognosTypes, false, true, true).then(data => {
            this.selectablePrognoseTypes = data;
            this.selectedPrognoseType = null;
        });
    }
}