import { ReportMenuDTO } from "../../../../../Common/Models/ReportDTOs";
import { SelectionCollection } from "../../SelectionCollection";
import { SelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";

export class TestAllSelections {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TestAllSelections,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/TestAllSelections/TestAllSelectionsView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        };

        return options;
    }
    public static componentKey = "testAllSelections";

    //binding fields
    private selections: SelectionCollection;

    //@ngInject
    constructor() {
    }

    public $onInit() {
       
    }

    public onAnySelectionUpdated(key: string, selection: (SelectionDTO | SelectionDTO[])) {
        this.selections.upsert(key, selection);
    }
}